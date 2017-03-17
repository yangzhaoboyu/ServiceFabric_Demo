using System.Collections.Generic;

namespace Employee.WebApi.Models.Response
{
    internal class ConsumerConfigureQueryResponse
    {
        public List<ConsumerConfigureResponse> Configure { get; set; }
        public int ResultCode { get; set; }
        public string ResultDesc { get; set; }
    }
}