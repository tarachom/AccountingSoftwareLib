﻿/*
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
	/// Запис регістра
	/// </summary>
	public abstract class RegisterRecord
	{
		/// <summary>
		/// Унікальний ідентифікатор
		/// </summary>
		public Guid UID { get; set; }

		/// <summary>
		/// Дата запису
		/// </summary>
		public DateTime Period { get; set; }

		/// <summary>
		/// Власник запису
		/// </summary>
		public Guid Owner { get; set; }

		/// <summary>
		/// Тип власника запису (Назва = Довідники або Документи, Текст = Тип як задано в конфігураторі)
		/// </summary>
		public NameAndText? OwnerType { get; set; } = null;

		/// <summary>
		/// Назва власника запису
		/// </summary>
		public string OwnerName { get; set; } = "";
	}
}