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
	/// Журнал Вибірка Документів
	/// </summary>
	public abstract class JournalSelect
	{
		public JournalSelect(Kernel kernel, string[] table, string[] typeDocument)
		{
			Kernel = kernel;
			Tables = table;
			TypeDocuments = typeDocument;
		}

		/// <summary>
		/// Ядро
		/// </summary>
		protected Kernel Kernel { get; private set; }

		/// <summary>
		/// Масив таблиць
		/// </summary>
		public string[] Tables { get; private set; }

		/// <summary>
		/// Масив типів документів
		/// </summary>
		public string[] TypeDocuments { get; private set; }

		/// <summary>
		/// Переміститися в початок вибірки
		/// </summary>
		public void MoveToFirst()
		{
			Position = 0;
			MoveNext();
		}

		/// <summary>
		/// Кількість елементів вибірки
		/// </summary>
		public int Count()
		{
			return BaseSelectList.Count;
		}

		/// <summary>
		/// Поточна позиція
		/// </summary>
		protected int Position { get; private set; }

		/// <summary>
		/// Список вибраних вказівників
		/// </summary>
		protected List<JournalDocument> BaseSelectList { get; private set; } = [];

		/// <summary>
		/// Переміститися на наступну позицію
		/// </summary>
		public bool MoveNext()
		{
			if (Position < BaseSelectList.Count)
			{
				Current = BaseSelectList[Position];
				Position++;
				return true;
			}
			else
			{
				Current = null;
				return false;
			}
		}

		public JournalDocument? Current { get; private set; } = null;

		/// <summary>
		/// Зчитати
		/// </summary>
		public async ValueTask<bool> Select(DateTime periodStart, DateTime periodEnd, string[]? typeDocSelect = null, bool? spendDocSelect = null)
		{
			Position = 0;
			Current = null;
			BaseSelectList.Clear();

			await Kernel.DataBase.SelectJournalDocumentPointer(Tables, TypeDocuments, BaseSelectList, periodStart, periodEnd, typeDocSelect, spendDocSelect);

			return Count() > 0;
		}
	}
}