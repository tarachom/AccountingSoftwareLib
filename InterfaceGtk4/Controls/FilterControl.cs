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
    /// Таблиця для фільтрів
    /// </summary>
    Grid FilterGrid = Grid.New();

    /// <summary>
    /// Галочка яка вказує на те що потрібно враховувати період у фільтрі
    /// </summary>
    CheckButton UsePeriod { get; set; } = CheckButton.NewWithLabel("В межах періоду");

    public FilterControl(bool usePeriod = false)
    {
        SetOrientation(Orientation.Vertical);

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

            FilterGrid.Hexpand = true;
            FilterGrid.ColumnSpacing = FilterGrid.RowSpacing = 10;

            hBox.Append(FilterGrid);
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
                if (GridCountRows > 0)
                {
                    bool existFilter = GetWhere?.Invoke() ?? false;
                    if (existFilter)
                        Select?.Invoke();

                    PopoverParent?.Hide();
                }
            };
            hBox.Append(buttonFilter);

            Button buttonClear = Button.NewWithLabel("Скинути");
            buttonClear.OnClicked += (_, _) =>
            {
                //Є хоть один елемент в списку
                if (GridCountRows > 0)
                {
                    Clear?.Invoke();
                    PopoverParent?.Hide();
                }
            };
            hBox.Append(buttonClear);
        }
    }

    /// <summary>
    /// Чи враховувати період для відборів (В межах періоду)
    /// </summary>
    public bool IsUsePeriod { get => UsePeriod.Active; }

    /// <summary>
    /// Створене спливаюче вікно
    /// </summary>
    public bool IsFilterCreated { get; private set; } = false;

    /// <summary>
    /// Вспливаюче вікно власник
    /// </summary>
    public Popover? PopoverParent { get; private set; }

    /// <summary>
    /// Кількість рядків в таблиці
    /// </summary>
    int GridCountRows = 0;

    /// <summary>
    /// Колонки для таблиці фільтрів
    /// </summary>
    enum GridColumn
    {
        Name,
        Widget,
        Sw
    }

    /// <summary>
    /// Створити спливаюче вікно фільтру
    /// </summary>
    /// <param name="relative_to">Привязка</param>
    public void CreatePopover(Button button)
    {
        if (!IsFilterCreated)
        {
            PopoverParent = Popover.New();
            PopoverParent.SetParent(button);
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
        Label label = Label.New(caption);
        label.Halign = Align.End;
        FilterGrid.Attach(label, (int)GridColumn.Name, GridCountRows, 1, 1);

        //widget.Hexpand = true;
        FilterGrid.Attach(widget, (int)GridColumn.Widget, GridCountRows, 1, 1);

        //Включено / Виключено
        {
            Box vBox = New(Orientation.Vertical, 0);
            Box hBox = New(Orientation.Horizontal, 0);
            hBox.Vexpand = hBox.Hexpand = true;
            hBox.Valign = Align.Center;
            hBox.Append(sw);
            vBox.Append(hBox);

            FilterGrid.Attach(vBox, (int)GridColumn.Sw, GridCountRows, 1, 1);
        }

        GridCountRows++;
    }
}