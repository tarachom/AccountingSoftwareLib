
using GObject;

namespace InterfaceGtk4;

/// <summary>
/// Для сховища вибору варіантів періоду
/// </summary>
[Subclass<GObject.Object>]
partial class PeriodItemRow
{
    /// <summary>
    /// Тип періоду
    /// </summary>
    public PeriodForJournal.TypePeriod TypePeriod { get; set; } = PeriodForJournal.TypePeriod.AllPeriod;

    /// <summary>
    /// Назва
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="typePeriod">Тип періоду</param>
    /// <param name="name">Назва</param>
    public PeriodItemRow(PeriodForJournal.TypePeriod typePeriod, string name) : this()
    {
        TypePeriod = typePeriod;
        Name = name;
    }
}