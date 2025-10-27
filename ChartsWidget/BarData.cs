using System.Globalization;

namespace ChartsWidget;

public class BarData
{
    public double Value { get; set; }
    public double Height { get; set; }
    public string Display => Value.ToString(CultureInfo.CurrentCulture);
}
