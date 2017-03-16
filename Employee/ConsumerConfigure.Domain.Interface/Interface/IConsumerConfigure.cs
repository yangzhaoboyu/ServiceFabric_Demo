using System.Threading.Tasks;
using ConsumerConfigure.Domain.Interface.Models.Request;
using ConsumerConfigure.Domain.Interface.Models.Response;
using Microsoft.ServiceFabric.Services.Remoting;

namespace ConsumerConfigure.Domain.Interface.Interface
{
    public interface IConsumerConfigure : IService
    {
        /// <summary>
        ///     Queries the configuration.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        Task<ConsumerConfigureQueryResponseModel> QueryConfiguration(ConsumerConfigureQueryRequestModel request);

        /// <summary>
        ///     Registers the configuration.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        Task<bool> RegisterConfiguration(ConsumerConfigureRequestModel request);
    }
}