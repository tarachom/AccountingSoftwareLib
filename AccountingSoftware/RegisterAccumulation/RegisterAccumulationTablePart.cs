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
    /// Таблична частина
    /// </summary>
    public abstract class RegisterAccumulationTablePart
    {
        /// <summary>
        /// Таблична частина
        /// </summary>
        /// <param name="kernel">Ядро</param>
        /// <param name="table">Таблиця</param>
        /// <param name="fieldsArray">Масив полів</param>
        public RegisterAccumulationTablePart(Kernel kernel, string table, string[] fieldsArray)
        {
            Kernel = kernel;
            Table = table;
            FieldArray = fieldsArray;
        }

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
        /// Очистити вн. масив
        /// </summary>
        protected void BaseClear()
        {
            FieldValueList.Clear();
        }

        /// <summary>
        /// Прочитати значення у вн. масив
        /// </summary>
        protected async ValueTask BaseRead()
        {
            BaseClear();
            await Kernel.DataBase.SelectRegisterAccumulationTablePartRecords(Table, FieldArray, FieldValueList);
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
        /// Очистити табличну частину
        /// </summary>
        protected async ValueTask BaseDelete()
        {
            await Kernel.DataBase.DeleteRegisterAccumulationTablePartRecords(Table, TransactionID);
        }

        /// <summary>
        /// Записати значення в базу
        /// </summary>
        /// <param name="UID"></param>
        /// <param name="fieldValue"></param>
        protected async ValueTask<Guid> BaseSave(Guid UID, Dictionary<string, object> fieldValue)
        {
            Guid recordUnigueID = UID == Guid.Empty ? Guid.NewGuid() : UID;
            await Kernel.DataBase.InsertRegisterAccumulationTablePartRecords(recordUnigueID, Table, FieldArray, fieldValue, TransactionID);
            return recordUnigueID;
        }
    }
}