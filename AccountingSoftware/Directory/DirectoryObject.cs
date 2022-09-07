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
				FieldValue.Add(field, null);

			UnigueID = new UnigueID(Guid.Empty);
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
		/// <param name="use_uid">01.09.2021 Добавив можливість вказувати uid для нових елементів довідника. Використовується для обміну</param>
		public void New(UnigueID use_uid = null)
		{
			if (use_uid != null)
				UnigueID = use_uid;
			else
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

			if (Kernel.DataBase.SelectDirectoryObject(this, uid, Table, FieldArray, FieldValue))
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
				Kernel.DataBase.UpdateDirectoryObject(this, Table, FieldArray, FieldValue);
			}

			BaseClear();
		}

		/// <summary>
		/// Видалення з бази даних
		/// </summary>
		protected void BaseDelete(string[] tablePartsTables)
		{
			Kernel.DataBase.BeginTransaction();

			//Видалити сам елемент
			Kernel.DataBase.DeleteDirectoryObject(UnigueID, Table);

			//Видалення даних з табличних частин
			foreach (string tablePartsTable in tablePartsTables)
				Kernel.DataBase.DeleteDirectoryTablePartRecords(UnigueID, tablePartsTable);

			Kernel.DataBase.CommitTransaction();
						
			BaseClear();
		}
	}
}