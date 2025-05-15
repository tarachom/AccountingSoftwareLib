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
    public abstract class ДокументЖурнал : ФормаЖурнал
    {
        /// <summary>
        /// Для вибору і позиціювання
        /// </summary>
        public UnigueID? DocumentPointerItem { get; set; }

        /// <summary>
        /// Функція зворотнього виклику при виборі
        /// </summary>
        public Action<UnigueID>? CallBack_OnSelectPointer { get; set; }

        /// <summary>
        /// Верхній набір меню
        /// </summary>
        protected Toolbar ToolbarTop = new Toolbar();

        /// <summary>
        /// Верхній бок для додаткових кнопок
        /// </summary>
        protected Box HBoxTop = new Box(Orientation.Horizontal, 0);

        /// <summary>
        /// Період
        /// </summary>
        protected PeriodControl Період = new PeriodControl();

        /// <summary>
        /// Пошук
        /// </summary>
        SearchControl Пошук = new SearchControl();

        /// <summary>
        /// Фільтр
        /// </summary>
        protected ListFilterControl Фільтр = new ListFilterControl(true);

        /// <summary>
        /// Додатковий ключ форми журналу для налаштувань
        /// Використовується для ідентифікації форми яка відкрита наприклад із звіту
        /// </summary>
        public string KeyForSetting { get; set; } = "";

        public ДокументЖурнал()
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

            //Фільтр
            Фільтр.Select = async () => await BeforeLoadRecords_OnFilter();
            Фільтр.Clear = async () => await BeforeLoadRecords();
            Фільтр.FillFilterList = FillFilterList;
            Фільтр.Період = Період;

            CreateToolbar();

            TreeViewGrid.RowActivated += OnRowActivated;
            TreeViewGrid.ButtonPressEvent += OnButtonPressEvent;
            TreeViewGrid.ButtonReleaseEvent += OnButtonReleaseEvent;
            TreeViewGrid.KeyReleaseEvent += OnKeyReleaseEvent;

            //Сторінки
            AddPages(new Сторінки.Налаштування() { Тип = Сторінки.ТипЖурналу.Документи });

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

            //Separator
            ToolItem toolItemSeparator = new ToolItem { new Separator(Orientation.Horizontal) };
            ToolbarTop.Add(toolItemSeparator);

            ToolButton filterButton = new ToolButton(new Image(Stock.SortAscending, IconSize.Menu), "Фільтрувати") { TooltipText = "Фільтрувати" };
            filterButton.Clicked += OnFilterClick;
            ToolbarTop.Add(filterButton);

            MenuToolButton provodkyButton = new MenuToolButton(new Image(Stock.Find, IconSize.Menu), "Проводки") { IsImportant = true };
            provodkyButton.Clicked += OnReportSpendTheDocumentClick;
            provodkyButton.Menu = ToolbarProvodkySubMenu();
            ToolbarTop.Add(provodkyButton);

            Menu? menuItem = ToolbarNaOsnoviSubMenu();
            if (menuItem != null)
            {
                MenuToolButton naOsnoviButton = new MenuToolButton(new Image(Stock.New, IconSize.Menu), "Ввести на основі") { IsImportant = true };
                naOsnoviButton.Clicked += (sender, arg) => { ((Menu)((MenuToolButton)sender!).Menu).Popup(); };
                naOsnoviButton.Menu = menuItem;
                ToolbarTop.Add(naOsnoviButton);
            }

            MenuToolButton printingButton = new MenuToolButton(new Image(Stock.Print, IconSize.Menu), "Друк") { IsImportant = true };
            printingButton.Clicked += (sender, arg) => { ((Menu)((MenuToolButton)sender!).Menu).Popup(); };
            printingButton.Menu = ToolbarPrintingSubMenu();
            ToolbarTop.Add(printingButton);

            //Експорт
            MenuToolButton exportButton = new MenuToolButton(new Image(Stock.Convert, IconSize.Menu), "Експорт") { IsImportant = true };
            exportButton.Clicked += (sender, arg) => { ((Menu)((MenuToolButton)sender!).Menu).Popup(); };
            exportButton.Menu = ToolbarExportSubMenu();
            ToolbarTop.Add(exportButton);

            ToolButton versionshistoryButton = new ToolButton(new Image(Stock.FindAndReplace, IconSize.Menu), "Історія зміни даних") { TooltipText = "Історія зміни даних" };
            versionshistoryButton.Clicked += OnVersionsHistoryClick;
            ToolbarTop.Add(versionshistoryButton);
        }

        Menu ToolbarProvodkySubMenu()
        {
            Menu Menu = new Menu();

            MenuItem spendTheDocumentButton = new MenuItem("Провести документ");
            spendTheDocumentButton.Activated += (sender, args) => SpendTheDocumentOrClear(true);
            Menu.Append(spendTheDocumentButton);

            MenuItem clearSpendButton = new MenuItem("Відмінити проведення");
            clearSpendButton.Activated += (sender, args) => SpendTheDocumentOrClear(false);
            Menu.Append(clearSpendButton);

            Menu.ShowAll();

            return Menu;
        }

        Menu PopUpContextMenu()
        {
            Menu Menu = new Menu();

            MenuItem spendTheDocumentButton = new MenuItem("Провести документ");
            spendTheDocumentButton.Activated += (sender, args) => SpendTheDocumentOrClear(true);
            Menu.Append(spendTheDocumentButton);

            MenuItem clearSpendButton = new MenuItem("Відмінити проведення");
            clearSpendButton.Activated += (sender, args) => SpendTheDocumentOrClear(false);
            Menu.Append(clearSpendButton);

            MenuItem setDeletionLabel = new MenuItem("Помітка на видалення");
            setDeletionLabel.Activated += OnDeleteClick;
            Menu.Append(setDeletionLabel);

            Menu.ShowAll();

            return Menu;
        }

        Menu ToolbarPrintingSubMenu()
        {
            Menu Menu = new Menu();

            MenuItem printButton = new MenuItem("Документ");
            printButton.Activated += OnPrintingInvoiceClick;
            Menu.Append(printButton);

            PrintingSubMenu(Menu);

            Menu.ShowAll();

            return Menu;
        }

        Menu ToolbarExportSubMenu()
        {
            Menu Menu = new Menu();

            MenuItem exportXMLButton = new MenuItem("Формат XML");
            exportXMLButton.Activated += OnExportXMLClick;
            Menu.Append(exportXMLButton);

            MenuItem exportExcelButton = new MenuItem("Формат Excel");
            exportExcelButton.Activated += OnExportExcelClick;
            Menu.Append(exportExcelButton);

            Menu.ShowAll();

            return Menu;
        }

        #endregion

        #region Virtual & Abstract Function

        protected virtual Menu? ToolbarNaOsnoviSubMenu() { return null; }

        protected virtual void PrintingSubMenu(Menu Menu) { }

        protected abstract ValueTask OpenPageElement(bool IsNew, UnigueID? unigueID = null);

        protected abstract ValueTask SetDeletionLabel(UnigueID unigueID);

        protected abstract ValueTask<UnigueID?> Copy(UnigueID unigueID);

        protected virtual async void CallBack_LoadRecords(UnigueID? selectPointer)
        {
            SelectPointerItem = selectPointer;
            await BeforeLoadRecords();
        }

        protected virtual void FillFilterList(ListFilterControl filterControl) { }

        protected abstract void PeriodChanged();

        protected abstract ValueTask SpendTheDocument(UnigueID unigueID, bool spendDoc);

        protected abstract void ReportSpendTheDocument(UnigueID unigueID);

        protected virtual bool IsExportXML() { return false; } //Дозволити експорт

        protected virtual async ValueTask ExportXML(UnigueID unigueID, string pathToFolder) { await ValueTask.FromResult(true); }

        protected virtual bool IsExportExcel() { return false; } //Дозволити експорт

        protected virtual async ValueTask ExportExcel(UnigueID unigueID, string pathToFolder) { await ValueTask.FromResult(true); }

        protected virtual async ValueTask PrintingDoc(UnigueID unigueID) { await ValueTask.FromResult(true); }

        protected virtual async ValueTask VersionsHistory(UnigueID unigueID) { await ValueTask.FromResult(true); }

        #endregion

        #region TreeView

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

                    if (DocumentPointerItem == null)
                        await OpenPageElement(false, unigueID);
                    else
                    {
                        CallBack_OnSelectPointer?.Invoke(unigueID);
                        Notebook? notebook = NotebookFunction.GetNotebookFromWidget(this);
                        if (notebook != null) NotebookFunction.CloseNotebookPageToCode(notebook, this.Name);
                        PopoverParent?.Hide();
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
                if (Message.Request(null, "Встановити або зняти помітку на видалення?") == ResponseType.Yes)
                {
                    ToolButtonSensitive(sender, false);

                    TreePath[] selectionRows = TreeViewGrid.Selection.GetSelectedRows();
                    foreach (TreePath itemPath in selectionRows)
                    {
                        TreeViewGrid.Model.GetIter(out TreeIter iter, itemPath);
                        UnigueID unigueID = new UnigueID((string)TreeViewGrid.Model.GetValue(iter, 1));

                        await SetDeletionLabel(unigueID);

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

        void OnFilterClick(object? sender, EventArgs args)
        {
            if (!Фільтр.IsFilterCreated)
                Фільтр.CreatePopover((ToolButton)sender!);

            Фільтр.PopoverParent?.ShowAll();
        }

        void OnReportSpendTheDocumentClick(object? sender, EventArgs args)
        {
            if (TreeViewGrid.Selection.CountSelectedRows() != 0)
                if (TreeViewGrid.Model.GetIter(out TreeIter iter, TreeViewGrid.Selection.GetSelectedRows()[0]))
                {
                    UnigueID unigueID = new UnigueID((string)TreeViewGrid.Model.GetValue(iter, 1));
                    ReportSpendTheDocument(unigueID);
                }
        }

        async void SpendTheDocumentOrClear(bool spend)
        {
            if (TreeViewGrid.Selection.CountSelectedRows() != 0)
            {
                TreePath[] selectionRows = TreeViewGrid.Selection.GetSelectedRows();
                foreach (TreePath itemPath in selectionRows)
                {
                    TreeViewGrid.Model.GetIter(out TreeIter iter, itemPath);
                    UnigueID unigueID = new UnigueID((string)TreeViewGrid.Model.GetValue(iter, 1));

                    await SpendTheDocument(unigueID, spend);

                    SelectPointerItem = unigueID;
                }

                await BeforeLoadRecords();
            }
        }

        async void OnExportXMLClick(object? sender, EventArgs arg)
        {
            if (IsExportXML() && TreeViewGrid.Selection.CountSelectedRows() != 0)
            {
                string pathToFolder = "";

                FileChooserDialog fc = new FileChooserDialog("Виберіть каталог", null,
                    FileChooserAction.SelectFolder, "Закрити", ResponseType.Cancel, "Вибрати", ResponseType.Accept);

                if (fc.Run() == (int)ResponseType.Accept)
                    pathToFolder = fc.CurrentFolder;

                fc.Dispose();
                fc.Destroy();

                if (!string.IsNullOrEmpty(pathToFolder) && System.IO.Directory.Exists(pathToFolder))
                    foreach (TreePath itemPath in TreeViewGrid.Selection.GetSelectedRows())
                    {
                        TreeViewGrid.Model.GetIter(out TreeIter iter, itemPath);
                        UnigueID unigueID = new UnigueID((string)TreeViewGrid.Model.GetValue(iter, 1));

                        await ExportXML(unigueID, pathToFolder);
                    }
            }
        }

        async void OnExportExcelClick(object? sender, EventArgs arg)
        {
            if (IsExportExcel() && TreeViewGrid.Selection.CountSelectedRows() != 0)
            {
                string pathToFolder = "";

                FileChooserDialog fc = new FileChooserDialog("Виберіть каталог", null,
                    FileChooserAction.SelectFolder, "Закрити", ResponseType.Cancel, "Вибрати", ResponseType.Accept);

                if (fc.Run() == (int)ResponseType.Accept)
                    pathToFolder = fc.CurrentFolder;

                fc.Dispose();
                fc.Destroy();

                if (!string.IsNullOrEmpty(pathToFolder) && System.IO.Directory.Exists(pathToFolder))
                    foreach (TreePath itemPath in TreeViewGrid.Selection.GetSelectedRows())
                    {
                        TreeViewGrid.Model.GetIter(out TreeIter iter, itemPath);
                        UnigueID unigueID = new UnigueID((string)TreeViewGrid.Model.GetValue(iter, 1));

                        await ExportExcel(unigueID, pathToFolder);
                    }
            }
        }

        async void OnPrintingInvoiceClick(object? sender, EventArgs arg)
        {
            if (TreeViewGrid.Selection.CountSelectedRows() != 0)
                foreach (TreePath itemPath in TreeViewGrid.Selection.GetSelectedRows())
                {
                    TreeViewGrid.Model.GetIter(out TreeIter iter, itemPath);
                    UnigueID unigueID = new UnigueID((string)TreeViewGrid.Model.GetValue(iter, 1));

                    await PrintingDoc(unigueID);
                }
        }

        async void OnVersionsHistoryClick(object? sender, EventArgs args)
        {
            ToolButtonSensitive(sender, false);

            foreach (TreePath itemPath in TreeViewGrid.Selection.GetSelectedRows())
            {
                TreeViewGrid.Model.GetIter(out TreeIter iter, itemPath);
                UnigueID unigueID = new UnigueID((string)TreeViewGrid.Model.GetValue(iter, 1));

                await VersionsHistory(unigueID);
            }

            ToolButtonSensitive(sender, true);
        }

        #endregion
    }
}