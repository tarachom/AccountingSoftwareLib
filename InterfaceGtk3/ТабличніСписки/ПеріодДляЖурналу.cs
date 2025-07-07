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

namespace InterfaceGtk3;

public static class ПеріодДляЖурналу
{
    /*
    public enum ТипПеріоду
    {
        ВесьПеріод = 1,
        Особливий,
        Рік,
        ПівРоку,
        Квартал,
        ДваМісяці,
        Місяць,
        ТриТижні,
        ДваТижні,
        Тиждень,
        ШістьДнів,
        ПятьДнів,
        ЧотириДні,
        ТриДні,
        ДваДні,
        День
    }
    */

    public enum ТипПеріоду
    {
        ВесьПеріод = 1,
        Особливий,
        МинулийРік,
        ЦейРік,
        МинулийКвартал,
        ЦейКвартал,
        МинулийМісяць,
        ЦейМісяць,
        МинулийТиждень,
        ЦейТиждень,
        Вчора,
        Сьогодні
    }

    /*
    static string ТипПеріоду_Alias(ТипПеріоду value)
    {
        return value switch
        {
            ТипПеріоду.ВесьПеріод => "Весь період",
            ТипПеріоду.Особливий => "Особливий",
            ТипПеріоду.Рік => "Рік",
            ТипПеріоду.ПівРоку => "Пів року",
            ТипПеріоду.Квартал => "Три місяці",
            ТипПеріоду.ДваМісяці => "Два місяці",
            ТипПеріоду.Місяць => "Місяць",
            ТипПеріоду.ТриТижні => "Три тижні",
            ТипПеріоду.ДваТижні => "Два тижні",
            ТипПеріоду.Тиждень => "Тиждень",
            ТипПеріоду.ШістьДнів => "Шість днів",
            ТипПеріоду.ПятьДнів => "П'ять днів",
            ТипПеріоду.ЧотириДні => "Чотири дні",
            ТипПеріоду.ТриДні => "Три дні",
            ТипПеріоду.ДваДні => "Два дні",
            ТипПеріоду.День => "День",
            _ => ""
        };
    }
    */

    static string ТипПеріоду_Alias(ТипПеріоду value)
    {
        return value switch
        {
            ТипПеріоду.ВесьПеріод => "Весь період",
            ТипПеріоду.Особливий => "Особливий",
            ТипПеріоду.МинулийРік => "Минулий рік",
            ТипПеріоду.ЦейРік => "Цей рік",
            ТипПеріоду.МинулийКвартал => "Минулий квартал",
            ТипПеріоду.ЦейКвартал => "Цей квартал",
            ТипПеріоду.МинулийМісяць => "Минулий місяць",
            ТипПеріоду.ЦейМісяць => "Цей місяць",
            ТипПеріоду.МинулийТиждень => "Минулий тиждень",
            ТипПеріоду.ЦейТиждень => "Цей тиждень",
            ТипПеріоду.Вчора => "Вчора",
            ТипПеріоду.Сьогодні => "Сьогодні",
            _ => ""
        };
    }

    public static ComboBoxText СписокВідбірПоПеріоду()
    {
        ComboBoxText сomboBox = new ComboBoxText();

        foreach (ТипПеріоду value in Enum.GetValues<ТипПеріоду>())
            сomboBox.Append(value.ToString(), ТипПеріоду_Alias(value));

        return сomboBox;
    }

    /*
    public static DateTime? ДатаПочатокЗПеріоду(ТипПеріоду типПеріоду)
    {
        DateTime? dateTime = типПеріоду switch
        {
            ТипПеріоду.Рік => DateTime.Now.AddYears(-1),
            ТипПеріоду.ПівРоку => DateTime.Now.AddMonths(-6),
            ТипПеріоду.Квартал => DateTime.Now.AddMonths(-3),
            ТипПеріоду.ДваМісяці => DateTime.Now.AddMonths(-2),
            ТипПеріоду.Місяць => DateTime.Now.AddMonths(-1),
            ТипПеріоду.ТриТижні => DateTime.Now.AddDays(-20),
            ТипПеріоду.ДваТижні => DateTime.Now.AddDays(-13),
            ТипПеріоду.Тиждень => DateTime.Now.AddDays(-6),
            ТипПеріоду.ШістьДнів => DateTime.Now.AddDays(-5),
            ТипПеріоду.ПятьДнів => DateTime.Now.AddDays(-4),
            ТипПеріоду.ЧотириДні => DateTime.Now.AddDays(-3),
            ТипПеріоду.ТриДні => DateTime.Now.AddDays(-2),
            ТипПеріоду.ДваДні => DateTime.Now.AddDays(-1),
            ТипПеріоду.День => DateTime.Now,
            _ => null
        };

        return dateTime?.Date;
    }
    */

    #region Функції

    static DateTime ПочатокТижня(DateTime dt) => dt.AddDays(-(((int)dt.DayOfWeek + 6) % 7));

    static DateTime КінецьТижня(DateTime dt) => ПочатокТижня(dt).AddDays(6);

    static List<(DateTime Початок, DateTime Кінець)> СписокКварталів(DateTime dt)
    {
        DateTime ПочатокРоку = new DateTime(DateTime.Now.Year, 1, 1);

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

    public static DateTime? ДатаПочатокЗПеріоду(ТипПеріоду типПеріоду)
    {
        DateTime? dateTime = типПеріоду switch
        {
            ТипПеріоду.МинулийРік => new DateTime(DateTime.Now.AddYears(-1).Year, 1, 1),
            ТипПеріоду.ЦейРік => new DateTime(DateTime.Now.Year, 1, 1),
            ТипПеріоду.МинулийКвартал => ПочатокКварталу(DateTime.Now).AddMonths(-3),
            ТипПеріоду.ЦейКвартал => ПочатокКварталу(DateTime.Now),
            ТипПеріоду.МинулийМісяць => new DateTime(DateTime.Now.Year, DateTime.Now.AddMonths(-1).Month, 1),
            ТипПеріоду.ЦейМісяць => new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
            ТипПеріоду.МинулийТиждень => ПочатокТижня(DateTime.Now.AddDays(-7)),
            ТипПеріоду.ЦейТиждень => ПочатокТижня(DateTime.Now),
            ТипПеріоду.Вчора => DateTime.Now.AddDays(-1),
            ТипПеріоду.Сьогодні => DateTime.Now,
            _ => null
        };

        return dateTime?.Date;
    }

    public static DateTime? ДатаКінецьЗПеріоду(ТипПеріоду типПеріоду)
    {
        DateTime? dateTime = типПеріоду switch
        {
            ТипПеріоду.МинулийРік => new DateTime(DateTime.Now.Year, 1, 1).AddDays(-1),
            ТипПеріоду.ЦейРік => null,
            ТипПеріоду.МинулийКвартал => КінецьКварталу(DateTime.Now.AddMonths(-3)),
            ТипПеріоду.ЦейКвартал => null,
            ТипПеріоду.МинулийМісяць => new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1),
            ТипПеріоду.ЦейМісяць => null,
            ТипПеріоду.МинулийТиждень => КінецьТижня(DateTime.Now.AddDays(-7)),
            ТипПеріоду.ЦейТиждень => null,
            ТипПеріоду.Вчора => DateTime.Now.AddDays(-1),
            ТипПеріоду.Сьогодні => null,
            _ => null
        };

        return dateTime?.Date;
    }

    public static Where? ВідбірПоПеріоду(string fieldWhere, ТипПеріоду типПеріоду, DateTime? start = null, DateTime? stop = null)
    {
        if (типПеріоду == ТипПеріоду.ВесьПеріод)
            return null;
        else if (типПеріоду == ТипПеріоду.Особливий)
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
            DateTime dateStartTime = ДатаПочатокЗПеріоду(типПеріоду) ?? DateTime.MinValue;
            DateTime dateStopTime = ДатаКінецьЗПеріоду(типПеріоду) ?? DateTime.Now;

            string start_format = dateStartTime.ToString("yyyy-MM-dd 00:00:00");
            string stop_format = dateStopTime.ToString("yyyy-MM-dd 23:59:59");

            return new Where(fieldWhere, Comparison.BETWEEN, $"'{start_format}' AND '{stop_format}'", true);
        }
    }
}