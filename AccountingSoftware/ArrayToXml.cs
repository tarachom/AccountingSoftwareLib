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
using System.Xml;
using System.Xml.XPath;

namespace AccountingSoftware
{
	/// <summary>
	/// Перетворює масив даних в ХМЛ стрічку виду "<e>значення 1</e><e>значення 2</e>"
	/// </summary>
	/// <typeparam name="T">Тип даних масиву</typeparam>
	public static class ArrayToXml<T>
	{
		/// <summary>
		/// Функції перетворення масиву даних в ХМЛ стрічку
		/// </summary>
		/// <param name="value">Масив</param>
		/// <returns>ХМЛ стрічку</returns>
		public static string Convert(T[] value)
		{
			string XmlData = "";
			string LeftCData = "";
			string RightCData = "";

			if (value.GetType().Name == "String[]")
			{
				LeftCData = "<![CDATA[";
				RightCData = "]]>";
			}

			foreach (T item in value)
				XmlData += "<e>" + LeftCData + item.ToString() + RightCData + "</e>";

			return XmlData;
		}
	}

	/// <summary>
	/// Перетворити стрічку виду "<e>значення 1</e><e>значення 2</e>"
	/// в масив типу string[]
	/// </summary>
	public static class ArrayToXml
    {
		/// <summary>
		/// Функція перетоврення стрічки ХМЛ в масив
		/// </summary>
		/// <param name="xmlValue">Стрічка хмл</param>
		/// <returns>Текстовий масив</returns>
		public static string[] Convert(string xmlValue)
		{
			xmlValue = $"<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<root>" + xmlValue + "\n</root>";

			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(xmlValue);

			XPathNavigator xPathNavigator = xmlDocument.CreateNavigator();
			XPathNodeIterator eNode = xPathNavigator.Select("root/e");

			int counter = 0;
			string[] stringValue = new string[eNode.Count];

			while (eNode.MoveNext())
			{
				stringValue[counter] = eNode.Current.Value;
				counter++;
			}

			return stringValue;
		}

		/// <summary>
		/// Функція перетоврення стрічки ХМЛ в масив
		///		<uuid>c3f4b98f-1a6c-4073-aa10-66fda00d0618</uuid>
		///		<text>Введення залишків №00000009 від 07.08.2022</text>
		/// </summary>
		/// <param name="xmlValue">Стрічка хмл</param>
		/// <returns>Текстовий масив</returns>
		public static UuidAndText ConvertUuidAndText(string xmlValue)
		{
			xmlValue = $"<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<root>" + xmlValue + "\n</root>";

			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(xmlValue);

			XPathNavigator xPathNavigator = xmlDocument.CreateNavigator();
			XPathNavigator root = xPathNavigator.SelectSingleNode("root");

			Guid uuid = Guid.Parse(root.SelectSingleNode("uuid").Value);
			string text = root.SelectSingleNode("text").Value;

			return new UuidAndText(uuid, text);
		}
	}
}