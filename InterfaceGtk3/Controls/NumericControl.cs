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
using System.Globalization;

namespace InterfaceGtk3;

public class NumericControl : Box
{
    Entry entryNumeric = new Entry() { WidthChars = 16 };
    Box hBoxInfoValid = new Box(Orientation.Horizontal, 0) { WidthRequest = 16 };
    NumberFormatInfo numberFormatUA = new CultureInfo("uk-UA", false).NumberFormat;

    public NumericControl() : base(Orientation.Horizontal, 0)
    {
        numberFormatUA.NumberGroupSeparator = " ";
        numberFormatUA.NumberDecimalDigits = 2;

        PackStart(hBoxInfoValid, false, false, 1);

        entryNumeric.Changed += OnEntryNumericChanged;
        PackStart(entryNumeric, false, false, 2);
    }

    decimal mValue;
    public decimal Value
    {
        get
        {
            return mValue;
        }
        set
        {
            mValue = value;
            entryNumeric.Text = mValue.ToString("N", numberFormatUA);
            entryNumeric.TooltipText = entryNumeric.Text;
        }
    }

    void ClearHBoxInfoValid()
    {
        foreach (Widget item in hBoxInfoValid.Children)
            hBoxInfoValid.Remove(item);
    }

    public bool IsValidValue()
    {
        ClearHBoxInfoValid();

        if (decimal.TryParse(entryNumeric.Text, out decimal value))
        {
            mValue = value;

            hBoxInfoValid.Add(new Image(Іконки.ДляІнформування.Ok));
            hBoxInfoValid.ShowAll();

            return true;
        }
        else
        {
            hBoxInfoValid.Add(new Image(Іконки.ДляІнформування.Error));
            hBoxInfoValid.ShowAll();

            return false;
        }
    }

    void OnEntryNumericChanged(object? sender, EventArgs args)
    {
        IsValidValue();
    }

    public static decimal FormatUA(decimal value)
    {
        return Math.Round(decimal.Parse(value.ToString("N", new CultureInfo("uk-UA", false).NumberFormat)), 2);
    }
}