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
using AccountingSoftware;

namespace InterfaceGtk
{
    public abstract class ДовідникДерево : ФормаДерево
    {
        /// <summary>
        /// Елемент на який треба спозиціонувати список при обновленні
        /// </summary>
        public UnigueID? SelectPointerItem { get; set; }

        /// <summary>
        /// ???
        /// </summary>
        public Widget? ParentWidget { get; set; }

        /// <summary>
        /// Для вибору
        /// </summary>
        public UnigueID? DirectoryPointerItem { get; set; }

        /// <summary>
        /// Відкрита папка.
        /// Використовується при загрузці дерева щоб приховати вітку.
        /// Актуальну у випадку вибору родича, щоб не можна було вибрати у якості родича відкриту папку
        /// </summary>
        public UnigueID? OpenFolder { get; set; }

        /// <summary>
        /// Функція яка викликається коли в дереві активується вітка.
        /// Це зазвичай завантаження списку елементів у таблиці
        /// </summary>
        public System.Action? CallBack_RowActivated { get; set; }

        /// <summary>
        /// Функція вибору
        /// </summary>
        public Action<UnigueID>? CallBack_OnSelectPointer { get; set; }

        /// <summary>
        /// Верхній набір меню
        /// </summary>
        protected Toolbar ToolbarTop = new Toolbar();

        /// <summary>
        /// Дерево
        /// </summary>
        protected TreeView TreeViewGrid = new TreeView();

        public ДовідникДерево() : base()
        {
            CreateToolbar();

            ScrolledWindow scrollTree = new ScrolledWindow() { ShadowType = ShadowType.In };
            scrollTree.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);

            TreeViewGrid.Selection.Mode = SelectionMode.Single;
            TreeViewGrid.ActivateOnSingleClick = true;
            TreeViewGrid.RowActivated += OnRowActivated;
            TreeViewGrid.ButtonPressEvent += OnButtonPressEvent;

            scrollTree.Add(TreeViewGrid);

            PackStart(scrollTree, true, true, 0);

            ShowAll();
        }

        public async ValueTask SetValue()
        {
            await LoadRecords();
        }

        async void BeforeAndAfterOpenElement(bool IsNew, UnigueID? unigueID = null)
        {
            Notebook? notebook = NotebookFunction.GetNotebookFromWidget(ParentWidget ?? this);
            (string Name, Func<Widget>? FuncWidget, System.Action? SetValue) page = await OpenPageElement(IsNew, unigueID);
            if (notebook != null && page.FuncWidget != null)
                NotebookFunction.CreateNotebookPage(notebook, page.Name + (IsNew ? " *" : ""), page.FuncWidget);
            page.SetValue?.Invoke();
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

            MenuItem setDeletionLabel = new MenuItem("Помітка на видалення");
            setDeletionLabel.Activated += OnDeleteClick;
            Menu.Append(setDeletionLabel);

            Menu.ShowAll();

            return Menu;
        }

        #endregion

        #region Virtual & Abstract Function

        protected abstract ValueTask LoadRecords();

        protected abstract ValueTask<(string Name, Func<Widget>? FuncWidget, System.Action? SetValue)> OpenPageElement(bool IsNew, UnigueID? unigueID = null);

        protected abstract ValueTask SetDeletionLabel(UnigueID unigueID);

        protected abstract ValueTask<UnigueID?> Copy(UnigueID unigueID);

        protected virtual async void CallBack_LoadRecords(UnigueID? selectPointer)
        {
            DirectoryPointerItem = selectPointer;
            await LoadRecords();
        }

        #endregion

        #region  TreeView

        protected void RowActivated()
        {
            if (TreeViewGrid.Selection.CountSelectedRows() != 0)
            {
                TreeViewGrid.Model.GetIter(out TreeIter iter, TreeViewGrid.Selection.GetSelectedRows()[0]);
                DirectoryPointerItem = new UnigueID((string)TreeViewGrid.Model.GetValue(iter, 1));
                CallBack_RowActivated?.Invoke();
            }
        }

        void OnRowActivated(object sender, RowActivatedArgs args)
        {
            RowActivated();
        }

        void OnButtonReleaseEvent(object? sender, ButtonReleaseEventArgs args)
        {
            if (args.Event.Button == 3 && TreeViewGrid.Selection.CountSelectedRows() != 0)
                if (TreeViewGrid.Model.GetIter(out TreeIter iter, TreeViewGrid.Selection.GetSelectedRows()[0]))
                {
                    DirectoryPointerItem = new UnigueID((string)TreeViewGrid.Model.GetValue(iter, 1));
                    PopUpContextMenu().Popup();
                }
        }

        void OnButtonPressEvent(object? sender, ButtonPressEventArgs args)
        {
            if (args.Event.Type == Gdk.EventType.DoubleButtonPress && TreeViewGrid.Selection.CountSelectedRows() != 0)
                if (TreeViewGrid.Model.GetIter(out TreeIter iter, TreeViewGrid.Selection.GetSelectedRows()[0]))
                {
                    UnigueID unigueID = new UnigueID((string)TreeViewGrid.Model.GetValue(iter, 1));

                    if (DirectoryPointerItem == null)
                    {
                        if (!unigueID.IsEmpty())
                            BeforeAndAfterOpenElement(false, unigueID);
                    }
                    else
                    {
                        CallBack_OnSelectPointer?.Invoke(unigueID);
                        NotebookFunction.CloseNotebookPageToCode(NotebookFunction.GetNotebookFromWidget(this), this.Name);
                    }
                }
        }

        /*
        void OnKeyReleaseEvent(object? sender, KeyReleaseEventArgs args)
        {
            switch (args.Event.Key)
            {
                case Gdk.Key.Insert:
                    {
                        OpenPageElement(true);
                        break;
                    }
                case Gdk.Key.F5:
                    {
                        LoadTree();
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
        */

        #endregion

        #region ToolBar

        void OnAddClick(object? sender, EventArgs args)
        {
            BeforeAndAfterOpenElement(true);
        }

        void OnEditClick(object? sender, EventArgs args)
        {
            if (TreeViewGrid.Selection.CountSelectedRows() != 0)
                if (TreeViewGrid.Model.GetIter(out TreeIter iter, TreeViewGrid.Selection.GetSelectedRows()[0]))
                {
                    UnigueID unigueID = new UnigueID((string)TreeViewGrid.Model.GetValue(iter, 1));
                    if (!unigueID.IsEmpty())
                        BeforeAndAfterOpenElement(false, unigueID);
                }
        }

        async void OnRefreshClick(object? sender, EventArgs args)
        {
            await LoadRecords();
        }

        async void OnDeleteClick(object? sender, EventArgs args)
        {
            if (TreeViewGrid.Selection.CountSelectedRows() != 0)
                if (Message.Request(null, "Встановити або зняти помітку на видалення?") == ResponseType.Yes)
                {
                    TreePath selectionRow = TreeViewGrid.Selection.GetSelectedRows()[0];
                    TreeViewGrid.Model.GetIter(out TreeIter iter, selectionRow);
                    UnigueID unigueID = new UnigueID((string)TreeViewGrid.Model.GetValue(iter, 1));

                    if (unigueID.IsEmpty())
                        return;

                    await SetDeletionLabel(unigueID);

                    DirectoryPointerItem = unigueID;

                    await LoadRecords();
                }
        }

        async void OnCopyClick(object? sender, EventArgs args)
        {
            if (TreeViewGrid.Selection.CountSelectedRows() != 0)
                if (Message.Request(null, "Копіювати?") == ResponseType.Yes)
                {
                    TreePath selectionRow = TreeViewGrid.Selection.GetSelectedRows()[0];
                    TreeViewGrid.Model.GetIter(out TreeIter iter, selectionRow);
                    UnigueID unigueID = new UnigueID((string)TreeViewGrid.Model.GetValue(iter, 1));

                    if (unigueID.IsEmpty())
                        return;

                    UnigueID? newUnigueID = await Copy(unigueID);

                    if (newUnigueID != null)
                        DirectoryPointerItem = newUnigueID;

                    await LoadRecords();
                }
        }

        #endregion
    }
}