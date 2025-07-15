using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace NPMAPI.Helpers
{
    public class DynamicConnections: DelegatingHandler
    {

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var a = DateTime.Now;
            string v = request.RequestUri.Host.ToString();
            if (v == "http://localhost:9639/")
            {
                ConfigurationManager.ConnectionStrings["NPMDBEntities"].ConnectionString.ToString();
            }
            else
            {
                ConfigurationManager.ConnectionStrings["EraDbModel"].ConnectionString.ToString();
            }
                var response = await base.SendAsync(request, cancellationToken);
            var b= DateTime.Now;
                return response;
        }

    }

}