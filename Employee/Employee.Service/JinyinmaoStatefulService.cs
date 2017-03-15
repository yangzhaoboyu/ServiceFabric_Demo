using System;
using System.Collections.Generic;
using System.Fabric;
using Employee.Service.Models.User;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Data.Notifications;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Employee.Service
{
    public class JinyinmaoStatefulService : StatefulService, IService
    {
        /// <summary>
        ///     Creates a new StatefulService.
        ///     Override this to create a new StatefulService with non-default state manager replica.
        /// </summary>
        /// <param name="serviceContext">
        ///     A <see cref="T:System.Fabric.StatefulServiceContext" /> that describes the service context.
        /// </param>
        /// <param name="reliableStateManagerReplica">
        ///     A <see cref="T:Microsoft.ServiceFabric.Data.IReliableStateManagerReplica" /> that represents a reliable state provider replica.
        /// </param>
        public JinyinmaoStatefulService(StatefulServiceContext serviceContext, IReliableStateManagerReplica reliableStateManagerReplica) : base(serviceContext, reliableStateManagerReplica)
        {
            this.StateManager.StateManagerChanged += this.ReliableStateManagerReplica_StateManagerChanged;
        }

        /// <summary>
        ///     Creates a new StatefulService with default ReliableStateManager.
        /// </summary>
        /// <param name="serviceContext">
        ///     A <see cref="T:System.Fabric.StatefulServiceContext" /> that describes the service context.
        /// </param>
        protected JinyinmaoStatefulService(StatefulServiceContext serviceContext) : base(serviceContext)
        {
            this.StateManager.StateManagerChanged += this.ReliableStateManagerReplica_StateManagerChanged;
        }

        /// <summary>
        ///     Override this method to supply the communication listeners for the service replica. The endpoints returned by the communication listener's
        ///     are stored as a JSON string of ListenerName, Endpoint string pairs like
        ///     {"Endpoints":{"Listener1":"Endpoint1","Listener2":"Endpoint2" ...}}
        /// </summary>
        /// <returns>
        ///     List of ServiceReplicaListeners
        /// </returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners() => new[]
        {
            new ServiceReplicaListener(context => this.CreateServiceRemotingListener(context))
        };

        /// <summary>
        ///     Handles the DictionaryChanged event of the Dictionary control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="string" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
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
        ///     Handles the StateManagerChanged event of the ReliableStateManagerReplica control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Microsoft.ServiceFabric.Data.Notifications.NotifyStateManagerChangedEventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void ReliableStateManagerReplica_StateManagerChanged(object sender, NotifyStateManagerChangedEventArgs e)
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