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
    /// Таблична частина
    /// </summary>
    public class ConfigurationTablePart : ConfigurationObject
    {
        /// <summary>
        /// Таблична частина
        /// </summary>
        public ConfigurationTablePart() { }

        /// <summary>
        /// Таблична частина
        /// </summary>
        /// <param name="name">Назва</param>
        /// <param name="table">Таблиця в базі даних</param>
        /// <param name="desc">Опис</param>
        public ConfigurationTablePart(string name, string fullname, string table, string desc = "")
        {
            Name = name;
            FullName = fullname;
            Table = table;
            Desc = desc;
        }

        /// <summary>
        /// Записувати історію змін даних
        /// </summary>
        public bool VersionsHistory { get; set; }

        /// <summary>
        /// Поля
        /// </summary>
        public Dictionary<string, ConfigurationField> Fields { get; } = [];

        /// <summary>
        /// Табличні списки
        /// </summary>
        public Dictionary<string, ConfigurationTabularList> TabularList { get; set; } = [];

        /// <summary>
        /// Тригери
        /// </summary>
        public ConfigurationTriggerTablePartFunctions TriggerFunctions { get; set; } = new();

        // <summary>
        /// Форми
        /// </summary>
        public Dictionary<string, ConfigurationForms> Forms { get; } = [];

        /// <summary>
        /// Створення копії
        /// </summary>
        /// <returns></returns>
        public ConfigurationTablePart Copy()
        {
            ConfigurationTablePart confObjectTablePart = new(this.Name, this.FullName, this.Table, this.Desc);

            foreach (KeyValuePair<string, ConfigurationField> fields in this.Fields)
                confObjectTablePart.Fields.Add(fields.Key, fields.Value.Copy());

            foreach (KeyValuePair<string, ConfigurationTabularList> tabularList in this.TabularList)
                confObjectTablePart.TabularList.Add(tabularList.Key, tabularList.Value.Copy());

            foreach (KeyValuePair<string, ConfigurationForms> forms in this.Forms)
                confObjectTablePart.Forms.Add(forms.Key, forms.Value.Copy());

            confObjectTablePart.TriggerFunctions = this.TriggerFunctions.Copy();

            return confObjectTablePart;
        }

        /// <summary>
        /// Функція повертає масив попередньо визначених полів
        /// </summary>
        public static ConfigurationPredefinedField[] GetPredefinedFields()
        {
            return
            [
                new ConfigurationPredefinedField("uid", "any_pointer", true, false, true, "Первинний ключ (Primary key)"),

                /*
                Поле Власник є не у всіх табличних частинах

                Наприклад в Табличних частинах для Констант немає поля Власник,
                немає поля Власник і у Регістрів Накопичення
                */
                new ConfigurationPredefinedField("owner", "any_pointer", false, true, true, "Власник")
            ];
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
        /// Додати новий табличний список
        /// </summary>
        /// <param name="tablePart">Новий табличний список</param>
        public void AppendTableList(ConfigurationTabularList tabularList)
        {
            TabularList.Add(tabularList.Name, tabularList);
        }

        /// <summary>
        /// Додати нову форму
        /// </summary>
        /// <param name="forms">Нова форма</param>
        public void AppendForms(ConfigurationForms forms)
        {
            Forms.Add(forms.Name, forms);
        }
    }
}