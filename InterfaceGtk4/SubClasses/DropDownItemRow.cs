
using GObject;

namespace InterfaceGtk4;

/// <summary>
/// 
/// </summary>
[Subclass<GObject.Object>]
public partial class DropDownItemRow
{
    public static DropDownItemRow New() => NewWithProperties([]);
    public static DropDownItemRow NewWithValue(string name, string desc)
    {
        DropDownItemRow row = NewWithProperties([]);
        row.Name = name;
        row.Desc = desc;
        return row;
    }

    /// <summary>
    /// Назва
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Відображення
    /// </summary>
    public string Desc { get; set; } = "";
}