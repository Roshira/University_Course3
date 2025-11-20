using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TourismEcosystem.Model;

namespace TourismEcosystem.Data
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        // Доступ до всіх репозиторіїв
        ITourRepository Tours { get; }
        // IAccommodationRepository Accommodations { get; }
        // ...

        // --- Методи для складних транзакцій (через SP) ---

        /// <summary>
        /// Створює повне бронювання з усіма елементами в одній транзакції
        /// </summary>
        /// <param name="booking">Об'єкт бронювання з елементами</param>
        /// <param name="userId">Користувач, що створює бронювання</param>
        /// <returns>ID нового бронювання</returns>
        Task<long> CreateBookingAsync(Booking booking);

        // --- Керування транзакцією ---
        Task CommitAsync();
        Task RollbackAsync();
    }
}
