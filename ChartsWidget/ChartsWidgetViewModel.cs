using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Composition;
using Contracts;
using System.Globalization;

namespace ChartsWidget
{
    [Export(typeof(IWidget))]
    public class ChartsWidgetViewModel : IWidget, INotifyPropertyChanged
    {
        public string Name => "Charts widget";
        public object View { get; }

        public ObservableCollection<BarData> Bars { get; } = new();

        [ImportingConstructor]
        public ChartsWidgetViewModel(IEventAggregator aggregator)
        {
            View = new ChartsWidgetView { DataContext = this };
            aggregator.Subscribe<DataSubmittedEvent>(OnData);
        }

        private void OnData(DataSubmittedEvent ev)
        {
            var input = ev?.Data ?? string.Empty;
            var numbers = ParseNumbers(input);

            Bars.Clear();
            var max = numbers.Count > 0 ? numbers.Max() : 0;
            foreach (var number in numbers)
            {
                var height = max > 0 ? Math.Max(1, number / max * 300) : 1;
                Bars.Add(new BarData { Value = number, Height = height });
            }
        }

        private List<double> ParseNumbers(string input)
        {
            var tokens = input.Split(new[] { ' ', ',', ';', '\t', '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            var numbers = new List<double>();
            foreach (var token in tokens)
            {
                if (double.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ||
                    double.TryParse(token, NumberStyles.Any, CultureInfo.CurrentCulture, out v))
                {
                    numbers.Add(v);
                    continue;
                }
                var alt = token.Replace(',', '.');
                if (double.TryParse(alt, NumberStyles.Any, CultureInfo.InvariantCulture, out v))
                    numbers.Add(v);
            }
            return numbers;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
