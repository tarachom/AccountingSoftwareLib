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

Список активних користувачів

*/

using Gtk;
using AccountingSoftware;

namespace InterfaceGtk4;

/// <summary>
/// Список активних користувачів.
/// 
/// Клас не потребує наслідування і може зразу використовуватися.
/// </summary>
[GObject.Subclass<Box>]
public partial class ActiveUsersView : Box
{
    [GObject.Subclass<GObject.Object>]
    public partial class ItemRow
    {
        public static ItemRow New() => NewWithProperties([]);

        public UniqueID UniqueID { get; set; } = new();

        public Guid UserUID { get; set; } = Guid.Empty;

        public string UserName { get; set; } = "";

        public DateTime DateLogin { get; set; } = DateTime.MinValue;

        public DateTime DateUp { get; set; } = DateTime.MinValue;

        public bool Master { get; set; } = false;

        public string TypeForm { get; set; } = "";
    }

    Kernel? Kernel { get; set; } = null;
    Gio.ListStore Store { get; } = Gio.ListStore.New(ItemRow.GetGType());
    ColumnView Grid { get; set; } = ColumnView.New(null);
    ScrolledWindow Scroll { get; set; } = ScrolledWindow.New();

    partial void Initialize()
    {
        SetOrientation(Orientation.Vertical);

        Box hBoxCaption = New(Orientation.Horizontal, 0);
        hBoxCaption.MarginBottom = 5;
        Append(hBoxCaption);

        Label label = Label.New("<b>Сесії користувачів</b>");
        label.UseMarkup = true;
        hBoxCaption.Append(label);

        SingleSelection model = SingleSelection.New(Store);
        model.Autoselect = true;

        Grid = ColumnView.New(model);
        Columns();

        Scroll.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
        Scroll.SetChild(Grid);
        Append(Scroll);
    }

    public static ActiveUsersView New(Kernel kernel, int widthRequest, int heightRequest)
    {
        ActiveUsersView view = NewWithProperties([]);
        view.Scroll.WidthRequest = widthRequest;
        view.Scroll.HeightRequest = heightRequest;

        view.Kernel = kernel;
        view.Kernel.UpdateSession += async (_, _) => await LoadRecords(view);

        return view;
    }

    static async ValueTask LoadRecords(ActiveUsersView view)
    {
        if (view.Kernel == null) throw new Exception("Kernel null");

        var recordResult = await view.Kernel.DataBase.SpetialTableActiveUsersSelect();

        view.Store.RemoveAll();
        foreach (Dictionary<string, object> record in recordResult.ListRow)
        {
            var itemRow = ItemRow.New();
            itemRow.UniqueID = new UniqueID(record["uid"]);
            itemRow.UserUID = (Guid)record["usersuid"];
            itemRow.UserName = record["username"].ToString() ?? "";
            itemRow.DateLogin = DateTime.Parse(record["datelogin"].ToString() ?? DateTime.MinValue.ToString());
            itemRow.DateUp = DateTime.Parse(record["dateupdate"].ToString() ?? DateTime.MinValue.ToString());
            itemRow.Master = (bool)record["master"];
            itemRow.TypeForm = Kernel.TypeForm_Alias((TypeForm)record["type_form"]);

            view.Store.Append(itemRow);
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

        //Авторизація 
        {
            SignalListItemFactory factory = SignalListItemFactory.New();
            factory.OnSetup += OnSetup;
            factory.OnBind += OnBind_DateLogin;
            ColumnViewColumn column = ColumnViewColumn.New("Авторизація", factory);
            column.Resizable = true;
            Grid.AppendColumn(column);
        }

        //Підтвердження 
        {
            SignalListItemFactory factory = SignalListItemFactory.New();
            factory.OnSetup += OnSetup;
            factory.OnBind += OnBind_DateUp;
            ColumnViewColumn column = ColumnViewColumn.New("Підтвердження", factory);
            column.Resizable = true;
            Grid.AppendColumn(column);
        }

        //Розрахунок
        {
            SignalListItemFactory factory = SignalListItemFactory.New();
            factory.OnBind += OnBind_Master;
            ColumnViewColumn column = ColumnViewColumn.New("Розрахунок", factory);
            Grid.AppendColumn(column);
        }

        //Тип 
        {
            SignalListItemFactory factory = SignalListItemFactory.New();
            factory.OnSetup += OnSetup;
            factory.OnBind += OnBind_TypeForm;
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

    void OnBind_DateLogin(SignalListItemFactory factory, SignalListItemFactory.BindSignalArgs args)
    {
        ListItem listItem = (ListItem)args.Object;
        LabelTablePartCell? label = (LabelTablePartCell?)listItem.Child;
        ItemRow? itemrow = (ItemRow?)listItem.Item;
        if (label != null && itemrow != null)
            label.SetText(itemrow.DateLogin.ToString("dd.MM.yy HH:mm"));
    }

    void OnBind_DateUp(SignalListItemFactory factory, SignalListItemFactory.BindSignalArgs args)
    {
        ListItem listItem = (ListItem)args.Object;
        LabelTablePartCell? label = (LabelTablePartCell?)listItem.Child;
        ItemRow? itemrow = (ItemRow?)listItem.Item;
        if (label != null && itemrow != null)
            label.SetText(itemrow.DateUp.ToString("HH:mm:ss"));
    }

    void OnBind_Master(SignalListItemFactory factory, SignalListItemFactory.BindSignalArgs args)
    {
        ListItem listItem = (ListItem)args.Object;
        ItemRow? itemrow = (ItemRow?)listItem.Item;
        listItem.SetChild(ImageTablePartCell.NewFromPixbuf((itemrow?.Master ?? false) ? Icon.ForInformation.Check : null));
    }

    void OnBind_TypeForm(SignalListItemFactory factory, SignalListItemFactory.BindSignalArgs args)
    {
        ListItem listItem = (ListItem)args.Object;
        LabelTablePartCell? label = (LabelTablePartCell?)listItem.Child;
        ItemRow? itemrow = (ItemRow?)listItem.Item;
        if (label != null && itemrow != null)
            label.SetText(itemrow.TypeForm.ToString());
    }

    #endregion
}