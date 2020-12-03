// © 2020 Samu Lang. See LICENSE for MIT License details.

namespace Omega.Web
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Options;
    using VDS.RDF;
    using VDS.RDF.Query;
    using VDS.RDF.Storage;

    public sealed class DefaultSparqlQueryable : ISparqlQueryable, IDisposable
    {
        private static readonly NodeFactory Factory = new NodeFactory();
        private readonly IQueryableStorage storage;

        public DefaultSparqlQueryable(IOptions<OmegaOptions> options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var endpoint = options.Value.SparqlEndpoint;

            if (endpoint is null)
            {
                throw new InvalidOperationException("Missing configuration value Sparql Endpoint.");
            }

            this.storage = new SparqlConnector(endpoint);
        }

        public void Dispose() =>
            this.storage.Dispose();

        object ISparqlQueryable.Query(string sparql, IEnumerable<KeyValuePair<string, string>>? parameters)
        {
            var query = new SparqlParameterizedString(sparql);

            if (parameters is object)
            {
                foreach (var parameter in parameters)
                {
                    var paramNode = Convert(parameter.Value);
                    query.SetVariable(parameter.Key, paramNode);
                }
            }

            return this.storage.Query(query.ToString());
        }

        private static INode Convert(string parameter) =>
            Factory.CreateUriNode(new Uri(parameter));
    }
}
