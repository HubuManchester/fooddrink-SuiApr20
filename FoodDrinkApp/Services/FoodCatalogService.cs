using Microsoft.Maui.Storage;
using System.Net.Http.Json;
using System.Text.Json;
using FoodDrinkApp.Models;

namespace FoodDrinkApp.Services;

/// <summary>
/// 核心食品与饮品数据目录服务
/// 遵循严格的代码规范：三级高可用数据架构（mockAPI -> 本地文件持久化 -> 静态内存兜底）
/// </summary>
public static class FoodCatalogService
{
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // 本地持久化缓存文件路径
    private static readonly string LocalCacheFilePath = Path.Combine(FileSystem.AppDataDirectory, "food_catalog_cache.json");

    private static List<FoodItem> _cachedItems = new();

    public static bool LastLoadUsedMockApi { get; private set; }

    // 老师要求的静态初始数据，确保应用在极端断网情况下也绝对不会闪退
    private static readonly List<FoodItem> LocalFallbackItems = new()
    {
        new()
        {
            Id = "1",
            Name = "Berry Yogurt Bowl",
            Category = "Breakfast",
            Description = "Greek yogurt with mixed berries, oats, and a small drizzle of honey.",
            Calories = 340,
            Protein = 24,
            Carbs = 42,
            Fat = 8,
            AllergyNote = "Contains dairy and gluten.",
            Tags = "healthy breakfast yogurt berries"
        },
        new()
        {
            Id = "2",
            Name = "Chicken Brown Rice Box",
            Category = "Lunch",
            Description = "Grilled chicken breast with brown rice, spinach, cucumber, and lemon dressing.",
            Calories = 520,
            Protein = 38,
            Carbs = 58,
            Fat = 14,
            AllergyNote = "No common allergens recorded.",
            Tags = "meal prep protein lunch"
        }
    };

    /// <summary>
    /// 异步获取所有食品记录
    /// </summary>
    public static async Task<IReadOnlyList<FoodItem>> GetCatalogAsync(bool forceRefresh = false)
    {
        if (!forceRefresh && _cachedItems.Count > 0)
        {
            return _cachedItems;
        }

        if (_cachedItems.Count == 0)
        {
            LoadFromLocalFile();
        }

        if (MockApiConfig.IsConfigured)
        {
            try
            {
                var items = await HttpClient.GetFromJsonAsync<List<FoodItem>>(MockApiConfig.EndpointUrl, JsonOptions);
                if (items != null && items.Count > 0)
                {
                    _cachedItems = items;
                    LastLoadUsedMockApi = true;
                    SaveToLocalFile();
                    return _cachedItems;
                }
            }
            catch
            {
                LastLoadUsedMockApi = false;
            }
        }

        if (_cachedItems.Count == 0)
        {
            _cachedItems = new List<FoodItem>(LocalFallbackItems);
            SaveToLocalFile();
        }

        return _cachedItems;
    }

    /// <summary>
    /// 根据ID查询单条食品详情
    /// </summary>
    public static async Task<FoodItem?> GetByIdAsync(string id)
    {
        if (_cachedItems.Count == 0)
        {
            await GetCatalogAsync();
        }
        return _cachedItems.FirstOrDefault(item => item.Id == id);
    }

    /// <summary>
    /// 核心搜索与筛选逻辑（成功解决 MainPage.xaml.cs 报错的关键点）
    /// </summary>
    public static async Task<IReadOnlyList<FoodItem>> SearchAsync(string? query)
    {
        // 确保本地缓存已初始化
        if (_cachedItems.Count == 0)
        {
            await GetCatalogAsync();
        }

        if (string.IsNullOrWhiteSpace(query))
        {
            return _cachedItems;
        }

        var lowerQuery = query.ToLowerInvariant();
        return _cachedItems.Where(item =>
            (item.Name?.ToLowerInvariant().Contains(lowerQuery) ?? false) ||
            (item.Category?.ToLowerInvariant().Contains(lowerQuery) ?? false) ||
            (item.Tags?.ToLowerInvariant().Contains(lowerQuery) ?? false) ||
            (item.Description?.ToLowerInvariant().Contains(lowerQuery) ?? false)
        ).ToList();
    }

    /// <summary>
    /// 异步添加新食品记录（同时推送到云端并持久化到本地）
    /// </summary>
    public static async Task<FoodItem> AddAsync(FoodItem item)
    {
        if (string.IsNullOrEmpty(item.Id))
        {
            item.Id = Guid.NewGuid().ToString();
        }

        if (MockApiConfig.IsConfigured)
        {
            try
            {
                var response = await HttpClient.PostAsJsonAsync(MockApiConfig.EndpointUrl, item, JsonOptions);
                if (response.IsSuccessStatusCode)
                {
                    var created = await response.Content.ReadFromJsonAsync<FoodItem>(JsonOptions);
                    if (created != null)
                    {
                        item = created;
                    }
                }
            }
            catch
            {
                // 允许离线添加
            }
        }

        _cachedItems.Add(item);
        SaveToLocalFile();

        return item;
    }

    private static void SaveToLocalFile()
    {
        try
        {
            var json = JsonSerializer.Serialize(_cachedItems, JsonOptions);
            File.WriteAllText(LocalCacheFilePath, json);
        }
        catch
        {
        }
    }

    private static void LoadFromLocalFile()
    {
        try
        {
            if (File.Exists(LocalCacheFilePath))
            {
                var json = File.ReadAllText(LocalCacheFilePath);
                var items = JsonSerializer.Deserialize<List<FoodItem>>(json, JsonOptions);
                if (items != null)
                {
                    _cachedItems = items;
                }
            }
        }
        catch
        {
            _cachedItems = new List<FoodItem>();
        }
    }
}