/*

*/

using Gtk;
using InterfaceGtk4;

namespace StorageAndTrade;

[GObject.Subclass<Box>("TwoLabelReportCell")]
public partial class TwoLabelReportCell : Box, IReportCell
{
    public static TwoLabelReportCell New() => NewWithOrientation(Orientation.Horizontal);

    public static TwoLabelReportCell NewWithOrientation(Orientation orientation)
    {
        TwoLabelReportCell w = NewWithProperties([]);
        w.SetOrientation(orientation);
        return w;
    }

    partial void Initialize()
    {
        Append(Value1);
        Append(Value2);
    }

    public LabelReportCell Value1 { get; } = LabelReportCell.New();
    public LabelReportCell Value2 { get; } = LabelReportCell.New();

    public void SetValue(params object?[]? p)
    {
        Value1.SetValue(p is { Length: > 0 } ? p[0] : null);
        Value2.SetValue(p is { Length: > 1 } ? p[1] : null);
    }
}