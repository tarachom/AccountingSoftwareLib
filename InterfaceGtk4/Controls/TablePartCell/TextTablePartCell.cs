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
[GObject.Subclass<Box>]
public partial class TextTablePartCell : Box
{
    Box hBox = New(Orientation.Horizontal, 0);
    Entry entry = Entry.New();

    partial void Initialize()
    {
        SetOrientation(Orientation.Vertical);

        hBox.Valign = Align.Center;
        hBox.Vexpand = true;

        entry.OnChanged += (_, _) => OnСhanged?.Invoke();
        entry.Vexpand = entry.Hexpand = true;
        hBox.Append(entry);

        Append(hBox);
        AddCssClass("text");
    }

    public static TextTablePartCell New() => NewWithProperties([]);

    public static TextTablePartCell NewWithString(string? text)
    {
        TextTablePartCell lbl = NewWithProperties([]);
        lbl.SetText(text);

        return lbl;
    }

    public string Value
    {
        get => entry.GetText();
        set
        {
            if (Value != value)
                entry.SetText(value);
        }
    }

    /// <summary>
    /// Функція яка викликається після зміни
    /// </summary>
    public Action? OnСhanged { get; set; }

    public void SetText(string? text)
    {
        entry.SetText(text ?? "");
    }
}