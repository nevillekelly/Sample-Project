using SimpleInjector;
using System.Security.Claims;
using System.Net.Http;

namespace TestApp
{
    public class ConnectionStringGenerator : IConnectionStringGenerator
    {
        public IRequestMessageAccessor RequestMessage { get; set; }

        public ConnectionStringGenerator(IRequestMessageAccessor request)
        {
            RequestMessage = request;
        }

        public string GetConnectionString()
        {
            if (RequestMessage.CurrentMessage != null)
            {
                var requestContext = RequestMessage.CurrentMessage.GetRequestContext();
                if (requestContext != null)
                {
                    var dbServer = PrincipalInformationExtractor.GetInfo(requestContext.Principal, "dbServer");
                    var dbName = PrincipalInformationExtractor.GetInfo(requestContext.Principal, "dbName");
                    if (!string.IsNullOrEmpty(dbServer) && !string.IsNullOrEmpty(dbName))
                    {
                        return string.Format($"Server={dbServer};Database={dbName};Trusted_Connection=True");
                    }
                }
            }
            return System.Configuration.ConfigurationManager.ConnectionStrings["sqlclientdb"].ConnectionString;
        }
    }

    public interface IConnectionStringGenerator
    {
        string GetConnectionString();
    }

    public sealed class RequestMessageAccessor : IRequestMessageAccessor
    {
        private readonly Container container;

        public RequestMessageAccessor(Container container)
        {
            this.container = container;
        }

        public HttpRequestMessage CurrentMessage =>
            this.container.GetCurrentHttpRequestMessage();
    }


    public interface IRequestMessageAccessor
    {
        HttpRequestMessage CurrentMessage { get; }
    }

    public static class PrincipalInformationExtractor
    {
        public static string GetInfo(System.Security.Principal.IPrincipal objPrincipal, string dataKey)
        {
            var result = string.Empty;
            var principal = objPrincipal as ClaimsPrincipal;
            if (principal != null)
            {
                var value = principal.FindFirst(dataKey);
                if (value != null)
                {
                    result = value.Value;
                }
            }

            return result;
        }
    }

    public class DataAccessConfigurationProxy : Data.IDataAccessConfiguration
    {
        private readonly Data.IDataAccessConfigurationFactory factory;
        private readonly IConnectionStringGenerator generator;

        public DataAccessConfigurationProxy(
            Data.IDataAccessConfigurationFactory factory, IConnectionStringGenerator generator)
        {
            this.factory = factory;
            this.generator = generator;
        }

        public string ConnectionString
        {
            get { return GetRealInstance().ConnectionString; }
            set { }
        }

        private Data.IDataAccessConfiguration GetRealInstance() =>
            this.factory.GetConfiguration(this.generator.GetConnectionString());
    }
}