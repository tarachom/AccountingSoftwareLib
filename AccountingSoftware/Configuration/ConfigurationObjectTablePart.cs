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
	/// Таблична частина
	/// </summary>
	public class ConfigurationObjectTablePart : ConfigurationObject
	{
		/// <summary>
		/// Таблична частина
		/// </summary>
		public ConfigurationObjectTablePart()
		{
			Fields = new Dictionary<string, ConfigurationObjectField>();
		}

		/// <summary>
		/// Таблична частина
		/// </summary>
		/// <param name="name">Назва</param>
		/// <param name="table">Таблиця в базі даних</param>
		/// <param name="desc">Опис</param>
		public ConfigurationObjectTablePart(string name, string table, string desc = "") : this()
		{
			Name = name;
			Table = table;
			Desc = desc;
		}

		/// <summary>
		/// Поля
		/// </summary>
		public Dictionary<string, ConfigurationObjectField> Fields { get; }

		public ConfigurationObjectTablePart Copy()
        {
			ConfigurationObjectTablePart confObjectTablePart = new ConfigurationObjectTablePart(this.Name, this.Table, this.Desc);

			foreach (KeyValuePair<string, ConfigurationObjectField> fields in this.Fields)
				confObjectTablePart.Fields.Add(fields.Key, fields.Value);

			return confObjectTablePart;
		}

		/// <summary>
		/// Додати нове поле в список полів
		/// </summary>
		/// <param name="field">Нове поле</param>
		public void AppendField(ConfigurationObjectField field)
		{
			Fields.Add(field.Name, field);
		}
	}
}
