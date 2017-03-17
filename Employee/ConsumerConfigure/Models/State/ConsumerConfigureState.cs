using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Policy;

namespace ConsumerConfigure.Models.State
{
    [DataContract]
    public class ConsumerConfigureInfo
    {
        /// <summary>
        ///     操作标识
        /// </summary>
        [DataMember]
        public int Action { get; set; }

        /// <summary>
        ///     消费地址
        /// </summary>
        [DataMember]
        public Url Address { get; set; }

        /// <summary>
        ///     字典标识
        /// </summary>
        [DataMember]
        public string DictionaryKey { get; set; }

        /// <summary>
        ///     服务名称
        /// </summary>
        [DataMember]
        public string ServiceName { get; set; }
    }

    [DataContract]
    public class ConsumerConfiguresState
    {
        [DataMember]
        public List<ConsumerConfigureInfo> Configures { get; set; }
    }
}