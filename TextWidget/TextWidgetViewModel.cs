using System;
using System.ComponentModel;
using System.Composition;
using Contracts;

namespace TextWidget
{
    [Export(typeof(IWidget))]
    public class TextWidgetViewModel : IWidget, INotifyPropertyChanged
    {
        public string Name => "Text widget";
        public object View { get; }

        private string _data = string.Empty;

        private int _charCount;
        public int CharacterCount
        {
            get => _charCount;
            private set { _charCount = value; OnPropertyChanged(nameof(CharacterCount)); }
        }

        private int _wordCount;
        public int WordCount
        {
            get => _wordCount;
            private set { _wordCount = value; OnPropertyChanged(nameof(WordCount)); }
        }

        private string _preview = string.Empty;
        public string Preview
        {
            get => _preview;
            private set { _preview = value; OnPropertyChanged(nameof(Preview)); }
        }

        [ImportingConstructor]
        public TextWidgetViewModel(IEventAggregator eventAggregator)
        {
            View = new TextWidgetView { DataContext = this };
            eventAggregator.Subscribe<DataSubmittedEvent>(OnDataReceived);
        }

        private void OnDataReceived(DataSubmittedEvent ev)
        {
            _data = ev.Data ?? string.Empty;
            CharacterCount = _data.Length;
            WordCount = string.IsNullOrWhiteSpace(_data)
                ? 0
                : _data.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
            Preview = _data.Length > 120 ? _data.Substring(0, 120) + "..." : _data;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}