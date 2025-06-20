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

Список заблокованих об'єктів

*/

using Gtk;
using AccountingSoftware;

namespace InterfaceGtk
{
    public abstract class БлокДляСторінки_ЗаблокованіОбєкти : Форма
    {
        enum Columns
        {
            Session,
            UserUID,
            ObjValue,
            DateLock,
            UserName,
            ObjType
        }

        ListStore Store = new ListStore(
            typeof(string), //Session
            typeof(string), //UserUID
            typeof(string), //ObjValue
            typeof(string), //DateLock
            typeof(string), //UserName
            typeof(string)  //ObjType
        );

        TreeView TreeViewGrid;

        Kernel Kernel { get; set; }

        Dictionary<Guid, (CompositePointerPresentation_Record Record, DateTime DateExpire)> CacheData = [];

        public БлокДляСторінки_ЗаблокованіОбєкти(Kernel kernel) : base()
        {
            Kernel = kernel;
            Kernel.UpdateSession += async (sender, args) => await LoadRecords();

            Box hBoxCaption = new Box(Orientation.Horizontal, 0);
            hBoxCaption.PackStart(new Label("<b>Заблоковані об'єкти</b>") { UseMarkup = true }, false, false, 5);
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

        #region Virtual & Abstract Function

        protected abstract ValueTask<CompositePointerPresentation_Record> CompositePointerPresentation(UuidAndText uuidAndText);

        #endregion

        async ValueTask LoadRecords()
        {
            var recordResult = await Kernel.DataBase.SpetialTableLockedObjectSelect();

            //Очистка через певний час
            foreach (Guid remove in CacheData.Select(x => x.Value.DateExpire < DateTime.Now ? x.Key : Guid.Empty))
                if (remove != Guid.Empty && CacheData.ContainsKey(remove))
                    CacheData.Remove(remove);

            Store.Clear();
            foreach (Dictionary<string, object> record in recordResult.ListRow)
            {
                string datelock = DateTime.Parse(record["datelock"].ToString() ?? DateTime.MinValue.ToString()).ToString("HH:mm:ss");
                UuidAndText obj = (UuidAndText)record["obj"];

                if (!CacheData.TryGetValue(obj.Uuid, out (CompositePointerPresentation_Record Record, DateTime DateExpire) presentation))
                {
                    presentation.Record = await CompositePointerPresentation(obj);
                    presentation.DateExpire = DateTime.Now.AddMinutes(5); // 5 хвилин в кеші

                    CacheData.Add(obj.Uuid, presentation);
                }

                Store.AppendValues(
                    record["session"].ToString(),
                    record["users"].ToString(),
                    presentation.Record.result,
                    datelock,
                    record["username"].ToString(),
                    obj.Text
                );
            }
        }

        #region TreeView

        void AddColumn()
        {
            TreeViewGrid.AppendColumn(new TreeViewColumn("Session", new CellRendererText(), "text", (int)Columns.Session) { Visible = false });
            TreeViewGrid.AppendColumn(new TreeViewColumn("UserUID", new CellRendererText(), "text", (int)Columns.UserUID) { Visible = false });
            TreeViewGrid.AppendColumn(new TreeViewColumn("Об'єкт", new CellRendererText(), "text", (int)Columns.ObjValue));
            TreeViewGrid.AppendColumn(new TreeViewColumn("Дата", new CellRendererText(), "text", (int)Columns.DateLock) { Alignment = 0.5f });
            TreeViewGrid.AppendColumn(new TreeViewColumn("Користувач", new CellRendererText(), "text", (int)Columns.UserName));
            TreeViewGrid.AppendColumn(new TreeViewColumn("Тип", new CellRendererText(), "text", (int)Columns.ObjType));

            //Пустишка
            TreeViewGrid.AppendColumn(new TreeViewColumn());
        }

        #endregion
    }
}