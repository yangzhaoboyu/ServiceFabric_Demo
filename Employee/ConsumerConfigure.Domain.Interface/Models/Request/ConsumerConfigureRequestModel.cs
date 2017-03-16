namespace ConsumerConfigure.Domain.Interface.Models.Request
{
    public class ConsumerConfigureRequestModel
    {
        /// <summary>
        ///     操作标识
        /// </summary>
        public int Action { get; set; }

        /// <summary>
        ///     消费地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        ///     字典标识
        /// </summary>
        public string DictionaryKey { get; set; }

        /// <summary>
        ///     服务名称
        /// </summary>
        public string ServiceName { get; set; }
    }
}