using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TourismEcosystem.Interface
{
    public interface IRepository<T> where T : class
    {
        // ЧИТАННЯ (йде через Views)
        Task<T?> GetByIdAsync(long id);
        Task<IEnumerable<T>> GetAllAsync();
    }
}
