
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
            Store.Append(new ConfiguratorItemRow()
            {
                Group = "Block",
                Name = block.BlockName,
                Obj = block
            });
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
                        store.Append(new ConfiguratorItemRow()
                        {
                            Group = "Const",
                            Name = constant.Name,
                            Obj = constant,
                            Type = constant.Type,
                            Desc = constant.Pointer
                        });

                    return store;
                }
            case "Const" when obj is ConfigurationConstants constant && constant.TabularParts.Count > 0:
                {
                    //Для константи Група Табличні частини
                    store.Append(new ConfiguratorItemRow()
                    {
                        Group = "TablePartGroup",
                        Name = "[ Табличні частини ]",
                        Obj = constant
                    });

                    return store;
                }
            case "TablePartGroup" when obj is ConfigurationConstants constant:
                {
                    //Для групи Табличні частини заповнюю саме табличні частини
                    foreach (ConfigurationTablePart tablePart in constant.TabularParts.Values)
                        store.Append(new ConfiguratorItemRow()
                        {
                            Group = "TablePart",
                            Name = tablePart.Name,
                            Obj = tablePart
                        });

                    return store;
                }
            case "TablePart" when obj is ConfigurationTablePart tablePart:
                {
                    //Для табличної частини заповнюю поля
                    foreach (ConfigurationField field in tablePart.Fields.Values)
                        store.Append(new ConfiguratorItemRow()
                        {
                            Group = "TablePartField",
                            Name = field.Name,
                            Obj = field,
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