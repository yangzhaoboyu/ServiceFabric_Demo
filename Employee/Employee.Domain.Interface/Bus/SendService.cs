using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace Employee.Domain.Interface.Bus
{
    public class SendService
    {
        private static readonly string ServiceBusConnectionString = "Endpoint=sb://masstransitbuild.servicebus.chinacloudapi.cn/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=TtEDnMGcajxzxL7WusoNfxZ9DoskU/9GVOeKGFp/xgA=";
        private static readonly string ServiceBusQueueName = "sample-quartz-scheduler";
        private readonly MessageSender messageSender = InitMessageSender();

        public async Task SendMessageAsync<T>(T model)
        {
            await this.messageSender.SendAsync(new BrokeredMessage(JsonConvert.SerializeObject(model)));
        }

        private static MessageSender InitMessageSender()
        {
            MessagingFactory factory = MessagingFactory.CreateFromConnectionString(ServiceBusConnectionString);
            factory.RetryPolicy = RetryPolicy.Default;
            return factory.CreateMessageSender(ServiceBusQueueName);
        }
    }
}