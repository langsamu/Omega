// © 2020 Samu Lang. See LICENSE for MIT License details.

namespace Omega.Web
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Xml;
    using VDS.RDF;
    using VDS.RDF.Nodes;
    using VDS.RDF.Parsing;
    using VDS.RDF.Query;
    using VDS.RDF.Writing;

    public class HtmlWriter : IRdfWriter
    {
        private static readonly Uri InstanceUri = new Uri("http://omega.langsamu.net/id/");

        public event RdfWriterWarning Warning;

        public void Save(IGraph g, string filename) => throw new NotImplementedException();

        public void Save(IGraph g, TextWriter output) => this.Save(g, output, false);

        public void Save(IGraph g, TextWriter output, bool leaveOpen)
        {
            using (var writer = XmlWriter.Create(output, new XmlWriterSettings { Indent = true, CloseOutput = !leaveOpen, OmitXmlDeclaration = true }))
            {
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
                writer.WriteAttributeString("rowspan", tripleCount.ToString());
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
                    .Replace("http://omega.langsamu.net/id/", string.Empty)
                    .Replace("http://www.w3.org/1999/02/22-rdf-syntax-ns#", "rdf:")
                    .Replace("http://www.w3.org/2000/01/rdf-schema#", "rdfs:")
                    .Replace("http://www.w3.org/ns/prov#", "prov:")
                    .Replace("http://purl.org/dc/terms/", "dct:")
                    .Replace("http://www.w3.org/ns/odrl/2/", "odrl:")
                    .Replace("http://www.loc.gov/premis/rdf/v3/", "premis:")
                    .Replace("http://rdaregistry.info/Elements/a/", "rdaa:")
                    .Replace("http://rdaregistry.info/Elements/c/", "rdac:")
                    .Replace("http://rdaregistry.info/Elements/u/", "rdau:")
                    .Replace("http://purl.org/linked-data/version#", "ver:")
                    .Replace(RdfSpecsHelper.RdfType, "a")
                    .Replace("rdac:C10005", "corporate body (rdac:C10005)")
                    .Replace("rdaa:P50292", "givenName (rdac:P50292)")
                    .Replace("rdaa:P50291", "surname (rdac:P50291)")
                    .Replace("rdaa:P50104", "professionOrOccupation (rdac:P50104)")
                    .Replace("rdaa:P50096", "employer (rdac:P50096)")
                    .Replace("rdaa:P50240", "broaderAffiliatedBody (rdac:P50240)")
                    .Replace("rdaa:P50032", "nameOfCorporateBody (rdac:P50032)")
                    .Replace("rdac:C10004", "person (rdac:C10004)");

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
                        writer.WriteString(dto.ToString("yyyy-MM-dd"));
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