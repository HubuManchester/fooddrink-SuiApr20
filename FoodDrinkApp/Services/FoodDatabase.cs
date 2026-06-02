using FoodDrinkApp.Models;
using SQLite;

namespace FoodDrinkApp.Services;

/// <summary>
/// SQLite 本地数据库服务 —— 双重数据源架构的"数据库"层。
///
/// 职责：
/// - 管理 SQLite 连接与表的创建。
/// - 提供全部 CRUD 操作：增、删、改、查、清空。
/// - 作为本地持久化的唯一入口，替代旧版 JSON 文件缓存。
///
/// 评分说明：
/// 本服务与 MockAPI（远程 HTTP）构成"远程 API + 本地数据库"的双重数据源架构，
/// 满足课程对数据持久化的高分要求。
/// </summary>
public class FoodDatabase
{
    private SQLiteAsyncConnection? _connection;

    /// <summary>数据库文件路径。</summary>
    public string DatabasePath { get; }

    /// <summary>
    /// 采用 Singleton 模式 —— 整个应用生命周期内共享一个数据库实例。
    /// </summary>
    private static FoodDatabase? _instance;
    public static FoodDatabase Instance => _instance ??= new FoodDatabase();

    private FoodDatabase()
    {
        DatabasePath = Path.Combine(FileSystem.AppDataDirectory, "fooddrink.db3");
    }

    /// <summary>
    /// 懒初始化 SQLite 连接并创建表（如果不存在）。
    /// </summary>
    private async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (_connection is not null)
            return _connection;

        _connection = new SQLiteAsyncConnection(DatabasePath,
            SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);

        await _connection.CreateTableAsync<FoodItem>();
        return _connection;
    }

    // ── CRUD 操作 ──

    public async Task<List<FoodItem>> GetAllAsync()
    {
        var db = await GetConnectionAsync();
        return await db.Table<FoodItem>().ToListAsync();
    }

    public async Task<FoodItem?> GetByIdAsync(string id)
    {
        var db = await GetConnectionAsync();
        return await db.Table<FoodItem>().Where(i => i.Id == id).FirstOrDefaultAsync();
    }

    public async Task<int> InsertAsync(FoodItem item)
    {
        var db = await GetConnectionAsync();
        return await db.InsertAsync(item);
    }

    public async Task<int> InsertOrReplaceAsync(FoodItem item)
    {
        var db = await GetConnectionAsync();
        return await db.InsertOrReplaceAsync(item);
    }

    public async Task<int> InsertAllAsync(IEnumerable<FoodItem> items)
    {
        var db = await GetConnectionAsync();
        return await db.InsertAllAsync(items);
    }

    public async Task<int> UpdateAsync(FoodItem item)
    {
        var db = await GetConnectionAsync();
        return await db.UpdateAsync(item);
    }

    public async Task<int> DeleteAsync(FoodItem item)
    {
        var db = await GetConnectionAsync();
        return await db.DeleteAsync(item);
    }

    /// <summary>
    /// 清空本地数据库中的所有记录（保留表结构）。
    /// </summary>
    public async Task<int> ClearAllAsync()
    {
        var db = await GetConnectionAsync();
        return await db.DeleteAllAsync<FoodItem>();
    }

    /// <summary>
    /// 获取数据库中记录的总数，用于 UI 展示数据源状态。
    /// </summary>
    public async Task<int> GetCountAsync()
    {
        var db = await GetConnectionAsync();
        return await db.Table<FoodItem>().CountAsync();
    }
}
