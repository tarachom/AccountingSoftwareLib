/*
Copyright (C) 2019-2025 TARAKHOMYN YURIY IVANOVYCH
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

public static class PeriodForJournal
{
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

    public static ComboBoxText СписокВідбірПоПеріоду()
    {
        ComboBoxText сomboBox = new();

        foreach (TypePeriod value in Enum.GetValues<TypePeriod>())
            сomboBox.Append(value.ToString(), TypePeriod_Alias(value));

        return сomboBox;
    }

    #region Функції

    static DateTime ПочатокТижня(DateTime dt) => dt.AddDays(-(((int)dt.DayOfWeek + 6) % 7));

    static DateTime КінецьТижня(DateTime dt) => ПочатокТижня(dt).AddDays(6);

    static List<(DateTime Початок, DateTime Кінець)> СписокКварталів(DateTime dt)
    {
        DateTime ПочатокРоку = new(DateTime.Now.Year, 1, 1);

        List<(DateTime Початок, DateTime Кінець)> Квартали =
        [
            (ПочатокРоку, ПочатокРоку.AddMonths(3).AddDays(-1)),              //1                
            (ПочатокРоку.AddMonths(3), ПочатокРоку.AddMonths(6).AddDays(-1)), //2                
            (ПочатокРоку.AddMonths(6), ПочатокРоку.AddMonths(9).AddDays(-1)), //3                
            (ПочатокРоку.AddMonths(9), ПочатокРоку.AddMonths(12).AddDays(-1)) //4
        ];

        return Квартали;
    }

    static DateTime ПочатокКварталу(DateTime dt)
    {
        DateTime? Дата = null;
        DateTime dtDateOnly = dt.Date;

        foreach (var (Початок, Кінець) in СписокКварталів(dt))
            if (dtDateOnly >= Початок && dtDateOnly <= Кінець)
            {
                Дата = Початок;
                break;
            }

        return Дата ?? DateTime.MinValue;
    }

    static DateTime КінецьКварталу(DateTime dt)
    {
        DateTime? Дата = null;
        DateTime dtDateOnly = dt.Date;

        foreach (var (Початок, Кінець) in СписокКварталів(dt))
            if (dtDateOnly >= Початок && dtDateOnly <= Кінець)
            {
                Дата = Кінець;
                break;
            }

        return Дата ?? DateTime.MinValue;
    }

    #endregion

    public static DateTime? ДатаПочатокЗПеріоду(TypePeriod типПеріоду)
    {
        DateTime? dateTime = типПеріоду switch
        {
            TypePeriod.LastYear => new DateTime(DateTime.Now.AddYears(-1).Year, 1, 1),
            TypePeriod.ThisYear => new DateTime(DateTime.Now.Year, 1, 1),
            TypePeriod.LastQuarter => ПочатокКварталу(DateTime.Now).AddMonths(-3),
            TypePeriod.ThisQuarter => ПочатокКварталу(DateTime.Now),
            TypePeriod.LastMonth => new DateTime(DateTime.Now.Year, DateTime.Now.AddMonths(-1).Month, 1),
            TypePeriod.ThisMonth => new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
            TypePeriod.LastWeek => ПочатокТижня(DateTime.Now.AddDays(-7)),
            TypePeriod.ThisWeek => ПочатокТижня(DateTime.Now),
            TypePeriod.Yesterday => DateTime.Now.AddDays(-1),
            TypePeriod.Today => DateTime.Now,
            _ => null
        };

        return dateTime?.Date;
    }

    public static DateTime? ДатаКінецьЗПеріоду(TypePeriod типПеріоду)
    {
        DateTime? dateTime = типПеріоду switch
        {
            TypePeriod.LastYear => new DateTime(DateTime.Now.Year, 1, 1).AddDays(-1),
            TypePeriod.ThisYear => null,
            TypePeriod.LastQuarter => КінецьКварталу(DateTime.Now.AddMonths(-3)),
            TypePeriod.ThisQuarter => null,
            TypePeriod.LastMonth => new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1),
            TypePeriod.ThisMonth => null,
            TypePeriod.LastWeek => КінецьТижня(DateTime.Now.AddDays(-7)),
            TypePeriod.ThisWeek => null,
            TypePeriod.Yesterday => DateTime.Now.AddDays(-1),
            TypePeriod.Today => null,
            _ => null
        };

        return dateTime?.Date;
    }

    public static Where? ВідбірПоПеріоду(string fieldWhere, TypePeriod typePeriod, DateTime? start = null, DateTime? stop = null)
    {
        if (typePeriod == TypePeriod.AllPeriod)
            return null;
        else if (typePeriod == TypePeriod.Special)
        {
            if (start != null && stop != null)
            {
                string start_format = start.Value.ToString("yyyy-MM-dd 00:00:00");
                string stop_format = stop.Value.ToString("yyyy-MM-dd 23:59:59");

                return new Where(fieldWhere, Comparison.BETWEEN, $"'{start_format}' AND '{stop_format}'", true);
            }
            else
                return null;
        }
        else
        {
            DateTime dateStartTime = ДатаПочатокЗПеріоду(typePeriod) ?? DateTime.MinValue;
            DateTime dateStopTime = ДатаКінецьЗПеріоду(typePeriod) ?? DateTime.Now;

            string start_format = dateStartTime.ToString("yyyy-MM-dd 00:00:00");
            string stop_format = dateStopTime.ToString("yyyy-MM-dd 23:59:59");

            return new Where(fieldWhere, Comparison.BETWEEN, $"'{start_format}' AND '{stop_format}'", true);
        }
    }
}