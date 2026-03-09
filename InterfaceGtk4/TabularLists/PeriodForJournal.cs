/*
Copyright (C) 2019-2026 TARAKHOMYN YURIY IVANOVYCH
All rights reserved.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

/*
Автор:    Тарахомин Юрій Іванович
Адреса:   Україна, м. Львів
Сайт:     accounting.org.ua
*/

using Gtk;
using AccountingSoftware;

namespace InterfaceGtk4;

/// <summary>
/// Періоди для журналів
/// </summary>
public static class PeriodForJournal
{
    /// <summary>
    /// Варіанти типів періодів
    /// </summary>
    public enum TypePeriod
    {
        AllPeriod = 1,
        Special,
        LastYear,
        ThisYear,
        LastQuarter,
        ThisQuarter,
        LastMonth,
        ThisMonth,
        LastWeek,
        ThisWeek,
        Yesterday,
        Today
    }

    /// <summary>
    /// Псевдонім для типу періоду
    /// </summary>
    /// <param name="value">Тип періоду</param>
    /// <returns>Назва</returns>
    static string TypePeriod_Alias(TypePeriod value)
    {
        return value switch
        {
            TypePeriod.AllPeriod => "Весь період",
            TypePeriod.Special => "Особливий",
            TypePeriod.LastYear => "Минулий рік",
            TypePeriod.ThisYear => "Цей рік",
            TypePeriod.LastQuarter => "Минулий квартал",
            TypePeriod.ThisQuarter => "Цей квартал",
            TypePeriod.LastMonth => "Минулий місяць",
            TypePeriod.ThisMonth => "Цей місяць",
            TypePeriod.LastWeek => "Минулий тиждень",
            TypePeriod.ThisWeek => "Цей тиждень",
            TypePeriod.Yesterday => "Вчора",
            TypePeriod.Today => "Сьогодні",
            _ => ""
        };
    }

    /// <summary>
    /// Список доступних відборів.
    /// Використовується із PeriodControl
    /// </summary>
    /// <returns>ComboBoxText</returns>
    public static ComboBoxText PeriodSelectionList()
    {
        ComboBoxText сomboBox = new();

        foreach (TypePeriod value in Enum.GetValues<TypePeriod>())
            сomboBox.Append(value.ToString(), TypePeriod_Alias(value));

        return сomboBox;
    }

    /// <summary>
    /// Повертає сховище з набором варантів періоду.
    /// Використовується із PeriodControl. Заготовка на майбутнє
    /// </summary>
    /// <returns>Сховище Gio.ListStore</returns>
    public static Gio.ListStore PeriodSelectionStore()
    {
        Gio.ListStore store = Gio.ListStore.New(PeriodItemRow.GetGType());

        foreach (TypePeriod value in Enum.GetValues<TypePeriod>())
            store.Append(new PeriodItemRow(value, TypePeriod_Alias(value)));

        return store;
    }

    #region Функції

    /// <summary>
    /// Початок тижня
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Дата початок тижня</returns>
    static DateTime StartOfWeek(DateTime dt) => dt.AddDays(-(((int)dt.DayOfWeek + 6) % 7));

    /// <summary>
    /// Кінець тижня
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Дата кінець тижня</returns>
    static DateTime EndOfWeek(DateTime dt) => StartOfWeek(dt).AddDays(6);

    /// <summary>
    /// Список кварталів для року із дати
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Список із кортежами де перший параметр дата початку кварталу а другий це кінець кварталу</returns>
    static List<(DateTime Start, DateTime End)> GetQuarterList(DateTime dt)
    {
        DateTime StartOfYear = new(dt.Year, 1, 1);

        List<(DateTime Start, DateTime End)> Quarters =
        [
            (StartOfYear, StartOfYear.AddMonths(3).AddDays(-1)),              //1                
            (StartOfYear.AddMonths(3), StartOfYear.AddMonths(6).AddDays(-1)), //2                
            (StartOfYear.AddMonths(6), StartOfYear.AddMonths(9).AddDays(-1)), //3                
            (StartOfYear.AddMonths(9), StartOfYear.AddMonths(12).AddDays(-1)) //4
        ];

        return Quarters;
    }

    /// <summary>
    /// Початок кварталу
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Дата початку кварталу для дати</returns>
    static DateTime StartQuarter(DateTime dt)
    {
        DateTime? date = null;
        DateTime dtDateOnly = dt.Date;

        foreach (var (Start, End) in GetQuarterList(dt))
            if (dtDateOnly >= Start && dtDateOnly <= End)
            {
                date = Start;
                break;
            }

        return date ?? DateTime.MinValue;
    }

    /// <summary>
    /// Кінець кварталу
    /// </summary>
    /// <param name="dt">Дата</param>
    /// <returns>Дата кінця кварталу для дати</returns>
    static DateTime EndQuarter(DateTime dt)
    {
        DateTime? date = null;
        DateTime dtDateOnly = dt.Date;

        foreach (var (Start, End) in GetQuarterList(dt))
            if (dtDateOnly >= Start && dtDateOnly <= End)
            {
                date = End;
                break;
            }

        return date ?? DateTime.MinValue;
    }

    #endregion

    /// <summary>
    /// Дата початку для періоду
    /// </summary>
    /// <param name="typePeriod">Тип періоду</param>
    /// <returns>Дата</returns>
    public static DateTime? DateStartOfPeriod(TypePeriod typePeriod)
    {
        DateTime? dateTime = typePeriod switch
        {
            TypePeriod.LastYear => new DateTime(DateTime.Now.AddYears(-1).Year, 1, 1),
            TypePeriod.ThisYear => new DateTime(DateTime.Now.Year, 1, 1),
            TypePeriod.LastQuarter => StartQuarter(DateTime.Now).AddMonths(-3),
            TypePeriod.ThisQuarter => StartQuarter(DateTime.Now),
            TypePeriod.LastMonth => new DateTime(DateTime.Now.Year, DateTime.Now.AddMonths(-1).Month, 1),
            TypePeriod.ThisMonth => new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
            TypePeriod.LastWeek => StartOfWeek(DateTime.Now.AddDays(-7)),
            TypePeriod.ThisWeek => StartOfWeek(DateTime.Now),
            TypePeriod.Yesterday => DateTime.Now.AddDays(-1),
            TypePeriod.Today => DateTime.Now,
            _ => null
        };

        return dateTime?.Date;
    }

    /// <summary>
    /// Дата кінця для періоду
    /// </summary>
    /// <param name="typePeriod">Тип періоду</param>
    /// <returns>Дата</returns>
    public static DateTime? DateEndOfPeriod(TypePeriod typePeriod)
    {
        DateTime? dateTime = typePeriod switch
        {
            TypePeriod.LastYear => new DateTime(DateTime.Now.Year, 1, 1).AddDays(-1),
            TypePeriod.ThisYear => null,
            TypePeriod.LastQuarter => EndQuarter(DateTime.Now.AddMonths(-3)),
            TypePeriod.ThisQuarter => null,
            TypePeriod.LastMonth => new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1),
            TypePeriod.ThisMonth => null,
            TypePeriod.LastWeek => EndOfWeek(DateTime.Now.AddDays(-7)),
            TypePeriod.ThisWeek => null,
            TypePeriod.Yesterday => DateTime.Now.AddDays(-1),
            TypePeriod.Today => null,
            _ => null
        };

        return dateTime?.Date;
    }

    /// <summary>
    /// Відбір по періоду
    /// </summary>
    /// <param name="fieldWhere">Назва поля для відбору</param>
    /// <param name="typePeriod">Тип періоду</param>
    /// <param name="start">Дата початку</param>
    /// <param name="stop">Дата кінця</param>
    /// <returns>Відбір</returns>
    public static Where? SelectionByPeriod(string fieldWhere, TypePeriod typePeriod, DateTime? start = null, DateTime? stop = null)
    {
        //Локальна функція
        Where whereLocalFunc(DateTime startDate, DateTime endDate)
        {
            string start_format = startDate.ToString("yyyy-MM-dd 00:00:00");
            string stop_format = endDate.ToString("yyyy-MM-dd 23:59:59");

            return new Where(fieldWhere, Comparison.BETWEEN, $"'{start_format}' AND '{stop_format}'", true);
        }

        if (typePeriod == TypePeriod.AllPeriod)
            return null;
        else if (typePeriod == TypePeriod.Special)
            return (start != null && stop != null) ? whereLocalFunc(start.Value, stop.Value) : null;
        else
            return whereLocalFunc(DateStartOfPeriod(typePeriod) ?? DateTime.MinValue, DateEndOfPeriod(typePeriod) ?? DateTime.Now);
    }
}