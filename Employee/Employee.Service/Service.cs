using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Threading;
using System.Threading.Tasks;
using Employee.Domain.Interface;
using Employee.Domain.Interface.Backup;
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
        private IBackupStore backupManager;
        private BackupManagerType backupStorageType;

        public Service(StatefulServiceContext context)
            : base(context)
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
        ///     This method is called as the final step before completing <see cref="M:System.Fabric.IStatefulServiceReplica.ChangeRoleAsync(System.Fabric.ReplicaRole,System.Threading.CancellationToken)" />.
        ///     Override this method to be notified that ChangeRole has completed for this replica's internal components.
        /// </summary>
        /// <param name="newRole">New <see cref="T:System.Fabric.ReplicaRole" /> for this service replica.</param>
        /// <param name="cancellationToken">Cancellation token to monitor for cancellation requests.</param>
        /// <returns>
        ///     A <see cref="T:System.Threading.Tasks.Task" /> that represents outstanding operation.
        /// </returns>
        protected override Task OnChangeRoleAsync(ReplicaRole newRole, CancellationToken cancellationToken)
        {
            return base.OnChangeRoleAsync(newRole, cancellationToken);
        }

        /// <summary>
        ///     Services that want to implement a processing loop which runs when it is primary and has write status,
        ///     just override this method with their logic.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to monitor for cancellation requests.</param>
        /// <returns>
        ///     A <see cref="T:System.Threading.Tasks.Task">Task</see> that represents outstanding operation.
        /// </returns>
        protected override Task RunAsync(CancellationToken cancellationToken)
        {
            return Task.WhenAll(this.PeriodicTakeBackupAsync(cancellationToken));
        }

        /// <summary>
        ///     Backups the callback asynchronous.
        /// </summary>
        /// <param name="backupInfo">The backup information.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private async Task<bool> BackupCallbackAsync(BackupInfo backupInfo, CancellationToken cancellationToken)
        {
            try
            {
                await this.backupManager.ArchiveBackupAsync(backupInfo, cancellationToken);
            }
            catch (Exception)
            {
                // ignored
            }
            await this.backupManager.DeleteBackupsAsync(cancellationToken);
            return true;
        }

        /// <summary>
        ///     Handles the DictionaryChanged event of the Dictionary control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="string" /> instance containing the event data.</param>
        private void Dictionary_DictionaryChanged(object sender, NotifyDictionaryChangedEventArgs<string, UserState> e)
        {
            if (this.Partition.WriteStatus != PartitionAccessStatus.Granted) return;
            switch (e.Action)
            {
                case NotifyDictionaryChangedAction.Add:
                    ServiceEventSource.Current.Message("Add Dictionary.");
                    return;

                case NotifyDictionaryChangedAction.Update:
                    ServiceEventSource.Current.Message("Update Dictionary.");
                    return;

                case NotifyDictionaryChangedAction.Remove:
                    ServiceEventSource.Current.Message("Remove Dictionary.");
                    return;

                case NotifyDictionaryChangedAction.Clear:
                    break;

                case NotifyDictionaryChangedAction.Rebuild:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     Periodics the take backup asynchronous.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private async Task PeriodicTakeBackupAsync(CancellationToken cancellationToken)
        {
            this.SetupBackupManager();
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (this.backupStorageType == BackupManagerType.None)
                {
                    break;
                }
                await Task.Delay(TimeSpan.FromSeconds(this.backupManager.backupFrequencyInSeconds));
                BackupDescription backupDescription = new BackupDescription(BackupOption.Full, this.BackupCallbackAsync);
                await this.BackupAsync(backupDescription, TimeSpan.FromHours(1), cancellationToken);
            }
        }

        /// <summary>
        ///     Processes the state manager single entity notification.
        /// </summary>
        /// <param name="e">The <see cref="NotifyStateManagerChangedEventArgs" /> instance containing the event data.</param>
        private void ProcessStateManagerSingleEntityNotification(NotifyStateManagerChangedEventArgs e)
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

        /// <summary>
        ///     Setups the backup manager.
        /// </summary>
        /// <exception cref="System.ArgumentException">Unknown backup type</exception>
        private void SetupBackupManager()
        {
            string partitionId = this.Context.PartitionId.ToString("N");
            long minKey = ((Int64RangePartitionInformation)this.Partition.PartitionInfo).LowKey;
            long maxKey = ((Int64RangePartitionInformation)this.Partition.PartitionInfo).HighKey;

            if (this.Context.CodePackageActivationContext != null)
            {
                ICodePackageActivationContext codePackageContext = this.Context.CodePackageActivationContext;
                ConfigurationPackage configPackage = codePackageContext.GetConfigurationPackageObject("Config");
                ConfigurationSection configSection = configPackage.Settings.Sections["Inventory.Service.Settings"];
                string backupSettingValue = configSection.Parameters["BackupMode"].Value;
                if (string.Equals(backupSettingValue, "none", StringComparison.InvariantCultureIgnoreCase))
                {
                    this.backupStorageType = BackupManagerType.None;
                }
                else if (string.Equals(backupSettingValue, "local", StringComparison.InvariantCultureIgnoreCase))
                {
                    this.backupStorageType = BackupManagerType.Local;
                    ConfigurationSection localBackupConfigSection = configPackage.Settings.Sections["Inventory.Service.BackupSettings.Local"];
                    this.backupManager = new DiskBackupManager(localBackupConfigSection, partitionId, minKey, maxKey, codePackageContext.TempDirectory);
                }
                else
                {
                    throw new ArgumentException("Unknown backup type");
                }
            }
        }

        /// <summary>
        ///     Handles the StateManagerChanged event of the StateManager control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="NotifyStateManagerChangedEventArgs" /> instance containing the event data.</param>
        private void StateManager_StateManagerChanged(object sender, NotifyStateManagerChangedEventArgs e)
        {
            if (e.Action != NotifyStateManagerChangedAction.Rebuild)
            {
                this.ProcessStateManagerSingleEntityNotification(e);
            }
        }
    }
}