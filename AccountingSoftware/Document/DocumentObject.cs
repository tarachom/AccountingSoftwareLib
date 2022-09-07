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
	/// Документ Об'єкт
	/// </summary>
	public abstract class DocumentObject
	{
		public DocumentObject(Kernel kernel, string table, string typeDocument, string[] fieldsArray)
		{
			Kernel = kernel;
			Table = table;
			TypeDocument = typeDocument;
			FieldArray = fieldsArray;

			FieldValue = new Dictionary<string, object>();

			foreach (string field in FieldArray)
				FieldValue.Add(field, null);
		}

		/// <summary>
		/// Ядро
		/// </summary>
		private Kernel Kernel { get; set; }

		/// <summary>
		/// Таблиця
		/// </summary>
		public string Table { get; private set; }

		/// <summary>
		/// Назва як задано в конфігураторі
		/// </summary>
		public string TypeDocument { get; private set; }

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
		/// Документ проведений
		/// </summary>
		public bool Spend { get; private set; }

		/// <summary>
		/// Дата проведення документу
		/// </summary>
		public DateTime SpendDate { get; private set; }

		/// <summary>
		/// Чи це новий?
		/// </summary>
		public bool IsNew { get; private set; }

		/// <summary>
		/// Новий обєкт
		/// </summary>
		public void New()
		{
			UnigueID = new UnigueID(Guid.NewGuid());
			IsNew = true;
			IsSave = false;
		}

		/// <summary>
		/// Чи вже записаний документ
		/// </summary>
		public bool IsSave { get; private set; }

		/// <summary>
		/// Очистка вн. списку
		/// </summary>
		protected void BaseClear()
		{
			foreach (string field in FieldArray)
				FieldValue[field] = null;
		}

		/// <summary>
		/// Зчитати дані
		/// </summary>
		/// <param name="uid">Унікальний ідентифікатор </param>
		/// <returns></returns>
		protected bool BaseRead(UnigueID uid)
		{
			if (uid == null || uid.UGuid == Guid.Empty)
				return false;

			BaseClear();

			bool spend = false;
			DateTime spend_date = DateTime.MinValue;

			if (Kernel.DataBase.SelectDocumentObject(uid, ref spend, ref spend_date, Table, FieldArray, FieldValue))
			{
				UnigueID = uid;
				Spend = spend;
				SpendDate = spend_date;

				IsSave = true;
				return true;
			}
			else
				return false;
		}

		/// <summary>
		/// Зберегти дані
		/// </summary>
		protected void BaseSave()
		{
			if (IsNew)
			{
				Kernel.DataBase.InsertDocumentObject(UnigueID, Spend, SpendDate, Table, FieldArray, FieldValue);
				IsNew = false;
			}
			else
			{
				if (UnigueID != null)
					Kernel.DataBase.UpdateDocumentObject(UnigueID, Spend, SpendDate, Table, FieldArray, FieldValue);
				else
					throw new Exception("Спроба записати неіснуючий документ. Потрібно спочатку створити новий - функція New()");
			}

			IsSave = true;

			BaseClear();
		}

		protected void BaseSpend(bool spend, DateTime spend_date)
        {
			Spend = spend;
			SpendDate = spend_date;

			if (IsSave)
				//Обновлення поля spend документу, решта полів не зачіпаються
				Kernel.DataBase.UpdateDocumentObject(UnigueID, Spend, SpendDate, Table, new string[] { }, new Dictionary<string, object>());
			else
				throw new Exception("Документ спочатку треба записати, а потім вже провести");
		}

		/// <summary>
		/// Видалити запис
		/// </summary>
		/// <param name="tablePartsTables">Список таблиць табличних частин</param>
		protected void BaseDelete(string[] tablePartsTables)
		{
			Kernel.DataBase.BeginTransaction();

			//Видалити сам документ
			Kernel.DataBase.DeleteDocumentObject(UnigueID, Table);

			//Видалення даних з табличних частин
			foreach (string tablePartsTable in tablePartsTables)
				Kernel.DataBase.DeleteDocumentTablePartRecords(UnigueID, tablePartsTable);

			Kernel.DataBase.CommitTransaction();

			BaseClear();
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
	}
}
