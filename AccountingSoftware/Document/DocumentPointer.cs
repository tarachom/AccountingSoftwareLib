/*
Copyright (C) 2019-2023 TARAKHOMYN YURIY IVANOVYCH
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
    /// Документ Вказівник
    /// </summary>
    public class DocumentPointer
    {
        public DocumentPointer() { }

        public DocumentPointer(Kernel kernel, string table, string typeDocument)
        {
            Table = table;
            Kernel = kernel;
            TypeDocument = typeDocument;
        }

        /// <summary>
        /// Ініціалізація вказівника
        /// </summary>
        /// <param name="uid">Унікальний ідентифікатор</param>
        /// <param name="fields">Поля які потрібно додатково зчитати</param>
        public void Init(UnigueID uid, Dictionary<string, object>? fields = null)
        {
            UnigueID = uid;
            Fields = fields;
        }

        /// <summary>
        /// Ядро
        /// </summary>
        private Kernel? Kernel { get; set; }

        /// <summary>
        /// Таблиця
        /// </summary>
        private string Table { get; set; } = "";

        /// <summary>
        /// Назва як задано в конфігураторі
        /// </summary>
        public string TypeDocument { get; private set; } = "";

        /// <summary>
        /// Унікальний ідентифікатор запису
        /// </summary>
        public UnigueID UnigueID { get; private set; } = new UnigueID();

        /// <summary>
        /// Поля які потрібно додатково зчитати
        /// </summary>
        public Dictionary<string, object>? Fields { get; private set; }

        /// <summary>
        /// Чи пустий ідентифікатор?
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return UnigueID.IsEmpty();
        }

        /// <summary>
        /// Отримати ідентифікатор
        /// </summary>
        /// <returns></returns>
        public Guid GetPointer()
        {
            return UnigueID.UGuid;
        }

        /// <summary>
        /// Представлення обєкта
        /// </summary>
        /// <param name="fieldPresentation">Масив полів які представляють обєкт (Наприклад Назва, Дата, Номер і т.д)</param>
        /// <returns>Представлення обєкта</returns>
        protected string BasePresentation(string[] fieldPresentation)
        {
            if (Kernel != null && !IsEmpty() && fieldPresentation.Length != 0)
            {
                Query query = new Query(Table);
                query.Field.AddRange(fieldPresentation);
                query.Where.Add(new Where("uid", Comparison.EQ, UnigueID.UGuid));

                return Kernel.DataBase.GetDocumentPresentation(query, fieldPresentation);
            }
            else return "";
        }

        /// <summary>
        /// Проведення
        /// </summary>
        /// <param name="spend">Мітка проведення</param>
        /// <param name="spend_date">Дата проведення</param>
        protected async ValueTask BaseSpend(bool spend, DateTime spend_date)
        {
            if (Kernel != null && !IsEmpty())
                await Kernel.DataBase.UpdateDocumentObject(UnigueID, (spend ? false : null), spend, spend_date, Table, null, null);
        }

        /// <summary>
        /// Встановлення мітки на видалення
        /// </summary>
        /// <param name="label">Мітка</param>
        /// <exception cref="Exception">Не записаний</exception>
        protected async ValueTask BaseDeletionLabel(bool label)
        {
            if (Kernel != null && !IsEmpty())
            {
                //Обновлення поля deletion_label елементу, решта полів не зачіпаються
                await Kernel.DataBase.UpdateDocumentObject(UnigueID, label, null, null, Table, null, null);

                //Видалення з повнотекстового пошуку
                if (label)
                    Kernel.DataBase.SpetialTableFullTextSearchDelete(UnigueID, 0);
            }
        }
    }
}