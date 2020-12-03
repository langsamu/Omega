// © 2020 Samu Lang. See LICENSE for MIT License details.

namespace Omega.Web
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using Microsoft.AspNetCore.Mvc;
    using VDS.RDF.Query;

    [Route("id")]
    public class DereferenceController : ControllerBase
    {
        private readonly ISparqlQueryable queryable;

        public DereferenceController(ISparqlQueryable queryable)
        {
            if (queryable is null)
            {
                throw new ArgumentNullException(nameof(queryable));
            }

            this.queryable = queryable;
        }

        [HttpGet("{name}")]
        public IActionResult Index(string name)
        {
            var baseUri = "http://omega.langsamu.net";
            var fullName = $"{baseUri}/id/{name}";

            if (this.Exists(fullName))
            {
                var encodedName = WebUtility.UrlEncode(fullName);
                this.Response.Headers.Add("Location", $"{baseUri}/query/resource?uri={encodedName}");
                return this.StatusCode((int)HttpStatusCode.SeeOther);
            }
            else
            {
                return this.NotFound($"<{fullName}> is not a known resource in the Omega catalogue.\n\nExample resource:\n<http://omega.langsamu.net/id/MSW.2020.2YC.P.1>");
            }
        }

        private bool Exists(string name)
        {
            var query = Resources.Sparql("exists");
            var parameters = new Dictionary<string, string>
            {
                ["uri"] = name,
            };

            return this.Ask(query!, parameters);
        }

        private bool Ask(string query, IEnumerable<KeyValuePair<string, string>>? parameters = null)
        {
            var result = (SparqlResultSet)this.queryable.Query(query, parameters);
            return result.Result;
        }
    }
}
