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
			List<FieldType> fieldTypes = new List<FieldType>();

			fieldTypes.Add(new FieldType("string",            "[ string ] - Текст"));
			fieldTypes.Add(new FieldType("integer",           "[ integer ] - Ціле число")); //От -2 147 483 648 до 2 147 483 647
			//fieldTypes.Add(new FieldType("long",            "[ long ] - Велике ціле число")); //От -9 223 372 036 854 775 808 до 9 223 372 036 854 775 807
			fieldTypes.Add(new FieldType("numeric",           "[ numeric ] - Число з комою")); //от ±1,0 x 10^-28 до ±7,92^28 x 1028 16 байт
			fieldTypes.Add(new FieldType("boolean",           "[ boolean ] - Логічне значення"));
			fieldTypes.Add(new FieldType("date",              "[ date ] - Дата"));
			fieldTypes.Add(new FieldType("datetime",          "[ datetime ] - Дата та час"));
			fieldTypes.Add(new FieldType("time",              "[ time ] - Час"));
			fieldTypes.Add(new FieldType("enum",              "[ enum ] - Перелічення"));
			fieldTypes.Add(new FieldType("pointer",           "[ pointer ] - Вказівник на елемент конфігурації"));
			fieldTypes.Add(new FieldType("any_pointer",       "[ any_pointer ] - Вказівник на різні елементи конфігурації"));
			fieldTypes.Add(new FieldType("empty_pointer",     "[ empty_pointer ] - Пустий вказівник"));
			fieldTypes.Add(new FieldType("composite_pointer", "[ composite_pointer ] - Вказівник + текст"));
			//fieldTypes.Add(new FieldType("uuid[]",        "[ uuid1, uuid2, uuid3 ... ] - Масив вказівників на елемент конфігурації"));
			fieldTypes.Add(new FieldType("string[]",          "[ Текст1, Текст2, ... ] - [ string[] ] - Масив текстових даних"));
			fieldTypes.Add(new FieldType("integer[]",         "[ Число1, Число2, ... ] - [ integer[] ] - Масив цілих чисел"));
			fieldTypes.Add(new FieldType("numeric[]",         "[ Число1.0, Число2.0, ... ] - [ numeric[] ] - Масив чисел з комою"));

			//character [ (n) ] - char [ (n) ] - fixed-length character string
			//character varying [ (n) ] - varchar [ (n) ] - variable-length character string
			//bytea - binary data (“byte array”)
			//serial - autoincrementing four-byte integer
			//xml
			//uuid

			return fieldTypes;
		}
	}
}