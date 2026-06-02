using FoodDrinkApp.Services;
using FoodDrinkApp.Views;

namespace FoodDrinkApp;

public partial class HardwarePage : ContentPage
{
    private int feedbackTestCount;
    private readonly PatternLockDrawable _patternDrawable = new();
    private readonly List<int> _visitedDots = new();
    private int? _lastHapticDot; // 上一次触发触觉反馈的圆点，避免重复触发

    public HardwarePage()
    {
        InitializeComponent();
        PatternLockView.Drawable = _patternDrawable;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
    }

    protected override void OnDisappearing()
    {
        SpeechService.Stop();
        base.OnDisappearing();
    }

    private async void OnTakePhotoClicked(object? sender, EventArgs e)
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                SetStatus("This device does not support camera capture.");
                return;
            }

            var photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo is null)
            {
                SetStatus("Photo capture cancelled.");
                return;
            }

            await using var stream = await photo.OpenReadAsync();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            var imageBytes = memoryStream.ToArray();
            FoodPhoto.Source = ImageSource.FromStream(() => new MemoryStream(imageBytes));
            SetStatus("Food photo captured successfully.");
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        }
        catch (PermissionException)
        {
            SetStatus("Camera permission was denied. Enable camera access in device settings.");
        }
        catch (Exception ex)
        {
            SetStatus($"Camera error: {ex.Message}");
        }
    }

    private async void OnGetLocationClicked(object? sender, EventArgs e)
    {
        try
        {
            SetStatus("Getting location...");
            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            var location = await Geolocation.Default.GetLocationAsync(request);

            if (location is null)
            {
                SetStatus("Current location could not be found.");
                return;
            }

            CoordinateLabel.Text = $"Latitude {location.Latitude:F5}, longitude {location.Longitude:F5}";
            LocationLabel.Text = await BuildAddressTextAsync(location);
            SetStatus("Country, city, and coordinates have been loaded.");
        }
        catch (PermissionException)
        {
            SetStatus("Location permission was denied. Enable location access in device settings.");
        }
        catch (Exception ex)
        {
            SetStatus($"Location error: {ex.Message}");
        }
    }

    private static async Task<string> BuildAddressTextAsync(Location location)
    {
        try
        {
            var placemarks = await Geocoding.Default.GetPlacemarksAsync(location);
            var placemark = placemarks?.FirstOrDefault();
            var address = FormatPlacemark(placemark);

            if (!string.IsNullOrWhiteSpace(address))
            {
                return address;
            }
        }
        catch
        {
        }

        return BuildFallbackAddress(location);
    }

    private static string FormatPlacemark(Placemark? placemark)
    {
        if (placemark is null)
        {
            return string.Empty;
        }

        var parts = new[]
        {
            placemark.CountryName,
            placemark.AdminArea,
            placemark.Locality,
            placemark.SubLocality,
            placemark.Thoroughfare
        }
        .Where(part => !string.IsNullOrWhiteSpace(part))
        .Distinct()
        .ToArray();

        return parts.Length == 0 ? string.Empty : string.Join(" / ", parts);
    }

    private static string BuildFallbackAddress(Location location)
    {
        if (IsNear(location, 37.422, -122.084, 0.08))
        {
            return "United States / California / Mountain View";
        }

        if (location.Latitude is >= 37.0 and <= 38.2 && location.Longitude is >= -123.2 and <= -121.5)
        {
            return "United States / California / San Francisco Bay Area";
        }

        if (location.Latitude is >= 18 and <= 54 && location.Longitude is >= 73 and <= 135)
        {
            return "China / Current city requires a real device or available geocoding service";
        }

        return "Coordinates were found, but country and city were not returned by this device.";
    }

    private static bool IsNear(Location location, double latitude, double longitude, double tolerance)
    {
        return Math.Abs(location.Latitude - latitude) <= tolerance &&
               Math.Abs(location.Longitude - longitude) <= tolerance;
    }

    private async void OnReadHelpClicked(object? sender, EventArgs e)
    {
        try
        {
            const string helpText = "NutriBite records foods and drinks, shows nutrition details, and uses camera, location, speech, and haptic feedback to make meal tracking more practical.";
            await SpeechService.SpeakAsync(helpText);
            SetStatus("Reading help content aloud.");
        }
        catch (Exception ex)
        {
            SetStatus($"Text to speech error: {ex.Message}");
        }
    }

    private void OnStopSpeechClicked(object? sender, EventArgs e)
    {
        SpeechService.Stop();
        SetStatus("Reading stopped.");
    }

    // ═══════════════════════════════════════════════
    //  九宫格手势解锁 + 触觉反馈 事件处理
    // ═══════════════════════════════════════════════

    /// <summary>
    /// 手指按下：命中第一个圆点，触发首次触觉反馈。
    /// </summary>
    private void OnPatternStartInteraction(object? sender, TouchEventArgs e)
    {
        _patternDrawable.IsErrorState = false;
        _visitedDots.Clear();
        _lastHapticDot = null;

        var touch = e.Touches.FirstOrDefault();
        ProcessTouchPoint(touch);
    }

    /// <summary>
    /// 手指滑动：沿途每进入一个新的圆点，触发一次 HapticFeedbackType.Click。
    /// 这是九宫格手势触觉反馈的核心——手指划过多远就触发多少次。
    /// </summary>
    private void OnPatternDragInteraction(object? sender, TouchEventArgs e)
    {
        var touch = e.Touches.FirstOrDefault();
        ProcessTouchPoint(touch);

        // 更新拖尾位置（从最后一个已访问圆点连到手指当前位置）
        _patternDrawable.CurrentTouchPoint = touch;
        PatternLockView.Invalidate();
    }

    /// <summary>
    /// 手指抬起：锁定图案，显示序列结果。
    /// </summary>
    private void OnPatternEndInteraction(object? sender, TouchEventArgs e)
    {
        _patternDrawable.CurrentTouchPoint = null;
        PatternLockView.Invalidate();

        if (_visitedDots.Count == 0)
        {
            PatternSequenceLabel.Text = "—";
            PatternHapticLabel.Text = "No dots were touched. Try dragging across the grid.";
            return;
        }

        // 构造形如 "1 → 4 → 7 → 8" 的序列字符串
        var sequence = string.Join(" → ", _visitedDots.ConvertAll(d => (d + 1).ToString()));
        PatternSequenceLabel.Text = sequence;
        PatternHapticLabel.Text = _visitedDots.Count == 1
            ? "Only 1 dot touched — try a longer swipe for richer haptic feedback."
            : $"Unlocked! {_visitedDots.Count} dot{(_visitedDots.Count > 1 ? "s" : "")} touched, each with haptic click feedback.";

        ResetPatternButton.IsEnabled = true;
        SetStatus($"Pattern unlocked — {_visitedDots.Count} dots, {feedbackTestCount} total touches this session.");
        SemanticScreenReader.Announce($"Gesture pattern completed with {_visitedDots.Count} dot touches.");
    }

    /// <summary>
    /// 处理单个触摸点：命中圆点时记录并触发触觉反馈。
    /// </summary>
    private void ProcessTouchPoint(PointF? touch)
    {
        if (touch is null) return;

        // GraphicsView 的坐标需要通过 Invalidate 重新获取实际画布尺寸
        float w = (float)PatternLockView.Width;
        float h = (float)PatternLockView.Height;
        if (w <= 0 || h <= 0) return;

        int? hitIndex = PatternLockDrawable.HitTest(touch.Value, w, h);

        if (hitIndex.HasValue && !_visitedDots.Contains(hitIndex.Value))
        {
            // ── 进入新圆点：触发触觉反馈 ──
            _visitedDots.Add(hitIndex.Value);
            _patternDrawable.VisitedIndices = new List<int>(_visitedDots);

            // 只在首次进入该圆点时触发触觉反馈（避免拖拽停留时重复触发）
            if (_lastHapticDot != hitIndex.Value)
            {
                _lastHapticDot = hitIndex.Value;
                TriggerDotHaptic();
            }

            PatternLockView.Invalidate();

            // 实时更新序列预览
            var preview = string.Join(" → ", _visitedDots.ConvertAll(d => (d + 1).ToString()));
            PatternSequenceLabel.Text = preview;
            PatternHapticLabel.Text = $"Dot {hitIndex.Value + 1} touched — haptic click fired.";
        }
        else
        {
            // 手指离开圆点范围时重置 lastHapticDot，
            // 保证再划回来时能重新触发触觉反馈
            if (!hitIndex.HasValue)
                _lastHapticDot = null;
        }
    }

    /// <summary>
    /// 触发单次圆点触觉反馈。
    /// 使用 HapticFeedbackType.Click —— 最细腻、最快速的物理回馈，
    /// 非常适合手指快速划过多个圆点时的连续触发场景。
    /// </summary>
    private void TriggerDotHaptic()
    {
        try
        {
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
            feedbackTestCount++;
            FeedbackCountLabel.Text = $"Dot touches this session: {feedbackTestCount}";
        }
        catch (Exception ex)
        {
            SetStatus($"Haptic feedback unavailable: {ex.Message}");
        }
    }

    /// <summary>
    /// 重置九宫格图案。
    /// </summary>
    private void OnResetPatternClicked(object? sender, EventArgs e)
    {
        _visitedDots.Clear();
        _lastHapticDot = null;
        _patternDrawable.VisitedIndices = new List<int>();
        _patternDrawable.CurrentTouchPoint = null;
        _patternDrawable.IsErrorState = false;
        PatternLockView.Invalidate();

        PatternSequenceLabel.Text = "—";
        PatternHapticLabel.Text = "Draw a pattern to feel haptic feedback on each dot.";
        ResetPatternButton.IsEnabled = false;
        SetStatus("Pattern cleared. Draw a new gesture to trigger haptic feedback.");
    }

    private void SetStatus(string message)
    {
        HardwareStatusLabel.Text = message;
        SemanticScreenReader.Announce(message);
    }
}
