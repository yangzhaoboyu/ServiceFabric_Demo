using System;
using System.Fabric;
using System.Fabric.Description;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ConsumerConfigure.Domain.Interface.Interface;
using ConsumerConfigure.Domain.Interface.Models.Request;
using ConsumerConfigure.Domain.Interface.Models.Response;
using MessageConsumer;
using Microsoft.ServiceBus.Messaging;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Newtonsoft.Json;

namespace Employee.Domain.Interface.Bus
{
    /// <summary>
    /// </summary>
    /// <seealso cref="Microsoft.ServiceFabric.Services.Communication.Runtime.ICommunicationListener" />
    /// <seealso cref="System.IDisposable" />
    public sealed class ServiceBusCommunicationListener : ICommunicationListener, IDisposable
    {
        /// <summary>
        ///     The service bus connection string
        /// </summary>
        private readonly string serviceBusConnectionString;

        /// <summary>
        ///     The service bus queue name
        /// </summary>
        private readonly string serviceBusQueueName;

        /// <summary>
        ///     The client
        /// </summary>
        private IConsumerConfigure client = ServiceProxy.Create<IConsumerConfigure>(new Uri("fabric:/Consumer/ConsumerConfigure"), new ServicePartitionKey(0));

        /// <summary>
        ///     Initializes a new instance of the <see cref="ServiceBusCommunicationListener" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="serviceBusConnectionString">The service bus connection string.</param>
        /// <param name="serviceBusQueueName">Name of the service bus queue.</param>
        public ServiceBusCommunicationListener(ServiceContext context, string serviceBusConnectionString, string serviceBusQueueName)
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

        /// <summary>
        ///     Gets the processing message.
        /// </summary>
        private ManualResetEvent ProcessingMessage { get; } = new ManualResetEvent(true);

        /// <summary>
        ///     Gets the service bus client.
        /// </summary>
        private QueueClient ServiceBusClient { get; set; }

        #region ICommunicationListener Members

        /// <summary>
        ///     This method causes the communication listener to close. Close is a terminal state and
        ///     this method causes the transition to close ungracefully. Any outstanding operations
        ///     (including close) should be canceled when this method is called.
        /// </summary>
        public void Abort()
        {
            this.Dispose();
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
            this.ProcessingMessage.WaitOne(10);
            this.ProcessingMessage.Dispose();
            return this.ServiceBusClient.CloseAsync();
        }

        /// <summary>
        ///     This method causes the communication listener to be opened. Once the Open
        ///     completes, the communication listener becomes usable - accepts and sends messages.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>
        ///     A <see cref="T:System.Threading.Tasks.Task">Task</see> that represents outstanding operation. The result of the Task is
        ///     the endpoint string.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">ServiceBusConnectionString Or ServiceBusQueueName Configuration Is Empty</exception>
        Task<string> ICommunicationListener.OpenAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(this.serviceBusConnectionString) || string.IsNullOrWhiteSpace(this.serviceBusQueueName))
                throw new ArgumentNullException("ServiceBusConnectionString Or ServiceBusQueueName Configuration Is Empty", new Exception());
            this.ServiceBusClient = QueueClient.CreateFromConnectionString(this.serviceBusConnectionString, this.serviceBusQueueName);
            this.ServiceBusClient.OnMessage(message =>
            {
                try
                {
                    //分发 具体处理
                    this.ProcessingMessage.Reset();
                    dynamic eventSource = JsonConvert.DeserializeObject<dynamic>(message.GetBody<string>());
                    ConsumerConfigureQueryResponseModel result = client.QueryConfiguration(new ConsumerConfigureQueryRequestModel
                    {
                        Action = eventSource.Action,
                        AppName = eventSource.ServiceName,
                        DictionaryKey = eventSource.DictionaryKey
                    }).GetAwaiter().GetResult();
                    if (result.ResultCode == 1)
                    {
                        HttpClient client = new HttpClient();
                    }
                    ServiceEventSource.Current.Message($"Consumer Message {message.GetBody<string>()}");
                }
                catch (Exception)
                {
                    // ignored
                }
                finally
                {
                    this.ProcessingMessage.Set();
                }
            });
            return Task.FromResult(this.serviceBusConnectionString);
        }

        #endregion ICommunicationListener Members

        #region IDisposable Members

        /// <summary>
        ///     执行与释放或重置非托管资源相关的应用程序定义的任务。
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this.Dispose(true);
        }

        #endregion IDisposable Members

        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (!disposing) return;
            this.ProcessingMessage.Set();
            this.ProcessingMessage.Dispose();
        }
    }
}