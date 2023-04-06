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
    /// Довідник Об'єкт
    /// </summary>
    public abstract class DirectoryObject
    {
        public DirectoryObject(Kernel kernel, string table, string[] fieldsArray)
        {
            Kernel = kernel;
            Table = table;
            FieldArray = fieldsArray;

            FieldValue = new Dictionary<string, object>();

            foreach (string field in FieldArray)
                FieldValue.Add(field, new object());

            UnigueID = new UnigueID();
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
        /// Масив назв полів
        /// </summary>
        private string[] FieldArray { get; set; }

        /// <summary>
        /// Значення полів
        /// </summary>
        protected Dictionary<string, object> FieldValue { get; set; }

        /// <summary>
        /// Унікальний ідентифікатор запису
        /// </summary>
        public UnigueID UnigueID { get; private set; }

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
        /// Чи вже записаний документ
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
        /// <returns></returns>
        protected bool BaseRead(UnigueID uid)
        {
            if (uid == null || uid.IsEmpty())
                return false;

            BaseClear();

            bool deletion_label = false;

            if (Kernel.DataBase.SelectDirectoryObject(uid, ref deletion_label, Table, FieldArray, FieldValue))
            {
                UnigueID = uid;
                DeletionLabel = deletion_label;

                IsSave = true;
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Збереження даних в базу даних
        /// </summary>
        protected void BaseSave()
        {
            if (IsNew)
            {
                Kernel.DataBase.InsertDirectoryObject(this, Table, FieldArray, FieldValue);
                IsNew = false;
            }
            else
            {
                if (!UnigueID.IsEmpty())
                    Kernel.DataBase.UpdateDirectoryObject(this, Table, FieldArray, FieldValue);
                else
                    throw new Exception("Спроба записати неіснуючий елемент довідника. Потрібно спочатку створити новий - функція New()");
            }

            IsSave = true;

            BaseClear();
        }

        /// <summary>
        /// Запис даних полів в таблицю для повнотекстового пошуку
        /// </summary>
        /// <param name="obj">Обєкт</param>
        /// <param name="values">Масив значень полів</param>
        protected void BaseWriteFullTextSearch(UuidAndText obj, string[] values)
        {
            if (values.Length != 0)
                Kernel.DataBase.SpetialTableFullTextSearchAddValue(obj, string.Join(" ", values));
        }

        /// <summary>
        /// Встановлення мітки на видалення
        /// </summary>
        /// <param name="label">Мітка</param>
        /// <exception cref="Exception">Не записаний</exception>
        protected void BaseDeletionLabel(bool label)
        {
            DeletionLabel = label;

            if (IsSave)
                //Обновлення поля deletion_label елементу, решта полів не зачіпаються
                Kernel.DataBase.UpdateDirectoryObject(this, Table, new string[] { }, new Dictionary<string, object>());
            else
                throw new Exception("Елемент спочатку треба записати, а потім вже встановлювати мітку видалення");
        }

        /// <summary>
        /// Видалення з бази даних
        /// </summary>
        protected void BaseDelete(string[] tablePartsTables)
        {
            byte TransactionID = Kernel.DataBase.BeginTransaction();

            //Видалити сам елемент
            Kernel.DataBase.DeleteDirectoryObject(UnigueID, Table, TransactionID);

            //Видалення даних з табличних частин
            foreach (string tablePartsTable in tablePartsTables)
                Kernel.DataBase.DeleteDirectoryTablePartRecords(UnigueID, tablePartsTable, TransactionID);

            //Видалення з повнотекстового пошуку
            Kernel.DataBase.SpetialTableFullTextSearchDelete(UnigueID, TransactionID);

            Kernel.DataBase.CommitTransaction(TransactionID);

            BaseClear();
        }
    }
}