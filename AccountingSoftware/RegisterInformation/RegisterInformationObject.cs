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
    /// Обєкт запис регістру інформації
    /// </summary>
    public abstract class RegisterInformationObject
    {
        public RegisterInformationObject(Kernel kernel, string table, string[] fieldsArray)
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
        protected Dictionary<string, object> FieldValue { get; set; } = [];

        /// <summary>
        /// Унікальний ідентифікатор запису
        /// </summary>
        public UnigueID UnigueID { get; private set; } = new UnigueID();

        /// <summary>
        /// Період
        /// </summary>
        public DateTime Period { get; set; } = DateTime.Now;

        /// <summary>
        /// Власник
        /// </summary>
        public Guid Owner { get; set; } = Guid.Empty;

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
        protected async ValueTask<bool> BaseRead(UnigueID uid)
        {
            if (uid == null || uid.UGuid == Guid.Empty)
                return false;

            BaseClear();

            UnigueID = uid;

            if (await Kernel.DataBase.SelectRegisterInformationObject(this, Table, FieldArray, FieldValue))
                return true;
            else
            {
                UnigueID = new UnigueID();
                return false;
            }
        }

        /// <summary>
        /// Збереження даних в базу даних
        /// </summary>
        protected async ValueTask BaseSave()
        {
            if (IsNew)
            {
                await Kernel.DataBase.InsertRegisterInformationObject(this, Table, FieldArray, FieldValue);
                IsNew = false;
            }
            else
            {
                await Kernel.DataBase.UpdateRegisterInformationObject(this, Table, FieldArray, FieldValue);
            }

            BaseClear();
        }

        /// <summary>
        /// Видалення з бази даних
        /// </summary>
        protected async ValueTask BaseDelete()
        {
            await Kernel.DataBase.DeleteRegisterInformationObject(Table, UnigueID);
            BaseClear();
        }
    }
}