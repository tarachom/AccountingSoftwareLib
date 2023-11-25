/*
Copyright (C) 2019-2023 TARAKHOMYN YURIY IVANOVYCH
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
	/// Типи даних полів
	/// </summary>
	public class FieldType
	{
		/// <summary>
		/// Типи даних полів
		/// </summary>
		/// <param name="confTypeName">Тип даних в конфігурації</param>
		/// <param name="viewTypeName">Представлення типу даних в списку</param>
		public FieldType(string confTypeName, string viewTypeName)
		{
			ConfTypeName = confTypeName;
			ViewTypeName = viewTypeName;
		}

		/// <summary>
		/// Тип даних в конфігурації
		/// </summary>
		public string ConfTypeName { get; set; }

		/// <summary>
		/// Представлення типу даних в списку
		/// </summary>
		public string ViewTypeName { get; set; }

		/// <summary>
		/// ViewTypeName
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return ViewTypeName;
		}

		/// <summary>
		/// Функція повертає повний список типів даних які використовується в конфігурації
		/// </summary>
		/// <returns>Список типів даних</returns>
		public static List<FieldType> DefaultList()
		{
			List<FieldType> fieldTypes =
            [
                new FieldType("string", "[ string ] - Текст"),
				new FieldType("integer", "[ integer ] - Ціле число"), //От -2 147 483 648 до 2 147 483 647
				new FieldType("numeric", "[ numeric ] - Число з комою"), //от ±1,0 x 10^-28 до ±7,92^28 x 1028 16 байт
				new FieldType("boolean", "[ boolean ] - Логічне значення"),
				new FieldType("date", "[ date ] - Дата"),
				new FieldType("datetime", "[ datetime ] - Дата та час"),
				new FieldType("time", "[ time ] - Час"),
				new FieldType("enum", "[ enum ] - Перелічення"),
				new FieldType("pointer", "[ pointer ] - Вказівник статичний"),
				new FieldType("composite_pointer", "[ composite_pointer ] - Вказівник динамічний"),
				new FieldType("any_pointer", "[ any_pointer ] - Унікальний ідентифікатор"),
				new FieldType("bytea", "[ byte ] - Бінарні дані"),
				new FieldType("string[]", "[ a, b, ... ]"),
				new FieldType("integer[]", "[ 1, 2, ... ]"),
				new FieldType("numeric[]", "[ 1.0, 2.0, ... ]")
			];

			//fieldTypes.Add(new FieldType("long", "[ long ] - Велике ціле число")); //От -9 223 372 036 854 775 808 до 9 223 372 036 854 775 807
			//fieldTypes.Add(new FieldType("uuid[]", "[ uuid1, uuid2, uuid3 ... ] - Масив вказівників на елемент конфігурації"));

			//character [ (n) ] - char [ (n) ] - fixed-length character string
			//character varying [ (n) ] - varchar [ (n) ] - variable-length character string
			//serial - autoincrementing four-byte integer
			//xml

			return fieldTypes;
		}
	}
}