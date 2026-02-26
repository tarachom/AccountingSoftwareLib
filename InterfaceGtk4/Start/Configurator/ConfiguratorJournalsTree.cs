
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
/// Клас для виводу журналів
/// </summary>
/// <param name="conf">Конфігурація</param>
/// <param name="activate">Процедура активації вітки в дереві</param>
public class ConfiguratorJournalsTree(Configuration conf, Action<string, string>? activate) : ConfiguratorTree(activate)
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
        foreach (ConfigurationJournals journal in Conf.Journals.Values)
            Store.Append(new ConfiguratorItemRow()
            {
                Group = "Journals",
                Name = journal.Name,
                Obj = journal
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
        _ = store.Ref();

        switch (group)
        {
            case "Journals" when obj is ConfigurationJournals journal && journal.Fields.Count > 0:
                {
                    //Для журналу заповнюю поля
                    foreach (ConfigurationJournalField field in journal.Fields.Values)
                        store.Append(new ConfiguratorItemRow()
                        {
                            Group = "Field",
                            Name = field.Name,
                            Obj = field,
                            Type = field.Type,
                            Desc = (field.WherePeriod ? "Відбір по періоду, " : "") + (field.SortField ? "Сортування" : "")
                        });

                    return store;
                }
            default:
                return null;
        }
    }
}