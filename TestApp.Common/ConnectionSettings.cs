using System;

namespace TestApp.Common
{
    public class ConnectionSettings : IConnectionSettings
    {
        public string ConnectionString { get; set; }
    }

    public interface IConnectionSettings
    {
        string ConnectionString { get; set; }
    }
}
