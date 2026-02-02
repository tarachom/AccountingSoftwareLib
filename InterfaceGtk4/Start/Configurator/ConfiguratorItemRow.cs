
using GObject;
using GObject.Internal;

namespace InterfaceGtk4;

/// <summary>
/// 
/// </summary>
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
    /// Ключ
    /// </summary>
    public string Key { get; set; } = "";

    /// <summary>
    /// Ключ 2
    /// </summary>
    public string Key2 { get; set; } = "";

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