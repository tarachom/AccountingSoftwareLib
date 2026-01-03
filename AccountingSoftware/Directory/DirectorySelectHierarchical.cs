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
	public abstract class DirectorySelectHierarchical(Kernel kernel, string table, string parentField) : Select(kernel, table, parentField)
	{
		/// <summary>
		/// Поточний вказівник
		/// </summary>
		protected (UnigueID UnigueID, UnigueID Parent, int Level, Dictionary<string, object>? Fields)? CurrentPointerPositionHierarchical { get; private set; } = null;

		/// <summary>
		/// Вибірка вказівників
		/// </summary>
		protected List<(UnigueID UnigueID, UnigueID Parent, int Level, Dictionary<string, object>? Fields)> BaseSelectListHierarchical { get; private set; } = [];

		/// <summary>
		/// Переміститися на одну позицію у вибірці
		/// </summary>
		protected override bool MoveToPosition()
		{
			if (Position < BaseSelectListHierarchical.Count)
			{
				CurrentPointerPositionHierarchical = BaseSelectListHierarchical[Position++];
				return true;
			}
			else
			{
				CurrentPointerPositionHierarchical = null;
				return false;
			}
		}

		/// <summary>
		/// Вибрати дані
		/// </summary>
		protected async ValueTask<bool> BaseSelect()
		{
			Position = 0;
			CurrentPointerPositionHierarchical = null;
			BaseSelectListHierarchical.Clear();

			await Kernel.DataBase.SelectDirectoryPointersHierarchical(QuerySelect, BaseSelectListHierarchical);

			return Count() > 0;
		}

		/// <summary>
		/// Вибрати один запис з бази даних
		/// </summary>
		protected async ValueTask<bool> BaseSelectSingle()
		{
			long? oldLimitValue = QuerySelect.Limit;
			QuerySelect.Limit = 1;

			await BaseSelect();

			QuerySelect.Limit = oldLimitValue;

			return Count() > 0;
		}
	}
}