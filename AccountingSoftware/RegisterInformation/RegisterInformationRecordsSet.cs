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
    /// Регістр Інформації
    /// </summary>
    public abstract class RegisterInformationRecordsSet
    {
        public RegisterInformationRecordsSet(Kernel kernel, string table, string[] fieldsArray)
        {
            Kernel = kernel;
            Table = table;
            FieldArray = fieldsArray;

            QuerySelect = new Query(Table);
            QuerySelect.Field.AddRange(["period", "owner"]);
            QuerySelect.Field.AddRange(fieldsArray);
        }

        /// <summary>
        /// Запит SELECT
        /// </summary>
        public Query QuerySelect { get; set; }

        /// <summary>
        /// Ядро
        /// </summary>
        private Kernel Kernel { get; set; }

        /// <summary>
        /// Таблиця
        /// </summary>
        private string Table { get; set; }

        /// <summary>
        /// Масив полів
        /// </summary>
        private string[] FieldArray { get; set; }

        /// <summary>
        /// Масив значень полів
        /// </summary>
        protected List<Dictionary<string, object>> FieldValueList { get; private set; } = [];

        /// <summary>
        /// Додаткові поля
        /// </summary>
        public Dictionary<string, Dictionary<string, string>> JoinValue { get; private set; } = [];

        /// <summary>
        /// Очищення вн. списків
        /// </summary>
        protected void BaseClear()
        {
            FieldValueList.Clear();
        }

        /// <summary>
        /// Чи вже зчитувалися дані?
        /// </summary>
        public bool IsRead { get; private set; }

        /// <summary>
        /// Зчитування даних
        /// </summary>
        protected async ValueTask BaseRead()
        {
            BaseClear();
            JoinValue.Clear();

            await Kernel.DataBase.SelectRegisterInformationRecords(QuerySelect, FieldValueList, JoinValue);

            IsRead = true;
        }

        /// <summary>
        /// Обчислення розміру вибірки і обчислнення кількості сторінок
        /// </summary>
        /// <param name="unigueID">Вибраний елемент</param>
        /// <param name="pageSize">Розмір сторінки</param>
        /// <returns></returns>
        public async ValueTask<SplitSelectToPages_Record> SplitSelectToPages(UnigueID? unigueID, int pageSize = 1000)
        {
            return await Kernel.DataBase.SplitSelectToPages(QuerySelect, unigueID, pageSize);
        }

        private byte TransactionID = 0;

        protected async ValueTask BaseBeginTransaction()
        {
            TransactionID = await Kernel.DataBase.BeginTransaction();
        }

        protected async ValueTask BaseCommitTransaction()
        {
            await Kernel.DataBase.CommitTransaction(TransactionID);
            TransactionID = 0;
        }

        protected async ValueTask BaseRollbackTransaction()
        {
            await Kernel.DataBase.RollbackTransaction(TransactionID);
            TransactionID = 0;
        }

        /// <summary>
        /// Видалити запис
        /// </summary>
        /// <param name="UID">Ключ запису</param>
        protected async ValueTask BaseRemove(Guid UID)
        {
            UnigueID unigueID = new(UID);
            if (!unigueID.IsEmpty() && await Kernel.DataBase.IsExistUniqueID(unigueID, Table))
                await Kernel.DataBase.RemoveRegisterInformationRecords(UID,  Table, TransactionID);
        }

        /// <summary>
        /// Видалення записів для власника
        /// </summary>
        /// <param name="owner">Унікальний ідентифікатор власника</param>
        protected async ValueTask BaseDelete(Guid owner)
        {
            await Kernel.DataBase.DeleteRegisterInformationRecords(Table, owner, TransactionID);
        }

        /// <summary>
        /// Запис даних в регістр
        /// </summary>
        /// <param name="UID">Унікальний ідентифікатор</param>
        /// <param name="period">Період - дата запису або дата документу</param>
        /// <param name="owner">Власник запису</param>
        /// <param name="fieldValue">Значення полів</param>
        protected async ValueTask<Guid> BaseSave(Guid UID, DateTime period, Guid owner, Dictionary<string, object> fieldValue)
        {
            Guid recordUnigueID = UID == Guid.Empty ? Guid.NewGuid() : UID;
            await Kernel.DataBase.InsertRegisterInformationRecords(recordUnigueID, Table, period, owner, FieldArray, fieldValue, TransactionID);
            return recordUnigueID;
        }
    }
}