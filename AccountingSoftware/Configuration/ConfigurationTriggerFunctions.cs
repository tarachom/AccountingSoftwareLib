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
        /// Доступність тригеру
        /// </summary>
        public bool NewAction { get; set; }

        /// <summary>
        /// При копіюванні
        /// </summary>
        public string Copying { get; set; } = "";

        /// <summary>
        /// Доступність тригеру
        /// </summary>
        public bool CopyingAction { get; set; }

        /// <summary>
        /// Перед записом
        /// </summary>
        public string BeforeSave { get; set; } = "";

        /// <summary>
        /// Доступність тригеру
        /// </summary>
        public bool BeforeSaveAction { get; set; }

        /// <summary>
        /// Після запису
        /// </summary>
        public string AfterSave { get; set; } = "";

        /// <summary>
        /// Доступність тригеру
        /// </summary>
        public bool AfterSaveAction { get; set; }

        /// <summary>
        /// Встановлення мітки на видалення
        /// </summary>
        public string SetDeletionLabel { get; set; } = "";

        /// <summary>
        /// Доступність тригеру
        /// </summary>
        public bool SetDeletionLabelAction { get; set; }

        /// <summary>
        /// Перед видаленням
        /// </summary>
        public string BeforeDelete { get; set; } = "";

        /// <summary>
        /// Доступність тригеру
        /// </summary>
        public bool BeforeDeleteAction { get; set; }

        /// <summary>
        /// Створення копії
        /// </summary>
        /// <returns></returns>
        public ConfigurationTriggerFunctions Copy()
        {
            return new ConfigurationTriggerFunctions()
            {
                New = this.New,
                NewAction = this.NewAction,

                Copying = this.Copying,
                CopyingAction = this.CopyingAction,

                BeforeSave = this.BeforeSave,
                BeforeSaveAction = this.BeforeSaveAction,

                AfterSave = this.AfterSave,
                AfterSaveAction = this.AfterSaveAction,

                SetDeletionLabel = this.SetDeletionLabel,
                SetDeletionLabelAction = this.SetDeletionLabelAction,

                BeforeDelete = this.BeforeDelete,
                BeforeDeleteAction = this.BeforeDeleteAction
            };
        }
    }
}