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
/// Клітинка табличної частини - Ціле число
/// </summary>
public class IntegerTablePartCell : Box
{
    Box hBox;
    Entry entry = Entry.New();

    public IntegerTablePartCell()
    {
        SetOrientation(Orientation.Vertical);

        hBox = New(Orientation.Horizontal, 0);
        hBox.Vexpand = true;

        entry.OnChanged += (_, _) => IsValid();
        entry.Vexpand = entry.Hexpand = true;
        entry.SetAlignment(0.8f);
        hBox.Append(entry);

        Append(hBox);
        AddCssClass("integer");
    }

    public int Value
    {
        get
        {
            return value_;
        }
        set
        {
            if (value_ != value)
            {
                value_ = value;
                entry.SetText(value_ == 0 ? "" : value_.ToString());
                entry.TooltipText = entry.GetText();
            }
        }
    }
    int value_ = 0;

    /// <summary>
    /// Функція яка викликається після зміни
    /// </summary>
    public Action? OnСhanged { get; set; }

    void IsValid()
    {
        foreach (var cssclass in entry.CssClasses)
            entry.RemoveCssClass(cssclass);

        if (string.IsNullOrEmpty(entry.Text_))
        {
            value_ = 0;
            OnСhanged?.Invoke();
            return;
        }

        if (int.TryParse(entry.Text_, out int value))
        {
            value_ = value;
            OnСhanged?.Invoke();
        }
        else
            entry.AddCssClass("error");
    }
}