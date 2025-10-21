using System;
using System.Collections.Generic;
using System.Composition;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Contracts;

namespace ChartsWidget
{
    [Export(typeof(IWidget))]
    public class ChartsWidgetViewModel : IWidget
    {
        public string Name => "Charts";
        public object View { get; } = new ChartsWidgetView();

        private readonly StackPanel? _barsPanel;
        private readonly TextBlock? _statusText;

        [ImportingConstructor]
        public ChartsWidgetViewModel(IEventAggregator aggregator)
        {
            if (View is ChartsWidgetView view)
            {
                _barsPanel = view.BarsPanelPublic;
                _statusText = view.StatusTextPublic;
            }

            try
            {
                aggregator.Subscribe<DataSubmittedEvent>(OnData);
            }
            catch (Exception ex)
            {
                SafeSetStatus($"ChartsWidget: błąd subskrypcji agregatora: {ex.Message}");
                Console.WriteLine("ChartsWidget: subskrypcja - wyjątek: " + ex);
            }
        }

        private void SafeSetStatus(string text)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (_statusText != null) _statusText.Text = "Status: " + text;
                });
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void OnData(DataSubmittedEvent ev)
        {
            var s = ev?.Data ?? string.Empty;
            Console.WriteLine("ChartsWidget: otrzymano dane: \"" + s + "\"");

            // parsuj tokeny (spacja, przecinek, średnik) i różne formaty liczby
            var tokens = s.Split(new[] { ' ', ',', ';', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var numbers = new List<double>();
            foreach (var t in tokens)
            {
                var token = t.Trim();
                if (token == "") continue;

                // spróbuj standardowej kropki/culture invariant, potem culture current, potem replace ','->'.'
                if (double.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ||
                    double.TryParse(token, NumberStyles.Any, CultureInfo.CurrentCulture, out v))
                {
                    numbers.Add(v);
                    continue;
                }

                var alt = token.Replace(',', '.');
                if (double.TryParse(alt, NumberStyles.Any, CultureInfo.InvariantCulture, out v))
                {
                    numbers.Add(v);
                }
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_barsPanel == null || _statusText == null)
                {
                    Console.WriteLine("ChartsWidget: UI elementy są null (barsPanel/statusText).");
                    // jeśli nie mamy panelu, spróbuj ustawić status w konsoli
                    try { MessageBox.Show("ChartsWidget: UI nieprawidłowo zainicjowany (barsPanel == null)."); } catch { }
                    return;
                }

                _barsPanel.Children.Clear();

                if (numbers.Count == 0)
                {
                    _statusText.Text = $"Status: otrzymano \"{s}\", parsed 0 numbers";
                    Console.WriteLine("ChartsWidget: parsed 0 numbers for input: \"" + s + "\"");
                    var tb = new TextBlock { Text = "Brak parsowalnych liczb. Wpisz np.: 10 20 30 40", Margin = new Thickness(8) };
                    _barsPanel.Children.Add(tb);
                    return;
                }

                _statusText.Text = $"Status: otrzymano \"{s}\", parsed {numbers.Count} numbers";

                var max = numbers.Max();
                var scale = max > 0 ? 150.0 / max : 1.0;
                
                foreach (var n in numbers)
                {
                    var height = Math.Max(4, n * scale);

                    var rect = new Rectangle
                    {
                        Width = 40,
                        Height = height,
                        Margin = new Thickness(6, 0, 6, 0),
                        VerticalAlignment = VerticalAlignment.Bottom
                    };

                    rect.ToolTip = n.ToString(CultureInfo.CurrentCulture);
                    
                    var stack = new StackPanel 
                    { 
                        Orientation = Orientation.Vertical,
                        VerticalAlignment = VerticalAlignment.Bottom,
                        Background = Brushes.Brown,
                        Margin = new Thickness(10)
                    };

                    var valText = new TextBlock { Text = n.ToString(CultureInfo.CurrentCulture), HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0,0,0,4) };
                    stack.Children.Add(valText);
                    stack.Children.Add(rect);

                    _barsPanel.Children.Add(stack);
                }
            });
        }
    }
}
