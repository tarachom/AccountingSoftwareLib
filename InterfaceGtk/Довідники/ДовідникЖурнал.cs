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
    public abstract class ДовідникЖурнал : ФормаЖурнал
    {
        /// <summary>
        /// Елемент на який треба спозиціонувати список при виборі
        /// </summary>
        public UnigueID? DirectoryPointerItem { get; set; }

        /// <summary>
        /// Відкрита папка.
        /// Використовується при загрузці дерева щоб приховати вітку.
        /// Актуально у випадку вибору родича, щоб не можна було вибрати у якості родича відкриту папку
        /// </summary>
        public UnigueID? OpenFolder { get; set; }

        /// <summary>
        /// Функція яка викликається коли в дереві активується вітка.
        /// Це зазвичай завантаження списку елементів у таблиці
        /// </summary>
        public System.Action? CallBack_RowActivated { get; set; }

        /// <summary>
        /// Функція зворотнього виклику при виборі
        /// </summary>
        public Action<UnigueID>? CallBack_OnSelectPointer { get; set; }

        /// <summary>
        /// Верхній набір меню
        /// </summary>
        protected Toolbar ToolbarTop = new Toolbar();

        /// <summary>
        /// Верхній горизонтальний бокс.
        /// можна додаткові кнопки і різні лінки добавляти
        /// </summary>
        protected Box HBoxTop = new Box(Orientation.Horizontal, 0);

        /// <summary>
        /// Панелька яка містить одну область за замовчуванням і туди добавляється список
        /// У випадку якщо потрібно можна додати ще одну область, це використовується для ієрархічних довідників
        /// </summary>
        protected Paned HPanedTable = new Paned(Orientation.Horizontal);

        /// <summary>
        /// Пошук
        /// </summary>
        SearchControl Пошук = new SearchControl();

        public ДовідникЖурнал()
        {
            //Кнопки
            PackStart(HBoxTop, false, false, 10);

            //Пошук
            Пошук.Select = async (string x) => await LoadRecords_OnSearch(x);
            Пошук.Clear = async () => await LoadRecords();
            HBoxTop.PackStart(Пошук, false, false, 2);

            CreateToolbar();

            ScrolledWindow scrollTree = new ScrolledWindow() { ShadowType = ShadowType.In };
            scrollTree.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);

            TreeViewGrid.RowActivated += OnRowActivated;
            TreeViewGrid.ButtonPressEvent += OnButtonPressEvent;
            TreeViewGrid.ButtonReleaseEvent += OnButtonReleaseEvent;
            TreeViewGrid.KeyReleaseEvent += OnKeyReleaseEvent;
            //TreeViewGrid.KeyPressEvent += OnKeyPressEvent;

            scrollTree.Add(TreeViewGrid);

            HPanedTable.Pack1(scrollTree, true, true);

            PackStart(HPanedTable, true, true, 0);

            ShowAll();
        }

        public override async ValueTask SetValue()
        {
            await LoadRecords();
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

            //Separator
            ToolItem toolItemSeparator = new ToolItem { new Separator(Orientation.Horizontal) };
            ToolbarTop.Add(toolItemSeparator);

            ToolButton filterButton = new ToolButton(new Image(Stock.SortAscending, IconSize.Menu), "Фільтрувати") { TooltipText = "Фільтрувати" };
            filterButton.Clicked += OnFilterClick;
            ToolbarTop.Add(filterButton);
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

        protected abstract ValueTask OpenPageElement(bool IsNew, UnigueID? unigueID = null);

        protected abstract ValueTask SetDeletionLabel(UnigueID unigueID);

        protected abstract ValueTask<UnigueID?> Copy(UnigueID unigueID);

        protected virtual async void CallBack_LoadRecords(UnigueID? selectPointer)
        {
            SelectPointerItem = selectPointer;
            await LoadRecords();
        }

        protected abstract Widget? FilterRecords(Box hBox);

        #endregion

        #region  TreeView

        void OnRowActivated(object sender, RowActivatedArgs args)
        {
            if (TreeViewGrid.Selection.CountSelectedRows() != 0)
            {
                TreeViewGrid.Model.GetIter(out TreeIter iter, TreeViewGrid.Selection.GetSelectedRows()[0]);
                SelectPointerItem = new UnigueID((string)TreeViewGrid.Model.GetValue(iter, 1));
                CallBack_RowActivated?.Invoke();
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

                    if (DirectoryPointerItem == null)
                    {
                        if (!unigueID.IsEmpty())
                            await OpenPageElement(false, unigueID);
                    }
                    else
                    {
                        CallBack_OnSelectPointer?.Invoke(unigueID);
                        NotebookFunction.CloseNotebookPageToCode(NotebookFunction.GetNotebookFromWidget(this), this.Name);
                    }
                }
        }

        async void OnKeyReleaseEvent(object? sender, KeyReleaseEventArgs args)
        {
            /*
            if (args.Event.State == (Gdk.ModifierType.ControlMask | Gdk.ModifierType.Mod2Mask))
            {
                switch (args.Event.Key)
                {
                    case Gdk.Key.Return:
                        {
                            Console.WriteLine(1);
                            break;
                        }
                }
            }
            else
            */
            switch (args.Event.Key)
            {
                case Gdk.Key.Insert:
                    {
                        await OpenPageElement(true);
                        break;
                    }
                case Gdk.Key.F5:
                    {
                        await LoadRecords();
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

        // void OnKeyPressEvent(object? sender, KeyPressEventArgs args)
        // {
        // Console.WriteLine(args.Event.State);
        // Console.WriteLine(Gdk.ModifierType.ControlMask);
        // Console.WriteLine(Gdk.ModifierType.Mod2Mask);
        // Console.WriteLine(Gdk.ModifierType.ControlMask | Gdk.ModifierType.Mod2Mask);
        // Console.WriteLine(args.Event.Key);

        // if (args.Event.State == (Gdk.ModifierType.ControlMask | Gdk.ModifierType.Mod2Mask) && args.Event.Key == Gdk.Key.Return)
        // {
        //     Console.WriteLine(1);
        // }
        //}

        #endregion

        #region ToolBar

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

        async void OnRefreshClick(object? sender, EventArgs args)
        {
            await LoadRecords();
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

        async void OnCopyClick(object? sender, EventArgs args)
        {
            if (TreeViewGrid.Selection.CountSelectedRows() != 0)
                if (Message.Request(null, "Копіювати?") == ResponseType.Yes)
                {
                    foreach (TreePath itemPath in TreeViewGrid.Selection.GetSelectedRows())
                    {
                        TreeViewGrid.Model.GetIter(out TreeIter iter, itemPath);
                        UnigueID unigueID = new UnigueID((string)TreeViewGrid.Model.GetValue(iter, 1));
                        if (!unigueID.IsEmpty())
                        {
                            UnigueID? newUnigueID = await Copy(unigueID);
                            if (newUnigueID != null)
                                SelectPointerItem = newUnigueID;
                        }
                    }

                    await LoadRecords();
                }
        }

        void OnFilterClick(object? sender, EventArgs args)
        {
            Popover popover = new Popover((ToolButton)sender!)
            {
                Position = PositionType.Bottom,
                BorderWidth = 2
            };

            Box vBox = new Box(Orientation.Vertical, 0);
            Box hBox = new Box(Orientation.Horizontal, 0);
            vBox.PackStart(hBox, false, false, 5);

            Widget? widget = FilterRecords(hBox);
            if (widget != null)
                hBox.PackStart(widget, false, false, 5);

            popover.Add(vBox);
            popover.ShowAll();
        }

        #endregion
    }
}