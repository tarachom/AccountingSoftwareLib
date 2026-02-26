
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
public class ConfiguratorRegistersInformationTree(Configuration conf, Action<string, string>? activate) : ConfiguratorTree(activate)
{
    Configuration Conf { get; set; } = conf;

    /// <summary>
    /// Заповнення
    /// </summary>
    /// <returns>Бокс із заповненою таблицею</returns>
    public Box Fill()
    {
        Store.RemoveAll();

        //Заповнення сховища
        foreach (ConfigurationRegistersInformation registers in Conf.RegistersInformation.Values)
            Store.Append(new ConfiguratorItemRow()
            {
                Group = "RegistersInformation",
                Name = registers.Name,
                Obj = registers
            });

        return HBox;
    }

    protected override Gio.ListModel? CreateFunc(GObject.Object item)
    {
        if (item is not ConfiguratorItemRow) return null;
        ConfiguratorItemRow itemRow = (ConfiguratorItemRow)item;

        string group = itemRow.Group;
        object? obj = itemRow.Obj;

        Gio.ListStore store = Gio.ListStore.New(ConfiguratorItemRow.GetGType());
        store.Ref();

        switch (group)
        {
            case "RegistersInformation" when obj is ConfigurationRegistersInformation registers:
                {
                    store.Append(new ConfiguratorItemRow()
                    {
                        Group = "DimensionFields",
                        Name = "Виміри",
                        Obj = registers
                    });

                    store.Append(new ConfiguratorItemRow()
                    {
                        Group = "ResourcesFields",
                        Name = "Ресурси",
                        Obj = registers
                    });

                    store.Append(new ConfiguratorItemRow()
                    {
                        Group = "PropertyFields",
                        Name = "Реквізити",
                        Obj = registers
                    });

                    return store;
                }
            case "DimensionFields" when obj is ConfigurationRegistersInformation registers:
                {
                    //Виміри
                    foreach (ConfigurationField field in registers.DimensionFields.Values)
                        store.Append(new ConfiguratorItemRow()
                        {
                            Group = "DimensionField",
                            Name = field.Name,
                            Obj = field,
                            Type = field.Type,
                            Desc = field.Pointer
                        });

                    return store;
                }
            case "ResourcesFields" when obj is ConfigurationRegistersInformation registers:
                {
                    //Русурси
                    foreach (ConfigurationField field in registers.ResourcesFields.Values)
                        store.Append(new ConfiguratorItemRow()
                        {
                            Group = "ResourcesField",
                            Name = field.Name,
                            Obj = field,
                            Type = field.Type,
                            Desc = field.Pointer
                        });

                    return store;
                }
            case "PropertyFields" when obj is ConfigurationRegistersInformation registers:
                {
                    //Реквізити
                    foreach (ConfigurationField field in registers.PropertyFields.Values)
                        store.Append(new ConfiguratorItemRow()
                        {
                            Group = "PropertyField",
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