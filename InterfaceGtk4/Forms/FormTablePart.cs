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
/// Основа для класів:
///         ConstantTablePart (Константа таблична частина) 
///         DocumentFormTablePart (Документ таблична частина)
///         DirectoryFormTablePart (Довідник таблична частина)
/// </summary>
public abstract class FormTablePart : Form
{
    /// <summary>
    /// Елемент на який треба спозиціонувати список при обновленні
    /// </summary>
    protected uint SelectPosition { get; set; }

    /// <summary>
    /// Табличний список
    /// </summary>
    protected ColumnView Grid { get; } = ColumnView.New(null);

    /// <summary>
    /// Дані для табличного списку
    /// </summary>
    protected abstract Gio.ListStore Store { get; }

    /// <summary>
    /// Верхній набір меню
    /// </summary>
    protected Box HBoxToolbarTop { get; } = New(Orientation.Horizontal, 0);

    public FormTablePart(NotebookFunction? notebookFunc) : base(notebookFunc)
    {
        Append(HBoxToolbarTop);
        CreateToolbar();

        Grid.Reorderable = false;
        Grid.AccessibleRole = AccessibleRole.Table;

        Columns();

        ScrolledWindow scroll = ScrolledWindow.New();
        scroll.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
        scroll.Vexpand = true;
        scroll.SetChild(Grid);
        Append(scroll);
    }

    #region Virtual & Abstract Function

    protected abstract void Columns();
    public abstract ValueTask LoadRecords();
    public abstract ValueTask SaveRecords();
    public abstract bool NewRecord();

    #endregion

    #region Toolbar

    void CreateToolbar()
    {
        HBoxToolbarTop.MarginTop = HBoxToolbarTop.MarginBottom = 6;
        Append(HBoxToolbarTop);

        {
            Button button = Button.NewFromIconName("new");
            button.MarginEnd = 5;
            button.TooltipText = "Додати";
            button.OnClicked += OnAdd;
            HBoxToolbarTop.Append(button);
        }

        {
            Button button = Button.NewFromIconName("copy");
            button.MarginEnd = 5;
            button.TooltipText = "Копіювати";
            button.OnClicked += OnCopy;
            HBoxToolbarTop.Append(button);
        }

        {
            Button button = Button.NewFromIconName("delete");
            button.MarginEnd = 5;
            button.TooltipText = "Видалити";
            button.OnClicked += OnDelete;
            HBoxToolbarTop.Append(button);
        }
    }

    /// <summary>
    /// Прокрутка
    /// </summary>
    /// <param name="selectPosition"></param>
    protected void ScrollTo(uint selectPosition)
    {
        uint rowCount = Store.GetNItems();
        if (rowCount > 0 && Grid.Vadjustment != null)
        {
            //Видима частина
            double pageSize = Grid.Vadjustment.PageSize;

            //Максимальне значення
            double upper = Math.Round(Grid.Vadjustment.Upper);

            if (pageSize > 0 && upper > 0 && upper > pageSize)
            {
                //Висота одного рядка
                double rowHeidth = upper / rowCount;

                //Висота для потрібної позиції
                double value = rowHeidth * selectPosition;

                //Розмір половини видимої частини
                double pageSizePart = pageSize / 2;

                if (value > pageSizePart)
                    Task.Run(async () =>
                    {
                        //Вимушена затримка, щоб все промалювалося
                        await Task.Delay(100);

                        //Позиціювання потрібного рядка посередині
                        Grid.Vadjustment.SetValue(value - pageSizePart);
                    });
            }
        }
    }

    /// <summary>
    /// При виділенні елементів в таблиці
    /// </summary>
    protected void GridOnSelectionChanged(SelectionModel sender, SelectionModel.SelectionChangedSignalArgs args)
    {
        Bitset selection = Grid.Model.GetSelection();

        //Коли виділений один рядок
        if (selection.GetMinimum() == selection.GetMaximum())
            SelectPosition = selection.GetMaximum();
    }

    protected List<RowTablePart> GetSelection()
    {
        List<RowTablePart> rows = [];

        MultiSelection model = (MultiSelection)Grid.Model;
        Bitset selection = model.GetSelection();

        for (uint i = selection.GetMinimum(); i <= selection.GetMaximum(); i++)
            if (model.IsSelected(i) && model.GetObject(i) is RowTablePart row)
                rows.Add(row);

        return rows;
    }

    void OnAdd(Button button, EventArgs args)
    {
        if (NewRecord() && Store.GetNItems() > 0)
        {
            SelectPosition = Store.GetNItems() - 1;
            Grid.Model.SelectItem(SelectPosition, true);

            ScrollTo(SelectPosition);
        }
    }

    void OnCopy(Button button, EventArgs args)
    {
        List<RowTablePart> rows = GetSelection();
        if (rows.Count > 0)
        {
            SelectPosition = 0;
            foreach (RowTablePart row in rows)
            {
                Store.Append(row.Copy());
                SelectPosition = Store.GetNItems() - 1;
            }

            Grid.Model.SelectItem(SelectPosition, true);
            ScrollTo(SelectPosition);
        }
    }

    void OnDelete(Button button, EventArgs args)
    {
        MultiSelection model = (MultiSelection)Grid.Model;
        Bitset selection = model.GetSelection();

        SelectPosition = selection.GetMinimum();
        for (uint i = selection.GetMaximum(); i >= selection.GetMinimum(); i--)
            if (model.IsSelected(i)) Store.Remove(i);

        if (Store.GetNItems() > 0)
        {
            if (SelectPosition > Store.GetNItems() - 1)
                SelectPosition = Store.GetNItems() - 1;

            Grid.Model.SelectItem(SelectPosition, true);
        }
    }

    #endregion
}