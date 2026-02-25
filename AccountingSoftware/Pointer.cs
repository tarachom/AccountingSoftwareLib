/*
Copyright (C) 2019-2026 TARAKHOMYN YURIY IVANOVYCH
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
    /// Вказівник
    /// 
    /// Основа для класів:
    ///     DirectoryPointer
    ///     DocumentPointer
    /// </summary>
    public abstract class Pointer(Kernel kernel, string table)
    {
        /// <summary>
        /// Ядро
        /// </summary>
        protected Kernel Kernel { get; set; } = kernel;

        /// <summary>
        /// Таблиця
        /// </summary>
        public string Table { get; protected set; } = table;

        /// <summary>
        /// Унікальний ідентифікатор запису
        /// </summary>
        public UniqueID UniqueID { get; protected set; } = new();

        /// <summary>
        /// Поля які були додатково прочитані з бази даних
        /// </summary>
        public Dictionary<string, object> Fields { get; protected set; } = [];

        /// <summary>
        /// Назва
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Ініціалізація вказівника
        /// </summary>
        /// <param name="uid">Унікальний ідентифікатор</param>
        /// <param name="fields">Поля які потрібно додатково зчитати</param>
        protected void Init(UniqueID uid, Dictionary<string, object>? fields = null)
        {
            UniqueID = uid;
            Fields = fields ?? [];
            Name = "";
        }

        /// <summary>
        /// Очищення
        /// </summary>
        public void Clear()
        {
            Init(new UniqueID());
        }

        /// <summary>
        /// Чи це пустий ідентифікатор
        /// </summary>
        public bool IsEmpty()
        {
            return UniqueID.IsEmpty();
        }

        /// <summary>
        /// Отримати ідентифікатор
        /// </summary>
        public Guid GetPointer()
        {
            return UniqueID.UGuid;
        }

        /// <summary>
        /// Переоприділення базової функції
        /// </summary>
        /// <returns>Ідентифікатор у вигляді тексту</returns>
        public override string ToString()
        {
            return UniqueID.UGuid.ToString();
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(UniqueID.UGuid);
        }

        public override bool Equals(object? obj)
        {
            return obj != null && UniqueID.UGuid == ((Pointer)obj).UniqueID.UGuid;
        }
    }
}