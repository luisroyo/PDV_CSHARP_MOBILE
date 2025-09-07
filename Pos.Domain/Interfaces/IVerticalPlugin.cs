using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Pos.Domain.Interfaces
{
    /// <summary>
    /// Interface base para plugins verticais
    /// </summary>
    public interface IVerticalPlugin
    {
        /// <summary>
        /// Chave única do plugin
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Nome do plugin
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Versão do plugin
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Configura os serviços específicos do plugin
        /// </summary>
        void ConfigureServices(IServiceCollection services, IConfiguration configuration);

        /// <summary>
        /// Configura o modelo de dados específico do plugin
        /// </summary>
        void ConfigureModel(ModelBuilder modelBuilder);

        /// <summary>
        /// Registra as políticas específicas do plugin
        /// </summary>
        void RegisterPolicies(IVerticalPolicyRegistry registry);

        /// <summary>
        /// Registra os handlers de eventos específicos do plugin
        /// </summary>
        void RegisterEventHandlers(IEventHandlerRegistry registry);
    }
}
