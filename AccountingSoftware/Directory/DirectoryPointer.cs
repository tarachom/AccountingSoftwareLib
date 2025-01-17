﻿/*
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
    /// Довідник Вказівник
    /// </summary>
    public abstract class DirectoryPointer
    {
        public DirectoryPointer(Kernel kernel, string table)
        {
            Table = table;
            Kernel = kernel;
        }

        /// <summary>
        /// Ініціалізація вказівника
        /// </summary>
        /// <param name="uid">Унікальний ідентифікатор</param>
        /// <param name="fields">Поля які потрібно додатково зчитати з бази даних</param>
        protected void Init(UnigueID uid, Dictionary<string, object>? fields = null)
        {
            UnigueID = uid;
            Fields = fields;
        }

        /// <summary>
        /// Ядро
        /// </summary>
        private Kernel Kernel { get; set; }

        /// <summary>
        /// Таблиця
        /// </summary>
        private string Table { get; set; } = "";

        /// <summary>
        /// Унікальний ідентифікатор
        /// </summary>
        public UnigueID UnigueID { get; private set; } = new UnigueID();

        /// <summary>
        /// Поля які потрібно додатково зчитати з бази даних 
        /// </summary>
        public Dictionary<string, object>? Fields { get; private set; }

        /// <summary>
        /// Чи це пустий ідентифікатор
        /// </summary>
        public bool IsEmpty()
        {
            return UnigueID.IsEmpty();
        }

        /// <summary>
        /// Отримати представлення вказівника
        /// </summary>
        /// <param name="fieldPresentation">Список полів які представляють вказівник (Назва, опис і т.д)</param>
        protected async ValueTask<string> BasePresentation(string[] fieldPresentation)
        {
            if (Kernel != null && !IsEmpty() && fieldPresentation.Length != 0)
            {
                Query query = new(Table);
                query.Field.AddRange(fieldPresentation);

                //Відбір по uid
                query.Where.Add(new Where("uid", Comparison.EQ, UnigueID.UGuid));

                return await Kernel.DataBase.GetDirectoryPresentation(query, fieldPresentation);
            }
            else return "";
        }

        /// <summary>
        /// Встановлення мітки на видалення
        /// </summary>
        /// <param name="label">Мітка</param>
        protected async ValueTask BaseDeletionLabel(bool label)
        {
            if (Kernel != null && !IsEmpty())
            {
                //Обновлення поля deletion_label елементу, решта полів не зачіпаються
                await Kernel.DataBase.UpdateDirectoryObject(this.UnigueID, label, Table, null, null);
            }
        }

        /// <summary>
        /// Отримати ункальний ідентифікатор у форматі Guid
        /// </summary>
        public Guid GetPointer()
        {
            return UnigueID.UGuid;
        }

        public override string ToString()
        {
            return UnigueID.UGuid.ToString();
        }
    }
}