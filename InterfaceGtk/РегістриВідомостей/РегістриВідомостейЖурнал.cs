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
using AccountingSoftware;

namespace InterfaceGtk
{
    public abstract class РегістриВідомостейЖурнал : ФормаЖурнал
    {
        /// <summary>
        /// Період
        /// </summary>
        protected PeriodControl Період = new PeriodControl();

        /// <summary>
        /// Верхній набір меню
        /// </summary>
        protected Toolbar ToolbarTop = new Toolbar();

        /// <summary>
        /// Верхній бок для додаткових кнопок
        /// </summary>
        protected Box HBoxTop = new Box(Orientation.Horizontal, 0);

        /// <summary>
        /// Пошук
        /// </summary>
        SearchControl Пошук = new SearchControl();

        public РегістриВідомостейЖурнал()
        {
            //Кнопки
            PackStart(HBoxTop, false, false, 10);

            //Період
            Період.Changed = PeriodChanged;
            HBoxTop.PackStart(Період, false, false, 2);

            //Пошук
            Пошук.Select = async x => await BeforeLoadRecords_OnSearch(x);
            Пошук.Clear = async () => await BeforeLoadRecords();
            HBoxTop.PackStart(Пошук, false, false, 2);

            CreateToolbar();

            TreeViewGrid.RowActivated += OnRowActivated;
            TreeViewGrid.ButtonPressEvent += OnButtonPressEvent;
            TreeViewGrid.ButtonReleaseEvent += OnButtonReleaseEvent;
            TreeViewGrid.KeyReleaseEvent += OnKeyReleaseEvent;

            //Сторінки
            AddPages(new Сторінки.Налаштування() { Тип = Сторінки.ТипЖурналу.РегістриВідомостей });

            ScrollTree.Add(TreeViewGrid);

            PackStart(ScrollTree, true, true, 0);
            PackStart(ScrollPages, false, true, 0);

            ShowAll();
        }

        public override async ValueTask SetValue()
        {
            DefaultGrabFocus();

            await BeforeSetValue();
        }

        #region Toolbar & Menu

        void CreateToolbar()
        {
            PackStart(ToolbarTop, false, false, 0);

            ToolButton addButton = new ToolButton(new Image(Stock.Add, IconSize.Menu), "Додати") { TooltipText = "Додати" };
            addButton.Clicked += OnAddClick;
            ToolbarTop.Add(addButton);

            ToolButton upButton = new ToolButton(new Image(Stock.Edit, IconSize.Menu), "Редагувати") { TooltipText = "Редагувати" };
            upButton.Clicked += OnEditClick;
            ToolbarTop.Add(upButton);

            ToolButton copyButton = new ToolButton(new Image(Stock.Copy, IconSize.Menu), "Копіювати") { TooltipText = "Копіювати" };
            copyButton.Clicked += OnCopyClick;
            ToolbarTop.Add(copyButton);

            ToolButton deleteButton = new ToolButton(new Image(Stock.Delete, IconSize.Menu), "Видалити") { TooltipText = "Видалити" };
            deleteButton.Clicked += OnDeleteClick;
            ToolbarTop.Add(deleteButton);

            ToolButton refreshButton = new ToolButton(new Image(Stock.Refresh, IconSize.Menu), "Обновити") { TooltipText = "Обновити" };
            refreshButton.Clicked += OnRefreshClick;
            ToolbarTop.Add(refreshButton);
        }

        Menu PopUpContextMenu()
        {
            Menu Menu = new Menu();

            MenuItem delete = new MenuItem("Видалити");
            delete.Activated += OnDeleteClick;
            Menu.Append(delete);

            Menu.ShowAll();

            return Menu;
        }

        #endregion

        #region Virtual Function

        protected abstract ValueTask OpenPageElement(bool IsNew, UnigueID? unigueID = null);

        protected virtual ValueTask Delete(UnigueID unigueID) { return new ValueTask(); }

        protected virtual ValueTask<UnigueID?> Copy(UnigueID unigueID) { return new ValueTask<UnigueID?>(); }

        protected virtual async void CallBack_LoadRecords(UnigueID? selectPointer)
        {
            SelectPointerItem = selectPointer;
            await BeforeLoadRecords();
        }

        protected abstract void PeriodChanged();

        #endregion

        #region  TreeView

        void OnRowActivated(object sender, RowActivatedArgs args)
        {
            if (TreeViewGrid.Selection.CountSelectedRows() != 0)
            {
                TreeViewGrid.Model.GetIter(out TreeIter iter, TreeViewGrid.Selection.GetSelectedRows()[0]);
                SelectPointerItem = new UnigueID((string)TreeViewGrid.Model.GetValue(iter, 1));
            }
        }

        void OnButtonReleaseEvent(object? sender, ButtonReleaseEventArgs args)
        {
            if (args.Event.Button == 3 && TreeViewGrid.Selection.CountSelectedRows() != 0)
                if (TreeViewGrid.Model.GetIter(out TreeIter iter, TreeViewGrid.Selection.GetSelectedRows()[0]))
                {
                    SelectPointerItem = new UnigueID((string)TreeViewGrid.Model.GetValue(iter, 1));
                    PopUpContextMenu().Popup();
                }
        }

        async void OnButtonPressEvent(object? sender, ButtonPressEventArgs args)
        {
            if (args.Event.Type == Gdk.EventType.DoubleButtonPress && TreeViewGrid.Selection.CountSelectedRows() != 0)
                if (TreeViewGrid.Model.GetIter(out TreeIter iter, TreeViewGrid.Selection.GetSelectedRows()[0]))
                {
                    UnigueID unigueID = new UnigueID((string)TreeViewGrid.Model.GetValue(iter, 1));
                    await OpenPageElement(false, unigueID);
                }
        }

        async void OnKeyReleaseEvent(object? sender, KeyReleaseEventArgs args)
        {
            switch (args.Event.Key)
            {
                case Gdk.Key.Insert:
                    {
                        await OpenPageElement(true);
                        break;
                    }
                case Gdk.Key.F5:
                    {
                        OnRefreshClick(null, new EventArgs());
                        break;
                    }
                case Gdk.Key.KP_Enter:
                case Gdk.Key.Return:
                    {
                        OnEditClick(null, new EventArgs());
                        break;
                    }
                case Gdk.Key.End:
                case Gdk.Key.Home:
                case Gdk.Key.Up:
                case Gdk.Key.Down:
                case Gdk.Key.Prior:
                case Gdk.Key.Next:
                    {
                        OnRowActivated(TreeViewGrid, new RowActivatedArgs());
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

        async void OnAddClick(object? sender, EventArgs args)
        {
            await OpenPageElement(true);
        }

        async void OnEditClick(object? sender, EventArgs args)
        {
            if (TreeViewGrid.Selection.CountSelectedRows() != 0)
            {
                ToolButtonSensitive(sender, false);

                TreePath[] selectionRows = TreeViewGrid.Selection.GetSelectedRows();
                foreach (TreePath itemPath in selectionRows)
                    if (TreeViewGrid.Model.GetIter(out TreeIter iter, itemPath))
                    {
                        UnigueID unigueID = new UnigueID((string)TreeViewGrid.Model.GetValue(iter, 1));
                        await OpenPageElement(false, unigueID);
                    }

                ToolButtonSensitive(sender, true);
            }
        }

        async void OnRefreshClick(object? sender, EventArgs args)
        {
            ToolButtonSensitive(sender, false);

            await BeforeLoadRecords();

            ToolButtonSensitive(sender, true);
        }

        async void OnDeleteClick(object? sender, EventArgs args)
        {
            if (TreeViewGrid.Selection.CountSelectedRows() != 0)
                if (Message.Request(null, "Видалити?") == ResponseType.Yes)
                {
                    ToolButtonSensitive(sender, false);

                    TreePath[] selectionRows = TreeViewGrid.Selection.GetSelectedRows();
                    foreach (TreePath itemPath in selectionRows)
                    {
                        TreeViewGrid.Model.GetIter(out TreeIter iter, itemPath);
                        UnigueID unigueID = new UnigueID((string)TreeViewGrid.Model.GetValue(iter, 1));

                        await Delete(unigueID);

                        SelectPointerItem = unigueID;
                    }

                    await BeforeLoadRecords();

                    ToolButtonSensitive(sender, true);
                }
        }

        async void OnCopyClick(object? sender, EventArgs args)
        {
            if (TreeViewGrid.Selection.CountSelectedRows() != 0)
                if (Message.Request(null, "Копіювати?") == ResponseType.Yes)
                {
                    ToolButtonSensitive(sender, false);

                    TreePath[] selectionRows = TreeViewGrid.Selection.GetSelectedRows();
                    foreach (TreePath itemPath in selectionRows)
                    {
                        TreeViewGrid.Model.GetIter(out TreeIter iter, itemPath);
                        UnigueID unigueID = new UnigueID((string)TreeViewGrid.Model.GetValue(iter, 1));
                        UnigueID? newUnigueID = await Copy(unigueID);

                        if (newUnigueID != null)
                            SelectPointerItem = newUnigueID;
                    }

                    await BeforeLoadRecords();

                    ToolButtonSensitive(sender, true);
                }
        }

        #endregion
    }
}