using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading.Tasks;
using ConsumerConfigure.Domain.Interface.Interface;
using ConsumerConfigure.Domain.Interface.Models.Request;
using ConsumerConfigure.Domain.Interface.Models.Response;
using ConsumerConfigure.Models.State;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace ConsumerConfigure
{
    /// <summary>
    ///     通过 Service Fabric 运行时为每个服务副本创建此类的一个实例。
    /// </summary>
    public sealed class ConsumerConfigure : StatefulService, IConsumerConfigure
    {
        private static readonly string ConfigureKey = "ConsumerConfigure";
        private static readonly string DictionaryKey = "DictionaryConfigure";

        public ConsumerConfigure(StatefulServiceContext context)
            : base(context)
        {
        }

        #region IConsumerConfigure Members

        /// <summary>
        ///     Queries the configuration.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public async Task<ConsumerConfigureQueryResponseModel> QueryConfiguration(ConsumerConfigureQueryRequestModel request)
        {
            IReliableDictionary<string, ConsumerConfiguresState> dictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, ConsumerConfiguresState>>(DictionaryKey);
            using (ITransaction tx = this.StateManager.CreateTransaction())
            {
                ConditionalValue<ConsumerConfiguresState> result = await dictionary.TryGetValueAsync(tx, ConfigureKey);
                if (!result.HasValue) return null;
                List<ConsumerConfigureInfo> configureState = result.Value.Configures.Where(x => x.Action.Equals(request.Action) && x.ServiceName.Equals(request.AppName) && x.DictionaryKey.Equals(request.DictionaryKey)).ToList();
                ConsumerConfigureQueryResponseModel response = new ConsumerConfigureQueryResponseModel
                {
                    ResultCode = 1,
                    ResultDesc = "查询成功",
                    Configure = configureState.ConvertAll(x => (new ConsumerConfigureQueryResponseInfo
                    {
                        Action = x.Action,
                        Address = x.Address,
                        DictionaryKey = x.DictionaryKey,
                        ServiceName = x.ServiceName
                    }))
                };
                return response;
            }
        }

        /// <summary>
        ///     Registers the configuration.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public async Task<bool> RegisterConfiguration(ConsumerConfigureRequestModel request)
        {
            IReliableDictionary<string, ConsumerConfiguresState> dictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, ConsumerConfiguresState>>(DictionaryKey);
            using (ITransaction tx = this.StateManager.CreateTransaction())
            {
                ConditionalValue<ConsumerConfiguresState> result = await dictionary.TryGetValueAsync(tx, ConfigureKey);
                bool isSuc;
                if (result.HasValue)
                {
                    int count = result.Value.Configures.Count(x => x.Action.Equals(request.Action) && x.ServiceName.Equals(request.ServiceName) && x.DictionaryKey.Equals(request.DictionaryKey));
                    if (count > 0) return false;
                    ConsumerConfiguresState configure = result.Value;
                    configure.Configures.Add(new ConsumerConfigureInfo
                    {
                        Action = request.Action,
                        Address = request.Address,
                        ServiceName = request.ServiceName,
                        DictionaryKey = request.DictionaryKey
                    });
                    isSuc = await dictionary.TryUpdateAsync(tx, ConfigureKey, configure, result.Value);
                    return isSuc;
                }
                ConsumerConfiguresState state = new ConsumerConfiguresState
                {
                    Configures = new List<ConsumerConfigureInfo>
                    {
                        new ConsumerConfigureInfo
                        {
                            Action = request.Action,
                            Address = request.Address,
                            ServiceName = request.ServiceName,
                            DictionaryKey = request.DictionaryKey
                        }
                    }
                };
                isSuc = await dictionary.TryAddAsync(tx, ConfigureKey, state);
                await tx.CommitAsync();
                return isSuc;
            }
        }

        #endregion IConsumerConfigure Members

        /// <summary>
        ///     可选择性地替代以创建侦听器(如 HTTP、服务远程、WCF 等)，从而使此服务副本可处理客户端或用户请求。
        /// </summary>
        /// <remarks>
        ///     有关服务通信的详细信息，请参阅 https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>侦听器集合。</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[] { new ServiceReplicaListener(context => this.CreateServiceRemotingListener(context)) };
        }
    }
}