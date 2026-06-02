using FoodDrinkApp.Models;
using FoodDrinkApp.Services;

namespace FoodDrinkApp;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        ShakeService.ShakeDetected += OnShakeDetected;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
        await LoadFoodItemsAsync(SearchFoodBar.Text);
        ShakeService.Start();
    }

    protected override void OnDisappearing()
    {
        ShakeService.Stop();
        base.OnDisappearing();
    }

    /// <summary>
    /// 摇一摇回调：从当前数据源中随机选取一个食物/饮品，直接导航到详情页。
    /// </summary>
    private async void OnShakeDetected()
    {
        try
        {
            var items = await FoodCatalogService.GetCatalogAsync();

            if (items.Count == 0)
            {
                await DisplayAlert(
                    "Nothing to recommend",
                    "Add some food or drink records first, then shake again.",
                    "OK");
                return;
            }

            // 随机选取
            var randomItem = items[Random.Shared.Next(items.Count)];

            // 短暂的视觉反馈：闪烁提示条
            ShakeHintLabel.Text = "✅ Shake detected! Opening random recommendation...";
            await Task.Delay(400);
            ShakeHintLabel.Text = "📳 Shake your phone to discover a random healthy recipe";

            // 导航到详情页
            await Shell.Current.GoToAsync(
                $"{nameof(FoodDetailPage)}?id={Uri.EscapeDataString(randomItem.Id)}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MainPage] Shake recommendation failed: {ex.Message}");
        }
    }

    private async Task LoadFoodItemsAsync(string? query = null, bool forceRefresh = false)
    {
        // Force refresh = re-fetch from MockAPI (pull-to-refresh), then search
        if (forceRefresh)
        {
            await FoodCatalogService.GetCatalogAsync(forceRefresh: true);
        }

        FoodCollection.ItemsSource = await FoodCatalogService.SearchAsync(query);
    }

    private async void OnAddClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AddItemPage));
    }

    private async void OnDetailsClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string id)
        {
            await Shell.Current.GoToAsync($"{nameof(FoodDetailPage)}?id={Uri.EscapeDataString(id)}");
        }
    }

    private async void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        await LoadFoodItemsAsync(e.NewTextValue);
    }

    private async void OnSearchButtonPressed(object? sender, EventArgs e)
    {
        await LoadFoodItemsAsync(SearchFoodBar.Text);
    }

    private async void OnRefreshing(object? sender, EventArgs e)
    {
        // Pull-to-refresh: force re-fetch from MockAPI to pick up latest remote data
        await LoadFoodItemsAsync(SearchFoodBar.Text, forceRefresh: true);
        FoodRefreshView.IsRefreshing = false;
        var source = FoodCatalogService.LastLoadUsedMockApi ? "mockapi.io" : "local cache & fallback data";
        SemanticScreenReader.Announce($"Food and drink list refreshed. Current source: {source}.");
    }
}
