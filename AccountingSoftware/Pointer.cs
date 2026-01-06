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
        public UnigueID UnigueID { get; protected set; } = new();

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
        protected void Init(UnigueID uid, Dictionary<string, object>? fields = null)
        {
            UnigueID = uid;
            Fields = fields ?? [];
            Name = "";
        }

        /// <summary>
        /// Очищення
        /// </summary>
        public void Clear()
        {
            Init(new UnigueID());
        }

        /// <summary>
        /// Чи це пустий ідентифікатор
        /// </summary>
        public bool IsEmpty()
        {
            return UnigueID.IsEmpty();
        }

        /// <summary>
        /// Отримати ідентифікатор
        /// </summary>
        public Guid GetPointer()
        {
            return UnigueID.UGuid;
        }

        /// <summary>
        /// Переоприділення базової функції
        /// </summary>
        /// <returns>Ідентифікатор у вигляді тексту</returns>
        public override string ToString()
        {
            return UnigueID.UGuid.ToString();
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(UnigueID.UGuid);
        }

        public override bool Equals(object? obj)
        {
            return obj != null && UnigueID.UGuid == ((Pointer)obj).UnigueID.UGuid;
        }
    }
}