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
	/// Довідник Таблична частина
	/// </summary>
	public abstract class DirectoryTablePart
	{
		public DirectoryTablePart(Kernel kernel, string table, string[] fieldsArray)
		{
			Kernel = kernel;
			Table = table;
			FieldArray = fieldsArray;

			FieldValueList = new List<Dictionary<string, object>>();
		}

		/// <summary>
		/// Ядро
		/// </summary>
		private Kernel Kernel { get; set; }

		/// <summary>
		/// Таблиця
		/// </summary>
		private string Table { get; set; }

		/// <summary>
		/// Масив назв полів
		/// </summary>
		private string[] FieldArray { get; set; }

		/// <summary>
		/// Список даних
		/// </summary>
		protected List<Dictionary<string, object>> FieldValueList { get; private set; }

		/// <summary>
		/// Очистити вн. списки
		/// </summary>
		protected void BaseClear()
		{
			FieldValueList.Clear();
		}

		/// <summary>
		/// Зчитати дані з бази даних
		/// </summary>
		/// <param name="ownerUnigueID"></param>
		protected void BaseRead(UnigueID ownerUnigueID)
		{
			BaseClear();
			Kernel.DataBase.SelectDirectoryTablePartRecords(ownerUnigueID, Table, FieldArray, FieldValueList);
		}

		protected void BaseBeginTransaction()
		{
			Kernel.DataBase.BeginTransaction();
		}

		protected void BaseCommitTransaction()
		{
			Kernel.DataBase.CommitTransaction();
		}

		protected void BaseRollbackTransaction()
		{
			Kernel.DataBase.RollbackTransaction();
		}

		/// <summary>
		/// Видалити всі записи з таб. частини.
		/// Функція очищає всю таб. частину
		/// </summary>
		/// <param name="ownerUnigueID">Унікальний ідентифікатор власника таб. частини</param>
		protected void BaseDelete(UnigueID ownerUnigueID)
		{
			Kernel.DataBase.DeleteDirectoryTablePartRecords(ownerUnigueID, Table);
		}

		/// <summary>
		/// Зберегти дані таб.частини. Добавляється один запис в таблицю.
		/// </summary>
		/// <param name="UID">Унікальний ідентифікатор запису</param>
		/// <param name="ownerUnigueID">Унікальний ідентифікатор власника таб. частини</param>
		/// <param name="fieldValue">Значення полів запису</param>
		protected void BaseSave(Guid UID, UnigueID ownerUnigueID, Dictionary<string, object> fieldValue)
		{
			Guid recordUnigueID = (UID == Guid.Empty ? Guid.NewGuid() : UID);
			Kernel.DataBase.InsertDirectoryTablePartRecords(recordUnigueID, ownerUnigueID, Table, FieldArray, fieldValue);
		}
	}
}

