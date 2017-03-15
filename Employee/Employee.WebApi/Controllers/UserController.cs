using System;
using System.Threading.Tasks;
using System.Web.Http;
using Employee.Domain.Interface;
using Employee.Domain.Interface.Models.Request;
using Employee.WebApi.Models.Request;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace Employee.WebApi.Controllers
{
    /// <summary>
    ///     LoginController.
    /// </summary>
    [ServiceRequestActionFilter]
    [RoutePrefix("User")]
    public class UserController : ApiController
    {
        /// <summary>
        ///     用户登陆
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        [Route("Login")]
        public async Task<IHttpActionResult> Login(LoginRequest request)
        {
            //    this.RequestContext.Principal.Identity;
            //    Thread.CurrentPrincipal.Identity

            IUserDomainService client = ServiceProxy.Create<IUserDomainService>(new Uri("fabric:/Employee/Service"), new ServicePartitionKey(0));
            bool isSuc = await client.Login(new UserLoginRequestModel
            {
                CellPhone = request.CallPhone,
                PassWord = request.PassWord
            });
            return this.Ok(isSuc);
        }

        /// <summary>
        ///     用户注册
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        [Route("Register")]
        public async Task<IHttpActionResult> Register(RegisterRequest request)
        {
            IUserDomainService client = ServiceProxy.Create<IUserDomainService>(new Uri("fabric:/Employee/Service"), new ServicePartitionKey(0));
            UserRegisterRequestModel response = await client.Register(new UserRegisterRequestModel
            {
                CellPhone = request.CellPhone,
                PassWord = request.PassWord,
                RealName = request.RealName
            });
            return this.Ok(response);
        }
    }
}