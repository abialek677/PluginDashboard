using System.Windows.Controls;

namespace ChartsWidget
{
    public partial class ChartsWidgetView : UserControl
    {
        public ChartsWidgetView() => InitializeComponent();
        
        public StackPanel BarsPanelPublic => BarsPanel;
        public TextBlock StatusTextPublic => StatusText;
    }
}