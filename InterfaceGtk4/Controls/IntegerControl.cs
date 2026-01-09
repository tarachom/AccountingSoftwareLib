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

namespace InterfaceGtk4;

public class IntegerControl : Box
{
    Entry entryInteger = new();
    Box hBoxInfoValid = New(Orientation.Horizontal, 0);

    public IntegerControl()
    {
        SetOrientation(Orientation.Horizontal);

        //Info box valid
        hBoxInfoValid.WidthRequest = 16;
        hBoxInfoValid.MarginEnd = 2;
        Append(hBoxInfoValid);

        //Entry
        entryInteger.OnChanged += (_, _) => IsValidValue();
        //entryInteger.MarginEnd = 2;
        Append(entryInteger);
    }

    int mValue;
    public int Value
    {
        get => mValue;
        set
        {
            mValue = value;
            entryInteger.SetText( mValue.ToString());
            entryInteger.TooltipText = entryInteger.GetText();
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

        if (string.IsNullOrEmpty(entryInteger.Text_))
        {
            mValue = 0;
            return true;
        }

        if (int.TryParse(entryInteger.Text_, out int value))
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
}