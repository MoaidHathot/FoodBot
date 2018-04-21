using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Internals.Fibers;

namespace FoodBot
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            RegisterModules();
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }

        private void RegisterModules()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new ReflectionSurrogateModule());

            builder.RegisterModule<GlobalMessageHandlersBotModule>();

            builder.Update(Conversation.Container);
        }
    }
}
