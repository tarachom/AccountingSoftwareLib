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
    public abstract class Select
    {
        /// <summary>
        /// Назва тимчасового поля для вибірки презентації вказівника
        /// </summary>
        protected const string PresentationTmpFieldName = "fld_special_presentation";

        public Select(Kernel kernel, string table, string parentField, string isFolderField, string[] fieldPresentation)
        {
            Kernel = kernel;
            Table = table;
            QuerySelect = new(table) { ParentField = parentField, IsFolderField = isFolderField };
            FieldPresentation = fieldPresentation;

            //Добавляється додаткове поле для отримання презентація вказівника
            QuerySelect.SpecialСoncatFields.Add(new ValueName<string[]>(FieldPresentation, PresentationTmpFieldName));
        }

        /// <summary>
        /// Ядро
        /// </summary>
        protected Kernel Kernel { get; private set; }

        /// <summary>
        /// Таблиця
        /// </summary>
        public string Table { get; protected set; }

        /// <summary>
		/// Запит SELECT
		/// </summary>
		public Query QuerySelect { get; set; }

        /// <summary>
        /// Поточна позиція
        /// </summary>
        protected int Position { get; set; } = 0;

        /// <summary>
        /// Презентація для поточного вказівника
        /// </summary>
        protected object? CurrentPointerPresentation { get; set; } = null;

        /// <summary>
        /// Поля для презентації
        /// </summary>
        protected string[] FieldPresentation { get; set; }

        /// <summary>
        /// Поточний вказівник
        /// </summary>
        protected (UniqueID UniqueID, Dictionary<string, object>? Fields)? CurrentPointerPosition { get; set; } = null;

        /// <summary>
		/// Вибірка вказівників
		/// </summary>
		protected List<(UniqueID UniqueID, Dictionary<string, object>? Fields)> BaseSelectList { get; private set; } = [];

        /// <summary>
        /// 
        /// </summary>
        protected Dictionary<string, ConfigurationField>.ValueCollection? ConfFields { get; set; } = null;

        /// <summary>
        /// Опрацьовує колекцію полів і перевіряє наявність полів
        /// </summary>
        protected void ExistFields()
        {
            List<string> newFields = new(QuerySelect.Field.Count);
            bool changed = false;

            foreach (var field in QuerySelect.Field)
            {
                string nameInTable = ExistField(field);
                newFields.Add(nameInTable);
                if (nameInTable != field && !changed) changed = true;
            }

            if (changed)
            {
                QuerySelect.Field.Clear();
                QuerySelect.Field.AddRange(newFields);
            }
        }

        // <summary>
        /// Перевіряє чи є в колекції полів поле з даною назвою. 
        /// Шукає як по назві в базі даних так і по звичайній назві. 
        /// Повертає назву поля як у базі даних.
        /// </summary>
        /// <param name="name">Назва поля</param>
        /// <returns>Назва поля як у базі даних</returns>
        /// <exception cref="KeyNotFoundException">Вибиває помилку якщо поле не знайдено</exception>
        protected string ExistField(string name) => name switch
        {
            "uid" or "deletion_label" or "spend" or "spend_date" => name,
            _ => ConfFields?.FirstOrDefault(x => x.NameInTable == name || x.Name == name)?.NameInTable ??
                throw new KeyNotFoundException($"Не знайдено поле '{name}' в колекції полів!")
        };

        /// <summary>
        /// Обчислення розміру вибірки і обчислення кількості сторінок
        /// </summary>
        /// <param name="uniqueID">Вибраний елемент</param>
        /// <param name="pageSize">Розмір сторінки</param>
        /// <returns></returns>
        public async Task<SplitSelectToPages_Record> SplitSelectToPages(UniqueID? uniqueID, int pageSize = 1000) =>
            await Kernel.DataBase.SplitSelectToPages(QuerySelect, uniqueID, pageSize);

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

                (_, Dictionary<string, object>? Fields) = CurrentPointerPosition.Value;
                if (Fields != null && Fields.TryGetValue(PresentationTmpFieldName, out object? presentation))
                {
                    // Отримую значення презентації
                    CurrentPointerPresentation = presentation;

                    //Видаляю поле презентації
                    Fields.Remove(PresentationTmpFieldName);
                }

                return true;
            }
            else
            {
                CurrentPointerPosition = null;
                CurrentPointerPresentation = null;
                return false;
            }
        }

        /// <summary>
        /// Кількість елементів у вибірці
        /// </summary>
        public int Count() => BaseSelectList.Count;
    }
}