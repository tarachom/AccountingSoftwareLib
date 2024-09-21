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
    /// Документ Об'єкт
    /// </summary>
    public abstract class DocumentObject
    {
        public DocumentObject(Kernel kernel, string table, string typeDocument, string[] fieldsArray)
        {
            Kernel = kernel;
            Table = table;
            TypeDocument = typeDocument;
            FieldArray = fieldsArray;

            foreach (string field in FieldArray)
                FieldValue.Add(field, new object());
        }

        /// <summary>
        /// Ядро
        /// </summary>
        private Kernel Kernel { get; set; }

        /// <summary>
        /// Таблиця
        /// </summary>
        public string Table { get; private set; }

        /// <summary>
        /// Назва як задано в конфігураторі
        /// </summary>
        public string TypeDocument { get; private set; }

        /// <summary>
        /// Масив назв полів
        /// </summary>
        private string[] FieldArray { get; set; }

        /// <summary>
        /// Значення полів
        /// </summary>
        protected Dictionary<string, object> FieldValue { get; set; } = [];

        /// <summary>
        /// Унікальний ідентифікатор запису
        /// </summary>
        public UnigueID UnigueID { get; private set; } = new UnigueID();

        /// <summary>
        /// Документ проведений
        /// </summary>
        public bool Spend { get; private set; }

        /// <summary>
        /// Дата проведення документу
        /// </summary>
        public DateTime SpendDate { get; private set; } = DateTime.MinValue;

        /// <summary>
        /// Мітка видалення
        /// </summary>
        public bool DeletionLabel { get; private set; }

        /// <summary>
        /// Чи це новий?
        /// </summary>
        public bool IsNew { get; private set; }

        /// <summary>
        /// Новий обєкт
        /// </summary>
        protected void BaseNew()
        {
            UnigueID = UnigueID.NewUnigueID();
            IsNew = true;
            IsSave = false;
        }

        /// <summary>
        /// Чи вже записаний документ
        /// </summary>
        public bool IsSave { get; private set; }

        /// <summary>
        /// Чи прочитані тільки базові поля?
        /// </summary>
        public bool IsReadOnlyBaseFields { get; private set; }

        /// <summary>
        /// Очистка вн. списку
        /// </summary>
        protected void BaseClear()
        {
            foreach (string field in FieldArray)
                FieldValue[field] = new object();
        }

        /// <summary>
        /// Зчитати дані
        /// </summary>
        /// <param name="uid">Унікальний ідентифікатор </param>
        protected async ValueTask<bool> BaseRead(UnigueID uid, bool readOnlyBaseFields = false)
        {
            BaseClear();

            if (uid.IsEmpty() || IsNew == true)
                return false;

            var record = await Kernel.DataBase.SelectDocumentObject(uid, Table, readOnlyBaseFields ? [] : FieldArray, FieldValue);

            if (record.Result)
            {
                UnigueID = uid;
                DeletionLabel = record.DeletionLabel;
                Spend = record.Spend;
                SpendDate = record.SpendDate;

                IsSave = true;
                IsReadOnlyBaseFields = readOnlyBaseFields;

                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Зберегти дані
        /// </summary>
        protected async ValueTask<bool> BaseSave()
        {
            bool result;

            if (IsNew)
            {
                result = await Kernel.DataBase.InsertDocumentObject(UnigueID, Spend, SpendDate, Table, FieldArray, FieldValue);
                if (result) IsNew = false;
            }
            else
            {
                if (!UnigueID.IsEmpty() && await Kernel.DataBase.IsExistUniqueID(UnigueID, Table))
                    result = await Kernel.DataBase.UpdateDocumentObject(UnigueID, DeletionLabel, Spend, SpendDate, Table, FieldArray, FieldValue);
                else
                    throw new Exception("Спроба записати неіснуючий документ");
            }

            IsSave = result;
            BaseClear();

            return result;
        }

        /// <summary>
        /// Запис даних полів в таблицю для повнотекстового пошуку
        /// </summary>
        /// <param name="obj">Обєкт</param>
        /// <param name="values">Масив значень полів</param>
        protected async ValueTask BaseWriteFullTextSearch(UuidAndText obj, string[] values)
        {
            if (values.Length != 0)
                await Kernel.DataBase.SpetialTableFullTextSearchAddValue(obj, string.Join(" ", values), Kernel.Conf.DictTSearch);
        }

        protected async ValueTask BaseAddIgnoreDocumentList()
        {
            await Kernel.DataBase.SpetialTableRegAccumTrigerDocIgnoreAdd(Kernel.User, Kernel.Session, UnigueID.UGuid, "");
        }

        protected async ValueTask BaseRemoveIgnoreDocumentList()
        {
            await Kernel.DataBase.SpetialTableRegAccumTrigerDocIgnoreClear(Kernel.User, Kernel.Session, UnigueID.UGuid);
        }

        protected async ValueTask BaseSpend(bool spend, DateTime spend_date)
        {
            Spend = spend;
            SpendDate = spend_date;

            if (IsSave)
                await Kernel.DataBase.UpdateDocumentObject(UnigueID, Spend ? false : DeletionLabel, Spend, SpendDate, Table, null, null);
            else
                throw new Exception("Документ спочатку треба записати, а потім вже провести");
        }

        /// <summary>
        /// Встановлення мітки на видалення
        /// </summary>
        /// <param name="label">Мітка</param>
        /// <exception cref="Exception">Не записаний</exception>
        protected async ValueTask BaseDeletionLabel(bool label)
        {
            DeletionLabel = label;

            if (IsSave)
            {
                //Обновлення поля deletion_label елементу, решта полів не зачіпаються
                await Kernel.DataBase.UpdateDocumentObject(UnigueID, DeletionLabel, null, null, Table, null, null);

                //Видалення з повнотекстового пошуку
                /* if (DeletionLabel)
                    await Kernel.DataBase.SpetialTableFullTextSearchDelete(UnigueID, 0); */
            }
            else
                throw new Exception("Документ спочатку треба записати, а потім вже встановлювати мітку видалення");
        }

        /// <summary>
        /// Видалити запис
        /// </summary>
        /// <param name="tablePartsTables">Список таблиць табличних частин</param>
        protected async ValueTask BaseDelete(string[] tablePartsTables)
        {
            byte TransactionID = await Kernel.DataBase.BeginTransaction();

            //Видалити сам документ
            await Kernel.DataBase.DeleteDocumentObject(UnigueID, Table, TransactionID);

            //Видалення даних з табличних частин
            foreach (string tablePartsTable in tablePartsTables)
                await Kernel.DataBase.DeleteDocumentTablePartRecords(UnigueID, tablePartsTable, TransactionID);

            //Видалення з повнотекстового пошуку
            await Kernel.DataBase.SpetialTableFullTextSearchDelete(UnigueID, TransactionID);

            await Kernel.DataBase.CommitTransaction(TransactionID);

            BaseClear();
        }

        /// <summary>
        /// Представлення обєкта
        /// </summary>
        /// <param name="fieldPresentation">Масив полів які представляють обєкт (Наприклад Назва, Дата, Номер і т.д)</param>
        /// <returns>Представлення обєкта</returns>
        protected async ValueTask<string> BasePresentation(string[] fieldPresentation)
        {
            if (Kernel != null && !UnigueID.IsEmpty() && IsSave && fieldPresentation.Length != 0)
            {
                Query query = new(Table);
                query.Field.AddRange(fieldPresentation);

                //Відбір по uid
                query.Where.Add(new Where("uid", Comparison.EQ, UnigueID.UGuid));

                return await Kernel.DataBase.GetDocumentPresentation(query, fieldPresentation);
            }
            else return "";
        }
    }
}