/*
Copyright (C) 2019-2025 TARAKHOMYN YURIY IVANOVYCH
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
    /// <summary>
    /// Журнали
    /// </summary>
    public class ConfigurationJournals
    {
        /// <summary>
        /// Журнали
        /// </summary>
        public ConfigurationJournals() { }

        /// <summary>
        /// Журнали
        /// </summary>
        /// <param name="name">Назва</param>
        /// <param name="desc">Опис</param>
        public ConfigurationJournals(string name, string desc = "")
        {
            Name = name;
            Desc = desc;
        }

        /// <summary>
        /// Назва
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Опис
        /// </summary>
        public string Desc { get; set; } = "";

        /// <summary>
        /// Поля
        /// </summary>
        public Dictionary<string, ConfigurationJournalField> Fields { get; } = [];

        /// <summary>
        /// Документи які входять в журнал
        /// </summary>
        public List<string> AllowDocuments { get; private set; } = [];

        /// <summary>
        /// Табличні списки
        /// </summary>
        public Dictionary<string, ConfigurationTabularList> TabularList { get; set; } = [];

        /// <summary>
        /// Створення копії
        /// </summary>
        /// <returns></returns>
        public ConfigurationJournals Copy()
        {
            ConfigurationJournals newJournal = new ConfigurationJournals(this.Name, this.Desc);

            foreach (ConfigurationJournalField field in Fields.Values)
                newJournal.AppendField(field.Copy());

            return newJournal;
        }

        /// <summary>
        /// Додати нове поле в список полів
        /// </summary>
        /// <param name="field">Нове поле</param>
        public void AppendField(ConfigurationJournalField field)
        {
            Fields.Add(field.Name, field);
        }

        /// <summary>
        /// Додати новий табличний список
        /// </summary>
        /// <param name="tabularList"></param>
        public void AppendTableList(ConfigurationTabularList tabularList)
        {
            TabularList.Add(tabularList.Name, tabularList);
        }
    }
}