/*
Copyright (C) 2019-2026 TARAKHOMYN YURIY IVANOVYCH
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

Список заблокованих об'єктів (відкриті для редагування)

*/

using Gtk;
using AccountingSoftware;

namespace InterfaceGtk4;

/// <summary>
/// Заблоковані об'єкти (відкриті для редагування).
/// 
/// Це клас є базовим і потребує наслідування і переоприділення віртуальних функцій.
/// </summary>
[GObject.Subclass<Box>]
public partial class LockObjectsView : Box
{
    [GObject.Subclass<GObject.Object>]
    public partial class ItemRow
    {
        public static ItemRow New() => NewWithProperties([]);

        public UniqueID UniqueID { get; set; } = new();

        public Guid UserUID { get; set; } = Guid.Empty;

        public string UserName { get; set; } = "";

        public UuidAndText ObjValue { get; set; } = new();

        public string ObjPresentation { get; set; } = "";

        public DateTime DateLock { get; set; } = DateTime.MinValue;
    }

    Kernel Kernel { get; set; }
    Dictionary<Guid, (CompositePointerPresentation_Record Record, DateTime DateExpire)> CacheData { get; set; } = [];
    Gio.ListStore Store { get; } = Gio.ListStore.New(ItemRow.GetGType());
    ColumnView Grid { get; set; } = ColumnView.New(null);

    public void Init(Kernel kernel, int widthRequest, int heightRequest)
    {
        SetOrientation(Orientation.Vertical);

        Kernel = kernel;
        Kernel.UpdateSession += async (_, _) => await LoadRecords();

        Box hBoxCaption = New(Orientation.Horizontal, 0);
        hBoxCaption.MarginBottom = 5;
        Append(hBoxCaption);

        Label label = Label.New("<b>Заблоковані об'єкти</b>");
        label.UseMarkup = true;
        hBoxCaption.Append(label);

        SingleSelection model = SingleSelection.New(Store);
        model.Autoselect = true;

        Grid = ColumnView.New(model);
        Columns();

        ScrolledWindow Scroll = ScrolledWindow.New();
        Scroll.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
        Scroll.WidthRequest = widthRequest;
        Scroll.HeightRequest = heightRequest;
        Scroll.SetChild(Grid);
        Append(Scroll);
    }

    #region Virtual Function

    protected virtual ValueTask<CompositePointerPresentation_Record> CompositePointerPresentation(UuidAndText uuidAndText) =>
        ValueTask.FromResult<CompositePointerPresentation_Record>(new());

    #endregion

    async ValueTask LoadRecords()
    {
        var recordResult = await Kernel.DataBase.SpetialTableLockedObjectSelect();

        //Очистка через певний час
        foreach (Guid remove in CacheData.Select(x => x.Value.DateExpire < DateTime.Now ? x.Key : Guid.Empty))
            if (remove != Guid.Empty && CacheData.ContainsKey(remove))
                CacheData.Remove(remove);

        Store.RemoveAll();
        foreach (Dictionary<string, object> record in recordResult.ListRow)
        {
            UuidAndText obj = (UuidAndText)record["obj"];

            if (!CacheData.TryGetValue(obj.Uuid, out (CompositePointerPresentation_Record Record, DateTime DateExpire) presentation))
            {
                presentation.Record = await CompositePointerPresentation(obj);
                presentation.DateExpire = DateTime.Now.AddMinutes(5); // 5 хвилин в кеші

                CacheData.Add(obj.Uuid, presentation);
            }

            var itemRow = ItemRow.New();
            itemRow.UniqueID = new UniqueID(record["session"]);
            itemRow.UserUID = (Guid)record["users"];
            itemRow.UserName = record["username"].ToString() ?? "";
            itemRow.ObjValue = obj;
            itemRow.ObjPresentation = SubstringName(presentation.Record.result);
            itemRow.DateLock = DateTime.Parse(record["datelock"].ToString() ?? DateTime.MinValue.ToString());

            Store.Append(itemRow);
        }
    }

    void Columns()
    {
        //Користувач 
        {
            SignalListItemFactory factory = SignalListItemFactory.New();
            factory.OnSetup += OnSetup;
            factory.OnBind += OnBind_UserName;
            ColumnViewColumn column = ColumnViewColumn.New("Користувач", factory);
            column.Resizable = true;
            Grid.AppendColumn(column);
        }

        //Об'єкт 
        {
            SignalListItemFactory factory = SignalListItemFactory.New();
            factory.OnSetup += OnSetup;
            factory.OnBind += OnBind_ObjPresentation;
            ColumnViewColumn column = ColumnViewColumn.New("Об'єкт", factory);
            column.Resizable = true;
            Grid.AppendColumn(column);
        }

        //Дата блокування 
        {
            SignalListItemFactory factory = SignalListItemFactory.New();
            factory.OnSetup += OnSetup;
            factory.OnBind += OnBind_DateLock;
            ColumnViewColumn column = ColumnViewColumn.New("Дата", factory);
            column.Resizable = true;
            Grid.AppendColumn(column);
        }

        //Тип 
        {
            SignalListItemFactory factory = SignalListItemFactory.New();
            factory.OnSetup += OnSetup;
            factory.OnBind += OnBind_Type;
            ColumnViewColumn column = ColumnViewColumn.New("Тип", factory);
            column.Resizable = true;
            Grid.AppendColumn(column);
        }

        //Пустишка
        {
            ColumnViewColumn column = ColumnViewColumn.New(null, null);
            column.Resizable = true;
            column.Expand = true;
            Grid.AppendColumn(column);
        }
    }

    #region OnSetup

    void OnSetup(SignalListItemFactory factory, SignalListItemFactory.SetupSignalArgs args)
    {
        ListItem listItem = (ListItem)args.Object;
        LabelTablePartCell label = LabelTablePartCell.New();
        listItem.Child = label;
    }

    #endregion

    #region OnBind

    void OnBind_UserName(SignalListItemFactory factory, SignalListItemFactory.BindSignalArgs args)
    {
        ListItem listItem = (ListItem)args.Object;
        LabelTablePartCell? label = (LabelTablePartCell?)listItem.Child;
        ItemRow? itemrow = (ItemRow?)listItem.Item;
        if (label != null && itemrow != null)
            label.SetText(itemrow.UserName.ToString());
    }

    void OnBind_ObjPresentation(SignalListItemFactory factory, SignalListItemFactory.BindSignalArgs args)
    {
        ListItem listItem = (ListItem)args.Object;
        LabelTablePartCell? label = (LabelTablePartCell?)listItem.Child;
        ItemRow? itemrow = (ItemRow?)listItem.Item;
        if (label != null && itemrow != null)
            label.SetText(itemrow.ObjPresentation);
    }

    void OnBind_DateLock(SignalListItemFactory factory, SignalListItemFactory.BindSignalArgs args)
    {
        ListItem listItem = (ListItem)args.Object;
        LabelTablePartCell? label = (LabelTablePartCell?)listItem.Child;
        ItemRow? itemrow = (ItemRow?)listItem.Item;
        if (label != null && itemrow != null)
            label.SetText(itemrow.DateLock.ToString("HH:mm:ss"));
    }

    void OnBind_Type(SignalListItemFactory factory, SignalListItemFactory.BindSignalArgs args)
    {
        ListItem listItem = (ListItem)args.Object;
        LabelTablePartCell? label = (LabelTablePartCell?)listItem.Child;
        ItemRow? itemrow = (ItemRow?)listItem.Item;
        if (label != null && itemrow != null)
            label.SetText(itemrow.ObjValue.Text);
    }

    #endregion

    static string SubstringName(string name) => name.Length >= 102 ? name[..100] + " ..." : name;
}