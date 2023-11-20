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
    /// Довідник Таблична частина
    /// </summary>
    public abstract class DirectoryTablePart
    {
        public DirectoryTablePart(Kernel kernel, string table, string[] fieldsArray)
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
        /// Список даних
        /// </summary>
        protected List<Dictionary<string, object>> FieldValueList { get; private set; } = new List<Dictionary<string, object>>();

        /// <summary>
        /// Значення додаткових полів
        /// </summary>
        public Dictionary<string, Dictionary<string, string>> JoinValue { get; private set; } = new Dictionary<string, Dictionary<string, string>>();

        /// <summary>
        /// Очистити вн. списки
        /// </summary>
        protected void BaseClear()
        {
            FieldValueList.Clear();
        }

        /// <summary>
        /// Зчитати дані з бази даних
        /// </summary>
        /// <param name="ownerUnigueID"></param>
        protected async ValueTask BaseRead(UnigueID ownerUnigueID)
        {
            BaseClear();

            JoinValue.Clear();

            QuerySelect.Where.Clear();
            QuerySelect.Where.Add(new Where("owner", Comparison.EQ, ownerUnigueID.UGuid));

            await Kernel.DataBase.SelectDirectoryTablePartRecords(QuerySelect, FieldValueList);

            //Якщо задані додаткові поля з псевдонімами, їх потрібно зчитати в список JoinValue
            if (QuerySelect.FieldAndAlias.Count > 0)
            {
                foreach (Dictionary<string, object> fieldValue in FieldValueList)
                {
                    Dictionary<string, string> joinFieldValue = new Dictionary<string, string>();
                    JoinValue.Add(fieldValue["uid"].ToString() ?? "", joinFieldValue);

                    foreach (NameValue<string> fieldAndAlias in QuerySelect.FieldAndAlias)
                        joinFieldValue.Add(fieldAndAlias.Value ?? "", fieldValue[fieldAndAlias.Value ?? ""].ToString() ?? "");
                }
            }
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
        /// Видалити всі записи з таб. частини.
        /// Функція очищає всю таб. частину
        /// </summary>
        /// <param name="ownerUnigueID">Унікальний ідентифікатор власника таб. частини</param>
        protected async ValueTask BaseDelete(UnigueID ownerUnigueID)
        {
            await Kernel.DataBase.DeleteDirectoryTablePartRecords(ownerUnigueID, Table, TransactionID);
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
            return recordUnigueID;
        }
    }
}