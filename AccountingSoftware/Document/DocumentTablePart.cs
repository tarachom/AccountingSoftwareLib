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
	/// Документ Таблична частина
	/// </summary>
	public abstract class DocumentTablePart
	{
		public DocumentTablePart(Kernel kernel, string table, string[] fieldsArray)
		{
			Kernel = kernel;
			Table = table;
			FieldArray = fieldsArray;

			QuerySelect = new Query(Table);
			QuerySelect.Field.AddRange(fieldsArray);

			FieldValueList = new List<Dictionary<string, object>>();
			JoinValue = new Dictionary<string, Dictionary<string, string>>();
		}

		/// <summary>
		/// Запит SELECT
		/// </summary>
		public Query QuerySelect { get; set; }

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
		/// Значення полів
		/// </summary>
		protected List<Dictionary<string, object>> FieldValueList { get; private set; }

		/// <summary>
		/// Значення додаткових полів
		/// </summary>
		public Dictionary<string, Dictionary<string,string>> JoinValue { get; private set; }

		/// <summary>
		/// Очистка вн. списків
		/// </summary>
		protected void BaseClear()
		{
			FieldValueList.Clear();
		}

		//Зчитування даних
		protected void BaseRead(UnigueID ownerUnigueID)
		{
			BaseClear();

			JoinValue.Clear();

			QuerySelect.Where.Clear();
			QuerySelect.Where.Add(new Where("owner", Comparison.EQ, ownerUnigueID.UGuid));

			Kernel.DataBase.SelectDocumentTablePartRecords(QuerySelect, FieldValueList);

			//Якщо задані додаткові поля з псевдонімами, їх потрібно зчитати в список JoinValue
			if (QuerySelect.FieldAndAlias.Count > 0)
            {
				foreach (Dictionary<string, object> fieldValue in FieldValueList)
                {
					Dictionary<string, string> joinFieldValue = new Dictionary<string, string>();
					JoinValue.Add(fieldValue["uid"].ToString(), joinFieldValue);

					foreach (NameValue<string> fieldAndAlias in QuerySelect.FieldAndAlias)
						joinFieldValue.Add(fieldAndAlias.Value, fieldValue[fieldAndAlias.Value].ToString());
				}
			}
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
		/// Видалити всі дані з таб. частини
		/// </summary>
		/// <param name="ownerUnigueID">Унікальний ідентифікатор власника таб. частини</param>
		protected void BaseDelete(UnigueID ownerUnigueID)
		{
			Kernel.DataBase.DeleteDocumentTablePartRecords(ownerUnigueID, Table);
		}

		/// <summary>
		/// Зберегти один запис таб частини
		/// </summary>
		/// <param name="UID">Унікальний ідентифікатор запису</param>
		/// <param name="ownerUnigueID">Унікальний ідентифікатор власника таб. частини</param>
		/// <param name="fieldValue">Список значень полів</param>
		protected void BaseSave(Guid UID, UnigueID ownerUnigueID, Dictionary<string, object> fieldValue)
		{
			Guid recordUnigueID = (UID == Guid.Empty ? Guid.NewGuid() : UID);
			Kernel.DataBase.InsertDocumentTablePartRecords(recordUnigueID, ownerUnigueID, Table, FieldArray, fieldValue);
		}
	}
}
