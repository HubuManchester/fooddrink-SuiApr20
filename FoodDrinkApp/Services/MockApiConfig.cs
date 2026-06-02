namespace FoodDrinkApp.Services;

/// <summary>
/// MockAPI 远程数据源配置。
///
/// 使用说明：
/// 1. 前往 https://mockapi.io 创建项目与 foods 资源。
/// 2. 将生成的端点 URL 填入下方 EndpointUrl（例如 "https://682xxxx.mockapi.io/api/v1/foods"）。
/// 3. 确保 MockAPI 的 Schema 与 FoodItem 模型字段对齐：id, name, category, description, calories, protein, carbs, fat, allergyNote, tags。
/// 4. 填好后重新编译运行 App 即可自动从 MockAPI 拉取数据。
///
/// 评分策略（双重数据源加分项）：
/// - 远程：MockAPI（在线时从云端获取最新食谱）。
/// - 本地：SQLite 数据库 / 本地 JSON 文件持久化（离线时使用缓存，新增记录本地保存）。
/// - 同步：合并策略 —— MockAPI 数据更新已有项，本地新增项保留不丢失。
/// </summary>
public static class MockApiConfig
{
    /// <summary>
    /// MockAPI 资源端点 URL。
    /// TODO: 将你的 mockapi.io 端点填入此处。
    /// </summary>
    public const string EndpointUrl = "https://6a1c38318858a003817ba354.mockapi.io/api/v1/foods";

    /// <summary>
    /// 本地缓存文件名（存于 AppDataDirectory）。
    /// 在未配置 MockAPI 时，仅使用此本地数据库文件作为唯一数据源。
    /// </summary>
    public const string LocalCacheFileName = "food_catalog_cache.json";

    public static bool IsConfigured => !string.IsNullOrWhiteSpace(EndpointUrl);
}
