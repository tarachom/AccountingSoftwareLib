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
[GObject.Subclass<DirectoryFormJournalBase>]
public partial class DirectoryFormJournalBaseTree : DirectoryFormJournalBase
{
    /// <summary>
    /// Перевизначення сховища для нового типу даних
    /// </summary>
    public override Gio.ListStore Store { get; } = Gio.ListStore.New(DirectoryHierarchicalRow.GetGType());

    /// <summary>
    /// Модель для дерева
    /// </summary>
    TreeListModel? TreeList { get; set; } = null;

    /// <summary>
    /// Функція вибору рядка в дереві
    /// </summary>
    public Action<UniqueID>? CallBack_Activate { get; set; } = null;

    partial void Initialize()
    {
        if (GetType().Namespace == "InterfaceGtk4") return;

        GridModel();
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

        PopoverParent?.OnHide += (_, _) => GC.Collect();
    }

    /// <summary>
    /// Функція заповнення дерева
    /// </summary>
    /// <param name="item">Поточний об'єкт для якого викликається функція</param>
    /// <returns></returns>
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
                            List<DirectoryHierarchicalRow> list = [];
                            if (TreeCache.TryGetValue(itemRow.UniqueID, out List<DirectoryHierarchicalRow>? cacheList))
                                list = cacheList;
                            else
                            {
                                list = await LoadChildren(itemRow.UniqueID);
                                TreeCache.Add(itemRow.UniqueID, list);
                            }

                            //Видалення
                            itemRow.Store.RemoveAll();

                            //Заповнення
                            if (list.Count > 0)
                                foreach (var item in list)
                                    itemRow.Store.Append(item);
                            else
                            {
                                itemRow.Store.Dispose();
                                itemRow.Store = null;
                            }
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
            /* Вітка для завантаження */
            if (!itemRow.UniqueID.IsEmpty() && //Не порожній вказівник
                (
                    itemRow.AllowedContent == ConfigurationDirectories.HierarchicalContentType.Elements || //Елементи
                    itemRow.AllowedContent == ConfigurationDirectories.HierarchicalContentType.Folders ||  //Папки
                    (itemRow.AllowedContent == ConfigurationDirectories.HierarchicalContentType.FoldersAndElements && itemRow.IsFolder)) //Папки та елементи і це папка
                )
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

    /// <summary>
    /// 
    /// </summary>
    protected Dictionary<UniqueID, List<DirectoryHierarchicalRow>> TreeCache { get; set; } = [];

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
                await OpenPageElement(false, rowItem.UniqueID);
            else
                if (AllowedContentSelection == null || //Вибір папок і елементів
                    AllowedContentSelection == ConfigurationDirectories.HierarchicalContentType.FoldersAndElements || //Вибір папок і елементів
                    AllowedContentSelection == ConfigurationDirectories.HierarchicalContentType.Folders && rowItem.IsFolder || //Вибір елементів
                    AllowedContentSelection == ConfigurationDirectories.HierarchicalContentType.Elements && !rowItem.IsFolder) //Вибір папок
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

    public override async ValueTask AfterLoadRecords(List<UniqueID> parents, UniqueID? select = null)
    {
        #region Local Func

        //Для пошуку верхнього рівня
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
            //Підвантаження в кеш
            foreach (var parent in parents)
            {
                //Для вибраного елементу не потрібно підвантажувати
                if (parent.Equals(select))
                    break;

                if (!TreeCache.ContainsKey(parent))
                {
                    List<DirectoryHierarchicalRow> list = await LoadChildren(parent);
                    if (list.Count > 0)
                        TreeCache.Add(parent, list);
                }
            }

            uint position = 0;
            int counter = 0;
            TreeListRow? currentRow = null;

            //Відкриття віток
            foreach (var parent in parents)
            {
                if (counter == 0)
                {
                    (currentRow, position) = FindTop(parent);

                    //Якщо верхня вітка не знайдено - продовжити крутити цикл
                    if (currentRow == null) continue;
                }
                else
                    if (currentRow != null)
                    {
                        await Task.Delay(10);
                        (currentRow, uint pos) = FindChild(currentRow, parent);
                        if (currentRow != null)
                            position += pos;
                        else
                            break;
                    }
                    else
                        break;

                counter++;
            }

            Grid.Model.SelectItem(position, false);

            //Функція яка викликається після повного завантаження
            GLib.Functions.IdleAdd(GLib.Constants.PRIORITY_LOW, () =>
            {
                Console.WriteLine(position);
                ScrollTo(position);
                return false;
            });
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

                //Максимальне значення
                double upper = Math.Round(Grid.Vadjustment.Upper);
                if (pageSize > 0 && upper > 0 && upper >= pageSize)
                {
                    //Висота одного рядка
                    double rowHeidth = upper / rowCount;

                    //Висота для потрібної позиції
                    double value = rowHeidth * selectPosition;

                    //Розмір половини видимої частини
                    double pageSizePart = pageSize / 2;

                    if (value > pageSizePart)
                        Grid.Vadjustment.SetValue(value - pageSizePart);
                }
            }
        }
    }
}