using System.Text.Json.Serialization;
using SQLite;

namespace FoodDrinkApp.Models;

/// <summary>
/// Food/drink data model. Supports both JSON (MockAPI) and SQLite (local database).
/// </summary>
[Table("foods")]
public sealed class FoodItem
{
    [PrimaryKey]
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("calories")]
    public int Calories { get; set; }

    [JsonPropertyName("protein")]
    public int Protein { get; set; }

    [JsonPropertyName("carbs")]
    public int Carbs { get; set; }

    [JsonPropertyName("fat")]
    public int Fat { get; set; }

    [JsonPropertyName("allergyNote")]
    public string AllergyNote { get; set; } = string.Empty;

    [JsonPropertyName("tags")]
    public string Tags { get; set; } = string.Empty;

    [Ignore]
    [JsonIgnore]
    public string CaloriesLabel => $"{Calories} kcal";

    [Ignore]
    [JsonIgnore]
    public string MacroSummary => $"Protein {Protein}g, carbs {Carbs}g, fat {Fat}g";

    /// <summary>
    /// 动态映射：无论后端数据是纯文本（如"Breakfast"）还是已带 Emoji（如"Breakfast 🥪"），
    /// 统一返回带 Emoji 的显示字符串，兼容大小写。
    /// </summary>
    [Ignore]
    [JsonIgnore]
    public string CategoryWithEmoji => MapCategoryToEmoji(Category);

    [Ignore]
    [JsonIgnore]
    public string AccessibleSummary => $"{Name}. {Category}. {Calories} kcal. {MacroSummary}. {AllergyNote}";

    /// <summary>
    /// 静态映射方法：供外部 Converter 或任意代码复用，确保映射逻辑只有一处定义。
    /// </summary>
    public static string MapCategoryToEmoji(string? category)
    {
        if (string.IsNullOrWhiteSpace(category))
            return string.Empty;

        var trimmed = category.Trim();

        if (trimmed.StartsWith("Breakfast", StringComparison.OrdinalIgnoreCase))
            return "Breakfast 🥪";
        if (trimmed.StartsWith("Lunch", StringComparison.OrdinalIgnoreCase))
            return "Lunch 🍛";
        if (trimmed.StartsWith("Dinner", StringComparison.OrdinalIgnoreCase))
            return "Dinner 🥙";
        if (trimmed.StartsWith("Snack", StringComparison.OrdinalIgnoreCase))
            return "Snack 🍬";
        if (trimmed.StartsWith("Drink", StringComparison.OrdinalIgnoreCase))
            return "Drink 🍹";

        // 未知分类或无法匹配时原样返回
        return trimmed;
    }
}
