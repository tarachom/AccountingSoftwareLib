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
        /// Функція для множинного вибору
        /// </summary>
        public Action<UnigueID[]>? CallBack_OnMultipleSelectPointer { get; set; }

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
        protected SearchControl Пошук = new SearchControl();

        /// <summary>
        /// Фільтр
        /// </summary>
        protected ListFilterControl Фільтр = new ListFilterControl();

        /// <summary>
        /// Переключатель для довідника з деревом
        /// </summary>
        protected CheckButton IsHierarchy = new CheckButton("Ієрархія папок") { Active = true, Name = "IsHierarchy" };

        public ДовідникЖурнал()
        {
            //Кнопки
            PackStart(HBoxTop, false, false, 10);

            //Пошук
            Пошук.Select = async x =>
            {
                Фільтр.IsFiltered = false;
                await BeforeLoadRecords_OnSearch(x);
            };
            Пошук.Clear = async () => await BeforeLoadRecords();
            HBoxTop.PackStart(Пошук, false, false, 2);

            //Фільтр
            Фільтр.Select = async () => await BeforeLoadRecords_OnFilter();
            Фільтр.Clear = async () => await BeforeLoadRecords();
            Фільтр.FillFilterList = FillFilterList;

            CreateToolbar();

            TreeViewGrid.RowActivated += OnRowActivated;
            TreeViewGrid.ButtonPressEvent += OnButtonPressEvent;
            TreeViewGrid.ButtonReleaseEvent += OnButtonReleaseEvent;
            TreeViewGrid.KeyReleaseEvent += OnKeyReleaseEvent;

            //Сторінки
            AddPages(new Сторінки.Налаштування() { Тип = Сторінки.ТипЖурналу.Довідники });

            ScrollTree.Add(TreeViewGrid);
            HPanedTable.Pack1(ScrollTree, true, true);

            PackStart(HPanedTable, true, true, 0);
            PackStart(ScrollPages, false, true, 0);

            ShowAll();
        }

        public override async ValueTask SetValue()
        {
            DefaultGrabFocus();

            await BeforeSetValue();            
        }

        #region Hierarchy

        /// <summary>
        /// Додати переключатель на форму
        /// </summary>
        public void AddIsHierarchy()
        {
            if (HBoxTop.Children.Where(x => x.Name == "IsHierarchy").Any())
                return;

            //Враховувати ієрархію папок
            IsHierarchy.Clicked += async (sender, args) =>
            {
                if (IsHierarchy.Active)
                    await BeforeLoadRecords();
                else
                    await BeforeLoadRecords_OnTree();
            };

            HBoxTop.PackStart(IsHierarchy, false, false, 10);
        }

        #endregion

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

            ToolButton multipleSelectButton = new ToolButton(new Image(Stock.RevertToSaved, IconSize.Menu), "Вибрати") { TooltipText = "Вибрати" };
            multipleSelectButton.Clicked += OnMultipleSelectClick;
            ToolbarTop.Add(multipleSelectButton);

            ToolButton versionshistoryButton = new ToolButton(new Image(Stock.FindAndReplace, IconSize.Menu), "Історія зміни даних") { TooltipText = "Історія зміни даних" };
            versionshistoryButton.Clicked += OnVersionsHistoryClick;
            ToolbarTop.Add(versionshistoryButton);
        }

        Menu PopUpContextMenu()
        {
            Menu Menu = new Menu();

            {
                MenuItem menu = new MenuItem("Додати");
                menu.Activated += OnAddClick;
                Menu.Append(menu);
            }

            {
                MenuItem menu = new MenuItem("Редагувати");
                menu.Activated += OnEditClick;
                Menu.Append(menu);
            }

            {
                MenuItem menu = new MenuItem("Копіювати");
                menu.Activated += OnCopyClick;
                Menu.Append(menu);
            }

            {
                MenuItem menu = new MenuItem("Видалити");
                menu.Activated += OnDeleteClick;
                Menu.Append(menu);
            }

            {
                MenuItem menu = new MenuItem("Обновити");
                menu.Activated += OnRefreshClick;
                Menu.Append(menu);
            }

            Menu.Append(new SeparatorMenuItem());

            {
                MenuItem menu = new MenuItem("Вибрати");
                menu.Activated += OnMultipleSelectClick;
                Menu.Append(menu);
            }

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
            await BeforeLoadRecords();
        }

        protected virtual void FillFilterList(ListFilterControl filterControl) { }

        protected virtual async ValueTask VersionsHistory(UnigueID unigueID) { await ValueTask.FromResult(true); }

        #endregion

        #region TreeView

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

                foreach (TreePath itemPath in TreeViewGrid.Selection.GetSelectedRows())
                    if (TreeViewGrid.Model.GetIter(out TreeIter iter, itemPath))
                    {
                        UnigueID unigueID = new UnigueID((string)TreeViewGrid.Model.GetValue(iter, 1));
                        if (!unigueID.IsEmpty())
                            await OpenPageElement(false, unigueID);
                    }

                ToolButtonSensitive(sender, true);
            }
        }

        async void OnRefreshClick(object? sender, EventArgs args)
        {
            ToolButtonSensitive(sender, false);

            if (Фільтр.IsFiltered)
                await BeforeLoadRecords_OnFilter();
            else
                await BeforeLoadRecords();

            ToolButtonSensitive(sender, true);
        }

        async void OnDeleteClick(object? sender, EventArgs args)
        {
            if (TreeViewGrid.Selection.CountSelectedRows() != 0)
                if (Message.Request(null, "Встановити або зняти помітку на видалення?") == ResponseType.Yes)
                {
                    SpinnerOn(NotebookFunction.GetNotebookFromWidget(this));
                    ToolButtonSensitive(sender, false);

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

                    if (Фільтр.IsFiltered)
                        await BeforeLoadRecords_OnFilter();
                    else
                        await BeforeLoadRecords();

                    ToolButtonSensitive(sender, true);
                }
        }

        async void OnCopyClick(object? sender, EventArgs args)
        {
            if (TreeViewGrid.Selection.CountSelectedRows() != 0)
                if (Message.Request(null, "Копіювати?") == ResponseType.Yes)
                {
                    SpinnerOn(NotebookFunction.GetNotebookFromWidget(this));
                    ToolButtonSensitive(sender, false);

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

                    if (Фільтр.IsFiltered)
                        await BeforeLoadRecords_OnFilter();
                    else
                        await BeforeLoadRecords();

                    ToolButtonSensitive(sender, true);
                }
        }

        void OnFilterClick(object? sender, EventArgs args)
        {
            if (!Фільтр.IsFilterCreated)
                Фільтр.CreatePopover((ToolButton)sender!);

            Фільтр.PopoverParent?.ShowAll();
        }

        void OnMultipleSelectClick(object? sender, EventArgs args)
        {
            if (CallBack_OnMultipleSelectPointer != null && TreeViewGrid.Selection.CountSelectedRows() != 0)
            {
                TreePath[] selectionRows = TreeViewGrid.Selection.GetSelectedRows();
                UnigueID[] unigueIDs = new UnigueID[selectionRows.Length];
                for (int i = 0; i < selectionRows.Length; i++)
                {
                    TreeViewGrid.Model.GetIter(out TreeIter iter, selectionRows[i]);
                    unigueIDs[i] = new UnigueID((string)TreeViewGrid.Model.GetValue(iter, 1));
                }

                CallBack_OnMultipleSelectPointer.Invoke(unigueIDs);
            }
        }

        async void OnVersionsHistoryClick(object? sender, EventArgs args)
        {
            ToolButtonSensitive(sender, false);

            foreach (TreePath itemPath in TreeViewGrid.Selection.GetSelectedRows())
            {
                TreeViewGrid.Model.GetIter(out TreeIter iter, itemPath);
                UnigueID unigueID = new UnigueID((string)TreeViewGrid.Model.GetValue(iter, 1));

                if (!unigueID.IsEmpty())
                    await VersionsHistory(unigueID);
            }

            ToolButtonSensitive(sender, true);
        }

        #endregion
    }
}