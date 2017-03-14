using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Employee.WebApi.Models.Request
{
    /// <summary>
    /// </summary>
    public class RegisterRequest
    {
        /// <summary>
        ///     手机号
        /// </summary>
        [JsonProperty("cellPhone")]
        [RegularExpression(@"^(13|14|15|16|17|18)\d{9}$")]
        public string CellPhone { get; set; }

        /// <summary>
        ///     密码
        /// </summary>
        [JsonProperty("passWord")]
        [MaxLength(50)]
        public string PassWord { get; set; }

        /// <summary>
        ///     真实姓名
        /// </summary>
        [JsonProperty("realName")]
        [MaxLength(50)]
        public string RealName { get; set; }
    }
}