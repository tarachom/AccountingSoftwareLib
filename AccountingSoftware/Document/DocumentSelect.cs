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
	public abstract class DocumentSelect(Kernel kernel, string table) : Select(kernel, table)
	{
		/// <summary>
		/// Зчитати
		/// </summary>
		protected async ValueTask<bool> BaseSelect()
		{
			Position = 0;
			CurrentPointerPosition = null;
			BaseSelectList.Clear();

			await Kernel.DataBase.SelectDocumentPointer(QuerySelect, BaseSelectList);

			return Count() > 0;
		}

		/// <summary>
		/// Зчитати один вказівник
		/// </summary>
		protected async ValueTask<bool> BaseSelectSingle()
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
		/// <returns>Повертає перший знайдений вказівник</returns>
		protected async ValueTask<UniqueID?> BaseFindByField(string fieldName, object fieldValue, string funcToField = "", string funcToField_Param1 = "")
		{
			Query querySelect = new(Table);
			querySelect.Where.Add(new Where(fieldName, Comparison.EQ, fieldValue) { FuncToField = funcToField, FuncToField_Param1 = funcToField_Param1 });

			return await Kernel.DataBase.FindDocumentPointer(querySelect);
		}

		/// <summary>
		/// Пошук по значенню поля (наприклад пошук по назві)
		/// </summary>
		/// <param name="fieldName">Назва поля в базі даних<</param>
		/// <param name="fieldValue">Значення поля</param>
		/// <param name="limit">Кількість елементів які можна вибрати</param>
		/// <param name="offset">Зміщення від початку вибірки</param>
		/// <returns>Повертає список знайдених вказівників</returns>
		protected async ValueTask<List<(UniqueID UniqueID, Dictionary<string, object>? Fields)>> BaseFindListByField(string fieldName, object fieldValue, int limit = 0, int offset = 0)
		{
			List<(UniqueID UniqueID, Dictionary<string, object>? Fields)> documentPointerList = [];

			Query querySelect = new(Table) { Limit = limit, Offset = offset };
			querySelect.Where.Add(new Where(fieldName, Comparison.EQ, fieldValue));

			await Kernel.DataBase.SelectDocumentPointer(querySelect, documentPointerList);

			return documentPointerList;
		}
	}
}