using System.Web.Http;
using Owin;

namespace Consumer.WebApi
{
    public static class Startup
    {
        // 此代码会配置 Web API。启动类指定为
        // WebApp.Start 方法中的类型参数。
        public static void ConfigureApp(IAppBuilder appBuilder)
        {
            // 配置自托管的 Web API。 
            HttpConfiguration config = new HttpConfiguration();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            appBuilder.UseWebApi(config);
        }
    }
}
