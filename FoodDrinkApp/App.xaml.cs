using FoodDrinkApp.Services;

namespace FoodDrinkApp;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		// 清除旧缓存：首次启动时清理残留的 50 条旧 MockAPI 数据，
		// 下次 GetCatalogAsync 会从 MockAPI 重新拉取最新的 6 条数据。
		FoodCatalogService.ClearCache();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}