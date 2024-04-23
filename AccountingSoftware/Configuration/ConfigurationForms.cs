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
        /// Поля для форми елементу
        /// </summary>
        public Dictionary<string, ConfigurationFormsElementField> ElementFields { get; set; } = [];

        /// <summary>
        /// Додати додаткове поле
        /// </summary>
        public void AppendElementField(ConfigurationFormsElementField field)
        {
            ElementFields.Add(field.Name, field);
        }

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
            List = 2,

            /// <summary>
            /// Елемент
            /// </summary>
            Element = 3
        }
    }
}