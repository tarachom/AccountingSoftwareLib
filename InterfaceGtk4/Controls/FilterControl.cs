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

namespace InterfaceGtk4;

public class FilterControl : Box
{
    /// <summary>
    /// Функція вибірки даних
    /// </summary>
    public Action? Select { get; set; }

    /// <summary>
    /// Скидання фільтрів
    /// </summary>
    public Action? Clear { get; set; }

    /// <summary>
    /// Рекорд для колекції
    /// </summary>
    /// <param name="Field">Поле</param>
    /// <param name="GetValueFunc">Функція яка повертає значення</param>
    /// <param name="IsOn">Включено / Виключено</param>
    public record FilterListItem(string Field, Func<object> GetValueFunc, Switch IsOn);

    /// <summary>
    /// Список для фільтрів
    /// </summary>
    public Action<FilterControl>? FillFilterList { get; set; }

    /// <summary>
    /// Функція яка згенерована в коді
    /// Вона формує набір відборів
    /// </summary>
    public Func<bool>? GetWhere { get; set; }

    /// <summary>
    /// Список фільтрів
    /// </summary>
    public ListBox FilterList { get; private set; } = new ListBox() { SelectionMode = SelectionMode.None, Hexpand = true };

    /// <summary>
    /// Період із журналу
    /// </summary>
    public PeriodControl? Період { get; set; } = null;

    /// <summary>
    /// Галочка яка вказує на те що потрібно враховувати період у фільтрі
    /// </summary>
    public CheckButton UsePeriod { get; private set; } = CheckButton.NewWithLabel("В межах періоду");

    public FilterControl(bool usePeriod = false)
    {
        SetOrientation(Orientation.Vertical);
        //Hexpand = Vexpand = true;

        //В межах періоду
        if (usePeriod)
        {
            Box hBox = New(Orientation.Horizontal, 0);
            Append(hBox);

            UsePeriod.Active = true;
            hBox.Append(UsePeriod);
        }

        //Список
        {
            Box hBox = New(Orientation.Horizontal, 0);
            Append(hBox);

            hBox.Append(FilterList);
        }

        //Кнопки
        {
            Box hBox = New(Orientation.Horizontal, 0);
            hBox.Halign = Align.Center;
            hBox.MarginTop = 10;
            Append(hBox);

            Button buttonFilter = Button.NewWithLabel("Фільтрувати");
            buttonFilter.MarginEnd = 5;
            buttonFilter.OnClicked += (_, _) =>
            {
                //Є хоть один елемент в списку
                if (FilterList.GetFirstChild() != null)
                {
                    bool existFilter = GetWhere?.Invoke() ?? false;
                    IsFiltered = existFilter;

                    if (existFilter) Select?.Invoke();

                    PopoverParent?.Hide();
                }
            };
            hBox.Append(buttonFilter);

            Button buttonClear = Button.NewWithLabel("Скинути");
            buttonClear.OnClicked += (_, _) =>
            {
                //Є хоть один елемент в списку
                if (FilterList.GetFirstChild() != null)
                {
                    IsFiltered = false;
                    Clear?.Invoke();

                    PopoverParent?.Hide();
                }
            };
            hBox.Append(buttonClear);
        }
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
            PopoverParent = Popover.New();
            PopoverParent.SetParent(relative_to);
            PopoverParent.Position = PositionType.Bottom;
            PopoverParent.MarginTop = PopoverParent.MarginEnd = PopoverParent.MarginBottom = PopoverParent.MarginStart = 2;
            PopoverParent.SetChild(this);

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
        Box hBox = New(Orientation.Horizontal, 0);
        hBox.MarginStart = hBox.MarginEnd = hBox.MarginTop = hBox.MarginBottom = 5;
        hBox.Hexpand = true;

        Box vBox = New(Orientation.Vertical, 0);
        vBox.Hexpand = true;
        vBox.Append(hBox);

        //Заголовок
        Label label = Label.New(caption);
        label.MarginEnd = 5;
        hBox.Append(label);

        //Віджет
        widget.MarginEnd = 5;
        hBox.Append(widget);

        //Включено / Виключено
        {
            Box vBoxSw = New(Orientation.Vertical, 0);
            hBox.Append(vBoxSw);

            Box hBoxSw = New(Orientation.Horizontal, 0);
            hBoxSw.Append(sw);
            vBoxSw.Append(hBoxSw);
        }

        FilterList.Append(new ListBoxRow() { Child = vBox });
    }
}