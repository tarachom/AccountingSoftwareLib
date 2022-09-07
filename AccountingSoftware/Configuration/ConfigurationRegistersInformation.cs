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
	/// Регістри відомостей
	/// </summary>
	public class ConfigurationRegistersInformation : ConfigurationObject
	{
		/// <summary>
		/// Регістри відомостей
		/// </summary>
		public ConfigurationRegistersInformation()
		{
			DimensionFields = new Dictionary<string, ConfigurationObjectField>();
			ResourcesFields = new Dictionary<string, ConfigurationObjectField>();
			PropertyFields = new Dictionary<string, ConfigurationObjectField>();
		}

		/// <summary>
		/// Регістри відомостей
		/// </summary>
		/// <param name="name">Назва</param>
		/// <param name="table">Таблиця в базі даних</param>
		/// <param name="desc">Опис</param>
		public ConfigurationRegistersInformation(string name, string table, string desc = "") : this()
		{
			Name = name;
			Table = table;
			Desc = desc;
		}

		/// <summary>
		/// Виміри
		/// </summary>
		public Dictionary<string, ConfigurationObjectField> DimensionFields { get; }

		/// <summary>
		/// Русурси
		/// </summary>
		public Dictionary<string, ConfigurationObjectField> ResourcesFields { get; }

		/// <summary>
		/// Реквізити
		/// </summary>
		public Dictionary<string, ConfigurationObjectField> PropertyFields { get; }

		#region Append

		public void AppendDimensionField(ConfigurationObjectField field)
		{
			DimensionFields.Add(field.Name, field);
		}

		public void AppendResourcesField(ConfigurationObjectField field)
		{
			ResourcesFields.Add(field.Name, field);
		}

		public void AppendPropertyField(ConfigurationObjectField field)
		{
			PropertyFields.Add(field.Name, field);
		}

		#endregion
	}
}
