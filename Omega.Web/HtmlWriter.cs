// © 2020 Samu Lang. See LICENSE for MIT License details.

namespace Omega.Web
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Xml;
    using VDS.RDF;
    using VDS.RDF.Parsing;
    using VDS.RDF.Writing;

    public class HtmlWriter : IRdfWriter
    {
#pragma warning disable 67
        public event RdfWriterWarning? Warning;
#pragma warning restore 67

        public void Save(IGraph g, string filename) =>
            throw new NotImplementedException();

        public void Save(IGraph g, TextWriter output) =>
            this.Save(g, output, false);

        public void Save(IGraph g, TextWriter output, bool leaveOpen)
        {
            if (g is null)
            {
                throw new ArgumentNullException(nameof(g));
            }

            using var writer = XmlWriter.Create(output, new XmlWriterSettings { Indent = true, CloseOutput = !leaveOpen, OmitXmlDeclaration = true });
            var n = g.Nodes.Union(g.Triples.PredicateNodes).UriNodes();
            var n2 = string.Join(",", n.Select(nn => nn.Uri.AbsoluteUri));

            writer.WriteStartElement("html");
            writer.WriteStartElement("head");
            writer.WriteRaw(@"
<style>
    body {
        margin: 0;
        font-family: sans-serif;
    }

    table {
        width: 100%;
        border-collapse: collapse;
    }

    thead th {
        height: 50px;
        position: sticky;
        top: 0;
        z-index: 1;
        background-color: black;
        color: white;
        text-align: left;
        padding-left: 20px;
    }

    tr.divider {
        border-top: 1px solid;
    }

    tr.divider:nth-child(1) {
        border-top: none;
    }

    td {
        padding: 20px;
        vertical-align: top;
    }

    div {
        position: sticky;
        left: 20px;
        top: 70px;
    }

    data {
        font-family: monospace;
    }

    data.map {
        height: 180px;
        display: block
    }

    a {
        text-decoration: none;
    }

    a:hover {
        text-decoration: underline;
    }
</style>
");
            writer.WriteEndElement(); // head
            writer.WriteStartElement("body");
            writer.WriteStartElement("table");
            HtmlWriter.WriteTHead(writer);
            HtmlWriter.WriteTBody(g, writer);
            writer.WriteEndElement(); // table
            writer.WriteEndElement(); // body
            writer.WriteEndElement(); // html
        }

        private static void WriteTBody(IGraph g, XmlWriter writer)
        {
            writer.WriteStartElement("tbody");

            var subjects = g.Triples.SubjectNodes;
            foreach (var subject in subjects)
            {
                var writeSubject = true;

                var subjectTriples = g.GetTriplesWithSubject(subject);
                var predicates = subjectTriples.Select(t => t.Predicate).Distinct();
                foreach (var predicate in predicates)
                {
                    var writePredicate = true;

                    var predicateTriples = subjectTriples.WithPredicate(predicate);
                    foreach (var t in subjectTriples.WithPredicate(predicate))
                    {
                        writer.WriteStartElement("tr");

                        if (writeSubject || writePredicate)
                        {
                            writer.WriteAttributeString("class", "divider");
                        }

                        if (writeSubject)
                        {
                            writeSubject = false;

                            HtmlWriter.WriteCell(writer, t, TripleSegment.Subject, subjectTriples.Count());
                        }

                        if (writePredicate)
                        {
                            writePredicate = false;

                            HtmlWriter.WriteCell(writer, t, TripleSegment.Predicate, predicateTriples.Count());
                        }

                        HtmlWriter.WriteCell(writer, t, TripleSegment.Object, 0);

                        writer.WriteEndElement(); // tr
                    }
                }
            }

            writer.WriteEndElement(); // tbody
        }

        private static void WriteCell(XmlWriter writer, Triple t, TripleSegment segment, int tripleCount)
        {
            writer.WriteStartElement("td");

            if (tripleCount > 1)
            {
                writer.WriteAttributeString("rowspan", tripleCount.ToString(CultureInfo.InvariantCulture));
            }

            HtmlWriter.WriteNode(writer, t, segment);
            writer.WriteEndElement(); // td
        }

        private static void WriteTHead(XmlWriter writer)
        {
            writer.WriteStartElement("thead");
            writer.WriteStartElement("tr");
            writer.WriteStartElement("th");
            writer.WriteString("Subject");
            writer.WriteEndElement(); // th
            writer.WriteStartElement("th");
            writer.WriteString("Predicate");
            writer.WriteEndElement(); // th
            writer.WriteStartElement("th");
            writer.WriteString("Object");
            writer.WriteEndElement(); // th
            writer.WriteEndElement(); // tr
            writer.WriteEndElement(); // thead
        }

        private static void WriteNode(XmlWriter writer, Triple triple, TripleSegment segment)
        {
            var node = HtmlWriter.GetNode(triple, segment);

            if (node is IUriNode uriNode)
            {
                var uri = uriNode.Uri.AbsoluteUri;

                var label = uri
                    .Replace("http://omega.langsamu.net/id/", string.Empty, StringComparison.Ordinal)
                    .Replace("http://www.w3.org/1999/02/22-rdf-syntax-ns#", "rdf:", StringComparison.Ordinal)
                    .Replace("http://www.w3.org/2000/01/rdf-schema#", "rdfs:", StringComparison.Ordinal)
                    .Replace("http://www.w3.org/ns/prov#", "prov:", StringComparison.Ordinal)
                    .Replace("http://purl.org/dc/terms/", "dct:", StringComparison.Ordinal)
                    .Replace("http://www.w3.org/ns/odrl/2/", "odrl:", StringComparison.Ordinal)
                    .Replace("http://www.loc.gov/premis/rdf/v3/", "premis:", StringComparison.Ordinal)
                    .Replace("http://rdaregistry.info/Elements/a/", "rdaa:", StringComparison.Ordinal)
                    .Replace("http://rdaregistry.info/Elements/c/", "rdac:", StringComparison.Ordinal)
                    .Replace("http://rdaregistry.info/Elements/u/", "rdau:", StringComparison.Ordinal)
                    .Replace("http://purl.org/linked-data/version#", "ver:", StringComparison.Ordinal)
                    .Replace(RdfSpecsHelper.RdfType, "a", StringComparison.Ordinal)
                    .Replace("rdac:C10005", "corporate body (rdac:C10005)", StringComparison.Ordinal)
                    .Replace("rdaa:P50292", "givenName (rdac:P50292)", StringComparison.Ordinal)
                    .Replace("rdaa:P50291", "surname (rdac:P50291)", StringComparison.Ordinal)
                    .Replace("rdaa:P50104", "professionOrOccupation (rdac:P50104)", StringComparison.Ordinal)
                    .Replace("rdaa:P50096", "employer (rdac:P50096)", StringComparison.Ordinal)
                    .Replace("rdaa:P50240", "broaderAffiliatedBody (rdac:P50240)", StringComparison.Ordinal)
                    .Replace("rdaa:P50032", "nameOfCorporateBody (rdac:P50032)", StringComparison.Ordinal)
                    .Replace("rdac:C10004", "person (rdac:C10004)", StringComparison.Ordinal);

                writer.WriteStartElement("div");
                writer.WriteStartElement("a");
                writer.WriteAttributeString("href", "resource?uri=" + WebUtility.UrlEncode(uri));

                writer.WriteStartElement("data");
                writer.WriteAttributeString("value", uri);
                writer.WriteString(label);
                writer.WriteEndElement(); // data

                writer.WriteEndElement(); // a

                writer.WriteStartElement("a");
                writer.WriteAttributeString("href", "/view#query/resource.dot?uri=" + WebUtility.UrlEncode(uri));
                writer.WriteString("👁");
                writer.WriteEndElement(); // a

                writer.WriteEndElement(); // div

                return;
            }

            if (node is IBlankNode blankNode)
            {
                if (segment == TripleSegment.Subject)
                {
                    writer.WriteStartElement("div");
                    writer.WriteStartElement("a");
                    writer.WriteAttributeString("name", blankNode.InternalID);
                    writer.WriteFullEndElement(); // a

                    writer.WriteString(blankNode.InternalID);
                    writer.WriteEndElement(); // div

                    return;
                }

                if (segment == TripleSegment.Object)
                {
                    writer.WriteStartElement("a");
                    writer.WriteAttributeString("href", "#" + blankNode.InternalID);
                    writer.WriteString(blankNode.InternalID);
                    writer.WriteEndElement(); // a

                    return;
                }
            }

            if (node is ILiteralNode literalNode)
            {
                var datatype = literalNode.DataType?.AbsoluteUri;

                if (datatype == XmlSpecsHelper.XmlSchemaDataTypeDate)
                {
                    if (DateTimeOffset.TryParse(literalNode.Value, out DateTimeOffset dto))
                    {
                        writer.WriteStartElement("time");
                        writer.WriteString(dto.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                        writer.WriteEndElement(); // time

                        return;
                    }
                }

                writer.WriteString(literalNode.Value);

                return;
            }

            writer.WriteString(node.ToString());
        }

        private static INode GetNode(Triple triple, TripleSegment segment)
        {
            switch (segment)
            {
                case TripleSegment.Subject:
                    return triple.Subject;

                case TripleSegment.Predicate:
                    return triple.Predicate;

                case TripleSegment.Object:
                    return triple.Object;

                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
