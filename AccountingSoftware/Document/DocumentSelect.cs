/*
Copyright (C) 2019-2024 TARAKHOMYN YURIY IVANOVYCH
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
	public abstract class DocumentSelect
	{
		public DocumentSelect(Kernel kernel, string table)
		{
			QuerySelect = new Query(table);
			Kernel = kernel;
		}

		/// <summary>
		/// Запит SELECT
		/// </summary>
		public Query QuerySelect { get; set; }

		/// <summary>
		/// Переміститися в початок вибірки
		/// </summary>
		public void MoveToFirst()
		{
			Position = 0;
			MoveToPosition();
		}

		/// <summary>
		/// Кількість елементів вибірки
		/// </summary>
		public int Count()
		{
			return BaseSelectList.Count;
		}

		/// <summary>
		/// Ядро
		/// </summary>
		protected Kernel Kernel { get; private set; }

		/// <summary>
		/// Поточна позиція
		/// </summary>
		protected int Position { get; private set; }

		/// <summary>
		/// Поточний вказівник
		/// </summary>
		protected (UnigueID, Dictionary<string, object>?)? DocumentPointerPosition { get; private set; } = null;

		/// <summary>
		/// Список вибраних вказівників (UnigueID and Dictionary<string, object>?)
		/// </summary>
		protected List<(UnigueID, Dictionary<string, object>?)> BaseSelectList { get; private set; } = [];

		/// <summary>
		/// Переміститися на наступну позицію
		/// </summary>
		protected bool MoveToPosition()
		{
			if (Position < BaseSelectList.Count)
			{
				DocumentPointerPosition = BaseSelectList[Position++];
				return true;
			}
			else
			{
				DocumentPointerPosition = null;
				return false;
			}
		}

		/// <summary>
		/// Зчитати
		/// </summary>
		protected async ValueTask<bool> BaseSelect()
		{
			Position = 0;
			DocumentPointerPosition = null;
			BaseSelectList.Clear();

			await Kernel.DataBase.SelectDocumentPointer(QuerySelect, BaseSelectList);

			return Count() > 0;
		}

		/// <summary>
		/// Зчитати один вказівник
		/// </summary>
		protected async ValueTask<bool> BaseSelectSingle()
		{
			int oldLimitValue = QuerySelect.Limit;
			QuerySelect.Limit = 1;

			await BaseSelect();

			QuerySelect.Limit = oldLimitValue;

			return Count() > 0;
		}
	}
}