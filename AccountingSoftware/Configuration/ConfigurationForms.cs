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
    /// Форма
    /// </summary>
    public class ConfigurationForms
    {
        /// <summary>
        /// Форма
        /// </summary>
        public ConfigurationForms() { }

        /// <summary>
        /// Форма
        /// </summary>
        /// <param name="name">Назва</param>
        /// <param name="desc">Опис</param>
        /// <param name="type">Тип</param>
        public ConfigurationForms(string name, string desc = "", TypeForms type = TypeForms.None)
        {
            Name = name;
            Desc = desc;
            Type = type;
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
        /// Тип форми
        /// </summary>
        public TypeForms Type { get; set; } = TypeForms.None;

        /// <summary>
        /// Згенерований код для форми
        /// </summary>
        public string GeneratedCode { get; set; } = "";

        /// <summary>
        /// Форма модифікована
        /// </summary>
        public bool Modified { get; set; }

        /// <summary>
        /// Не зберігати форму в файл
        /// </summary>
        public bool NotSaveToFile { get; set; }

        #region Для форми елементу (Type == TypeForms.Element)

        /// <summary>
        /// Поля для форми елементу
        /// </summary>
        public Dictionary<string, ConfigurationFormsElementField> ElementFields { get; set; } = [];

        /// <summary>
        /// Табличні частини для форми елементу
        /// </summary>
        public Dictionary<string, ConfigurationFormsElementTablePart> ElementTableParts { get; set; } = [];

        /// <summary>
        /// Додати поле для форми елементу
        /// </summary>
        public void AppendElementField(ConfigurationFormsElementField field)
        {
            ElementFields.Add(field.Name, field);
        }

        /// <summary>
        /// Додати табличну частину для форми елементу
        /// </summary>
        public void AppendElementTablePart(ConfigurationFormsElementTablePart tablePart)
        {
            ElementTableParts.Add(tablePart.Name, tablePart);
        }

        #endregion

        #region Для форми списку (Type == TypeForms.List)

        public string TabularList { get; set; } = "";

        #endregion

        #region Для форми списку (Type == TypeForms.TablePart)

        public bool IncludeIconColumn { get; set; }

        #endregion

        /// <summary>
        /// Створення копії
        /// </summary>
        /// <returns></returns>
        public ConfigurationForms Copy()
        {
            ConfigurationForms newForms = new ConfigurationForms(Name, Desc, Type);

            //Поля для форми елементу
            foreach (KeyValuePair<string, ConfigurationFormsElementField> item in ElementFields)
            {
                ConfigurationFormsElementField ElementFieldCopy = new(item.Value.Name, item.Value.Caption);
                newForms.ElementFields.Add(item.Key, ElementFieldCopy);
            }

            //Табличні частини для форми елементу
            foreach (KeyValuePair<string, ConfigurationFormsElementTablePart> item in ElementTableParts)
            {
                ConfigurationFormsElementTablePart ElementTablePartCopy = new(item.Value.Name, item.Value.Caption);
                newForms.ElementTableParts.Add(item.Key, ElementTablePartCopy);
            }

            return newForms;
        }

        /// <summary>
        /// Типи форм
        /// </summary>
        public enum TypeForms
        {
            /// <summary>
            /// Неопреділено
            /// </summary>
            None = 1,

            /// <summary>
            /// Список
            /// </summary>
            List,

            /// <summary>
            /// Елемент
            /// </summary>
            Element,

            /// <summary>
            /// Швидкий вибір
            /// </summary>
            ListSmallSelect,

            /// <summary>
            /// Контрол вибору
            /// </summary>
            PointerControl,

            /// <summary>
            /// Контрол вибору масиву
            /// </summary>
            MultiplePointerControl,

            /// <summary>
            /// Список з Деревом
            /// </summary>
            ListAndTree,

            /// <summary>
            /// Таблична частина
            /// </summary>
            TablePart,

            /// <summary>
            /// Спільні функції
            /// </summary>
            Function,

            /// <summary>
            /// Тригери
            /// </summary>
            Triggers,

            /// <summary>
            /// Проведення документу
            /// </summary>
            SpendTheDocument,

            /// <summary>
            /// Звіт
            /// </summary>
            Report
        }
    }
}