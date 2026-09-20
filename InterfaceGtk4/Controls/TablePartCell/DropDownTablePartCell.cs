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
/// Клітинка табличної частини - Випадаючий список
/// </summary>
[GObject.Subclass<Box>]
public partial class DropDownTablePartCell : Box
{
    Box hBox = New(Orientation.Horizontal, 0);
    public Gio.ListStore Store { get; } = Gio.ListStore.New(DropDownItemRow.GetGType());
    public DropDown DropDown { get; private set; } = DropDown.NewWithProperties([]);

    partial void Initialize()
    {
        SetOrientation(Orientation.Vertical);

        DropDown = DropDown.New(Store, null);
        DropDown.Vexpand = DropDown.Hexpand = true;
        DropDown.OnNotify += (_, e) =>
        {
            if (e.Pspec.GetName() == "selected" && DropDown.SelectedItem != null)
            {
                DropDownItemRow itemRow = (DropDownItemRow)DropDown.SelectedItem;
                value_ = itemRow.Name;
                OnСhanged?.Invoke();
            }
        };

        //Фабрика
        {
            var factory = SignalListItemFactory.New();
            factory.OnSetup += (_, args) =>
            {
                if (args.Object is not ListItem listItem) return;

                Label label = Label.New(null);
                label.Halign = Align.Start;
                listItem.Child = label;
            };

            factory.OnBind += (_, args) =>
            {
                if (args.Object is not ListItem listItem) return;
                if (listItem.Child is not Label label) return;
                if (listItem.Item is not DropDownItemRow row) return;

                label.SetText(row.Desc);
            };
            DropDown.Factory = factory;
        }

        hBox.Append(DropDown);

        Append(hBox);
        AddCssClass("dropdown");
    }

    public static DropDownTablePartCell New() => NewWithProperties([]);

    public static DropDownTablePartCell NewWithValues(Dictionary<string, string> values, bool allowEmpty = true)
    {
        DropDownTablePartCell dropDown = NewWithProperties([]);
        dropDown.AllowEmpty = allowEmpty;
        dropDown.Fill(values);

        return dropDown;
    }

    /// <summary>
    /// Заповнення списку значеннями
    /// </summary>
    /// <param name="values">Словник із значеннями</param>
    public void Fill(Dictionary<string, string> values)
    {
        Store.RemoveAll();

        //Пусте значення
        if (AllowEmpty) AppendEmpty();

        foreach (var value in values)
            Store.Append(DropDownItemRow.NewWithPair(value));
    }

    /// <summary>
    /// Додає пусте значення
    /// </summary>
    void AppendEmpty() => Store.Append(DropDownItemRow.NewWithValue("", ""));

    /// <summary>
    /// Додає значення
    /// </summary>
    public void Append(string name, string desc)
    {
        //Пусте значення
        if (AllowEmpty && Store.GetNItems() == 0) AppendEmpty();
        Store.Append(DropDownItemRow.NewWithValue(name, desc));
    }

    public void Append(string name) => Append(name, name);

    public void RemoveAll() => Store.RemoveAll();

    public uint Count() => Store.GetNItems();

    // <summary>
    /// Список може містити значення 0, Незаповнено
    /// </summary>
    public bool AllowEmpty { get; set; } = true;

    public string Value
    {
        get => value_;
        set
        {
            if (value_ != value)
                value_ = FunctionForDropDown.SelectByValue(DropDown, value) ? value : "";
        }
    }
    string value_ = "";

    /// <summary>
    /// Функція яка викликається після зміни
    /// </summary>
    public Action? OnСhanged { get; set; } = null;
}