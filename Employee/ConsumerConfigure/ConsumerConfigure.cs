using System;
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
        private static readonly string DictionaryKey = "ConsumerConfigure";
        private IReliableDictionary<string, List<ConsumerConfigureState>> dictionary;

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
            this.dictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, List<ConsumerConfigureState>>>(DictionaryKey);
            using (ITransaction tx = this.StateManager.CreateTransaction())
            {
                ConditionalValue<List<ConsumerConfigureState>> result = await this.dictionary.TryGetValueAsync(tx, ConfigureKey);
                if (!result.HasValue) return null;
                ConsumerConfigureState configureState = result.Value.First(x => x.Action.Equals(request.Action) && x.ServiceName.Equals(request.AppName) && x.DictionaryKey.Equals(request.DictionaryKey));
                ConsumerConfigureQueryResponseModel response = new ConsumerConfigureQueryResponseModel
                {
                    Action = configureState.Action,
                    Address = configureState.Address,
                    ServiceName = configureState.ServiceName,
                    DictionaryKey = configureState.DictionaryKey
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
            this.dictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, List<ConsumerConfigureState>>>(DictionaryKey);
            using (ITransaction tx = this.StateManager.CreateTransaction())
            {
                ConditionalValue<List<ConsumerConfigureState>> result = await this.dictionary.TryGetValueAsync(tx, ConfigureKey);
                if (result.HasValue) return false;
                int count = result.Value.Count(x => x.Action.Equals(request.Action) && x.ServiceName.Equals(request.ServiceName) && x.DictionaryKey.Equals(request.DictionaryKey));
                if (count > 0) return false;
                List<ConsumerConfigureState> configure = result.Value;
                configure.Add(new ConsumerConfigureState
                {
                    Action = request.Action,
                    Address = new Uri(request.Address),
                    ServiceName = request.ServiceName,
                    DictionaryKey = request.DictionaryKey
                });
                return await this.dictionary.TryUpdateAsync(tx, ConfigureKey, configure, result.Value);
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