using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Threading;
using Contracts;

namespace DashboardApp
{
    public class PluginManager : IDisposable
    {
        private readonly Dictionary<string, PluginContext> _plugins = new();
        private FileSystemWatcher? _watcher;
        public event Action<IEnumerable<IWidget>>? WidgetsChanged;
        private readonly string _pluginsFolder;
        private readonly IEventAggregator _eventAggregator;
        
        private Dispatcher? _dispatcher;

        public PluginManager()
        {
            _pluginsFolder = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\Plugins\"));
            _eventAggregator = new SimpleEventAggregator();
        }

        public void Initialize(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            Directory.CreateDirectory(_pluginsFolder);
            LoadExistingPlugins();
            SetupWatcher();
        }

        private void LoadExistingPlugins()
        {
            foreach (var file in Directory.GetFiles(_pluginsFolder, "*.dll"))
                LoadPlugin(file);

            NotifyWidgetsChanged();
        }

        private void SetupWatcher()
        {
            _watcher = new FileSystemWatcher(_pluginsFolder, "*.dll")
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = false
            };

            _watcher.Created += (s, e) =>
            {
                System.Threading.Thread.Sleep(500);

                _dispatcher?.Invoke(() =>
                {
                    LoadPlugin(e.FullPath);
                    NotifyWidgetsChanged();
                });
            };

            _watcher.Deleted += (s, e) =>
            {
                UnloadPlugin(e.FullPath);
                NotifyWidgetsChanged();
            };

            _watcher.Renamed += (s, e) =>
            {
                UnloadPlugin(e.OldFullPath);
                LoadPlugin(e.FullPath);
                NotifyWidgetsChanged();
            };
        }

        private void LoadPlugin(string path)
        {
            try
            {
                if (_plugins.ContainsKey(path))
                    return;

                Console.WriteLine($"[PluginManager] Loading {path}");
                var ctx = new PluginContext(path, _eventAggregator);
                ctx.Load();

                if (ctx.Widget != null)
                {
                    _plugins[path] = ctx;
                    Console.WriteLine($"[PluginManager] Loaded: {ctx.Widget.Name}");
                }
                else
                {
                    ctx.Dispose();
                    Console.WriteLine($"[PluginManager] Failed: no widget exported");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PluginManager] Error loading {path}: {ex.Message}");
            }
        }


        private void UnloadPlugin(string path)
        {
            if (_plugins.TryGetValue(path, out var ctx))
            {
                ctx.Dispose();
                _plugins.Remove(path);
            }
        }

        public IEnumerable<IWidget> GetWidgets() =>
            _plugins.Values.Select(p => p.Widget).Where(w => w != null)!;

        private void NotifyWidgetsChanged() =>
            WidgetsChanged?.Invoke(GetWidgets());

        public void Publish<T>(T ev) =>
            _eventAggregator.Publish(ev);

        public void Dispose()
        {
            _watcher?.Dispose();
            foreach (var ctx in _plugins.Values)
                ctx.Dispose();
            _plugins.Clear();
        }
    }
}
