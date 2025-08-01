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
    /// Документ Об'єкт
    /// </summary>
    public abstract class DocumentObject(Kernel kernel, string table, string typeDocument, string[] fieldsArray, bool versionsHistory = false) : Object(kernel, table, fieldsArray)
    {
        /// <summary>
        /// Назва типу як задано в конфігураторі
        /// </summary>
        public string TypeDocument { get; private set; } = typeDocument;

        /// <summary>
        /// Мітка видалення
        /// </summary>
        public bool DeletionLabel { get; private set; }

        /// <summary>
        /// Документ проведений
        /// </summary>
        public bool Spend { get; private set; }

        /// <summary>
        /// Дата проведення документу
        /// </summary>
        public DateTime SpendDate { get; private set; } = DateTime.MinValue;

        /// <summary>
        /// Унікальний ідентифікатор для збереження версій
        /// </summary>
        public Guid VersionID { get; private set; } = Guid.NewGuid();

        /// <summary>
        /// Вести історію версій значень полів
        /// </summary>
        private bool VersionsHistory { get; set; } = versionsHistory;

        /// <summary>
        /// Зчитати дані
        /// </summary>
        /// <param name="uid">Унікальний ідентифікатор </param>
        protected async ValueTask<bool> BaseRead(UnigueID uid)
        {
            BaseClear();

            if (uid.IsEmpty() || IsNew) return false;

            var record = await Kernel.DataBase.SelectDocumentObject(uid, Table, FieldArray, FieldValue);
            if (record.Result)
            {
                UnigueID = uid;
                DeletionLabel = record.DeletionLabel;
                Spend = record.Spend;
                SpendDate = record.SpendDate;
                IsSave = true;

                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Зберегти дані
        /// </summary>
        protected async ValueTask<bool> BaseSave()
        {
            bool result;

            char? operation = null;

            if (IsNew)
            {
                result = await Kernel.DataBase.InsertDocumentObject(UnigueID, Spend, SpendDate, Table, FieldArray, FieldValue);
                if (result)
                {
                    IsNew = false;
                    operation = 'A';
                }
            }
            else
            {
                if (!UnigueID.IsEmpty() && await Kernel.DataBase.IsExistUniqueID(UnigueID, Table))
                {
                    result = await Kernel.DataBase.UpdateDocumentObject(UnigueID, DeletionLabel, Spend, SpendDate, Table, FieldArray, FieldValue);
                    if (result)
                        operation = 'U';
                }
                else
                    throw new Exception("Спроба записати неіснуючий документ");
            }

            IsSave = result;

            if (result)
            {
                //Тригер оновлення обєкту
                await Kernel.DataBase.SpetialTableObjectUpdateTrigerAdd(GetBasis(), operation ?? 'U');

                //Записати в історію поточну версію значень полів
                if (VersionsHistory)
                    await Kernel.DataBase.SpetialTableObjectVersionsHistoryAdd(VersionID, Kernel.User, GetBasis(), FieldValue, operation ?? 'U');
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

        protected async ValueTask BaseAddIgnoreDocumentList()
        {
            await Kernel.DataBase.SpetialTableRegAccumTrigerDocIgnoreAdd(Kernel.User, Kernel.Session, UnigueID.UGuid, "");
        }

        protected async ValueTask BaseRemoveIgnoreDocumentList()
        {
            await Kernel.DataBase.SpetialTableRegAccumTrigerDocIgnoreClear(Kernel.User, Kernel.Session, UnigueID.UGuid);
        }

        protected async ValueTask BaseSpend(bool spend, DateTime spend_date)
        {
            Spend = spend;
            SpendDate = spend_date;

            if (IsSave)
            {
                await Kernel.DataBase.UpdateDocumentObject(UnigueID, Spend ? false : DeletionLabel, Spend, SpendDate, Table, null, null);

                //Тригер оновлення обєкту
                await Kernel.DataBase.SpetialTableObjectUpdateTrigerAdd(GetBasis(), 'U');
            }
            else
                throw new Exception("Документ спочатку треба записати, а потім вже проводити!");
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
                await Kernel.DataBase.UpdateDocumentObject(UnigueID, DeletionLabel, null, null, Table, null, null);

                //Тригер оновлення обєкту
                await Kernel.DataBase.SpetialTableObjectUpdateTrigerAdd(GetBasis(), 'U');
            }
            else
                throw new Exception("Документ спочатку треба записати, а потім вже встановлювати мітку видалення");
        }

        /// <summary>
        /// Видалити запис
        /// </summary>
        /// <param name="tablePartsTables">Список таблиць табличних частин</param>
        protected async ValueTask BaseDelete(string[] tablePartsTables)
        {
            byte TransactionID = await Kernel.DataBase.BeginTransaction();

            //Видалити сам документ
            await Kernel.DataBase.DeleteDocumentObject(UnigueID, Table, TransactionID);

            //Видалення даних з табличних частин
            foreach (string tablePartsTable in tablePartsTables)
                await Kernel.DataBase.DeleteDocumentTablePartRecords(UnigueID, tablePartsTable, TransactionID);

            //Видалення з повнотекстового пошуку
            await Kernel.DataBase.SpetialTableFullTextSearchDelete(UnigueID, TransactionID);

            //Видалення з історії зміни даних
            await Kernel.DataBase.SpetialTableObjectVersionsHistoryRemoveAll(GetBasis(), TransactionID);

            await Kernel.DataBase.CommitTransaction(TransactionID);

            //Тригер оновлення обєкту
            await Kernel.DataBase.SpetialTableObjectUpdateTrigerAdd(GetBasis(), 'D');

            BaseClear();
        }

        /// <summary>
        /// Представлення обєкта
        /// </summary>
        /// <param name="fieldPresentation">Масив полів які представляють обєкт (Наприклад Назва, Дата, Номер і т.д)</param>
        /// <returns>Представлення обєкта</returns>
        protected async ValueTask<string> BasePresentation(string[] fieldPresentation)
        {
            if (!UnigueID.IsEmpty() && IsSave && fieldPresentation.Length != 0)
            {
                Query query = new(Table);
                query.Field.AddRange(fieldPresentation);
                query.Where.Add(new Where("uid", Comparison.EQ, UnigueID.UGuid)); //Відбір по uid

                return await Kernel.DataBase.GetDocumentPresentation(query, fieldPresentation);
            }
            else
                return "";
        }

        /// <summary>
        /// Для композитного типу даних
        /// </summary>
        public override UuidAndText GetBasis()
        {
            return new UuidAndText(UnigueID, $"Документи.{TypeDocument}");
        }

        /// <summary>
        /// При створенні нового об'єкту змінюється і VersionID версії
        /// </summary>
        protected override void BeforeBaseNew()
        {
            VersionID = Guid.NewGuid();
        }

    }
}