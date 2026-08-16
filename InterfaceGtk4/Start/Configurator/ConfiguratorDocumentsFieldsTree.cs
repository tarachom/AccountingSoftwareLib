
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

/// <summary>
/// Клас для виводу довідників
/// </summary>
/// <param name="conf">Конфігурація</param>
/// <param name="activate">Процедура активації вітки в дереві</param>
public class ConfiguratorDocumentsFieldsTree(ConfigurationDocuments doc, Action<ConfiguratorItemRow>? activate, ConfiguratorTree.ToolbarAction toolbar) : ConfiguratorTree(activate, toolbar)
{
    ConfigurationDocuments Documents { get; set; } = doc;

    /// <summary>
    /// Заповнення
    /// </summary>
    /// <returns>Бокс із заповненою таблицею</returns>
    public Box Fill()
    {
        FillGrid();
        return VBox;
    }

    /// <summary>
    /// Відкриття верхніх віток
    /// </summary>
    /*
    void Open()
    {
        if (TreeList != null)
        {
            List<TreeListRow> rows = [];
            for (uint i = 0; i < TreeList.GetNItems(); i++)
            {
                TreeListRow? row = TreeList.GetRow(i);
                if (row != null) rows.Add(row);
            }

            foreach (var row in rows)
                row.Expanded = true;
        }
    }
    */

    protected override void FillGrid()
    {
        Store.RemoveAll();

        //Заповнення сховища
        {
            var row = ConfiguratorItemRow.New();
            row.Group = "FieldGroup";
            row.Name = "Поля";
            row.Obj = Documents;

            Store.Append(row);
        }

        {
            var row = ConfiguratorItemRow.New();
            row.Group = "TablePartGroup";
            row.Name = "Табличні частини";
            row.Obj = Documents;

            Store.Append(row);
        }

        {
            var row = ConfiguratorItemRow.New();
            row.Group = "TabularListGroup";
            row.Name = "Табличні списки";
            row.Obj = Documents;

            Store.Append(row);
        }

        {
            var row = ConfiguratorItemRow.New();
            row.Group = "FormsGroup";
            row.Name = "Форми";
            row.Obj = Documents;

            Store.Append(row);
        }
    }

    protected override Gio.ListModel? CreateFunc(GObject.Object item)
    {
        if (item is not ConfiguratorItemRow) return null;
        ConfiguratorItemRow itemRow = (ConfiguratorItemRow)item;

        string group = itemRow.Group;
        object? obj = itemRow.Obj;

        Gio.ListStore store = Gio.ListStore.New(ConfiguratorItemRow.GetGType());

        switch (group)
        {
            case "FieldGroup" when obj is ConfigurationDocuments documents:
                {
                    //Для документу заповнюю поля
                    foreach (ConfigurationField field in documents.Fields.Values)
                    {
                        var row = ConfiguratorItemRow.New();
                        row.Group = "Field";
                        row.Name = field.Name;
                        row.Obj = field;
                        row.ParentObj = documents;
                        row.TableOrField = field.NameInTable;
                        row.Type = field.Type;
                        row.Desc = field.Pointer;

                        store.Append(row);
                    }

                    return store;
                }
            case "TablePartGroup" when obj is ConfigurationDocuments documents:
                {
                    //Для групи Табличні частини заповнюю саме табличні частини
                    foreach (ConfigurationTablePart tablePart in documents.TabularParts.Values)
                    {
                        var row = ConfiguratorItemRow.New();
                        row.Group = "TablePart";
                        row.Name = tablePart.Name;
                        row.Obj = tablePart;
                        row.ParentObj = documents;
                        row.TableOrField = tablePart.Table;

                        store.Append(row);
                    }

                    return store;
                }
            case "TabularListGroup" when obj is ConfigurationDocuments documents:
                {
                    //Для групи Табличні списки заповнюю саме табличні списки
                    foreach (ConfigurationTabularList tabularList in documents.TabularList.Values)
                    {
                        var row = ConfiguratorItemRow.New();
                        row.Group = "TabularList";
                        row.Name = tabularList.Name;
                        row.Obj = tabularList;
                        row.ParentObj = documents;

                        store.Append(row);
                    }

                    return store;
                }
            case "FormsGroup" when obj is ConfigurationDocuments documents:
                {
                    //Для групи Форми заповнюю саме форми
                    foreach (ConfigurationForms form in documents.Forms.Values)
                    {
                        var row = ConfiguratorItemRow.New();
                        row.Group = "Form";
                        row.Name = form.Name;
                        row.Obj = form;
                        row.ParentObj = documents;

                        store.Append(row);
                    }

                    return store;
                }
            case "TablePart" when obj is ConfigurationTablePart tablePart:
                {
                    //Для табличної частини заповнюю поля
                    foreach (ConfigurationField field in tablePart.Fields.Values)
                    {
                        var row = ConfiguratorItemRow.New();
                        row.Group = "TablePartField";
                        row.Name = field.Name;
                        row.Obj = field;
                        row.ParentObj = tablePart;
                        row.TableOrField = field.NameInTable;
                        row.Type = field.Type;
                        row.Desc = field.Pointer;

                        store.Append(row);
                    }

                    return store;
                }
            default:
                return null;
        }
    }
}