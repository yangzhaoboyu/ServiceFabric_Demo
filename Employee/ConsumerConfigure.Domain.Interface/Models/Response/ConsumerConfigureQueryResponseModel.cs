using System.Collections.Generic;
using System.Security.Policy;

namespace ConsumerConfigure.Domain.Interface.Models.Response
{
    public class ConsumerConfigureQueryResponseInfo
    {
        /// <summary>
        ///     操作标识
        /// </summary>
        public int Action { get; set; }

        /// <summary>
        ///     消费地址
        /// </summary>
        public Url Address { get; set; }

        /// <summary>
        ///     字典标识
        /// </summary>
        public string DictionaryKey { get; set; }

        /// <summary>
        ///     服务名称
        /// </summary>
        public string ServiceName { get; set; }
    }

    public class ConsumerConfigureQueryResponseModel
    {
        public List<ConsumerConfigureQueryResponseInfo> Configure { get; set; }
        public int ResultCode { get; set; }
        public string ResultDesc { get; set; }
    }
}