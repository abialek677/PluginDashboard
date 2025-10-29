using System;
using System.Composition.Hosting;
using System.Reflection;
using System.Runtime.Loader;
using Contracts;

namespace DashboardApp
{
    public class PluginContext : IDisposable
    {
        public string Path { get; }
        public IWidget? Widget { get; private set; }
        private AssemblyLoadContext? _loadContext;
        private CompositionHost? _container;
        private readonly IEventAggregator _eventAggregator;

        public PluginContext(string path, IEventAggregator aggregator)
        {
            Path = path;
            _eventAggregator = aggregator;
        }

        public void Load()
        {
            _loadContext = new AssemblyLoadContext(Path, isCollectible: true);
            var asm = _loadContext.LoadFromAssemblyPath(System.IO.Path.GetFullPath(Path));

            var config = new ContainerConfiguration()
                .WithAssembly(asm)
                .WithExport<IEventAggregator>(_eventAggregator);

            _container = config.CreateContainer();
            Widget = _container.GetExport<IWidget>();
        }

        public void Dispose()
        {
            Widget = null;
            _container?.Dispose();
            _container = null;

            _loadContext?.Unload();
            _loadContext = null;
        }
    }
}