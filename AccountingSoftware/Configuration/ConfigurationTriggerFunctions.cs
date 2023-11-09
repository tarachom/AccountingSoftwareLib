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
    /// Тригери-функції які запускаються перед записом, після запису або перед видаленням обєктів (довідник, документ і т.д) 
    /// </summary>
    public class ConfigurationTriggerFunctions
    {
        /// <summary>
        /// При створенні нового
        /// </summary>
        public string New { get; set; } = "";

        /// <summary>
        /// При копіюванні
        /// </summary>
        public string Copying { get; set; } = "";

        /// <summary>
        /// Перед записом
        /// </summary>
        public string BeforeSave { get; set; } = "";

        /// <summary>
        /// Після запису
        /// </summary>
        public string AfterSave { get; set; } = "";

        /// <summary>
        /// Встановлення мітки на видалення
        /// </summary>
        public string SetDeletionLabel { get; set; } = "";

        /// <summary>
        /// Перед видаленням
        /// </summary>
        public string BeforeDelete { get; set; } = "";

        /// <summary>
        /// Створення копії
        /// </summary>
        /// <returns></returns>
        public ConfigurationTriggerFunctions Copy()
        {
            return new ConfigurationTriggerFunctions()
            {
                New = this.New,
                Copying = this.Copying,
                BeforeSave = this.BeforeSave,
                AfterSave = this.AfterSave,
                SetDeletionLabel = this.SetDeletionLabel,
                BeforeDelete = this.BeforeDelete
            };
        }
    }
}