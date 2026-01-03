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

/*

Контрол для вибору масиву елементів

*/

using Gtk;

namespace InterfaceGtk4;

public abstract class MultiplePointerControl : PointerControl
{
    public MultiplePointerControl()
    {
        Button bMultiple = Button.NewFromIconName("go-down");
        bMultiple.MarginEnd = 1;
        bMultiple.OnClicked += OnMultiple;
        Append(bMultiple);
    }

    /// <summary>
    /// Заповнення списку елементами
    /// </summary>
    /// <param name="listBox">Список</param>
    /// <returns></returns>
    protected abstract ValueTask FillList(ListBox listBox);

    protected virtual async void OnMultiple(object? sender, EventArgs args)
    {
        Popover popover = Popover.New();
        popover.SetParent((Button)sender!);
        popover.Position = PositionType.Bottom;
        popover.MarginTop = popover.MarginEnd = popover.MarginBottom = popover.MarginStart = 5;

        ListBox listBox = new() { SelectionMode = SelectionMode.None };

        ScrolledWindow scroll = new() { HeightRequest = 300, WidthRequest = 750 };
        scroll.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
        scroll.SetChild(listBox);

        popover.SetChild(scroll);

        await FillList(listBox);
    }

    protected static string SubstringName(string name) => name.Length >= 90 ? name[..87] + "..." : name;
}