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
    /// Довідник
    /// </summary>
    public class ConfigurationDirectories : ConfigurationObject
    {
        /// <summary>
        /// Довідник
        /// </summary>
        public ConfigurationDirectories() { }

        /// <summary>
        /// Довідник
        /// </summary>
        /// <param name="name">Назва</param>
        /// <param name="fullname">Повна назва</param>
        /// <param name="table">Таблиця в базі даних</param>
        /// <param name="desc">Опис</param>
        /// <param name="automaticNumeration">Автоматична нумерація</param>
        /// <param name="typeDirectory">Тип довідника</param>
        /// <param name="pointerFolders">Вказівник на ієрархію у іншому довіднику</param>
        public ConfigurationDirectories(string name, string fullname, string table,
            string desc = "", bool automaticNumeration = false,
            TypeDirectories typeDirectory = TypeDirectories.Normal, string pointerFolders = "")
        {
            Name = name;
            FullName = fullname;
            Table = table;
            Desc = desc;
            AutomaticNumeration = automaticNumeration;
            TypeDirectory = typeDirectory;
            PointerFolders = pointerFolders;
        }

        /// <summary>
        /// Поля
        /// </summary>
        public Dictionary<string, ConfigurationField> Fields { get; } = [];

        /// <summary>
        /// Табличні частини
        /// </summary>
        public Dictionary<string, ConfigurationTablePart> TabularParts { get; } = [];

        /// <summary>
        /// Тригери
        /// </summary>
        public ConfigurationTriggerFunctions TriggerFunctions { get; set; } = new();

        /// <summary>
        /// Табличні списки
        /// </summary>
        public Dictionary<string, ConfigurationTabularList> TabularList { get; set; } = [];

        /// <summary>
        /// Форми
        /// </summary>
        public Dictionary<string, ConfigurationForms> Forms { get; } = [];

        /// <summary>
        /// Автоматична нумерація
        /// </summary>
        public bool AutomaticNumeration { get; set; }

        /// <summary>
        /// Тип довідника
        /// </summary>
        public TypeDirectories TypeDirectory { get; set; } = TypeDirectories.Normal;

        /// <summary>
        /// Вказівник на довідник папок, якщо тип довідника Hierarchical
        /// </summary>
        public string PointerFolders { get; set; } = "";

        /// <summary>
        /// Створити копію
        /// </summary>
        /// <returns></returns>
        public ConfigurationDirectories Copy()
        {
            ConfigurationDirectories confDirCopy = new ConfigurationDirectories(this.Name, this.FullName, this.Table, this.Desc,
                false, /* Автоматична нумерація не копіюється */
                this.TypeDirectory, this.PointerFolders);

            foreach (KeyValuePair<string, ConfigurationField> fields in this.Fields)
                confDirCopy.Fields.Add(fields.Key, fields.Value.Copy());

            foreach (KeyValuePair<string, ConfigurationTablePart> tablePart in this.TabularParts)
                confDirCopy.TabularParts.Add(tablePart.Key, tablePart.Value.Copy());

            foreach (KeyValuePair<string, ConfigurationForms> forms in this.Forms)
                confDirCopy.Forms.Add(forms.Key, forms.Value.Copy());

            confDirCopy.TriggerFunctions = this.TriggerFunctions.Copy();

            return confDirCopy;
        }

        /// <summary>
        /// Додати нове поле
        /// </summary>
        /// <param name="field">Нове поле</param>
        public void AppendField(ConfigurationField field)
        {
            Fields.Add(field.Name, field);
        }

        /// <summary>
        /// Додати нову табличну частину
        /// </summary>
        /// <param name="tablePart">Нова таблична частина</param>
        public void AppendTablePart(ConfigurationTablePart tablePart)
        {
            TabularParts.Add(tablePart.Name, tablePart);
        }

        /// <summary>
        /// Додати новий табличний список
        /// </summary>
        /// <param name="tablePart">Новий табличний список</param>
        public void AppendTableList(ConfigurationTabularList tabularList)
        {
            TabularList.Add(tabularList.Name, tabularList);
        }

        /// <summary>
        /// Додати нову форму
        /// </summary>
        /// <param name="forms">Нова форма</param>
        public void AppendForms(ConfigurationForms forms)
        {
            Forms.Add(forms.Name, forms);
        }

        /// <summary>
        /// Типи довідників
        /// </summary>
        public enum TypeDirectories
        {
            /// <summary>
            /// Звичайний, табличний
            /// </summary>
            Normal = 1,

            /// <summary>
            /// Ієрархічний
            /// </summary>
            Hierarchical = 2,

            /// <summary>
            /// Ієрархія в іншому довіднику
            /// </summary>
            HierarchyInAnotherDirectory = 3
        }
    }
}
