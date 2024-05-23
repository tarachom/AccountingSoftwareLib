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

/*
Структури для повернення результатів з функцій в класі PostgreSQL
*/

namespace AccountingSoftware
{
    #region Константи

    /// <summary>
    /// Структура для повернення результату з функції зчитування константи
    /// </summary>
    public record SelectConstants_Record
    {
        /// <summary>
        /// Результат функції
        /// </summary>
        public bool Result;

        /// <summary>
        /// Значення
        /// </summary>
        public object Value = new();
    }

    #endregion

    #region Довідники

    /// <summary>
    /// Структура для повернення результату з функції зчитування об'єкту довідника
    /// </summary>
    public record SelectDirectoryObject_Record
    {
        /// <summary>
        /// Результат функції
        /// </summary>
        public bool Result;

        /// <summary>
        /// Помітка на видалення
        /// </summary>
        public bool DeletionLabel;
    }

    #endregion

    #region Документи

    /// <summary>
    /// Структура для повернення результату з функції зчитування об'єкту документу
    /// </summary>
    public record SelectDocumentObject_Record
    {
        /// <summary>
        /// Результат функції
        /// </summary>
        public bool Result;

        /// <summary>
        /// Помітка на видалення
        /// </summary>
        public bool DeletionLabel;

        /// <summary>
        /// Чи проведений документ
        /// </summary>
        public bool Spend;

        /// <summary>
        /// Дата проведення документу
        /// </summary>
        public DateTime SpendDate = DateTime.MinValue;
    }

    #endregion

    #region Вибірка даних

    /// <summary>
    /// Структура для повернення результату з функції SelectRequestAsync
    /// </summary>
    public record SelectRequestAsync_Record
    {
        /// <summary>
        /// Результат функції
        /// </summary>
        public bool Result;

        /// <summary>
        /// Колонки
        /// </summary>
        public string[] ColumnsName = [];

        /// <summary>
        /// Список рядків
        /// </summary>
        public List<Dictionary<string, object>> ListRow = [];
    }

    #endregion
}