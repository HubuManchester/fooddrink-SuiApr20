using FoodDrinkApp.Services;

namespace FoodDrinkApp;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		// Migrate from old JSON-file cache to SQLite database.
		// Delete the legacy cache file if it still exists.
		try
		{
			var legacyCachePath = Path.Combine(FileSystem.AppDataDirectory, "food_catalog_cache.json");
			if (File.Exists(legacyCachePath))
			{
				File.Delete(legacyCachePath);
			}
		}
		catch
		{
			// Best-effort migration
		}

		// Clear stale SQLite data on first launch after the architecture change.
		// This ensures the old 50-item JSON cache doesn't persist in any form.
		FoodCatalogService.ClearCache();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}