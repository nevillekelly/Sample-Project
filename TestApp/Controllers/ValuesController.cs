using System.Collections.Generic;
using System.Web.Http;
using TestApp.Data;

namespace TestApp.Controllers
{
    [Authorize]
    [RoutePrefix("api/Values")]
    public class ValuesController : ApiController
    {
        public IRepository Repo { get; set; }
        public ValuesController(IRepository repo)
        {
            Repo = repo;
        }
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            var dbServer = PrincipalInformationExtractor.GetInfo(ActionContext.RequestContext.Principal, "dbServer");
            var dbName = PrincipalInformationExtractor.GetInfo(ActionContext.RequestContext.Principal, "dbName");
            var list = Repo.GetList() as List<string>;
            list.AddRange(new string[] { dbServer, dbName });
            return list;
        }

        // GET api/values/5
        [HttpPost]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete]
        public void Delete(int id)
        {
        }
    }
}
