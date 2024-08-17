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
    /// Документ Таблична частина
    /// </summary>
    public abstract class DocumentTablePart
    {
        public DocumentTablePart(Kernel kernel, string table, string[] fieldsArray)
        {
            Kernel = kernel;
            Table = table;
            FieldArray = fieldsArray;

            QuerySelect = new Query(Table);
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
        /// Масив назв полів
        /// </summary>
        private string[] FieldArray { get; set; }

        /// <summary>
        /// Значення полів
        /// </summary>
        protected List<Dictionary<string, object>> FieldValueList { get; private set; } = [];

        /// <summary>
        /// Значення додаткових полів
        /// </summary>
        public Dictionary<string, Dictionary<string, string>> JoinValue { get; private set; } = [];

        /// <summary>
        /// Очистка вн. списків
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
        /// Зчитати дані з бази даних
        /// </summary>
        /// <param name="ownerUnigueID"></param>
        protected async ValueTask BaseRead(UnigueID ownerUnigueID)
        {
            BaseClear();
            JoinValue.Clear();
            
            //Відбір по власнику
            QuerySelect.Where.Clear();
            QuerySelect.Where.Add(new Where("owner", Comparison.EQ, ownerUnigueID.UGuid));

            await Kernel.DataBase.SelectDocumentTablePartRecords(QuerySelect, FieldValueList, JoinValue);

            IsRead = true;
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
        /// <param name="UID">Ключ</param>
        /// <param name="ownerUnigueID">Вкласник</param>
        /// <exception cref="Exception"></exception>
        protected async ValueTask BaseRemove(Guid UID, UnigueID ownerUnigueID)
        {
            UnigueID unigueID = new(UID);
            if (!unigueID.IsEmpty() && await Kernel.DataBase.IsExistUniqueID(unigueID, Table))
                await Kernel.DataBase.RemoveDocumentTablePartRecords(UID, ownerUnigueID, Table, TransactionID);
            else
                throw new Exception("Спроба видалити неіснуючий елемент табличної частини");
        }

        /// <summary>
        /// Видалити всі дані з таб. частини
        /// </summary>
        /// <param name="ownerUnigueID">Унікальний ідентифікатор власника таб. частини</param>
        protected async ValueTask BaseDelete(UnigueID ownerUnigueID)
        {
            await Kernel.DataBase.DeleteDocumentTablePartRecords(ownerUnigueID, Table, TransactionID);
        }

        /// <summary>
        /// Перевірка наявності запису власника
        /// </summary>
        /// <param name="ownerUnigueID">Ід власника</param>
        /// <param name="ownerTable">Таблиця власника</param>
        protected async ValueTask<bool> IsExistOwner(UnigueID ownerUnigueID, string ownerTable)
        {
            return await Kernel.DataBase.IsExistUniqueID(ownerUnigueID, ownerTable);
        }

        /// <summary>
        /// Зберегти один запис таб частини
        /// </summary>
        /// <param name="UID">Унікальний ідентифікатор запису</param>
        /// <param name="ownerUnigueID">Унікальний ідентифікатор власника таб. частини</param>
        /// <param name="fieldValue">Список значень полів</param>
        protected async ValueTask<Guid> BaseSave(Guid UID, UnigueID ownerUnigueID, Dictionary<string, object> fieldValue)
        {
            Guid recordUnigueID = UID == Guid.Empty ? Guid.NewGuid() : UID;
            await Kernel.DataBase.InsertDocumentTablePartRecords(recordUnigueID, ownerUnigueID, Table, FieldArray, fieldValue, TransactionID);
            return recordUnigueID;
        }
    }
}