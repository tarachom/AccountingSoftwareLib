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

public class ActiveUsersView : Box
{
    class ItemRow: Row
    {
        public Guid UserUID { get; set; } = Guid.Empty;

        public string UserName { get; set; } = "";

        public DateTime DateLogin { get; set; } = DateTime.MinValue;

        public DateTime DateUp { get; set; } = DateTime.MinValue;

        public bool Master { get; set; } = false;

        public string TypeForm { get; set; } = "";
    }

    Kernel Kernel { get; set; }

    Gio.ListStore Store { get; } = Gio.ListStore.New(ItemRow.GetGType());

    ColumnView Grid { get; }

    public ActiveUsersView(Kernel kernel, int widthRequest = 800, int heightRequest = 500)
    {
        SetOrientation(Orientation.Vertical);

        Kernel = kernel;
        Kernel.UpdateSession += async (sender, args) => await LoadRecords();

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

        ScrolledWindow scroll = ScrolledWindow.New();
        scroll.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
        scroll.WidthRequest = widthRequest;
        scroll.HeightRequest = heightRequest;
        scroll.SetChild(Grid);
        Append(scroll);
    }

    async ValueTask LoadRecords()
    {
        var recordResult = await Kernel.DataBase.SpetialTableActiveUsersSelect();

        Store.RemoveAll();
        foreach (Dictionary<string, object> record in recordResult.ListRow)
        {
            ItemRow itemRow = new()
            {
                UniqueID = new UniqueID(record["uid"]),
                UserUID = (Guid)record["usersuid"],
                UserName = record["username"].ToString() ?? "",
                DateLogin = DateTime.Parse(record["datelogin"].ToString() ?? DateTime.MinValue.ToString()),
                DateUp = DateTime.Parse(record["dateupdate"].ToString() ?? DateTime.MinValue.ToString()),
                Master = (bool)record["master"],
                TypeForm = Kernel.TypeForm_Alias((TypeForm)record["type_form"])
            };

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
        LabelTablePartCell label = LabelTablePartCell.New(null);
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
        listItem.SetChild(ImageTablePartCell.NewForPixbuf((itemrow?.Master ?? false) ? Icon.ForInformation.Check : null));
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