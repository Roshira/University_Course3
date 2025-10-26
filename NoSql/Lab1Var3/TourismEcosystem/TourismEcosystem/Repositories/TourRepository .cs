// Файл: TourRepository.cs
using Dapper;
using Npgsql;
using System.Data;
using TourismEcosystem.Model;
using TourismEcosystem.Data;

namespace TourismEcosystem.Repositories
{
public class TourRepository : ITourRepository
{
    private readonly IDbConnection _connection;
    private readonly IDbTransaction _transaction;

    // Репозиторій отримує з'єднання та транзакцію від UnitOfWork
    public TourRepository(IDbConnection connection, IDbTransaction transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    // --- ЧИТАННЯ (з Views) ---

    public async Task<IEnumerable<Tour>> GetAllAsync()
    {
        // 1. Використовуємо Розріз (View) для отримання даних
        const string sql = "SELECT * FROM v_active_tours_with_rating";
        return await _connection.QueryAsync<Tour>(sql, transaction: _transaction);
    }

    public async Task<Tour?> GetByIdAsync(long id)
    {
        // 2. Використовуємо Розріз (View) для отримання даних
        const string sql = "SELECT * FROM v_active_tours_with_rating WHERE tour_id = @Id";
        return await _connection.QuerySingleOrDefaultAsync<Tour>(
            sql,
            new { Id = id },
            transaction: _transaction
        );
    }

    // --- ЗАПИС (через Stored Procedures) ---

    public async Task<long> CreateAsync(Tour tour, long createdByUserId)
    {
        // 3. Використовуємо Збережену Процедуру
        const string sp = "sp_create_tour"; // Припустимо, у вас є така SP

        var parameters = new DynamicParameters();
        parameters.Add("p_provider_id", tour.ProviderId); // Припустимо, ці поля є
        parameters.Add("p_name", tour.Name);
        parameters.Add("p_description", tour.Description);
        parameters.Add("p_start_city_id", tour.StartCityId);
        parameters.Add("p_price", tour.Price);
        parameters.Add("p_user_id", createdByUserId);
        parameters.Add("p_new_id", dbType: DbType.Int64, direction: ParameterDirection.Output);

        await _connection.ExecuteAsync(
            sp,
            parameters,
            transaction: _transaction,
            commandType: CommandType.StoredProcedure
        );

        return parameters.Get<long>("p_new_id");
    }

    public async Task UpdateAsync(Tour tour, long updatedByUserId)
    {
        // 4. Використовуємо Збережену Процедуру
        const string sp = "sp_update_tour"; // Припустимо, у вас є така SP

        await _connection.ExecuteAsync(
            sp,
            new
            {
                p_tour_id = tour.TourId,
                p_name = tour.Name,
                p_description = tour.Description,
                p_price = tour.Price,
                p_user_id = updatedByUserId
            },
            transaction: _transaction,
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task DeleteAsync(long id, long deletedByUserId)
    {
        // 5. Використовуємо Збережену Процедуру (для Soft Delete)
        const string sp = "sp_soft_delete_tour";

        await _connection.ExecuteAsync(
            sp,
            new { p_tour_id = id, p_user_id = deletedByUserId },
            transaction: _transaction,
            commandType: CommandType.StoredProcedure
        );
    }
}
}