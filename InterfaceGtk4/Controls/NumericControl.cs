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

namespace InterfaceGtk4;

public class NumericControl : Box
{
    Entry entryNumeric = new();
    Box hBoxInfoValid = New(Orientation.Horizontal, 0);
    NumberFormatInfo numberFormatUA = new CultureInfo("uk-UA", false).NumberFormat;

    public NumericControl()
    {
        SetOrientation(Orientation.Horizontal);

        numberFormatUA.NumberGroupSeparator = " ";
        numberFormatUA.NumberDecimalDigits = 2;

        //Info box valid
        hBoxInfoValid.WidthRequest = 16;
        hBoxInfoValid.MarginEnd = 2;
        Append(hBoxInfoValid);

        //Entry
        entryNumeric.OnChanged += (_, _) => IsValidValue();
        entryNumeric.MarginEnd = 2;
        Append(entryNumeric);
    }

    decimal mValue;
    public decimal Value
    {
        get => mValue;
        set
        {
            mValue = value;
            entryNumeric.Text_ = mValue.ToString("N", numberFormatUA);
            entryNumeric.TooltipText = entryNumeric.Text_;
        }
    }

    void ClearHBoxInfoValid()
    {
        Widget? child = hBoxInfoValid.GetFirstChild();
        if (child != null) hBoxInfoValid.Remove(child);
    }

    public bool IsValidValue()
    {
        ClearHBoxInfoValid();

        if (string.IsNullOrEmpty(entryNumeric.Text_))
        {
            mValue = 0;
            return true;
        }

        if (decimal.TryParse(entryNumeric.Text_, out decimal value))
        {
            mValue = value;

            hBoxInfoValid.Append(Image.NewFromPixbuf(Icon.ForInformation.Ok));
            return true;
        }
        else
        {
            hBoxInfoValid.Append(Image.NewFromPixbuf(Icon.ForInformation.Error));
            return false;
        }
    }

    public static decimal FormatUA(decimal value) => Math.Round(decimal.Parse(value.ToString("N", new CultureInfo("uk-UA", false).NumberFormat)), 2);
}