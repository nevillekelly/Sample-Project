using System;
using System.Collections.Generic;
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
        }

        public IEnumerable<string> GetList()
        {
            var connStr = ConnectionSettings.ConnectionString;

            return new List<string>() { connStr, "Test1", "Test2", "Test3" };
        }
    }

    public interface IRepository
    {
        IEnumerable<string> GetList();
    }
}
