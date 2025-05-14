/*
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
                new ("string", "[ string ] - Текст"),
				new ("integer", "[ integer ] - Ціле число"),    // -2 147 483 648 до 2 147 483 647
				new ("numeric", "[ numeric ] - Число з комою"), // ±1,0 x 10^-28 до ±7,92^28 x 1028 16 байт
				new ("boolean", "[ boolean ] - Логічне значення"),
				new ("date", "[ date ] - Дата"),
				new ("datetime", "[ datetime ] - Дата та час"),
				new ("time", "[ time ] - Час"),
				new ("enum", "[ enum ] - Перелічення"),
				new ("pointer", "[ pointer ] - Вказівник"),
				new ("composite_pointer", "[ composite_pointer ] - Вказівник з вибором типу"),
				new ("any_pointer", "[ any_pointer ] - Унікальний ідентифікатор Guid"),
				new ("composite_text", "[ composite_text ] - Кортеж - два текстові поля"),
				new ("bytea", "[ bytea ] - Бінарні дані"),
				new ("string[]", "[ a, b, ... ] - Масив текст"),
				new ("integer[]", "[ 1, 2, ... ] - Масив ціле число"),
				new ("numeric[]", "[ 1.5, 2.8, ... ] - Масив число з комою"),
				new ("uuid[]", "[ uuid, ... ] - Масив унікальних ідентифікаторів")
			];

			//fieldTypes.Add(new FieldType("long", "[ long ] - Велике ціле число")); // -9 223 372 036 854 775 808 до 9 223 372 036 854 775 807

			//character [ (n) ] - char [ (n) ] - fixed-length character string
			//character varying [ (n) ] - varchar [ (n) ] - variable-length character string
			//serial - autoincrementing four-byte integer
			//xml

			return fieldTypes;
		}
	}
}