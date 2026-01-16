
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

public partial class ConfiguratorRegistersInformationTree(Configuration conf, Action<string, string>? activate)
{
    Configuration Conf { get; set; } = conf;

    public Action<string, string>? Activate { get; set; } = activate;

    public Box Fill()
    {
        Box hBox = Box.New(Orientation.Horizontal, 0);

        //Сховище
        Gio.ListStore Store = Gio.ListStore.New(ConfiguratorItemRow.GetGType());

        //Заповнення сховища початковими даними
        foreach (ConfigurationRegistersInformation registers in Conf.RegistersInformation.Values)
            Store.Append(new ConfiguratorItemRow()
            {
                Group = "RegistersInformation",
                Name = registers.Name,
                Obj = registers
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
            var column = ColumnViewColumn.New("Регістри інформації", factory);
            column.Resizable = true;
            columnView.AppendColumn(column);
        }

        //Тип даних
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
                        cell.SetText(itemRow.Type);
                }
            };
            var column = ColumnViewColumn.New("Тип даних", factory);
            column.Resizable = true;
            columnView.AppendColumn(column);
        }

        //Деталізація
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
            var column = ColumnViewColumn.New("Деталізація", factory);
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

        ScrolledWindow scroll = new();
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
            case "RegistersInformation" when obj is ConfigurationRegistersInformation registers:
                {
                    Store.Append(new ConfiguratorItemRow()
                    {
                        Group = "DimensionFields",
                        Name = "Виміри",
                        Obj = registers
                    });

                    Store.Append(new ConfiguratorItemRow()
                    {
                        Group = "ResourcesFields",
                        Name = "Ресурси",
                        Obj = registers
                    });

                    Store.Append(new ConfiguratorItemRow()
                    {
                        Group = "PropertyFields",
                        Name = "Реквізити",
                        Obj = registers
                    });

                    return Store;
                }
            case "DimensionFields" when obj is ConfigurationRegistersInformation registers:
                {
                    //Виміри
                    foreach (ConfigurationField field in registers.DimensionFields.Values)
                        Store.Append(new ConfiguratorItemRow()
                        {
                            Group = "DimensionField",
                            Name = field.Name,
                            Obj = field,
                            Type = field.Type,
                            Desc = field.Pointer
                        });

                    return Store;
                }
            case "ResourcesFields" when obj is ConfigurationRegistersInformation registers:
                {
                    //Русурси
                    foreach (ConfigurationField field in registers.ResourcesFields.Values)
                        Store.Append(new ConfiguratorItemRow()
                        {
                            Group = "ResourcesField",
                            Name = field.Name,
                            Obj = field,
                            Type = field.Type,
                            Desc = field.Pointer
                        });

                    return Store;
                }
            case "PropertyFields" when obj is ConfigurationRegistersInformation registers:
                {
                    //Реквізити
                    foreach (ConfigurationField field in registers.PropertyFields.Values)
                        Store.Append(new ConfiguratorItemRow()
                        {
                            Group = "PropertyField",
                            Name = field.Name,
                            Obj = field,
                            Type = field.Type,
                            Desc = field.Pointer
                        });

                    return Store;
                }
            default:
                return null;
        }
    }
}