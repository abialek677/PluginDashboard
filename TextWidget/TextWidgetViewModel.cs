using System;
using System.ComponentModel;
using System.Composition;
using System.Windows.Controls;
using Contracts;

namespace TextWidget
{
    [Export(typeof(IWidget))]
    public class TextWidgetViewModel : IWidget, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private string _data = string.Empty;

        public string Name => "Analizator Tekstu";

        public object View { get; } = new TextWidgetView();

        private int _charCount;
        public int CharacterCount { get => _charCount; private set { _charCount = value; Notify(nameof(CharacterCount)); } }

        private int _wordCount;
        public int WordCount { get => _wordCount; private set { _wordCount = value; Notify(nameof(WordCount)); } }

        private string _preview = "";
        public string Preview { get => _preview; private set { _preview = value; Notify(nameof(Preview)); } }

        private void Notify(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        [ImportingConstructor]
        public TextWidgetViewModel(IEventAggregator eventAggregator)
        {
            if (View is UserControl uc) uc.DataContext = this;
            eventAggregator.Subscribe<DataSubmittedEvent>(OnDataReceived);
        }

        private void OnDataReceived(DataSubmittedEvent ev)
        {
            _data = ev.Data ?? string.Empty;
            CharacterCount = _data.Length;
            WordCount = string.IsNullOrWhiteSpace(_data) ? 0 :
                _data.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
            Preview = _data.Length > 120 ? _data.Substring(0, 120) + "..." : _data;
        }
    }
}