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
	public abstract class DirectorySelect
	{
		public DirectorySelect(Kernel kernel, string table)
		{
			Kernel = kernel;
			Table = table;

			QuerySelect = new Query(table);
		}

		/// <summary>
		/// Запит SELECT
		/// </summary>
		public Query QuerySelect { get; set; }

		/// <summary>
		/// Перейти на початок вибірки
		/// </summary>
		public void MoveToFirst()
		{
			Position = 0;
			MoveToPosition();
		}

		/// <summary>
		/// Кількість елементів у вибірці
		/// </summary>
		public int Count()
		{
			return BaseSelectList.Count;
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
		/// Поточна позиція
		/// </summary>
		protected int Position { get; private set; }

		/// <summary>
		/// Поточний вказівник
		/// </summary>
		protected (UnigueID UnigueID, Dictionary<string, object>? Fields)? DirectoryPointerPosition { get; private set; } = null;

		/// <summary>
		/// Вибірка вказівників
		/// </summary>
		protected List<(UnigueID UnigueID, Dictionary<string, object>? Fields)> BaseSelectList { get; private set; } = [];

		/// <summary>
		/// Переміститися на одну позицію у вибірці
		/// </summary>
		protected bool MoveToPosition()
		{
			if (Position < BaseSelectList.Count)
			{
				DirectoryPointerPosition = BaseSelectList[Position++];
				return true;
			}
			else
			{
				DirectoryPointerPosition = null;
				return false;
			}
		}

		/// <summary>
		/// Вибрати дані
		/// </summary>
		protected async ValueTask<bool> BaseSelect()
		{
			Position = 0;
			DirectoryPointerPosition = null;
			BaseSelectList.Clear();

			await Kernel.DataBase.SelectDirectoryPointers(QuerySelect, BaseSelectList);

			return Count() > 0;
		}

		/// <summary>
		/// Вибрати один запис з бази даних
		/// </summary>
		protected async ValueTask<bool> BaseSelectSingle()
		{
			int oldLimitValue = QuerySelect.Limit;
			QuerySelect.Limit = 1;

			await BaseSelect();

			QuerySelect.Limit = oldLimitValue;

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

		public async ValueTask DeleteTempTable()
		{
			await Kernel.DataBase.DeleteDirectoryTempTable(this);
			QuerySelect.CreateTempTable = false;
		}
	}
}