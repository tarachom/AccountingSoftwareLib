/*
Copyright (C) 2019-2026 TARAKHOMYN YURIY IVANOVYCH
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
	/// Документ Вибірка вказівників
	/// </summary>
	public abstract class DocumentSelect : Select
	{
		public DocumentSelect(Kernel kernel, string table, string typeDocument, string[] fieldPresentation) : base(kernel, table, "", "", fieldPresentation)
		{
			TypeDocument = typeDocument;
			ConfFields = Kernel.Conf.Documents[TypeDocument].Fields.Values;
		}

		/// <summary>
		/// Назва типу як задано в конфігураторі
		/// </summary>
		public string TypeDocument { get; private set; }

		/// <summary>
		/// Зчитати
		/// </summary>
		protected async Task<bool> BaseSelect()
		{
			Position = 0;
			CurrentPointerPosition = null;
			CurrentPointerPresentation = null;
			BaseSelectList.Clear();

			ExistFields();

			await Kernel.DataBase.SelectDocumentPointer(QuerySelect, BaseSelectList);

			return Count() > 0;
		}

		/// <summary>
		/// Зчитати один вказівник
		/// </summary>
		protected async Task<bool> BaseSelectSingle()
		{
			long? oldLimit = QuerySelect.Limit;
			QuerySelect.Limit = 1;

			await BaseSelect();

			QuerySelect.Limit = oldLimit;

			return Count() > 0;
		}

		/// <summary>
		/// Пошук по значенню поля (наприклад пошук по назві)
		/// </summary>
		/// <param name="fieldName">Назва поля в базі даних</param>
		/// <param name="fieldValue">Значення поля</param>
		/// <param name="funcToField">Функція для поля</param>
		/// <param name="funcToField_Param1">Перший параметр для функції</param>
		/// <returns>Повертає true якщо є елемент у вибірці</returns>
		protected async Task<bool> BaseFindByField(string fieldName, object fieldValue, string funcToField = "", string funcToField_Param1 = "")
		{
			Where where = new(ExistField(fieldName), Comparison.EQ, fieldValue) { FuncToField = funcToField, FuncToField_Param1 = funcToField_Param1 };
			QuerySelect.Where.Add(where);

			bool result = await BaseSelectSingle();

			QuerySelect.Where.Remove(where);

			return result;
		}

		/// <summary>
		/// Пошук по значенню поля (наприклад пошук по назві)
		/// </summary>
		/// <param name="fieldName">Назва поля в базі даних<</param>
		/// <param name="fieldValue">Значення поля</param>
		/// <param name="limit">Кількість елементів які можна вибрати</param>
		/// <param name="offset">Зміщення від початку вибірки</param>
		/// <param name="funcToField">Функція для поля</param>
		/// <param name="funcToField_Param1">Перший параметр для функції</param>
		/// <returns>Повертає true якщо є елементи у вибірці</returns>
		protected async Task<bool> BaseFindListByField(string fieldName, object fieldValue, string funcToField = "", string funcToField_Param1 = "")
		{
			long? oldLimit = QuerySelect.Limit;
			long? oldOffset = QuerySelect.Offset;

			Where where = new(ExistField(fieldName), Comparison.EQ, fieldValue) { FuncToField = funcToField, FuncToField_Param1 = funcToField_Param1 };
			QuerySelect.Where.Add(where);

			bool result = await BaseSelect();

			QuerySelect.Where.Remove(where);
			QuerySelect.Limit = oldLimit;
			QuerySelect.Offset = oldOffset;

			return result;
		}

		protected async Task<bool> BaseSelectByField(string[] selectFields, string fieldName, object fieldValue, string funcToField = "", string funcToField_Param1 = "")
		{
			Where where = new(ExistField(fieldName), Comparison.EQ, fieldValue) { FuncToField = funcToField, FuncToField_Param1 = funcToField_Param1 };
			QuerySelect.Where.Add(where);

			List<string> existFields = new(selectFields.Length);
			foreach (var selectField in selectFields)
				existFields.Add(ExistField(selectField));

			QuerySelect.Field.AddRange(existFields);

			bool result = await BaseSelectSingle();

			QuerySelect.Where.Remove(where);
			QuerySelect.Field.RemoveAll(existFields.Contains);

			return result;
		}
	}
}