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

using Gtk;

namespace InterfaceGtk3;

/// <summary>
/// Основа для класів:
///         КонстантаТабличнаЧастина
///         ДовідникТабличнаЧастина, 
///         ДокументТабличнаЧастина
/// </summary>
public abstract class ФормаТабличнаЧастина : Форма
{
    /// <summary>
    /// Верхній набір меню
    /// </summary>
    protected Toolbar ToolbarTop = new Toolbar();

    /// <summary>
    /// Дерево
    /// </summary>
    protected TreeView TreeViewGrid = new TreeView();

    /// <summary>
    /// Прокрутка дерева
    /// </summary>
    ScrolledWindow ScrollTree;

    /// <summary>
    /// Виділений рядок
    /// Використовується коли треба спозиціонувати список при обновленні
    /// </summary>
    TreePath? SelectTreePath = null;

    public ФормаТабличнаЧастина()
    {
        CreateToolbar();

        ScrollTree = new ScrolledWindow() { ShadowType = ShadowType.In };
        ScrollTree.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);

        TreeViewGrid.Selection.Mode = SelectionMode.Multiple;
        TreeViewGrid.ActivateOnSingleClick = true;
        TreeViewGrid.RowActivated += OnRowActivated;
        TreeViewGrid.ButtonPressEvent += OnButtonPressEvent;
        TreeViewGrid.ButtonReleaseEvent += OnButtonReleaseEvent;
        TreeViewGrid.KeyReleaseEvent += OnKeyReleaseEvent;
        TreeViewGrid.EnableGridLines = TreeViewGridLines.Both;

        ScrollTree.Add(TreeViewGrid);
        PackStart(ScrollTree, true, true, 0);

        ShowAll();
    }

    void CreateToolbar()
    {
        PackStart(ToolbarTop, false, false, 0);

        ToolButton addButton = new ToolButton(new Image(Stock.Add, IconSize.Menu), "Додати") { TooltipText = "Додати" };
        addButton.Clicked += OnAddClick;
        ToolbarTop.Add(addButton);

        ToolButton copyButton = new ToolButton(new Image(Stock.Copy, IconSize.Menu), "Копіювати") { TooltipText = "Копіювати" };
        copyButton.Clicked += OnCopyClick;
        ToolbarTop.Add(copyButton);

        ToolButton deleteButton = new ToolButton(new Image(Stock.Delete, IconSize.Menu), "Видалити") { TooltipText = "Видалити" };
        deleteButton.Clicked += OnDeleteClick;
        ToolbarTop.Add(deleteButton);
    }

    #region Func

    /// <summary>
    /// Функція повертає набір параметрів для вибраної ячейки табличної частини
    /// після кліку або подвійного кліку мишкою
    /// </summary>
    /// <returns>Кортеж параметрів</returns>
    (TreePath Path, TreeViewColumn Column, TreeIter Iter, int RowNumber, int ColNumber)? GetCellInfo()
    {
        if (TreeViewGrid.Selection.CountSelectedRows() != 0)
        {
            TreeViewGrid.GetCursor(out TreePath itemPath, out TreeViewColumn treeColumn);

            //Для кожної колонки табличної частини має бути 
            //додатковий параметр який вказує на номер колонки
            if (GetColIndex(treeColumn, out int colNumber))
            {
                TreeViewGrid.Model.GetIter(out TreeIter iter, itemPath);
                int rowNumber = int.Parse(itemPath.ToString());

                return (itemPath, treeColumn, iter, rowNumber, colNumber);
            }
        }

        return null;
    }

    /// <summary>
    /// Функція відкриває вибір у спливаючому вікні для довідника або документу.
    /// 
    /// Добавилась можливість відкривати довільну форму в спливаючому вікні якщо не оприділений вибір з довідника чи документа,
    /// корисно наприклад для перегляду великого тексту чи вибору дати
    /// </summary>
    async void Select(TreePath path, TreeViewColumn column, TreeIter iter, int rowNumber, int colNumber)
    {
        //Прив'язка до ячейки
        Gdk.Rectangle rectangleCell = TreeViewGrid.GetCellArea(path, column);
        rectangleCell.Offset(-(int)ScrollTree.Hadjustment.Value, rectangleCell.Height);

        //Форма для вибору
        ФормаЖурнал? page = OpenSelect(iter, rowNumber, colNumber);
        if (page != null)
        {
            //Спливаюче вікно
            page.PopoverParent = new Popover(TreeViewGrid)
            {
                PointingTo = rectangleCell,
                Position = PositionType.Bottom,
                BorderWidth = 2
            };

            // if (page is ДовідникШвидкийВибір)
            // {
            // Зарезервовано на майбутнє
            // }
            // else
            if (page is ДокументЖурнал журнал)
            {
                page.PopoverParent.HeightRequest = 400;

                //Окремий ключ для налаштувань журналу
                журнал.KeyForSetting = ".SmallSelect";
            }

            page.PopoverParent.Add(page);
            page.PopoverParent.ShowAll();

            await page.SetValue(); //Заповнення даними
        }
        else
        {
            ФормаСпливаючеВікно? formPopover = OpenForm(iter, rowNumber, colNumber);
            if (formPopover != null)
            {
                //Спливаюче вікно
                formPopover.PopoverParent = new Popover(TreeViewGrid)
                {
                    PointingTo = rectangleCell,
                    Position = PositionType.Bottom,
                    BorderWidth = 2
                };

                formPopover.PopoverParent.Add(formPopover);
                formPopover.PopoverParent.ShowAll();
            }
        }
    }

    /// <summary>
    /// Спливаюче меню для правої кнопки миші для ячейки
    /// </summary>
    void OpenMenu(TreePath path, TreeViewColumn column, TreeIter iter, int rowNumber, int colNumber)
    {
        Menu menu = new Menu();

        MenuItem caption = new MenuItem("[ " + column.Title + " ]");
        menu.Append(caption);

        MenuItem select = new MenuItem("Вибрати");
        select.Activated += (sender, args) => Select(path, column, iter, rowNumber, colNumber);
        menu.Append(select);

        MenuItem copy = new MenuItem("Копіювати");
        copy.Activated += (sender, args) => CopyRecord(rowNumber);
        menu.Append(copy);

        MenuItem clear = new MenuItem("Очистити");
        clear.Activated += (sender, args) => ClearCell(iter, rowNumber, colNumber);
        menu.Append(clear);

        MenuItem delete = new MenuItem("Видалити");
        delete.Activated += (sender, args) => DeleteRecord(iter, rowNumber);
        menu.Append(delete);

        menu.ShowAll();
        menu.Popup();
    }

    #endregion

    #region Редагування ячейки

    bool IsEditingCell = false;

    protected void EditingStarted(object? sender, EditingStartedArgs args) => IsEditingCell = true;

    /// <summary>
    /// Текст
    /// </summary>
    protected void EditCell(object sender, EditedArgs args)
    {
        var cellInfoObj = GetCellInfo();
        if (cellInfoObj.HasValue)
        {
            var cellInfo = cellInfoObj.Value;
            ChangeCell(cellInfo.Iter, cellInfo.RowNumber, cellInfo.ColNumber, args.NewText);
        }

        IsEditingCell = false;
    }

    /// <summary>
    /// Переключатель
    /// </summary>
    protected void EditCell(object sender, ToggledArgs args)
    {
        var cellInfoObj = GetCellInfo();
        if (cellInfoObj.HasValue)
        {
            var cellInfo = cellInfoObj.Value;
            bool newValue = (bool)TreeViewGrid.Model.GetValue(cellInfo.Iter, cellInfo.ColNumber);
            ChangeCell(cellInfo.Iter, cellInfo.RowNumber, cellInfo.ColNumber, !newValue);
        }

        IsEditingCell = false;
    }

    #endregion

    #region Задання/Зчитування індексу для колонки

    protected void SetColIndex(TreeViewColumn column, Enum index)
    {
        column.Data.Add("Column", index);
    }

    protected void SetColIndex(TreeViewColumn column, int index)
    {
        column.Data.Add("Column", index);
    }

    protected bool GetColIndex(TreeViewColumn column, out int colNumber)
    {
        object? objColumn = column.Data["Column"];
        if (objColumn != null && (objColumn is Enum || objColumn is int))
        {
            colNumber = (int)objColumn;
            return true;
        }
        else
        {
            colNumber = 0;
            return false;
        }
    }

    #endregion

    #region Virtual & Abstract 

    public abstract ValueTask LoadRecords();
    public virtual async ValueTask SaveRecords() { await ValueTask.FromResult(true); }
    protected virtual ФормаЖурнал? OpenSelect(TreeIter iter, int rowNumber, int colNumber) { return null; }
    protected virtual ФормаСпливаючеВікно? OpenForm(TreeIter iter, int rowNumber, int colNumber) { return null; }
    protected virtual void ClearCell(TreeIter iter, int rowNumber, int colNumber) { }
    protected virtual void AddRecord() { }
    protected virtual void CopyRecord(int rowNumber) { }
    protected virtual void DeleteRecord(TreeIter iter, int rowNumber) { }
    protected virtual void ChangeCell(TreeIter iter, int rowNumber, int colNumber, string newText) { }
    protected virtual void ChangeCell(TreeIter iter, int rowNumber, int colNumber, bool newValue) { }

    #endregion

    #region TreeView

    /// <summary>
    /// Функція позиціонує список на рядок який раніше був активований OnRowActivated
    /// </summary>
    protected void SelectRowActivated()
    {
        if (SelectTreePath != null)
            TreeViewGrid.SetCursor(SelectTreePath, TreeViewGrid.Columns[0], false);
    }

    /// <summary>
    /// Звичайний клік
    /// </summary>
    void OnRowActivated(object? sender, RowActivatedArgs args)
    {
        var cellInfoObj = GetCellInfo();
        if (cellInfoObj.HasValue)
        {
            var cellInfo = cellInfoObj.Value;
            SelectTreePath = cellInfo.Path;
        }
        else
            SelectTreePath = null;
    }

    /// <summary>
    /// Подвійний клік лівою кнопкою миші
    /// </summary>
    void OnButtonPressEvent(object? sender, ButtonPressEventArgs args)
    {
        if (args.Event.Button == 1 && args.Event.Type == Gdk.EventType.DoubleButtonPress)
        {
            var cellInfoObj = GetCellInfo();
            if (cellInfoObj.HasValue)
            {
                var cellInfo = cellInfoObj.Value;
                Select(cellInfo.Path, cellInfo.Column, cellInfo.Iter, cellInfo.RowNumber, cellInfo.ColNumber);
            }
        }
    }

    /// <summary>
    /// Клік правою кнопкою миші
    /// </summary>
    void OnButtonReleaseEvent(object? sender, ButtonReleaseEventArgs args)
    {
        if (args.Event.Button == 3)
        {
            var cellInfoObj = GetCellInfo();
            if (cellInfoObj.HasValue)
            {
                var cellInfo = cellInfoObj.Value;
                OpenMenu(cellInfo.Path, cellInfo.Column, cellInfo.Iter, cellInfo.RowNumber, cellInfo.ColNumber);
            }
        }
    }

    void OnKeyReleaseEvent(object? sender, KeyReleaseEventArgs args)
    {
        switch (args.Event.Key)
        {
            case Gdk.Key.Insert:
                {
                    OnAddClick(null, new EventArgs());
                    break;
                }
            case Gdk.Key.Delete:
                {
                    if (!IsEditingCell) OnDeleteClick(TreeViewGrid, new EventArgs());
                    break;
                }
        }
    }

    #endregion

    #region ToolBar

    void OnAddClick(object? sender, EventArgs args)
    {
        AddRecord();
    }

    void OnCopyClick(object? sender, EventArgs args)
    {
        if (TreeViewGrid.Selection.CountSelectedRows() != 0)
            foreach (TreePath itemPath in TreeViewGrid.Selection.GetSelectedRows())
            {
                TreeViewGrid.Model.GetIter(out TreeIter iter, itemPath);
                int rowNumber = int.Parse(itemPath.ToString());

                CopyRecord(rowNumber);
            }
    }

    void OnDeleteClick(object? sender, EventArgs args)
    {
        if (TreeViewGrid.Selection.CountSelectedRows() != 0)
        {
            TreePath[] selectionRows = TreeViewGrid.Selection.GetSelectedRows();
            for (int i = selectionRows.Length - 1; i >= 0; i--)
            {
                TreePath itemPath = selectionRows[i];
                TreeViewGrid.Model.GetIter(out TreeIter iter, itemPath);

                int rowNumber = int.Parse(itemPath.ToString());

                DeleteRecord(iter, rowNumber);
            }
        }
    }

    #endregion
}