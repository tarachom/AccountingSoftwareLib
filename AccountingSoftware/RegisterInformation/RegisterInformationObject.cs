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
    /// Обєкт запис регістру інформації
    /// </summary>
    public abstract class RegisterInformationObject(Kernel kernel, string table, string[] fieldsArray) : Object(kernel, table, fieldsArray)
    {
        /// <summary>
        /// Період
        /// </summary>
        public DateTime Period { get; set; } = DateTime.Now;

        /// <summary>
        /// Власник
        /// </summary>
        public Guid Owner { get; set; } = Guid.Empty;

        /// <summary>
        /// Тип власника запису
        /// </summary>
        public NameAndText OwnerType { get; set; } = new NameAndText();

        /// <summary>
        /// Зчитування полів обєкту з бази даних
        /// </summary>
        /// <param name="uid">Унікальний ідентифікатор обєкту</param>
        /// <returns></returns>
        protected async ValueTask<bool> BaseRead(UnigueID uid)
        {
            BaseClear();

            if (uid.IsEmpty() || IsNew == true) return false;

            var record = await Kernel.DataBase.SelectRegisterInformationObject(uid, Table, FieldArray, FieldValue);
            if (record.Result)
            {
                UnigueID = uid;
                Period = record.Period;
                Owner = record.Owner;
                OwnerType = record.OwnerType;
                
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
                result = await Kernel.DataBase.InsertRegisterInformationObject(UnigueID, Period, Owner, OwnerType, Table, FieldArray, FieldValue);
                if (result) IsNew = false;
            }
            else
            {
                if (!UnigueID.IsEmpty() && await Kernel.DataBase.IsExistUniqueID(UnigueID, Table))
                    result = await Kernel.DataBase.UpdateRegisterInformationObject(UnigueID, Period, Owner, OwnerType, Table, FieldArray, FieldValue);
                else
                    throw new Exception("Спроба записати неіснуючий об'єкт");
            }

            IsSave = result;

            BaseClear();

            return result;
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