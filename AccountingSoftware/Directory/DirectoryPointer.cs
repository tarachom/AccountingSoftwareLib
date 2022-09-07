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
	/// Довідник Вказівник
	/// </summary>
	public class DirectoryPointer
	{
		public DirectoryPointer()
		{
			UnigueID = new UnigueID(Guid.Empty);
		}

		public DirectoryPointer(Kernel kernel, string table) : this()
		{
			Table = table;
			Kernel = kernel;
		}

		/// <summary>
		/// Ініціалізація вказівника
		/// </summary>
		/// <param name="uid">Унікальний ідентифікатор</param>
		/// <param name="fields">Поля які потрібно додатково зчитати з бази даних</param>
		public void Init(UnigueID uid, Dictionary<string, object> fields = null)
		{
			UnigueID = uid;
			Fields = fields;
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
		/// Унікальний ідентифікатор
		/// </summary>
		public UnigueID UnigueID { get; private set; }

		/// <summary>
		/// Поля які потрібно додатково зчитати з бази даних 
		/// </summary>
		public Dictionary<string, object> Fields { get; private set; }

		/// <summary>
		/// Чи це пустий ідентифікатор
		/// </summary>
		/// <returns></returns>
		public bool IsEmpty()
		{
			return (UnigueID.UGuid == Guid.Empty);
		}

		/// <summary>
		/// Отримати представлення вказівника
		/// </summary>
		/// <param name="fieldPresentation">Список полів які представляють вказівник (Назва, опис і т.д)</param>
		/// <returns></returns>
		protected string BasePresentation(string[] fieldPresentation)
		{
			if (!IsEmpty() && fieldPresentation.Length != 0)
			{
				Query query = new Query(Table);
				query.Field.AddRange(fieldPresentation);
				
				query.Where.Add(new Where("uid", Comparison.EQ, UnigueID.UGuid));

				return Kernel.DataBase.GetDirectoryPresentation(query, fieldPresentation);
			}
			else
				return "";
		}

		/// <summary>
		/// Отримати ункальний ідентифікатор у форматі Guid
		/// </summary>
		/// <returns></returns>
		public Guid GetPointer()
		{
			return UnigueID.UGuid;
		}

		public override string ToString()
		{
			return UnigueID.UGuid.ToString();
		}
	}
}
