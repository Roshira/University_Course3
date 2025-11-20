using MongoDB.Driver;
using TourismEcosystem.Model.NoSql;

namespace TourismEcosystem.Repositories.NoSql
{
    public class MongoTourRepository
    {
        private readonly IMongoCollection<TourDocument> _collection;

        public MongoTourRepository(string connectionString)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase("tourism_mongo_db");
            // Працюємо з колекцією "tours"
            _collection = database.GetCollection<TourDocument>("tours");
        }

        // Створити новий тур
        public async Task CreateAsync(TourDocument tour)
        {
            await _collection.InsertOneAsync(tour);
        }

        // Знайти тур за ID з SQL бази
        public async Task<TourDocument?> GetBySqlIdAsync(long sqlId)
        {
            var filter = Builders<TourDocument>.Filter.Eq(t => t.SqlTourId, sqlId);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        // Додати відгук (без перезапису всього документу - це дуже швидко)
        public async Task AddReviewAsync(long sqlId, ReviewDocument review)
        {
            var filter = Builders<TourDocument>.Filter.Eq(t => t.SqlTourId, sqlId);
            var update = Builders<TourDocument>.Update.Push(t => t.Reviews, review);

            await _collection.UpdateOneAsync(filter, update);
        }

        // Отримати всі тури (для тесту)
        public async Task<List<TourDocument>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task DeleteBySqlIdAsync(long sqlId)
        {
            var filter = Builders<TourDocument>.Filter.Eq(t => t.SqlTourId, sqlId);
            await _collection.DeleteOneAsync(filter);
        }
    }
}