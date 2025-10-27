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
            _pluginManager.Initialize();
            OnWidgetsChanged(_pluginManager.GetWidgets());
        }

        private void OnWidgetsChanged(IEnumerable<IWidget> widgets)
        {
            WidgetsTabControl.Items.Clear();
            foreach (var w in widgets)
            {
                var tab = new TabItem
                {
                    Header = w.Name ?? "Widget",
                    Content = w.View as UIElement ?? new TextBlock { Text = w.View?.ToString() ?? "" }
                };
                WidgetsTabControl.Items.Add(tab);
            }
        }

        private void SendData_Click(object sender, RoutedEventArgs e)
        {
            var text = InputTextBox.Text ?? string.Empty;
            _pluginManager.Publish(new DataSubmittedEvent(text));
        }
    }
}