using System;
using System.Diagnostics;

namespace PlusUltra.WebApi.Hosting
{
    public sealed class DiagnosticSourceAdapterOptions
    {
        internal static readonly DiagnosticSourceAdapterOptions Default = new DiagnosticSourceAdapterOptions();

        /// <summary>
        /// By default we subscribe to all listeners but this allows you to filter by listener.
        /// </summary>
        public Func<DiagnosticListener, bool> ListenerFilterPredicate = _ => true;
    }
}