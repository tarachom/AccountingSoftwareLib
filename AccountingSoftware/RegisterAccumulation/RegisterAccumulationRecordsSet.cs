﻿/*
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
    /// Регістр накопичення
    /// </summary>
    public abstract class RegisterAccumulationRecordsSet
    {
        public RegisterAccumulationRecordsSet(Kernel kernel, string table, string typeRegAccum, string[] fieldsArray)
        {
            Kernel = kernel;
            Table = table;
            TypeRegAccum = typeRegAccum;
            FieldArray = fieldsArray;

            QuerySelect = new Query(Table);
            QuerySelect.Field.AddRange(["period", "income", "owner", "ownertype"]);
            QuerySelect.Field.AddRange(fieldsArray);
        }

        /// <summary>
        /// Назва як задано в конфігураторі
        /// </summary>
        public string TypeRegAccum { get; private set; }

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
        /// Список полів
        /// </summary>
        private string[] FieldArray { get; set; }

        /// <summary>
        /// Значення полів
        /// </summary>
        protected List<Dictionary<string, object>> FieldValueList { get; private set; } = [];

        /// <summary>
        /// Додаткові поля
        /// </summary>
        public Dictionary<string, Dictionary<string, string>> JoinValue { get; private set; } = [];

        /// <summary>
        /// Очитска вн. списків
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

            await Kernel.DataBase.SelectRegisterAccumulationRecords(QuerySelect, FieldValueList, JoinValue);

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
        /// Видалення записів для власника
        /// </summary>
        /// <param name="owner">Унікальний ідентифікатор власника</param>
        protected async ValueTask BaseDelete(Guid owner)
        {
            await Kernel.DataBase.DeleteRegisterAccumulationRecords(Table, owner, TransactionID);
            BaseClear();
        }

        /// <summary>
        /// Запис даних в регістр
        /// </summary>
        /// <param name="UID">Унікальний ідентифікатор</param>
        /// <param name="period">Період - дата запису або дата документу</param>
        /// <param name="income">Тип запису - прибуток чи зменшення</param>
        /// <param name="owner">Власник запису</param>
        /// <param name="fieldValue">Значення полів</param>
        protected async ValueTask<Guid> BaseSave(Guid UID, DateTime period, bool income, Guid owner, NameAndText ownertype, Dictionary<string, object> fieldValue)
        {
            Guid recordUnigueID = UID == Guid.Empty ? Guid.NewGuid() : UID;
            await Kernel.DataBase.InsertRegisterAccumulationRecords(recordUnigueID, Table, period, income, owner, ownertype, FieldArray, fieldValue, TransactionID);
            return recordUnigueID;
        }

        /// <summary>
        /// Запис даних в системну таблицю
        /// </summary>
        /// <param name="period">Період - дата запису або дата документу</param>
        /// <param name="owner">Власник запису</param>
        /// <param name="regAccumName">Назва регістру</param>
        protected async ValueTask BaseTrigerAdd(DateTime period, Guid owner)
        {
            await Kernel.DataBase.SpetialTableRegAccumTrigerAdd(period, owner, TypeRegAccum, "add", TransactionID);
        }

        /// <summary>
        /// Вибірка періоду (або періодів) для запису крім поточного якщо такий заданий.
        /// Використовується для вибірки паріоду і запису в системну таблицю тригерів для розрахунку.
        /// Спрацьовує коли проведений документ проводиться іншим числом і змінилась дата документу. Відповідно треба розрахувати регістри на дві дати - стару і нову.
        /// Також спрацьовує при очищенні записів.
        /// </summary>
        /// <param name="owner">Власник запису</param>
        /// <param name="periodCurrent">Поточний період запису</param>
        protected async ValueTask BaseSelectPeriodForOwner(Guid owner, DateTime? periodCurrent = null)
        {
            List<DateTime>? recordPeriod = await Kernel.DataBase.SelectRegisterAccumulationRecordPeriodForOwner(Table, owner, periodCurrent, TransactionID);

            if (recordPeriod != null)
                foreach (DateTime period in recordPeriod)
                    await Kernel.DataBase.SpetialTableRegAccumTrigerAdd(period, owner, TypeRegAccum, "clear", TransactionID);
        }
    }
}