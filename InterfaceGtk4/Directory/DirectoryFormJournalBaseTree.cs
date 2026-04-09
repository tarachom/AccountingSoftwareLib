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
        PopoverParent?.OnHide += (_, _) => GC.Collect(); //!!! Помістити це в RunUpdateRecords
    }

    /// <summary>
    /// Функція заповнення дерева
    /// </summary>
    /// <param name="item">Поточний об'єкт для якого викликається функція</param>
    /// <returns></returns>
    /*private Gio.ListModel? CreateFunc(GObject.Object item)
    {
        DirectoryHierarchicalRow itemRow = (DirectoryHierarchicalRow)item;
        Gio.ListStore? store = null;

        if (itemRow.Sub.Count > 0)
        {
            store = Gio.ListStore.New(DirectoryHierarchicalRow.GetGType());

            foreach (DirectoryHierarchicalRow subRow in itemRow.Sub)
                store.Append(subRow);
        }

        return store;
    }*/

    private Gio.ListModel? CreateFunc(GObject.Object item)
    {
        DirectoryHierarchicalRow itemRow = (DirectoryHierarchicalRow)item;
        Gio.ListStore? store = null;

        //Якщо це вітка для завантаження
        if (itemRow.IsLoading)
        {
            if (itemRow.Store != null)
            {
                GLib.Functions.IdleAdd(0, () =>
                {
                    async void f()
                    {
                        try
                        {
                            //Видалення
                            itemRow.Store.RemoveAll();

                            List<DirectoryHierarchicalRow> list = await LoadChildren(itemRow.UniqueID);

                            //Заповнення
                            foreach (var item in list)
                                itemRow.Store.Append(item);

                            Console.WriteLine("Заповнено");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }

                    f();
                    return false;
                });
            }
        }
        else
        {
            if (!itemRow.UniqueID.IsEmpty())
                if (itemRow.AllowedContent == ConfigurationDirectories.HierarchicalContentType.Elements ||
                    itemRow.AllowedContent == ConfigurationDirectories.HierarchicalContentType.Folders ||
                    (itemRow.AllowedContent == ConfigurationDirectories.HierarchicalContentType.FoldersAndElements && itemRow.IsFolder))
                {
                    store = Gio.ListStore.New(DirectoryHierarchicalRow.GetGType());

                    //Додається одна пуста вітка для завантаження
                    DirectoryHierarchicalRow itemEmpty = LoadEmptyChildren();
                    itemEmpty.UniqueID = itemRow.UniqueID;
                    itemEmpty.IsLoading = true;
                    itemEmpty.Store = store;

                    store.Append(itemEmpty);
                }
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
    public override List<IRowSubclassJournal> GetSelection()
    {
        List<IRowSubclassJournal> rows = [];

        MultiSelection model = (MultiSelection)Grid.Model;
        Bitset selection = model.GetSelection();

        for (uint i = selection.GetMinimum(); i <= selection.GetMaximum(); i++)
            if (model.IsSelected(i))
            {
                TreeListRow? row = TreeList?.GetRow(i);
                IRowSubclassJournal? rowItem = (IRowSubclassJournal?)row?.GetItem();
                if (rowItem != null) rows.Add(rowItem);
            }

        return rows;
    }

    /// <summary>
    /// При виділенні елементів в таблиці
    /// </summary>
    protected override void GridOnSelectionChanged(SelectionModel sender, SelectionModel.SelectionChangedSignalArgs args)
    {
        Console.WriteLine("GridOnSelectionChanged");

        /*
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
            }*//*
        }*/
    }

    public override void AfterLoadRecords(Stack<UniqueID> parents)
    {
        #region Local Func

        (TreeListRow? Row, uint Position) FindTop(UniqueID select)
        {
            for (uint i = 0; i < TreeList.GetNItems(); i++)
            {
                TreeListRow? row = TreeList.GetRow(i);
                if (row != null)
                {
                    DirectoryHierarchicalRow? rowItem = (DirectoryHierarchicalRow?)row.GetItem();
                    if (rowItem != null && rowItem.UniqueID.Equals(select))
                    {
                        row.Expanded = true;
                        return (row, i);
                    }
                }
            }

            return (null, 0);
        }

        (TreeListRow? Row, uint Position) FindChild(TreeListRow? treeListRow, UniqueID select)
        {
            Gio.ListModel? children = treeListRow?.GetChildren();
            for (uint j = 0; j < children?.GetNItems(); ++j)
            {
                TreeListRow? row = treeListRow?.GetChildRow(j);
                if (row != null)
                {
                    DirectoryHierarchicalRow? rowItem = (DirectoryHierarchicalRow?)row.GetItem();
                    if (rowItem != null && rowItem.UniqueID.Equals(select))
                    {
                        row.Expanded = true;
                        return (row, j + 1);
                    }
                }
            }

            return (null, 0);
        }

        #endregion

        if (PopoverParent == null)
            NotebookFunc?.SpinnerOff(GetName());

        //Якщо є перший пустий рядок і нічого не вибрано тоді зразу його виділяю
        if (InsertEmptyFirstRow && parents.Count == 0)
        {
            Grid.Model.SelectItem(0, false);
            return;
        }

        if (TreeList != null && parents.Count > 0)
        {
            bool isTopLevel = true;
            TreeListRow? currentRow = null;
            uint position = 0;

            while (parents.Count > 0)
            {
                UniqueID select = parents.Pop();

                if (isTopLevel)
                {
                    (currentRow, position) = FindTop(select);
                    if (currentRow != null)
                        isTopLevel = false;
                    else
                        break;
                }
                else if (currentRow != null)
                {
                    (currentRow, uint position_) = FindChild(currentRow, select);
                    if (currentRow != null)
                        position += position_;
                    else
                        break;
                }
            }

            Grid.Model.SelectItem(position, false);

            //Функція яка викликається після повного завантаження
            GLib.Functions.IdleAdd(GLib.Constants.PRIORITY_LOW, () =>
            {
                ScrollTo(position);
                return false;
            });
        }
    }

    /// <summary>
    /// Функція викликається після завантаження даних в Store (в кінці LoadRecords) і передає UniqueID
    /// </summary>
    /// <param name="select">UniqueID елемента який треба виділити</param>
    /*
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

        {
            //Store.FindWithEqualFunc
        }

        if (TreeList != null && select != null && !select.IsEmpty())
        {
            
            //Принцип роботи:
            //    Рекусивно обходяться всі вітки дерева і якщо знайдений
            //    елемент то обробка припиняється і вітки залишаються відкритими.
            //    Вітки де не знайдено закриваються після обробки.
            
            for (uint i = 0; i < TreeList.GetNItems(); i++)
            {
                TreeListRow? row = TreeList.GetRow(i);
                if (row != null)
                {
                    DirectoryHierarchicalRow? rowItem = (DirectoryHierarchicalRow?)row.GetItem();
                    if (rowItem != null && rowItem.IsFolder)
                    {
                        //Console.WriteLine(rowItem.Fields["Назва"]);
                        row.Expanded = true;
                        if (RecursionFind(row, i))
                        {
                            //Console.WriteLine($"Break recursion {i}");
                            break;
                        }
                        else
                            row.Expanded = false;
                    }
                }
            }

            //uint CountNItems(TreeListRow? row)
            //{
            //    if (row != null)
            //    {
            //        uint count = row.GetChildren()?.GetNItems() ?? 0;
            //        return count + CountNItems(row.GetParent());
            //    }
            //    else
            //        return 0;
            //}

            bool RecursionFind(TreeListRow row, uint position)
            {
                //Console.WriteLine($"Start recursion {position}");

                DirectoryHierarchicalRow? rowItem = (DirectoryHierarchicalRow?)row.GetItem();
                if (rowItem != null && rowItem.UniqueID.Equals(select))
                {
                    Grid.Model.SelectItem(position, false);
                    ScrollToEnable = true;

                    //Console.WriteLine($"Exit recursion {position}");
                    return true;
                }

                Gio.ListModel? children = row.GetChildren();
                for (uint j = 0; j < children?.GetNItems(); ++j)
                {
                    TreeListRow? childRow = row.GetChildRow(j);
                    if (childRow != null)
                    {
                        DirectoryHierarchicalRow? rowItem2 = (DirectoryHierarchicalRow?)row.GetItem();
                        if (rowItem2 != null && rowItem2.IsFolder)
                        {
                            //Console.WriteLine(rowItem2.Fields["Назва"]);
                            childRow.Expanded = true;
                            if (RecursionFind(childRow, ++position))
                            {
                                //Console.WriteLine($"Exit recursion {position}");
                                return true;
                            }
                            else
                                childRow.Expanded = false;
                        }
                    }
                }

                return false;
            }
        }
    }*/

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
                if (pageSize > 0 && upper > 0 && upper >= pageSize)
                {
                    //Висота одного рядка
                    double rowHeidth = upper / rowCount;
                    //Console.WriteLine($"rowHeidth {rowHeidth}");

                    //Висота для потрібної позиції
                    double value = rowHeidth * selectPosition;
                    //Console.WriteLine($"value {value}");

                    //Розмір половини видимої частини
                    double pageSizePart = pageSize / 2;
                    //Console.WriteLine($"pageSizePart {pageSizePart}");

                    if (value > pageSizePart)
                        Grid.Vadjustment.SetValue(value - pageSizePart);
                }
            }
        }
    }
}
