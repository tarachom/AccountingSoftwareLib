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
	/// Журнал Вибірка Документів
	/// </summary>
	public abstract class JournalSelect
	{
		public JournalSelect(Kernel kernel, string[] table, string[] typeDocument)
		{
			Kernel = kernel;
			Tables = table;
			TypeDocuments = typeDocument;

			BaseSelectList = new List<JournalDocument>();
		}

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
		/// <returns></returns>
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
		/// Список вибраних вказівників
		/// </summary>
		protected List<JournalDocument> BaseSelectList { get; private set; }

		/// <summary>
		/// Переміститися на наступну позицію
		/// </summary>
		/// <returns></returns>
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

		public JournalDocument Current { get; private set; }

		/// <summary>
		/// Зчитати
		/// </summary>
		/// <returns></returns>
		public bool Select(DateTime periodStart, DateTime periodEnd, string[] typeDocSelect = null)
		{
			Position = 0;
			Current = null;
			BaseSelectList.Clear();

			Kernel.DataBase.SelectJournalDocumentPointer(Tables, TypeDocuments, BaseSelectList, periodStart, periodEnd, typeDocSelect);

			return Count() > 0;
		}

		public void Dispose()
		{
			Kernel = null;
			Current = null;
			BaseSelectList = null;
			Position = 0;
		}
	}
}