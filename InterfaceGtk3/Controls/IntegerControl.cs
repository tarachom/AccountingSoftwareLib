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

namespace InterfaceGtk3;

public class IntegerControl : Box
{
    Entry entryInteger = new Entry() { WidthChars = 10 };
    Box hBoxInfoValid = new Box(Orientation.Horizontal, 0) { WidthRequest = 16 };

    public IntegerControl() : base(Orientation.Horizontal, 0)
    {
        PackStart(hBoxInfoValid, false, false, 1);

        entryInteger.Changed += OnEntryIntegerChanged;
        PackStart(entryInteger, false, false, 2);
    }

    int mValue;
    public int Value
    {
        get
        {
            return mValue;
        }
        set
        {
            mValue = value;
            entryInteger.Text = mValue.ToString();
            entryInteger.TooltipText = entryInteger.Text;
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

        if (int.TryParse(entryInteger.Text, out int value))
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

    void OnEntryIntegerChanged(object? sender, EventArgs args)
    {
        IsValidValue();
    }
}