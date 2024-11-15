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
    /// Довідник Об'єкт
    /// </summary>
    public abstract class DirectoryObject
    {
        public DirectoryObject(Kernel kernel, string table, string typeDirectory, string[] fieldsArray)
        {
            Kernel = kernel;
            Table = table;
            TypeDirectory = typeDirectory;
            FieldArray = fieldsArray;

            foreach (string field in FieldArray)
                FieldValue.Add(field, new object());
        }

        /// <summary>
        /// Ядро
        /// </summary>
        private Kernel Kernel { get; set; }

        /// <summary>
        /// Назва таблиці
        /// </summary>
        private string Table { get; set; }

        /// <summary>
        /// Назва як задано в конфігураторі
        /// </summary>
        public string TypeDirectory { get; private set; }

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
        /// Мітка видалення
        /// </summary>
        public bool DeletionLabel { get; private set; }

        /// <summary>
        /// Чи це новий запис?
        /// </summary>
        public bool IsNew { get; private set; }

        /// <summary>
        /// Новий елемент
        /// </summary>
        protected void BaseNew()
        {
            UnigueID = UnigueID.NewUnigueID();
            IsNew = true;
            IsSave = false;
        }

        /// <summary>
        /// Чи вже записаний
        /// </summary>
        public bool IsSave { get; private set; }

        /// <summary>
        /// Очистка вн. масивів
        /// </summary>
        protected void BaseClear()
        {
            foreach (string field in FieldArray)
                FieldValue[field] = new object();
        }

        /// <summary>
        /// Зчитування полів обєкту з бази даних
        /// </summary>
        /// <param name="uid">Унікальний ідентифікатор обєкту</param>
        protected async ValueTask<bool> BaseRead(UnigueID uid)
        {
            if (uid == null || uid.IsEmpty())
                return false;

            BaseClear();

            var record = await Kernel.DataBase.SelectDirectoryObject(uid, Table, FieldArray, FieldValue);

            if (record.Result)
            {
                UnigueID = uid;
                DeletionLabel = record.DeletionLabel;
                IsSave = true;

                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Збереження даних в базу даних
        /// </summary>
        protected async ValueTask<bool> BaseSave()
        {
            bool result;

            if (IsNew)
            {
                result = await Kernel.DataBase.InsertDirectoryObject(UnigueID, Table, FieldArray, FieldValue);
                if (result) IsNew = false;
            }
            else
            {
                if (!UnigueID.IsEmpty() && await Kernel.DataBase.IsExistUniqueID(UnigueID, Table))
                    result = await Kernel.DataBase.UpdateDirectoryObject(UnigueID, DeletionLabel, Table, FieldArray, FieldValue);
                else
                    throw new Exception("Спроба оновити неіснуючий елемент довідника");
            }

            IsSave = result;

            //Тригер оновлення обєкту
            if (result)
                await Kernel.DataBase.SpetialTableObjectUpdateTrigerAdd(GetBasis());

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
                await Kernel.DataBase.UpdateDirectoryObject(UnigueID, DeletionLabel, Table, null, null);

                //Тригер оновлення обєкту
                await Kernel.DataBase.SpetialTableObjectUpdateTrigerAdd(GetBasis());

                //Видалення з повнотекстового пошуку
                /* if (DeletionLabel)
                    await Kernel.DataBase.SpetialTableFullTextSearchDelete(UnigueID, 0); */
            }
            else
                throw new Exception("Елемент спочатку треба записати, а потім вже встановлювати мітку видалення");
        }

        /// <summary>
        /// Видалення з бази даних
        /// </summary>
        protected async ValueTask BaseDelete(string[] tablePartsTables)
        {
            byte TransactionID = await Kernel.DataBase.BeginTransaction();

            //Видалити сам елемент
            await Kernel.DataBase.DeleteDirectoryObject(UnigueID, Table, TransactionID);

            //Видалення даних з табличних частин
            foreach (string tablePartsTable in tablePartsTables)
                await Kernel.DataBase.DeleteDirectoryTablePartRecords(UnigueID, tablePartsTable, TransactionID);

            //Видалення з повнотекстового пошуку
            await Kernel.DataBase.SpetialTableFullTextSearchDelete(UnigueID, TransactionID);

            await Kernel.DataBase.CommitTransaction(TransactionID);

            //Тригер оновлення обєкту
            await Kernel.DataBase.SpetialTableObjectUpdateTrigerAdd(GetBasis());

            BaseClear();
        }

        /// <summary>
        /// Отримати представлення вказівника
        /// </summary>
        /// <param name="fieldPresentation">Список полів які представляють вказівник (Назва, опис і т.д)</param>
        protected async ValueTask<string> BasePresentation(string[] fieldPresentation)
        {
            if (!UnigueID.IsEmpty() && IsSave && fieldPresentation.Length != 0)
            {
                Query query = new Query(Table);
                query.Field.AddRange(fieldPresentation);

                //Відбір по uid
                query.Where.Add(new Where("uid", Comparison.EQ, UnigueID.UGuid));

                return await Kernel.DataBase.GetDirectoryPresentation(query, fieldPresentation);
            }
            else return "";
        }

        /// <summary>
        /// Для композитного типу даних
        /// </summary>
        public virtual UuidAndText GetBasis()
        {
            return new UuidAndText(UnigueID, $"Довідники.{TypeDirectory}");
        }

        #region LockedObject

        /// <summary>
        /// Ключ блокування
        /// </summary>
        UnigueID LockKey { get; set; } = new UnigueID();

        /// <summary>
        /// Заблокувати
        /// </summary>
        /// <returns>true якщо вдалось заблокувати</returns>
        public async ValueTask<bool> Lock()
        {
            UnigueID unigueID = await Kernel.DataBase.SpetialTableLockedObjectAdd(Kernel.User, Kernel.Session, GetBasis());
            if (!unigueID.IsEmpty())
            {
                LockKey = unigueID;
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Чи заблокований?
        /// </summary>
        /// <returns>true якщо заблокований</returns>
        public async ValueTask<bool> IsLock()
        {
            return await Kernel.DataBase.SpetialTableLockedObjectIsLock(GetBasis());
        }

        /// <summary>
        /// Розширена версія Чи заблокований?
        /// </summary>
        /// <returns>Набір даних</returns>
        public async ValueTask<LockedObject_Record> IsLockInfo()
        {
            if (!LockKey.IsEmpty())
                return await Kernel.DataBase.SpetialTableLockedObjectIsLockInfo(GetBasis());
            else
                return new LockedObject_Record();
        }

        /// <summary>
        /// Розблокувати
        /// </summary>
        public async ValueTask UnLock()
        {
            await Kernel.DataBase.SpetialTableLockedObjectClear(LockKey);
            LockKey.Clear();
        }

        #endregion
    }
}