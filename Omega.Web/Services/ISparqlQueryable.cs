// © 2020 Samu Lang. See LICENSE for MIT License details.

namespace Omega.Web
{
    using System.Collections.Generic;

    public interface ISparqlQueryable
    {
        object Query(string sparql, IEnumerable<KeyValuePair<string, string>>? parameters = null);
    }
}
