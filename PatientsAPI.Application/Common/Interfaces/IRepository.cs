using PatientsAPI.Domain.Common;
using PatientsAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PatientsAPI.Application.Common.Interfaces
{
    public interface IRepository<T> where T : BaseEntity, IAggregateRoot
    {
        Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

        void Update(T entity);

        void Delete(T entity);

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyList<T>> FindWithDatePrefixAsync(
            Expression<Func<T, DateTime>> dateSelector,
            string dateParameter,
            CancellationToken cancellationToken = default);
    }
}
