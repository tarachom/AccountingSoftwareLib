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
    /// <summary>
    /// Регістер накопичення
    /// </summary>
    public class ConfigurationRegistersAccumulation : ConfigurationObject
    {
        /// <summary>
        /// Регістри накопичення
        /// </summary>
        public ConfigurationRegistersAccumulation() { }

        /// <summary>
        /// Регістри накопичення
        /// </summary>
        /// <param name="name">Назва</param>
        /// <param name="fullname">Повна назва</param>
        /// <param name="table">Таблиця в базі даних</param>
        /// <param name="type">Тип</param>
        /// <param name="desc">Опис</param>
        public ConfigurationRegistersAccumulation(string name, string fullname, string table, TypeRegistersAccumulation type, string desc = "")
        {
            Name = name;
            FullName = fullname;
            Table = table;
            TypeRegistersAccumulation = type;
            Desc = desc;
        }

        /// <summary>
        /// Тип регістру
        /// </summary>
        public TypeRegistersAccumulation TypeRegistersAccumulation { get; set; } = TypeRegistersAccumulation.Residues;

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
        /// Документи які роблять рухи по даному регістру
        /// </summary>
        public List<string> AllowDocumentSpend { get; } = [];

        /// <summary>
        /// Табличні частини
        /// </summary>
        public Dictionary<string, ConfigurationTablePart> TabularParts { get; } = [];

        /// <summary>
        /// Блоки запитів
        /// </summary>
        public Dictionary<string, ConfigurationQueryBlock> QueryBlockList { get; } = [];

        /// <summary>
        /// Табличні списки
        /// </summary>
        public Dictionary<string, ConfigurationTabularList> TabularList { get; set; } = [];

        /// <summary>
        /// Форми
        /// </summary>
        public Dictionary<string, ConfigurationForms> Forms { get; } = [];

        /// <summary>
        /// Без таблиці Підсумки
        /// </summary>
        public bool NoSummary { get; set; }

        /// <summary>
        /// Створення копії
        /// </summary>
        /// <returns></returns>
        public ConfigurationRegistersAccumulation Copy()
        {
            ConfigurationRegistersAccumulation confRegCopy = new(this.Name, this.FullName, this.Table, this.TypeRegistersAccumulation, this.Desc);

            foreach (KeyValuePair<string, ConfigurationField> fields in this.DimensionFields)
                confRegCopy.DimensionFields.Add(fields.Key, fields.Value.Copy());

            foreach (KeyValuePair<string, ConfigurationField> fields in this.ResourcesFields)
                confRegCopy.ResourcesFields.Add(fields.Key, fields.Value.Copy());

            foreach (KeyValuePair<string, ConfigurationField> fields in this.PropertyFields)
                confRegCopy.PropertyFields.Add(fields.Key, fields.Value.Copy());

            foreach (KeyValuePair<string, ConfigurationTablePart> tablePart in this.TabularParts)
                confRegCopy.TabularParts.Add(tablePart.Key, tablePart.Value.Copy());

            foreach (KeyValuePair<string, ConfigurationQueryBlock> query in this.QueryBlockList)
                confRegCopy.QueryBlockList.Add(query.Key, query.Value.Copy());

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
                new ConfigurationPredefinedField("income", "boolean", false, true, true, "Рух"),
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
        /// Додати нову табличну частину
        /// </summary>
        /// <param name="tablePart">Нова таблична частина</param>
        public void AppendTablePart(ConfigurationTablePart tablePart)
        {
            TabularParts.Add(tablePart.Name, tablePart);
        }

        /// <summary>
        /// Додати нову форму
        /// </summary>
        /// <param name="forms">Нова форма</param>
        public void AppendForms(ConfigurationForms forms)
        {
            Forms.Add(forms.Name, forms);
        }

        public void AppendQueryBlockList(ConfigurationQueryBlock queryBlock)
        {
            QueryBlockList.Add(queryBlock.Name, queryBlock);
        }

        public void AppendTableList(ConfigurationTabularList tabularList)
        {
            TabularList.Add(tabularList.Name, tabularList);
        }

        #endregion
    }

    /// <summary>
    /// Тип регістру
    /// </summary>
    public enum TypeRegistersAccumulation
    {
        /// <summary>
        /// Залишки
        /// </summary>
        Residues = 1,

        /// <summary>
        /// Обороти
        /// </summary>
        Turnover = 2
    }
}