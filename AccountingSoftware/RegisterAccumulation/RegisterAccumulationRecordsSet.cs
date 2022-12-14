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
            QuerySelect.Field.AddRange(new string[] { "period", "income", "owner" });
            QuerySelect.Field.AddRange(fieldsArray);

            FieldValueList = new List<Dictionary<string, object>>();
            JoinValue = new Dictionary<string, Dictionary<string, string>>();
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
        protected List<Dictionary<string, object>> FieldValueList { get; private set; }

        /// <summary>
        /// Додаткові поля
        /// </summary>
        public Dictionary<string, Dictionary<string, string>> JoinValue { get; private set; }

        /// <summary>
        /// Очитска вн. списків
        /// </summary>
        protected void BaseClear()
        {
            FieldValueList.Clear();
        }

        /// <summary>
        /// Зчитування даних
        /// </summary>
        protected void BaseRead()
        {
            BaseClear();

            JoinValue.Clear();

            Kernel.DataBase.SelectRegisterAccumulationRecords(QuerySelect, FieldValueList);

            //Зчитування додаткових полів
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

        protected void BaseBeginTransaction()
        {
            TransactionID = Kernel.DataBase.BeginTransaction();
        }

        protected void BaseCommitTransaction()
        {
            Kernel.DataBase.CommitTransaction(TransactionID);
            TransactionID = 0;
        }

        protected void BaseRollbackTransaction()
        {
            Kernel.DataBase.RollbackTransaction(TransactionID);
            TransactionID = 0;
        }

        /// <summary>
        /// Видалення записів для власника
        /// </summary>
        /// <param name="owner">Унікальний ідентифікатор власника</param>
        protected void BaseDelete(Guid owner)
        {
            Kernel.DataBase.DeleteRegisterAccumulationRecords(Table, owner, TransactionID);
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
        protected void BaseSave(Guid UID, DateTime period, bool income, Guid owner, Dictionary<string, object> fieldValue)
        {
            Guid recordUnigueID = (UID == Guid.Empty ? Guid.NewGuid() : UID);
            Kernel.DataBase.InsertRegisterAccumulationRecords(recordUnigueID, Table, period, income, owner, FieldArray, fieldValue, TransactionID);
        }

        /// <summary>
        /// Запис даних в системну таблицю
        /// </summary>
        /// <param name="period">Період - дата запису або дата документу</param>
        /// <param name="owner">Власник запису</param>
        /// <param name="regAccumName">Назва регістру</param>
        protected void BaseTrigerAdd(DateTime period, Guid owner)
        {
            Kernel.DataBase.SpetialTableRegAccumTrigerAdd(period, owner, TypeRegAccum, "add", TransactionID);
        }

        /// <summary>
        /// Вибірка періоду (або періодів) для запису крім поточного якщо такий заданий.
        /// Використовується для вибірки паріоду і запису в системну таблицю тригерів для розрахунку.
        /// Спрацьовує коли проведений документ проводиться іншим числом і змінилась дата документу. Відповідно треба розрахувати регістри на дві дати - стару і нову.
        /// Також спрацьовує при очищенні записів.
        /// </summary>
        /// <param name="owner">Власник запису</param>
        /// <param name="periodCurrent">Поточний період запису</param>
        protected void BaseSelectPeriodForOwner(Guid owner, DateTime? periodCurrent = null)
        {
            List<DateTime>? recordPeriod = Kernel.DataBase.SelectRegisterAccumulationRecordPeriodForOwner(Table, owner, periodCurrent, TransactionID);
            
            if (recordPeriod != null)
            {
                foreach (DateTime period in recordPeriod)
                    Kernel.DataBase.SpetialTableRegAccumTrigerAdd(period, owner, TypeRegAccum, "clear", TransactionID);
            }
        }
    }
}