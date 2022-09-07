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
	/// Документ
	/// </summary>
	public class ConfigurationDocuments : ConfigurationObject
	{
		/// <summary>
		/// Документ
		/// </summary>
		public ConfigurationDocuments()
		{
			Fields = new Dictionary<string, ConfigurationObjectField>();
			TabularParts = new Dictionary<string, ConfigurationObjectTablePart>();
			AllowRegisterAccumulation = new List<string>();
			TriggerFunctions = new ConfigurationTriggerFunctions();
			SpendFunctions = new ConfigurationSpendFunctions();
		}

		/// <summary>
		/// Документ
		/// </summary>
		/// <param name="name">Назва</param>
		/// <param name="table">Таблиця в базі даних</param>
		/// <param name="desc">Опис</param>
		public ConfigurationDocuments(string name, string table, string desc = "") : this()
		{
			Name = name;
			Table = table;
			Desc = desc;
		}

		/// <summary>
		/// Поля
		/// </summary>
		public Dictionary<string, ConfigurationObjectField> Fields { get; }

		/// <summary>
		/// Табличні частини
		/// </summary>
		public Dictionary<string, ConfigurationObjectTablePart> TabularParts { get; }

		/// <summary>
		/// Регістри накопичення по яких може робити рухи документ
		/// </summary>
		public List<string> AllowRegisterAccumulation { get; private set; }

		/// <summary>
		/// Тригери
		/// </summary>
		public ConfigurationTriggerFunctions TriggerFunctions { get; set; }

		/// <summary>
		/// Функції (проведення/очищення проводок) документу
		/// </summary>
		public ConfigurationSpendFunctions SpendFunctions { get; set; }

		public ConfigurationDocuments Copy()
		{
			ConfigurationDocuments confDocCopy = new ConfigurationDocuments(this.Name, this.Table, this.Desc);

			foreach (KeyValuePair<string, ConfigurationObjectField> fields in this.Fields)
				confDocCopy.Fields.Add(fields.Key, fields.Value);

			foreach (KeyValuePair<string, ConfigurationObjectTablePart> tablePart in this.TabularParts)
				confDocCopy.TabularParts.Add(tablePart.Key, tablePart.Value.Copy());

			confDocCopy.TriggerFunctions = this.TriggerFunctions;

			confDocCopy.SpendFunctions = this.SpendFunctions;

			return confDocCopy;
		}

		/// <summary>
		/// Додати нове поле в список полів
		/// </summary>
		/// <param name="field">Нове поле</param>
		public void AppendField(ConfigurationObjectField field)
		{
			Fields.Add(field.Name, field);
		}

		/// <summary>
		/// Додати нову табличну частину
		/// </summary>
		/// <param name="tablePart">Нова таблична частина</param>
		public void AppendTablePart(ConfigurationObjectTablePart tablePart)
		{
			TabularParts.Add(tablePart.Name, tablePart);
		}
	}
}
