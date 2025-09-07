using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Pos.Domain.Entities.Base;

namespace Pos.Domain.Interfaces
{
    /// <summary>
    /// Interface base para repositórios
    /// </summary>
    public interface IRepository<T> where T : Entity
    {
        /// <summary>
        /// Obtém uma entidade por ID
        /// </summary>
        Task<T> GetByIdAsync(Guid id);

        /// <summary>
        /// Obtém todas as entidades
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Obtém entidades baseado em uma expressão
        /// </summary>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Obtém a primeira entidade que atende ao critério
        /// </summary>
        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Adiciona uma nova entidade
        /// </summary>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// Atualiza uma entidade existente
        /// </summary>
        Task UpdateAsync(T entity);

        /// <summary>
        /// Remove uma entidade
        /// </summary>
        Task DeleteAsync(T entity);

        /// <summary>
        /// Remove uma entidade por ID
        /// </summary>
        Task DeleteAsync(Guid id);

        /// <summary>
        /// Verifica se existe uma entidade com o ID especificado
        /// </summary>
        Task<bool> ExistsAsync(Guid id);

        /// <summary>
        /// Conta o número de entidades que atendem ao critério
        /// </summary>
        Task<int> CountAsync(Expression<Func<T, bool>> predicate = null);
    }
}
