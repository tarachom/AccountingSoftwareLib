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
	/// Документ Вибірка вказівників
	/// </summary>
	public abstract class DocumentSelect(Kernel kernel, string table) : Select(kernel, table)
	{
		/// <summary>
		/// Поточний вказівник !!! Видалити пізніше
		/// </summary>
		/*protected (UnigueID UnigueID, Dictionary<string, object>? Fields)? DocumentPointerPosition
		{
			get
			{
				return CurrentPointerPosition;
			}
		}*/

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
			int oldLimit = QuerySelect.Limit;
			QuerySelect.Limit = 1;

			await BaseSelect();

			QuerySelect.Limit = oldLimit;

			return Count() > 0;
		}
	}
}