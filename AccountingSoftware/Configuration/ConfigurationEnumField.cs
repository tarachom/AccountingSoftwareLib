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

namespace AccountingSoftware
{
	/// <summary>
	/// Поле перелічення
	/// </summary>
	public class ConfigurationEnumField
	{
		/// <summary>
		/// Поле перелічення
		/// </summary>
		public ConfigurationEnumField() { /*..*/ }

		/// <summary>
		/// Поле перелічення 
		/// </summary>
		/// <param name="name">Назва</param>
		/// <param name="value">Значення</param>
		/// <param name="desc">Опис</param>
		public ConfigurationEnumField(string name, int value, string desc = "")
		{
			Name = name;
			Value = value;
			Desc = desc;
		}

		/// <summary>
		/// Назва
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Значення
		/// </summary>
		public int Value { get; set; }

		/// <summary>
		/// Опис
		/// </summary>
		public string Desc { get; set; }
	}
}
