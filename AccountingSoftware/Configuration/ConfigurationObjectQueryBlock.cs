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
    /// Блоки запитів для розрахунків регістрів накопичення
    /// </summary>
    public class ConfigurationObjectQueryBlock
    {
        public ConfigurationObjectQueryBlock() { }

        public ConfigurationObjectQueryBlock(string name, bool finalCalculation = false) : this()
        {
            Name = name;
            FinalCalculation = finalCalculation;
        }

        /// <summary>
        /// Запити
        /// </summary>
        public Dictionary<string, string> Query { get; } = [];

        /// <summary>
        /// Назва блоку
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Признак того що блок запитів призначений для фінального розрахунку
        /// </summary>
        public bool FinalCalculation { get; set; }

        /// <summary>
        /// Стоврення копії
        /// </summary>
        /// <returns></returns>
        public ConfigurationObjectQueryBlock Copy()
        {
            ConfigurationObjectQueryBlock newQueryBlock = new ConfigurationObjectQueryBlock(Name);

            foreach (KeyValuePair<string, string> item in Query)
                newQueryBlock.Query.Add(item.Key, item.Value);

            return newQueryBlock;
        }
    }
}