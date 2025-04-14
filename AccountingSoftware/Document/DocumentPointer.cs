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
    /// Документ Вказівник
    /// </summary>
    public abstract class DocumentPointer(Kernel kernel, string table, string typeDocument) : Pointer(kernel, table)
    {
        /// <summary>
        /// Назва типу як задано в конфігураторі
        /// </summary>
        public string TypeDocument { get; private set; } = typeDocument;

        /// <summary>
        /// Представлення обєкта
        /// </summary>
        /// <param name="fieldPresentation">Масив полів які представляють обєкт (Наприклад Назва, Дата, Номер і т.д)</param>
        /// <returns>Представлення обєкта</returns>
        protected async ValueTask<string> BasePresentation(string[] fieldPresentation)
        {
            if (!IsEmpty() && fieldPresentation.Length != 0)
            {
                Query query = new(Table);
                query.Field.AddRange(fieldPresentation);
                query.Where.Add(new Where("uid", Comparison.EQ, UnigueID.UGuid)); //Відбір по uid

                return await Kernel.DataBase.GetDocumentPresentation(query, fieldPresentation);
            }
            else
                return "";
        }

        /// <summary>
        /// Повертає признак що документ проведений
        /// </summary>
        protected async ValueTask<bool?> BaseIsSpend()
        {
            if (!IsEmpty())
            {
                var record = await Kernel.DataBase.SelectDocumentObject(this.UnigueID, Table, []);
                return record.Result ? record.Spend : null;
            }
            else
                return null;
        }

        /// <summary>
        /// Повертає признак що документ проведний і дату проведення
        /// </summary>
        protected async ValueTask<(bool? Spend, DateTime SpendDate)> BaseGetSpend()
        {
            if (!IsEmpty())
            {
                var record = await Kernel.DataBase.SelectDocumentObject(this.UnigueID, Table, []);
                return record.Result ? (record.Spend, record.SpendDate) : (null, DateTime.MinValue);
            }
            else
                return (null, DateTime.MinValue);
        }

        /// <summary>
        /// Проведення
        /// </summary>
        /// <param name="spend">Мітка проведення</param>
        /// <param name="spend_date">Дата проведення</param>
        protected async ValueTask BaseSpend(bool spend, DateTime spend_date)
        {
            if (!IsEmpty())
            {
                await Kernel.DataBase.UpdateDocumentObject(UnigueID, spend ? false : null, spend, spend_date, Table, null, null);

                //Тригер оновлення обєкту
                await Kernel.DataBase.SpetialTableObjectUpdateTrigerAdd(GetBasis());
            }
        }

        /// <summary>
        /// Повертає мітку на видалення або null якщо вказівник пустий або не вдалось прочитати
        /// </summary>
        protected async ValueTask<bool?> BaseGetDeletionLabel()
        {
            if (!IsEmpty())
            {
                var record = await Kernel.DataBase.SelectDocumentObject(this.UnigueID, Table, []);
                return record.Result ? record.DeletionLabel : null;
            }
            else
                return null;
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

                //Тригер оновлення обєкту
                await Kernel.DataBase.SpetialTableObjectUpdateTrigerAdd(GetBasis());
            }
        }

        /// <summary>
        /// Для композитного типу даних
        /// </summary>
        public virtual UuidAndText GetBasis()
        {
            return new UuidAndText(UnigueID, $"Документи.{TypeDocument}");
        }
    }
}