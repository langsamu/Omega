// © 2020 Samu Lang. See LICENSE for MIT License details.

namespace Omega.Web
{
    using System;
    using AspNetCore.Proxy;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using VDS.RDF;
    using VDS.RDF.Writing;

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(this.ConfigureMvc);
            services.AddOptions<OmegaOptions>().Configure<IConfiguration>(ConfigureOptions);
            services.AddSingleton<ISparqlQueryable, DefaultSparqlQueryable>();
            services.AddProxies();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }

        private static void ConfigureOptions(OmegaOptions options, IConfiguration configuration)
        {
            configuration.GetSection(nameof(Omega)).Bind(options);
        }

        private void ConfigureMvc(MvcOptions mvc)
        {
            // TODO: Add
            // - VDS.RDF.Writing.GraphMLWriter
            // - VDS.RDF.Writing.JsonLdWriter
            // - VDS.RDF.Writing.NQuadsWriter
            // - VDS.RDF.Writing.TriGWriter
            // - VDS.RDF.Writing.TriXWriter
            var definitions = new (string Extension, string Mime, Func<IRdfWriter> RdfWriter, Func<ISparqlResultsWriter>? SparqlWriter)[]
            {
                ("csv", "text/csv", () => new CsvWriter(), () => new SparqlCsvWriter()),
                ("dot", "text/vnd.graphviz", () => new GraphVizWriter(), null),
                ("n3", "text/n3", () => new Notation3Writer(), null),
                ("tsv", "text/tab-separated-values", () => new TsvWriter(), () => new SparqlTsvWriter()),
                ("nt", "text/n-triples", () => new NTriplesWriter(), null),
                ("json", "application/rdf+json", () => new RdfJsonWriter(), () => new SparqlJsonWriter()),
                ("xml", "text/xml", () => new PrettyRdfXmlWriter(), () => new SparqlXmlWriter()),
                ("ttl", "text/turtle", () => new CompressingTurtleWriter(), null),
                ("html", "text/html", () => new HtmlWriter(), () => new SparqlHtmlWriter()),
            };

            foreach (var definition in definitions)
            {
                mvc.OutputFormatters.Insert(0, new RdfFormatter(definition));
                mvc.FormatterMappings.SetMediaTypeMappingForFormat(definition.Extension, definition.Mime);
            }
        }
    }
}
