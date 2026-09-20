
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
/// 
/// </summary>
/// <param name="conf">Конфігурація</param>
/// <param name="activate">Процедура активації вітки в дереві</param>
public class ConfiguratorTablePartsFieldsTree(ConfigurationTablePart tp, Action<ConfiguratorItemRow>? activate, ConfiguratorTree.ToolbarAction toolbar) : ConfiguratorTree(activate, toolbar)
{
    ConfigurationTablePart TablePart { get; set; } = tp;

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
            row.Obj = TablePart;

            Store.Append(row);
        }

        {
            var row = ConfiguratorItemRow.New();
            row.Group = "TabularListGroup";
            row.Name = "Табличні списки";
            row.Obj = TablePart;

            Store.Append(row);
        }

        {
            var row = ConfiguratorItemRow.New();
            row.Group = "FormGroup";
            row.Name = "Форми";
            row.Obj = TablePart;

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
            case "FieldGroup" when obj is ConfigurationTablePart tablePart:
                {
                    //Для табчастини заповнюю поля
                    foreach (ConfigurationField field in tablePart.Fields.Values)
                    {
                        var row = ConfiguratorItemRow.New();
                        row.Group = "Field";
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
            case "TabularListGroup" when obj is ConfigurationTablePart tablePart:
                {
                    //Для групи Табличні списки заповнюю саме табличні списки
                    foreach (ConfigurationTabularList tabularList in tablePart.TabularList.Values)
                    {
                        var row = ConfiguratorItemRow.New();
                        row.Group = "TabularList";
                        row.Name = tabularList.Name;
                        row.Obj = tabularList;
                        row.ParentObj = tablePart;

                        store.Append(row);
                    }

                    return store;
                }
            case "FormGroup" when obj is ConfigurationTablePart tablePart:
                {
                    //Для групи Форми заповнюю саме форми
                    foreach (ConfigurationForms form in tablePart.Forms.Values)
                    {
                        var row = ConfiguratorItemRow.New();
                        row.Group = "Form";
                        row.Name = form.Name;
                        row.Obj = form;
                        row.ParentObj = tablePart;

                        store.Append(row);
                    }

                    return store;
                }
            default:
                return null;
        }
    }
}