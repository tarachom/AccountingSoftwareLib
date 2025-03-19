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
    /// Об'єкт
    /// 
    /// Основа для класів:
    ///     DirectoryObject
    ///     DocumentObject
    ///     RegisterInformationObject
    /// </summary>
    public abstract class Object
    {
        public Object(Kernel kernel, string table, string[] fieldsArray)
        {
            Kernel = kernel;
            Table = table;
            FieldArray = fieldsArray;

            foreach (string field in FieldArray)
                FieldValue.Add(field, new object());
        }

        /// <summary>
        /// Ядро
        /// </summary>
        protected Kernel Kernel { get; set; }

        /// <summary>
        /// Таблиця
        /// </summary>
        public string Table { get; protected set; }

        /// <summary>
        /// Масив назв полів
        /// </summary>
        protected string[] FieldArray { get; private set; }

        /// <summary>
        /// Значення полів
        /// </summary>
        protected Dictionary<string, object> FieldValue { get; private set; } = [];

        /// <summary>
        /// Унікальний ідентифікатор запису
        /// </summary>
        public UnigueID UnigueID { get; protected set; } = new UnigueID();

        /// <summary>
        /// Чи вже записаний
        /// </summary>
        public bool IsSave { get; protected set; }

        /// <summary>
        /// Чи це новий запис?
        /// </summary>
        public bool IsNew { get; protected set; }

        /// <summary>
        /// Очистка вн. масивів
        /// </summary>
        protected void BaseClear()
        {
            foreach (string field in FieldArray)
                FieldValue[field] = new object();
        }

        /// <summary>
        /// Новий елемент
        /// </summary>
        protected void BaseNew()
        {
            UnigueID = UnigueID.NewUnigueID();
            IsNew = true;
            IsSave = false;
        }

        #region Virtual Function

        public virtual UuidAndText GetBasis() { return new UuidAndText(); }

        #endregion

        #region LockedObject

        /// <summary>
        /// Ключ блокування
        /// </summary>
        private UnigueID LockKey { get; set; } = new UnigueID();

        /// <summary>
        /// Заблокувати
        /// </summary>
        public async ValueTask<bool> Lock()
        {
            LockKey = await Kernel.DataBase.SpetialTableLockedObjectAdd(Kernel.User, Kernel.Session, GetBasis());
            return !LockKey.IsEmpty();
        }

        /// <summary>
        /// Розблокувати
        /// </summary>
        public async ValueTask UnLock()
        {
            await Kernel.DataBase.SpetialTableLockedObjectClear(LockKey);
            LockKey.Clear();
        }

        /// <summary>
        /// Розширена версія Чи заблокований?
        /// </summary>
        /// <returns>Набір даних</returns>
        public async ValueTask<LockedObject_Record> LockInfo()
        {
            return await Kernel.DataBase.SpetialTableLockedObjectIsLockInfo(GetBasis());
        }

        #endregion
    }
}