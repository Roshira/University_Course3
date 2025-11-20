using StackExchange.Redis;
using System.Text.Json;
using TourismEcosystem.Model; // Ваш namespace з моделями

namespace TourismEcosystem.Services
{
    public class RedisCacheService
    {
        private readonly IDatabase _db;

        public RedisCacheService(string connectionString)
        {
            // Підключення до Redis (зазвичай "localhost:6379")
            var redis = ConnectionMultiplexer.Connect(connectionString);
            _db = redis.GetDatabase();
        }

        // ЗАПИТ 1: Збереження даних (SET) з часом життя (TTL)
        public async Task SetCachedToursAsync(string key, List<Tour> tours, TimeSpan expiry)
        {
            // Серіалізуємо об'єкти в JSON-рядок
            string json = JsonSerializer.Serialize(tours);

            // Зберігаємо в Redis
            await _db.StringSetAsync(key, json, expiry);
            Console.WriteLine($"[Redis] Дані збережено за ключем '{key}'. Час життя: {expiry.TotalMinutes} хв.");
        }

        // ЗАПИТ 2: Отримання даних (GET)
        public async Task<List<Tour>?> GetCachedToursAsync(string key)
        {
            // Намагаємося отримати рядок
            var json = await _db.StringGetAsync(key);

            if (json.IsNullOrEmpty)
            {
                return null; // Ключа немає або він прострочений
            }

            Console.WriteLine($"[Redis] HIT! Дані отримано з кешу за ключем '{key}'.");
            // Десеріалізуємо назад у C# об'єкти
            return JsonSerializer.Deserialize<List<Tour>>(json);
        }

        // ЗАПИТ 3: Перевірка наявності ключа (EXISTS)
        public async Task<bool> KeyExistsAsync(string key)
        {
            return await _db.KeyExistsAsync(key);
        }

        // ЗАПИТ 4: Видалення ключа (DEL) - наприклад, якщо ціни оновилися
        public async Task ClearCacheAsync(string key)
        {
            await _db.KeyDeleteAsync(key);
            Console.WriteLine($"[Redis] Ключ '{key}' видалено.");
        }
    }
}