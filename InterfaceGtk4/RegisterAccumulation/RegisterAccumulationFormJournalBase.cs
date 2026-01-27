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
using Gdk;
using AccountingSoftware;

namespace InterfaceGtk4;

/// <summary>
/// ДокументЖурналБазовий
/// 
/// Основа для класів:
/// 
/// </summary>
public abstract class RegisterAccumulationFormJournalBase : FormJournal
{
    /// <summary>
    /// Перевизначення сховища для нового типу даних 
    /// </summary>
    public override Gio.ListStore Store { get; } = Gio.ListStore.New(RegisterAccumulationRowJournal.GetGType());

    /// <summary>
    /// Верхній бокс для додаткових кнопок
    /// </summary>
    protected Box HBoxTop { get; } = New(Orientation.Horizontal, 0);

    /// <summary>
    /// Верхній набір меню
    /// </summary>
    protected Box HBoxToolbarTop { get; } = New(Orientation.Horizontal, 0);

    /// <summary>
    /// Період
    /// </summary>
    public PeriodControl Period { get; } = new();

    /// <summary>
    /// Фільтр
    /// </summary>
    public FilterControl Filter { get; } = new(true);

    public RegisterAccumulationFormJournalBase(NotebookFunction? notebookFunc) : base(notebookFunc)
    {
        //Кнопки
        HBoxTop.MarginBottom = 6;
        Append(HBoxTop);

        //Період
        Period.MarginEnd = 2;
        Period.Changed = async () =>
        {
            PagesClear();
            await LoadRecords();

            PeriodChanged();
        };
        HBoxTop.Append(Period);

        //Інформування про стан відборів
        TypeWhereStateInfo.MarginStart = 8;
        TypeWhereStateInfo.MarginEnd = 10;
        HBoxTop.Append(TypeWhereStateInfo);

        //Фільтр
        {
            Filter.MarginEnd = 2;
            Filter.Select = async () =>
            {
                TypeWhereState = TypeWhere.Filter;

                PagesClear();
                await LoadRecords();
            };
            Filter.Clear = async () =>
            {
                TypeWhereState = TypeWhere.Standart;
                WhereList = null;

                PagesClear();
                await LoadRecords();
            };
            Filter.FillFilterList = FillFilter;
        }

        CreateToolbar();
        GridModel();

        ScrollGrid.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
        ScrollGrid.SetChild(Grid);
        ScrollGrid.Vexpand = ScrollGrid.Hexpand = true;
        Append(ScrollGrid);

        ScrollPages.SetPolicy(PolicyType.Automatic, PolicyType.Never);
        ScrollPages.SetChild(HBoxPages);
        Append(ScrollPages);

        //Прокрутка до виділеного рядка
        Grid.Vadjustment?.OnChanged += (sender, args) =>
        {
            Bitset selection = Grid.Model.GetSelection();
            if (selection.GetSize() > 0) ScrollTo(selection.GetMaximum());
        };
    }

    protected override void GridModel()
    {
        //Модель
        MultiSelection model = MultiSelection.New(Store);
        model.OnSelectionChanged += GridOnSelectionChanged;

        Grid.Model = model;
    }

    public override async ValueTask SetValue()
    {
        DefaultGrabFocus();
        await BeforeSetValue();
    }

    #region Virtual & Abstract Function

    /// <summary>
    /// Заповнити поля для фільтру
    /// </summary>
    /// <param name="filterControl">Контрол Фільтр</param>
    protected virtual void FillFilter(FilterControl filterControl) { }

    /// <summary>
    /// При зміні періоду в контролі Period
    /// </summary>
    protected abstract void PeriodChanged();

    #endregion

    #region Toolbar

    void CreateToolbar()
    {
        HBoxToolbarTop.MarginBottom = 6;
        Append(HBoxToolbarTop);

        {
            Button button = Button.NewFromIconName("refresh");
            button.MarginEnd = 5;
            button.TooltipText = "Оновити";
            button.OnClicked += OnRefresh;
            HBoxToolbarTop.Append(button);
        }

        {
            Button button = Button.NewFromIconName("view-sort-descending");
            button.MarginEnd = 5;
            button.TooltipText = "Фільтр";
            button.OnClicked += OnFilter;
            HBoxToolbarTop.Append(button);
        }
    }

    async void OnRefresh(Button sender, EventArgs args)
    {
        await Refresh();
    }

    void OnFilter(Button button, EventArgs args)
    {
        if (!Filter.IsFilterCreated)
            Filter.CreatePopover(button);

        Filter.PopoverParent?.Show();
    }

    #endregion
}
