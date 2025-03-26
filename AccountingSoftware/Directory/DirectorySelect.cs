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
	/// Довідник Вибірка Вказівників
	/// </summary>
	public abstract class DirectorySelect(Kernel kernel, string table) : Select(kernel, table)
	{
		/// <summary>
		/// Вибрати дані
		/// </summary>
		protected async ValueTask<bool> BaseSelect()
		{
			Position = 0;
			CurrentPointerPosition = null;
			BaseSelectList.Clear();

			await Kernel.DataBase.SelectDirectoryPointers(QuerySelect, BaseSelectList);

			return Count() > 0;
		}

		/// <summary>
		/// Зчитати один вказівник
		/// </summary>
		protected async ValueTask<bool> BaseSelectSingle()
		{
			int oldLimit = QuerySelect.Limit;
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
		/// <returns>Повертає перший знайдений вказівник</returns>
		protected async ValueTask<UnigueID?> BaseFindByField(string fieldName, object fieldValue)
		{
			Query querySelect = new(Table);
			querySelect.Where.Add(new Where(fieldName, Comparison.EQ, fieldValue));

			return await Kernel.DataBase.FindDirectoryPointer(querySelect);
		}

		/// <summary>
		/// Пошук по значенню поля (наприклад пошук по назві)
		/// </summary>
		/// <param name="fieldName">Назва поля в базі даних<</param>
		/// <param name="fieldValue">Значення поля</param>
		/// <param name="limit">Кількість елементів які можна вибрати</param>
		/// <param name="offset">Зміщення від початку вибірки</param>
		/// <returns>Повертає список знайдених вказівників</returns>
		protected async ValueTask<List<(UnigueID UnigueID, Dictionary<string, object>? Fields)>> BaseFindListByField(string fieldName, object fieldValue, int limit = 0, int offset = 0)
		{
			List<(UnigueID UnigueID, Dictionary<string, object>? Fields)> directoryPointerList = [];

			Query querySelect = new(Table) { Limit = limit, Offset = offset };
			querySelect.Where.Add(new Where(fieldName, Comparison.EQ, fieldValue));

			await Kernel.DataBase.SelectDirectoryPointers(querySelect, directoryPointerList);

			return directoryPointerList;
		}
	}
}