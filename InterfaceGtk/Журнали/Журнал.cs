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
using System.Reflection;
using AccountingSoftware;

namespace InterfaceGtk
{
    public abstract class Журнал : ФормаЖурнал
    {
        /// <summary>
        /// Період
        /// </summary>
        protected PeriodControl Період = new PeriodControl();

        /// <summary>
        /// Список документів
        /// </summary>
        protected ToolButton? TypeDocToolButton;

        #region Динамічне створення обєктів

        private Assembly ExecutingAssembly { get; } = Assembly.GetCallingAssembly();
        private string NameSpageCodeGeneration { get; set; }

        #endregion

        public Журнал(string nameSpageCodeGeneration)
        {
            NameSpageCodeGeneration = nameSpageCodeGeneration;

            Box HBoxTop = new Box(Orientation.Horizontal, 0);
            PackStart(HBoxTop, false, false, 10);

            //Період
            Період.Changed = PeriodChanged;
            HBoxTop.PackStart(Період, false, false, 2);

            CreateToolbar();

            TreeViewGrid.RowActivated += OnRowActivated;
            TreeViewGrid.ButtonPressEvent += OnButtonPressEvent;
            TreeViewGrid.ButtonReleaseEvent += OnButtonReleaseEvent;
            TreeViewGrid.KeyReleaseEvent += OnKeyReleaseEvent;

            ScrollTree.Add(TreeViewGrid);

            PackStart(ScrollTree, true, true, 0);

            PackStart(ScrollPages, false, true, 0);

            ShowAll();
        }

        public override async ValueTask SetValue()
        {
            await BeforeSetValue();
        }

        #region Menu

        void CreateToolbar()
        {
            Toolbar toolbar = new Toolbar();
            PackStart(toolbar, false, false, 0);

            ToolButton findButton = new ToolButton(new Image(Stock.GoUp, IconSize.Menu), "Знайти в журналі") { TooltipText = "Знайти в журналі" };
            findButton.Clicked += OFindClick;
            toolbar.Add(findButton);

            ToolButton deleteButton = new ToolButton(new Image(Stock.Delete, IconSize.Menu), "Видалити") { TooltipText = "Видалити" };
            deleteButton.Clicked += OnDeleteClick;
            toolbar.Add(deleteButton);

            ToolButton refreshButton = new ToolButton(new Image(Stock.Refresh, IconSize.Menu), "Обновити") { TooltipText = "Обновити" };
            refreshButton.Clicked += OnRefreshClick;
            toolbar.Add(refreshButton);

            //Separator
            ToolItem toolItemSeparator = new ToolItem { new Separator(Orientation.Horizontal) };
            toolbar.Add(toolItemSeparator);

            TypeDocToolButton = new ToolButton(new Image(Stock.Index, IconSize.Menu), "Документи") { IsImportant = true };
            TypeDocToolButton.Clicked += OnTypeDocsClick;
            toolbar.Add(TypeDocToolButton);

            MenuToolButton provodkyButton = new MenuToolButton(new Image(Stock.Find, IconSize.Menu), "Проводки") { IsImportant = true };
            provodkyButton.Clicked += OnReportSpendTheDocumentClick;
            provodkyButton.Menu = ToolbarProvodkySubMenu();
            toolbar.Add(provodkyButton);
        }

        Menu ToolbarProvodkySubMenu()
        {
            Menu Menu = new Menu();

            MenuItem spend = new MenuItem("Провести документ");
            spend.Activated += OnSpendTheDocument;
            Menu.Append(spend);

            MenuItem clear = new MenuItem("Відмінити проведення");
            clear.Activated += OnClearSpend;
            Menu.Append(clear);

            Menu.ShowAll();

            return Menu;
        }

        Menu PopUpContextMenu()
        {
            Menu Menu = new Menu();

            MenuItem findButton = new MenuItem("Знайти в журналі");
            findButton.Activated += OFindClick;
            Menu.Append(findButton);

            MenuItem refreshButton = new MenuItem("Обновити");
            refreshButton.Activated += OnRefreshClick;
            Menu.Append(refreshButton);

            MenuItem deleteButton = new MenuItem("Помітка на видалення");
            deleteButton.Activated += OnDeleteClick;
            Menu.Append(deleteButton);

            MenuItem provodkyButton = new MenuItem("Проводки");
            provodkyButton.Activated += OnReportSpendTheDocumentClick;
            Menu.Append(provodkyButton);

            MenuItem spendTheDocumentButton = new MenuItem("Провести документ");
            spendTheDocumentButton.Activated += OnSpendTheDocument;
            Menu.Append(spendTheDocumentButton);

            MenuItem clearSpendButton = new MenuItem("Відмінити проведення");
            clearSpendButton.Activated += OnClearSpend;
            Menu.Append(clearSpendButton);

            Menu.ShowAll();

            return Menu;
        }

        #endregion

        #region Virtual & Abstract Function

        protected abstract void OpenTypeListDocs(Widget relative_to);

        protected abstract void PeriodChanged();

        protected abstract void ReportSpendTheDocument(DocumentPointer documentPointer);

        protected abstract void ErrorSpendTheDocument(UnigueID unigueID);

        protected abstract void OpenDoc(string typeDoc, UnigueID unigueID);

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
                    string uid = (string)TreeViewGrid.Model.GetValue(iter, 1);
                    SelectPointerItem = new UnigueID(uid);

                    PopUpContextMenu().Popup();
                }
        }

        void OnButtonPressEvent(object? sender, ButtonPressEventArgs args)
        {
            if (args.Event.Type == Gdk.EventType.DoubleButtonPress)
                OpenDocJournal();
        }

        void OpenDocJournal()
        {
            if (TreeViewGrid.Selection.CountSelectedRows() != 0)
                if (TreeViewGrid.Model.GetIter(out TreeIter iter, TreeViewGrid.Selection.GetSelectedRows()[0]))
                {
                    string uid = (string)TreeViewGrid.Model.GetValue(iter, 1);
                    string typeDoc = (string)TreeViewGrid.Model.GetValue(iter, 2);
                    OpenDoc(typeDoc, new UnigueID(uid));
                }
        }

        void OnKeyReleaseEvent(object? sender, KeyReleaseEventArgs args)
        {
            switch (args.Event.Key)
            {
                case Gdk.Key.Insert:
                    {
                        if (TypeDocToolButton != null)
                            OnTypeDocsClick(TypeDocToolButton, new EventArgs());
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
                        OpenDocJournal();
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

        void OFindClick(object? sender, EventArgs args)
        {
            OpenDocJournal();
        }

        async void OnRefreshClick(object? sender, EventArgs args)
        {
            ToolButtonSensitive(sender, false);

            ClearPages();
            await LoadRecords();

            ToolButtonSensitive(sender, true);
        }

        void OnTypeDocsClick(object? sender, EventArgs args)
        {
            OpenTypeListDocs((ToolButton)sender!);
        }

        async void OnDeleteClick(object? sender, EventArgs args)
        {
            if (TreeViewGrid.Selection.CountSelectedRows() != 0)
            {
                if (Message.Request(null, "Встановити або зняти помітку на видалення?") == ResponseType.Yes)
                {
                    ToolButtonSensitive(sender, false);

                    TreePath[] selectionRows = TreeViewGrid.Selection.GetSelectedRows();
                    foreach (TreePath itemPath in selectionRows)
                    {
                        TreeViewGrid.Model.GetIter(out TreeIter iter, itemPath);

                        string uid = (string)TreeViewGrid.Model.GetValue(iter, 1);
                        string typeDoc = (string)TreeViewGrid.Model.GetValue(iter, 2);

                        //Використовується другий конструктор (UnigueID uid, Dictionary<string, object>? fields = null)
                        object? docPointerInstance = ExecutingAssembly.CreateInstance($"{NameSpageCodeGeneration}.Документи.{typeDoc}_Pointer",
                            false, BindingFlags.CreateInstance, null, [new UnigueID(uid), null!], null, null);

                        if (docPointerInstance != null)
                        {
                            dynamic docPointer = docPointerInstance;
                            bool? label = await docPointer.GetDeletionLabel();
                            if (label.HasValue) await docPointer.SetDeletionLabel(!label.Value);
                        }
                    }

                    await LoadRecords();
                    ToolButtonSensitive(sender, true);
                }
            }
        }

        void OnReportSpendTheDocumentClick(object? sender, EventArgs args)
        {
            if (TreeViewGrid.Selection.CountSelectedRows() != 0)
                if (TreeViewGrid.Model.GetIter(out TreeIter iter, TreeViewGrid.Selection.GetSelectedRows()[0]))
                {
                    string uid = (string)TreeViewGrid.Model.GetValue(iter, 1);
                    string typeDoc = (string)TreeViewGrid.Model.GetValue(iter, 2);

                    UnigueID unigueID = new UnigueID(uid);
                    object? documentPointer = ExecutingAssembly.CreateInstance($"{NameSpageCodeGeneration}.Документи.{typeDoc}_Pointer", false, BindingFlags.CreateInstance, null, [unigueID, null!], null, null);
                    if (documentPointer != null)
                        ReportSpendTheDocument((DocumentPointer)documentPointer);
                }
        }

        #endregion

        #region Проведення

        async ValueTask SpendTheDocument(string uid, string typeDoc, bool spendDoc)
        {
            UnigueID unigueID = new UnigueID(uid);

            object? documentObjestInstance = ExecutingAssembly.CreateInstance($"{NameSpageCodeGeneration}.Документи.{typeDoc}_Objest");
            if (documentObjestInstance != null)
            {
                dynamic documentObjest = documentObjestInstance;
                if (await documentObjest.Read(unigueID, true))
                {
                    if (spendDoc)
                    {
                        if (!await documentObjest.SpendTheDocument(documentObjest.ДатаДок))
                            ErrorSpendTheDocument(unigueID);
                    }
                    else
                        await documentObjest.ClearSpendTheDocument();
                }
                else
                    Message.Error(null, "Не вдалось прочитати!");
            }
        }

        //
        // Проведення або очищення проводок для вибраних документів
        //

        async void SpendTheDocumentOrClear(bool spend)
        {
            if (TreeViewGrid.Selection.CountSelectedRows() != 0)
            {
                TreePath[] selectionRows = TreeViewGrid.Selection.GetSelectedRows();
                foreach (TreePath itemPath in selectionRows)
                {
                    TreeViewGrid.Model.GetIter(out TreeIter iter, itemPath);

                    string uid = (string)TreeViewGrid.Model.GetValue(iter, 1);
                    string typeDoc = (string)TreeViewGrid.Model.GetValue(iter, 2);

                    await SpendTheDocument(uid, typeDoc, spend);
                }

                await LoadRecords();
            }
        }

        void OnSpendTheDocument(object? sender, EventArgs args)
        {
            SpendTheDocumentOrClear(true);
        }

        void OnClearSpend(object? sender, EventArgs args)
        {
            SpendTheDocumentOrClear(false);
        }

        #endregion
    }
}