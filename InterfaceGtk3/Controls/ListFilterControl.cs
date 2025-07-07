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

Фільтер для форми журнал

*/

using Gtk;

namespace InterfaceGtk3;

public class ListFilterControl : Box
{
    /// <summary>
    /// Функція вибірки даних
    /// </summary>
    public System.Action? Select { get; set; }

    /// <summary>
    /// Скидання фільтрів
    /// </summary>
    public System.Action? Clear { get; set; }

    public record FilterListItem(string Field, Func<object> GetValueFunc, Switch IsOn);

    /// <summary>
    /// Список для фільтрів
    /// </summary>
    public System.Action<ListFilterControl>? FillFilterList { get; set; }

    /// <summary>
    /// Функція яка згенерована в коді
    /// Вона формує набір відборів
    /// </summary>
    public System.Func<bool>? GetWhere { get; set; }

    /// <summary>
    /// Список фільтрів
    /// </summary>
    public ListBox FilterList { get; private set; } = new ListBox() { SelectionMode = SelectionMode.None };

    /// <summary>
    /// Період із журналу
    /// </summary>
    public PeriodControl? Період { get; set; } = null;

    /// <summary>
    /// Галочка яка вказує на те що потрібно враховувати період у фільтрі
    /// </summary>
    public CheckButton UsePeriod { get; private set; } = new CheckButton("В межах періоду");

    public ListFilterControl(bool usePeriod = false) : base(Orientation.Vertical, 0)
    {
        //В межах періоду
        if (usePeriod)
        {
            Box hBox = new Box(Orientation.Horizontal, 0);
            PackStart(hBox, false, false, 5);

            hBox.PackStart(UsePeriod, false, false, 5);
            UsePeriod.Active = true;
        }

        //Список
        {
            Box hBox = new Box(Orientation.Horizontal, 0);
            PackStart(hBox, false, false, 5);

            hBox.PackStart(FilterList, false, false, 5);
        }

        //Кнопки
        {
            Box hBox = new Box(Orientation.Horizontal, 0) { Halign = Align.Center };
            PackStart(hBox, false, false, 5);

            Button buttonFilter = new Button("Фільтрувати");
            buttonFilter.Clicked += (sender, arrg) =>
            {
                if (FilterList.Children.Cast<ListBoxRow>().Any())
                {
                    bool existFilter = GetWhere?.Invoke() ?? false;
                    IsFiltered = existFilter;

                    if (existFilter) Select?.Invoke();
                }
            };
            hBox.PackStart(buttonFilter, false, false, 5);

            Button buttonClear = new Button("Скинути");
            buttonClear.Clicked += (sender, arrg) =>
            {
                if (FilterList.Children.Cast<ListBoxRow>().Any())
                {
                    IsFiltered = false;
                    Clear?.Invoke();
                }
            };

            hBox.PackStart(buttonClear, false, false, 5);
        }

        ShowAll();
    }

    /// <summary>
    /// Відфільтровано, є фільтр
    /// </summary>
    public bool IsFiltered { get; set; } = false;

    /// <summary>
    /// Створене спливаюче вікно
    /// </summary>
    public bool IsFilterCreated { get; private set; } = false;

    /// <summary>
    /// Вспливаюче вікно власник
    /// </summary>
    public Popover? PopoverParent { get; private set; }

    /// <summary>
    /// Створити спливаюче вікно фільтру
    /// </summary>
    /// <param name="relative_to">Привязка</param>
    public void CreatePopover(Widget relative_to)
    {
        if (!IsFilterCreated)
        {
            PopoverParent = new Popover(relative_to) { Position = PositionType.Bottom, BorderWidth = 2 };
            PopoverParent.Add(this);

            FillFilterList?.Invoke(this);
            IsFilterCreated = true;
        }
    }

    /// <summary>
    /// Добавлення елементу фільтру у список фільтрів
    /// </summary>
    /// <param name="caption">Заголовок</param>
    /// <param name="widget">Віджет</param>
    /// <param name="sw">Включено / Виключено</param>
    public void Append(string caption, Widget widget, Switch sw)
    {
        Box hBox = new Box(Orientation.Horizontal, 0);
        Box vBox = new Box(Orientation.Vertical, 0);
        vBox.PackStart(hBox, false, false, 5);

        //Заголовок
        hBox.PackStart(new Label(caption), false, false, 5);

        //Включено / Виключено
        {
            Box vBoxSw = new Box(Orientation.Vertical, 0) { Valign = Align.Center };
            hBox.PackEnd(vBoxSw, false, false, 0);

            Box hBoxSw = new Box(Orientation.Horizontal, 0) { Halign = Align.Center };
            hBoxSw.PackStart(sw, false, false, 5);
            vBoxSw.PackStart(hBoxSw, false, false, 0);
        }

        //Віджет
        hBox.PackEnd(widget, false, false, 5);

        FilterList.Add(new ListBoxRow() { vBox });
    }
}