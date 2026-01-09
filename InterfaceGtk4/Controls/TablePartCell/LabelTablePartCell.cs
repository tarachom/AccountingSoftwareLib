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

/// <summary>
/// Клітинка табличної частини - Текст
/// </summary>
public class LabelTablePartCell : Box
{
    Box hBox;
    Label label;

    public LabelTablePartCell()
    {
        SetOrientation(Orientation.Vertical);

        hBox = New(Orientation.Horizontal, 0);
        hBox.Valign = Align.Center;
        hBox.Vexpand = true;

        label = Label.New(null);
        hBox.Append(label);

        Append(hBox);
        AddCssClass("base");
    }

    public string Text
    {
        get => label.GetText();
        set => label.SetText(value);
    }

    public void SetText(string? text)
    {
        label.SetText(text ?? "");
    }

    public void SetText(object? text)
    {
        label.SetText(text?.ToString() ?? "");
    }

    public void SetType(string type)
    {
        if (type == "integer" || type == "numeric")
        {
            hBox.Halign = Align.End;
            AddCssClass("numeric");
        }
    }

    public static LabelTablePartCell New(string? text)
    {
        LabelTablePartCell lbl = new();
        lbl.SetText(text);

        return lbl;
    }

    public static LabelTablePartCell NewFromType(string type)
    {
        LabelTablePartCell lbl = new();
        lbl.SetType(type);

        return lbl;
    }
}