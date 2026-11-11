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

namespace AccountingSoftware
{
    public class ConfigurationJournalFieldMapping : ConfigurationObject
    {
        public ConfigurationJournalFieldMapping() { }

        public ConfigurationJournalFieldMapping(string name, string fullname = "", string desc = "")
        {
            Name = name;
            FullName = fullname;
            Desc = desc;
        }

        /// <summary>
        /// Співідношення полів (поле журналу - до поля документу)
        /// </summary>
        public Dictionary<string, string> Fields { get; } = [];

        /// <summary>
        /// Створення копії
        /// </summary>
        /// <returns></returns>
        public ConfigurationJournalFieldMapping Copy()
        {
            ConfigurationJournalFieldMapping newFieldMapping = new(this.Name, this.FullName, this.Desc)
            {
                LatinName = this.LatinName
            };

            foreach (var field in this.Fields)
                newFieldMapping.Fields.Add(field.Key, field.Value);

            return newFieldMapping;
        }
    }
}