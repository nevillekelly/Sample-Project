using System;
using TestApp.Common;

namespace TestApp.Data
{
    public class SampleRepository : IRepository
    {
        private string _connectionString;
        public IDataAccessConfiguration ConnectionSettings { get; set; }
        public SampleRepository(IDataAccessConfiguration connectionSettings)
        {
            ConnectionSettings = connectionSettings;
            _connectionString = ConnectionSettings.ConnectionString;
        }
    }

    public interface IRepository
    {

    }
}
