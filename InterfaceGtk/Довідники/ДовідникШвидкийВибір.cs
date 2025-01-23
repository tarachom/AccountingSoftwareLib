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
    public abstract class ДовідникШвидкийВибір : ФормаЖурнал
    {
        /// <summary>
        /// Елемент для вибору
        /// </summary>
        public UnigueID? DirectoryPointerItem { get; set; }

        /// <summary>
        /// Відкрита папка.
        /// Використовується при загрузці дерева щоб приховати вітку.
        /// Актуально у випадку вибору родича, щоб не можна було вибрати у якості родича відкриту папку
        /// </summary>
        public UnigueID? OpenFolder { get; set; }

        /// <summary>
        /// Функція вибору
        /// </summary>
        public Action<UnigueID>? CallBack_OnSelectPointer { get; set; }

        /// <summary>
        /// Функція для множинного вибору
        /// </summary>
        public Action<UnigueID[]>? CallBack_OnMultipleSelectPointer { get; set; }

        /// <summary>
        /// Верхній набір меню
        /// </summary>
        protected Toolbar ToolbarTop = new Toolbar();

        /// <summary>
        /// Верхній горизонтальний блок
        /// </summary>
        protected Box HBoxTop = new Box(Orientation.Horizontal, 0);

        /// <summary>
        /// Пошук
        /// </summary>
        protected SearchControl Пошук = new SearchControl();

        public ДовідникШвидкийВибір(bool visibleSearch = true, int width = 600, int height = 300)
        {
            PackStart(HBoxTop, false, false, 0);

            if (visibleSearch)
            {
                Пошук.Select = async (string x) => await LoadRecords_OnSearch(x);
                Пошук.Clear = async () => await LoadRecords();
            }

            CreateToolbar(visibleSearch);

            ScrolledWindow scrollTree = new ScrolledWindow() { ShadowType = ShadowType.In, WidthRequest = width, HeightRequest = height };
            scrollTree.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);

            TreeViewGrid.Selection.Mode = SelectionMode.Multiple;
            TreeViewGrid.ActivateOnSingleClick = true;
            TreeViewGrid.RowActivated += OnRowActivated;
            TreeViewGrid.ButtonPressEvent += OnButtonPressEvent;
            TreeViewGrid.ButtonReleaseEvent += OnButtonReleaseEvent;

            scrollTree.Add(TreeViewGrid);

            PackStart(scrollTree, true, true, 0);

            ShowAll();
        }

        public override async ValueTask SetValue()
        {
            await LoadRecords();
        }

        #region Virtual & Abstract Function

        protected abstract ValueTask OpenPageList(UnigueID? unigueID = null);
        protected abstract ValueTask OpenPageElement(bool IsNew, UnigueID? unigueID = null);
        protected abstract ValueTask SetDeletionLabel(UnigueID unigueID);

        #endregion

        #region Toolbar & Menu

        void CreateToolbar(bool visibleSearch)
        {
            PackStart(ToolbarTop, false, false, 0);

            if (visibleSearch)
                ToolbarTop.Add(new ToolItem { Пошук });

            ToolButton openButton = new ToolButton(new Image(Stock.GoUp, IconSize.Menu), "Відкрити") { TooltipText = "Відкрити" };
            openButton.Clicked += OnListClick;
            ToolbarTop.Add(openButton);

            ToolButton addButton = new ToolButton(new Image(Stock.Add, IconSize.Menu), "Додати") { TooltipText = "Додати" };
            addButton.Clicked += OnAddClick;
            ToolbarTop.Add(addButton);

            ToolButton upButton = new ToolButton(new Image(Stock.Edit, IconSize.Menu), "Редагувати") { TooltipText = "Редагувати" };
            upButton.Clicked += OnEditClick;
            ToolbarTop.Add(upButton);

            ToolButton deleteButton = new ToolButton(new Image(Stock.Delete, IconSize.Menu), "Видалити") { TooltipText = "Видалити" };
            deleteButton.Clicked += OnDeleteClick;
            ToolbarTop.Add(deleteButton);

            ToolButton refreshButton = new ToolButton(new Image(Stock.Refresh, IconSize.Menu), "Обновити") { TooltipText = "Обновити" };
            refreshButton.Clicked += OnRefreshClick;
            ToolbarTop.Add(refreshButton);

            ToolButton multipleSelectButton = new ToolButton(new Image(Stock.RevertToSaved, IconSize.Menu), "Вибрати") { TooltipText = "Вибрати" };
            multipleSelectButton.Clicked += OnMultipleSelectClick;
            ToolbarTop.Add(multipleSelectButton);
        }

        Menu PopUpContextMenu()
        {
            Menu menu = new Menu();

            {
                MenuItem item = new MenuItem("Вибрати");
                item.Activated += OnMultipleSelectClick;
                menu.Append(item);
            }

            menu.ShowAll();

            return menu;
        }

        #endregion

        #region  TreeView

        void OnRowActivated(object sender, RowActivatedArgs args)
        {
            if (TreeViewGrid.Selection.CountSelectedRows() != 0)
            {
                TreeViewGrid.Model.GetIter(out TreeIter iter, TreeViewGrid.Selection.GetSelectedRows()[0]);
                DirectoryPointerItem = new UnigueID((string)TreeViewGrid.Model.GetValue(iter, 1));
            }
        }

        void OnButtonPressEvent(object? sender, ButtonPressEventArgs args)
        {
            if (args.Event.Type == Gdk.EventType.DoubleButtonPress && TreeViewGrid.Selection.CountSelectedRows() != 0)
                if (TreeViewGrid.Model.GetIter(out TreeIter iter, TreeViewGrid.Selection.GetSelectedRows()[0]))
                {
                    UnigueID unigueID = new UnigueID((string)TreeViewGrid.Model.GetValue(iter, 1));
                    if (unigueID.IsEmpty()) return;

                    DirectoryPointerItem = unigueID;

                    CallBack_OnSelectPointer?.Invoke(unigueID);
                    PopoverParent?.Hide();
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

        #endregion

        #region ToolBar

        async void OnListClick(object? sender, EventArgs args)
        {
            UnigueID? unigueID = null;
            if (TreeViewGrid.Selection.CountSelectedRows() != 0)
                if (TreeViewGrid.Model.GetIter(out TreeIter iter, TreeViewGrid.Selection.GetSelectedRows()[0]))
                    unigueID = new UnigueID((string)TreeViewGrid.Model.GetValue(iter, 1));

            await OpenPageList(unigueID);
        }

        async void OnAddClick(object? sender, EventArgs args)
        {
            await OpenPageElement(true);
        }

        async void OnEditClick(object? sender, EventArgs args)
        {
            if (TreeViewGrid.Selection.CountSelectedRows() != 0)
                foreach (TreePath itemPath in TreeViewGrid.Selection.GetSelectedRows())
                    if (TreeViewGrid.Model.GetIter(out TreeIter iter, itemPath))
                    {
                        UnigueID unigueID = new UnigueID((string)TreeViewGrid.Model.GetValue(iter, 1));
                        if (!unigueID.IsEmpty())
                            await OpenPageElement(false, unigueID);
                    }
        }

        async void OnDeleteClick(object? sender, EventArgs args)
        {
            if (TreeViewGrid.Selection.CountSelectedRows() != 0)
                if (Message.Request(null, "Встановити або зняти помітку на видалення?") == ResponseType.Yes)
                {
                    foreach (TreePath itemPath in TreeViewGrid.Selection.GetSelectedRows())
                    {
                        TreeViewGrid.Model.GetIter(out TreeIter iter, itemPath);
                        UnigueID unigueID = new UnigueID((string)TreeViewGrid.Model.GetValue(iter, 1));

                        if (!unigueID.IsEmpty())
                        {
                            await SetDeletionLabel(unigueID);
                            SelectPointerItem = unigueID;
                        }
                    }

                    await LoadRecords();
                }
        }

        async void OnRefreshClick(object? sender, EventArgs args)
        {
            await LoadRecords();
        }

        void OnMultipleSelectClick(object? sender, EventArgs args)
        {
            if (CallBack_OnMultipleSelectPointer != null && TreeViewGrid.Selection.CountSelectedRows() != 0)
            {
                List<UnigueID> listUnigueID = [];
                foreach (var selectionRow in TreeViewGrid.Selection.GetSelectedRows())
                {
                    TreeViewGrid.Model.GetIter(out TreeIter iter, selectionRow);

                    UnigueID unigueID = new UnigueID((string)TreeViewGrid.Model.GetValue(iter, 1));
                    if (unigueID.IsEmpty()) continue;

                    listUnigueID.Add(unigueID);
                }

                CallBack_OnMultipleSelectPointer.Invoke([.. listUnigueID]);
            }
        }

        #endregion
    }
}