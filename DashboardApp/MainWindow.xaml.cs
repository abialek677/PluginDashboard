using System;
using System.Collections.Generic;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Contracts;

namespace DashboardApp
{
    public partial class MainWindow : Window
    {
        private readonly string widgetsFolder;
        private CompositionHost? container;
        private readonly List<Assembly> pluginAssemblies = new();

        public MainWindow()
        {
            InitializeComponent();

            widgetsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Widgets");
            Directory.CreateDirectory(widgetsFolder);

            // zarejestruj hostowe assembly (zawiera SimpleEventAggregator) i stwórz kontener
            Recompose();

            // FileSystemWatcher dla dynamicznego dodawania/usuwania pluginów
            var watcher = new FileSystemWatcher(widgetsFolder, "*.dll");
            watcher.Created += (s, e) => Dispatcher.Invoke(OnPluginsChanged);
            watcher.Deleted += (s, e) => Dispatcher.Invoke(OnPluginsChanged);
            watcher.Changed += (s, e) => Dispatcher.Invoke(OnPluginsChanged);
            watcher.Renamed += (s, e) => Dispatcher.Invoke(OnPluginsChanged);
            watcher.EnableRaisingEvents = true;

            // załaduj początkowe pluginy
            OnPluginsChanged();
        }

        private void Recompose(IEnumerable<Assembly>? additionalAssemblies = null)
        {
            var assemblies = new List<Assembly> { Assembly.GetExecutingAssembly() };
            if (additionalAssemblies != null) assemblies.AddRange(additionalAssemblies);
            assemblies.AddRange(pluginAssemblies.Where(a => !assemblies.Contains(a)));

            var config = new ContainerConfiguration().WithAssemblies(assemblies);
            container?.Dispose();
            container = config.CreateContainer();

            // sprawdź, że agregator jest dostępny
            try
            {
                var agg = container.GetExport<IEventAggregator>();
                // ok
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd kompozycji: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Assembly? SafeLoadAssembly(string path)
        {
            try
            {
                // LoadFrom pozwala na załadowanie ze ścieżki. MOŻesz alternatywnie użyć AssemblyLoadContext dla izolacji.
                return Assembly.LoadFrom(path);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void OnPluginsChanged()
        {
            WidgetsTabControl.Items.Clear();
            var dlls = Directory.GetFiles(widgetsFolder, "*.dll");
            Console.WriteLine("=== Widgets folder content ===");
            foreach (var d in dlls) Console.WriteLine(d);
            Console.WriteLine("=== Koniec listy ===");
            
            pluginAssemblies.Clear();
            foreach (var dll in dlls)
            {
                var asm = SafeLoadAssembly(dll);
                if (asm != null) pluginAssemblies.Add(asm);
            }

            Recompose(pluginAssemblies);

            // Pobierz instancje IWidget
            IEnumerable<IWidget> widgets;
            try
            {
                widgets = container.GetExports<IWidget>();
            }
            catch
            {
                widgets = Array.Empty<IWidget>();
            }

            foreach (var w in widgets)
            {
                var header = w.Name ?? "Widget";
                var viewObj = w.View;
                var content = viewObj as UIElement ?? new TextBlock { Text = viewObj?.ToString() ?? "" };
                var tab = new TabItem { Header = header, Content = content };
                WidgetsTabControl.Items.Add(tab);
            }
        }

        private void SendData_Click(object sender, RoutedEventArgs e)
        {
            var text = InputTextBox.Text ?? string.Empty;
            if (container == null) return;
            var agg = container.GetExport<IEventAggregator>();
            agg?.Publish(new DataSubmittedEvent(text));
        }
    }
}
