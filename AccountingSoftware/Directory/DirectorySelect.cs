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
	/// Довідник Вибірка Вказівників
	/// </summary>
	public abstract class DirectorySelect(Kernel kernel, string table, string[]? fieldPresentation = null) : Select(kernel, table, "", "", fieldPresentation)
	{
		/// <summary>
		/// Вибрати дані
		/// </summary>
		protected async Task<bool> BaseSelect()
		{
			Position = 0;
			CurrentPointerPosition = null;
			CurrentPointerPresentation = null;
			BaseSelectList.Clear();

			await Kernel.DataBase.SelectDirectoryPointers(QuerySelect, BaseSelectList);

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
			Where where = new(fieldName, Comparison.EQ, fieldValue) { FuncToField = funcToField, FuncToField_Param1 = funcToField_Param1 };
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
		protected async Task<bool> BaseFindListByField(string fieldName, object fieldValue, int limit = 0, int offset = 0, string funcToField = "", string funcToField_Param1 = "")
		{
			long? oldLimit = QuerySelect.Limit;
			long? oldOffset = QuerySelect.Offset;

			Where where = new(fieldName, Comparison.EQ, fieldValue) { FuncToField = funcToField, FuncToField_Param1 = funcToField_Param1 };
			QuerySelect.Where.Add(where);

			if (limit > 0) QuerySelect.Limit = limit;
			if (offset > 0) QuerySelect.Offset = offset;

			bool result = await BaseSelect();

			QuerySelect.Where.Remove(where);
			QuerySelect.Limit = oldLimit;
			QuerySelect.Offset = oldOffset;

			return result;
		}
	}
}