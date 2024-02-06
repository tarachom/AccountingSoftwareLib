/*
Copyright (C) 2019-2023 TARAKHOMYN YURIY IVANOVYCH
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
    /// Документ
    /// </summary>
    public class ConfigurationDocuments : ConfigurationObject
    {
        /// <summary>
        /// Документ
        /// </summary>
        public ConfigurationDocuments() { }

        /// <summary>
        /// Документ
        /// </summary>
        /// <param name="name">Назва</param>
        /// <param name="fullname">Повна назва</param>
        /// <param name="table">Таблиця в базі даних</param>
        /// <param name="desc">Опис</param>
        /// <param name="automaticNumeration">Автоматична нумерація</param>
        public ConfigurationDocuments(string name, string fullname, string table, string desc = "", bool automaticNumeration = false)
        {
            Name = name;
            FullName = fullname;
            Table = table;
            Desc = desc;
            AutomaticNumeration = automaticNumeration;
        }

        /// <summary>
        /// Поля
        /// </summary>
        public Dictionary<string, ConfigurationField> Fields { get; } = [];

        /// <summary>
        /// Табличні частини
        /// </summary>
        public Dictionary<string, ConfigurationTablePart> TabularParts { get; } = [];

        /// <summary>
        /// Регістри накопичення по яких може робити рухи документ
        /// </summary>
        public List<string> AllowRegisterAccumulation { get; private set; } = [];

        /// <summary>
        /// Тригери
        /// </summary>
        public ConfigurationTriggerFunctions TriggerFunctions { get; set; } = new();

        /// <summary>
        /// Функції (проведення/очищення проводок) документу
        /// </summary>
        public ConfigurationSpendFunctions SpendFunctions { get; set; } = new();

        /// <summary>
        /// Табличні списки
        /// </summary>
        public Dictionary<string, ConfigurationTabularList> TabularList { get; set; } = [];

        /// <summary>
        /// Автоматична нумерація
        /// </summary>
        public bool AutomaticNumeration { get; set; }

        /// <summary>
        /// Створити копію
        /// </summary>
        /// <returns></returns>
        public ConfigurationDocuments Copy()
        {
            ConfigurationDocuments confDocCopy = new ConfigurationDocuments(this.Name, this.FullName, this.Table, this.Desc);

            foreach (KeyValuePair<string, ConfigurationField> fields in this.Fields)
                confDocCopy.Fields.Add(fields.Key, fields.Value.Copy());

            foreach (KeyValuePair<string, ConfigurationTablePart> tablePart in this.TabularParts)
                confDocCopy.TabularParts.Add(tablePart.Key, tablePart.Value.Copy());

            confDocCopy.TriggerFunctions = this.TriggerFunctions.Copy();

            confDocCopy.SpendFunctions = this.SpendFunctions.Copy();

            return confDocCopy;
        }

        /// <summary>
        /// Додати нове поле в список полів
        /// </summary>
        /// <param name="field">Нове поле</param>
        public void AppendField(ConfigurationField field)
        {
            Fields.Add(field.Name, field);
        }

        /// <summary>
        /// Додати нову табличну частину
        /// </summary>
        /// <param name="tablePart">Нова таблична частина</param>
        public void AppendTablePart(ConfigurationTablePart tablePart)
        {
            TabularParts.Add(tablePart.Name, tablePart);
        }

        /// <summary>
        /// Додати новий табличний список
        /// </summary>
        /// <param name="tablePart">Новий табличний список</param>
        public void AppendTableList(ConfigurationTabularList tabularList)
        {
            TabularList.Add(tabularList.Name, tabularList);
        }
    }
}