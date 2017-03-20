using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading.Tasks;
using BusNotify;
using Employee.Domain.Interface;
using Employee.Domain.Interface.Backup;
using Employee.Domain.Interface.Bus;
using Employee.Domain.Interface.Models.Request;
using Employee.Service.Models.User;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Data.Notifications;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Employee.Service
{
    /// <summary>
    /// </summary>
    /// <seealso cref="Microsoft.ServiceFabric.Services.Runtime.StatefulService" />
    /// <seealso cref="Employee.Domain.Interface.IUserDomainService" />
    internal sealed class Service : StatefulService, IUserDomainService
    {
        private readonly SendService sendService = new SendService();
        private IBackupStore backupManager;
        private BackupManagerType backupStorageType;

        /// <summary>
        ///     Creates a new StatefulService with default ReliableStateManager.
        /// </summary>
        /// <param name="serviceContext">
        ///     A <see cref="T:System.Fabric.StatefulServiceContext" /> that describes the service context.
        /// </param>
        public Service(StatefulServiceContext serviceContext) : base(serviceContext)
        {
            this.StateManager.StateManagerChanged += this.StateManager_StateManagerChanged;
        }

        #region IUserDomainService Members

        /// <summary>
        ///     Login
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<bool> Login(UserLoginRequestModel request)
        {
            IReliableDictionary<string, UserState> myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, UserState>>("Users");
            using (ITransaction tx = this.StateManager.CreateTransaction())
            {
                ConditionalValue<UserState> result = await myDictionary.TryGetValueAsync(tx, request.CellPhone);
                if (result.HasValue)
                {
                    return result.Value.PassWord.Equals(request.PassWord, StringComparison.OrdinalIgnoreCase);
                }
            }
            return false;
        }

        /// <summary>
        ///     Register
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<UserRegisterRequestModel> Register(UserRegisterRequestModel request)
        {
            IReliableDictionary<string, UserState> myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, UserState>>("Users");
            using (ITransaction tx = this.StateManager.CreateTransaction())
            {
                ConditionalValue<UserState> result = await myDictionary.TryGetValueAsync(tx, request.CellPhone);
                if (!result.HasValue)
                {
                    UserState state = new UserState
                    {
                        CellPhone = request.CellPhone,
                        PassWord = request.PassWord,
                        RealName = request.RealName,
                        UserIdentifier = Guid.NewGuid()
                    };
                    bool isSuc = await myDictionary.TryAddAsync(tx, state.CellPhone, state);
                    await tx.CommitAsync();
                    if (!isSuc) return null;
                    request.UserIdentifier = state.UserIdentifier;
                    return request;
                }
                request.UserIdentifier = result.Value.UserIdentifier;
                return request;
            }
        }

        #endregion IUserDomainService Members

        /// <summary>
        ///     Override this method to supply the communication listeners for the service replica. The endpoints returned by the communication listener's
        ///     are stored as a JSON string of ListenerName, Endpoint string pairs like
        ///     {"Endpoints":{"Listener1":"Endpoint1","Listener2":"Endpoint2" ...}}
        /// </summary>
        /// <returns>
        ///     List of ServiceReplicaListeners
        /// </returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[] { new ServiceReplicaListener(context => this.CreateServiceRemotingListener(context)) };
        }

        /// <summary>
        ///     Handles the DictionaryChanged event of the Dictionary control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="string" /> instance containing the event data.</param>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        private void Dictionary_DictionaryChanged(object sender, NotifyDictionaryChangedEventArgs<string, UserState> e)
        {
            if (this.Partition.WriteStatus != PartitionAccessStatus.Granted) return;
            IReliableDictionary<string, UserState> dictionary = sender as IReliableDictionary<string, UserState>;
            switch (e.Action)
            {
                case NotifyDictionaryChangedAction.Add:
                    NotifyDictionaryItemAddedEventArgs<string, UserState> add = e as NotifyDictionaryItemAddedEventArgs<string, UserState>;
                    this.sendService.SendMessageAsync(new FabricNotifyModel
                    {
                        Action = NotifyAction.Add,
                        Value = add?.Value.ToJson(),
                        ServiceName = this.Context.ServiceName.AbsoluteUri,
                        DictionaryKey = dictionary?.Name.AbsolutePath,
                        Key = null
                    }).GetAwaiter().GetResult();
                    break;

                case NotifyDictionaryChangedAction.Update:
                    NotifyDictionaryItemUpdatedEventArgs<string, UserState> update = e as NotifyDictionaryItemUpdatedEventArgs<string, UserState>;
                    this.sendService.SendMessageAsync(new FabricNotifyModel
                    {
                        Action = NotifyAction.Update,
                        Value = update?.Value.ToJson(),
                        ServiceName = this.Context.ServiceTypeName,
                        DictionaryKey = dictionary?.Name.AbsolutePath,
                        Key = null
                    }).GetAwaiter().GetResult();
                    break;

                case NotifyDictionaryChangedAction.Remove:
                    NotifyDictionaryItemRemovedEventArgs<string, UserState> remove = e as NotifyDictionaryItemRemovedEventArgs<string, UserState>;
                    this.sendService.SendMessageAsync(new FabricNotifyModel
                    {
                        Action = NotifyAction.Delete,
                        Key = remove?.Key,
                        ServiceName = this.Context.ServiceTypeName,
                        DictionaryKey = dictionary?.Name.AbsolutePath,
                        Value = null
                    }).GetAwaiter().GetResult();
                    break;

                case NotifyDictionaryChangedAction.Clear:
                    break;

                case NotifyDictionaryChangedAction.Rebuild:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     Handles the StateManagerChanged event of the StateManager control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="NotifyStateManagerChangedEventArgs" /> instance containing the event data.</param>
        private void StateManager_StateManagerChanged(object sender, NotifyStateManagerChangedEventArgs e)
        {
            NotifyStateManagerSingleEntityChangedEventArgs operation = e as NotifyStateManagerSingleEntityChangedEventArgs;
            if (operation != null && operation.Action == NotifyStateManagerChangedAction.Add)
            {
                if (operation.ReliableState is IReliableDictionary<string, UserState>)
                {
                    IReliableDictionary<string, UserState> dictionary = (IReliableDictionary<string, UserState>)operation.ReliableState;
                    dictionary.DictionaryChanged += this.Dictionary_DictionaryChanged;
                }
            }
        }
    }
}