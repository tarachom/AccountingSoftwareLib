
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
/// Клас для виводу констант
/// </summary>
/// <param name="conf">Конфігурація</param>
/// <param name="activate">Процедура активації вітки в дереві</param>
public class ConfiguratorConstantsTree(Configuration conf, Action<string, string>? activate, ConfiguratorTree.ToolbarAction toolbar) : ConfiguratorTree(activate, toolbar)
{
    Configuration Conf { get; set; } = conf;

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
        foreach (ConfigurationConstantsBlock block in Conf.ConstantsBlock.Values)
        {
            var row = ConfiguratorItemRow.New();
            row.Group = "Block";
            row.Name = block.BlockName;
            row.Obj = block;

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
        _ = store.Ref();

        switch (group)
        {
            case "Block" when obj is ConfigurationConstantsBlock block:
                {
                    //Для блоку заповнюємо контстанти
                    foreach (ConfigurationConstants constant in block.Constants.Values)
                    {
                        var row = ConfiguratorItemRow.New();
                        row.Group = "Const";
                        row.Name = constant.Name;
                        row.Obj = constant;
                        row.Type = constant.Type;
                        row.Desc = constant.Pointer;

                        store.Append(row);
                    }

                    return store;
                }
            case "Const" when obj is ConfigurationConstants constant && constant.TabularParts.Count > 0:
                {
                    var row = ConfiguratorItemRow.New();
                    row.Group = "TablePartGroup";
                    row.Name = "[ Табличні частини ]";
                    row.Obj = constant;

                    //Для константи Група Табличні частини
                    store.Append(row);

                    return store;
                }
            case "TablePartGroup" when obj is ConfigurationConstants constant:
                {
                    //Для групи Табличні частини заповнюю саме табличні частини
                    foreach (ConfigurationTablePart tablePart in constant.TabularParts.Values)
                    {
                        var row = ConfiguratorItemRow.New();
                        row.Group = "TablePart";
                        row.Name = tablePart.Name;
                        row.Obj = tablePart;

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