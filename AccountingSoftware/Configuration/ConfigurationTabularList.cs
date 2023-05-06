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
    /// Табличний список (вивід таблиці на формі)
    /// </summary>
    public class ConfigurationTabularList
    {
        public ConfigurationTabularList()
        {
            Name = "";
            Desc = "";
            Fields = new Dictionary<string, ConfigurationTabularListField>();
        }

        public ConfigurationTabularList(string name, string desc, bool isTree = false) : this()
        {
            Name = name;
            Desc = desc;
            IsTree = isTree;
        }

        /// <summary>
        /// Назва
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Опис
        /// </summary>
        public string Desc { get; set; }

        /// <summary>
        /// Список для виводу дерева (для довідників)
        /// </summary>
        public bool IsTree { get; set; }

        /// <summary>
        /// Поля
        /// </summary>
        public Dictionary<string, ConfigurationTabularListField> Fields { get; }

        /// <summary>
        /// Додати поле
        /// </summary>
        /// <param name="field"></param>
        public void AppendField(ConfigurationTabularListField field)
        {
            Fields.Add(field.Name, field);
        }

        /// <summary>
        /// Створення копії
        /// </summary>
        /// <returns></returns>
        public ConfigurationTabularList Copy()
        {
            ConfigurationTabularList newTabularList = new ConfigurationTabularList(Name, Desc);
            foreach (ConfigurationTabularListField item in Fields.Values)
                newTabularList.Fields.Add(item.Name, item);

            return newTabularList;
        }
    }
}