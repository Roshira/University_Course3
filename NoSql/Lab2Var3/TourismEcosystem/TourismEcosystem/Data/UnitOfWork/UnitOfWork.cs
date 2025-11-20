using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TourismEcosystem.Model; // Переконайтеся, що ці using'и правильні
using TourismEcosystem.Repositories; // Переконайтеся, що ці using'и правильні
using TourismEcosystem.Data; // Переконайтеся, що ці using'и правильні

namespace TourismEcosystem.Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IDbConnection _connection;
        private readonly IDbTransaction _transaction;

        // Lazy-loading для репозиторіїв
        private ITourRepository? _tourRepository;

        public UnitOfWork(string connectionString)
        {
            _connection = new NpgsqlConnection(connectionString);
            _connection.Open();
            _transaction = _connection.BeginTransaction();
        }

        // Реалізація доступу до репозиторіїв
        public ITourRepository Tours => _tourRepository ??= new TourRepository(_connection, _transaction);

        // public IAccommodationRepository Accommodations => ...

        // --- Реалізація складних транзакцій ---

        public async Task<long> CreateBookingAsync(Booking booking)
        {
            // Ця частина коду правильна і залишається без змін
            const string sp = "sp_create_booking";
            var itemsJson = JsonSerializer.Serialize(new { items = booking.Items });

            var parameters = new DynamicParameters();
            parameters.Add("p_user_id", booking.UserId);
            parameters.Add("p_items", itemsJson);
            parameters.Add("p_booking_id", dbType: DbType.Int64, direction: ParameterDirection.Output);

            await _connection.ExecuteAsync(
                sp,
                parameters,
                transaction: _transaction,
                commandType: CommandType.StoredProcedure
            );

            long newBookingId = parameters.Get<long>("p_booking_id");
            booking.BookingId = newBookingId;
            return newBookingId;
        }

        // --- Керування транзакцією (ВИПРАВЛЕНО) ---

        // Змінюємо public async Task -> public Task
        public Task CommitAsync()
        {
            try
            {
                // ВИПРАВЛЕНО: Викликаємо синхронний Commit()
                _transaction.Commit();
            }
            catch
            {
                // ВИПРАВЛЕНО: Викликаємо синхронний Rollback()
                _transaction.Rollback();
                throw;
            }
            // Повертаємо завершений Task, щоб задовольнити інтерфейс
            return Task.CompletedTask;
        }

        // Змінюємо public async Task -> public Task
        public Task RollbackAsync()
        {
            // ВИПРАВЛЕНО: Викликаємо синхронний Rollback()
            _transaction.Rollback();
            return Task.CompletedTask;
        }

        // Змінюємо public async ValueTask -> public ValueTask
        public ValueTask DisposeAsync()
        {
            // ВИПРАВЛЕНО: Викликаємо синхронний Dispose()
            _transaction.Dispose();
            _connection.Dispose();
            // Повертаємо завершений ValueTask, щоб задовольнити інтерфейс
            return ValueTask.CompletedTask;
        }
    }
}