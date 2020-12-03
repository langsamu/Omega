// © 2020 Samu Lang. See LICENSE for MIT License details.

namespace Omega.Web
{
    using System;
    using System.IO;
    using System.Reflection;

    internal static class Resources
    {
        internal static Stream ServiceDescription =>
            Stream($"{nameof(ServiceDescription)}.rdf")
            ?? throw new InvalidOperationException("SPARQL Service Description resource file not found.");

        internal static string? Sparql(string name)
        {
            var stream = Stream(ResolveSparqlName(name));
            if (stream is null)
            {
                return null;
            }

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        private static Assembly Assembly =>
            typeof(Resources).Assembly;

        private static Stream? Stream(string name) =>
            Assembly.GetManifestResourceStream(
                ResolveResourceName(name));

        private static string ResolveSparqlName(string name) =>
            $"Sparql.{name}.sparql";

        private static string ResolveResourceName(string name) =>
            $"{typeof(Resources).Namespace}.Resources.{name}";
    }
}
