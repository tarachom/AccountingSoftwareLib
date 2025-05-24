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
    public abstract class СпільніФорми_ІсторіяЗміниДаних_Список : Форма
    {
        enum Columns
        {
            Image,
            VersionID,
            UserID,
            DateWrite,
            UserName,
            Operation,
            Info
        }

        ListStore Store = new ListStore(
            typeof(Gdk.Pixbuf), //Image
            typeof(string), //VersionID
            typeof(string), //UserID
            typeof(string), //DateWrite
            typeof(string), //UserName
            typeof(string), //Operation
            typeof(string)  //Info
        );

        TreeView TreeViewGrid;

        /// <summary>
        /// Верхній набір меню
        /// </summary>
        Toolbar ToolbarTop = new Toolbar();

        Kernel Kernel { get; set; }

        public СпільніФорми_ІсторіяЗміниДаних_Список(Kernel kernel) : base()
        {
            Kernel = kernel;

            TreeViewGrid = new TreeView(Store) { ActivateOnSingleClick = true };
            TreeViewGrid.Selection.Mode = SelectionMode.Multiple;
            TreeViewGrid.ButtonPressEvent += OnButtonPressEvent;

            ScrolledWindow scrollTree = new ScrolledWindow() { ShadowType = ShadowType.In };
            scrollTree.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
            scrollTree.Add(TreeViewGrid);

            AddColumn();

            CreateToolbar();

            PackStart(scrollTree, true, true, 0);
            ShowAll();
        }

        #region Toolbar & Menu

        void CreateToolbar()
        {
            PackStart(ToolbarTop, false, false, 0);

            ToolButton deleteButton = new ToolButton(new Image(Stock.Delete, IconSize.Menu), "Видалити") { TooltipText = "Видалити" };
            deleteButton.Clicked += OnDeleteClick;
            ToolbarTop.Add(deleteButton);

            ToolButton upButton = new ToolButton(new Image(Stock.Edit, IconSize.Menu), "Переглянути") { TooltipText = "Переглянути" };
            upButton.Clicked += OnOpenElementClick;
            ToolbarTop.Add(upButton);

            ToolButton refreshButton = new ToolButton(new Image(Stock.Refresh, IconSize.Menu), "Обновити") { TooltipText = "Обновити" };
            refreshButton.Clicked += OnRefreshClick;
            ToolbarTop.Add(refreshButton);
        }

        #endregion

        public async ValueTask Load()
        {
            if (!Obj.IsEmpty())
            {
                SelectVersionsHistoryList_Record recordResult = await Kernel.DataBase.SpetialTableObjectVersionsHistoryList(Obj);
                Store.Clear();

                if (recordResult.Result)
                    foreach (var row in recordResult.ListRow)
                        Store.AppendValues(
                            Іконки.ДляТабличногоСписку.Normal,
                            row.VersionID.ToString(),
                            row.UserID.ToString(),
                            row.DateWrite.ToString(),
                            row.UserName,
                            row.Operation switch { 'A' => "Додано новий", 'U' => "Записано", 'E' => "Зміни в табличних частинах", _ => "" },
                            row.Info
                        );
            }
        }

        public UuidAndText Obj { get; set; } = new UuidAndText();

        #region Virtual & Abstract Function

        protected abstract ValueTask OpenElement(Guid versionID);

        #endregion

        #region TreeView

        void AddColumn()
        {
            TreeViewGrid.AppendColumn(new TreeViewColumn("", new CellRendererPixbuf(), "pixbuf", (int)Columns.Image));
            TreeViewGrid.AppendColumn(new TreeViewColumn("VersionID", new CellRendererText(), "text", (int)Columns.VersionID) { Visible = false });
            TreeViewGrid.AppendColumn(new TreeViewColumn("UserID", new CellRendererText(), "text", (int)Columns.UserID) { Visible = false });
            TreeViewGrid.AppendColumn(new TreeViewColumn("Дата", new CellRendererText(), "text", (int)Columns.DateWrite));
            TreeViewGrid.AppendColumn(new TreeViewColumn("Користувач", new CellRendererText(), "text", (int)Columns.UserName));
            TreeViewGrid.AppendColumn(new TreeViewColumn("Операція", new CellRendererText(), "text", (int)Columns.Operation));
            TreeViewGrid.AppendColumn(new TreeViewColumn("Інформація", new CellRendererText(), "text", (int)Columns.Info));

            //Пустишка
            TreeViewGrid.AppendColumn(new TreeViewColumn());
        }

        async void OnButtonPressEvent(object? sender, ButtonPressEventArgs args)
        {
            if (args.Event.Type == Gdk.EventType.DoubleButtonPress && TreeViewGrid.Selection.CountSelectedRows() != 0)
                if (TreeViewGrid.Model.GetIter(out TreeIter iter, TreeViewGrid.Selection.GetSelectedRows()[0]))
                {
                    Guid versionID = Guid.Parse((string)TreeViewGrid.Model.GetValue(iter, (int)Columns.VersionID));
                    await OpenElement(versionID);
                }
        }

        #endregion

        #region ToolBar

        async void OnRefreshClick(object? sender, EventArgs args)
        {
            await Load();
        }

        async void OnOpenElementClick(object? sender, EventArgs args)
        {
            if (TreeViewGrid.Selection.CountSelectedRows() != 0)
            {
                TreePath[] selectionRows = TreeViewGrid.Selection.GetSelectedRows();
                foreach (TreePath itemPath in selectionRows)
                    if (TreeViewGrid.Model.GetIter(out TreeIter iter, itemPath))
                    {
                        Guid versionID = Guid.Parse((string)TreeViewGrid.Model.GetValue(iter, (int)Columns.VersionID));
                        await OpenElement(versionID);
                    }
            }
        }

        async void OnDeleteClick(object? sender, EventArgs args)
        {
            if (TreeViewGrid.Selection.CountSelectedRows() != 0)
                if (Message.Request(null, "Видалити?") == ResponseType.Yes)
                {
                    foreach (TreePath itemPath in TreeViewGrid.Selection.GetSelectedRows())
                    {
                        TreeViewGrid.Model.GetIter(out TreeIter iter, itemPath);
                        UnigueID versionID = new UnigueID((string)TreeViewGrid.Model.GetValue(iter, (int)Columns.VersionID));

                        await Kernel.DataBase.SpetialTableObjectVersionsHistoryRemove(versionID.UGuid, Obj);
                    }

                    await Load();
                }
        }

        #endregion
    }
}