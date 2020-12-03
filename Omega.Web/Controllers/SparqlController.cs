// © 2020 Samu Lang. See LICENSE for MIT License details.

namespace Omega.Web
{
    using System;
    using System.Threading.Tasks;
    using AspNetCore.Proxy;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;

    [Route("sparql")]
    public class SparqlController : Controller
    {
        private readonly Uri sparqlEndpoint;

        public SparqlController(IOptions<OmegaOptions> options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (options.Value.SparqlEndpoint is null)
            {
                throw new InvalidOperationException("Missing configuration value Sparql Endpoint.");
            }

            this.sparqlEndpoint = options.Value.SparqlEndpoint;
        }

        [HttpGet("ui")]
        public IActionResult Index() =>
            this.View();

        [HttpGet]
        public object ProxyGet(string query)
        {
            if (query is null)
            {
                return this.File(Resources.ServiceDescription, "application/rdf+xml","OmegaSparqlServiceDescription.rdf");
            }
            else
            {
                return this.Proxy();
            }
        }

        [HttpPost]
        public Task Proxy()
        {
            return this.HttpProxyAsync(
                new UriBuilder(this.sparqlEndpoint)
                {
                    Query = this.Request.QueryString.Value,
                }
                .ToString());
        }
    }
}
