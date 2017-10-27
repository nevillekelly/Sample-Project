using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Serialization;
using Owin;
using System;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Configuration;
using Microsoft.Owin.Security.DataHandler.Encoder;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using SimpleInjector.Integration.WebApi;
using TestApp.jwt;

namespace TestApp
{
    public class Startup

    {
        public void Configuration(IAppBuilder app)
        {

            ConfigureOAuthTokenGeneration(app);


            ConfigureOAuthTokenConsumption(app);

            ConfigureIoc(app, GlobalConfiguration.Configuration);

            GlobalConfiguration.Configure(WebApiConfig.Register);

            app.UseWebApi(GlobalConfiguration.Configuration);

            GlobalConfiguration.Configuration.EnsureInitialized();
        }


        private void ConfigureOAuthTokenGeneration(IAppBuilder app)
        {
            //// Configure the db context and user manager to use a single instance per request

            OAuthAuthorizationServerOptions OAuthServerOptions = new OAuthAuthorizationServerOptions()
            {
                //For Dev enviroment only (on production should be AllowInsecureHttp = false)
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromHours(12),
                Provider = new CustomOAuthProvider(),
                AccessTokenFormat = new CustomJwtFormat(ConfigurationManager.AppSettings["as:Issuer"])
            };

            // OAuth 2.0 Bearer Access Token Generation
            app.UseOAuthAuthorizationServer(OAuthServerOptions);
        }



        private void ConfigureWebApi(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            var jsonFormatter = config.Formatters.OfType<JsonMediaTypeFormatter>().First();
            jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }

        private void ConfigureOAuthTokenConsumption(IAppBuilder app)
        {

            var issuer = ConfigurationManager.AppSettings["as:Issuer"];
            string audienceId = ConfigurationManager.AppSettings["as:AudienceId"];
            byte[] audienceSecret = TextEncodings.Base64Url.Decode(ConfigurationManager.AppSettings["as:AudienceSecret"]);

            // Api controllers with an [Authorize] attribute will be validated with JWT
            app.UseJwtBearerAuthentication(
                new JwtBearerAuthenticationOptions
                {
                    AuthenticationMode = AuthenticationMode.Active,
                    AllowedAudiences = new[] { audienceId },
                    IssuerSecurityTokenProviders = new IIssuerSecurityTokenProvider[]
                    {
                        new SymmetricKeyIssuerSecurityTokenProvider(issuer, audienceSecret)
                    }
                });
        }

        // Note: SimpleInjector was chosen for its speed. The project also contains an older version of
        // Microsoft.Extensions.DependencyInjection which is required by Microsoft.AspNet.OData
        private void ConfigureIoc(IAppBuilder app, HttpConfiguration config)
        {
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            container.Options.DefaultLifestyle = Lifestyle.Scoped;

            container.RegisterSingleton<IRequestMessageAccessor>(new RequestMessageAccessor(container));
            container.Register<IConnectionStringGenerator, ConnectionStringGenerator>();
            container.Register<Data.IDataAccessConfigurationFactory, Data.DataAccessConfigurationFactory>();

            container.Register(() => container.GetInstance<Data.IDataAccessConfigurationFactory>().GetConfiguration(container.GetInstance<IConnectionStringGenerator>().GetConnectionString()));
            container.Register<Data.IRepository, Data.SampleRepository>();

            container.EnableHttpRequestMessageTracking(config);
            container.RegisterWebApiControllers(config);

            container.Verify();

            //app.Use(async (context, next) => { using (AsyncScopedLifestyle.BeginScope(container)) { await next(); } });
            config.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);
        }
    }
}