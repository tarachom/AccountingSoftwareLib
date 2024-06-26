﻿/*
Copyright (C) 2019-2024 TARAKHOMYN YURIY IVANOVYCH
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
    /// Поле
    /// </summary>
    public class ConfigurationField
    {
        /// <summary>
        /// Поле
        /// </summary>
        public ConfigurationField() { }

        /// <summary>Поле</summary>
        /// <param name="name">Назва поля</param>
        /// <param name="nameInTable">Назва колонки в базі даних</param>
        /// <param name="type">Тип поля (Всі типи описані в класі FieldType)</param>
        /// <param name="pointer">Вказівник</param>
        /// <param name="desc">Опис</param>
        /// <param name="isPresentation">Признак того що поле презентаційне</param>
        /// <param name="isIndex">Індексування</param>
        /// <param name="isFullTextSearch">Повнотекстовий пошук по полю</param>
        public ConfigurationField(string name, string nameInTable, string type, string pointer,
            string desc = "", bool isPresentation = false, bool isIndex = false, bool isFullTextSearch = false)
        {
            Name = name;
            NameInTable = nameInTable;
            Type = type;
            Pointer = pointer;
            Desc = desc;
            IsPresentation = isPresentation;
            IsIndex = isIndex;
            IsFullTextSearch = isFullTextSearch;
        }

        /// <summary>
        /// Назва поля в конфігурації
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Назва поля в базі даних
        /// </summary>
        public string NameInTable { get; set; } = "";

        /// <summary>
        /// Тип даних
        /// </summary>
        public string Type { get; set; } = "";

        /// <summary>
        /// Вказівник на об'єкт конфігурації
        /// </summary>
        public string Pointer { get; set; } = "";

        /// <summary>
        /// Опис
        /// </summary>
        public string Desc { get; set; } = "";

        /// <summary>
        /// Використовувати поле для презентації в списках і формах
        /// </summary>
        public bool IsPresentation { get; set; }

        /// <summary>
        /// Індексувати поле
        /// </summary>
        public bool IsIndex { get; set; }

        /// <summary>
        /// Використовувати поле для повнотекстового пошуку
        /// </summary>
        public bool IsFullTextSearch { get; set; }

        /// <summary>
        /// Багатострічкове поле (тільки для типу string)
        /// </summary>
        public bool Multiline { get; set; }

        /// <summary>
        /// Створення копії
        /// </summary>
        /// <returns></returns>
        public ConfigurationField Copy()
        {
            return new ConfigurationField(Name, NameInTable, Type, Pointer, Desc, IsPresentation, IsIndex, IsFullTextSearch);
        }
    }
}