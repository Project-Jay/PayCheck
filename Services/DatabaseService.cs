using SQLite;
using SubscriptionTracker.Models;

namespace SubscriptionTracker.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection? _database;

        private async Task InitAsync()
        {
            if (_database != null) return;

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "subscriptions.db");
            _database = new SQLiteAsyncConnection(dbPath,
                SQLiteOpenFlags.ReadWrite |
                SQLiteOpenFlags.Create |
                SQLiteOpenFlags.SharedCache);

            await _database.CreateTableAsync<SubscriptionModel>();
        }

        // 전체 조회
        public async Task<List<SubscriptionModel>> GetAllAsync()
        {
            await InitAsync();
            return await _database!.Table<SubscriptionModel>()
                .Where(s => s.IsActive)
                .OrderBy(s => s.BillingDay)
                .ToListAsync();
        }

        // 추가
        public async Task<int> AddAsync(SubscriptionModel subscription)
        {
            await InitAsync();
            return await _database!.InsertAsync(subscription);
        }

        // 수정
        public async Task<int> UpdateAsync(SubscriptionModel subscription)
        {
            await InitAsync();
            return await _database!.UpdateAsync(subscription);
        }

        // 삭제 (비활성화)
        public async Task DeleteAsync(SubscriptionModel subscription)
        {
            await InitAsync();
            subscription.IsActive = false;
            await _database!.UpdateAsync(subscription);
        }

        // 미사용 구독 조회
        public async Task<List<SubscriptionModel>> GetUnusedAsync()
        {
            await InitAsync();
            var all = await GetAllAsync();
            return all.Where(s => s.IsUnused).ToList();
        }
    }
}