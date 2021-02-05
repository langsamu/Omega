# TNA Omega showcase

This repository contains source-code for a showcase API application I had built for the interview for this role at The National Archives: [Developer to build alpha for managing catalogue data using RDF](https://www.digitalmarketplace.service.gov.uk/digital-outcomes-and-specialists/opportunities/13372).

~~The Application is available at omega.langsamu.net.~~

## Components
- [Dereference endpoint](Omega.Web/Controllers/DereferenceController.cs) for resolving resource URIs to their RDF representations.
- [Download endpoiont](Omega.Web/Controllers/DownloadController.cs) for retrieving the entire triplestore as a compressed TriG file.
- [Query endpoint](Omega.Web/Controllers/QueryController.cs) for running pre-written named SPARQL queries.
- [SPARQL endpoint](Omega.Web/Controllers/SparqlController.cs) for running arbitrary queries against the triplestore.
- [Viewer endpoint](Omega.Web/Controllers/ViewController.cs) for visualising graphviz files.
