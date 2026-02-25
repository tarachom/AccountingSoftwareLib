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
using AccountingSoftware;

namespace InterfaceGtk4;

/// <summary>
/// ДовідникФормаЖурналБазовийДерево
/// 
/// Основа для класів:
///     DirectoryFormJournalFullTree (ДовідникФормаЖурналПовнийДерево),
///     DirectoryFormJournalSmallTree (ДовідникФормаЖурналМініДерево),
/// </summary>
public abstract class DirectoryFormJournalBaseTree : DirectoryFormJournalBase
{
    /// <summary>
    /// Перевизначення сховища для нового типу даних
    /// </summary>
    public override Gio.ListStore Store { get; } = Gio.ListStore.New(DirectoryHierarchicalRow.GetGType());

    /// <summary>
    /// Модель для дерева
    /// </summary>
    TreeListModel? TreeList { get; set; }

    /// <summary>
    /// Функція вибору рядка в дереві
    /// </summary>
    public Action<UniqueID>? CallBack_Activate { get; set; }

    public DirectoryFormJournalBaseTree(NotebookFunction? notebookFunc) : base(notebookFunc)
    {
        GridModel();

        Grid.OnActivate += async (_, args) => await GridOnActivate(args.Position);
    }

    /// <summary>
    /// Встановлення моделі
    /// </summary>
    protected override void GridModel()
    {
        //Модель для дерева
        TreeList = TreeListModel.New(Store, false, false, CreateFunc);

        //Модель
        MultiSelection model = MultiSelection.New(TreeList);
        model.OnSelectionChanged += GridOnSelectionChanged;

        Grid.Model = model;
    }

    /// <summary>
    /// Перша функція яка викликається після створення форми
    /// </summary>
    public override async ValueTask SetValue()
    {
        DefaultGrabFocus();
        await BeforeSetValue();

        await LoadRecords();
        //RunUpdateRecords();
    }

    /// <summary>
    /// Функція заповнення дерева
    /// </summary>
    /// <param name="item">Поточний об'єкт для якого викликається функція</param>
    /// <returns></returns>
    private static Gio.ListModel? CreateFunc(GObject.Object item)
    {
        var itemRow = (DirectoryHierarchicalRow)item;
        Gio.ListStore? store = null;

        if (itemRow.Sub.Count > 0)
        {
            store = Gio.ListStore.New(DirectoryHierarchicalRow.GetGType());
            _ = store.Ref();

            foreach (DirectoryHierarchicalRow subRow in itemRow.Sub)
                store.Append(subRow);
        }

        return store;
    }

    long LastTicksActivate = 0; //!!! Тимчасове рішення

    /// <summary>
    /// При активації
    /// </summary>
    /// <param name="position">Позиція</param>
    protected override async ValueTask GridOnActivate(uint position)
    {
        //!!! Тимчасове рішення, бо відбувається двойна активація.
        //Можливо це повязано із глюком дерева яке мають виправити у версії 0.8.0
        {
            var ticks = DateTime.Now.Ticks;
            var exit = false;
            if (ticks - LastTicksActivate <= TimeSpan.TicksPerSecond) exit = true;
            LastTicksActivate = ticks;
            if (exit) return;
        }

        TreeListRow? row = TreeList?.GetRow(position);
        DirectoryHierarchicalRow? rowItem = (DirectoryHierarchicalRow?)row?.GetItem();
        if (rowItem != null)
        {
            if (DirectoryPointerItem == null)
                await OpenPageElement(false, rowItem.UniqueID);
            else
            {
                CallBack_OnSelectPointer?.Invoke(rowItem.UniqueID);

                NotebookFunc?.ClosePage(GetName());
                PopoverParent?.Hide();
            }
        }
    }

    /// <summary>
    /// Функція повертає список вибраних елементів
    /// </summary>
    /// <returns>Список вибраних елементів якщо є вибрані, або пустий список</returns>
    public override List<RowJournal> GetSelection()
    {
        List<RowJournal> rows = [];

        MultiSelection model = (MultiSelection)Grid.Model;
        Bitset selection = model.GetSelection();

        for (uint i = selection.GetMinimum(); i <= selection.GetMaximum(); i++)
            if (model.IsSelected(i))
            {
                TreeListRow? row = TreeList?.GetRow(i);
                DirectoryHierarchicalRow? rowItem = (DirectoryHierarchicalRow?)row?.GetItem();
                if (rowItem != null) rows.Add(rowItem);
            }

        return rows;
    }

    /// <summary>
    /// При виділенні елементів в таблиці
    /// </summary>
    protected override void GridOnSelectionChanged(SelectionModel sender, SelectionModel.SelectionChangedSignalArgs args)
    {
        MultiSelection model = (MultiSelection)Grid.Model;
        Bitset selection = model.GetSelection();

        //Коли виділений один рядок
        if (selection.GetMinimum() == selection.GetMaximum())
        {
            TreeListRow? row = TreeList?.GetRow(selection.GetMaximum());
            DirectoryHierarchicalRow? rowItem = (DirectoryHierarchicalRow?)row?.GetItem();
            if (rowItem != null)
            {
                SelectPointerItem = rowItem.UniqueID;
                CallBack_Activate?.Invoke(SelectPointerItem);
            }

            /*nint handle = model.GetItem(selection.GetMaximum());
            if (handle != nint.Zero)
            {
                TreeListRow treeListRow = new(new Gtk.Internal.TreeListRowHandle(handle, false));
                DirectoryHierarchicalRow? row = (DirectoryHierarchicalRow?)treeListRow.GetItem();
                if (row != null) SelectPointerItem = row.UniqueID;
            }*/
        }
    }

    /// <summary>
    /// Функція викликається після завантаження даних в Store (в кінці LoadRecords) і передає UniqueID
    /// </summary>
    /// <param name="select">UniqueID елемента який треба виділити</param>
    public override void AfterLoadRecords(UniqueID? select = null)
    {
        if (PopoverParent == null)
            NotebookFunc?.SpinnerOff(GetName());

        //Якщо є перший пустий рядок і нічого не вибрано тоді зразу його виділяю
        if (InsertEmptyFirstRow && select == null)
        {
            Grid.Model.SelectItem(0, false);
            return;
        }

        if (TreeList != null && select != null)
        {
            /*
            Принцип роботи:
                Рекусивно обходяться всі вітки дерева і якщо знайдений
                елемент то обробка припиняється і вітки залишаються відкритими.
                Вітки де не знайдено закриваються після обробки.
            */
            for (uint i = 0; i < TreeList.GetNItems(); i++)
            {
                TreeListRow? row = TreeList.GetRow(i);
                if (row != null)
                {
                    row.Expanded = true;
                    if (RecursionFind(row, i))
                        break;
                    else
                        row.Expanded = false;
                }
            }

            uint CountNItems(TreeListRow? row)
            {
                if (row != null)
                {
                    uint count = row.GetChildren()?.GetNItems() ?? 0;
                    return count + CountNItems(row.GetParent());
                }
                else
                    return 0;
            }

            bool RecursionFind(TreeListRow row, uint position)
            {
                DirectoryHierarchicalRow? rowItem = (DirectoryHierarchicalRow?)row.GetItem();
                if (rowItem != null && rowItem.UniqueID.Equals(select))
                {
                    Grid.Model.SelectItem(position, false);

                    //Це для того щоб активувати подію Grid.Vadjustment?.OnChanged
                    ScrollToEnable = true;
                    Grid.Vadjustment?.Upper += 0.1;

                    return true;
                }

                Gio.ListModel? children = row.GetChildren();
                for (uint j = 0; j < children?.GetNItems(); ++j)
                {
                    TreeListRow? childRow = row.GetChildRow(j);
                    if (childRow != null)
                    {
                        childRow.Expanded = true;
                        if (RecursionFind(childRow, ++position))
                            return true;
                        else
                            childRow.Expanded = false;
                    }
                }

                return false;
            }
        }
    }

    /// <summary>
    /// Переоприділення функції прокрутки
    /// </summary>
    /// <param name="selectPosition">Позиція</param>
    protected override void ScrollTo(uint selectPosition)
    {
        if (TreeList != null)
        {
            uint rowCount = TreeList.GetNItems();
            if (rowCount > 0 && Grid.Vadjustment != null)
            {
                //Видима частина
                double pageSize = Grid.Vadjustment.PageSize;
                //Console.WriteLine($"pageSize {pageSize}");

                //Максимальне значення
                double upper = Math.Round(Grid.Vadjustment.Upper);
                //Console.WriteLine($"upper {upper}");

                if (pageSize > 0 && upper > 0 && upper > pageSize)
                {
                    //Висота одного рядка
                    double rowHeidth = upper / rowCount;
                    //Console.WriteLine($"rowHeidth {rowHeidth}");

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

                            //Закрити дозвіл на прокрутку до виділеного рядка
                            ScrollToEnable = false;
                        });
                }
            }
        }
    }
}
