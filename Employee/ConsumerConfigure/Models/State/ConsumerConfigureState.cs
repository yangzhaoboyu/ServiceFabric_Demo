using System;

namespace ConsumerConfigure.Models.State
{
    public class ConsumerConfigureState
    {
        /// <summary>
        ///     操作标识
        /// </summary>
        public int Action { get; set; }

        /// <summary>
        ///     消费地址
        /// </summary>
        public Uri Address { get; set; }

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