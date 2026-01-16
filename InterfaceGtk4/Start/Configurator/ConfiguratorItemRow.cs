
using GObject;

namespace InterfaceGtk4;

[Subclass<GObject.Object>]
partial class ConfiguratorItemRow
{
    /// <summary>
    /// Група
    /// </summary>
    public string Group { get; set; } = "";

    /// <summary>
    /// Назва
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Об'єкт
    /// </summary>
    public object? Obj { get; set; } = null;

    /// <summary>
    /// Тип даних
    /// </summary>
    public string Type { get; set; } = "";

    /// <summary>
    /// Пояснення
    /// </summary>
    public string Desc { get; set; } = "";
}