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
/// ДовідникЖурналБазовийДерево
/// 
/// Основа для класів:
///     DirectoryFormJournalFullTree (ДовідникЖурналПовнийДерево),
///     DirectoryFormJournalSmallTree (ДовідникЖурналМініДерево),
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
    public Action<UnigueID>? CallBack_Activate { get; set; }

    public DirectoryFormJournalBaseTree(NotebookFunction? notebookFunc) : base(notebookFunc)
    {
        GridModel();

        Grid.OnActivate += async (_, args) => await GridOnActivate(args.Position);
    }

    protected override void GridModel()
    {
        //Модель для дерева
        TreeList = TreeListModel.New(Store, false, false, CreateFunc);

        //Модель
        MultiSelection model = MultiSelection.New(TreeList);
        model.OnSelectionChanged += GridOnSelectionChanged;

        Grid.Model = model;
    }

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
            store.Ref();
            
            foreach (DirectoryHierarchicalRow subRow in itemRow.Sub)
                store.Append(subRow);
        }

        return store;
    }

    /// <summary>
    /// При активації
    /// </summary>
    /// <param name="position">Позиція</param>
    protected override async ValueTask GridOnActivate(uint position)
    {
        TreeListRow? row = TreeList?.GetRow(position);
        DirectoryHierarchicalRow? rowItem = (DirectoryHierarchicalRow?)row?.GetItem();
        if (rowItem != null)
        {
            if (DirectoryPointerItem == null)
                await OpenPageElement(false, rowItem.UnigueID);
            else
            {
                CallBack_OnSelectPointer?.Invoke(rowItem.UnigueID);

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
                SelectPointerItem = rowItem.UnigueID;
                CallBack_Activate?.Invoke(SelectPointerItem);
            }

            /*nint handle = model.GetItem(selection.GetMaximum());
            if (handle != nint.Zero)
            {
                TreeListRow treeListRow = new(new Gtk.Internal.TreeListRowHandle(handle, false));
                DirectoryHierarchicalRow? row = (DirectoryHierarchicalRow?)treeListRow.GetItem();
                if (row != null) SelectPointerItem = row.UnigueID;
            }*/
        }
    }

    /// <summary>
    /// Функція викликається після завантаження даних в Store (в кінці LoadRecords) і передає UnigueID
    /// </summary>
    /// <param name="select">UnigueID елемента який треба виділити</param>
    public override void AfterLoadRecords(UnigueID? select = null)
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

            bool RecursionFind(TreeListRow row, uint position)
            {
                DirectoryHierarchicalRow? rowItem = (DirectoryHierarchicalRow?)row.GetItem();
                if (rowItem != null && rowItem.UnigueID.Equals(select))
                {
                    Grid.Model.SelectItem(position, false);

                    //Прокрутка
                    ScrollTo(position);

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

}
