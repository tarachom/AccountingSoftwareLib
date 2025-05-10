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

/*

Контрол для вибору масиву елементів

*/

using Gtk;

namespace InterfaceGtk
{
    public abstract class MultiplePointerControl : PointerControl
    {
        public MultiplePointerControl()
        {
            Button bMultiple = new Button(new Image(Stock.GoDown, IconSize.Menu));
            PackStart(bMultiple, false, false, 1);
            bMultiple.Clicked += OnMultiple;
        }

        /// <summary>
        /// Заповнення списку елементами
        /// </summary>
        /// <param name="listBox">Список</param>
        /// <returns></returns>
        protected abstract ValueTask FillList(ListBox listBox);

        protected virtual async void OnMultiple(object? sender, EventArgs args)
        {
            Popover popover = new Popover((Button)sender!) { Position = PositionType.Bottom, BorderWidth = 4 };

            ListBox listBox = new() { SelectionMode = SelectionMode.None };

            ScrolledWindow scroll = new ScrolledWindow() { ShadowType = ShadowType.In, HeightRequest = 300, WidthRequest = 750 };
            scroll.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
            scroll.Add(listBox);

            popover.Add(scroll);
            popover.ShowAll();

            await FillList(listBox);
        }
    }
}