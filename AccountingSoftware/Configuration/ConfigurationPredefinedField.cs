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
    /// Попередньо визначене поле
    /// </summary>
    public class ConfigurationPredefinedField
    {
        /// <summary>
        /// Попередньо визначене поле
        /// </summary>
        /// <param name="nameInTable">Назва поля в базі даних</param>
        /// <param name="type">Тип</param>
        /// <param name="isPrimaryKey">Чи це первинний ключ</param>
        /// <param name="isIndex">Чи це індексоване поле</param>
        /// <param name="isNotNull">Не нульове значення поля</param>
        /// <param name="desc">Опис</param>
        public ConfigurationPredefinedField(string nameInTable, string type, bool isPrimaryKey, bool isIndex, bool isNotNull, string desc = "")
        {
            NameInTable = nameInTable;
            Type = type;
            IsPrimaryKey = isPrimaryKey;
            IsIndex = isIndex;
            IsNotNull = isNotNull;
            Desc = desc;
        }

        /// <summary>
        /// Назва поля в базі даних
        /// </summary>
        public string NameInTable { get; set; } = "";

        /// <summary>
        /// Тип даних
        /// </summary>
        public string Type { get; set; } = "";

        /// <summary>
        /// Первинний ключ (Primary key)
        /// </summary>
        public bool IsPrimaryKey { get; set; }

        /// <summary>
        /// Індексувати поле
        /// </summary>
        public bool IsIndex { get; set; }

        /// <summary>
        /// Не нульове значення поля
        /// </summary>
        public bool IsNotNull { get; set; }

        /// <summary>
        /// Опис
        /// </summary>
        public string Desc { get; set; } = "";
    }
}