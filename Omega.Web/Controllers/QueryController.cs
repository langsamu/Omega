// © 2020 Samu Lang. See LICENSE for MIT License details.

namespace Omega.Web
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;

    [Route("query")]
    public class QueryController : ControllerBase
    {
        private readonly ISparqlQueryable queryable;

        public QueryController(ISparqlQueryable queryable)
        {
            if (queryable is null)
            {
                throw new ArgumentNullException(nameof(queryable));
            }

            this.queryable = queryable;
        }

        [FormatFilter]
        [HttpGet("{name}")]
        [HttpGet("{name}.{format}")]
        public ActionResult<object> Get(string name)
        {
            var sparql = Resources.Sparql(name);

            if (sparql is null)
            {
                return this.NotFound();
            }

            var parameters = this.Request.Query.Select(parameter => KeyValuePair.Create(parameter.Key, (string)parameter.Value));

            return this.queryable.Query(sparql, parameters);
        }
    }
}
