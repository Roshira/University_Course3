using System.Diagnostics;
using Dapper;
using Npgsql;
using TourismEcosystem.Repositories.NoSql; // Переконайтесь, що namespace правильний

public class BenchmarkService
{
    private readonly string _sqlConnString;
    private readonly MongoTourRepository _mongoRepo;

    public BenchmarkService(string sqlConnString, string mongoConnString)
    {
        _sqlConnString = sqlConnString;
        _mongoRepo = new MongoTourRepository(mongoConnString);
    }

    // ЗМІНА: приймаємо ID туру як параметр
    public async Task RunBenchmarkAsync(long tourId)
    {
        Console.WriteLine($"\n--- ПОЧИНАЄМО БЕНЧМАРК (Tour ID: {tourId}) ---");

        // ================= TEST SQL =================
        // Робимо "холодний старт" (відкриття з'єднання), щоб замір був чесним для запиту
        long sqlTime = 0;
        int sqlCount = 0;

        using (var sqlConn = new NpgsqlConnection(_sqlConnString))
        {
            await sqlConn.OpenAsync();
            string sql = @"
                SELECT t.name, t.description, r.rating, r.comment 
                FROM Tours t 
                LEFT JOIN Reviews r ON t.tour_id = r.tour_id 
                WHERE t.tour_id = @Id";

            var stopwatch = Stopwatch.StartNew();
            var result = await sqlConn.QueryAsync(sql, new { Id = tourId });
            sqlCount = result.Count(); // Матеріалізація
            stopwatch.Stop();
            sqlTime = stopwatch.ElapsedMilliseconds;
        }
        Console.WriteLine($"SQL Time (JOIN): {sqlTime} ms (Знайдено рядків: {sqlCount})");

        // ================= TEST MONGO =================
        var stopwatchMongo = Stopwatch.StartNew();

        var doc = await _mongoRepo.GetBySqlIdAsync(tourId);
        var reviewsCount = doc?.Reviews.Count ?? 0;

        stopwatchMongo.Stop();
        Console.WriteLine($"Mongo Time (Single Doc): {stopwatchMongo.ElapsedMilliseconds} ms (Знайдено відгуків: {reviewsCount})");

        Console.WriteLine("--- Висновок ---");
        if (stopwatchMongo.ElapsedMilliseconds < sqlTime)
            Console.WriteLine("MongoDB швидша, оскільки читає один документ без JOIN.");
        else
            Console.WriteLine("Час порівнянний (можливо, мало даних для суттєвої різниці).");
    }
}