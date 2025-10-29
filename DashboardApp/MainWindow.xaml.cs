using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Contracts;

namespace DashboardApp
{
    public partial class MainWindow : Window
    {
        private readonly PluginManager _pluginManager;

        public MainWindow()
        {
            InitializeComponent();
            _pluginManager = new PluginManager();
            _pluginManager.WidgetsChanged += OnWidgetsChanged;
            _pluginManager.Initialize(Dispatcher);
            OnWidgetsChanged(_pluginManager.GetWidgets());
        }

        private void OnWidgetsChanged(IEnumerable<IWidget> widgets)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => OnWidgetsChanged(widgets));
                return;
            }

            var newWidgets = widgets.ToDictionary(w => w.Name);
            var existingTabs = WidgetsTabControl.Items.Cast<TabItem>().ToList();

            // remove tabs of non-existing widgets
            foreach (var tab in existingTabs)
            {
                if (tab.Header is string name && !newWidgets.ContainsKey(name))
                {
                    WidgetsTabControl.Items.Remove(tab);
                }
            }

            // add new widgets
            foreach (var w in widgets)
            {
                if (existingTabs.All(t => (string)t.Header != w.Name))
                {
                    WidgetsTabControl.Items.Add(new TabItem
                    {
                        Header = w.Name ?? "Widget",
                        Content = w.View as UIElement ?? new TextBlock { Text = w.View?.ToString() ?? "" }
                    });
                }
            }
        }


        private void SendData_Click(object sender, RoutedEventArgs e)
        {
            var text = InputTextBox.Text ?? string.Empty;
            _pluginManager.Publish(new DataSubmittedEvent(text));
        }
    }
}