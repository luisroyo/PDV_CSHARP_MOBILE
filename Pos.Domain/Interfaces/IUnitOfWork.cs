using System;
using System.Threading.Tasks;

namespace Pos.Domain.Interfaces
{
    /// <summary>
    /// Interface para Unit of Work - gerencia transações
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Inicia uma nova transação
        /// </summary>
        Task BeginTransactionAsync();

        /// <summary>
        /// Confirma a transação atual
        /// </summary>
        Task CommitAsync();

        /// <summary>
        /// Desfaz a transação atual
        /// </summary>
        Task RollbackAsync();

        /// <summary>
        /// Salva as alterações pendentes
        /// </summary>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// Executa uma operação dentro de uma transação
        /// </summary>
        Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation);
    }
}
