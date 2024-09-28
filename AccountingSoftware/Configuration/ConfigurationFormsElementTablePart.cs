/*
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
    /// Табличні частини для форми елементу
    /// </summary>
    public class ConfigurationFormsElementTablePart
    {
        /// <summary>
        /// Табличні частини для форми елементу
        /// </summary>
        public ConfigurationFormsElementTablePart() { }

        /// <summary>
        /// Табличні частини для форми елементу
        /// </summary>
        /// <param name="name">Назва</param>
        /// <param name="caption">Заголовок</param>
        public ConfigurationFormsElementTablePart(string name, string caption = "", uint size = 0, uint height = 0, int sortNum = 100)
        {
            Name = name;
            Caption = caption;
            Size = size;
            SortNum = sortNum;
            Height = height;
        }

        /// <summary>
        /// Назва
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Заголовок
        /// </summary>
        public string Caption { get; set; } = "";

        /// <summary>
        /// Ширина
        /// </summary>
        public uint Size { get; set; }

        /// <summary>
        /// Висота
        /// </summary>
        public uint Height { get; set; }

        /// <summary>
        /// Порядок поля в списку
        /// </summary>
        public int SortNum { get; set; } = 100;
    }
}