using FoodDrinkApp.Models;
using FoodDrinkApp.Services;

namespace FoodDrinkApp;

public partial class AddItemPage : ContentPage
{
    public AddItemPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        try
        {
            var validationMessage = ValidateForm(out var calories, out var protein, out var carbs, out var fat);
            if (validationMessage is not null)
            {
                ShowValidation(validationMessage);
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(250));
                return;
            }

            var item = new FoodItem
            {
                Name = NameEntry.Text!.Trim(),
                Category = CategoryPicker.SelectedItem?.ToString() ?? "Snack 🍬",
                Description = DescriptionEditor.Text!.Trim(),
                Calories = calories,
                Protein = protein,
                Carbs = carbs,
                Fat = fat,
                AllergyNote = string.IsNullOrWhiteSpace(AllergyEntry.Text)
                    ? "No allergy note provided."
                    : AllergyEntry.Text.Trim(),
                Tags = $"{NameEntry.Text} {CategoryPicker.SelectedItem} {DescriptionEditor.Text}"
            };

            await FoodCatalogService.AddAsync(item);
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
            SemanticScreenReader.Announce("Food record saved.");

            await DisplayAlert(
                "Saved",
                MockApiConfig.IsConfigured
                    ? "The record has been saved to mockapi.io."
                    : "The record has been saved to local fallback data.",
                "OK");

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            ShowValidation($"The record could not be saved: {ex.Message}");
        }
    }

    private string? ValidateForm(out int calories, out int protein, out int carbs, out int fat)
    {
        calories = protein = carbs = fat = 0;

        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            return "Please enter a food or drink name.";
        }

        if (CategoryPicker.SelectedIndex < 0)
        {
            return "Please choose a category.";
        }

        if (string.IsNullOrWhiteSpace(DescriptionEditor.Text))
        {
            return "Please add a short description.";
        }

        return TryReadNumber(CaloriesEntry.Text, "calories", out calories)
            ?? TryReadNumber(ProteinEntry.Text, "protein", out protein)
            ?? TryReadNumber(CarbsEntry.Text, "carbs", out carbs)
            ?? TryReadNumber(FatEntry.Text, "fat", out fat);
    }

    private static string? TryReadNumber(string? value, string fieldName, out int number)
    {
        if (int.TryParse(value, out number) && number >= 0)
        {
            return null;
        }

        return $"Please enter a valid non-negative number for {fieldName}.";
    }

    private void ShowValidation(string message)
    {
        ValidationLabel.Text = message;
        ValidationPanel.IsVisible = true;
        SemanticScreenReader.Announce(message);
    }
}
