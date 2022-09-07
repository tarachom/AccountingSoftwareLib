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

namespace AccountingSoftware
{
	/// <summary>
	/// 
	/// </summary>
	public class JournalDocument
	{
		/// <summary>
		/// Назва як задано в конфігураторі
		/// </summary>
		public string TypeDocument { get; set; }

		/// <summary>
		/// Унікальний ідентифікатор запису
		/// </summary>
		public UnigueID UnigueID { get; set; }

		/// <summary>
		/// Назва документу
		/// </summary>
		public string DocName { get; set; }

		/// <summary>
		/// Номер документу
		/// </summary>
		public string DocNomer { get; set; }

		/// <summary>
		/// Дата документу
		/// </summary>
		public string DocDate { get; set; }

		/// <summary>
		/// Документ проведений
		/// </summary>
		public bool Spend { get; set; }

		/// <summary>
		/// Дата проведення документу
		/// </summary>
		public DateTime SpendDate { get; set; }

	}
}
