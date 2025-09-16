using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
// using Swashbuckle.Application; // Temporarily commented out until Swashbuckle package is restored

namespace UtilityHub360
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // Configure Swagger (temporarily commented out until Swashbuckle package is restored)
            /*
            config.EnableSwagger(c =>
            {
                c.SingleApiVersion("v1", "UtilityHub360 API")
                    .Description("A utility hub API for various tools and services")
                    .Contact(cc => cc
                        .Name("UtilityHub360 Team")
                        .Email("support@utilityhub360.com"));
            })
            .EnableSwaggerUi(c =>
            {
                c.DocumentTitle("UtilityHub360 API Documentation");
                c.DocExpansion(DocExpansion.List);
            });
            */
        }
    }
}
