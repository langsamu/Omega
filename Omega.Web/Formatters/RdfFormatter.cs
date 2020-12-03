// © 2020 Samu Lang. See LICENSE for MIT License details.

namespace Omega.Web
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.AspNetCore.Mvc.Formatters;
    using VDS.RDF;
    using VDS.RDF.Query;

    internal class RdfFormatter : TextOutputFormatter
    {
        private readonly (string Extension, string Mime, Func<IRdfWriter> RdfWriter, Func<ISparqlResultsWriter>? SparqlWriter) definition;

        public RdfFormatter((string Extension, string Mime, Func<IRdfWriter> RdfWriter, Func<ISparqlResultsWriter>? SparqlWriter) definition)
        {
            this.SupportedMediaTypes.Add(definition.Mime);
            this.SupportedEncodings.Add(Encoding.UTF8);

            this.definition = definition;
        }

        protected override bool CanWriteType(Type type) =>
            typeof(IGraph).IsAssignableFrom(type) ||
            (this.definition.SparqlWriter is object && typeof(SparqlResultSet).IsAssignableFrom(type));

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            // Because RDF writer writes synchronously
            AllowSynchronousIo(context);

            using var textWriter = context.WriterFactory(context.HttpContext.Response.Body, selectedEncoding);

            switch (context.Object)
            {
                case IGraph graph:
                    this.definition.RdfWriter().Save(graph, textWriter);
                    break;

                case SparqlResultSet results:
                    // SparqlWriter definitely there if we got past CanWriteType
                    this.definition.SparqlWriter()!.Save(results, textWriter);
                    break;
            }

            return Task.CompletedTask;
        }

        /// <remarks>See https://github.com/dotnet/aspnetcore/issues/7644.</remarks>
        private static void AllowSynchronousIo(OutputFormatterWriteContext context)
        {
            var syncIOFeature = context.HttpContext.Features.Get<IHttpBodyControlFeature>();
            if (syncIOFeature is object)
            {
                syncIOFeature.AllowSynchronousIO = true;
            }
        }
    }
}
