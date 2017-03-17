using System;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Web.Http;
using ConsumerConfigure.Domain.Interface.Interface;
using ConsumerConfigure.Domain.Interface.Models.Request;
using ConsumerConfigure.Domain.Interface.Models.Response;
using Employee.Domain.Interface;
using Employee.Domain.Interface.Models.Request;
using Employee.WebApi.Models.Request;
using Employee.WebApi.Models.Response;
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
            //this.RequestContext.Principal.Identity;
            //Thread.CurrentPrincipal.Identity
            //var owin = HttpContext.Current
            //IOwinContext context = this.Request.GetOwinContext();
            //HttpRequestContext newcontext = this.Request.GetRequestContext();
            //HttpRequestContext scontext = this.ActionContext.RequestContext;
            //HttpContext current = HttpContext.Current;
            //OwinContext owinContext = this.User
            IUserDomainService client = ServiceProxy.Create<IUserDomainService>(new Uri("fabric:/Employee/Service"), new ServicePartitionKey(0));

            bool isSuc = await client.Login(new UserLoginRequestModel
            {
                CellPhone = request.CallPhone,
                PassWord = request.PassWord
            });
            return this.Ok(isSuc);
        }

        /// <summary>
        ///     查询注册消费者
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        [Route("QueryConfiguration")]
        public async Task<IHttpActionResult> QueryConfiguration(ConsumerConfigureQueryRequest request)
        {
            IConsumerConfigure client = ServiceProxy.Create<IConsumerConfigure>(new Uri("fabric:/Consumer/ConsumerConfigure"), new ServicePartitionKey(0));
            ConsumerConfigureQueryResponseModel result = await client.QueryConfiguration(new ConsumerConfigureQueryRequestModel
            {
                Action = request.Action,
                AppName = request.AppName,
                DictionaryKey = request.DictionaryKey
            });
            ConsumerConfigureQueryResponse response = new ConsumerConfigureQueryResponse
            {
                ResultCode = result.ResultCode,
                ResultDesc = result.ResultDesc,
                Configure = result.Configure.ConvertAll(x => (new ConsumerConfigureResponse
                {
                    Action = x.Action,
                    Address = x.Address.Value,
                    DictionaryKey = x.DictionaryKey,
                    ServiceName = x.ServiceName
                }))
            };
            return this.Ok(response);
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

        /// <summary>
        ///     分发配置注册
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        [Route("RegisterConfiguration")]
        public async Task<IHttpActionResult> RegisterConfiguration(ConsumerConfigureRequest request)
        {
            IConsumerConfigure client = ServiceProxy.Create<IConsumerConfigure>(new Uri("fabric:/Consumer/ConsumerConfigure"), new ServicePartitionKey(0));
            bool isSuc = await client.RegisterConfiguration(new ConsumerConfigureRequestModel
            {
                Action = request.Action,
                Address = new Url(request.Address),
                ServiceName = request.ServiceName,
                DictionaryKey = request.DictionaryKey
            });
            return this.Ok(isSuc);
        }
    }
}