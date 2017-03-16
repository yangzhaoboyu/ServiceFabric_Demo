namespace ConsumerConfigure.Domain.Interface.Models.Request
{
    public class ConsumerConfigureQueryRequestModel
    {
        /// <summary>
        ///     操作标识
        /// </summary>
        public int Action { get; set; }

        /// <summary>
        ///     服务名称
        /// </summary>
        public string AppName { get; set; }

        /// <summary>
        ///     字典标识
        /// </summary>
        public string DictionaryKey { get; set; }
    }
}