// © 2020 Samu Lang. See LICENSE for MIT License details.

namespace Omega.Web
{
    using System;
    using Microsoft.AspNetCore.Mvc;

    [Route("view")]
    public class ViewController : Controller
    {
        private readonly Uri sparqlEndpoint;

        public IActionResult Index() =>
            this.View();
    }
}
