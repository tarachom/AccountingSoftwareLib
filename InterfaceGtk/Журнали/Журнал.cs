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
using System.Reflection;
using AccountingSoftware;

namespace InterfaceGtk
{
    public abstract class Журнал : ФормаЖурнал
    {
        /// <summary>
        /// Для позиціювання в списку
        /// </summary>
        public UnigueID? SelectPointerItem { get; set; }

        /// <summary>
        /// Період
        /// </summary>
        protected PeriodControl Період = new PeriodControl();

        /// <summary>
        /// Дерево
        /// </summary>
        protected TreeView TreeViewGrid = new TreeView();

        /// <summary>
        /// Список документів
        /// </summary>
        protected ToolButton? TypeDocToolButton;

        /// <summary>
        /// Прокрутка дерева
        /// </summary>
        protected ScrolledWindow ScrollTree = new ScrolledWindow() { ShadowType = ShadowType.In };

        #region Динамічне створення обєктів

        protected abstract Assembly ExecutingAssembly { get; }
        protected abstract string NameSpageCodeGeneration { get; }

        #endregion

        public Журнал() : base()
        {
            Box HBoxTop = new Box(Orientation.Horizontal, 0);
            PackStart(HBoxTop, false, false, 10);

            //Період
            Період.Changed = PeriodChanged;
            HBoxTop.PackStart(Період, false, false, 2);

            CreateToolbar();

            ScrollTree.SetPolicy(PolicyType.Never, PolicyType.Automatic);

            TreeViewGrid.Selection.Mode = SelectionMode.Multiple;
            TreeViewGrid.ActivateOnSingleClick = true;
            TreeViewGrid.RowActivated += OnRowActivated;
            TreeViewGrid.ButtonPressEvent += OnButtonPressEvent;
            TreeViewGrid.ButtonReleaseEvent += OnButtonReleaseEvent;
            TreeViewGrid.KeyReleaseEvent += OnKeyReleaseEvent;

            ScrollTree.Add(TreeViewGrid);

            PackStart(ScrollTree, true, true, 0);

            ShowAll();
        }

        public async ValueTask SetValue()
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

            ToolButton refreshButton = new ToolButton(new Image(Stock.Refresh, IconSize.Menu), "Обновити") { TooltipText = "Обновити" };
            refreshButton.Clicked += OnRefreshClick;
            toolbar.Add(refreshButton);

            ToolButton deleteButton = new ToolButton(new Image(Stock.Delete, IconSize.Menu), "Видалити") { TooltipText = "Видалити" };
            deleteButton.Clicked += OnDeleteClick;
            toolbar.Add(deleteButton);

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

            MenuItem spendTheDocumentButton = new MenuItem("Провести документ");
            spendTheDocumentButton.Activated += OnSpendTheDocument;
            Menu.Append(spendTheDocumentButton);

            MenuItem clearSpendButton = new MenuItem("Відмінити проведення");
            clearSpendButton.Activated += OnClearSpend;
            Menu.Append(clearSpendButton);

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

        protected virtual async ValueTask BeforeSetValue() { await ValueTask.FromResult(true); }

        public virtual void LoadRecords() { }

        public virtual void OpenTypeListDocs(Widget relative_to) { }

        public virtual void PeriodChanged() { }

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

                    // ФункціїДляДокументів.ВідкритиДокументВідповідноДоВиду(typeDoc, new UnigueID(uid),
                    //     Enum.Parse<ПеріодДляЖурналу.ТипПеріоду>(ComboBoxPeriodWhere.ActiveId));
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
                        LoadRecords();
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

        void OnRefreshClick(object? sender, EventArgs args)
        {
            LoadRecords();
        }

        void OnTypeDocsClick(object? sender, EventArgs args)
        {
            OpenTypeListDocs((ToolButton)sender!);
        }

        void OnDeleteClick(object? sender, EventArgs args)
        {
            if (TreeViewGrid.Selection.CountSelectedRows() != 0)
            {
                if (Message.Request(null, "Встановити або зняти помітку на видалення?") == ResponseType.Yes)
                {
                    TreePath[] selectionRows = TreeViewGrid.Selection.GetSelectedRows();
                    foreach (TreePath itemPath in selectionRows)
                    {
                        TreeViewGrid.Model.GetIter(out TreeIter iter, itemPath);

                        string uid = (string)TreeViewGrid.Model.GetValue(iter, 1);
                        string typeDoc = (string)TreeViewGrid.Model.GetValue(iter, 2);

                        UnigueID unigueID = new UnigueID(uid);

                        object? docObjest = ExecutingAssembly.CreateInstance($"{NameSpageCodeGeneration}.Документи.{typeDoc}_Objest");
                        if (docObjest != null)
                        {
                            /*
                            Використовується виклик асинхронних методів через синхронні які додатково згенеровані
                            Тобто функція Read викликається через ReadSync, хотя можна викликати Read, 
                            але є проблеми з отриманням результату, треба подумати
                            */
                            object? readObj = docObjest.GetType().InvokeMember("ReadSync", BindingFlags.InvokeMethod, null, docObjest, [unigueID, false]);
                            if (readObj != null && (bool)readObj)
                            {
                                bool DeletionLabel = (bool)(docObjest.GetType().GetProperty("DeletionLabel")?.GetValue(docObjest) ?? false);
                                docObjest.GetType().InvokeMember("SetDeletionLabelSync", BindingFlags.InvokeMethod, null, docObjest, [!DeletionLabel]);

                                SelectPointerItem = new UnigueID(uid);
                            }
                            else
                                Message.Error(null, "Не вдалось прочитати!");
                        }
                    }

                    LoadRecords();
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
                    {
                        // NotebookFunction.CreateNotebookPage(Program.GeneralNotebook,$"Проводки", () =>
                        // {
                        //     Звіт_РухДокументівПоРегістрах page = new Звіт_РухДокументівПоРегістрах();
                        //     page.CreateReport((DocumentPointer)documentPointer);

                        //     return page;
                        // });
                    }
                }
        }

        #endregion

        #region Проведення

        void SpendTheDocument(string uid, string typeDoc, bool spendDoc)
        {
            UnigueID unigueID = new UnigueID(uid);

            object? documentObjest = ExecutingAssembly.CreateInstance($"{NameSpageCodeGeneration}.Документи.{typeDoc}_Objest");
            if (documentObjest != null)
            {
                object? readObj = documentObjest.GetType().InvokeMember("ReadSync", BindingFlags.InvokeMethod, null, documentObjest, [unigueID, true]);
                if (readObj != null && (bool)readObj)
                {
                    if (spendDoc)
                    {
                        DateTime dateDoc = (DateTime)(documentObjest.GetType().GetProperty("ДатаДок")?.GetValue(documentObjest) ?? DateTime.MinValue);

                        object? documentObjestSpend = documentObjest.GetType().InvokeMember("SpendTheDocumentSync", BindingFlags.InvokeMethod, null, documentObjest, [dateDoc]);
                        // if (documentObjestSpend != null && !(bool)documentObjestSpend)
                        //     ФункціїДляПовідомлень.ПоказатиПовідомлення(unigueID);
                    }
                    else
                        documentObjest.GetType().InvokeMember("ClearSpendTheDocumentSync", BindingFlags.InvokeMethod, null, documentObjest, null);
                }
                else
                    Message.Error(null, "Не вдалось прочитати!");
            }
        }

        //
        // Проведення або очищення проводок для вибраних документів
        //

        void SpendTheDocumentOrClear(bool spend)
        {
            if (TreeViewGrid.Selection.CountSelectedRows() != 0)
            {
                TreePath[] selectionRows = TreeViewGrid.Selection.GetSelectedRows();
                foreach (TreePath itemPath in selectionRows)
                {
                    TreeViewGrid.Model.GetIter(out TreeIter iter, itemPath);

                    string uid = (string)TreeViewGrid.Model.GetValue(iter, 1);
                    string typeDoc = (string)TreeViewGrid.Model.GetValue(iter, 2);

                    SpendTheDocument(uid, typeDoc, spend);
                }

                LoadRecords();
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