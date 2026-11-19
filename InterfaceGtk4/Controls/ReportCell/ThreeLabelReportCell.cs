/*

*/

using Gtk;
using InterfaceGtk4;

namespace StorageAndTrade;

[GObject.Subclass<Box>("ThreeLabelReportCell")]
public partial class ThreeLabelReportCell : Box, IReportCell
{
    public static ThreeLabelReportCell New() => NewWithOrientation(Orientation.Horizontal);

    public static ThreeLabelReportCell NewWithOrientation(Orientation orientation)
    {
        ThreeLabelReportCell w = NewWithProperties([]);
        w.SetOrientation(orientation);
        return w;
    }

    partial void Initialize()
    {
        Append(Value1);
        Append(Value2);
        Append(Value3);
    }

    public LabelReportCell Value1 { get; } = LabelReportCell.New();
    public LabelReportCell Value2 { get; } = LabelReportCell.New();
    public LabelReportCell Value3 { get; } = LabelReportCell.New();

    public void SetValue(params object?[]? p)
    {
        Value1.SetValue(p is { Length: > 0 } ? p[0] : null);
        Value2.SetValue(p is { Length: > 1 } ? p[1] : null);
        Value3.SetValue(p is { Length: > 2 } ? p[2] : null);
    }
}