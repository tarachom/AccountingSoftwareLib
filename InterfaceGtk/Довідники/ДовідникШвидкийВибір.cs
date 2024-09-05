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
    public abstract class ДовідникШвидкийВибір : ФормаЖурнал
    {
        /// <summary>
        /// Вспливаюче вікно
        /// </summary>
        public Popover? PopoverParent { get; set; }

        /// <summary>
        /// Елемент для вибору
        /// </summary>
        public UnigueID? DirectoryPointerItem { get; set; }

        /// <summary>
        /// Функція вибору
        /// </summary>
        public Action<UnigueID>? CallBack_OnSelectPointer { get; set; }

        /// <summary>
        /// Верхній набір меню
        /// </summary>
        protected Toolbar ToolbarTop = new Toolbar();

        /// <summary>
        /// Верхній горизонтальний блок
        /// </summary>
        protected Box HBoxTop = new Box(Orientation.Horizontal, 0);

        /// <summary>
        /// Дерево
        /// </summary>
        protected TreeView TreeViewGrid = new TreeView();

        /// <summary>
        /// Пошук
        /// </summary>
        protected SearchControl Пошук = new SearchControl();

        public ДовідникШвидкийВибір(bool visibleSearch = true, int width = 600, int height = 300) : base()
        {
            PackStart(HBoxTop, false, false, 0);

            if (visibleSearch)
            {
                HBoxTop.PackStart(Пошук, false, false, 0);
                Пошук.Select = async (string x) => { await LoadRecords_OnSearch(x); };
                Пошук.Clear = async () => { await LoadRecords(); };
            }

            CreateToolbar();

            ScrolledWindow scrollTree = new ScrolledWindow() { ShadowType = ShadowType.In, WidthRequest = width, HeightRequest = height };
            scrollTree.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);

            TreeViewGrid.Selection.Mode = SelectionMode.Multiple;
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

        #region Toolbar & Menu
 
        void CreateToolbar()
        {
            PackStart(ToolbarTop, false, false, 0);

            ToolButton openButton = new ToolButton(new Image(Stock.GoUp, IconSize.Menu), "Відкрити") { TooltipText = "Відкрити" };
            openButton.Clicked += OnOpenClick;
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
        }

        #endregion

        #region Virtual Function

        protected abstract ValueTask LoadRecords();

        protected abstract ValueTask LoadRecords_OnSearch(string searchText);

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
                    DirectoryPointerItem = new UnigueID((string)TreeViewGrid.Model.GetValue(iter, 1));

                    CallBack_OnSelectPointer?.Invoke(DirectoryPointerItem);
                    PopoverParent?.Hide();
                }
        }

        #endregion

        #region ToolBar

        void OnOpenClick(object? sender, EventArgs args)
        {

        }

        void OnAddClick(object? sender, EventArgs args)
        {

        }

        void OnEditClick(object? sender, EventArgs args)
        {

        }

        void OnDeleteClick(object? sender, EventArgs args)
        {

        }

        void OnRefreshClick(object? sender, EventArgs args)
        {
            LoadRecords();
        }

        #endregion
    }
}