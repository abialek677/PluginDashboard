using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Composition;
using Contracts;

namespace DashboardApp
{
    [Export(typeof(IEventAggregator))]
    [Shared]
    public class SimpleEventAggregator : IEventAggregator
    {
        private readonly ConcurrentDictionary<Type, List<Delegate>> _subs = new();

        public void Publish<TEvent>(TEvent ev)
        {
            if (_subs.TryGetValue(typeof(TEvent), out var list))
            {
                var copy = list.ToArray();
                foreach (var d in copy)
                {
                    if (d is Action<TEvent> action)
                    {
                        try { action(ev); }
                        catch { /* swallow/log if needed */ }
                    }
                }
            }
        }

        public void Subscribe<TEvent>(Action<TEvent> handler)
        {
            var list = _subs.GetOrAdd(typeof(TEvent), _ => new List<Delegate>());
            lock (list)
            {
                list.Add(handler);
            }
        }
    }
}