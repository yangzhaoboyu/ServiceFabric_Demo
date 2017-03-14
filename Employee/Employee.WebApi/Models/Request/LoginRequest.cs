using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Employee.WebApi.Models.Request
{
    /// <summary>
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        ///     手机号
        /// </summary>
        [JsonProperty("cellPhone")]
        [RegularExpression(@"^(13|14|15|16|17|18)\d{9}$")]
        public string CallPhone { get; set; }

        /// <summary>
        ///     用户密码
        /// </summary>
        [JsonProperty("passWord")]
        [MaxLength(50)]
        public string PassWord { get; set; }
    }
}