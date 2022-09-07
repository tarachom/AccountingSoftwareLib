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

using System.Collections.Generic;

namespace AccountingSoftware
{
	/// <summary>
	/// Перелічення
	/// </summary>
	public class ConfigurationEnums
	{
		/// <summary>
		/// Перелічення
		/// </summary>
		public ConfigurationEnums()
		{
			Fields = new Dictionary<string, ConfigurationEnumField>();
		}

		/// <summary>
		/// Перелічення
		/// </summary>
		/// <param name="name">Назва</param>
		/// <param name="serialNumber">Останній порядковий номер</param>
		/// <param name="desc">Опис</param>
		public ConfigurationEnums(string name, int serialNumber = 0, string desc = "") : this()
		{
			Name = name;
			Desc = desc;
			SerialNumber = serialNumber;
		}

		/// <summary>
		/// Назва перелічення
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Опис
		/// </summary>
		public string Desc { get; set; }

		/// <summary>
		/// Останній порядковий номер використаний для поля перелічення.
		/// Довідка: в базі даних перелічення зберігається як тип int4. Коли добавляється нове поле,
		/// йому задається зразу значення. SerialNumber зберігає останнє значення.
		/// </summary>
		public int SerialNumber { get; set; }

		/// <summary>
		/// Поля перелічення
		/// </summary>
		public Dictionary<string, ConfigurationEnumField> Fields { get; }

		/// <summary>
		/// Додати нове поле в список полів перелічення
		/// </summary>
		/// <param name="field">Нове поле</param>
		public void AppendField(ConfigurationEnumField field)
		{
			Fields.Add(field.Name, field);
		}
	}
}