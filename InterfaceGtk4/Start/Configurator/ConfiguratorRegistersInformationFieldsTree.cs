
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
/// Клас для виводу регістрів інформації
/// </summary>
/// <param name="conf">Конфігурація</param>
/// <param name="activate">Процедура активації вітки в дереві</param>
public class ConfiguratorRegistersInformationFieldsTree(ConfigurationRegistersInformation reg, Action<ConfiguratorItemRow>? activate, ConfiguratorTree.ToolbarAction toolbar) : ConfiguratorTree(activate, toolbar)
{
    ConfigurationRegistersInformation Register { get; set; } = reg;

    /// <summary>
    /// Заповнення
    /// </summary>
    /// <returns>Бокс із заповненою таблицею</returns>
    public Box Fill()
    {
        FillGrid();
        return VBox;
    }

    protected override void FillGrid()
    {
        Store.RemoveAll();

        //Заповнення сховища
        {
            var row = ConfiguratorItemRow.New();
            row.Group = "DimensionFields";
            row.Name = "Виміри";
            row.Obj = Register;

            Store.Append(row);
        }

        {
            var row = ConfiguratorItemRow.New();
            row.Group = "ResourcesFields";
            row.Name = "Ресурси";
            row.Obj = Register;

            Store.Append(row);
        }

        {
            var row = ConfiguratorItemRow.New();
            row.Group = "PropertyFields";
            row.Name = "Реквізити";
            row.Obj = Register;

            Store.Append(row);
        }

        {
            var row = ConfiguratorItemRow.New();
            row.Group = "TabularListGroup";
            row.Name = "Табличні списки";
            row.Obj = Register;

            Store.Append(row);
        }

        {
            var row = ConfiguratorItemRow.New();
            row.Group = "FormGroup";
            row.Name = "Форми";
            row.Obj = Register;

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
            case "DimensionFields" when obj is ConfigurationRegistersInformation registers:
                {
                    //Виміри
                    foreach (ConfigurationField field in registers.DimensionFields.Values)
                    {
                        var row = ConfiguratorItemRow.New();
                        row.Group = "DimensionField";
                        row.Name = field.Name;
                        row.Obj = field;
                        row.ParentObj = registers;
                        row.TableOrField = field.NameInTable;
                        row.Type = field.Type;
                        row.Desc = field.Pointer;

                        store.Append(row);
                    }

                    return store;
                }
            case "ResourcesFields" when obj is ConfigurationRegistersInformation registers:
                {
                    //Русурси
                    foreach (ConfigurationField field in registers.ResourcesFields.Values)
                    {
                        var row = ConfiguratorItemRow.New();
                        row.Group = "ResourcesField";
                        row.Name = field.Name;
                        row.Obj = field;
                        row.ParentObj = registers;
                        row.TableOrField = field.NameInTable;
                        row.Type = field.Type;
                        row.Desc = field.Pointer;

                        store.Append(row);
                    }

                    return store;
                }
            case "PropertyFields" when obj is ConfigurationRegistersInformation registers:
                {
                    //Реквізити
                    foreach (ConfigurationField field in registers.PropertyFields.Values)
                    {
                        var row = ConfiguratorItemRow.New();
                        row.Group = "PropertyField";
                        row.Name = field.Name;
                        row.Obj = field;
                        row.ParentObj = registers;
                        row.TableOrField = field.NameInTable;
                        row.Type = field.Type;
                        row.Desc = field.Pointer;

                        store.Append(row);
                    }

                    return store;
                }
            case "TabularListGroup" when obj is ConfigurationRegistersInformation registers:
                {
                    //Для групи Табличні списки заповнюю саме табличні списки
                    foreach (ConfigurationTabularList tabularList in registers.TabularList.Values)
                    {
                        var row = ConfiguratorItemRow.New();
                        row.Group = "TabularList";
                        row.Name = tabularList.Name;
                        row.Obj = tabularList;
                        row.ParentObj = registers;

                        store.Append(row);
                    }

                    return store;
                }
            case "FormGroup" when obj is ConfigurationRegistersInformation registers:
                {
                    //Для групи Форми заповнюю саме форми
                    foreach (ConfigurationForms form in registers.Forms.Values)
                    {
                        var row = ConfiguratorItemRow.New();
                        row.Group = "Form";
                        row.Name = form.Name;
                        row.Obj = form;
                        row.ParentObj = registers;

                        store.Append(row);
                    }

                    return store;
                }
            default:
                return null;
        }
    }
}