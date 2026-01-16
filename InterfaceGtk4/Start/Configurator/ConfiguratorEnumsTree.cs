
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

using Gtk;
using AccountingSoftware;

namespace InterfaceGtk4;

public partial class ConfiguratorEnumsTree(Configuration conf, Action<string, string>? activate)
{
    Configuration Conf { get; set; } = conf;

    public Action<string, string>? Activate { get; set; } = activate;

    public Box Fill()
    {
        Box hBox = Box.New(Orientation.Horizontal, 0);

        //Сховище
        Gio.ListStore Store = Gio.ListStore.New(ConfiguratorItemRow.GetGType());

        //Заповнення сховища початковими даними
        foreach (ConfigurationEnums enums in Conf.Enums.Values)
            Store.Append(new ConfiguratorItemRow()
            {
                Group = "Enums",
                Name = enums.Name,
                Obj = enums
            });

        TreeListModel list = TreeListModel.New(Store, false, false, CreateFunc);
        SingleSelection model = SingleSelection.New(list);
        ColumnView columnView = ColumnView.New(model);

        //Дерево
        {
            SignalListItemFactory factory = SignalListItemFactory.New();
            factory.OnSetup += (_, args) =>
            {
                ListItem listItem = (ListItem)args.Object;
                var cell = LabelTablePartCell.New(null);

                TreeExpander expander = TreeExpander.New();
                expander.SetChild(cell);

                listItem.SetChild(expander);
            };

            factory.OnBind += (_, args) =>
            {
                ListItem listItem = (ListItem)args.Object;
                TreeListRow? row = (TreeListRow?)listItem.GetItem();
                if (row != null)
                {
                    TreeExpander? expander = (TreeExpander?)listItem.GetChild();
                    var cell = (LabelTablePartCell?)expander?.GetChild();
                    ConfiguratorItemRow? itemRow = (ConfiguratorItemRow?)row.GetItem();
                    if (expander != null && cell != null && itemRow != null)
                    {
                        expander.SetListRow(row);
                        cell.SetText(itemRow.Name);
                    }
                }
            };
            var column = ColumnViewColumn.New("Документи", factory);
            column.Resizable = true;
            columnView.AppendColumn(column);
        }

        //Значення
        {
            SignalListItemFactory factory = SignalListItemFactory.New();
            factory.OnSetup += (_, args) =>
            {
                var listItem = (ListItem)args.Object;
                var cell = LabelTablePartCell.New(null);
                listItem.SetChild(cell);

            };
            factory.OnBind += (_, args) =>
            {
                ListItem listItem = (ListItem)args.Object;
                TreeListRow? row = (TreeListRow?)listItem.GetItem();
                if (row != null)
                {
                    var cell = (LabelTablePartCell?)listItem.Child;
                    ConfiguratorItemRow? itemRow = (ConfiguratorItemRow?)row.GetItem();
                    if (cell != null && itemRow != null)
                        cell.SetText(itemRow.Desc);
                }
            };
            var column = ColumnViewColumn.New("Значення", factory);
            column.Resizable = true;
            columnView.AppendColumn(column);
        }

        //Пуста колонка для заповнення вільного простору
        {
            ColumnViewColumn column = ColumnViewColumn.New(null, null);
            column.Resizable = true;
            column.Expand = true;
            columnView.AppendColumn(column);
        }

        columnView.OnActivate += (_, _) =>
        {
            SingleSelection model = (SingleSelection)columnView.Model;
            TreeListRow? row = (TreeListRow?)model.GetSelectedItem();
            if (row != null)
            {
                ConfiguratorItemRow? itemRow = (ConfiguratorItemRow?)row.Item;
                if (itemRow != null)
                    Activate?.Invoke(itemRow.Group, itemRow.Name);
            }
        };

        ScrolledWindow scroll = new() ;
        scroll.Vexpand = scroll.Hexpand = true;
        scroll.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
        scroll.Child = columnView;

        hBox.Append(scroll);

        return hBox;
    }

    Gio.ListModel? CreateFunc(GObject.Object item)
    {
        ConfiguratorItemRow itemRow = (ConfiguratorItemRow)item;

        string group = itemRow.Group;
        object? obj = itemRow.Obj;

        Gio.ListStore Store = Gio.ListStore.New(ConfiguratorItemRow.GetGType());

        switch (group)
        {
            case "Enums" when obj is ConfigurationEnums enums && enums.Fields.Count > 0:
                {
                    //Для перелічення заповнюю поля
                    foreach (ConfigurationEnumField field in enums.Fields.Values)
                        Store.Append(new ConfiguratorItemRow()
                        {
                            Group = "Field",
                            Name = field.Name,
                            Obj = field,
                            Desc = field.Value.ToString()
                        });

                    return Store;
                }
            default:
                return null;
        }
    }
}