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
    /// Додаткове поле табличного списку
    /// </summary>
    public class ConfigurationTabularListAdditionalField
    {
        /// <summary>
        /// Додаткове поле табличного списку
        /// </summary>
        public ConfigurationTabularListAdditionalField() { }

        /// <summary>
        /// Додаткове поле табличного списку
        /// </summary>
        /// <param name="visible">Видимість</param>
        /// <param name="name">Назва</param>
        /// <param name="caption">Заголовок</param>
        /// <param name="size">Розмір</param>
        /// <param name="sortNum">Порядок сортування</param>
        /// <param name="type">Тип</param>
        /// <param name="value">Значення</param>
        public ConfigurationTabularListAdditionalField(bool visible, string name, string caption, uint size, int sortNum, string type, string value)
        {
            Visible = visible;
            Name = name;
            Caption = caption;
            Size = size;
            SortNum = sortNum;
            Type = type;
            Value = value;
        }

        /// <summary>
        /// Видимість поля
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// Назва
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Заголовок
        /// </summary>
        public string Caption { get; set; } = "";

        /// <summary>
        /// Розмір
        /// </summary>
        public uint Size { get; set; }

        /// <summary>
        /// Порядок поля в списку
        /// </summary>
        public int SortNum { get; set; } = 100;

        /// <summary>
        /// Тип
        /// </summary>
        public string Type { get; set; } = "";

        /// <summary>
        /// Значення
        /// </summary>
        public string Value { get; set; } = "";
    }
}