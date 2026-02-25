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
    /// Константа Таблична частина
    /// </summary>
    public abstract class ConstantsTablePart
    {
        /// <summary>
        /// Константа Таблична частина
        /// </summary>
        /// <param name="kernel">Ядро</param>
        /// <param name="table">Таблиця</param>
        /// <param name="fieldsArray">Масив полів</param>
        public ConstantsTablePart(Kernel kernel, string table, string[] fieldsArray)
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
        /// Масив полів
        /// </summary>
        private string[] FieldArray { get; set; }

        /// <summary>
        /// Масив полів та значеннь
        /// </summary>
        protected List<Dictionary<string, object>> FieldValueList { get; private set; } = [];

        /// <summary>
        /// Значення додаткових полів
        /// </summary>
        public Dictionary<string, Dictionary<string, string>> JoinValue { get; private set; } = [];

        /// <summary>
        /// Очистити вн. масив
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
        /// Прочитати значення у вн. масив
        /// </summary>
        protected async ValueTask BaseRead()
        {
            BaseClear();
            JoinValue.Clear();

            await Kernel.DataBase.SelectConstantsTablePartRecords(QuerySelect, FieldArray, FieldValueList, JoinValue);

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
        /// <exception cref="Exception"></exception>
        protected async ValueTask BaseRemove(Guid UID)
        {
            UniqueID uniqueID = new(UID);
            if (!uniqueID.IsEmpty() && await Kernel.DataBase.IsExistUniqueID(uniqueID, Table))
                await Kernel.DataBase.RemoveConstantsTablePartRecords(UID, Table, TransactionID);
        }

        /// <summary>
        /// Очистити табличну частину
        /// </summary>
        protected async ValueTask BaseDelete()
        {
            await Kernel.DataBase.DeleteConstantsTablePartRecords(Table, TransactionID);
        }

        /// <summary>
        /// Записати значення в базу
        /// </summary>
        /// <param name="UID"></param>
        /// <param name="fieldValue"></param>
        protected async ValueTask<Guid> BaseSave(Guid UID, Dictionary<string, object> fieldValue)
        {
            Guid recordUnigueID = UID == Guid.Empty ? Guid.NewGuid() : UID;
            await Kernel.DataBase.InsertConstantsTablePartRecords(recordUnigueID, Table, FieldArray, fieldValue, TransactionID);
            return recordUnigueID;
        }
    }
}