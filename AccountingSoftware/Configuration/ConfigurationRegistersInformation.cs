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
    /// Регістри відомостей
    /// </summary>
    public class ConfigurationRegistersInformation : ConfigurationObject
    {
        /// <summary>
        /// Регістри відомостей
        /// </summary>
        public ConfigurationRegistersInformation() { }

        /// <summary>
        /// Регістри відомостей
        /// </summary>
        /// <param name="name">Назва</param>
        /// <param name="fullname">Повна назва</param>
        /// <param name="table">Таблиця в базі даних</param>
        /// <param name="desc">Опис</param>
        public ConfigurationRegistersInformation(string name, string fullname, string table, string desc = "")
        {
            Name = name;
            FullName = fullname;
            Table = table;
            Desc = desc;
        }

        /// <summary>
        /// Виміри
        /// </summary>
        public Dictionary<string, ConfigurationField> DimensionFields { get; } = [];

        /// <summary>
        /// Русурси
        /// </summary>
        public Dictionary<string, ConfigurationField> ResourcesFields { get; } = [];

        /// <summary>
        /// Реквізити
        /// </summary>
        public Dictionary<string, ConfigurationField> PropertyFields { get; } = [];

        /// <summary>
        /// Табличні списки
        /// </summary>
        public Dictionary<string, ConfigurationTabularList> TabularList { get; set; } = [];

        /// <summary>
        /// Форми
        /// </summary>
        public Dictionary<string, ConfigurationForms> Forms { get; } = [];

        /// <summary>
        /// Створення копії
        /// </summary>
        /// <returns></returns>
        public ConfigurationRegistersInformation Copy()
        {
            ConfigurationRegistersInformation confRegCopy = new ConfigurationRegistersInformation(this.Name, this.FullName, this.Table, this.Desc);

            foreach (KeyValuePair<string, ConfigurationField> fields in this.DimensionFields)
                confRegCopy.DimensionFields.Add(fields.Key, fields.Value.Copy());

            foreach (KeyValuePair<string, ConfigurationField> fields in this.ResourcesFields)
                confRegCopy.ResourcesFields.Add(fields.Key, fields.Value.Copy());

            foreach (KeyValuePair<string, ConfigurationField> fields in this.PropertyFields)
                confRegCopy.PropertyFields.Add(fields.Key, fields.Value.Copy());

            foreach (KeyValuePair<string, ConfigurationTabularList> tabularList in this.TabularList)
                confRegCopy.TabularList.Add(tabularList.Key, tabularList.Value.Copy());

            foreach (KeyValuePair<string, ConfigurationForms> forms in this.Forms)
                confRegCopy.Forms.Add(forms.Key, forms.Value.Copy());

            return confRegCopy;
        }

        /// <summary>
        /// Функція повертає масив попередньо визначених полів
        /// </summary>
        public static ConfigurationPredefinedField[] GetPredefinedFields()
        {
            return
            [
                new ConfigurationPredefinedField("uid", "any_pointer", true, false, true, "Первинний ключ (Primary key)"),
                new ConfigurationPredefinedField("period", "timestamp without time zone", false, true, true, "Період"),
                new ConfigurationPredefinedField("owner", "any_pointer", false, true, true, "Власник"),
                new ConfigurationPredefinedField("ownertype", "composite_text", false, true, false, "Тип власника")
            ];
        }

        #region Append

        public void AppendDimensionField(ConfigurationField field)
        {
            DimensionFields.Add(field.Name, field);
        }

        public void AppendResourcesField(ConfigurationField field)
        {
            ResourcesFields.Add(field.Name, field);
        }

        public void AppendPropertyField(ConfigurationField field)
        {
            PropertyFields.Add(field.Name, field);
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

        #endregion
    }
}