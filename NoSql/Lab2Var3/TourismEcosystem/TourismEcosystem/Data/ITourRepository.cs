using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TourismEcosystem.Interface;
using TourismEcosystem.Model;

namespace TourismEcosystem.Data
{
    // Специфічний репозиторій для Турів
    public interface ITourRepository : IRepository<Tour>
    {
        // ЗАПИС (йде через Stored Procedures)
        Task<long> CreateAsync(Tour tour, long createdByUserId);
        Task UpdateAsync(Tour tour, long updatedByUserId);
        Task DeleteAsync(long id, long deletedByUserId);
    }
}
