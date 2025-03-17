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
    /// Вибірка
    /// 
    /// Основа для класів:
    ///    DirectorySelect
    ///    DirectorySelectHierarchical
    ///    DocumentSelect
    /// </summary>
    /// <param name="kernel">Ядро</param>
    /// <param name="table">Таблиця</param>
    /// <param name="parentField">Батьківське поле</param>
    public abstract class Select(Kernel kernel, string table, string parentField = "")
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
		/// Запит SELECT
		/// </summary>
		public Query QuerySelect { get; set; } = new Query(table) { ParentField = parentField };

        /// <summary>
        /// Поточна позиція
        /// </summary>
        protected int Position { get; set; } = 0;

        /// <summary>
        /// Поточний вказівник
        /// </summary>
        protected (UnigueID UnigueID, Dictionary<string, object>? Fields)? CurrentPointerPosition { get; set; } = null;

        /// <summary>
		/// Вибірка вказівників
		/// </summary>
		protected List<(UnigueID UnigueID, Dictionary<string, object>? Fields)> BaseSelectList { get; private set; } = [];

        /// <summary>
        /// Перейти на початок вибірки
        /// </summary>
        public void MoveToFirst()
        {
            Position = 0;
            MoveToPosition();
        }

        /// <summary>
        /// Переміститися на одну позицію у вибірці
        /// </summary>
        protected virtual bool MoveToPosition()
        {
            if (Position < BaseSelectList.Count)
            {
                CurrentPointerPosition = BaseSelectList[Position++];
                return true;
            }
            else
            {
                CurrentPointerPosition = null;
                return false;
            }
        }

        /// <summary>
        /// Кількість елементів у вибірці
        /// </summary>
        public int Count()
        {
            return BaseSelectList.Count;
        }
    }
}