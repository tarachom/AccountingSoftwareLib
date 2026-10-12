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
    /// Поле журналу
    /// </summary>
    public class ConfigurationJournalField : ConfigurationObject
    {
        /// <summary>
        /// Поле журналу 
        /// </summary>
        public ConfigurationJournalField() { }

        /// <summary>
        /// Поле журналу 
        /// </summary>
        /// <param name="name">Назва</param>
        /// <param name="desc">Опис</param>
        /// <param name="type">Тип</param>
        /// <param name="sort">Сортувати</param>
        /// <param name="wherePeriod">Відбір по періоду</param>
        public ConfigurationJournalField(string name, string desc = "", string type = "", bool sort = false, bool wherePeriod = false)
        {
            Name = name;
            Desc = desc;
            Type = type;
            SortField = sort;
            WherePeriod = wherePeriod;
        }

        /// <summary>
        /// SQL тип поля
        /// </summary>
        public string Type { get; set; } = "";

        /// <summary>
        /// Сортувати по полю
        /// </summary>
        public bool SortField { get; set; }

        /// <summary>
        /// Відбір по періоду
        /// </summary>
        public bool WherePeriod { get; set; }

        /// <summary>
        /// Створення копії
        /// </summary>
        /// <returns></returns>
        public ConfigurationJournalField Copy() => new(this.Name, this.Desc) { FullName = this.FullName };
    }
}