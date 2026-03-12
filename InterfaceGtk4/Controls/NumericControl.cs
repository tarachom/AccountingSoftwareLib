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
using System.Globalization;

namespace InterfaceGtk4;

public class NumericControl : Box
{
    Entry entry = new();
    NumberFormatInfo numberFormatUA = new CultureInfo("uk-UA", false).NumberFormat;

    public NumericControl()
    {
        SetOrientation(Orientation.Horizontal);

        numberFormatUA.NumberGroupSeparator = " ";
        numberFormatUA.NumberDecimalDigits = 2;

        //Entry
        entry.OnChanged += (_, _) => IsValidValue();
        entry.MarginStart = 5;
        entry.MarginEnd = 2;
        Append(entry);
    }

    decimal mValue;
    public decimal Value
    {
        get => mValue;
        set
        {
            mValue = value;
            entry.SetText(mValue.ToString("N", numberFormatUA));
            entry.TooltipText = entry.GetText();
        }
    }

    /// <summary>
    /// Перевірити правильність заповнення
    /// </summary>
    /// <returns>true якщо все ок</returns>
    public bool IsValidValue()
    {
        /* Можна зробити обмеження .Where(x => x == "error") */
        foreach (var cssclass in entry.CssClasses)
            entry.RemoveCssClass(cssclass);

        if (string.IsNullOrEmpty(entry.GetText()))
        {
            mValue = 0;
            return true;
        }

        if (decimal.TryParse(entry.GetText(), out decimal value))
        {
            mValue = value;
            return true;
        }
        else
        {
            entry.AddCssClass("error");
            return false;
        }
    }

    public static decimal FormatUA(decimal value) => Math.Round(decimal.Parse(value.ToString("N", new CultureInfo("uk-UA", false).NumberFormat)), 2);
}