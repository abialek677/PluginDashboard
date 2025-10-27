using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using Contracts;

namespace DashboardApp
{
    public class PluginManager : IDisposable
    {
        private readonly string PluginsFolder;
        private CompositionHost? _container;
        private readonly List<Assembly> _pluginAssemblies = new();
        public event Action<IEnumerable<IWidget>>? WidgetsChanged;
        private FileSystemWatcher? _watcher;

        public void Initialize()
        {
            var pluginsFolder = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\Plugins\"));
            Directory.CreateDirectory(pluginsFolder);
            LoadPlugins(pluginsFolder);
            SetupWatcher(pluginsFolder);
        }

        public IEnumerable<IWidget> GetWidgets()
        {
            if (_container == null) return Array.Empty<IWidget>();
            try { return _container.GetExports<IWidget>(); }
            catch { return Array.Empty<IWidget>(); }
        }

        public void Publish<T>(T ev)
        {
            if (_container == null) return;
            var aggregator = _container.GetExport<IEventAggregator>();
            aggregator?.Publish(ev);
        }

        private void LoadPlugins(string folder)
        {
            _pluginAssemblies.Clear();
            var dlls = Directory.GetFiles(folder, "*.dll");
            foreach (var dll in dlls)
            {
                try
                {
                    var asm = Assembly.LoadFrom(dll);
                    _pluginAssemblies.Add(asm);
                }
                catch { }
            }

            var assemblies = new List<Assembly> { Assembly.GetExecutingAssembly() };
            assemblies.AddRange(_pluginAssemblies.Where(a => !assemblies.Contains(a)));
            var config = new ContainerConfiguration().WithAssemblies(assemblies);

            _container?.Dispose();
            _container = config.CreateContainer();
            WidgetsChanged?.Invoke(GetWidgets());
        }

        private void SetupWatcher(string folder)
        {
            _watcher = new FileSystemWatcher(folder, "*.dll")
            {
                EnableRaisingEvents = true
            };
            _watcher.Created += (s, e) => LoadPlugins(folder);
            _watcher.Deleted += (s, e) => LoadPlugins(folder);
            _watcher.Changed += (s, e) => LoadPlugins(folder);
            _watcher.Renamed += (s, e) => LoadPlugins(folder);
        }

        public void Dispose()
        {
            _container?.Dispose();
            _watcher?.Dispose();
        }
    }
}
