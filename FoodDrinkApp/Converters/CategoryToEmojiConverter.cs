using System.Globalization;
using FoodDrinkApp.Models;

namespace FoodDrinkApp.Converters;

/// <summary>
/// MAUI 值转换器：将后端存储的纯文本分类（如 "Breakfast"）在 UI 渲染时自动映射为带 Emoji 的字符串（如 "Breakfast 🥪"）。
///
/// 使用方式（XAML）：
/// <![CDATA[
///   xmlns:converters="clr-namespace:FoodDrinkApp.Converters"
///   <ContentPage.Resources>
///     <converters:CategoryToEmojiConverter x:Key="CategoryToEmojiConverter" />
///   </ContentPage.Resources>
///   <Label Text="{Binding Category, Converter={StaticResource CategoryToEmojiConverter}}" />
/// ]]>
///
/// 设计说明：
/// - 复用 FoodItem.MapCategoryToEmoji() 静态方法，确保映射逻辑只有一处定义（Single Source of Truth）。
/// - 兼容大小写：Breakfast / breakfast / BREAKFAST 均正确匹配。
/// - 兼容已带 Emoji 的字符串：若 Category 本身已是 "Breakfast 🥪"，不会重复追加。
/// - 只读转换（ConvertBack 不支持），因为这是一个单向的显示增强。
/// </summary>
public class CategoryToEmojiConverter : IValueConverter
{
    /// <summary>
    /// 将 Category 纯文本转换为带 Emoji 的显示字符串。
    /// </summary>
    /// <param name="value">后端存储的 Category 字符串（可含或不含 Emoji）</param>
    /// <param name="targetType">目标类型（通常为 string）</param>
    /// <param name="parameter">未使用</param>
    /// <param name="culture">未使用</param>
    /// <returns>带 Emoji 的 Category 字符串</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string category)
            return FoodItem.MapCategoryToEmoji(category);

        return value;
    }

    /// <summary>
    /// 单向转换器——不支持反向转换。
    /// </summary>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("CategoryToEmojiConverter 是单向（只读）转换器，不支持 ConvertBack。");
    }
}
