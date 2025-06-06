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
    /// Константи
    /// </summary>
    public class ConfigurationConstants
    {
        /// <summary>
        /// Константа
        /// </summary>
        public ConfigurationConstants() { }

        /// <summary>
        /// Константа
        /// </summary>
        /// <param name="name">Назва</param>
        /// <param name="nameInTable">Назва в таблиці</param>
        /// <param name="type">Тип даних</param>
        /// <param name="block">Блок</param>
        /// <param name="pointer">Вказівник</param>
        /// <param name="desc">Опис</param>
        public ConfigurationConstants(string name, string nameInTable, string type, ConfigurationConstantsBlock block, string pointer = "", string desc = "")
        {
            Name = name;
            NameInTable = nameInTable;
            Type = type;
            Block = block;
            Pointer = pointer;
            Desc = desc;
        }

        /// <summary>
        /// Блок
        /// </summary>
        public ConfigurationConstantsBlock Block { get; set; } = new();

        /// <summary>
        /// Назва
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Назва поля в базі даних
        /// </summary>
        public string NameInTable { get; set; } = "";

        /// <summary>
        /// Опис
        /// </summary>
        public string Desc { get; set; } = "";

        /// <summary>
        /// Тип даних
        /// </summary>
        public string Type { get; set; } = "";

        /// <summary>
        /// Вказівник на об'єкт конфігурації
        /// </summary>
        public string Pointer { get; set; } = "";

        /// <summary>
        /// Табличні частини
        /// </summary>
        public Dictionary<string, ConfigurationTablePart> TabularParts { get; } = [];

        /// <summary>
        /// Додати нову табличну частину
        /// </summary>
        /// <param name="tablePart">Нова таблична частина</param>
        public void AppendTablePart(ConfigurationTablePart tablePart)
        {
            TabularParts.Add(tablePart.Name, tablePart);
        }

        /// <summary>
        /// Створення копії
        /// </summary>
        /// <returns></returns>
        public ConfigurationConstants Copy()
        {
            ConfigurationConstants newConst = new ConfigurationConstants(Name, NameInTable, Type, Block, Pointer, Desc);

            foreach (ConfigurationTablePart tablePart in TabularParts.Values)
                newConst.TabularParts.Add(tablePart.Name, tablePart.Copy());

            return newConst;
        }
    }
}