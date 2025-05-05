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
    /// Довідник Об'єкт
    /// </summary>
    public abstract class DirectoryObject(Kernel kernel, string table, string typeDirectory, string[] fieldsArray, bool versionsHistory = false) : Object(kernel, table, fieldsArray, versionsHistory)
    {
        /// <summary>
        /// Назва типу як задано в конфігураторі
        /// </summary>
        public string TypeDirectory { get; private set; } = typeDirectory;

        /// <summary>
        /// Мітка видалення
        /// </summary>
        public bool DeletionLabel { get; private set; }

        /// <summary>
        /// Зчитування полів обєкту з бази даних
        /// </summary>
        /// <param name="uid">Унікальний ідентифікатор обєкту</param>
        protected async ValueTask<bool> BaseRead(UnigueID uid)
        {
            BaseClear();

            if (uid.IsEmpty() || IsNew) return false;

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
                if (result)
                {
                    IsNew = false;

                    //Тригер оновлення обєкту
                    await Kernel.DataBase.SpetialTableObjectUpdateTrigerAdd(GetBasis(), 'A');
                }
            }
            else
            {
                if (!UnigueID.IsEmpty() && await Kernel.DataBase.IsExistUniqueID(UnigueID, Table))
                {
                    result = await Kernel.DataBase.UpdateDirectoryObject(UnigueID, DeletionLabel, Table, FieldArray, FieldValue);
                    if (result)
                        //Тригер оновлення обєкту
                        await Kernel.DataBase.SpetialTableObjectUpdateTrigerAdd(GetBasis(), 'U');
                }
                else
                    throw new Exception("Спроба оновити неіснуючий елемент довідника");
            }

            IsSave = result;

            if (result)
            {
                //Записати в історію поточну версію значень полів
                if (VersionsHistory)
                    await Kernel.DataBase.SpetialTableObjectVersionsHistoryAdd(VersionID, Kernel.User, GetBasis(), FieldValue);
            }

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
                await Kernel.DataBase.SpetialTableObjectUpdateTrigerAdd(GetBasis(), 'U');
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

            //Видалення з історії версій
            if (VersionsHistory)
                await Kernel.DataBase.SpetialTableObjectVersionsHistoryDelete(GetBasis(), TransactionID);

            await Kernel.DataBase.CommitTransaction(TransactionID);

            //Тригер оновлення обєкту
            await Kernel.DataBase.SpetialTableObjectUpdateTrigerAdd(GetBasis(), 'D');

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
                Query query = new(Table);
                query.Field.AddRange(fieldPresentation);
                query.Where.Add(new Where("uid", Comparison.EQ, UnigueID.UGuid)); //Відбір по uid

                return await Kernel.DataBase.GetDirectoryPresentation(query, fieldPresentation);
            }
            else
                return "";
        }

        /// <summary>
        /// Для композитного типу даних
        /// </summary>
        public override UuidAndText GetBasis()
        {
            return new UuidAndText(UnigueID, $"Довідники.{TypeDirectory}");
        }
    }
}