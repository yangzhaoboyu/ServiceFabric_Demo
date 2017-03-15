using System;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;

namespace Employee.Service
{
    internal class JinyinmaoCommunicationListener : ICommunicationListener
    {
        public JinyinmaoCommunicationListener(StatefulServiceContext serviceContext)
        {
        }

        #region ICommunicationListener Members

        /// <summary>
        ///     This method causes the communication listener to close. Close is a terminal state and
        ///     this method causes the transition to close ungracefully. Any outstanding operations
        ///     (including close) should be canceled when this method is called.
        /// </summary>
        public void Abort()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     This method causes the communication listener to close. Close is a terminal state and
        ///     this method allows the communication listener to transition to this state in a
        ///     graceful manner.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>
        ///     A <see cref="T:System.Threading.Tasks.Task">Task</see> that represents outstanding operation.
        /// </returns>
        public Task CloseAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     This method causes the communication listener to be opened. Once the Open
        ///     completes, the communication listener becomes usable - accepts and sends messages.
        /// </summary>
        /// <param name="cancellationToken">
        ///     Cancellation token
        /// </param>
        /// <returns>
        ///     A <see cref="T:System.Threading.Tasks.Task">Task</see> that represents outstanding operation. The result of the Task is
        ///     the endpoint string.
        /// </returns>
        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            ServiceEventSource.Current.Message("Remove Dictionary.");
        }

        #endregion ICommunicationListener Members
    }
}