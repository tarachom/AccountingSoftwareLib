/*
Copyright (C) 2019-2022 TARAKHOMYN YURIY IVANOVYCH
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
        /// Чи це новий запис?
        /// </summary>
        public bool IsNew { get; private set; }

        /// <summary>
        /// Новий елемент
        /// </summary>
        public void New()
        {
            UnigueID = UnigueID.NewUnigueID();
            IsNew = true;
        }

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

            if (Kernel.DataBase.SelectDirectoryObject(uid, Table, FieldArray, FieldValue))
            {
                UnigueID = uid;
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

            BaseClear();
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

            Kernel.DataBase.CommitTransaction(TransactionID);

            BaseClear();
        }
    }
}