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
            Master,
            TypeForm
        }

        ListStore Store = new ListStore(
            typeof(string), //UID
            typeof(string), //UserUID
            typeof(string), //UserName
            typeof(string), //DateLogin
            typeof(string), //DateUp
            typeof(bool),   //Master
            typeof(string)  //TypeForm
        );

        TreeView TreeViewGrid;

        Kernel Kernel { get; set; }

        public БлокДляСторінки_АктивніКористувачі(Kernel kernel) : base()
        {
            Kernel = kernel;
            Kernel.UpdateSession += async (object? sender, EventArgs args) => await LoadRecords();

            Box hBoxCaption = new Box(Orientation.Horizontal, 0);
            hBoxCaption.PackStart(new Label("<b>Сесії користувачів</b>") { UseMarkup = true }, false, false, 5);
            PackStart(hBoxCaption, false, false, 5);

            TreeViewGrid = new TreeView(Store) { ActivateOnSingleClick = true };
            TreeViewGrid.Selection.Mode = SelectionMode.Multiple;

            ScrolledWindow scrollTree = new ScrolledWindow() { ShadowType = ShadowType.In };
            scrollTree.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
            scrollTree.Add(TreeViewGrid);

            AddColumn();

            PackStart(scrollTree, true, true, 5);
            ShowAll();
        }

        async ValueTask LoadRecords()
        {
            var recordResult = await Kernel.DataBase.SpetialTableActiveUsersSelect();

            Store.Clear();
            foreach (Dictionary<string, object> record in recordResult.ListRow)
            {
                /*
                DateTime datelogin = DateTime.Parse(record["datelogin"].ToString() ?? DateTime.MinValue.ToString());

                TimeSpan loginTime = DateTime.Now - datelogin;
                int days = loginTime.Days, hours = loginTime.Hours, minutes = loginTime.Minutes, seconds = loginTime.Seconds;

                string login = (days > 0 ? days + " дн. " : "") + (hours > 0 ? hours + " год. " : "") + (minutes > 0 ? minutes + " хв." : "") +
                    (days == 0 && hours == 0 && minutes == 0 && seconds > 0 ? seconds + " сек." : "");
                */

                string login = DateTime.Parse(record["datelogin"].ToString() ?? DateTime.MinValue.ToString()).ToString("dd.MM.yy HH:mm");
                string update = DateTime.Parse(record["dateupdate"].ToString() ?? DateTime.MinValue.ToString()).ToString("HH:mm:ss");

                Store.AppendValues(
                    record["uid"].ToString(),
                    record["usersuid"].ToString(),
                    record["username"].ToString(),
                    login,
                    update,
                    record["master"],
                    Kernel.TypeForm_Alias((TypeForm)record["type_form"])
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
            TreeViewGrid.AppendColumn(new TreeViewColumn("Розрахунок", new CellRendererToggle() { Xalign = 0.5f }, "active", (int)Columns.Master) { Alignment = 0.5f });
            TreeViewGrid.AppendColumn(new TreeViewColumn("Тип", new CellRendererText(), "text", (int)Columns.TypeForm));

            //Пустишка
            TreeViewGrid.AppendColumn(new TreeViewColumn());
        }

        #endregion
    }
}