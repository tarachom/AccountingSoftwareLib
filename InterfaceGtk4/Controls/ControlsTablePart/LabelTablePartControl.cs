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

public class LabelTablePartControl : Box
{
    Box hBox;
    Label label;

    public LabelTablePartControl()
    {
        SetOrientation(Orientation.Vertical);

        hBox = New(Orientation.Horizontal, 0);
        hBox.Valign = Align.Center;
        hBox.Vexpand = true;

        Append(hBox);
        AddCssClass("base");

        label = Label.New(null);
        hBox.Append(label);
    }

    public void SetText(string? text)
    {
        label.SetText(text ?? "");
    }

    public void SetType(string type)
    {
        if (type == "integer" || type == "numeric")
        {
            hBox.Halign = Align.End;
            AddCssClass("numeric");
        }
    }

    public static LabelTablePartControl New(string? text)
    {
        LabelTablePartControl lbl = new();
        lbl.SetText(text);

        return lbl;
    }

    public static LabelTablePartControl NewFromType(string type)
    {
        LabelTablePartControl lbl = new();
        lbl.SetType(type);

        return lbl;
    }
}