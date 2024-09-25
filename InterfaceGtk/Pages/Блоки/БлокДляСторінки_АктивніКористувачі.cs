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

/*

Список активних користувачів

*/

using Gtk;
using AccountingSoftware;

namespace InterfaceGtk
{
    public class БлокДляСторінки_АктивніКористувачі : Форма
    {
        enum Columns
        {
            UID,
            UserUID,
            UserName,
            DateLogin,
            DateUp,
            Master
        }

        ListStore Store = new ListStore(
            typeof(string), //UID
            typeof(string), //UserUID
            typeof(string), //UserName
            typeof(string), //DateLogin
            typeof(string), //DateUp
            typeof(bool)    //Master
        );

        TreeView TreeViewGrid;

        Kernel Kernel { get; set; }

        public БлокДляСторінки_АктивніКористувачі(Kernel kernel) : base()
        {
            Kernel = kernel;

            Box hBoxCaption = new Box(Orientation.Horizontal, 0);
            hBoxCaption.PackStart(new Label("<b>Сесії користувачів</b>") { UseMarkup = true }, false, false, 5);
            PackStart(hBoxCaption, false, false, 5);

            TreeViewGrid = new TreeView(Store) { ActivateOnSingleClick = true };
            TreeViewGrid.Selection.Mode = SelectionMode.Multiple;

            ScrolledWindow scrollTree = new ScrolledWindow() { ShadowType = ShadowType.In, HeightRequest = 150 };
            scrollTree.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
            scrollTree.Add(TreeViewGrid);

            AddColumn();

            PackStart(scrollTree, false, false, 5);
            ShowAll();
        }

        public void AutoRefreshRun()
        {
            LoadRecordsAsync();
        }

        public async void LoadRecordsAsync()
        {
            while (true)
            {
                await LoadRecords();

                //Затримка на 5 сек
                await Task.Delay(5000);
            }
        }

        async ValueTask LoadRecords()
        {
            var recordResult = await Kernel.DataBase.SpetialTableActiveUsersSelect();

            Store.Clear();
            foreach (Dictionary<string, object> record in recordResult.ListRow)
            {
                string update = DateTime.Parse(record["dateupdate"].ToString() ?? DateTime.MinValue.ToString()).ToString("HH:mm:ss");

                Store.AppendValues(
                    record["uid"].ToString(),
                    record["usersuid"].ToString(),
                    record["username"].ToString(),
                    record["datelogin"].ToString(),
                    update,
                    record["master"]
                );
            }
        }

        #region TreeView

        void AddColumn()
        {
            TreeViewGrid.AppendColumn(new TreeViewColumn("UID", new CellRendererText(), "text", (int)Columns.UID) { Visible = false });
            TreeViewGrid.AppendColumn(new TreeViewColumn("UserUID", new CellRendererText(), "text", (int)Columns.UserUID) { Visible = false });
            TreeViewGrid.AppendColumn(new TreeViewColumn("Користувач", new CellRendererText(), "text", (int)Columns.UserName));
            TreeViewGrid.AppendColumn(new TreeViewColumn("Авторизація", new CellRendererText() { Xalign = 0.5f }, "text", (int)Columns.DateLogin) { Alignment = 0.5f });
            TreeViewGrid.AppendColumn(new TreeViewColumn("Підтвердження", new CellRendererText() { Xalign = 0.5f }, "text", (int)Columns.DateUp) { Alignment = 0.5f });
            TreeViewGrid.AppendColumn(new TreeViewColumn("Головний", new CellRendererToggle() { Xalign = 0.5f }, "active", (int)Columns.Master) { Alignment = 0.5f });

            //Пустишка
            TreeViewGrid.AppendColumn(new TreeViewColumn());
        }

        #endregion
    }
}