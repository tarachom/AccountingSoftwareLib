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
    /// Поле табличного списку
    /// </summary>
    public class ConfigurationTabularListField
    {
        /// <summary>
        /// Поле табличного списку
        /// </summary>
        public ConfigurationTabularListField() { }

        /// <summary>
        /// Поле табличного списку
        /// </summary>
        /// <param name="name">Назва</param>
        public ConfigurationTabularListField(string name)
        {
            Name = Caption = name;
        }

        /// <summary>
        /// Поле табличного списку
        /// </summary>
        /// <param name="name">Назва</param>
        /// <param name="caption">Заголовок</param>
        /// <param name="size">Розмір</param>
        /// <param name="sortNum">Порядок</param>
        /// <param name="sortField">Сортувати</param>
        public ConfigurationTabularListField(string name, string caption, uint size = 0, int sortNum = 100, bool sortField = false, bool sortDirection = false)
        {
            Name = name;
            Caption = caption;
            Size = size;
            SortNum = sortNum;
            SortField = sortField;
            SortDirection = sortDirection;
        }

        #region ForJournals

        /// <summary>
        /// Поле табличного списку
        /// </summary>
        /// <param name="name">Назва</param>
        /// <param name="docfield">Поле документу</param>
        public ConfigurationTabularListField(string name, string docfield)
        {
            Name = name;
            DocField = docfield;
        }

        #endregion

        /// <summary>
        /// Назва
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Заголовок
        /// </summary>
        public string Caption { get; set; } = "";

        /// <summary>
        /// Розмір
        /// </summary>
        public uint Size { get; set; }

        /// <summary>
        /// Порядок поля в списку
        /// </summary>
        public int SortNum { get; set; } = 100;

        /// <summary>
        /// Сортувати по даному полю
        /// </summary>
        public bool SortField { get; set; }

        /// <summary>
        /// Напрямок сортування (false - звичайний ASC, true - у зворотньому напрямку DESC)
        /// </summary>
        public bool SortDirection { get; set; }

        #region ForJournals

        /// <summary>
        /// Поля для журналу документів, так як даний клас використовується і в журналах
        /// </summary>
        public string DocField { get; set; } = "";

        #endregion
    }
}