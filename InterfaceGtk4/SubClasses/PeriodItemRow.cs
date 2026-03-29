
using GObject;

namespace InterfaceGtk4;

/// <summary>
/// Для сховища вибору варіантів періоду
/// </summary>
[Subclass<GObject.Object>]
partial class PeriodItemRow
{
    public static PeriodItemRow New() => NewWithProperties([]);

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="typePeriod">Тип періоду</param>
    /// <param name="name">Назва</param>
    public static PeriodItemRow NewWithPeriod(PeriodForJournal.TypePeriod typePeriod, string name)
    {
        var item = NewWithProperties([]);
        item.TypePeriod = typePeriod;
        item.Name = name;

        return item;
    }

    /// <summary>
    /// Тип періоду
    /// </summary>
    public PeriodForJournal.TypePeriod TypePeriod { get; set; } = PeriodForJournal.TypePeriod.AllPeriod;

    /// <summary>
    /// Назва
    /// </summary>
    public string Name { get; set; } = "";


}