namespace FoodDrinkApp.Services;

/// <summary>
/// 摇一摇检测服务 —— 利用加速度计（Accelerometer）实时监测设备晃动，
/// 当加速度幅值超过阈值且满足去抖条件时触发 ShakeDetected 事件。
///
/// 算法说明：
/// - 读取三轴加速度 (X, Y, Z)，计算合加速度幅值 sqrt(X² + Y² + Z²)。
/// - 当幅值 > ShakeThresholdG（默认 1.6g）时判定为一次摇动。
/// - 每次触发后有 CooldownMs（默认 2000ms）冷却期，防止连续抖动误触发。
/// - 使用 SensorSpeed.Game 以获得最快响应（~20ms 采样间隔）。
///
/// 使用方式：
/// <code>
///   ShakeService.ShakeDetected += () => { /* 随机推荐逻辑 */ };
///   ShakeService.Start();
///   // ...
///   ShakeService.Stop();
/// </code>
/// </summary>
public static class ShakeService
{
    /// <summary>摇动检测阈值（单位 g，地球重力加速度倍数）。</summary>
    public static double ShakeThresholdG { get; set; } = 1.6;

    /// <summary>两次摇动触发之间的最小冷却时间（毫秒）。</summary>
    public static int CooldownMs { get; set; } = 2000;

    /// <summary>摇动事件 —— 每次有效摇动触发一次。</summary>
    public static event Action? ShakeDetected;

    /// <summary>最近一次触发的时间戳（用于冷却判定）。</summary>
    private static DateTime _lastShakeTime = DateTime.MinValue;

    /// <summary>上一次加速度读数的幅值（用于斜率/增量检测）。</summary>
    private static double _lastMagnitude;

    /// <summary>服务是否正在运行。</summary>
    public static bool IsRunning { get; private set; }

    /// <summary>
    /// 启动加速度计监听。若设备不支持或已在运行则静默返回。
    /// </summary>
    public static void Start()
    {
        if (IsRunning) return;

        try
        {
            if (!Accelerometer.Default.IsSupported)
            {
                System.Diagnostics.Debug.WriteLine("[ShakeService] Accelerometer is not supported on this device.");
                return;
            }

            Accelerometer.Default.ReadingChanged += OnAccelerometerReadingChanged;
            Accelerometer.Default.Start(SensorSpeed.Game);
            IsRunning = true;
            System.Diagnostics.Debug.WriteLine("[ShakeService] Started listening for shake gestures.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ShakeService] Failed to start: {ex.Message}");
        }
    }

    /// <summary>
    /// 停止加速度计监听。
    /// </summary>
    public static void Stop()
    {
        if (!IsRunning) return;

        try
        {
            Accelerometer.Default.ReadingChanged -= OnAccelerometerReadingChanged;
            Accelerometer.Default.Stop();
            IsRunning = false;
            System.Diagnostics.Debug.WriteLine("[ShakeService] Stopped.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ShakeService] Failed to stop: {ex.Message}");
        }
    }

    private static void OnAccelerometerReadingChanged(object? sender, AccelerometerChangedEventArgs e)
    {
        var reading = e.Reading;
        var acceleration = reading.Acceleration;

        // 计算合加速度幅值（单位 g）
        double magnitude = Math.Sqrt(
            acceleration.X * acceleration.X +
            acceleration.Y * acceleration.Y +
            acceleration.Z * acceleration.Z
        );

        // 双条件判定：
        // 1. 幅值超过阈值（绝对值判定）
        // 2. 幅值变化量 > 0.8g（增量判定，过滤缓慢倾斜）
        double delta = Math.Abs(magnitude - _lastMagnitude);
        _lastMagnitude = magnitude;

        bool exceedsThreshold = magnitude > ShakeThresholdG;
        bool rapidChange = delta > 0.8;

        if (exceedsThreshold && rapidChange)
        {
            var now = DateTime.UtcNow;
            if ((now - _lastShakeTime).TotalMilliseconds >= CooldownMs)
            {
                _lastShakeTime = now;
                System.Diagnostics.Debug.WriteLine($"[ShakeService] Shake detected! Magnitude={magnitude:F2}g, Delta={delta:F2}g");

                // 在主线程上触发事件
                MainThread.BeginInvokeOnMainThread(() => ShakeDetected?.Invoke());
            }
        }
    }
}
