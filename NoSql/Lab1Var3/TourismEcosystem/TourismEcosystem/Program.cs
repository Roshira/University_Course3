using Microsoft.Extensions.Configuration;
using Dapper;
using Npgsql;
using System.Diagnostics;
using System.IO;
using TourismEcosystem.Data.UnitOfWork;
using TourismEcosystem.Model;
using TourismEcosystem.Model.NoSql;
using TourismEcosystem.Repositories.NoSql;

// Налаштування
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

string connectionString = config.GetConnectionString("DefaultConnection");
string mongoConnectionString = "mongodb://localhost:27017";

Console.WriteLine("--- ПІДГОТОВКА СКЛАДНОГО СЦЕНАРІЮ (HEAVY JOIN vs DOCUMENT) ---");

long tourId = 999; // Тестовий ID

// ==========================================
// 1. ПІДГОТОВКА SQL (Створюємо "пекло" для JOIN-ів)
// ==========================================
using (var conn = new NpgsqlConnection(connectionString))
{
    await conn.OpenAsync();

    // Створюємо додаткові таблиці для зв'язків "Багато-до-Багатьох"
    await conn.ExecuteAsync(@"
        CREATE TABLE IF NOT EXISTS TourPhotos (
            photo_id serial PRIMARY KEY,
            tour_id bigint,
            url text
        );
        CREATE TABLE IF NOT EXISTS TourTags (
            tag_id serial PRIMARY KEY,
            tour_id bigint,
            tag_name text
        );
    ");

    // Очищаємо дані для тесту
    await conn.ExecuteAsync("DELETE FROM TourPhotos WHERE tour_id = @Id", new { Id = tourId });
    await conn.ExecuteAsync("DELETE FROM TourTags WHERE tour_id = @Id", new { Id = tourId });
    await conn.ExecuteAsync("DELETE FROM Tours WHERE tour_id = @Id", new { Id = tourId });

    // Створюємо тур
    await conn.ExecuteAsync(@"
        INSERT INTO Tours (tour_id, provider_id, start_city_id, name, description, price, duration_hours)
        VALUES (@Id, 1, 1, 'Complex SQL Tour', 'Testing Joins', 500, 10)", new { Id = tourId });

    // ГЕНЕРУЄМО БАГАТО ДАНИХ
    // 1. 200 Фотографій
    Console.WriteLine("SQL: Генеруємо 200 фото...");
    await conn.ExecuteAsync(@"
        INSERT INTO TourPhotos (tour_id, url)
        SELECT @Id, 'http://photo.com/' || generate_series(1, 200)", new { Id = tourId });

    // 2. 200 Тегів
    Console.WriteLine("SQL: Генеруємо 200 тегів...");
    await conn.ExecuteAsync(@"
        INSERT INTO TourTags (tour_id, tag_name)
        SELECT @Id, 'Tag #' || generate_series(1, 200)", new { Id = tourId });

    // (Примітка: Ми навіть не додаємо відгуки, 200*200 = 40,000 рядків дублікатів вже достатньо, щоб покласти SQL)
}

// ==========================================
// 2. ПІДГОТОВКА MONGO (Один документ)
// ==========================================
Console.WriteLine("Mongo: Створюємо насичений документ...");
var mongoRepo = new MongoTourRepository(mongoConnectionString);

// Видаляємо старий, якщо був (це важливо!)
try
{
    // Використовуємо фільтр для видалення вручну, якщо методу немає в репозиторії
    var client = new MongoDB.Driver.MongoClient(mongoConnectionString);
    var db = client.GetDatabase("tourism_mongo_db");
    var col = db.GetCollection<TourDocument>("tours");
    
    var filter = MongoDB.Driver.Builders<TourDocument>.Filter.Eq(x => x.SqlTourId, tourId);
    await col.DeleteOneAsync(filter);
}
catch { }

var mongoTour = new TourDocument
{
    SqlTourId = tourId,
    Name = "Complex NoSQL Tour",
    Description = "Testing Document Model",
    Price = 500,
    FlexibleAttributes = new Dictionary<string, object>()
};

// Додаємо ті самі дані, але як масиви
var photos = Enumerable.Range(1, 200).Select(i => $"http://photo.com/{i}").ToList();
var tags = Enumerable.Range(1, 200).Select(i => $"Tag #{i}").ToList();

mongoTour.FlexibleAttributes.Add("Photos", photos);
mongoTour.FlexibleAttributes.Add("Tags", tags);

await mongoRepo.CreateAsync(mongoTour);


// ==========================================
// 3. БЕНЧМАРК
// ==========================================
Console.WriteLine("\n--- ПОЧИНАЄМО БИТВУ! ---");

// --- SQL TEST ---
// Завдання: Отримати ВСІ дані про тур (Інфо + Всі фото + Всі теги) одним запитом
using (var conn = new NpgsqlConnection(connectionString))
{
    await conn.OpenAsync();

    // Цей запит провокує Cartesian Product (Декартовий добуток)
    // Кожне фото з'єднається з кожним тегом.
    // 200 фото * 200 тегів = 40,000 рядків поверне база!
    string heavySql = @"
        SELECT t.name, ph.url, tg.tag_name
        FROM Tours t
        LEFT JOIN TourPhotos ph ON t.tour_id = ph.tour_id
        LEFT JOIN TourTags tg ON t.tour_id = tg.tour_id
        WHERE t.tour_id = @Id";

    var sw = Stopwatch.StartNew();
    var result = await conn.QueryAsync(heavySql, new { Id = tourId });
    var count = result.Count(); // Читаємо всі 40,000 рядків
    sw.Stop();

    Console.WriteLine($"SQL (Complex JOIN): {sw.ElapsedMilliseconds} ms");
    Console.WriteLine($"   -> SQL повернув {count} рядків дубльованих даних (Data Explosion)");
}

// --- MONGO TEST ---
var swMongo = Stopwatch.StartNew();
var doc = await mongoRepo.GetBySqlIdAsync(tourId);
// Просто перевіримо, що дані є
int photosCount = ((IEnumerable<object>)doc.FlexibleAttributes["Photos"]).Count();
int tagsCount = ((IEnumerable<object>)doc.FlexibleAttributes["Tags"]).Count();
swMongo.Stop();

Console.WriteLine($"Mongo (Single Doc): {swMongo.ElapsedMilliseconds} ms");
Console.WriteLine($"   -> Mongo повернула 1 документ (Фото: {photosCount}, Теги: {tagsCount})");

Console.WriteLine("\nНатисніть будь-яку клавішу...");
Console.ReadKey();