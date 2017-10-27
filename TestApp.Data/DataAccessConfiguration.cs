using System;
using System.Collections.Generic;
using System.Text;

namespace TestApp.Data
{
    public class DataAccessConfiguration : IDataAccessConfiguration
    {
        public string ConnectionString { get; set; }
    }

    public class DataAccessConfigurationFactory : IDataAccessConfigurationFactory
    {
        public IDataAccessConfiguration GetConfiguration(string connectionString)
        {
            return new DataAccessConfiguration { ConnectionString = connectionString };
        }
    }

    public interface IDataAccessConfiguration
    {
        string ConnectionString { get; set; }
    }
    public interface IDataAccessConfigurationFactory
    {
        IDataAccessConfiguration GetConfiguration(string connectionString);
    }
}
