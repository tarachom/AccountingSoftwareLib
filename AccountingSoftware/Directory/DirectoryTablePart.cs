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
    /// Довідник Таблична частина
    /// </summary>
    public abstract class DirectoryTablePart
    {
        public DirectoryTablePart(Kernel kernel, string table, string[] fieldsArray, bool versionsHistory = false)
        {
            Kernel = kernel;
            Table = table;
            FieldArray = fieldsArray;
            VersionsHistory = versionsHistory;

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

        #region Version

        /// <summary>
        /// Унікальний ідентифікатор версії даних з власника таб. частини
        /// </summary>
        protected Guid OwnerVersionID { get; set; } = Guid.Empty;

        /// <summary>
        /// Базис із власника
        /// </summary>
        protected UuidAndText OwnerBasis { get; set; } = new UuidAndText();

        /// <summary>
        /// Вести історію версій значень полів
        /// </summary>
        private bool VersionsHistory { get; set; }

        #endregion

        /// <summary>
        /// Список даних
        /// </summary>
        protected List<Dictionary<string, object>> FieldValueList { get; private set; } = [];

        /// <summary>
        /// Значення додаткових полів
        /// </summary>
        public Dictionary<string, Dictionary<string, string>> JoinValue { get; private set; } = [];

        /// <summary>
        /// Очистити вн. списки
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
            {
                QuerySelect.Where.RemoveAll(w => w.Name == "owner");
                QuerySelect.Where.Add(new Where("owner", Comparison.EQ, ownerUnigueID.UGuid));
            }

            await Kernel.DataBase.SelectDirectoryTablePartRecords(QuerySelect, FieldValueList, JoinValue);

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
        /*
        protected async ValueTask BaseRemove(Guid UID, UnigueID ownerUnigueID)
        {
            UnigueID unigueID = new(UID);
            if (!unigueID.IsEmpty() && await Kernel.DataBase.IsExistUniqueID(unigueID, Table))
                await Kernel.DataBase.RemoveDirectoryTablePartRecords(UID, ownerUnigueID, Table, TransactionID);
        }
        */

        /// <summary>
        /// Видалити всі записи з таб. частини.
        /// Функція очищає всю таб. частину
        /// </summary>
        /// <param name="ownerUnigueID">Унікальний ідентифікатор власника таб. частини</param>
        protected async ValueTask BaseDelete(UnigueID ownerUnigueID)
        {
            await Kernel.DataBase.DeleteDirectoryTablePartRecords(ownerUnigueID, Table, TransactionID);
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

        protected async ValueTask IsExistOwnerVersion()
        {
            await Kernel.DataBase.SpetialTableObjectVersionsHistoryAddIfNotExist(OwnerVersionID, Kernel.User, OwnerBasis, TransactionID);

            //Очистка попередніх записів
            await Kernel.DataBase.SpetialTableTablePartVersionsHistoryRemoveBeforeSave(OwnerVersionID, OwnerBasis, Table, TransactionID);
        }

        /// <summary>
        /// Зберегти дані таб.частини. Добавляється один запис в таблицю.
        /// </summary>
        /// <param name="UID">Унікальний ідентифікатор запису</param>
        /// <param name="ownerUnigueID">Унікальний ідентифікатор власника таб. частини</param>
        /// <param name="fieldValue">Значення полів запису</param>
        protected async ValueTask<Guid> BaseSave(Guid UID, UnigueID ownerUnigueID, Dictionary<string, object> fieldValue)
        {
            Guid recordUnigueID = UID == Guid.Empty ? Guid.NewGuid() : UID;
            await Kernel.DataBase.InsertDirectoryTablePartRecords(recordUnigueID, ownerUnigueID, Table, FieldArray, fieldValue, TransactionID);

            //Записати в історію поточну версію значень полів
            if (VersionsHistory && OwnerVersionID != Guid.Empty && !OwnerBasis.IsEmpty())
                await Kernel.DataBase.SpetialTableTablePartVersionsHistoryAdd(OwnerVersionID, Kernel.User, OwnerBasis, Table, fieldValue);

            return recordUnigueID;
        }
    }
}