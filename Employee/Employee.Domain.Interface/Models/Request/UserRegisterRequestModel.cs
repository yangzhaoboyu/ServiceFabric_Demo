using System;

namespace Employee.Domain.Interface.Models.Request
{
    public class UserRegisterRequestModel
    {
        /// <summary>
        ///     手机号
        /// </summary>
        public string CellPhone { get; set; }

        /// <summary>
        ///     密码
        /// </summary>
        public string PassWord { get; set; }

        /// <summary>
        ///     真实姓名
        /// </summary>
        public string RealName { get; set; }

        /// <summary>
        ///     用户唯一标识
        /// </summary>
        public Guid UserIdentifier { get; set; }
    }
}