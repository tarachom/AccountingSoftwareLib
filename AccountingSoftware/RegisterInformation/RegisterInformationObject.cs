/*
Copyright (C) 2019-2020 TARAKHOMYN YURIY IVANOVYCH
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

using System;
using System.Collections.Generic;

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

			FieldValue = new Dictionary<string, object>();

			foreach (string field in FieldArray)
				FieldValue.Add(field, null);

			UnigueID = new UnigueID(Guid.Empty);
			Period = DateTime.Now;
			Owner = Guid.Empty;
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
		/// Період
		/// </summary>
		public DateTime Period { get; set; }

		/// <summary>
		/// Власник
		/// </summary>
		public Guid Owner { get; set; }

		/// <summary>
		/// Чи це новий запис?
		/// </summary>
		public bool IsNew { get; private set; }

		/// <summary>
		/// Новий елемент
		/// </summary>
		public void New()
		{
			UnigueID = new UnigueID(Guid.NewGuid());
			IsNew = true;
		}

		/// <summary>
		/// Очистка вн. масивів
		/// </summary>
		protected void BaseClear()
		{
			foreach (string field in FieldArray)
				FieldValue[field] = null;
		}

		/// <summary>
		/// Зчитування полів обєкту з бази даних
		/// </summary>
		/// <param name="uid">Унікальний ідентифікатор обєкту</param>
		/// <returns></returns>
		protected bool BaseRead(UnigueID uid)
		{
			if (uid == null || uid.UGuid == Guid.Empty)
				return false;

			BaseClear();

			UnigueID = uid;

			if (Kernel.DataBase.SelectRegisterInformationObject(this, Table, FieldArray, FieldValue))
				return true;
			else
			{
				UnigueID = new UnigueID(Guid.Empty);
				return false;
			}
		}

		/// <summary>
		/// Збереження даних в базу даних
		/// </summary>
		protected void BaseSave()
		{
			if (IsNew)
			{
				Kernel.DataBase.InsertRegisterInformationObject(this, Table, FieldArray, FieldValue);
				IsNew = false;
			}
			else
			{
				Kernel.DataBase.UpdateRegisterInformationObject(this, Table, FieldArray, FieldValue);
			}

			BaseClear();
		}

		/// <summary>
		/// Видалення з бази даних
		/// </summary>
		protected void BaseDelete()
		{
			Kernel.DataBase.BeginTransaction();
			Kernel.DataBase.DeleteRegisterInformationObject(Table, UnigueID);
			Kernel.DataBase.CommitTransaction();
			
			BaseClear();
		}
	}
}