﻿/*
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
    /// Функції які запускаються при проведенні або відміні проведення документів
    /// </summary>
    public class ConfigurationSpendFunctions
    {
        /// <summary>
        /// Обробка проведення документу
        /// </summary>
        public string Spend { get; set; } = "Spend";

        /// <summary>
        /// Доступність тригеру
        /// </summary>
        public bool SpendAction { get; set; }

        /// <summary>
        /// Обробка очищення проводок документу
        /// </summary>
        public string ClearSpend { get; set; } = "Clear";

        /// <summary>
        /// Доступність тригеру
        /// </summary>
        public bool ClearSpendAction { get; set; }

        /// <summary>
        /// Створення копії
        /// </summary>
        /// <returns></returns>
        public ConfigurationSpendFunctions Copy()
        {
            return new ConfigurationSpendFunctions()
            {
                Spend = this.Spend,
                SpendAction = this.SpendAction,
                ClearSpend = this.ClearSpend,
                ClearSpendAction = this.ClearSpendAction,
            };
        }
    }
}