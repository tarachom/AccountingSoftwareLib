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
public class ComboTextTablePartCell : Box
{
    Box hBox;
    public ComboBoxText Combo { get; } = ComboBoxText.New();

    public ComboTextTablePartCell()
    {
        SetOrientation(Orientation.Vertical);

        hBox = New(Orientation.Horizontal, 0);
        hBox.Vexpand = true;

        Combo.OnChanged += (_, _) => OnСhanged?.Invoke();
        Combo.Vexpand = Combo.Hexpand = true;
        hBox.Append(Combo);

        Append(hBox);
        AddCssClass("combotext");
    }

    public string Value
    {
        get
        {
            return Value_;
        }
        set
        {
            if (Value_ != value)
            {
                Value_ = value;
                Combo.ActiveId = Value_;
            }
        }
    }
    string Value_ = "";

    /// <summary>
    /// Функція яка викликається після зміни
    /// </summary>
    public Action? OnСhanged { get; set; }
}