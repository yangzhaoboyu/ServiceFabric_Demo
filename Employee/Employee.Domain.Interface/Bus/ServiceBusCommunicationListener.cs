using System;
using System.Fabric;
using System.Fabric.Description;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Microsoft.ServiceFabric.Services.Communication.Runtime;

namespace Employee.Domain.Interface.Bus
{
    internal class ServiceBusCommunicationListener : ICommunicationListener, IDisposable
    {
        private readonly string serviceBusConnectionString;
        private readonly string serviceBusQueueName;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ServiceBusCommunicationListener" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="serviceBusConnectionString">The service bus connection string.</param>
        /// <param name="serviceBusQueueName">Name of the service bus queue.</param>
        private ServiceBusCommunicationListener(ServiceContext context, string serviceBusConnectionString, string serviceBusQueueName)
        {
            if (string.IsNullOrWhiteSpace(this.serviceBusConnectionString) || string.IsNullOrWhiteSpace(this.serviceBusQueueName))
            {
                ICodePackageActivationContext codePackageContext = context.CodePackageActivationContext;
                ConfigurationPackage configPackage = codePackageContext.GetConfigurationPackageObject("Config");
                ConfigurationSection configSection = configPackage.Settings.Sections["ServiceBusSettings"];
                this.serviceBusConnectionString = serviceBusConnectionString ?? configSection.Parameters["ServiceBusConnectionString"].Value;
                this.serviceBusQueueName = serviceBusQueueName ?? configSection.Parameters["ServiceBusQueueName"].Value;
            }
            else
            {
                this.serviceBusConnectionString = serviceBusQueueName;
                this.serviceBusQueueName = serviceBusQueueName;
            }
        }

        protected QueueClient ServiceBusClient { get; private set; }

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

        Task<string> ICommunicationListener.OpenAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        #endregion ICommunicationListener Members

        #region IDisposable Members

        /// <summary>
        ///     执行与释放或重置非托管资源相关的应用程序定义的任务。
        /// </summary>
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion IDisposable Members

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
        public Task OpenAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(this.serviceBusConnectionString) || string.IsNullOrWhiteSpace(this.serviceBusQueueName))
                throw new ArgumentNullException("ServiceBusConnectionString Or ServiceBusQueueName Configuration Is Empty");
            this.ServiceBusClient = QueueClient.CreateFromConnectionString(this.serviceBusConnectionString, this.serviceBusQueueName);
            this.ServiceBusClient.OnMessage(message =>
            {
                string messageBody = message.GetBody<string>();
            });
            return Task.FromResult(true);
        }
    }
}