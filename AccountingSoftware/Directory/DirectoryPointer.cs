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
    /// Довідник Вказівник
    /// </summary>
    public abstract class DirectoryPointer(Kernel kernel, string table, string typeDirectory) : Pointer(kernel, table)
    {
        /// <summary>
        /// Назва типу як задано в конфігураторі
        /// </summary>
        public string TypeDirectory { get; private set; } = typeDirectory;

        /// <summary>
        /// Отримати представлення вказівника
        /// </summary>
        /// <param name="fieldPresentation">Список полів які представляють вказівник (Назва, опис і т.д)</param>
        protected async ValueTask<string> BasePresentation(string[] fieldPresentation)
        {
            if (!IsEmpty() && fieldPresentation.Length != 0)
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
        /// Повертає мітку на видалення або null якщо вказівник пустий або не вдалось прочитати
        /// </summary>
        /// <returns></returns>
        protected async ValueTask<bool?> BaseGetDeletionLabel()
        {
            if (!IsEmpty())
            {
                var record = await Kernel.DataBase.SelectDirectoryObject(this.UnigueID, Table, []);
                return record.Result ? record.DeletionLabel : null;
            }
            else
                return null;
        }

        /// <summary>
        /// Встановлення мітки на видалення
        /// </summary>
        /// <param name="label">Мітка</param>
        protected async ValueTask BaseDeletionLabel(bool label)
        {
            if (!IsEmpty())
            {
                //Обновлення поля deletion_label елементу, решта полів не зачіпаються
                await Kernel.DataBase.UpdateDirectoryObject(this.UnigueID, label, Table, null, null);

                //Тригер оновлення обєкту
                await Kernel.DataBase.SpetialTableObjectUpdateTrigerAdd(GetBasis(), 'U');
            }
        }

        /// <summary>
        /// Для композитного типу даних
        /// </summary>
        public virtual UuidAndText GetBasis()
        {
            return new UuidAndText(UnigueID, $"Довідники.{TypeDirectory}");
        }
    }
}