// © 2020 Samu Lang. See LICENSE for MIT License details.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "No synchronisation context in ASP.NET")]
