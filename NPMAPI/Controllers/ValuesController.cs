using NPMAPI.Repositories;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Cors;

namespace NPMAPI.Controllers
{
    public class ValuesController : ApiController
    {
        private readonly IRepository _repository;
        // GET api/values
        public ValuesController(IRepository repository)
        {
            _repository = repository;
        }
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
