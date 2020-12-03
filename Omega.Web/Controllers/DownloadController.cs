// © 2020 Samu Lang. See LICENSE for MIT License details.

namespace Omega.Web
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using Microsoft.AspNetCore.Mvc;
    using VDS.RDF;
    using VDS.RDF.Writing;

    [Route("download")]
    public class DownloadController : Controller
    {
        private readonly ISparqlQueryable queryable;

        public DownloadController(ISparqlQueryable queryable)
        {
            if (queryable is null)
            {
                throw new ArgumentNullException(nameof(queryable));
            }

            this.queryable = queryable;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var graph = this.All();
            using var store = new TripleStore();
            store.Add(graph);

            var zip = Zip(store);

            return this.File(zip, "application/zip", "omega.zip");
        }

        private static byte[] Zip(TripleStore store)
        {
            using var stream = new MemoryStream();
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Create))
            {
                var entry = zip.CreateEntry("omega.trig");
                using var writer = new StreamWriter(entry.Open());

                var trig = new TriGWriter();
                trig.Save(store, writer);
            }

            return stream.ToArray();
        }

        private IGraph All()
        {
            var query = Resources.Sparql("all");

            return this.Construct(query!);
        }

        private IGraph Construct(string query, IEnumerable<KeyValuePair<string, string>>? parameters = null)
        {
            var result = (IGraph)this.queryable.Query(query, parameters);
            return result;
        }
    }
}
