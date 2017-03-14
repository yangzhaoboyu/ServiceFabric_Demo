using System.Threading.Tasks;
using Employee.Domain.Interface.Models.Request;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Employee.Domain.Interface
{
    public interface IUserDomainService : IService
    {
        /// <summary>
        ///     用户登陆
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<bool> Login(UserLoginRequestModel request);

        /// <summary>
        ///     用户注册
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<UserRegisterRequestModel> Register(UserRegisterRequestModel request);
    }
}