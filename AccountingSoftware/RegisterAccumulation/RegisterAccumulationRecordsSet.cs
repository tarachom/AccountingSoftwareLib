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
	/// Регістр накопичення
	/// </summary>
	public abstract class RegisterAccumulationRecordsSet
	{
		public RegisterAccumulationRecordsSet(Kernel kernel, string table, string[] fieldsArray)
		{
			Kernel = kernel;
			Table = table;
			FieldArray = fieldsArray;

			QuerySelect = new Query(Table);
			QuerySelect.Field.AddRange(new string[] { "period", "income", "owner" });
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
		/// Список полів
		/// </summary>
		private string[] FieldArray { get; set; }

		/// <summary>
		/// Значення полів
		/// </summary>
		protected List<Dictionary<string, object>> FieldValueList { get; private set; }

		/// <summary>
		/// Додаткові поля
		/// </summary>
		public Dictionary<string, Dictionary<string, string>> JoinValue { get; private set; }

		/// <summary>
		/// Очитска вн. списків
		/// </summary>
		protected void BaseClear()
		{
			FieldValueList.Clear();
		}

		/// <summary>
		/// Зчитування даних
		/// </summary>
		protected void BaseRead()
		{
			BaseClear();

			JoinValue.Clear();

			Kernel.DataBase.SelectRegisterAccumulationRecords(QuerySelect, FieldValueList);

			//Зчитування додаткових полів
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
		/// Видалення записів для власника
		/// </summary>
		/// <param name="owner">Унікальний ідентифікатор власника</param>
		protected void BaseDelete(Guid owner)
		{
			Kernel.DataBase.DeleteRegisterAccumulationRecords(Table, owner);
		}

		/// <summary>
		/// Запис даних в регістр
		/// </summary>
		/// <param name="UID">Унікальний ідентифікатор</param>
		/// <param name="period">Період - дата запису або дата документу</param>
		/// <param name="income">Тип запису - прибуток чи зменшення</param>
		/// <param name="owner">Власник запису</param>
		/// <param name="fieldValue">Значення полів</param>
		protected void BaseSave(Guid UID, DateTime period, bool income, Guid owner, Dictionary<string, object> fieldValue)
		{
			Guid recordUnigueID = (UID == Guid.Empty ? Guid.NewGuid() : UID);
			Kernel.DataBase.InsertRegisterAccumulationRecords(recordUnigueID, Table, period, income, owner, FieldArray, fieldValue);
		}
	}
}
