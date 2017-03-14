using System;
using System.IO;
using System.Web.Http;
using Owin;
using Swashbuckle.Application;

namespace Employee.WebApi
{
    /// <summary>
    /// </summary>
    public static class Startup
    {
        // 此代码会配置 Web API。启动类指定为
        // WebApp.Start 方法中的类型参数。
        /// <summary>
        /// </summary>
        /// <param name="appBuilder"></param>
        public static void ConfigureApp(IAppBuilder appBuilder)
        {
            // 配置自托管的 Web API。
            HttpConfiguration config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{id}", new { id = RouteParameter.Optional });

            //Swagger 配置
            config.EnableSwagger(c =>
            {
                c.SingleApiVersion("V1", "Employee.WebApi");
                c.IncludeXmlComments(Path.Combine(Environment.CurrentDirectory, "Employee.WebApi.XML"));
            }).EnableSwaggerUi();
            appBuilder.UseWebApi(config);
        }
    }
}