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
        public ConfigurationField(string name, string fullname, string nameInTable, string type, string pointer,
            string desc = "", bool isPresentation = false, bool isIndex = false,
            bool isFullTextSearch = false, bool isSearch = false, bool isExport = false)
        {
            Name = name;
            FullName = fullname;
            NameInTable = nameInTable;
            Type = type;
            Pointer = pointer;
            Desc = desc;
            IsPresentation = isPresentation;
            IsIndex = isIndex;
            IsFullTextSearch = isFullTextSearch;
            IsSearch = isSearch;
            IsExport = isExport;
        }

        /// <summary>
        /// Назва поля в конфігурації
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
		/// Повна назва
		/// </summary>
		public string FullName { get; set; } = "";

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
        /// Використовувати поле для пошуку у списку
        /// </summary>
        public bool IsSearch { get; set; }

        /// <summary>
        /// Дозволити експорт поля при вигрузці наприклад в XML документ
        /// </summary>
        public bool IsExport { get; set; }

        #region Додаткові поля які залежать від типу (string)

        /// <summary>
        /// Багатострічкове поле
        /// </summary>
        public bool Multiline { get; set; }

        #endregion

        #region Додаткові поля які залежать від типу (integer)

        /// <summary>
        /// Автоматична нумерація
        /// </summary>
        public bool AutomaticNumbering { get; set; }

        #endregion

        #region Додаткові поля які залежать від типу (composite_pointer)

        /// <summary>
        /// Не використовувати довідники
        /// </summary>
        public bool CompositePointerNotUseDirectories { get; set; }

        /// <summary>
        /// Не використовувати документи
        /// </summary>
        public bool CompositePointerNotUseDocuments { get; set; }

        /// <summary>
        /// Доступні довідники для вибору
        /// </summary>
        public List<string> CompositePointerAllowDirectories { get; set; } = [];

        /// <summary>
        /// Доступні документи для вибору
        /// </summary>
        public List<string> CompositePointerAllowDocuments { get; set; } = [];

        #endregion

        /// <summary>
        /// Створення копії
        /// </summary>
        public ConfigurationField Copy()
        {
            return new ConfigurationField(Name, FullName, NameInTable, Type, Pointer, Desc, IsPresentation, IsIndex, IsFullTextSearch);
        }
    }
}