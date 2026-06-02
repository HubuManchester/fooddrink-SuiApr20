using Microsoft.Maui.Storage;
using System.Net.Http.Json;
using System.Text.Json;
using FoodDrinkApp.Models;

namespace FoodDrinkApp.Services;

/// <summary>
/// ����ʳƷ����Ʒ����Ŀ¼����
/// ��ѭ�ϸ�Ĵ���淶�������߿������ݼܹ���mockAPI -> �����ļ��־û� -> ��̬�ڴ涵�ף�
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

    // ���س־û������ļ�·��
    private static readonly string LocalCacheFilePath = Path.Combine(FileSystem.AppDataDirectory, "food_catalog_cache.json");

    private static List<FoodItem> _cachedItems = new();

    public static bool LastLoadUsedMockApi { get; private set; }

    // ��ʦҪ��ľ�̬��ʼ���ݣ�ȷ��Ӧ���ڼ��˶��������Ҳ���Բ�������
    private static readonly List<FoodItem> LocalFallbackItems = new()
    {
        new()
        {
            Id = "1",
            Name = "Berry Yogurt Bowl",
            Category = "Breakfast 🥪",
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
            Category = "Lunch 🍛",
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
    /// �첽��ȡ����ʳƷ��¼
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
    /// ����ID��ѯ����ʳƷ����
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
    /// ����������ɸѡ�߼����ɹ���� MainPage.xaml.cs �����Ĺؼ��㣩
    /// </summary>
    public static async Task<IReadOnlyList<FoodItem>> SearchAsync(string? query)
    {
        // ȷ�����ػ����ѳ�ʼ��
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
    /// �첽������ʳƷ��¼��ͬʱ���͵��ƶ˲��־û������أ�
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
                // ������������
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