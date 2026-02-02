
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

public class ConfiguratorDocumentsTree(Configuration conf, Action<string, string>? activate)
{
    Configuration Conf { get; set; } = conf;

    public Action<string, string>? Activate { get; set; } = activate;

    //Сховище
    readonly Gio.ListStore Store = Gio.ListStore.New(ConfiguratorItemRow.GetGType());

    public Box Fill()
    {
        Box hBox = Box.New(Orientation.Horizontal, 0);

        //Заповнення сховища початковими даними
        foreach (ConfigurationDocuments documents in Conf.Documents.Values)
            Store.Append(new ConfiguratorItemRow()
            {
                Group = "Documents",
                Name = documents.Name,
                Key = documents.Name
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

                /*GestureClick gesture = GestureClick.New();
                expander.AddController(gesture);
                gesture.OnPressed += (_, args) =>
                {
                    Console.WriteLine("click");
                };*/

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
        if (item is not ConfiguratorItemRow) return null;
        ConfiguratorItemRow itemRow = (ConfiguratorItemRow)item;

        string group = itemRow.Group;
        string key = itemRow.Key;
        string key2 = itemRow.Key2;
        object? obj = itemRow.Obj;

        Console.WriteLine($"group {group}, obj {obj}");

        Gio.ListStore store = Gio.ListStore.New(ConfiguratorItemRow.GetGType());
        store.Ref();

        switch (group)
        {
            case "Documents":
                {
                    if (Conf.Documents.TryGetValue(key, out ConfigurationDocuments? document))
                    {
                        //Для документу заповнюю поля
                        foreach (ConfigurationField field in document.Fields.Values)
                            store.Append(new ConfiguratorItemRow()
                            {
                                Group = "Field",
                                Name = field.Name,
                                Key = field.Name,
                                //Obj = field,
                                Type = field.Type,
                                Desc = field.Pointer
                            });

                        if (document.TabularParts.Count > 0)
                            store.Append(new ConfiguratorItemRow()
                            {
                                Group = "TablePartGroup",
                                Name = "[ Табличні частини ]",
                                Key = key
                                //Obj = documents
                            });
                    }

                    return store;
                }
            case "TablePartGroup":
                {
                    if (Conf.Documents.TryGetValue(key, out ConfigurationDocuments? document))
                        //Для групи Табличні частини заповнюю саме табличні частини
                        foreach (ConfigurationTablePart tablePart in document.TabularParts.Values)
                            Store.Append(new ConfiguratorItemRow()
                            {
                                Group = "TablePart",
                                Name = tablePart.Name,
                                //Obj = tablePart,
                                Key = key,
                                Key2 = tablePart.Name
                            });

                    return store;
                }
            case "TablePart":
                {
                    if (Conf.Documents.TryGetValue(key, out ConfigurationDocuments? document))
                        if (document.TabularParts.TryGetValue(key2, out ConfigurationTablePart? tablePart))
                            //Для табличної частини заповнюю поля
                            foreach (ConfigurationField field in tablePart.Fields.Values)
                                store.Append(new ConfiguratorItemRow()
                                {
                                    Group = "TablePartField",
                                    Name = field.Name,
                                    Key = field.Name,
                                    //Obj = field,
                                    Type = field.Type,
                                    Desc = field.Pointer
                                });

                    return store;
                }
            default:
                return null;
        }
    }
}