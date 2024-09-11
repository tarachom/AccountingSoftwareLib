/*
Copyright (C) 2019-2024 TARAKHOMYN YURIY IVANOVYCH
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

namespace InterfaceGtk
{
    public abstract class ДокументТабличнаЧастина : ФормаТабличнаЧастина
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

        public ДокументТабличнаЧастина()
        {
            CreateToolbar();

            ScrollTree = new ScrolledWindow() { ShadowType = ShadowType.In };
            ScrollTree.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);

            TreeViewGrid.Selection.Mode = SelectionMode.Multiple;
            TreeViewGrid.ActivateOnSingleClick = true;
            TreeViewGrid.ButtonPressEvent += OnButtonPressEvent;
            TreeViewGrid.ButtonReleaseEvent += OnButtonReleaseEvent;
            TreeViewGrid.KeyReleaseEvent += OnKeyReleaseEvent;

            ScrollTree.Add(TreeViewGrid);
            PackStart(ScrollTree, true, true, 0);

            ShowAll();
        }

        void CreateToolbar()
        {
            PackStart(ToolbarTop, false, false, 0);

            ToolButton upButton = new ToolButton(new Image(Stock.Add, IconSize.Menu), "Додати") { TooltipText = "Додати" };
            upButton.Clicked += OnAddClick;
            ToolbarTop.Add(upButton);

            ToolButton copyButton = new ToolButton(new Image(Stock.Copy, IconSize.Menu), "Копіювати") { TooltipText = "Копіювати" };
            copyButton.Clicked += OnCopyClick;
            ToolbarTop.Add(copyButton);

            ToolButton deleteButton = new ToolButton(new Image(Stock.Delete, IconSize.Menu), "Видалити") { TooltipText = "Видалити" };
            deleteButton.Clicked += OnDeleteClick;
            ToolbarTop.Add(deleteButton);
        }

        (TreePath Path, TreeViewColumn Column, TreeIter Iter, int RowNumber, int ColNumber)? GetCellInfo()
        {
            if (TreeViewGrid.Selection.CountSelectedRows() != 0)
            {
                TreeViewGrid.GetCursor(out TreePath itemPath, out TreeViewColumn treeColumn);
                TreeViewGrid.Model.GetIter(out TreeIter iter, itemPath);
                object? objColumn = treeColumn.Data["Column"];
                if (objColumn != null)
                {
                    int colNumber = (int)objColumn;
                    int rowNumber = int.Parse(itemPath.ToString());

                    return (itemPath, treeColumn, iter, rowNumber, colNumber);
                }
            }

            return null;
        }

        async void Select(TreePath path, TreeViewColumn column, TreeIter iter, int rowNumber, int colNumber)
        {
            //Швидкий вибір
            ДовідникШвидкийВибір? page = await OpenSelect2(iter, rowNumber, colNumber);
            if (page != null)
            {
                //Прив'язка до ячейки
                Gdk.Rectangle rectangleCell = TreeViewGrid.GetCellArea(path, column);
                rectangleCell.Offset(-(int)ScrollTree.Hadjustment.Value, rectangleCell.Height);

                page.PopoverParent = new Popover(TreeViewGrid)
                {
                    PointingTo = rectangleCell,
                    Position = PositionType.Bottom,
                    BorderWidth = 2
                };
                page.PopoverParent.Add(page);
                page.PopoverParent.ShowAll();

                //Заповнення даними
                await page.SetValue();
            }
        }

        void OpenMenu(TreePath path, TreeViewColumn column, TreeIter iter, int rowNumber, int colNumber)
        {
            Menu menu = new Menu();

            MenuItem caption = new MenuItem("[ " + column.Title + " ]");
            menu.Append(caption);

            MenuItem select = new MenuItem("Вибрати");
            select.Activated += (object? sender, EventArgs args) => Select(path, column, iter, rowNumber, colNumber);
            menu.Append(select);

            MenuItem copy = new MenuItem("Копіювати");
            copy.Activated += (object? sender, EventArgs args) => CopyRecord(rowNumber);
            menu.Append(copy);

            MenuItem clear = new MenuItem("Очистити");
            clear.Activated += (object? sender, EventArgs args) => ClearCell(iter, rowNumber, colNumber);
            menu.Append(clear);

            MenuItem delete = new MenuItem("Видалити");
            delete.Activated += (object? sender, EventArgs args) => DeleteRecord(iter, rowNumber);
            menu.Append(delete);

            menu.ShowAll();
            menu.Popup();
        }

        protected void EditCell(object sender, EditedArgs args)
        {
            var cellInfoObj = GetCellInfo();
            if (cellInfoObj.HasValue)
            {
                var cellInfo = cellInfoObj.Value;
                ChangeCell(cellInfo.Iter, cellInfo.RowNumber, cellInfo.ColNumber, args.NewText);
            }
        }

        #region Virtual & Abstract 

        protected virtual void OpenSelect(TreeIter iter, int rowNumber, int colNumber, Popover popover) { } //Del
        protected virtual void ButtonPopupClear(TreeIter iter, int rowNumber, int colNumber) { } //Del

        public abstract ValueTask LoadRecords();
        public abstract ValueTask SaveRecords();
        protected virtual async ValueTask<ДовідникШвидкийВибір?> OpenSelect2(TreeIter iter, int rowNumber, int colNumber)
        {
            return await ValueTask.FromResult<ДовідникШвидкийВибір?>(null);
        }
        protected virtual void ChangeCell(TreeIter iter, int rowNumber, int colNumber, string newText) { }
        protected abstract void AddRecord();
        protected abstract void CopyRecord(int rowNumber);
        protected abstract void DeleteRecord(TreeIter iter, int rowNumber);
        protected virtual void ClearCell(TreeIter iter, int rowNumber, int colNumber) { }
        protected virtual bool IsEditingCell() { return false; }

        #endregion

        #region TreeView

        void OnButtonPressEvent(object sender, ButtonPressEventArgs args)
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
                        OnDeleteClick(TreeViewGrid, new EventArgs());
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
            if (IsEditingCell())
                return;

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
}