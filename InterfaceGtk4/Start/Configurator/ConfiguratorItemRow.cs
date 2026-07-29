
using GObject;

namespace InterfaceGtk4;

/// <summary>
/// Клас для даних моделі яка викоритовується для виведення дерева конфігурації
/// </summary>
[Subclass<GObject.Object>]
partial class ConfiguratorItemRow
{
    public static ConfiguratorItemRow New() => NewWithProperties([]);

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
    /// Назва таблиці чи поля
    /// </summary>
    public string TableOrField { get; set; } = "";

    /// <summary>
    /// Тип даних
    /// </summary>
    public string Type { get; set; } = "";

    /// <summary>
    /// Пояснення
    /// </summary>
    public string Desc { get; set; } = "";
}