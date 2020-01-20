using System;
using System.Collections.Generic;
using System.Diagnostics;
using App.Metrics;
using App.Metrics.Counter;

namespace PlusUltra.WebApi.Hosting
{
    public static class MetricsRegistry
    {
        public static CounterOptions DiagnosticSource => new CounterOptions
        {
            Name = "diagnostic events total",
            MeasurementUnit = Unit.Calls,
        };
    }

    /// <summary>
    /// Monitors all DiagnosticSource events and exposes them as Prometheus counters.
    /// The event data is discarded, only the number of occurrences is measured.
    /// </summary>
    /// <remarks>
    /// This is a very coarse data set due to lacking any intelligence on the payload.
    /// Users are recommended to make custom adapters with more detail for specific use cases.
    /// </remarks>
    public sealed class DiagnosticSourceAdapter : IDisposable
    {
        /// <summary>
        /// Starts listening for DiagnosticSource events and reporting them as Prometheus metrics.
        /// Dispose of the return value to stop listening.
        /// </summary>
        public static IDisposable StartListening(IMetricsRoot metrics) => StartListening(metrics, DiagnosticSourceAdapterOptions.Default);


        /// <summary>
        /// Starts listening for DiagnosticSource events and reporting them as Prometheus metrics.
        /// Dispose of the return value to stop listening.
        /// </summary>
        public static IDisposable StartListening(IMetricsRoot metrics, DiagnosticSourceAdapterOptions options) => new DiagnosticSourceAdapter(metrics.Measure.Counter, options);

        private DiagnosticSourceAdapter(IMeasureCounterMetrics counter, DiagnosticSourceAdapterOptions options)
        {
            _options = options;
            _metric = counter;

            var newListenerObserver = new NewListenerObserver(OnNewListener);
            _newListenerSubscription = DiagnosticListener.AllListeners.Subscribe(newListenerObserver);
        }

        private readonly DiagnosticSourceAdapterOptions _options;
        private readonly IMeasureCounterMetrics _metric;

        private readonly IDisposable _newListenerSubscription;

        // listener name -> subscription
        private readonly Dictionary<string, IDisposable> _newEventSubscription = new Dictionary<string, IDisposable>();
        private readonly object _newEventSubscriptionLock = new object();

        private void OnNewListener(DiagnosticListener listener)
        {
            lock (_newEventSubscriptionLock)
            {
                if (_newEventSubscription.TryGetValue(listener.Name, out var oldSubscription))
                {
                    oldSubscription.Dispose();
                    _newEventSubscription.Remove(listener.Name);
                }

                if (!_options.ListenerFilterPredicate(listener))
                    return;

                var listenerName = listener.Name;
                var newEventObserver = new NewEventObserver(kvp => OnEvent(listenerName, kvp.Key, kvp.Value));
                _newEventSubscription[listenerName] = listener.Subscribe(newEventObserver);
            }
        }

        private void OnEvent(string listenerName, string eventName, object payload)
        {
            _metric.Increment(MetricsRegistry.DiagnosticSource, new MetricTags(new string[] { "source", "event" }, new string[] { listenerName, eventName }));
        }

        private sealed class NewListenerObserver : IObserver<DiagnosticListener>
        {
            private readonly Action<DiagnosticListener> _onNewListener;

            public NewListenerObserver(Action<DiagnosticListener> onNewListener)
            {
                _onNewListener = onNewListener;
            }

            public void OnCompleted()
            {
            }

            public void OnError(Exception error)
            {
            }

            public void OnNext(DiagnosticListener listener)
            {
                _onNewListener(listener);
            }
        }

        private sealed class NewEventObserver : IObserver<KeyValuePair<string, object>>
        {
            private readonly Action<KeyValuePair<string, object>> _onEvent;

            public NewEventObserver(Action<KeyValuePair<string, object>> onEvent)
            {
                _onEvent = onEvent;
            }

            public void OnCompleted()
            {
            }

            public void OnError(Exception error)
            {
            }

            public void OnNext(KeyValuePair<string, object> receivedEvent)
            {
                _onEvent(receivedEvent);
            }
        }

        public void Dispose()
        {
            _newListenerSubscription.Dispose();

            lock (_newEventSubscriptionLock)
            {
                foreach (var subscription in _newEventSubscription.Values)
                    subscription.Dispose();
            }
        }
    }
}