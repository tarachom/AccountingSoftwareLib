
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
/// Клас для виводу перелічень
/// </summary>
/// <param name="conf">Конфігурація</param>
/// <param name="activate">Процедура активації вітки в дереві</param>
public class ConfiguratorEnumsTree(Configuration conf, Action<string, string>? activate, ConfiguratorTree.ToolbarAction toolbar) : ConfiguratorTree(activate, toolbar)
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
        foreach (ConfigurationEnums enums in Conf.Enums.Values)
        {
            var row = ConfiguratorItemRow.New();
            row.Group = "Enums";
            row.Name = enums.Name;
            row.Obj = enums;

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
            case "Enums" when obj is ConfigurationEnums enums && enums.Fields.Count > 0:
                {
                    //Для перелічення заповнюю поля
                    foreach (ConfigurationEnumField field in enums.Fields.Values)
                    {
                        var row = ConfiguratorItemRow.New();
                        row.Group = "Field";
                        row.Name = field.Name;
                        row.Obj = field;
                        row.Desc = field.Value.ToString();

                        store.Append(row);
                    }

                    return store;
                }
            default:
                return null;
        }
    }
}