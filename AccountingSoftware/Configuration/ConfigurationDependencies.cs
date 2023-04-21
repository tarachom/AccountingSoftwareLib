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
    /// Залежності. Використовується для аналізу перед видаленням з бази.
    /// </summary>
    public class ConfigurationDependencies
    {
        /// <summary>
        /// Група - Константи, Довідники, Документи, Регістри
        /// </summary>
        public string ConfigurationGroupName { get; set; } = "";

        /// <summary>
        /// Рівень Групи
        /// </summary>
        public GroupLevel ConfigurationGroupLevel { get; set; } = GroupLevel.Object;

        /// <summary>
        /// Назва субгрупи (Таблична Частина)
        /// </summary>
        public string ConfigurationTablePartName { get; set; } = "";

        /// <summary>
        /// Назва обєкту конфігурації
        /// </summary>
        public string ConfigurationObjectName { get; set; } = "";

        /// <summary>
        /// Опис обєкту конфігурації
        /// </summary>
        public string ConfigurationObjectDesc { get; set; } = "";

        /// <summary>
        /// Таблиця
        /// </summary>
        public string Table { get; set; } = "";

        /// <summary>
        /// Назва поля
        /// </summary>
        public string ConfigurationFieldName { get; set; } = "";

        /// <summary>
        /// Поле
        /// </summary>
        public string Field { get; set; } = "";

        /// <summary>
        /// Рівень
        /// </summary>
        public enum GroupLevel
        {
            /// <summary>
            /// Обєкт
            /// </summary>
            Object,

            /// <summary>
            /// Таблична частина
            /// </summary>
            TablePart
        }
    }
}