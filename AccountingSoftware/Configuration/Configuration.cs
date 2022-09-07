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
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace AccountingSoftware
{
	/// <summary>
	/// Конфігурація.
	/// В цьому класі міститься вся інформація про конфігурацію.
	/// </summary>
	public class Configuration
	{
		/// <summary>
		/// Конструктор
		/// </summary>
		public Configuration()
		{
			ConstantsBlock = new Dictionary<string, ConfigurationConstantsBlock>();
			Directories = new Dictionary<string, ConfigurationDirectories>();
			Documents = new Dictionary<string, ConfigurationDocuments>();
			Enums = new Dictionary<string, ConfigurationEnums>();
			RegistersInformation = new Dictionary<string, ConfigurationRegistersInformation>();
			RegistersAccumulation = new Dictionary<string, ConfigurationRegistersAccumulation>();

			ReservedUnigueTableName = new List<string>();
			ReservedUnigueColumnName = new Dictionary<string, List<string>>();
		}

        #region Поля

        /// <summary>
        /// Назва конфігурації
        /// </summary>
        public string Name { get; set; }

		/// <summary>
		/// Простір імен для конфігурації
		/// </summary>
		public string NameSpace { get; set; }

		/// <summary>
		/// Автор конфігурації
		/// </summary>
		public string Author { get; set; }

		/// <summary>
		/// Опис
		/// </summary>
		public string Desc { get; set; }

		/// <summary>
		/// Шлях до хмл файлу конфігурації
		/// </summary>
		public string PathToXmlFileConfiguration { get; set; }

		/// <summary>
		/// Шлях до копії хмл файлу конфігурації
		/// </summary>
		public string PathToCopyXmlFileConfiguration { get; set; } //???

		/// <summary>
		/// Шлях до тимчасового хмл файлу конфігурації
		/// </summary>
		public string PathToTempXmlFileConfiguration { get; set; } //???

		/// <summary>
		/// Блоки констант
		/// </summary>
		public Dictionary<string, ConfigurationConstantsBlock> ConstantsBlock { get; }

		/// <summary>
		/// Довідники
		/// </summary>
		public Dictionary<string, ConfigurationDirectories> Directories { get; }

		/// <summary>
		/// Документи
		/// </summary>
		public Dictionary<string, ConfigurationDocuments> Documents { get; }

		/// <summary>
		/// Перелічення
		/// </summary>
		public Dictionary<string, ConfigurationEnums> Enums { get; }

		/// <summary>
		/// Регістри відомостей
		/// </summary>
		public Dictionary<string, ConfigurationRegistersInformation> RegistersInformation { get; }

		/// <summary>
		/// Регістри накопичення
		/// </summary>
		public Dictionary<string, ConfigurationRegistersAccumulation> RegistersAccumulation { get; }

        #endregion

        #region Private_Function

        /// <summary>
        /// Список зарезервованих назв таблиць.
        /// Довідка: коли створюється новий довідник, чи документ 
        /// для нього резервується нова унікальна назва таблиці в базі даних. 
        /// </summary>
        private List<string> ReservedUnigueTableName { get; set; }

		/// <summary>
		/// Список зарезервованих назв стовпців.
		/// Ключем виступає назва таблиці для якої резервуються стовпці.
		/// </summary>
		private Dictionary<string, List<string>> ReservedUnigueColumnName { get; set; }

		/// <summary>
		/// Масив з бук анг. алфавіту. Використовується для задання назви таблиці або стовпчика в базі даних
		/// </summary>
		/// <returns></returns>
		private static string[] GetEnglishAlphabet()
		{
			return new string[]
			{
				"a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "n",
				"m", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z"
			};
		}

		#endregion

		#region Append

		/// <summary>
		/// Додати константу
		/// </summary>
		/// <param name="blockName"></param>
		/// <param name="constants"></param>
		public void AppendConstants(string blockName, ConfigurationConstants constants)
		{
			ConstantsBlock[blockName].Constants.Add(constants.Name, constants);
			constants.Block = ConstantsBlock[blockName];
		}

		/// <summary>
		/// Додати блок констант
		/// </summary>
		/// <param name="constantsBlock"></param>
		public void AppendConstantsBlock(ConfigurationConstantsBlock constantsBlock)
		{
			ConstantsBlock.Add(constantsBlock.BlockName, constantsBlock);
		}

		/// <summary>
		/// Додати довідник в список довідників
		/// </summary>
		/// <param name="Directory">Довідник</param>
		public void AppendDirectory(ConfigurationDirectories Directory)
		{
			Directories.Add(Directory.Name, Directory);
		}

		/// <summary>
		/// Додати перелічення в список перелічень
		/// </summary>
		/// <param name="Enum">Перелічення</param>
		public void AppendEnum(ConfigurationEnums Enum)
		{
			Enums.Add(Enum.Name, Enum);
		}

		/// <summary>
		/// Додати документ в список документів
		/// </summary>
		/// <param name="Document">Документ</param>
		public void AppendDocument(ConfigurationDocuments document)
		{
			Documents.Add(document.Name, document);
		}

		/// <summary>
		/// Додати регістр в список регістрів
		/// </summary>
		/// <param name="registersInformation">Регістр</param>
		public void AppendRegistersInformation(ConfigurationRegistersInformation registersInformation)
		{
			RegistersInformation.Add(registersInformation.Name, registersInformation);
		}

		/// <summary>
		/// Додати регістр в список регістрів
		/// </summary>
		/// <param name="registersAccumulation">Регістр</param>
		public void AppendRegistersAccumulation(ConfigurationRegistersAccumulation registersAccumulation)
		{
			RegistersAccumulation.Add(registersAccumulation.Name, registersAccumulation);
		}

		#endregion

		#region Function

		/// <summary>
		/// Пошук ссилок довідників і документів
		/// </summary>
		/// <param name="searchName">Назва довідника або документу</param>
		/// <returns>Повертає список довідників або документів які вказують на searchName</returns>
		public List<string> SearchForPointers(string searchName)
		{
			if (searchName.IndexOf(".") > 0)
			{
				string[] searchNameSplit = searchName.Split(new string[] { "." }, StringSplitOptions.None);

				if (!(searchNameSplit[0] == "Довідники" || searchNameSplit[0] == "Документи"))
					throw new Exception("Перша частина назви має бути 'Довідники' або 'Документи'");
			}
			else
				throw new Exception("Назва для пошуку має бути 'Довідники.<Назва довідника>' або 'Документи.<Назва документу>'");

			List<string> ListPointer = new List<string>();

			//Перевірити константи
			foreach (ConfigurationConstantsBlock constantsBlockItem in ConstantsBlock.Values)
			{
				foreach (ConfigurationConstants constantsItem in constantsBlockItem.Constants.Values)
				{
					if (constantsItem.Type == "pointer" && constantsItem.Pointer == searchName)
						ListPointer.Add("Константа (Блок " + constantsBlockItem.BlockName + "): " + constantsItem.Name + "." + constantsItem.Name);
				}
			}

			//Перевірити поля довідників та поля табличних частин чи часом вони не ссилаються на цей довідник
			foreach (ConfigurationDirectories directoryItem in Directories.Values)
			{
				//Поля довідника
				foreach (ConfigurationObjectField directoryField in directoryItem.Fields.Values)
				{
					if (directoryField.Type == "pointer" && directoryField.Pointer == searchName)
						ListPointer.Add("Довідники: " + directoryItem.Name + "." + directoryField.Name);
				}

				//Табличні частини
				foreach (ConfigurationObjectTablePart directoryTablePart in directoryItem.TabularParts.Values)
				{
					//Поля табличної частини
					foreach (ConfigurationObjectField tablePartField in directoryTablePart.Fields.Values)
					{
						if (tablePartField.Type == "pointer" && tablePartField.Pointer == searchName)
							ListPointer.Add("Довідники (таблична частина): " + directoryItem.Name + "." + directoryTablePart.Name + "." + tablePartField.Name);
					}
				}
			}

			//Перевірка документів
			foreach (ConfigurationDocuments documentItem in Documents.Values)
			{
				//Поля довідника
				foreach (ConfigurationObjectField documentField in documentItem.Fields.Values)
				{
					if (documentField.Type == "pointer" && documentField.Pointer == searchName)
						ListPointer.Add("Документи: " + documentItem.Name + "." + documentField.Name);
				}

				//Табличні частини
				foreach (ConfigurationObjectTablePart documentTablePart in documentItem.TabularParts.Values)
				{
					//Поля табличної частини
					foreach (ConfigurationObjectField tablePartField in documentTablePart.Fields.Values)
					{
						if (tablePartField.Type == "pointer" && tablePartField.Pointer == searchName)
							ListPointer.Add("Документи (таблична частина): " + documentItem.Name + "." + documentTablePart.Name + "." + tablePartField.Name);
					}
				}
			}

			//Перевірка регістра RegistersInformation
			foreach (ConfigurationRegistersInformation registersInformationItem in RegistersInformation.Values)
			{
				//Поля
				foreach (ConfigurationObjectField registersField in registersInformationItem.DimensionFields.Values)
				{
					if (registersField.Type == "pointer" && registersField.Pointer == searchName)
						ListPointer.Add("Регістри відомостей: " + registersInformationItem.Name + "." + registersField.Name);
				}

				foreach (ConfigurationObjectField registersField in registersInformationItem.ResourcesFields.Values)
				{
					if (registersField.Type == "pointer" && registersField.Pointer == searchName)
						ListPointer.Add("Регістри відомостей: " + registersInformationItem.Name + "." + registersField.Name);
				}

				foreach (ConfigurationObjectField registersField in registersInformationItem.PropertyFields.Values)
				{
					if (registersField.Type == "pointer" && registersField.Pointer == searchName)
						ListPointer.Add("Регістри відомостей: " + registersInformationItem.Name + "." + registersField.Name);
				}
			}

			//Перевірка регістра RegistersAccumulation
			foreach (ConfigurationRegistersAccumulation registersAccumulationItem in RegistersAccumulation.Values)
			{
				//Поля
				foreach (ConfigurationObjectField registersField in registersAccumulationItem.DimensionFields.Values)
				{
					if (registersField.Type == "pointer" && registersField.Pointer == searchName)
						ListPointer.Add("Регістри накопичення: " + registersAccumulationItem.Name + "." + registersField.Name);
				}

				foreach (ConfigurationObjectField registersField in registersAccumulationItem.ResourcesFields.Values)
				{
					if (registersField.Type == "pointer" && registersField.Pointer == searchName)
						ListPointer.Add("Регістри накопичення: " + registersAccumulationItem.Name + "." + registersField.Name);
				}

				foreach (ConfigurationObjectField registersField in registersAccumulationItem.PropertyFields.Values)
				{
					if (registersField.Type == "pointer" && registersField.Pointer == searchName)
						ListPointer.Add("Регістри накопичення: " + registersAccumulationItem.Name + "." + registersField.Name);
				}
			}

			return ListPointer;
		}

		/// <summary>
		/// Пошук ссилок на перелічення
		/// </summary>
		/// <param name="searchName">Назва перелічення</param>
		/// <returns>Повертає список довідників або документів які вказують на searchName</returns>
		public List<string> SearchForPointersEnum(string searchName)
		{
			if (searchName.IndexOf(".") > 0)
			{
				string[] searchNameSplit = searchName.Split(new string[] { "." }, StringSplitOptions.None);

				if (!(searchNameSplit[0] == "Перелічення"))
					throw new Exception("Перша частина назви має бути 'Перелічення'");
			}
			else
				throw new Exception("Назва для пошуку має бути 'Перелічення.<Назва перелічення>'");

			List<string> ListPointer = new List<string>();

			//Перевірити константи
			foreach (ConfigurationConstantsBlock constantsBlockItem in ConstantsBlock.Values)
			{
				foreach (ConfigurationConstants constantsItem in constantsBlockItem.Constants.Values)
				{
					if (constantsItem.Type == "enum" && constantsItem.Pointer == searchName)
						ListPointer.Add("Константа (Блок " + constantsBlockItem.BlockName + "): " + constantsItem.Name + "." + constantsItem.Name);
				}
			}

			//Перевірити поля довідників та поля табличних частин чи часом вони не ссилаються на перелічення
			foreach (ConfigurationDirectories directoryItem in Directories.Values)
			{
				//Поля довідника
				foreach (ConfigurationObjectField directoryField in directoryItem.Fields.Values)
				{
					if (directoryField.Type == "enum" && directoryField.Pointer == searchName)
						ListPointer.Add("Довідники: " + directoryItem.Name + "." + directoryField.Name);
				}

				//Табличні частини
				foreach (ConfigurationObjectTablePart directoryTablePart in directoryItem.TabularParts.Values)
				{
					//Поля табличної частини
					foreach (ConfigurationObjectField tablePartField in directoryTablePart.Fields.Values)
					{
						if (tablePartField.Type == "enum" && tablePartField.Pointer == searchName)
							ListPointer.Add("Довідники (таблична частина): " + directoryItem.Name + "." + directoryTablePart.Name + "." + tablePartField.Name);
					}
				}
			}

			//Перевірка документів
			foreach (ConfigurationDocuments documentItem in Documents.Values)
			{
				//Поля довідника
				foreach (ConfigurationObjectField documentField in documentItem.Fields.Values)
				{
					if (documentField.Type == "enum" && documentField.Pointer == searchName)
						ListPointer.Add("Документи: " + documentItem.Name + "." + documentField.Name);
				}

				//Табличні частини
				foreach (ConfigurationObjectTablePart documentTablePart in documentItem.TabularParts.Values)
				{
					//Поля табличної частини
					foreach (ConfigurationObjectField tablePartField in documentTablePart.Fields.Values)
					{
						if (tablePartField.Type == "enum" && tablePartField.Pointer == searchName)
							ListPointer.Add("Документи (таблична частина): " + documentItem.Name + "." + documentTablePart.Name + "." + tablePartField.Name);
					}
				}
			}

			//Перевірка регістра RegistersInformation
			foreach (ConfigurationRegistersInformation registersInformationItem in RegistersInformation.Values)
			{
				//Поля
				foreach (ConfigurationObjectField registersField in registersInformationItem.DimensionFields.Values)
				{
					if (registersField.Type == "enum" && registersField.Pointer == searchName)
						ListPointer.Add("Регістри відомостей: " + registersInformationItem.Name + "." + registersField.Name);
				}

				foreach (ConfigurationObjectField registersField in registersInformationItem.ResourcesFields.Values)
				{
					if (registersField.Type == "enum" && registersField.Pointer == searchName)
						ListPointer.Add("Регістри відомостей: " + registersInformationItem.Name + "." + registersField.Name);
				}

				foreach (ConfigurationObjectField registersField in registersInformationItem.PropertyFields.Values)
				{
					if (registersField.Type == "enum" && registersField.Pointer == searchName)
						ListPointer.Add("Регістри відомостей: " + registersInformationItem.Name + "." + registersField.Name);
				}
			}

			//Перевірка регістра RegistersAccumulation
			foreach (ConfigurationRegistersAccumulation registersAccumulationItem in RegistersAccumulation.Values)
			{
				//Поля
				foreach (ConfigurationObjectField registersField in registersAccumulationItem.DimensionFields.Values)
				{
					if (registersField.Type == "enum" && registersField.Pointer == searchName)
						ListPointer.Add("Регістри накопичення: " + registersAccumulationItem.Name + "." + registersField.Name);
				}

				foreach (ConfigurationObjectField registersField in registersAccumulationItem.ResourcesFields.Values)
				{
					if (registersField.Type == "enum" && registersField.Pointer == searchName)
						ListPointer.Add("Регістри накопичення: " + registersAccumulationItem.Name + "." + registersField.Name);
				}

				foreach (ConfigurationObjectField registersField in registersAccumulationItem.PropertyFields.Values)
				{
					if (registersField.Type == "enum" && registersField.Pointer == searchName)
						ListPointer.Add("Регістри накопичення: " + registersAccumulationItem.Name + "." + registersField.Name);
				}
			}

			return ListPointer;
		}

		/// <summary>
		/// Повертає унікальну назву стовпця для вказаної таблиці
		/// </summary>
		/// <param name="Kernel">Ядро</param>
		/// <param name="table">Назва таблиці для якої генерується нова назва стовпця</param>
		/// <param name="Fields">Список існуючих полів</param>
		/// <returns>Повертає унікальну назву стовпця</returns>
		public static string GetNewUnigueColumnName(Kernel Kernel, string table, Dictionary<string, ConfigurationObjectField> Fields)
		{
			string[] englishAlphabet = GetEnglishAlphabet();

			bool noExistInReserved = false;
			bool noExistInConf = false;
			string columnNewName = "";

			if (String.IsNullOrWhiteSpace(table))
			{
				table = "0";
			}

			if (!Kernel.Conf.ReservedUnigueColumnName.ContainsKey(table))
				Kernel.Conf.ReservedUnigueColumnName.Add(table, new List<string>());

			for (int j = 0; j < englishAlphabet.Length; j++)
			{
				for (int i = 1; i < 10; i++)
				{
					columnNewName = "col_" + englishAlphabet[j] + i.ToString();

					if (!Kernel.Conf.ReservedUnigueColumnName[table].Contains(columnNewName))
					{
						noExistInReserved = true;
					}
					else
						continue;

					noExistInConf = true;

					foreach (ConfigurationObjectField configurationObjectField in Fields.Values)
					{
						//Console.WriteLine($"{configurationObjectField.NameInTable} = {columnNewName}");
						if (configurationObjectField.NameInTable == columnNewName)
						{
							noExistInConf = false;
							break;
						}
					}

					if (noExistInReserved && noExistInConf)
					{
						break;
					}
				}

				if (noExistInReserved && noExistInConf)
				{
					break;
				}
			}

			Kernel.Conf.ReservedUnigueColumnName[table].Add(columnNewName);

			return columnNewName;
		}

		/// <summary>
		/// Повертає унікальну назву таблиці
		/// </summary>
		/// <param name="Kernel">Ядро</param>
		/// <returns>Повертає унікальну назву таблиці</returns>
		public static string GetNewUnigueTableName(Kernel Kernel)
		{
			string[] englishAlphabet = GetEnglishAlphabet();

			bool noExistInReserved = false;
			bool noExistInBase = false;
			bool noExistInConf = false;
			string tabNewName = "";

			for (int j = 0; j < englishAlphabet.Length; j++)
			{
				for (int i = 1; i < 100; i++)
				{
					tabNewName = "tab_" + englishAlphabet[j] + (i < 10 ? "0" : "") + i.ToString();

					if (!Kernel.Conf.ReservedUnigueTableName.Contains(tabNewName))
					{
						noExistInReserved = true;
					}
					else
						continue;

					if (!Kernel.DataBase.IfExistsTable(tabNewName))
					{
						noExistInBase = true;
					}
					else
						continue;

					noExistInConf = true;

					foreach (ConfigurationConstantsBlock block in Kernel.Conf.ConstantsBlock.Values)
					{
						foreach (ConfigurationConstants constantsItem in block.Constants.Values)
						{
							foreach (ConfigurationObjectTablePart constantsTablePart in constantsItem.TabularParts.Values)
							{
								if (constantsTablePart.Table == tabNewName)
								{
									noExistInConf = false;
									break;
								}
							}

							if (!noExistInConf)
							{
								break;
							}
						}

						if (!noExistInConf)
						{
							break;
						}
					}

					if (noExistInConf)
						foreach (ConfigurationDirectories directoryItem in Kernel.Conf.Directories.Values)
						{
							if (directoryItem.Table == tabNewName)
							{
								noExistInConf = false;
								break;
							}

							foreach (ConfigurationObjectTablePart directoryTablePart in directoryItem.TabularParts.Values)
							{
								if (directoryTablePart.Table == tabNewName)
								{
									noExistInConf = false;
									break;
								}
							}

							if (!noExistInConf)
							{
								break;
							}
						}

					if (noExistInConf)
						foreach (ConfigurationDocuments documentItem in Kernel.Conf.Documents.Values)
						{
							if (documentItem.Table == tabNewName)
							{
								noExistInConf = false;
								break;
							}

							foreach (ConfigurationObjectTablePart documentTablePart in documentItem.TabularParts.Values)
							{
								if (documentTablePart.Table == tabNewName)
								{
									noExistInConf = false;
									break;
								}
							}

							if (!noExistInConf)
							{
								break;
							}
						}

					if (noExistInConf)
						foreach (ConfigurationRegistersInformation registersInformation in Kernel.Conf.RegistersInformation.Values)
						{
							if (registersInformation.Table == tabNewName)
							{
								noExistInConf = false;
								break;
							}
						}

					if (noExistInConf)
						foreach (ConfigurationRegistersAccumulation registersAccumulation in Kernel.Conf.RegistersAccumulation.Values)
						{
							if (registersAccumulation.Table == tabNewName)
							{
								noExistInConf = false;
								break;
							}
						}

					if (noExistInReserved && noExistInBase && noExistInConf)
					{
						break;
					}
				}

				if (noExistInReserved && noExistInBase && noExistInConf)
				{
					break;
				}
			}

			Kernel.Conf.ReservedUnigueTableName.Add(tabNewName);

			return tabNewName;
		}

		/// <summary>
		/// Перевіряє назву обєкту конфігурації (довідники, документи, поля) на валідність
		/// </summary>
		/// <param name="Kernel">Ядро</param>
		/// <param name="configurationObjectName">Назва обєкту конфігурації</param>
		/// <returns>Повертає інформацію про помилки у вигляді стрічки</returns>
		public static string ValidateConfigurationObjectName(Kernel Kernel, ref string configurationObjectName)
		{
			string errorList = "";

			configurationObjectName = configurationObjectName.Trim();

			if (String.IsNullOrWhiteSpace(configurationObjectName))
			{
				errorList += "Назва не задана";
				return errorList;
			}

			string allovChar = "abcdefghijklnmopqrstuvwxyz_";
			string allovNum = "0123456789";
			string allovCharCyrillic = "абвгґдеєжзиіїйклмнопрстуфхцчшщьюя"; //"АБВГДЕЄЖЗИІЇКЛМНОПРСТУФХЦЧШЩЪЫЭЮЯЬ";
			string allovAll = allovChar + allovNum + allovCharCyrillic;

			string configurationObjectModificeName = "";

			for (int i = 0; i < configurationObjectName.Length; i++)
			{
				string checkChar = configurationObjectName.Substring(i, 1);
				string checkCharLover = checkChar.ToLower();

				if (allovAll.IndexOf(checkCharLover) >= 0)
				{
					if (i == 0 && allovNum.IndexOf(checkCharLover) >= 0)
					{
						errorList += "Назва має починатися з букви\n";
					}
				}
				else
				{
					errorList += "Недопустимий символ (" + i.ToString() + "): " + "[" + checkChar + "]\n";
				}

				configurationObjectModificeName += checkChar;
			}

			configurationObjectName = configurationObjectModificeName;
			return errorList;
		}

		/// <summary>
		/// Функція обчислює для регістрів документи які роблять по ньому рухи.
		/// </summary>
		public void CalculateAllowDocumentSpendForRegistersAccumulation()
		{
			//Очистити список доступних документів для всіх регістрів
			foreach (ConfigurationRegistersAccumulation registersAccumulationItem in RegistersAccumulation.Values)
				registersAccumulationItem.AllowDocumentSpend.Clear();

			//Документи
			foreach (ConfigurationDocuments documentItem in Documents.Values)
			{
                foreach (string register in documentItem.AllowRegisterAccumulation)
                {
					ConfigurationRegistersAccumulation confRegAccum = RegistersAccumulation[register];
					if (!confRegAccum.AllowDocumentSpend.Contains(documentItem.Name))
						confRegAccum.AllowDocumentSpend.Add(documentItem.Name);
                }
			}
		}

		#endregion

		#region Load (завантаження конфігурації з ХМЛ файлу)

		/// <summary>
		/// Завантаження конфігурації
		/// </summary>
		/// <param name="pathToConf">Шлях до файлу конфігурації</param>
		/// <param name="Conf">Конфігурація</param>
		public static void Load(string pathToConf, out Configuration Conf)
		{
			Conf = new Configuration();

			//??
			if (!File.Exists(pathToConf))
			{
				Configuration EmptyConf = new Configuration();
				EmptyConf.Name = "Нова конфігурація";
				EmptyConf.NameSpace = "НоваКонфігурація_1_0";

				Save(pathToConf, EmptyConf);
			}

			XPathDocument xPathDoc = new XPathDocument(pathToConf);
			XPathNavigator xPathDocNavigator = xPathDoc.CreateNavigator();

			LoadConfigurationInfo(Conf, xPathDocNavigator);

			LoadConstants(Conf, xPathDocNavigator);

			LoadDirectories(Conf, xPathDocNavigator);

			LoadEnums(Conf, xPathDocNavigator);

			LoadDocuments(Conf, xPathDocNavigator);

			LoadRegistersInformation(Conf, xPathDocNavigator);

			LoadRegistersAccumulation(Conf, xPathDocNavigator);

			//Перерахувати документи які роблять рухи по регістрах накопичення
			//Conf.CalculateAllowDocumentSpendForRegistersAccumulation();
		}

		private static void LoadConfigurationInfo(Configuration Conf, XPathNavigator xPathDocNavigator)
		{
			XPathNavigator rootNodeConfiguration = xPathDocNavigator.SelectSingleNode("/Configuration");

			string name = rootNodeConfiguration.SelectSingleNode("Name").Value;
			Conf.Name = name;

			string nameSpace = rootNodeConfiguration.SelectSingleNode("NameSpace").Value;
			Conf.NameSpace = nameSpace;

			string author = rootNodeConfiguration.SelectSingleNode("Author").Value;
			Conf.Author = author;

            string desc = rootNodeConfiguration.SelectSingleNode("Desc").Value;
            Conf.Desc = desc;
        }

		private static void LoadConstants(Configuration Conf, XPathNavigator xPathDocNavigator)
		{
			XPathNodeIterator constantsBlockNodes = xPathDocNavigator.Select("/Configuration/ConstantsBlocks/ConstantsBlock");
			while (constantsBlockNodes.MoveNext())
			{
				string blockName = constantsBlockNodes.Current.SelectSingleNode("Name").Value;
				string blockDesc = constantsBlockNodes.Current.SelectSingleNode("Desc").Value;

				ConfigurationConstantsBlock configurationConstantsBlock = new ConfigurationConstantsBlock(blockName, blockDesc);
				Conf.ConstantsBlock.Add(configurationConstantsBlock.BlockName, configurationConstantsBlock);

				XPathNodeIterator constantsNodes = constantsBlockNodes.Current.Select("Constants/Constant");
				while (constantsNodes.MoveNext())
				{
					string constName = constantsNodes.Current.SelectSingleNode("Name").Value;
					string nameInTable = constantsNodes.Current.SelectSingleNode("NameInTable").Value;
					string constType = constantsNodes.Current.SelectSingleNode("Type").Value;
					string constDesc = constantsNodes.Current.SelectSingleNode("Desc").Value;

					string constPointer = "";
					if (constType == "pointer" || constType == "enum")
						constPointer = constantsNodes.Current.SelectSingleNode("Pointer").Value;

					ConfigurationConstants configurationConstants = new ConfigurationConstants(constName, nameInTable, constType, configurationConstantsBlock, constPointer, constDesc);
					configurationConstantsBlock.Constants.Add(configurationConstants.Name, configurationConstants);

					LoadTabularParts(configurationConstants.TabularParts, constantsNodes.Current);
				}
			}
		}

		private static void LoadDirectories(Configuration Conf, XPathNavigator xPathDocNavigator)
		{
			//Довідники
			XPathNodeIterator directoryNodes = xPathDocNavigator.Select("/Configuration/Directories/Directory");
			while (directoryNodes.MoveNext())
			{
				string name = directoryNodes.Current.SelectSingleNode("Name").Value;
				string table = directoryNodes.Current.SelectSingleNode("Table").Value;
				string desc = directoryNodes.Current.SelectSingleNode("Desc").Value;

				ConfigurationDirectories ConfObjectDirectories = new ConfigurationDirectories(name, table, desc);
				Conf.Directories.Add(ConfObjectDirectories.Name, ConfObjectDirectories);

				LoadFields(ConfObjectDirectories.Fields, directoryNodes.Current, "Directory");

				LoadTabularParts(ConfObjectDirectories.TabularParts, directoryNodes.Current);

				//LoadViews(ConfObjectDirectories.Views, directoryNodes.Current);

				LoadTriggerFunctions(ConfObjectDirectories.TriggerFunctions, directoryNodes.Current);
			}
		}

		private static void LoadFields(Dictionary<string, ConfigurationObjectField> fields, XPathNavigator xPathDocNavigator, string parentName)
		{
			XPathNodeIterator fieldNodes = xPathDocNavigator.Select("Fields/Field");
			while (fieldNodes.MoveNext())
			{
				string name = fieldNodes.Current.SelectSingleNode("Name").Value;
				string nameInTable = fieldNodes.Current.SelectSingleNode("NameInTable").Value;
				string type = fieldNodes.Current.SelectSingleNode("Type").Value;
				string desc = fieldNodes.Current.SelectSingleNode("Desc").Value;

				bool isPresentation = false;
				if (parentName == "Directory" || parentName == "Document")
				{
					string isPresentationString = fieldNodes.Current.SelectSingleNode("IsPresentation").Value;
					isPresentation = isPresentationString == "1";
				}

				string isIndexString = fieldNodes.Current.SelectSingleNode("IsIndex").Value;
				bool isIndex = isIndexString == "1";

				string pointer = "";
				if (type == "pointer" || type == "enum")
				{
					pointer = fieldNodes.Current.SelectSingleNode("Pointer").Value;
				}

				ConfigurationObjectField ConfObjectField = new ConfigurationObjectField(name, nameInTable, type, pointer, desc, isPresentation, isIndex);

				fields.Add(name, ConfObjectField);
			}
		}

		private static void LoadTabularParts(Dictionary<string, ConfigurationObjectTablePart> tabularParts, XPathNavigator xPathDocNavigator)
		{
			XPathNodeIterator tablePartNodes = xPathDocNavigator.Select("TabularParts/TablePart");
			while (tablePartNodes.MoveNext())
			{
				string name = tablePartNodes.Current.SelectSingleNode("Name").Value;
				string table = tablePartNodes.Current.SelectSingleNode("Table").Value;
				string desc = tablePartNodes.Current.SelectSingleNode("Desc").Value;

				ConfigurationObjectTablePart ConfObjectTablePart = new ConfigurationObjectTablePart(name, table, desc);

				tabularParts.Add(ConfObjectTablePart.Name, ConfObjectTablePart);

				LoadFields(ConfObjectTablePart.Fields, tablePartNodes.Current, "TablePart");
			}
		}

		private static void LoadAllowRegisterAccumulation(List<string> allowRegisterAccumulation, XPathNavigator xPathDocNavigator)
        {
			XPathNodeIterator allowRegisterAccumulationNodes = xPathDocNavigator.Select("AllowRegisterAccumulation/Name");
			while (allowRegisterAccumulationNodes.MoveNext())
			{
				string name = allowRegisterAccumulationNodes.Current.Value;
				allowRegisterAccumulation.Add(name);
			}
		}

		private static void LoadTriggerFunctions(ConfigurationTriggerFunctions triggerFunctions, XPathNavigator xPathDocNavigator)
		{
			XPathNavigator nodeTriggerFunctions = xPathDocNavigator.SelectSingleNode("TriggerFunctions");

			XPathNavigator nodeBeforeSave = nodeTriggerFunctions.SelectSingleNode("BeforeSave");
			triggerFunctions.BeforeSave = nodeBeforeSave.Value;

			XPathNavigator nodeAfterSave = nodeTriggerFunctions.SelectSingleNode("AfterSave");
			triggerFunctions.AfterSave = nodeAfterSave.Value;

			XPathNavigator nodeBeforeDelete = nodeTriggerFunctions.SelectSingleNode("BeforeDelete");
			triggerFunctions.BeforeDelete = nodeBeforeDelete.Value;
		}

		private static void LoadSpendFunctions(ConfigurationSpendFunctions spendFunctions, XPathNavigator xPathDocNavigator)
		{
			XPathNavigator nodeSpendFunctions = xPathDocNavigator.SelectSingleNode("SpendFunctions");

			XPathNavigator nodeSpend = nodeSpendFunctions.SelectSingleNode("Spend");
			spendFunctions.Spend = nodeSpend.Value;

			XPathNavigator nodeClearSpend = nodeSpendFunctions.SelectSingleNode("ClearSpend");
			spendFunctions.ClearSpend = nodeClearSpend.Value;
		}

		public static void LoadEnums(Configuration Conf, XPathNavigator xPathDocNavigator)
		{
			//Перелічення
			XPathNodeIterator enumsNodes = xPathDocNavigator.Select("/Configuration/Enums/Enum");
			while (enumsNodes.MoveNext())
			{
				string name = enumsNodes.Current.SelectSingleNode("Name").Value;
				string desc = enumsNodes.Current.SelectSingleNode("Desc").Value;
				int serialNumber = int.Parse(enumsNodes.Current.SelectSingleNode("SerialNumber").Value);

				if (String.IsNullOrWhiteSpace(desc)) desc = "";

				ConfigurationEnums configurationEnums = new ConfigurationEnums(name, serialNumber, desc);
				Conf.Enums.Add(configurationEnums.Name, configurationEnums);

				XPathNodeIterator enumFieldsNodes = enumsNodes.Current.Select("Fields/Field");
				while (enumFieldsNodes.MoveNext())
				{
					string nameField = enumFieldsNodes.Current.SelectSingleNode("Name").Value;
					string valueField = enumFieldsNodes.Current.SelectSingleNode("Value").Value;
					string descField = enumFieldsNodes.Current.SelectSingleNode("Desc").Value;

					configurationEnums.AppendField(new ConfigurationEnumField(nameField, int.Parse(valueField), descField));
				}
			}
		}

		private static void LoadDocuments(Configuration Conf, XPathNavigator xPathDocNavigator)
		{
			//Документи
			XPathNodeIterator documentsNode = xPathDocNavigator.Select("/Configuration/Documents/Document");
			while (documentsNode.MoveNext())
			{
				string name = documentsNode.Current.SelectSingleNode("Name").Value;
				string table = documentsNode.Current.SelectSingleNode("Table").Value;
				string desc = documentsNode.Current.SelectSingleNode("Desc").Value;

				ConfigurationDocuments configurationDocuments = new ConfigurationDocuments(name, table, desc);
				Conf.Documents.Add(configurationDocuments.Name, configurationDocuments);

				LoadFields(configurationDocuments.Fields, documentsNode.Current, "Document");

				LoadTabularParts(configurationDocuments.TabularParts, documentsNode.Current);

				LoadAllowRegisterAccumulation(configurationDocuments.AllowRegisterAccumulation, documentsNode.Current);

				LoadTriggerFunctions(configurationDocuments.TriggerFunctions, documentsNode.Current);

				LoadSpendFunctions(configurationDocuments.SpendFunctions, documentsNode.Current);
			}
		}

		private static void LoadRegistersInformation(Configuration Conf, XPathNavigator xPathDocNavigator)
		{
			//Регістри відомостей
			XPathNodeIterator registerInformationNode = xPathDocNavigator.Select("/Configuration/RegistersInformation/RegisterInformation");
			while (registerInformationNode.MoveNext())
			{
				string name = registerInformationNode.Current.SelectSingleNode("Name").Value;
				string table = registerInformationNode.Current.SelectSingleNode("Table").Value;
				string desc = registerInformationNode.Current.SelectSingleNode("Desc").Value;

				ConfigurationRegistersInformation configurationRegistersInformation = new ConfigurationRegistersInformation(name, table, desc);
				Conf.RegistersInformation.Add(configurationRegistersInformation.Name, configurationRegistersInformation);

				XPathNavigator dimensionFieldsNode = registerInformationNode.Current.SelectSingleNode("DimensionFields");

				if (dimensionFieldsNode != null)
					LoadFields(configurationRegistersInformation.DimensionFields, dimensionFieldsNode, "RegisterInformation");

				XPathNavigator resourcesFieldsNode = registerInformationNode.Current.SelectSingleNode("ResourcesFields");

				if (resourcesFieldsNode != null)
					LoadFields(configurationRegistersInformation.ResourcesFields, resourcesFieldsNode, "RegisterInformation");

				XPathNavigator propertyFieldsNode = registerInformationNode.Current.SelectSingleNode("PropertyFields");

				if (propertyFieldsNode != null)
					LoadFields(configurationRegistersInformation.PropertyFields, propertyFieldsNode, "RegisterInformation");
			}
		}

		private static void LoadAllowDocumentSpendRegisterAccumulation(List<string> allowDocumentSpend, XPathNavigator xPathDocNavigator)
		{
			XPathNodeIterator allowDocumentSpendNodes = xPathDocNavigator.Select("AllowDocumentSpend/Name");
			while (allowDocumentSpendNodes.MoveNext())
			{
				string name = allowDocumentSpendNodes.Current.Value;
				allowDocumentSpend.Add(name);
			}
		}

		private static void LoadRegistersAccumulation(Configuration Conf, XPathNavigator xPathDocNavigator)
		{
			//Регістри накопичення
			XPathNodeIterator registerAccumulationNode = xPathDocNavigator.Select("/Configuration/RegistersAccumulation/RegisterAccumulation");
			while (registerAccumulationNode.MoveNext())
			{
				string name = registerAccumulationNode.Current.SelectSingleNode("Name").Value;
				string table = registerAccumulationNode.Current.SelectSingleNode("Table").Value;
				string type = registerAccumulationNode.Current.SelectSingleNode("Type").Value;
				string desc = registerAccumulationNode.Current.SelectSingleNode("Desc").Value;

				TypeRegistersAccumulation typeRegistersAccumulation;
				if (type == "Residues")
					typeRegistersAccumulation = TypeRegistersAccumulation.Residues;
				else if (type == "Turnover")
					typeRegistersAccumulation = TypeRegistersAccumulation.Turnover;
				else
					throw new Exception("Не оприділений тип регістру");

				ConfigurationRegistersAccumulation configurationRegistersAccumulation =
					new ConfigurationRegistersAccumulation(name, table, typeRegistersAccumulation, desc);

				Conf.RegistersAccumulation.Add(configurationRegistersAccumulation.Name, configurationRegistersAccumulation);

				XPathNavigator dimensionFieldsNode = registerAccumulationNode.Current.SelectSingleNode("DimensionFields");

				if (dimensionFieldsNode != null)
					LoadFields(configurationRegistersAccumulation.DimensionFields, dimensionFieldsNode, "RegisterAccumulation");

				XPathNavigator resourcesFieldsNode = registerAccumulationNode.Current.SelectSingleNode("ResourcesFields");

				if (resourcesFieldsNode != null)
					LoadFields(configurationRegistersAccumulation.ResourcesFields, resourcesFieldsNode, "RegisterAccumulation");

				XPathNavigator propertyFieldsNode = registerAccumulationNode.Current.SelectSingleNode("PropertyFields");

				if (propertyFieldsNode != null)
					LoadFields(configurationRegistersAccumulation.PropertyFields, propertyFieldsNode, "RegisterAccumulation");

				LoadAllowDocumentSpendRegisterAccumulation(configurationRegistersAccumulation.AllowDocumentSpend, registerAccumulationNode.Current);
			}
		}

		#endregion

		#region Save (Збереження конфігурації в ХМЛ файл)

		/// <summary>
		/// Збереження конфігурації в файл
		/// </summary>
		/// <param name="pathToConf">Шлях до ХМЛ файлу конфігурації</param>
		/// <param name="Conf">Конфігурація</param>
		public static void Save(string pathToConf, Configuration Conf)
		{
			//Перерахувати документи які роблять рухи по регістрах накопичення
			Conf.CalculateAllowDocumentSpendForRegistersAccumulation();

			XmlDocument xmlConfDocument = new XmlDocument();
			xmlConfDocument.AppendChild(xmlConfDocument.CreateXmlDeclaration("1.0", "utf-8", ""));

			XmlElement rootNode = xmlConfDocument.CreateElement("Configuration");
			xmlConfDocument.AppendChild(rootNode);

			SaveConfigurationInfo(Conf, xmlConfDocument, rootNode);

			SaveConstantsBlock(Conf.ConstantsBlock, xmlConfDocument, rootNode);

			SaveDirectories(Conf.Directories, xmlConfDocument, rootNode);

			SaveEnums(Conf.Enums, xmlConfDocument, rootNode);

			SaveDocuments(Conf.Documents, xmlConfDocument, rootNode);

			SaveRegistersInformation(Conf.RegistersInformation, xmlConfDocument, rootNode);

			SaveRegistersAccumulation(Conf.RegistersAccumulation, xmlConfDocument, rootNode);

			xmlConfDocument.Save(pathToConf);
		}

		private static void SaveConfigurationInfo(Configuration Conf, XmlDocument xmlConfDocument, XmlElement rootNode)
		{
			XmlElement nodeName = xmlConfDocument.CreateElement("Name");
			nodeName.InnerText = Conf.Name;
			rootNode.AppendChild(nodeName);

			XmlElement nodeNameSpace = xmlConfDocument.CreateElement("NameSpace");
			nodeNameSpace.InnerText = Conf.NameSpace;
			rootNode.AppendChild(nodeNameSpace);

			XmlElement nodeAuthor = xmlConfDocument.CreateElement("Author");
			nodeAuthor.InnerText = Conf.Author;
			rootNode.AppendChild(nodeAuthor);

			XmlElement nodeDesc = xmlConfDocument.CreateElement("Desc");
			nodeDesc.InnerText = Conf.Desc;
			rootNode.AppendChild(nodeDesc);

			XmlElement nodeDateTimeSave = xmlConfDocument.CreateElement("DateTimeSave");
			nodeDateTimeSave.InnerText = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
			rootNode.AppendChild(nodeDateTimeSave);
		}

		private static void SaveConstantsBlock(Dictionary<string, ConfigurationConstantsBlock> ConfConstantsBlocks, XmlDocument xmlConfDocument, XmlElement rootNode)
		{
			XmlElement rootConstantsBlocks = xmlConfDocument.CreateElement("ConstantsBlocks");
			rootNode.AppendChild(rootConstantsBlocks);

			foreach (KeyValuePair<string, ConfigurationConstantsBlock> ConfConstantsBlock in ConfConstantsBlocks)
			{
				XmlElement rootConstantsBlock = xmlConfDocument.CreateElement("ConstantsBlock");
				rootConstantsBlocks.AppendChild(rootConstantsBlock);

				XmlElement nodeName = xmlConfDocument.CreateElement("Name");
				nodeName.InnerText = ConfConstantsBlock.Key;
				rootConstantsBlock.AppendChild(nodeName);

				XmlElement nodeDesc = xmlConfDocument.CreateElement("Desc");
				nodeDesc.InnerText = ConfConstantsBlock.Value.Desc;
				rootConstantsBlock.AppendChild(nodeDesc);

				SaveConstants(ConfConstantsBlock.Value.Constants, xmlConfDocument, rootConstantsBlock);
			}
		}

		private static void SaveConstants(Dictionary<string, ConfigurationConstants> ConfConstants, XmlDocument xmlConfDocument, XmlElement rootNode)
		{
			XmlElement rootConstants = xmlConfDocument.CreateElement("Constants");
			rootNode.AppendChild(rootConstants);

			foreach (KeyValuePair<string, ConfigurationConstants> ConfConstant in ConfConstants)
			{
				XmlElement rootConstant = xmlConfDocument.CreateElement("Constant");
				rootConstants.AppendChild(rootConstant);

				XmlElement nodeName = xmlConfDocument.CreateElement("Name");
				nodeName.InnerText = ConfConstant.Key;
				rootConstant.AppendChild(nodeName);

				XmlElement nodeNameInTable = xmlConfDocument.CreateElement("NameInTable");
				nodeNameInTable.InnerText = ConfConstant.Value.NameInTable;
				rootConstant.AppendChild(nodeNameInTable);

				XmlElement nodeDesc = xmlConfDocument.CreateElement("Desc");
				nodeDesc.InnerText = ConfConstant.Value.Desc;
				rootConstant.AppendChild(nodeDesc);

				XmlElement nodeType = xmlConfDocument.CreateElement("Type");
				nodeType.InnerText = ConfConstant.Value.Type;
				rootConstant.AppendChild(nodeType);

				if (ConfConstant.Value.Type == "pointer" || ConfConstant.Value.Type == "enum")
				{
					XmlElement nodePointer = xmlConfDocument.CreateElement("Pointer");
					nodePointer.InnerText = ConfConstant.Value.Pointer;
					rootConstant.AppendChild(nodePointer);
				}

				SaveTabularParts(ConfConstant.Value.TabularParts, xmlConfDocument, rootConstant);
			}
		}

		private static void SaveDirectories(Dictionary<string, ConfigurationDirectories> ConfDirectories, XmlDocument xmlConfDocument, XmlElement rootNode)
		{
			XmlElement rootDirectories = xmlConfDocument.CreateElement("Directories");
			rootNode.AppendChild(rootDirectories);

			foreach (KeyValuePair<string, ConfigurationDirectories> ConfDirectory in ConfDirectories)
			{
				XmlElement nodeDirectory = xmlConfDocument.CreateElement("Directory");
				rootDirectories.AppendChild(nodeDirectory);

				XmlElement nodeDirectoryName = xmlConfDocument.CreateElement("Name");
				nodeDirectoryName.InnerText = ConfDirectory.Key;
				nodeDirectory.AppendChild(nodeDirectoryName);

				XmlElement nodeDirectoryTable = xmlConfDocument.CreateElement("Table");
				nodeDirectoryTable.InnerText = ConfDirectory.Value.Table;
				nodeDirectory.AppendChild(nodeDirectoryTable);

				XmlElement nodeDirectoryDesc = xmlConfDocument.CreateElement("Desc");
				nodeDirectoryDesc.InnerText = ConfDirectory.Value.Desc;
				nodeDirectory.AppendChild(nodeDirectoryDesc);

				SaveFields(ConfDirectory.Value.Fields, xmlConfDocument, nodeDirectory, "Directory");

				SaveTabularParts(ConfDirectory.Value.TabularParts, xmlConfDocument, nodeDirectory);

				//SaveViews(ConfDirectory.Value.Views, ConfDirectory.Value, xmlConfDocument, nodeDirectory);

				SaveTriggerFunctions(ConfDirectory.Value.TriggerFunctions, xmlConfDocument, nodeDirectory);
			}
		}

		private static void SaveFields(Dictionary<string, ConfigurationObjectField> fields, XmlDocument xmlConfDocument, XmlElement rootNode, string parentName)
		{
			XmlElement nodeFields = xmlConfDocument.CreateElement("Fields");
			rootNode.AppendChild(nodeFields);

			foreach (KeyValuePair<string, ConfigurationObjectField> field in fields)
			{
				XmlElement nodeField = xmlConfDocument.CreateElement("Field");
				nodeFields.AppendChild(nodeField);

				XmlElement nodeFieldName = xmlConfDocument.CreateElement("Name");
				nodeFieldName.InnerText = field.Key;
				nodeField.AppendChild(nodeFieldName);

				XmlElement nodeFieldNameInTable = xmlConfDocument.CreateElement("NameInTable");
				nodeFieldNameInTable.InnerText = field.Value.NameInTable;
				nodeField.AppendChild(nodeFieldNameInTable);

				XmlElement nodeFieldType = xmlConfDocument.CreateElement("Type");
				nodeFieldType.InnerText = field.Value.Type;
				nodeField.AppendChild(nodeFieldType);

				if (field.Value.Type == "pointer" || field.Value.Type == "enum")
				{
					XmlElement nodeFieldPointer = xmlConfDocument.CreateElement("Pointer");
					nodeFieldPointer.InnerText = field.Value.Pointer;
					nodeField.AppendChild(nodeFieldPointer);
				}

				XmlElement nodeFieldDesc = xmlConfDocument.CreateElement("Desc");
				nodeFieldDesc.InnerText = field.Value.Desc;
				nodeField.AppendChild(nodeFieldDesc);

				if (parentName == "Directory" || parentName == "Document")
				{
					XmlElement nodeFieldIsPresentation = xmlConfDocument.CreateElement("IsPresentation");
					nodeFieldIsPresentation.InnerText = field.Value.IsPresentation ? "1" : "0";
					nodeField.AppendChild(nodeFieldIsPresentation);
				}

				XmlElement nodeFieldIsIndex = xmlConfDocument.CreateElement("IsIndex");
				nodeFieldIsIndex.InnerText = field.Value.IsIndex ? "1" : "0";
				nodeField.AppendChild(nodeFieldIsIndex);
			}
		}

		private static void SaveTabularParts(Dictionary<string, ConfigurationObjectTablePart> tabularParts, XmlDocument xmlConfDocument, XmlElement rootNode)
		{
			XmlElement nodeTabularParts = xmlConfDocument.CreateElement("TabularParts");
			rootNode.AppendChild(nodeTabularParts);

			foreach (KeyValuePair<string, ConfigurationObjectTablePart> tablePart in tabularParts)
			{
				XmlElement nodeTablePart = xmlConfDocument.CreateElement("TablePart");
				nodeTabularParts.AppendChild(nodeTablePart);

				XmlElement nodeTablePartName = xmlConfDocument.CreateElement("Name");
				nodeTablePartName.InnerText = tablePart.Key;
				nodeTablePart.AppendChild(nodeTablePartName);

				XmlElement nodeTablePartTable = xmlConfDocument.CreateElement("Table");
				nodeTablePartTable.InnerText = tablePart.Value.Table;
				nodeTablePart.AppendChild(nodeTablePartTable);

				XmlElement nodeTablePartDesc = xmlConfDocument.CreateElement("Desc");
				nodeTablePartDesc.InnerText = tablePart.Value.Desc;
				nodeTablePart.AppendChild(nodeTablePartDesc);

				SaveFields(tablePart.Value.Fields, xmlConfDocument, nodeTablePart, "TablePart");
			}
		}

		private static void SaveAllowRegisterAccumulation(List<string> allowRegisterAccumulation, XmlDocument xmlConfDocument, XmlElement rootNode)
        {
			XmlElement nodeAllowRegisterAccumulation = xmlConfDocument.CreateElement("AllowRegisterAccumulation");
			rootNode.AppendChild(nodeAllowRegisterAccumulation);

			foreach (string name in allowRegisterAccumulation)
			{
				XmlElement nodeName = xmlConfDocument.CreateElement("Name");
				nodeName.InnerText = name;
				nodeAllowRegisterAccumulation.AppendChild(nodeName);
			}
		}

		private static void SaveTriggerFunctions(ConfigurationTriggerFunctions triggerFunctions, XmlDocument xmlConfDocument, XmlElement rootNode)
		{
			XmlElement nodeTriggerFunctions = xmlConfDocument.CreateElement("TriggerFunctions");
			rootNode.AppendChild(nodeTriggerFunctions);

			XmlElement nodeBeforeSave = xmlConfDocument.CreateElement("BeforeSave");
			nodeBeforeSave.InnerText = triggerFunctions.BeforeSave;
			nodeTriggerFunctions.AppendChild(nodeBeforeSave);

			XmlElement nodeAfterSave = xmlConfDocument.CreateElement("AfterSave");
			nodeAfterSave.InnerText = triggerFunctions.AfterSave;
			nodeTriggerFunctions.AppendChild(nodeAfterSave);

			XmlElement nodeBeforeDelete = xmlConfDocument.CreateElement("BeforeDelete");
			nodeBeforeDelete.InnerText = triggerFunctions.BeforeDelete;
			nodeTriggerFunctions.AppendChild(nodeBeforeDelete);
		}

		private static void SaveSpendFunctions(ConfigurationSpendFunctions spendFunctions, XmlDocument xmlConfDocument, XmlElement rootNode)
		{
			XmlElement nodeSpendFunctions = xmlConfDocument.CreateElement("SpendFunctions");
			rootNode.AppendChild(nodeSpendFunctions);

			XmlElement nodeSpend = xmlConfDocument.CreateElement("Spend");
			nodeSpend.InnerText = spendFunctions.Spend;
			nodeSpendFunctions.AppendChild(nodeSpend);

			XmlElement nodeClearSpend = xmlConfDocument.CreateElement("ClearSpend");
			nodeClearSpend.InnerText = spendFunctions.ClearSpend;
			nodeSpendFunctions.AppendChild(nodeClearSpend);
		}

		private static void SaveEnums(Dictionary<string, ConfigurationEnums> enums, XmlDocument xmlConfDocument, XmlElement rootNode)
		{
			XmlElement nodeEnums = xmlConfDocument.CreateElement("Enums");
			rootNode.AppendChild(nodeEnums);

			foreach (KeyValuePair<string, ConfigurationEnums> enum_item in enums)
			{
				XmlElement nodeEnum = xmlConfDocument.CreateElement("Enum");
				nodeEnums.AppendChild(nodeEnum);

				XmlElement nodeEnumName = xmlConfDocument.CreateElement("Name");
				nodeEnumName.InnerText = enum_item.Key;
				nodeEnum.AppendChild(nodeEnumName);

				XmlElement nodeEnumDesc = xmlConfDocument.CreateElement("Desc");
				nodeEnumDesc.InnerText = enum_item.Value.Desc;
				nodeEnum.AppendChild(nodeEnumDesc);

				XmlElement nodeEnumSerialNumber = xmlConfDocument.CreateElement("SerialNumber");
				nodeEnumSerialNumber.InnerText = enum_item.Value.SerialNumber.ToString();
				nodeEnum.AppendChild(nodeEnumSerialNumber);

				XmlElement nodeFields = xmlConfDocument.CreateElement("Fields");
				nodeEnum.AppendChild(nodeFields);

				foreach (KeyValuePair<string, ConfigurationEnumField> field in enum_item.Value.Fields)
				{
					XmlElement nodeField = xmlConfDocument.CreateElement("Field");
					nodeFields.AppendChild(nodeField);

					XmlElement nodeFieldName = xmlConfDocument.CreateElement("Name");
					nodeFieldName.InnerText = field.Value.Name;
					nodeField.AppendChild(nodeFieldName);

					XmlElement nodeFieldValue = xmlConfDocument.CreateElement("Value");
					nodeFieldValue.InnerText = field.Value.Value.ToString();
					nodeField.AppendChild(nodeFieldValue);

					XmlElement nodeFieldDesc = xmlConfDocument.CreateElement("Desc");
					nodeFieldDesc.InnerText = field.Value.Desc;
					nodeField.AppendChild(nodeFieldDesc);
				}
			}
		}

		private static void SaveDocuments(Dictionary<string, ConfigurationDocuments> ConfDocuments, XmlDocument xmlConfDocument, XmlElement rootNode)
		{
			XmlElement rootDocuments = xmlConfDocument.CreateElement("Documents");
			rootNode.AppendChild(rootDocuments);

			foreach (KeyValuePair<string, ConfigurationDocuments> ConfDocument in ConfDocuments)
			{
				XmlElement nodeDocument = xmlConfDocument.CreateElement("Document");
				rootDocuments.AppendChild(nodeDocument);

				XmlElement nodeDocumentName = xmlConfDocument.CreateElement("Name");
				nodeDocumentName.InnerText = ConfDocument.Key;
				nodeDocument.AppendChild(nodeDocumentName);

				XmlElement nodeDocumentTable = xmlConfDocument.CreateElement("Table");
				nodeDocumentTable.InnerText = ConfDocument.Value.Table;
				nodeDocument.AppendChild(nodeDocumentTable);

				XmlElement nodeDocumentDesc = xmlConfDocument.CreateElement("Desc");
				nodeDocumentDesc.InnerText = ConfDocument.Value.Desc;
				nodeDocument.AppendChild(nodeDocumentDesc);

				SaveFields(ConfDocument.Value.Fields, xmlConfDocument, nodeDocument, "Document");

				SaveTabularParts(ConfDocument.Value.TabularParts, xmlConfDocument, nodeDocument);

				SaveAllowRegisterAccumulation(ConfDocument.Value.AllowRegisterAccumulation, xmlConfDocument, nodeDocument);

				SaveTriggerFunctions(ConfDocument.Value.TriggerFunctions, xmlConfDocument, nodeDocument);

				SaveSpendFunctions(ConfDocument.Value.SpendFunctions, xmlConfDocument, nodeDocument);
			}
		}

		private static void SaveRegistersInformation(Dictionary<string, ConfigurationRegistersInformation> ConfRegistersInformation, XmlDocument xmlConfDocument, XmlElement rootNode)
		{
			XmlElement rootRegistersInformation = xmlConfDocument.CreateElement("RegistersInformation");
			rootNode.AppendChild(rootRegistersInformation);

			foreach (KeyValuePair<string, ConfigurationRegistersInformation> ConfRegisterInfo in ConfRegistersInformation)
			{
				XmlElement nodeRegister = xmlConfDocument.CreateElement("RegisterInformation");
				rootRegistersInformation.AppendChild(nodeRegister);

				XmlElement nodeRegisterName = xmlConfDocument.CreateElement("Name");
				nodeRegisterName.InnerText = ConfRegisterInfo.Key;
				nodeRegister.AppendChild(nodeRegisterName);

				XmlElement nodeRegisterTable = xmlConfDocument.CreateElement("Table");
				nodeRegisterTable.InnerText = ConfRegisterInfo.Value.Table;
				nodeRegister.AppendChild(nodeRegisterTable);

				XmlElement nodeRegisterDesc = xmlConfDocument.CreateElement("Desc");
				nodeRegisterDesc.InnerText = ConfRegisterInfo.Value.Desc;
				nodeRegister.AppendChild(nodeRegisterDesc);

				XmlElement nodeDimensionFields = xmlConfDocument.CreateElement("DimensionFields");
				nodeRegister.AppendChild(nodeDimensionFields);

				SaveFields(ConfRegisterInfo.Value.DimensionFields, xmlConfDocument, nodeDimensionFields, "RegisterInformation");

				XmlElement nodeResourcesFields = xmlConfDocument.CreateElement("ResourcesFields");
				nodeRegister.AppendChild(nodeResourcesFields);

				SaveFields(ConfRegisterInfo.Value.ResourcesFields, xmlConfDocument, nodeResourcesFields, "RegisterInformation");

				XmlElement nodePropertyFields = xmlConfDocument.CreateElement("PropertyFields");
				nodeRegister.AppendChild(nodePropertyFields);

				SaveFields(ConfRegisterInfo.Value.PropertyFields, xmlConfDocument, nodePropertyFields, "RegisterInformation");
			}
		}

		private static void SaveAllowDocumentSpendRegisterAccumulation(List<string> allowDocumentSpend, XmlDocument xmlConfDocument, XmlElement rootNode)
		{
			XmlElement nodeAllowDocumentSpend = xmlConfDocument.CreateElement("AllowDocumentSpend");
			rootNode.AppendChild(nodeAllowDocumentSpend);

			foreach (string name in allowDocumentSpend)
			{
				XmlElement nodeName = xmlConfDocument.CreateElement("Name");
				nodeName.InnerText = name;
				nodeAllowDocumentSpend.AppendChild(nodeName);
			}
		}

		private static void SaveRegistersAccumulation(Dictionary<string, ConfigurationRegistersAccumulation> ConfRegistersAccumulation, XmlDocument xmlConfDocument, XmlElement rootNode)
		{
			XmlElement rootRegistersAccumulation = xmlConfDocument.CreateElement("RegistersAccumulation");
			rootNode.AppendChild(rootRegistersAccumulation);

			foreach (KeyValuePair<string, ConfigurationRegistersAccumulation> ConfRegisterAccml in ConfRegistersAccumulation)
			{
				XmlElement nodeRegister = xmlConfDocument.CreateElement("RegisterAccumulation");
				rootRegistersAccumulation.AppendChild(nodeRegister);

				XmlElement nodeRegisterName = xmlConfDocument.CreateElement("Name");
				nodeRegisterName.InnerText = ConfRegisterAccml.Key;
				nodeRegister.AppendChild(nodeRegisterName);

				XmlElement nodeRegisterTable = xmlConfDocument.CreateElement("Table");
				nodeRegisterTable.InnerText = ConfRegisterAccml.Value.Table;
				nodeRegister.AppendChild(nodeRegisterTable);

				XmlElement nodeRegisterType = xmlConfDocument.CreateElement("Type");
				nodeRegisterType.InnerText = ConfRegisterAccml.Value.TypeRegistersAccumulation.ToString();
				nodeRegister.AppendChild(nodeRegisterType);

				XmlElement nodeRegisterDesc = xmlConfDocument.CreateElement("Desc");
				nodeRegisterDesc.InnerText = ConfRegisterAccml.Value.Desc;
				nodeRegister.AppendChild(nodeRegisterDesc);

				XmlElement nodeDimensionFields = xmlConfDocument.CreateElement("DimensionFields");
				nodeRegister.AppendChild(nodeDimensionFields);

				SaveFields(ConfRegisterAccml.Value.DimensionFields, xmlConfDocument, nodeDimensionFields, "RegisterAccumulation");

				XmlElement nodeResourcesFields = xmlConfDocument.CreateElement("ResourcesFields");
				nodeRegister.AppendChild(nodeResourcesFields);

				SaveFields(ConfRegisterAccml.Value.ResourcesFields, xmlConfDocument, nodeResourcesFields, "RegisterAccumulation");

				XmlElement nodePropertyFields = xmlConfDocument.CreateElement("PropertyFields");
				nodeRegister.AppendChild(nodePropertyFields);

				SaveFields(ConfRegisterAccml.Value.PropertyFields, xmlConfDocument, nodePropertyFields, "RegisterAccumulation");

				SaveAllowDocumentSpendRegisterAccumulation(ConfRegisterAccml.Value.AllowDocumentSpend, xmlConfDocument, nodeRegister);
			}
		}

		/// <summary>
		/// Зберігає інформацію про схему бази даних в ХМЛ файл
		/// Схема це реальна структура бази даних (таблиці, стовчики)
		/// </summary>
		/// <param name="InformationSchema">Схема</param>
		/// <param name="pathToSave">Шлях до файлу</param>
		public static void SaveInformationSchema(ConfigurationInformationSchema InformationSchema, string pathToSave)
		{
			XmlDocument xmlComparisonDocument = new XmlDocument();
			xmlComparisonDocument.AppendChild(xmlComparisonDocument.CreateXmlDeclaration("1.0", "utf-8", ""));

			XmlElement nodeInformationSchema = xmlComparisonDocument.CreateElement("InformationSchema");
			xmlComparisonDocument.AppendChild(nodeInformationSchema);

			//Таблиці
			foreach (KeyValuePair<string, ConfigurationInformationSchema_Table> informationSchemaTable in InformationSchema.Tables)
			{
				XmlElement nodeInformationSchemaTable = xmlComparisonDocument.CreateElement("Table");
				nodeInformationSchema.AppendChild(nodeInformationSchemaTable);

				XmlElement nodeInformationSchemaTableName = xmlComparisonDocument.CreateElement("Name");
				nodeInformationSchemaTableName.InnerText = informationSchemaTable.Value.TableName;
				nodeInformationSchemaTable.AppendChild(nodeInformationSchemaTableName);

				//Стовпчики
				foreach (KeyValuePair<string, ConfigurationInformationSchema_Column> informationSchemaColumn in informationSchemaTable.Value.Columns)
				{
					XmlElement nodeInformationSchemaColumn = xmlComparisonDocument.CreateElement("Column");
					nodeInformationSchemaTable.AppendChild(nodeInformationSchemaColumn);

					XmlElement nodeInformationSchemaColumnName = xmlComparisonDocument.CreateElement("Name");
					nodeInformationSchemaColumnName.InnerText = informationSchemaColumn.Value.ColumnName;
					nodeInformationSchemaColumn.AppendChild(nodeInformationSchemaColumnName);

					XmlElement nodeInformationSchemaColumnDataType = xmlComparisonDocument.CreateElement("DataType");
					nodeInformationSchemaColumnDataType.InnerText = informationSchemaColumn.Value.DataType;
					nodeInformationSchemaColumn.AppendChild(nodeInformationSchemaColumnDataType);

					XmlElement nodeInformationSchemaColumnUdtName = xmlComparisonDocument.CreateElement("UdtName");
					nodeInformationSchemaColumnUdtName.InnerText = informationSchemaColumn.Value.UdtName;
					nodeInformationSchemaColumn.AppendChild(nodeInformationSchemaColumnUdtName);
				}

				//Індекси
				foreach (KeyValuePair<string, ConfigurationInformationSchema_Index> informationSchemaIndex in informationSchemaTable.Value.Indexes)
                {
					XmlElement nodeInformationSchemaIndex = xmlComparisonDocument.CreateElement("Index");
					nodeInformationSchemaTable.AppendChild(nodeInformationSchemaIndex);

					XmlElement nodeInformationSchemaIndexName = xmlComparisonDocument.CreateElement("Name");
					nodeInformationSchemaIndexName.InnerText = informationSchemaIndex.Value.IndexName;
					nodeInformationSchemaIndex.AppendChild(nodeInformationSchemaIndexName);
				}
			}

			xmlComparisonDocument.Save(pathToSave);
		}

		#endregion

		#region Comparison (порівняння конфігурації і реальної структури бази даних, генерування коду, реструктуризація бази даних)

		/// <summary>
		/// Функція створює копію конфігурації в тій самій папці що і конфігурація
		/// </summary>
		/// <param name="pathToConf">Шлях до конфігурації</param>
		/// <param name="oldCopyConf">Шлях попередньої копії, якщо така є</param>
		/// <returns>Назву файлу копії</returns>
		public static string CreateCopyConfigurationFile(string pathToConf, string oldCopyConf = "")
		{
			if (File.Exists(pathToConf))
			{
				string dirName = Path.GetDirectoryName(pathToConf);
				string fileNewName = Path.GetFileNameWithoutExtension(pathToConf) + DateTime.Now.ToString("_dd_MM_yyyy_HH_mm_ss") + ".xml";
				string pathToCopyConf = Path.Combine(dirName, fileNewName);

				File.Copy(pathToConf, pathToCopyConf);

				if (!String.IsNullOrEmpty(oldCopyConf))
				{
					string pathToOldCopyConf = Path.Combine(dirName, oldCopyConf);
					if (File.Exists(pathToOldCopyConf))
						File.Delete(pathToOldCopyConf);
				}

				return fileNewName;
			}
			else
				throw new FileNotFoundException(pathToConf);
		}

		/// <summary>
		/// Функція створює тимчасовий файл конфігурації
		/// </summary>
		/// <param name="pathToConf">Шлях до конфігурації</param>
		/// <param name="oldTempConf">Шлях попередньої копії, якщо така є</param>
		/// <returns>Шлях до файлу</returns>
		public static string GetTempPathToConfigurationFile(string pathToConf, string oldTempConf = "")
		{
			if (File.Exists(pathToConf))
			{
				string dirName = Path.GetDirectoryName(pathToConf);
				string fileTempName = Path.GetFileNameWithoutExtension(pathToConf) + "_tmp_" + Guid.NewGuid().ToString() + ".xml";
				string pathToTempConf = Path.Combine(dirName, fileTempName);

				if (!String.IsNullOrEmpty(oldTempConf))
				{
					if (File.Exists(oldTempConf))
						File.Delete(oldTempConf);
				}

				return pathToTempConf;
			}
			else
				throw new FileNotFoundException(pathToConf);
		}

		/// <summary>
		/// Функція перезаписує файл конфігурації тимчасовим файлом конфи. Також видаляє копію та темп файл.
		/// </summary>
		/// <param name="pathToConf">Шлях до файлу конфігурації</param>
		/// <param name="pathToTempConf">Шлях до тимчасового файлу конфігурації</param>
		/// <param name="pathToCopyConf">Шлях до копії файлу конфігурації</param>
		public static void RewriteConfigurationFileFromTempFile(string pathToConf, string pathToTempConf, string pathToCopyConf)
		{
			if (File.Exists(pathToConf))
				File.Delete(pathToConf);

			if (File.Exists(pathToTempConf))
			{
				File.Copy(pathToTempConf, pathToConf);
				File.Delete(pathToTempConf);
			}

			if (File.Exists(pathToCopyConf))
				File.Delete(pathToCopyConf);
		}

		/// <summary>
		/// Функція видаляє тимчасові файли
		/// </summary>
		/// <param name="pathToCopyConf">Шлях до копії файлу конфігурації</param>
		/// <param name="pathToTempConf">Шлях до тимчасового файлу конфігурації</param>
		public static void ClearCopyAndTempConfigurationFile(string pathToConf, string pathToCopyConf, string pathToTempConf)
		{
			string dirName = Path.GetDirectoryName(pathToConf);
			string pathToOldCopyConf = Path.Combine(dirName, pathToCopyConf);

			if (File.Exists(pathToOldCopyConf))
				File.Delete(pathToOldCopyConf);

			if (File.Exists(pathToTempConf))
				File.Delete(pathToTempConf);
		}

		/// <summary>
		/// Функція генерує код на основі конфігурації
		/// </summary>
		/// <param name="pathToConf">Шлях до файлу конфігурації</param>
		/// <param name="pathToTemplate">Шлях до шаблону</param>
		/// <param name="pathToSaveCode">Шлях до файлу куди буде згенерований код</param>
		public static void GenerationCode(string pathToConf, string pathToTemplate, string pathToSaveCode)
		{
			XslCompiledTransform xsltCodeGnerator = new XslCompiledTransform();
			xsltCodeGnerator.Load(pathToTemplate);

			xsltCodeGnerator.Transform(pathToConf, pathToSaveCode);
		}

		/// <summary>
		/// Функція генерує ХМЛ файл на основі порівняння з базою даних та новою і старою конфігураціями
		/// </summary>
		/// <param name="pathToInformationSchemaXML">Шлях до ХМЛ фалу схеми бази даних</param>
		/// <param name="pathToTemplate">Шлях до шаблону</param>
		/// <param name="pathToSaveXml">Шлях куди зберігати результати</param>
		/// <param name="сonfigurationFileName">Назва файлу конфігурації</param>
		/// <param name="secondConfigurationFileName">Назва попередньої копії конфігурації</param>
		public static void Comparison(string pathToInformationSchemaXML, string pathToTemplate, string pathToSaveXml,
			string сonfigurationFileName, string secondConfigurationFileName)
		{
			XslCompiledTransform xsltCodeGnerator = new XslCompiledTransform();
			xsltCodeGnerator.Load(pathToTemplate, new XsltSettings(true, true), null);

			XsltArgumentList xsltArgumentList = new XsltArgumentList();
			xsltArgumentList.AddParam("Configuration", "", сonfigurationFileName);
			xsltArgumentList.AddParam("SecondConfiguration", "", secondConfigurationFileName);

			FileStream fileStream = new FileStream(pathToSaveXml, FileMode.Create);

			xsltCodeGnerator.Transform(pathToInformationSchemaXML, xsltArgumentList, fileStream);

			fileStream.Close();
		}

		/// <summary>
		/// Функція генерує ХМЛ файл з SQL запитами на основі файлу порівняння конфігурацій.
		/// </summary>
		/// <param name="pathToXML">Шлях до ХМЛ файлу, який згенерований функцією Comparison</param>
		/// <param name="pathToTemplate">Шлях до шаблону</param>
		/// <param name="pathToSaveCode">Шлях файлу куди буде збережений вихідний ХМЛ файл</param>
		/// <param name="replacementColumn">Параметр для шаблону (чи потрібно заміщати стовпчики)</param>
		public static void ComparisonAnalizeGeneration(string pathToXML, string pathToTemplate, string pathToSaveCode, string replacementColumn)
		{
			XslCompiledTransform xsltCodeGnerator = new XslCompiledTransform();
			xsltCodeGnerator.Load(pathToTemplate, new XsltSettings(true, true), null);

			XsltArgumentList xsltArgumentList = new XsltArgumentList();
			xsltArgumentList.AddParam("ReplacementColumn", "", replacementColumn);
			xsltArgumentList.AddParam("KeyUID", "", DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss"));

			FileStream fileStream = new FileStream(pathToSaveCode, FileMode.Create);

			xsltCodeGnerator.Transform(pathToXML, xsltArgumentList, fileStream);

			fileStream.Close();
		}

		/// <summary>
		/// Функція зчитує згенерований список SQL запитів функцією ComparisonAnalizeGeneration
		/// </summary>
		/// <param name="pathToXML">Шлях до файлу який згенерувала функція ComparisonAnalizeGeneration</param>
		/// <returns>Список SQL запитів</returns>
		public static List<string> ListComparisonSql(string pathToXML)
		{
			List<string> slqList = new List<string>();

			XPathDocument xPathDoc = new XPathDocument(pathToXML);
			XPathNavigator xPathDocNavigator = xPathDoc.CreateNavigator();

			XPathNodeIterator sqlNodes = xPathDocNavigator.Select("/root/sql");
			while (sqlNodes.MoveNext())
			{
				string sqlText = sqlNodes.Current.Value;
				slqList.Add(sqlText);
			}

			return slqList;
		}

		#endregion
	}
}