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
	/// Використовується для елементів ComboBox, коли потрібно задати відображення для обєкта
	/// </summary>
	/// <typeparam name="T">Тип обєкта для якого задається відображення</typeparam>
	/// <example>
	/// comboBoxRegisterType.Items.Add(new NameValue<TypeRegistersAccumulation>("Залишки", TypeRegistersAccumulation.Residues));
	/// comboBoxRegisterType.Items.Add(new NameValue<TypeRegistersAccumulation>("Обороти", TypeRegistersAccumulation.Turnover));
	///	comboBoxRegisterType.SelectedItem = comboBoxRegisterType.Items[0];
	///	
	/// ...
	/// 
	/// var a = ((NameValue<TypeRegistersAccumulation>)comboBoxRegisterType.SelectedItem).Value
	/// </example>
	public class NameValue<T>
	{
		public NameValue() { }

		public NameValue(string name, T value)
		{
			Name = name;
			Value = value;
		}

		public string Name { get; set; }

		public T Value { get; set; }

        public bool Equals(T value)
        {
			return Value.ToString() == value.ToString();
		}

        public override string ToString()
		{
			return Name;
		}
	}
}