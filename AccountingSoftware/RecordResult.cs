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

/*
Структури для повернення результатів з функцій в класі PostgreSQL
*/

namespace AccountingSoftware
{
    #region Розбивка вибірки на сторінки

    public record SplitSelectToPages_Record
    {
        /// <summary>
        /// Результат функції
        /// </summary>
        public bool Result;

        /// <summary>
        /// Позиція вибраного елементу у вибірці
        /// </summary>
        public long Position;

        /// <summary>
        /// Розмір вибірки
        /// </summary>
        public long Count;

        /// <summary>
        /// Кількість сторінок
        /// </summary>
        public int Pages;

        /// <summary>
        /// Поточна знайдена сторінка
        /// </summary>
        public int CurrentPage;

        /// <summary>
        /// Кількість елементів на сторінку
        /// </summary>
        public int PageSize;
    }

    #endregion

    #region LockedObject

    /// <summary>
    /// Інформація про блокування
    /// </summary>
    public record LockedObject_Record
    {
        /// <summary>
        /// Результат функції
        /// </summary>
        public bool Result;

        /// <summary>
        /// Ключ блокування
        /// </summary>
        public UnigueID? LockKey;

        /// <summary>
        /// Дата блокування
        /// </summary>
        public DateTime DateLock;

        /// <summary>
        /// Назва користувача
        /// </summary>
        public string UserName = "";
    }

    #endregion

    #region VersionsHistory

    public record SelectVersionsHistoryList_Record
    {
        /// <summary>
        /// Результат функції
        /// </summary>
        public bool Result;

        public UuidAndText Obj = new UuidAndText();

        public List<Row> ListRow = [];

        public record Row()
        {
            public Guid VersionID = Guid.Empty;
            public DateTime DateWrite = DateTime.MinValue;
            public Guid UserID = Guid.Empty;
            public string UserName = "";
            public char Operation = 'U';
            public string Info = "";
        }
    }

    public record SelectVersionsHistoryItem_Record
    {
        /// <summary>
        /// Результат функції
        /// </summary>
        public bool Result;

        public Guid VersionID = Guid.Empty;

        public DateTime DateWrite = DateTime.MinValue;

        public Guid UserID = Guid.Empty;

        public UuidAndText Obj = new UuidAndText();

        public NameAndText[] Fields = [];

        public Dictionary<string, string> GetDictionaryFields()
        {
            Dictionary<string, string> dictionaryFields = [];
            foreach (var field in Fields)
                dictionaryFields.Add(field.Name, field.Text);

            return dictionaryFields;
        }
    }

    public record SelectVersionsHistoryTablePart_Record
    {
        /// <summary>
        /// Результат функції
        /// </summary>
        public bool Result;

        public Guid VersionID = Guid.Empty;

        public UuidAndText Obj = new UuidAndText();

        public List<Row> ListRow = [];

        public record Row()
        {
            public DateTime DateWrite = DateTime.MinValue;

            public Guid UserID = Guid.Empty;

            public string TablePart = "";

            public NameAndText[] Fields = [];

            public Dictionary<string, string> GetDictionaryFields()
            {
                Dictionary<string, string> dictionaryFields = [];
                foreach (var field in Fields)
                    dictionaryFields.Add(field.Name, field.Text);

                return dictionaryFields;
            }
        }
    }

    #endregion

    #region CompositePointer

    /// <summary>
    /// Структура для повернення результату з функції CompositePointerPresentation яка знаходиться в згенерованому коді CodeGeneration.cs
    /// </summary>
    public record CompositePointerPresentation_Record
    {
        /// <summary>
        /// Презентація
        /// </summary>
        public string result = "";

        /// <summary>
        /// Вказівник (Довідники, Документи)
        /// </summary>
        public string pointer = "";

        /// <summary>
        /// Тип довідника чи документу
        /// </summary>
        public string type = "";
    }

    #endregion

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

    #region РегістриІнформації

    /// <summary>
    /// Структура для повернення результату з функції зчитування об'єкту документу
    /// </summary>
    public record SelectRegisterInformationObject_Record
    {
        /// <summary>
        /// Результат функції
        /// </summary>
        public bool Result;

        /// <summary>
        /// Період
        /// </summary>
        public DateTime Period { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Власник
        /// </summary>
        public Guid Owner { get; set; } = Guid.Empty;
    }

    #endregion

    #region Вибірка даних

    /// <summary>
    /// Структура для повернення результату з функції SelectRequestAsync
    /// </summary>
    public record SelectRequest_Record
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