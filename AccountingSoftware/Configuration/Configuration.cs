/*
Copyright (C) 2019-2026 TARAKHOMYN YURIY IVANOVYCH
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

using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.IO.Compression;

namespace AccountingSoftware
{
    /// <summary>
    /// Конфігурація.
    /// В цьому класі міститься вся інформація про конфігурацію.
    /// </summary>
    public class Configuration
    {
        #region Публічні поля

        /// <summary>
        /// Назва конфігурації
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Підзаголовок
        /// </summary>
        public string Subtitle { get; set; } = "";

        /// <summary>
        /// Простір імен для конфігурації
        /// </summary>
        public string NameSpaceGeneratedCode { get; set; } = "";

        /// <summary>
        /// Простір імен для програми
        /// </summary>
        public string NameSpace { get; set; } = "";

        /// <summary>
        /// Автор конфігурації
        /// </summary>
        public string Author { get; set; } = "";

        /// <summary>
        /// Опис
        /// </summary>
        public string Desc { get; set; } = "";

        /// <summary>
        /// Назва словника для повнотектового пошуку
        /// </summary>
        public string DictTSearch { get; set; } = DefaultDictTSearch;

        /// <summary>
        /// Назва словника за замовчуванням для повнотектового пошуку
        /// </summary>
        public const string DefaultDictTSearch = "simple";

        /// <summary>
        /// Версія Gtk
        /// </summary>
        public GtkVersion GtkLibVersion { get; set; } = GtkVersion.Gtk3;

        /// <summary>
        /// Варіант завантаження конфігурації
        /// </summary>
        public VariantLoadConf VariantLoadConfiguration { get; private set; } = VariantLoadConf.Full;

        /// <summary>
        /// Блоки констант
        /// </summary>
        public Dictionary<string, ConfigurationConstantsBlock> ConstantsBlock { get; } = [];

        /// <summary>
        /// Довідники
        /// </summary>
        public Dictionary<string, ConfigurationDirectories> Directories { get; } = [];

        /// <summary>
        /// Документи
        /// </summary>
        public Dictionary<string, ConfigurationDocuments> Documents { get; } = [];

        /// <summary>
        /// Журнали
        /// </summary>
        public Dictionary<string, ConfigurationJournals> Journals { get; } = [];

        /// <summary>
        /// Перелічення
        /// </summary>
        public Dictionary<string, ConfigurationEnums> Enums { get; } = [];

        /// <summary>
        /// Регістри відомостей
        /// </summary>
        public Dictionary<string, ConfigurationRegistersInformation> RegistersInformation { get; } = [];

        /// <summary>
        /// Регістри накопичення
        /// </summary>
        public Dictionary<string, ConfigurationRegistersAccumulation> RegistersAccumulation { get; } = [];

        #endregion

        #region Змінні які використовуються тільки при збереженні конфігурації

        /// <summary>
        /// Шлях до хмл файлу конфігурації
        /// </summary>
        public string PathToXmlFileConfiguration { get; set; } = "";

        /// <summary>
        /// Шлях до копії хмл файлу конфігурації
        /// </summary>
        public string PathToCopyXmlFileConfiguration { get; set; } = "";

        /// <summary>
        /// Шлях до тимчасового хмл файлу конфігурації
        /// </summary>
        public string PathToTempXmlFileConfiguration { get; set; } = "";

        #endregion

        #region Приватні поля

        /// <summary>
        /// Список зарезервованих назв таблиць.
        /// Довідка: коли створюється новий довідник, чи документ для нього резервується нова унікальна назва таблиці в базі даних. 
        /// </summary>
        List<string> ReservedUnigueTableName { get; set; } = [];

        /// <summary>
        /// Список зарезервованих назв стовпців.
        /// Ключем виступає назва таблиці для якої резервуються стовпці.
        /// </summary>
        Dictionary<string, List<string>> ReservedUnigueColumnName { get; set; } = [];

        #endregion

        #region Append

        /// <summary>
        /// Додати константу
        /// </summary>
        /// <param name="blockName"></param>
        /// <param name="constants"></param>
        public void AppendConstants(string blockName, ConfigurationConstants constants)
        {
            if (ConstantsBlock.TryGetValue(blockName, out ConfigurationConstantsBlock? block))
            {
                constants.Block = block;
                block.Constants.Add(constants.Name, constants);
            }
        }

        /// <summary>
        /// Додати блок констант
        /// </summary>
        /// <param name="constantsBlock"></param>
        public void AppendConstantsBlock(ConfigurationConstantsBlock constantsBlock) => ConstantsBlock.Add(constantsBlock.BlockName, constantsBlock);

        /// <summary>
        /// Додати довідник в список довідників
        /// </summary>
        /// <param name="Directory">Довідник</param>
        public void AppendDirectory(ConfigurationDirectories Directory) => Directories.Add(Directory.Name, Directory);

        /// <summary>
        /// Додати перелічення в список перелічень
        /// </summary>
        /// <param name="Enum">Перелічення</param>
        public void AppendEnum(ConfigurationEnums Enum) => Enums.Add(Enum.Name, Enum);

        /// <summary>
        /// Додати документ в список документів
        /// </summary>
        /// <param name="Document">Документ</param>
        public void AppendDocument(ConfigurationDocuments document) => Documents.Add(document.Name, document);

        /// <summary>
        /// Додати регістр в список регістрів
        /// </summary>
        /// <param name="registersInformation">Регістр</param>
        public void AppendRegistersInformation(ConfigurationRegistersInformation registersInformation) => RegistersInformation.Add(registersInformation.Name, registersInformation);

        /// <summary>
        /// Додати регістр в список регістрів
        /// </summary>
        /// <param name="registersAccumulation">Регістр</param>
        public void AppendRegistersAccumulation(ConfigurationRegistersAccumulation registersAccumulation) => RegistersAccumulation.Add(registersAccumulation.Name, registersAccumulation);

        /// <summary>
        /// Додати журнал в список журналів
        /// </summary>
        /// <param name="Journal">Журнал</param>
        public void AppendJournal(ConfigurationJournals Journal) => Journals.Add(Journal.Name, Journal);

        #endregion

        #region Function

        /// <summary>
        /// Масив з бук анг. алфавіту. Використовується для задання назви таблиці або стовпчика в базі даних
        /// </summary>
        /// <returns></returns>
        public static string[] GetEnglishAlphabet()
        {
            return
            [
                "a", "b", "c", "d", "e", "f",
                "g", "h", "i", "j", "k", "l",
                "n", "m", "o", "p", "q", "r",
                "s", "t", "u", "v", "w", "x",
                "y", "z"
            ];
        }

        /// <summary>
        /// Український алфавіт
        /// </summary>
        /// <returns></returns>
        public static string[] GetUkrainianAlphabet()
        {
            return
            [
                "а", "б", "в", "г", "ґ", "д",
                "е", "є", "ж", "з", "и", "і",
                "ї", "й", "к", "л", "м", "н",
                "о", "п", "р", "с", "т", "у",
                "ф", "х", "ц", "ч", "ш", "щ",
                "ь", "ю", "я"
            ];
        }

        /// <summary>
        /// Пошук ссилок довідників і документів
        /// </summary>
        /// <param name="searchName">Назва довідника або документу</param>
        /// <param name="variantWorkFunction">Варіант роботи функції</param>
        /// <returns>Повертає список довідників або документів які вказують на searchName</returns>
        public List<string> SearchForPointers(string searchName, VariantWorkSearchForPointers variantWork = VariantWorkSearchForPointers.Info)
        {
            PointerParse(searchName, out Exception? exception);
            if (exception != null) throw exception;

            /*
                if (searchName.IndexOf('.') > 0)
                {
                    string[] searchNameSplit = searchName.Split(".");

                    if (!(searchNameSplit[0] == "Довідники" || searchNameSplit[0] == "Документи"))
                        throw new Exception("Перша частина назви має бути 'Довідники' або 'Документи'");
                }
                else
                    throw new Exception("Назва для пошуку має бути 'Довідники.<Назва довідника>' або 'Документи.<Назва документу>'");
            */

            List<string> ListPointer = [];

            //Перевірити константи
            foreach (ConfigurationConstantsBlock constantsBlockItem in ConstantsBlock.Values)
            {
                foreach (ConfigurationConstants constantsItem in constantsBlockItem.Constants.Values)
                {
                    if (constantsItem.Type == "pointer" && constantsItem.Pointer == searchName)
                    {
                        if (variantWork == VariantWorkSearchForPointers.Tables)
                            ListPointer.Add(SpecialTables.Constants + "." + constantsItem.NameInTable);
                        else
                            ListPointer.Add("Константа (Блок " + constantsBlockItem.BlockName + "): " + constantsItem.Name + "." + constantsItem.Name + " [" + SpecialTables.Constants + "." + constantsItem.NameInTable + "]");
                    }
                }
            }

            //Перевірити поля довідників та поля табличних частин чи часом вони не ссилаються на цей довідник
            foreach (ConfigurationDirectories directoryItem in Directories.Values)
            {
                //Поля довідника
                foreach (ConfigurationField directoryField in directoryItem.Fields.Values)
                {
                    if (directoryField.Type == "pointer" && directoryField.Pointer == searchName)
                    {
                        if (variantWork == VariantWorkSearchForPointers.Tables)
                            ListPointer.Add(directoryItem.Table + "." + directoryField.NameInTable);
                        else
                            ListPointer.Add("Довідники: " + directoryItem.Name + "." + directoryField.Name +
                                " [" + directoryItem.Table + "." + directoryField.NameInTable + "]");
                    }
                }

                //Табличні частини
                foreach (ConfigurationTablePart directoryTablePart in directoryItem.TabularParts.Values)
                {
                    //Поля табличної частини
                    foreach (ConfigurationField tablePartField in directoryTablePart.Fields.Values)
                    {
                        if (tablePartField.Type == "pointer" && tablePartField.Pointer == searchName)
                        {
                            if (variantWork == VariantWorkSearchForPointers.Tables)
                                ListPointer.Add(directoryTablePart.Table + "." + tablePartField.NameInTable);
                            else
                                ListPointer.Add("Довідники (таблична частина): " + directoryItem.Name + "." + directoryTablePart.Name + "." + tablePartField.Name +
                                " [" + directoryTablePart.Table + "." + tablePartField.NameInTable + "]");
                        }
                    }
                }
            }

            //Перевірка документів
            foreach (ConfigurationDocuments documentItem in Documents.Values)
            {
                //Поля довідника
                foreach (ConfigurationField documentField in documentItem.Fields.Values)
                {
                    if (documentField.Type == "pointer" && documentField.Pointer == searchName)
                    {
                        if (variantWork == VariantWorkSearchForPointers.Tables)
                            ListPointer.Add(documentItem.Table + "." + documentField.NameInTable);
                        else
                            ListPointer.Add("Документи: " + documentItem.Name + "." + documentField.Name +
                                " [" + documentItem.Table + "." + documentField.NameInTable + "]");
                    }
                }

                //Табличні частини
                foreach (ConfigurationTablePart documentTablePart in documentItem.TabularParts.Values)
                {
                    //Поля табличної частини
                    foreach (ConfigurationField tablePartField in documentTablePart.Fields.Values)
                    {
                        if (tablePartField.Type == "pointer" && tablePartField.Pointer == searchName)
                        {
                            if (variantWork == VariantWorkSearchForPointers.Tables)
                                ListPointer.Add(documentTablePart.Table + "." + tablePartField.NameInTable);
                            else
                                ListPointer.Add("Документи (таблична частина): " + documentItem.Name + "." + documentTablePart.Name + "." + tablePartField.Name +
                                " [" + documentTablePart.Table + "." + tablePartField.NameInTable + "]");
                        }
                    }
                }
            }

            //Перевірка регістра RegistersInformation
            foreach (ConfigurationRegistersInformation registersInformationItem in RegistersInformation.Values)
            {
                //Поля
                foreach (ConfigurationField registersField in registersInformationItem.DimensionFields.Values)
                {
                    if (registersField.Type == "pointer" && registersField.Pointer == searchName)
                    {
                        if (variantWork == VariantWorkSearchForPointers.Tables)
                            ListPointer.Add(registersInformationItem.Table + "." + registersField.NameInTable);
                        else
                            ListPointer.Add("Регістри відомостей: " + registersInformationItem.Name + "." + registersField.Name +
                                " [" + registersInformationItem.Table + "." + registersField.NameInTable + "]");
                    }
                }

                foreach (ConfigurationField registersField in registersInformationItem.ResourcesFields.Values)
                {
                    if (registersField.Type == "pointer" && registersField.Pointer == searchName)
                    {
                        if (variantWork == VariantWorkSearchForPointers.Tables)
                            ListPointer.Add(registersInformationItem.Table + "." + registersField.NameInTable);
                        else
                            ListPointer.Add("Регістри відомостей: " + registersInformationItem.Name + "." + registersField.Name +
                                " [" + registersInformationItem.Table + "." + registersField.NameInTable + "]");
                    }
                }

                foreach (ConfigurationField registersField in registersInformationItem.PropertyFields.Values)
                {
                    if (registersField.Type == "pointer" && registersField.Pointer == searchName)
                    {
                        if (variantWork == VariantWorkSearchForPointers.Tables)
                            ListPointer.Add(registersInformationItem.Table + "." + registersField.NameInTable);
                        else
                            ListPointer.Add("Регістри відомостей: " + registersInformationItem.Name + "." + registersField.Name +
                                " [" + registersInformationItem.Table + "." + registersField.NameInTable + "]");
                    }
                }
            }

            //Перевірка регістра RegistersAccumulation
            foreach (ConfigurationRegistersAccumulation registersAccumulationItem in RegistersAccumulation.Values)
            {
                //Поля
                foreach (ConfigurationField registersField in registersAccumulationItem.DimensionFields.Values)
                {
                    if (registersField.Type == "pointer" && registersField.Pointer == searchName)
                    {
                        if (variantWork == VariantWorkSearchForPointers.Tables)
                            ListPointer.Add(registersAccumulationItem.Table + "." + registersField.NameInTable);
                        else
                            ListPointer.Add("Регістри накопичення: " + registersAccumulationItem.Name + "." + registersField.Name +
                                " [" + registersAccumulationItem.Table + "." + registersField.NameInTable + "]");
                    }
                }

                foreach (ConfigurationField registersField in registersAccumulationItem.ResourcesFields.Values)
                {
                    if (registersField.Type == "pointer" && registersField.Pointer == searchName)
                    {
                        if (variantWork == VariantWorkSearchForPointers.Tables)
                            ListPointer.Add(registersAccumulationItem.Table + "." + registersField.NameInTable);
                        else
                            ListPointer.Add("Регістри накопичення: " + registersAccumulationItem.Name + "." + registersField.Name +
                                " [" + registersAccumulationItem.Table + "." + registersField.NameInTable + "]");
                    }
                }

                foreach (ConfigurationField registersField in registersAccumulationItem.PropertyFields.Values)
                {
                    if (registersField.Type == "pointer" && registersField.Pointer == searchName)
                    {
                        if (variantWork == VariantWorkSearchForPointers.Tables)
                            ListPointer.Add(registersAccumulationItem.Table + "." + registersField.NameInTable);
                        else
                            ListPointer.Add("Регістри накопичення: " + registersAccumulationItem.Name + "." + registersField.Name +
                                " [" + registersAccumulationItem.Table + "." + registersField.NameInTable + "]");
                    }
                }
            }

            return ListPointer;
        }

        /// <summary>
        /// Пошук залежностей
        /// </summary>
        /// <param name="searchName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public List<ConfigurationDependencies> SearchDependencies(string searchName)
        {
            PointerParse(searchName, out Exception? exception);
            if (exception != null) throw exception;

            /*
            if (searchName.IndexOf('.') > 0)
            {
                string[] searchNameSplit = searchName.Split(".");

                if (!(searchNameSplit[0] == "Довідники" || searchNameSplit[0] == "Документи"))
                    throw new Exception("Перша частина назви має бути 'Довідники' або 'Документи'");
            }
            else
                throw new Exception("Назва для пошуку має бути 'Довідники.<Назва довідника>' або 'Документи.<Назва документу>'");
            */

            List<ConfigurationDependencies> ListDependencies = [];

            //Перевірити константи
            foreach (ConfigurationConstantsBlock constantsBlockItem in ConstantsBlock.Values)
                foreach (ConfigurationConstants constantsItem in constantsBlockItem.Constants.Values)
                {
                    if (constantsItem.Type == "pointer" && constantsItem.Pointer == searchName)
                        ListDependencies.Add(new ConfigurationDependencies()
                        {
                            ConfigurationGroupName = "Константи",
                            ConfigurationObjectName = constantsItem.Name,
                            ConfigurationObjectDesc = constantsItem.Desc,
                            Table = SpecialTables.Constants,
                            Field = constantsItem.NameInTable
                        });

                    //Табличні частини
                    foreach (ConfigurationTablePart constantsTablePart in constantsItem.TabularParts.Values)
                        //Поля табличної частини
                        foreach (ConfigurationField tablePartField in constantsTablePart.Fields.Values)
                            if (tablePartField.Type == "pointer" && tablePartField.Pointer == searchName)
                                ListDependencies.Add(new ConfigurationDependencies()
                                {
                                    ConfigurationGroupName = "Константи",
                                    ConfigurationGroupLevel = ConfigurationDependencies.GroupLevel.TablePartWithoutOwner,
                                    ConfigurationTablePartName = constantsTablePart.Name,
                                    ConfigurationObjectName = constantsItem.Name,
                                    ConfigurationObjectDesc = constantsItem.Desc,
                                    Table = constantsTablePart.Table,
                                    ConfigurationFieldName = tablePartField.Name,
                                    Field = tablePartField.NameInTable
                                });
                }

            //Перевірити поля довідників та поля табличних частин чи часом вони не ссилаються на цей довідник
            foreach (ConfigurationDirectories directoryItem in Directories.Values)
            {
                //Поля довідника
                foreach (ConfigurationField directoryField in directoryItem.Fields.Values)
                    if (directoryField.Type == "pointer" && directoryField.Pointer == searchName)
                        ListDependencies.Add(new ConfigurationDependencies()
                        {
                            ConfigurationGroupName = "Довідники",
                            ConfigurationObjectName = directoryItem.Name,
                            ConfigurationObjectDesc = directoryItem.Desc,
                            Table = directoryItem.Table,
                            ConfigurationFieldName = directoryField.Name,
                            Field = directoryField.NameInTable
                        });

                //Табличні частини
                foreach (ConfigurationTablePart directoryTablePart in directoryItem.TabularParts.Values)
                    //Поля табличної частини
                    foreach (ConfigurationField tablePartField in directoryTablePart.Fields.Values)
                        if (tablePartField.Type == "pointer" && tablePartField.Pointer == searchName)
                            ListDependencies.Add(new ConfigurationDependencies()
                            {
                                ConfigurationGroupName = "Довідники",
                                ConfigurationGroupLevel = ConfigurationDependencies.GroupLevel.TablePart,
                                ConfigurationTablePartName = directoryTablePart.Name,
                                ConfigurationObjectName = directoryItem.Name,
                                ConfigurationObjectDesc = directoryItem.Desc,
                                Table = directoryTablePart.Table,
                                ConfigurationFieldName = tablePartField.Name,
                                Field = tablePartField.NameInTable
                            });
            }

            //Перевірка документів
            foreach (ConfigurationDocuments documentItem in Documents.Values)
            {
                //Поля довідника
                foreach (ConfigurationField documentField in documentItem.Fields.Values)
                    if (documentField.Type == "pointer" && documentField.Pointer == searchName)
                        ListDependencies.Add(new ConfigurationDependencies()
                        {
                            ConfigurationGroupName = "Документи",
                            ConfigurationObjectName = documentItem.Name,
                            ConfigurationObjectDesc = documentItem.Desc,
                            Table = documentItem.Table,
                            ConfigurationFieldName = documentField.Name,
                            Field = documentField.NameInTable
                        });

                //Табличні частини
                foreach (ConfigurationTablePart documentTablePart in documentItem.TabularParts.Values)
                    //Поля табличної частини
                    foreach (ConfigurationField tablePartField in documentTablePart.Fields.Values)
                        if (tablePartField.Type == "pointer" && tablePartField.Pointer == searchName)
                            ListDependencies.Add(new ConfigurationDependencies()
                            {
                                ConfigurationGroupName = "Документи",
                                ConfigurationGroupLevel = ConfigurationDependencies.GroupLevel.TablePart,
                                ConfigurationTablePartName = documentTablePart.Name,
                                ConfigurationObjectName = documentItem.Name,
                                ConfigurationObjectDesc = documentItem.Desc,
                                Table = documentTablePart.Table,
                                ConfigurationFieldName = tablePartField.Name,
                                Field = tablePartField.NameInTable
                            });
            }

            //Перевірка регістра RegistersInformation
            foreach (ConfigurationRegistersInformation registersInformationItem in RegistersInformation.Values)
            {
                //Поля
                foreach (ConfigurationField registersField in registersInformationItem.DimensionFields.Values)
                    if (registersField.Type == "pointer" && registersField.Pointer == searchName)
                        ListDependencies.Add(new ConfigurationDependencies()
                        {
                            ConfigurationGroupName = "РегістриІнформації",
                            ConfigurationObjectName = registersInformationItem.Name,
                            ConfigurationObjectDesc = registersInformationItem.Desc,
                            Table = registersInformationItem.Table,
                            ConfigurationFieldName = registersField.Name,
                            Field = registersField.NameInTable
                        });

                foreach (ConfigurationField registersField in registersInformationItem.ResourcesFields.Values)
                    if (registersField.Type == "pointer" && registersField.Pointer == searchName)
                        ListDependencies.Add(new ConfigurationDependencies()
                        {
                            ConfigurationGroupName = "РегістриІнформації",
                            ConfigurationObjectName = registersInformationItem.Name,
                            ConfigurationObjectDesc = registersInformationItem.Desc,
                            Table = registersInformationItem.Table,
                            ConfigurationFieldName = registersField.Name,
                            Field = registersField.NameInTable
                        });

                foreach (ConfigurationField registersField in registersInformationItem.PropertyFields.Values)
                    if (registersField.Type == "pointer" && registersField.Pointer == searchName)
                        ListDependencies.Add(new ConfigurationDependencies()
                        {
                            ConfigurationGroupName = "РегістриІнформації",
                            ConfigurationObjectName = registersInformationItem.Name,
                            ConfigurationObjectDesc = registersInformationItem.Desc,
                            Table = registersInformationItem.Table,
                            ConfigurationFieldName = registersField.Name,
                            Field = registersField.NameInTable
                        });
            }

            //Перевірка регістра RegistersAccumulation
            foreach (ConfigurationRegistersAccumulation registersAccumulationItem in RegistersAccumulation.Values)
            {
                //Поля
                foreach (ConfigurationField registersField in registersAccumulationItem.DimensionFields.Values)
                    if (registersField.Type == "pointer" && registersField.Pointer == searchName)
                        ListDependencies.Add(new ConfigurationDependencies()
                        {
                            ConfigurationGroupName = "РегістриНакопичення",
                            ConfigurationObjectName = registersAccumulationItem.Name,
                            ConfigurationObjectDesc = registersAccumulationItem.Desc,
                            Table = registersAccumulationItem.Table,
                            ConfigurationFieldName = registersField.Name,
                            Field = registersField.NameInTable
                        });

                foreach (ConfigurationField registersField in registersAccumulationItem.ResourcesFields.Values)
                    if (registersField.Type == "pointer" && registersField.Pointer == searchName)
                        ListDependencies.Add(new ConfigurationDependencies()
                        {
                            ConfigurationGroupName = "РегістриНакопичення",
                            ConfigurationObjectName = registersAccumulationItem.Name,
                            ConfigurationObjectDesc = registersAccumulationItem.Desc,
                            Table = registersAccumulationItem.Table,
                            ConfigurationFieldName = registersField.Name,
                            Field = registersField.NameInTable
                        });

                foreach (ConfigurationField registersField in registersAccumulationItem.PropertyFields.Values)
                    if (registersField.Type == "pointer" && registersField.Pointer == searchName)
                        ListDependencies.Add(new ConfigurationDependencies()
                        {
                            ConfigurationGroupName = "РегістриНакопичення",
                            ConfigurationObjectName = registersAccumulationItem.Name,
                            ConfigurationObjectDesc = registersAccumulationItem.Desc,
                            Table = registersAccumulationItem.Table,
                            ConfigurationFieldName = registersField.Name,
                            Field = registersField.NameInTable
                        });

                //Табличні частини
                foreach (ConfigurationTablePart registersAccumulationTablePart in registersAccumulationItem.TabularParts.Values)
                    //Поля табличної частини
                    foreach (ConfigurationField tablePartField in registersAccumulationTablePart.Fields.Values)
                        if (tablePartField.Type == "pointer" && tablePartField.Pointer == searchName)
                            ListDependencies.Add(new ConfigurationDependencies()
                            {
                                ConfigurationGroupName = "РегістриНакопичення",
                                ConfigurationGroupLevel = ConfigurationDependencies.GroupLevel.TablePartWithoutOwner,
                                ConfigurationTablePartName = registersAccumulationTablePart.Name,
                                ConfigurationObjectName = registersAccumulationItem.Name,
                                ConfigurationObjectDesc = registersAccumulationItem.Desc,
                                Table = registersAccumulationTablePart.Table,
                                ConfigurationFieldName = tablePartField.Name,
                                Field = tablePartField.NameInTable
                            });
            }

            return ListDependencies;
        }

        /// <summary>
        /// Пошук ссилок на перелічення
        /// </summary>
        /// <param name="searchName">Назва перелічення</param>
        /// <returns>Повертає список довідників або документів які вказують на searchName</returns>
        public List<string> SearchForPointersEnum(string searchName)
        {
            if (searchName.IndexOf('.') > 0)
            {
                string[] searchNameSplit = searchName.Split(["."], StringSplitOptions.None);

                if (!(searchNameSplit[0] == "Перелічення"))
                    throw new Exception("Перша частина назви має бути 'Перелічення'");
            }
            else
                throw new Exception("Назва для пошуку має бути 'Перелічення.<Назва перелічення>'");

            List<string> ListPointer = [];

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
                foreach (ConfigurationField directoryField in directoryItem.Fields.Values)
                {
                    if (directoryField.Type == "enum" && directoryField.Pointer == searchName)
                        ListPointer.Add("Довідники: " + directoryItem.Name + "." + directoryField.Name);
                }

                //Табличні частини
                foreach (ConfigurationTablePart directoryTablePart in directoryItem.TabularParts.Values)
                {
                    //Поля табличної частини
                    foreach (ConfigurationField tablePartField in directoryTablePart.Fields.Values)
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
                foreach (ConfigurationField documentField in documentItem.Fields.Values)
                {
                    if (documentField.Type == "enum" && documentField.Pointer == searchName)
                        ListPointer.Add("Документи: " + documentItem.Name + "." + documentField.Name);
                }

                //Табличні частини
                foreach (ConfigurationTablePart documentTablePart in documentItem.TabularParts.Values)
                {
                    //Поля табличної частини
                    foreach (ConfigurationField tablePartField in documentTablePart.Fields.Values)
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
                foreach (ConfigurationField registersField in registersInformationItem.DimensionFields.Values)
                {
                    if (registersField.Type == "enum" && registersField.Pointer == searchName)
                        ListPointer.Add("Регістри відомостей: " + registersInformationItem.Name + "." + registersField.Name);
                }

                foreach (ConfigurationField registersField in registersInformationItem.ResourcesFields.Values)
                {
                    if (registersField.Type == "enum" && registersField.Pointer == searchName)
                        ListPointer.Add("Регістри відомостей: " + registersInformationItem.Name + "." + registersField.Name);
                }

                foreach (ConfigurationField registersField in registersInformationItem.PropertyFields.Values)
                {
                    if (registersField.Type == "enum" && registersField.Pointer == searchName)
                        ListPointer.Add("Регістри відомостей: " + registersInformationItem.Name + "." + registersField.Name);
                }
            }

            //Перевірка регістра RegistersAccumulation
            foreach (ConfigurationRegistersAccumulation registersAccumulationItem in RegistersAccumulation.Values)
            {
                //Поля
                foreach (ConfigurationField registersField in registersAccumulationItem.DimensionFields.Values)
                {
                    if (registersField.Type == "enum" && registersField.Pointer == searchName)
                        ListPointer.Add("Регістри накопичення: " + registersAccumulationItem.Name + "." + registersField.Name);
                }

                foreach (ConfigurationField registersField in registersAccumulationItem.ResourcesFields.Values)
                {
                    if (registersField.Type == "enum" && registersField.Pointer == searchName)
                        ListPointer.Add("Регістри накопичення: " + registersAccumulationItem.Name + "." + registersField.Name);
                }

                foreach (ConfigurationField registersField in registersAccumulationItem.PropertyFields.Values)
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
        public static string GetNewUnigueColumnName(Kernel Kernel, string table, Dictionary<string, ConfigurationField> Fields)
        {
            string[] englishAlphabet = GetEnglishAlphabet();

            bool noExistInReserved = false;
            bool noExistInConf = false;
            string columnNewName = "";

            if (string.IsNullOrEmpty(table))
                table = "0";

            if (!Kernel.Conf.ReservedUnigueColumnName.ContainsKey(table))
                Kernel.Conf.ReservedUnigueColumnName.Add(table, []);

            for (int j = 0; j < englishAlphabet.Length; j++)
            {
                for (int i = 1; i < 10; i++)
                {
                    columnNewName = "col_" + englishAlphabet[j] + i.ToString();

                    if (!Kernel.Conf.ReservedUnigueColumnName[table].Contains(columnNewName))
                        noExistInReserved = true;
                    else
                        continue;

                    noExistInConf = true;

                    foreach (ConfigurationField ConfigurationField in Fields.Values)
                        if (ConfigurationField.NameInTable == columnNewName)
                        {
                            noExistInConf = false;
                            break;
                        }

                    if (noExistInReserved && noExistInConf)
                        break;
                }

                if (noExistInReserved && noExistInConf)
                    break;
            }

            Kernel.Conf.ReservedUnigueColumnName[table].Add(columnNewName);

            return columnNewName;
        }

        /// <summary>
        /// Повертає унікальну назву таблиці
        /// </summary>
        /// <param name="Kernel">Ядро</param>
        /// <returns>Повертає унікальну назву таблиці</returns>
        public static async ValueTask<string> GetNewUnigueTableName(Kernel Kernel)
        {
            string[] englishAlphabet = GetEnglishAlphabet();
            List<string> tableList = await Kernel.DataBase.GetTableList();

            bool noExistInReserved = false;
            bool noExistInBase = false;
            bool noExistInConf = false;
            string tabNewName = "";

            for (int j = 0; j < englishAlphabet.Length; j++)
            {
                for (int i = 1; i < 100; i++)
                {
                    tabNewName = "tab_" + englishAlphabet[j] + i.ToString("D2");

                    if (!Kernel.Conf.ReservedUnigueTableName.Contains(tabNewName))
                        noExistInReserved = true;
                    else
                        continue;

                    if (!tableList.Contains(tabNewName))
                        noExistInBase = true;
                    else
                        continue;

                    noExistInConf = true;

                    foreach (ConfigurationConstantsBlock block in Kernel.Conf.ConstantsBlock.Values)
                    {
                        foreach (ConfigurationConstants constantsItem in block.Constants.Values)
                        {
                            foreach (ConfigurationTablePart constantsTablePart in constantsItem.TabularParts.Values)
                                if (constantsTablePart.Table == tabNewName)
                                {
                                    noExistInConf = false;
                                    break;
                                }

                            if (!noExistInConf)
                                break;
                        }

                        if (!noExistInConf)
                            break;
                    }

                    if (noExistInConf)
                        foreach (ConfigurationDirectories directoryItem in Kernel.Conf.Directories.Values)
                        {
                            if (directoryItem.Table == tabNewName)
                            {
                                noExistInConf = false;
                                break;
                            }

                            foreach (ConfigurationTablePart directoryTablePart in directoryItem.TabularParts.Values)
                                if (directoryTablePart.Table == tabNewName)
                                {
                                    noExistInConf = false;
                                    break;
                                }

                            if (!noExistInConf)
                                break;
                        }

                    if (noExistInConf)
                        foreach (ConfigurationDocuments documentItem in Kernel.Conf.Documents.Values)
                        {
                            if (documentItem.Table == tabNewName)
                            {
                                noExistInConf = false;
                                break;
                            }

                            foreach (ConfigurationTablePart documentTablePart in documentItem.TabularParts.Values)
                                if (documentTablePart.Table == tabNewName)
                                {
                                    noExistInConf = false;
                                    break;
                                }

                            if (!noExistInConf)
                                break;
                        }

                    if (noExistInConf)
                        foreach (ConfigurationRegistersInformation registersInformation in Kernel.Conf.RegistersInformation.Values)
                            if (registersInformation.Table == tabNewName)
                            {
                                noExistInConf = false;
                                break;
                            }

                    if (noExistInConf)
                        foreach (ConfigurationRegistersAccumulation registersAccumulation in Kernel.Conf.RegistersAccumulation.Values)
                            if (registersAccumulation.Table == tabNewName)
                            {
                                noExistInConf = false;
                                break;
                            }

                    if (noExistInReserved && noExistInBase && noExistInConf)
                        break;
                }

                if (noExistInReserved && noExistInBase && noExistInConf)
                    break;
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
        public static string ValidateConfigurationObjectName(ref string configurationObjectName)
        {
            string errorList = "";

            configurationObjectName = configurationObjectName.Trim();

            if (string.IsNullOrEmpty(configurationObjectName))
                return errorList += "Назва не задана";

            string allovChar = "abcdefghijklnmopqrstuvwxyz_";
            string allovNum = "0123456789";
            string allovCharCyrillic = "абвгґдеєжзиіїйклмнопрстуфхцчшщьюя"; //"АБВГДЕЄЖЗИІЇКЛМНОПРСТУФХЦЧШЩЪЫЭЮЯЬ";
            string allovAll = allovChar + allovNum + allovCharCyrillic;

            string configurationObjectModificeName = "";

            for (int i = 0; i < configurationObjectName.Length; i++)
            {
                string checkChar = configurationObjectName.Substring(i, 1);
                string checkCharLover = checkChar.ToLower();

                if (allovAll.Contains(checkCharLover))
                {
                    if (i == 0 && allovNum.Contains(checkCharLover))
                        errorList += "Назва має починатися з букви\n";
                }
                else
                    errorList += "Недопустимий символ (" + i.ToString() + "): " + "[" + checkChar + "]\n";

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
            foreach (ConfigurationRegistersAccumulation regAccumItem in RegistersAccumulation.Values)
                regAccumItem.AllowDocumentSpend.Clear();

            //Документи
            foreach (ConfigurationDocuments docItem in Documents.Values)
                foreach (string reg in docItem.AllowRegisterAccumulation) //Регістри накопичення по яких може робити рухи документ
                    if (RegistersAccumulation.TryGetValue(reg, out ConfigurationRegistersAccumulation? regAccum) && !regAccum.AllowDocumentSpend.Contains(docItem.Name))
                        regAccum.AllowDocumentSpend.Add(docItem.Name);
        }

        /// <summary>
        /// Функція об'єднує в один масив всі поля регістру
        /// </summary>
        public static Dictionary<string, ConfigurationField> CombineAllFieldForRegister(
            Dictionary<string, ConfigurationField>.ValueCollection DimensionFields,
            Dictionary<string, ConfigurationField>.ValueCollection ResourcesFields,
            Dictionary<string, ConfigurationField>.ValueCollection PropertyFields
            )
        {
            Dictionary<string, ConfigurationField> AllFields = [];

            foreach (ConfigurationField item in DimensionFields)
                AllFields.Add(item.Name, item);

            foreach (ConfigurationField item in ResourcesFields)
                AllFields.Add(item.Name, item);

            foreach (ConfigurationField item in PropertyFields)
                AllFields.Add(item.Name, item);

            return AllFields;
        }

        /// <summary>
        /// Функція парсить вказівник
        /// </summary>
        /// <param name="Pointer">Назва вказівника типу 'Довідники.<Назва довідника>' або 'Документи.<Назва документу>'</param>
        /// <param name="ex">Помилка</param>
        /// <returns>Кортеж</returns>
        public static (bool Result, string PointerGroup, string PointerType) PointerParse(string pointer, out Exception? ex)
        {
            ex = null;

            if (pointer.Contains('.'))
            {
                string[] pointerSplit = pointer.Split(".");
                string pointerGroup = pointerSplit[0];
                string pointerType = pointerSplit[1];

                if (!(pointerGroup == "Довідники" || pointerGroup == "Документи"))
                {
                    ex = new Exception("Перша частина Pointer має бути 'Довідники' або 'Документи'");
                    return (false, "", "");
                }

                return (true, pointerGroup, pointerType);
            }
            else
            {
                ex = new Exception("Pointer має бути 'Довідники.<Назва довідника>' або 'Документи.<Назва документу>'");
                return (false, "", "");
            }
        }

        /// <summary>
        /// Перетворює ім'я типу НавзваПоляТаОпис в Назва поля та опис
        /// </summary>
        /// <param name="name">Назва</param>
        /// <returns>Модифікована назва</returns>
        public static string CreateFullName(string name)
        {
            string fullName = "";

            foreach (char letter in name)
            {
                if (char.IsUpper(letter) && fullName.Length > 0)
                    fullName += " " + letter.ToString().ToLower();
                else
                    fullName += letter;
            }

            return fullName;
        }

        /// <summary>
        /// Стиснення та кодування тексту у формат Base64
        /// </summary>
        /// <param name="text">Текст</param>
        /// <returns>Заархівований текст</returns>
        public static string ZipAndBase64Text(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";

            var bytes = Encoding.UTF8.GetBytes(text);

            using var mso = new MemoryStream();
            using (var gs = new GZipStream(mso, CompressionMode.Compress))
                gs.Write(bytes, 0, bytes.Length);

            return Convert.ToBase64String(mso.ToArray());
        }

        /// <summary>
        /// Розкодування тексту з Base64 та розархівування
        /// </summary>
        /// <param name="text">Текст у форматі Base64</param>
        /// <returns>Звичайний текст</returns>
        public static string UnZipAndBase64Text(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";

            try
            {
                var bytes = Convert.FromBase64String(text);

                using var msi = new MemoryStream(bytes);
                using var gs = new GZipStream(msi, CompressionMode.Decompress);
                using var mso = new MemoryStream();
                gs.CopyTo(mso);

                return Encoding.UTF8.GetString(mso.ToArray());
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Створює вітку із стисненим вмістом
        /// </summary>
        /// <param name="xmlConfDocument">ХМЛ документ</param>
        /// <param name="nodeName">Назва вітки</param>
        /// <param name="nodeValue">Значення</param>
        /// <returns>Повертає створену вітку</returns>
        static XmlElement SetZipAndBase64TextToNode(XmlDocument xmlConfDocument, string nodeName, string nodeValue)
        {
            XmlElement node = xmlConfDocument.CreateElement(nodeName);
            node.InnerText = ZipAndBase64Text(nodeValue);

            XmlAttribute attrZip = xmlConfDocument.CreateAttribute("Zip");
            attrZip.Value = "1";
            node.Attributes.Append(attrZip);

            return node;
        }

        /// <summary>
        /// Отримує значення із вітки і за потреби розархівовує вміст
        /// </summary>
        /// <param name="node">Вітка</param>
        /// <returns>Вміст вітки</returns>
        static string GetUnZipAndBase64TextFromNode(XPathNavigator? node)
        {
            string value = node?.Value ?? "";
            string attrZip = node?.GetAttribute("Zip", "") ?? "";
            return attrZip == "1" ? UnZipAndBase64Text(value) : value;
        }

        #endregion

        #region Load (завантаження конфігурації з ХМЛ файлу)

        /// <summary>
        /// Завантаження конфігурації з ХМЛ файлу
        /// </summary>
        /// <param name="pathToConf">Шлях до файлу конфігурації</param>
        /// <param name="Conf">Конфігурація</param>
        /// <param name="variantLoadConf">Варіант завантаження конфігурації</param>
        public static void Load(string pathToConf, out Configuration Conf, VariantLoadConf variantLoadConf = VariantLoadConf.Full)
        {
            Conf = new Configuration() { VariantLoadConfiguration = variantLoadConf };

            //Якщо конфігурація не знайдена, створюю нову пусту конфігурацію і записую
            if (!File.Exists(pathToConf))
                Save(pathToConf, new Configuration()
                {
                    Name = "Нова конфігурація",
                    NameSpaceGeneratedCode = "НоваКонфігурація",
                    NameSpace = "НоваКонфігурація_1_0"
                });

            XPathDocument xPathDoc = new(pathToConf);
            XPathNavigator xPathDocNavigator = xPathDoc.CreateNavigator();

            LoadConfigurationInfo(Conf, xPathDocNavigator);

            LoadConstants(Conf, xPathDocNavigator);

            LoadDirectories(Conf, xPathDocNavigator);

            LoadDocuments(Conf, xPathDocNavigator);

            LoadEnums(Conf, xPathDocNavigator);

            LoadJournals(Conf, xPathDocNavigator);

            LoadRegistersInformation(Conf, xPathDocNavigator);

            LoadRegistersAccumulation(Conf, xPathDocNavigator);
        }

        /// <summary>
        /// Завантаження загальної інформації
        /// </summary>
        /// <param name="Conf">Конфігурація</param>
        /// <param name="xPathDocNavigator">Навігатор</param>
        private static void LoadConfigurationInfo(Configuration Conf, XPathNavigator xPathDocNavigator)
        {
            XPathNavigator? rootNodeConfiguration = xPathDocNavigator.SelectSingleNode("/Configuration");
            if (rootNodeConfiguration != null)
            {
                Conf.Name = rootNodeConfiguration.SelectSingleNode("Name")?.Value ?? "";
                Conf.Subtitle = rootNodeConfiguration.SelectSingleNode("Subtitle")?.Value ?? "";
                Conf.NameSpaceGeneratedCode = rootNodeConfiguration.SelectSingleNode("NameSpaceGeneratedCode")?.Value ?? "";
                Conf.NameSpace = rootNodeConfiguration.SelectSingleNode("NameSpace")?.Value ?? "";
                Conf.Author = rootNodeConfiguration.SelectSingleNode("Author")?.Value ?? "";
                Conf.Desc = rootNodeConfiguration.SelectSingleNode("Desc")?.Value ?? "";
                Conf.DictTSearch = rootNodeConfiguration.SelectSingleNode("DictTSearch")?.Value ?? DefaultDictTSearch;
                Conf.GtkLibVersion = Enum.TryParse(rootNodeConfiguration.SelectSingleNode("GtkVersion")?.Value, out GtkVersion gtkversion) ? gtkversion : GtkVersion.Gtk3;
            }
        }

        /// <summary>
        /// Завантаження констант. 
        /// Константи згруповані в блоки констант.
        /// </summary>
        /// <param name="Conf">Конфігурація</param>
        /// <param name="xPathDocNavigator">Навігатор</param>
        private static void LoadConstants(Configuration Conf, XPathNavigator xPathDocNavigator)
        {
            XPathNodeIterator constantsBlockNodes = xPathDocNavigator.Select("/Configuration/ConstantsBlocks/ConstantsBlock");
            while (constantsBlockNodes.MoveNext())
            {
                string? blockName = constantsBlockNodes.Current?.SelectSingleNode("Name")?.Value ?? throw new Exception("Не задана назва блоку констант");
                string blockDesc = constantsBlockNodes.Current?.SelectSingleNode("Desc")?.Value ?? "";

                ConfigurationConstantsBlock configurationConstantsBlock = new(blockName, blockDesc);
                Conf.ConstantsBlock.Add(configurationConstantsBlock.BlockName, configurationConstantsBlock);

                XPathNodeIterator? constantsNodes = constantsBlockNodes.Current?.Select("Constants/Constant");
                if (constantsNodes != null)
                    while (constantsNodes.MoveNext())
                    {
                        string? constName = constantsNodes.Current?.SelectSingleNode("Name")?.Value ?? throw new Exception("Не задана назва константи");
                        string nameInTable = constantsNodes.Current?.SelectSingleNode("NameInTable")?.Value ?? "";
                        string constType = constantsNodes.Current?.SelectSingleNode("Type")?.Value ?? "";
                        string constDesc = constantsNodes.Current?.SelectSingleNode("Desc")?.Value ?? "";

                        string constPointer = (constType == "pointer" || constType == "enum") ?
                            (constantsNodes.Current?.SelectSingleNode("Pointer")?.Value ?? "") : "";

                        ConfigurationConstants configurationConstants = new(constName, nameInTable, constType, configurationConstantsBlock, constPointer, constDesc);

                        configurationConstantsBlock.Constants.Add(configurationConstants.Name, configurationConstants);

                        LoadTabularParts(Conf, configurationConstants.TabularParts, constantsNodes.Current);
                    }
            }
        }

        /// <summary>
        /// Завантаження довідників
        /// </summary>
        /// <param name="Conf">Конфігурація</param>
        /// <param name="xPathDocNavigator">Навігатор</param>
        private static void LoadDirectories(Configuration Conf, XPathNavigator xPathDocNavigator)
        {
            //Довідники
            XPathNodeIterator directoryNodes = xPathDocNavigator.Select("/Configuration/Directories/Directory");
            while (directoryNodes.MoveNext())
            {
                string? name = directoryNodes.Current?.SelectSingleNode("Name")?.Value ?? throw new Exception("Не задана назва довідника");
                string fullName = directoryNodes.Current?.SelectSingleNode("FullName")?.Value ?? ""; //Повна назва
                string table = directoryNodes.Current?.SelectSingleNode("Table")?.Value ?? ""; //Таблиця
                string desc = directoryNodes.Current?.SelectSingleNode("Desc")?.Value ?? ""; //Опис
                string dirType = directoryNodes.Current?.SelectSingleNode("Type")?.Value ?? ""; //Тип довідника
                bool autoNum = (directoryNodes.Current?.SelectSingleNode("AutoNum")?.Value ?? "") == "1"; //Автонумерація
                bool versionsHistory = (directoryNodes.Current?.SelectSingleNode("VersionsHistory")?.Value ?? "") == "1"; //Ведення історії версій

                //Якщо довідник підпорядкований іншому довіднику
                string directoryOwner = directoryNodes.Current?.SelectSingleNode("DirectoryOwner")?.Value ?? ""; //Власник довідника, якщо є підпорядкування. Вказівник на довідник. Наприклад "Довідники.Номенклатура"
                string pointerFieldOwner = directoryNodes.Current?.SelectSingleNode("PointerFieldOwner")?.Value ?? ""; //Поле яке відповідає за підпорядкування власнику, назва поля наприклад "Номенклатура"

                //Тип довідника (звичайний , ієрархічний чи ієрархія в окремому довіднику)
                ConfigurationDirectories.TypeDirectories typeDirectory = dirType switch
                {
                    "Normal" => ConfigurationDirectories.TypeDirectories.Normal,
                    "Hierarchical" => ConfigurationDirectories.TypeDirectories.Hierarchical,
                    "HierarchyInAnotherDirectory" => ConfigurationDirectories.TypeDirectories.HierarchyInAnotherDirectory,
                    _ => ConfigurationDirectories.TypeDirectories.Normal
                };

                string parentField_Hierarchical = "";
                string iconTree_Hierarchical = "";
                string isFolderField_Hierarchical = "";
                string pointerFolders_HierarchyInAnotherDirectory = "";
                ConfigurationDirectories.HierarchicalContentType allowedContent_Hierarchical = ConfigurationDirectories.HierarchicalContentType.Folders;

                //Поля для ієрархічного довідника
                if (typeDirectory == ConfigurationDirectories.TypeDirectories.Hierarchical)
                {
                    parentField_Hierarchical = directoryNodes.Current?.SelectSingleNode("ParentField")?.Value ?? ""; //Поле родич. Тобто поле яке буде містити вказівник на родича чи папку
                    iconTree_Hierarchical = directoryNodes.Current?.SelectSingleNode("IconTree")?.Value ?? ""; //Тип іконки в дереві

                    //Тип вмісту ієрархічного довідника
                    string allowedContent = directoryNodes.Current?.SelectSingleNode("AllowedContent")?.Value ?? "";
                    allowedContent_Hierarchical = allowedContent switch
                    {
                        "Folders" => ConfigurationDirectories.HierarchicalContentType.Folders,
                        "Elements" => ConfigurationDirectories.HierarchicalContentType.Elements,
                        "FoldersAndElements" => ConfigurationDirectories.HierarchicalContentType.FoldersAndElements,
                        _ => ConfigurationDirectories.HierarchicalContentType.Folders
                    };

                    if (allowedContent_Hierarchical == ConfigurationDirectories.HierarchicalContentType.FoldersAndElements)
                        isFolderField_Hierarchical = directoryNodes.Current?.SelectSingleNode("IsFolderField")?.Value ?? ""; //Поле ЦеПапка
                }
                //Для типу ієрархія в окремому довіднику
                else if (typeDirectory == ConfigurationDirectories.TypeDirectories.HierarchyInAnotherDirectory)
                    pointerFolders_HierarchyInAnotherDirectory = directoryNodes.Current?.SelectSingleNode("PointerFolders")?.Value ?? ""; //Повна назва типу довідника, вказівник на тип довідника де зберігаються папки. Наприклад "Довідники.Номенклатура_Папки"

                ConfigurationDirectories ConfObjectDirectories = new(name, fullName, table, desc, autoNum, typeDirectory)
                {
                    //Записувати історію версій
                    VersionsHistory = versionsHistory,

                    //Для типу Hierarchical
                    ParentField_Hierarchical = parentField_Hierarchical,
                    IconTree_Hierarchical = iconTree_Hierarchical,
                    IsFolderField_Hierarchical = isFolderField_Hierarchical,
                    AllowedContent_Hierarchical = allowedContent_Hierarchical,

                    //Для типу HierarchyInAnotherDirectory
                    PointerFolders_HierarchyInAnotherDirectory = pointerFolders_HierarchyInAnotherDirectory,

                    //Підпорядкування / Subordination
                    DirectoryOwner_Subordination = directoryOwner,
                    PointerFieldOwner_Subordination = pointerFieldOwner
                };

                Conf.Directories.Add(ConfObjectDirectories.Name, ConfObjectDirectories);

                LoadFields(ConfObjectDirectories.Fields, directoryNodes.Current, "Directory");

                LoadTabularParts(Conf, ConfObjectDirectories.TabularParts, directoryNodes.Current);

                if (Conf.VariantLoadConfiguration == VariantLoadConf.Full)
                {
                    LoadTabularList(ConfObjectDirectories.TabularList, directoryNodes.Current);

                    LoadTriggerFunctions(ConfObjectDirectories.TriggerFunctions, directoryNodes.Current);

                    LoadForms(ConfObjectDirectories.Forms, directoryNodes.Current);
                }
            }
        }

        /// <summary>
        /// Завантаження полів
        /// </summary>
        /// <param name="fields">Колекція полів</param>
        /// <param name="xPathDocNavigator">Навігатор</param>
        /// <param name="parentName">Назва власника колеції полів</param>
        private static void LoadFields(Dictionary<string, ConfigurationField> fields, XPathNavigator? xPathDocNavigator, string parentName)
        {
            XPathNodeIterator? fieldNodes = xPathDocNavigator?.Select("Fields/Field");
            if (fieldNodes != null)
                while (fieldNodes.MoveNext())
                {
                    string? name = fieldNodes.Current?.SelectSingleNode("Name")?.Value ?? throw new Exception("Не задана назва поля");
                    string fullName = fieldNodes.Current?.SelectSingleNode("FullName")?.Value ?? "";
                    string nameInTable = fieldNodes.Current?.SelectSingleNode("NameInTable")?.Value ?? "";
                    string type = fieldNodes.Current?.SelectSingleNode("Type")?.Value ?? "";
                    string desc = fieldNodes.Current?.SelectSingleNode("Desc")?.Value ?? "";

                    bool isPresentation = (parentName == "Directory" || parentName == "Document" || parentName == "RegisterInformation") && (fieldNodes.Current?.SelectSingleNode("IsPresentation")?.Value ?? "") == "1";
                    bool isIndex = (fieldNodes.Current?.SelectSingleNode("IsIndex")?.Value ?? "") == "1";
                    bool isFullTextSearch = (fieldNodes.Current?.SelectSingleNode("IsFullTextSearch")?.Value ?? "") == "1";
                    bool isSearch = (fieldNodes.Current?.SelectSingleNode("IsSearch")?.Value ?? "") == "1";
                    bool isExport = (fieldNodes.Current?.SelectSingleNode("IsExport")?.Value ?? "") == "1";
                    string pointer = (type == "pointer" || type == "enum") ? (fieldNodes.Current?.SelectSingleNode("Pointer")?.Value ?? "") : "";

                    ConfigurationField ConfObjectField = new(name, fullName, nameInTable, type, pointer, desc, isPresentation, isIndex,
                        isFullTextSearch, isSearch, isExport);

                    //
                    // Додаткові поля які залежать від типу
                    //

                    if (type == "string")
                    {
                        ConfObjectField.Multiline = (fieldNodes.Current?.SelectSingleNode("Multiline")?.Value ?? "") == "1";
                    }
                    else if (type == "integer")
                    {
                        ConfObjectField.AutomaticNumbering = (fieldNodes.Current?.SelectSingleNode("AutomaticNumbering")?.Value ?? "") == "1";
                    }
                    else if (type == "composite_pointer")
                    {
                        //Не використовувати довідники
                        ConfObjectField.CompositePointerNotUseDirectories = (fieldNodes.Current?.SelectSingleNode("CompositePointerNotUseDirectories")?.Value ?? "") == "1";

                        //Не використовувати документи
                        ConfObjectField.CompositePointerNotUseDocuments = (fieldNodes.Current?.SelectSingleNode("CompositePointerNotUseDocuments")?.Value ?? "") == "1";

                        //Внутрішня функція вибірки
                        List<string> Get(string nameAllowBlock)
                        {
                            List<string> listAllow = [];

                            XPathNodeIterator? allowNodes = fieldNodes.Current?.Select($"{nameAllowBlock}/Name");
                            if (allowNodes != null)
                                while (allowNodes.MoveNext())
                                {
                                    string? name = allowNodes.Current?.Value;
                                    if (name != null) listAllow.Add(name);
                                }

                            return listAllow;
                        }

                        ConfObjectField.CompositePointerAllowDirectories = Get("CompositePointerAllowDirectories");
                        ConfObjectField.CompositePointerAllowDocuments = Get("CompositePointerAllowDocuments");
                    }

                    fields.Add(name, ConfObjectField);
                }
        }

        /// <summary>
        /// Завантаження табличних частин
        /// </summary>
        /// <param name="Conf">Конфігурація</param>
        /// <param name="tabularParts">Колекція табличних частин</param>
        /// <param name="xPathDocNavigator">Навігатор</param>
        private static void LoadTabularParts(Configuration Conf, Dictionary<string, ConfigurationTablePart> tabularParts, XPathNavigator? xPathDocNavigator)
        {
            XPathNodeIterator? tablePartNodes = xPathDocNavigator?.Select("TabularParts/TablePart");
            if (tablePartNodes != null)
                while (tablePartNodes.MoveNext())
                {
                    string? name = tablePartNodes.Current?.SelectSingleNode("Name")?.Value ?? throw new Exception("Не задана назва табличної частини");
                    string fullName = tablePartNodes.Current?.SelectSingleNode("FullName")?.Value ?? "";
                    string table = tablePartNodes.Current?.SelectSingleNode("Table")?.Value ?? "";
                    string desc = tablePartNodes.Current?.SelectSingleNode("Desc")?.Value ?? "";
                    bool versionsHistory = (tablePartNodes.Current?.SelectSingleNode("VersionsHistory")?.Value ?? "") == "1";

                    ConfigurationTablePart ConfObjectTablePart = new(name, fullName, table, desc)
                    {
                        //Записувати історію версій
                        VersionsHistory = versionsHistory
                    };

                    tabularParts.Add(ConfObjectTablePart.Name, ConfObjectTablePart);

                    LoadFields(ConfObjectTablePart.Fields, tablePartNodes.Current, "TablePart");

                    if (Conf.VariantLoadConfiguration == VariantLoadConf.Full)
                    {
                        LoadTabularList(ConfObjectTablePart.TabularList, tablePartNodes.Current);

                        LoadTriggerTablePartFunctions(ConfObjectTablePart.TriggerFunctions, tablePartNodes.Current);

                        LoadForms(ConfObjectTablePart.Forms, tablePartNodes.Current);
                    }
                }
        }

        /// <summary>
        /// Завантаження табличних списків
        /// </summary>
        /// <param name="tabularLists">Колекція табличних списків</param>
        /// <param name="xPathDocNavigator">Навігатор</param>
        private static void LoadTabularList(Dictionary<string, ConfigurationTabularList> tabularLists, XPathNavigator? xPathDocNavigator)
        {
            XPathNodeIterator? tabularListsNodes = xPathDocNavigator?.Select("TabularLists/TabularList");
            if (tabularListsNodes != null)
                while (tabularListsNodes!.MoveNext())
                {
                    string? name = tabularListsNodes.Current?.SelectSingleNode("Name")?.Value ?? throw new Exception("Не задана назва табличного списку");
                    string desc = tabularListsNodes.Current?.SelectSingleNode("Desc")?.Value ?? "";

                    ConfigurationTabularList ConfTabularList = new(name, desc);
                    tabularLists.Add(ConfTabularList.Name, ConfTabularList);

                    //Поля
                    XPathNodeIterator? tabularListFieldNodes = tabularListsNodes?.Current?.Select("Fields/Field");
                    if (tabularListFieldNodes != null)
                        while (tabularListFieldNodes.MoveNext())
                        {
                            string? nameField = tabularListFieldNodes.Current?.SelectSingleNode("Name")?.Value ?? throw new Exception("Не задана назва поля табличного списку");
                            string captionField = tabularListFieldNodes.Current?.SelectSingleNode("Caption")?.Value ?? "";
                            uint sizeField = uint.Parse(tabularListFieldNodes.Current?.SelectSingleNode("Size")?.Value ?? "0");
                            int sortNumField = int.Parse(tabularListFieldNodes.Current?.SelectSingleNode("SortNum")?.Value ?? "100");
                            bool sortField = bool.Parse(tabularListFieldNodes.Current?.SelectSingleNode("SortField")?.Value ?? "False");
                            bool sortDirection = bool.Parse(tabularListFieldNodes.Current?.SelectSingleNode("SortDirection")?.Value ?? "False");
                            bool filterField = bool.Parse(tabularListFieldNodes.Current?.SelectSingleNode("FilterField")?.Value ?? "False");

                            ConfigurationTabularListField ConfTabularListField = new(nameField, captionField, sizeField, sortNumField, sortField, sortDirection, filterField);
                            ConfTabularList.Fields.Add(ConfTabularListField.Name, ConfTabularListField);
                        }

                    //Додаткові поля
                    XPathNodeIterator? tabularListAdditionalFieldsNodes = tabularListsNodes?.Current?.Select("Fields/AdditionalField");
                    if (tabularListAdditionalFieldsNodes != null)
                        while (tabularListAdditionalFieldsNodes.MoveNext())
                        {
                            bool visibleField = bool.Parse(tabularListAdditionalFieldsNodes.Current?.SelectSingleNode("Visible")?.Value ?? "False");
                            string nameField = tabularListAdditionalFieldsNodes.Current?.SelectSingleNode("Name")?.Value ?? "";
                            string captionField = tabularListAdditionalFieldsNodes.Current?.SelectSingleNode("Caption")?.Value ?? "";
                            uint sizeField = uint.Parse(tabularListAdditionalFieldsNodes.Current?.SelectSingleNode("Size")?.Value ?? "0");
                            int sortNumField = int.Parse(tabularListAdditionalFieldsNodes.Current?.SelectSingleNode("SortNum")?.Value ?? "100");
                            string typeField = tabularListAdditionalFieldsNodes.Current?.SelectSingleNode("Type")?.Value ?? "string";
                            string valueField = tabularListAdditionalFieldsNodes.Current?.SelectSingleNode("Value")?.Value ?? "";

                            ConfigurationTabularListAdditionalField ConfTabularListAdditionalField = new(visibleField, nameField, captionField, sizeField, sortNumField, typeField, valueField);
                            ConfTabularList.AdditionalFields.Add(ConfTabularListAdditionalField.Name, ConfTabularListAdditionalField);
                        }
                }
        }

        /// <summary>
        /// Завантаження табличних списків журналів
        /// </summary>
        /// <param name="tabularLists">Колекція табличних списків</param>
        /// <param name="xPathDocNavigator">Навігатор</param>
        private static void LoadJournalTabularList(Dictionary<string, ConfigurationTabularList> tabularLists, XPathNavigator? xPathDocNavigator)
        {
            XPathNodeIterator? tabularListsNodes = xPathDocNavigator?.Select("TabularLists/TabularList");
            if (tabularListsNodes != null)
                while (tabularListsNodes!.MoveNext())
                {
                    string? name = tabularListsNodes.Current?.SelectSingleNode("Name")?.Value ?? throw new Exception("Не задана назва табличного списку");
                    string desc = tabularListsNodes.Current?.SelectSingleNode("Desc")?.Value ?? "";

                    ConfigurationTabularList ConfTabularList = new(name, desc);
                    tabularLists.Add(ConfTabularList.Name, ConfTabularList);

                    XPathNodeIterator? tabularListFieldNodes = tabularListsNodes?.Current?.Select("Fields/Field");
                    if (tabularListFieldNodes != null)
                        while (tabularListFieldNodes.MoveNext())
                        {
                            string? nameField = tabularListFieldNodes.Current?.SelectSingleNode("Name")?.Value ?? throw new Exception("Не задана назва поля табличного списку");
                            string docField = tabularListFieldNodes.Current?.SelectSingleNode("DocField")?.Value ?? "";

                            ConfigurationTabularListField ConfTabularListField = new(nameField, docField);
                            ConfTabularList.Fields.Add(ConfTabularListField.Name, ConfTabularListField);
                        }
                }
        }

        /// <summary>
        /// Завантаження форм
        /// </summary>
        /// <param name="forms">Колекція форм</param>
        /// <param name="xPathDocNavigator">Навігатор</param>
        private static void LoadForms(Dictionary<string, ConfigurationForms> forms, XPathNavigator? xPathDocNavigator)
        {
            XPathNodeIterator? tableForm = xPathDocNavigator?.Select("Forms/Form");
            if (tableForm != null)
                while (tableForm.MoveNext())
                {
                    string? name = tableForm.Current?.SelectSingleNode("Name")?.Value ?? throw new Exception("Не задана назва форми");
                    string desc = tableForm.Current?.SelectSingleNode("Desc")?.Value ?? "";
                    string type = tableForm.Current?.SelectSingleNode("Type")?.Value ?? "";
                    string genCode = GetUnZipAndBase64TextFromNode(tableForm.Current?.SelectSingleNode("GeneratedCode"));

                    string notSaveToFile = tableForm.Current?.SelectSingleNode("NotSaveToFile")?.Value ?? "";

                    if (!Enum.TryParse(type, out ConfigurationForms.TypeForms typeForms))
                        typeForms = ConfigurationForms.TypeForms.None;

                    ConfigurationForms form = new(name, desc, typeForms)
                    {
                        NotSaveToFile = notSaveToFile == "1",
                        GeneratedCode = genCode
                    };

                    //Поля які залежать від типу форми
                    if (typeForms == ConfigurationForms.TypeForms.List ||
                        typeForms == ConfigurationForms.TypeForms.ListSmallSelect ||
                        typeForms == ConfigurationForms.TypeForms.ListAndTree)
                    {
                        form.TabularList = tableForm.Current?.SelectSingleNode("TabularList")?.Value ?? "";
                    }
                    else if (typeForms == ConfigurationForms.TypeForms.Element || typeForms == ConfigurationForms.TypeForms.TablePart)
                    {
                        LoadFormElementField(form.ElementFields, tableForm.Current);

                        if (typeForms == ConfigurationForms.TypeForms.TablePart)
                            form.IncludeIconColumn = (tableForm.Current?.SelectSingleNode("IncludeIconColumn")?.Value ?? "") == "1";
                        else
                            LoadFormElementTablePart(form.ElementTableParts, tableForm.Current);
                    }

                    forms.Add(form.Name, form);
                }
        }

        /// <summary>
        /// Завантаження полів форми
        /// </summary>
        /// <param name="elementFields">Колекція полів форми</param>
        /// <param name="xPathDocNavigator">Навігатор</param>
        private static void LoadFormElementField(Dictionary<string, ConfigurationFormsElementField> elementFields, XPathNavigator? xPathDocNavigator)
        {
            XPathNodeIterator? elementFieldNodes = xPathDocNavigator?.Select("ElementFields/Field");
            if (elementFieldNodes != null)
                while (elementFieldNodes.MoveNext())
                {
                    string? name = elementFieldNodes.Current?.SelectSingleNode("Name")?.Value ?? throw new Exception("Не задана назва поля");
                    string caption = elementFieldNodes.Current?.SelectSingleNode("Caption")?.Value ?? "";
                    uint sizeField = uint.Parse(elementFieldNodes.Current?.SelectSingleNode("Size")?.Value ?? "0");
                    uint heightField = uint.Parse(elementFieldNodes.Current?.SelectSingleNode("Height")?.Value ?? "0");
                    int sortNumField = int.Parse(elementFieldNodes.Current?.SelectSingleNode("SortNum")?.Value ?? "100");
                    bool multipleSelectField = (elementFieldNodes.Current?.SelectSingleNode("MultipleSelect")?.Value ?? "0") == "1";
                    bool readOnly = (elementFieldNodes.Current?.SelectSingleNode("ReadOnly")?.Value ?? "0") == "1";

                    elementFields.Add(name, new ConfigurationFormsElementField(name, caption, sizeField, heightField, sortNumField, multipleSelectField, readOnly));
                }
        }

        /// <summary>
        /// Завантаження полів табличної частини
        /// </summary>
        /// <param name="elementTableParts">Колекція полів</param>
        /// <param name="xPathDocNavigator">Навігатор</param>
        private static void LoadFormElementTablePart(Dictionary<string, ConfigurationFormsElementTablePart> elementTableParts, XPathNavigator? xPathDocNavigator)
        {
            XPathNodeIterator? elementTablePartNodes = xPathDocNavigator?.Select("ElementTableParts/TablePart");
            if (elementTablePartNodes != null)
                while (elementTablePartNodes.MoveNext())
                {
                    string? name = elementTablePartNodes.Current?.SelectSingleNode("Name")?.Value ?? throw new Exception("Не задана назва поля");
                    string caption = elementTablePartNodes.Current?.SelectSingleNode("Caption")?.Value ?? "";
                    uint sizeField = uint.Parse(elementTablePartNodes.Current?.SelectSingleNode("Size")?.Value ?? "0");
                    uint heightField = uint.Parse(elementTablePartNodes.Current?.SelectSingleNode("Height")?.Value ?? "0");
                    int sortNumField = int.Parse(elementTablePartNodes.Current?.SelectSingleNode("SortNum")?.Value ?? "100");

                    elementTableParts.Add(name, new ConfigurationFormsElementTablePart(name, caption, sizeField, heightField, sortNumField));
                }
        }

        /// <summary>
        /// Завантаження колекції доступних регістрів для документу
        /// </summary>
        /// <param name="allowRegisterAccumulation">Список доступних регістрів накопичення</param>
        /// <param name="xPathDocNavigator">Навігатор</param>
        private static void LoadAllowRegisterAccumulation(List<string> allowRegisterAccumulation, XPathNavigator? xPathDocNavigator)
        {
            XPathNodeIterator? allowRegisterAccumulationNodes = xPathDocNavigator?.Select("AllowRegisterAccumulation/Name");
            if (allowRegisterAccumulationNodes != null)
                while (allowRegisterAccumulationNodes.MoveNext())
                {
                    string? name = allowRegisterAccumulationNodes.Current?.Value ?? throw new Exception("Не задана назва доступного регістру");
                    allowRegisterAccumulation.Add(name);
                }
        }

        /// <summary>
        /// Завантаження функцій тригерів
        /// </summary>
        /// <param name="triggerFunctions">Функції тригерів</param>
        /// <param name="xPathDocNavigator">Навігатор</param>
        private static void LoadTriggerFunctions(ConfigurationTriggerFunctions triggerFunctions, XPathNavigator? xPathDocNavigator)
        {
            XPathNavigator? nodeTriggerFunctions = xPathDocNavigator?.SelectSingleNode("TriggerFunctions");
            if (nodeTriggerFunctions != null)
            {
                var nodeNew = nodeTriggerFunctions.SelectSingleNode("New");
                triggerFunctions.New = nodeNew?.Value ?? "";
                triggerFunctions.NewAction = nodeNew?.GetAttribute("Action", "") == "1";

                var nodeCopying = nodeTriggerFunctions.SelectSingleNode("Copying");
                triggerFunctions.Copying = nodeCopying?.Value ?? "";
                triggerFunctions.CopyingAction = nodeCopying?.GetAttribute("Action", "") == "1";

                var nodeBeforeSave = nodeTriggerFunctions.SelectSingleNode("BeforeSave");
                triggerFunctions.BeforeSave = nodeBeforeSave?.Value ?? "";
                triggerFunctions.BeforeSaveAction = nodeBeforeSave?.GetAttribute("Action", "") == "1";

                var nodeAfterSave = nodeTriggerFunctions.SelectSingleNode("AfterSave");
                triggerFunctions.AfterSave = nodeAfterSave?.Value ?? "";
                triggerFunctions.AfterSaveAction = nodeAfterSave?.GetAttribute("Action", "") == "1";

                var nodeSetDeletionLabel = nodeTriggerFunctions.SelectSingleNode("SetDeletionLabel");
                triggerFunctions.SetDeletionLabel = nodeSetDeletionLabel?.Value ?? "";
                triggerFunctions.SetDeletionLabelAction = nodeSetDeletionLabel?.GetAttribute("Action", "") == "1";

                var nodeBeforeDelete = nodeTriggerFunctions.SelectSingleNode("BeforeDelete");
                triggerFunctions.BeforeDelete = nodeBeforeDelete?.Value ?? "";
                triggerFunctions.BeforeDeleteAction = nodeBeforeDelete?.GetAttribute("Action", "") == "1";
            }
        }

        /// <summary>
        /// Завантаження функцій тригерів для таб частини
        /// </summary>
        /// <param name="triggerFunctions">Функції тригерів</param>
        /// <param name="xPathDocNavigator">Навігатор</param>
        private static void LoadTriggerTablePartFunctions(ConfigurationTriggerTablePartFunctions triggerFunctions, XPathNavigator? xPathDocNavigator)
        {
            XPathNavigator? nodeTriggerFunctions = xPathDocNavigator?.SelectSingleNode("TriggerFunctions");
            if (nodeTriggerFunctions != null)
            {
                var nodeBeforeSave = nodeTriggerFunctions.SelectSingleNode("BeforeSave");
                triggerFunctions.BeforeSave = nodeBeforeSave?.Value ?? "";
                triggerFunctions.BeforeSaveAction = nodeBeforeSave?.GetAttribute("Action", "") == "1";

                var nodeAfterSave = nodeTriggerFunctions.SelectSingleNode("AfterSave");
                triggerFunctions.AfterSave = nodeAfterSave?.Value ?? "";
                triggerFunctions.AfterSaveAction = nodeAfterSave?.GetAttribute("Action", "") == "1";
            }
        }

        /// <summary>
        /// Завантаження функцій проведення для документу
        /// </summary>
        /// <param name="triggerFunctions">Функції проведення</param>
        /// <param name="xPathDocNavigator">Навігатор</param>
        private static void LoadSpendFunctions(ConfigurationSpendFunctions spendFunctions, XPathNavigator? xPathDocNavigator)
        {
            XPathNavigator? nodeSpendFunctions = xPathDocNavigator?.SelectSingleNode("SpendFunctions");
            if (nodeSpendFunctions != null)
            {
                var nodeSpend = nodeSpendFunctions.SelectSingleNode("Spend");
                spendFunctions.Spend = nodeSpend?.Value ?? "";
                spendFunctions.SpendAction = nodeSpend?.GetAttribute("Action", "") == "1";

                var nodeClearSpend = nodeSpendFunctions.SelectSingleNode("ClearSpend");
                spendFunctions.ClearSpend = nodeClearSpend?.Value ?? "";
                spendFunctions.ClearSpendAction = nodeClearSpend?.GetAttribute("Action", "") == "1";
            }
        }

        /// <summary>
        /// Завантаження перелічень
        /// </summary>
        /// <param name="Conf">Конфігурація</param>
        /// <param name="xPathDocNavigator">Навігатор</param>
        private static void LoadEnums(Configuration Conf, XPathNavigator? xPathDocNavigator)
        {
            //Перелічення
            XPathNodeIterator? enumsNodes = xPathDocNavigator?.Select("/Configuration/Enums/Enum");
            if (enumsNodes != null)
                while (enumsNodes!.MoveNext())
                {
                    string? name = enumsNodes.Current?.SelectSingleNode("Name")?.Value ?? throw new Exception("Не задана назва перелічення");
                    string desc = enumsNodes.Current?.SelectSingleNode("Desc")?.Value ?? "";
                    int serialNumber = int.Parse(enumsNodes.Current?.SelectSingleNode("SerialNumber")?.Value ?? "0");

                    ConfigurationEnums configurationEnums = new(name, serialNumber, desc);
                    Conf.Enums.Add(configurationEnums.Name, configurationEnums);

                    XPathNodeIterator? enumFieldsNodes = enumsNodes?.Current?.Select("Fields/Field");
                    if (enumFieldsNodes != null)
                        while (enumFieldsNodes.MoveNext())
                        {
                            string? nameField = enumFieldsNodes.Current?.SelectSingleNode("Name")?.Value ?? throw new Exception("Не задана назва елементу перелічення");
                            int valueField = int.Parse(enumFieldsNodes.Current?.SelectSingleNode("Value")?.Value ?? "0");
                            string descField = enumFieldsNodes.Current?.SelectSingleNode("Desc")?.Value ?? "";

                            configurationEnums.AppendField(new ConfigurationEnumField(nameField, valueField, descField));
                        }
                }
        }

        /// <summary>
        /// Завантаження журналів
        /// </summary>
        /// <param name="Conf">Конфігурація</param>
        /// <param name="xPathDocNavigator">Навігатор</param>
        private static void LoadJournals(Configuration Conf, XPathNavigator? xPathDocNavigator)
        {
            //Журнали
            XPathNodeIterator? journalsNodes = xPathDocNavigator?.Select("/Configuration/Journals/Journal");
            if (journalsNodes != null)
                while (journalsNodes!.MoveNext())
                {
                    string? name = journalsNodes.Current?.SelectSingleNode("Name")?.Value ?? throw new Exception("Не задана назва журналу");
                    string desc = journalsNodes.Current?.SelectSingleNode("Desc")?.Value ?? "";

                    ConfigurationJournals configurationJournals = new(name, desc);
                    Conf.Journals.Add(configurationJournals.Name, configurationJournals);

                    XPathNodeIterator? journalFieldsNodes = journalsNodes?.Current?.Select("Fields/Field");
                    if (journalFieldsNodes != null)
                        while (journalFieldsNodes.MoveNext())
                        {
                            string? nameField = journalFieldsNodes.Current?.SelectSingleNode("Name")?.Value ?? throw new Exception("Не задана назва поля журналу");
                            string descField = journalFieldsNodes.Current?.SelectSingleNode("Desc")?.Value ?? "";
                            string sortType = journalFieldsNodes.Current?.SelectSingleNode("Type")?.Value ?? "";
                            string sortField = journalFieldsNodes.Current?.SelectSingleNode("SortField")?.Value ?? "";
                            string sortWherePeriod = journalFieldsNodes.Current?.SelectSingleNode("WherePeriod")?.Value ?? "";

                            bool isSort = sortField == "1";
                            bool isWherePeriod = sortWherePeriod == "1";

                            configurationJournals.AppendField(new ConfigurationJournalField(nameField, descField, sortType, isSort, isWherePeriod));
                        }

                    LoadJournalsAllowDocument(configurationJournals.AllowDocuments, journalsNodes?.Current);

                    LoadJournalTabularList(configurationJournals.TabularList, journalsNodes?.Current);
                }
        }

        /// <summary>
        /// Завантаження документів які входить в журнал
        /// </summary>
        /// <param name="allowDocument">Колекція документів</param>
        /// <param name="xPathDocNavigator">Навігатор</param>
        private static void LoadJournalsAllowDocument(List<string> allowDocument, XPathNavigator? xPathDocNavigator)
        {
            XPathNodeIterator? allowDocumentNodes = xPathDocNavigator?.Select("AllowDocument/Item");
            if (allowDocumentNodes != null)
                while (allowDocumentNodes.MoveNext())
                {
                    string? name = allowDocumentNodes.Current?.SelectSingleNode("Name")?.Value ?? throw new Exception("Не задана назва документу");
                    allowDocument.Add(name);
                }
        }

        /// <summary>
        /// Завантаження документів
        /// </summary>
        /// <param name="Conf">Конфігурація</param>
        /// <param name="xPathDocNavigator">Навігатор</param>
        private static void LoadDocuments(Configuration Conf, XPathNavigator? xPathDocNavigator)
        {
            //Документи
            XPathNodeIterator? documentsNode = xPathDocNavigator?.Select("/Configuration/Documents/Document");
            if (documentsNode != null)
                while (documentsNode!.MoveNext())
                {
                    string? name = documentsNode.Current?.SelectSingleNode("Name")?.Value ?? throw new Exception("Не задана назва документу");
                    string fullName = documentsNode.Current?.SelectSingleNode("FullName")?.Value ?? "";
                    string table = documentsNode.Current?.SelectSingleNode("Table")?.Value ?? "";
                    string desc = documentsNode.Current?.SelectSingleNode("Desc")?.Value ?? "";
                    bool autoNum = (documentsNode.Current?.SelectSingleNode("AutoNum")?.Value ?? "") == "1";
                    bool versionsHistory = (documentsNode.Current?.SelectSingleNode("VersionsHistory")?.Value ?? "") == "1";
                    bool exportXml = (documentsNode.Current?.SelectSingleNode("ExportXml")?.Value ?? "") == "1";

                    ConfigurationDocuments configurationDocuments = new(name, fullName, table, desc, autoNum, exportXml) { VersionsHistory = versionsHistory };

                    Conf.Documents.Add(configurationDocuments.Name, configurationDocuments);

                    LoadFields(configurationDocuments.Fields, documentsNode?.Current, "Document");

                    LoadTabularParts(Conf, configurationDocuments.TabularParts, documentsNode?.Current);

                    LoadAllowRegisterAccumulation(configurationDocuments.AllowRegisterAccumulation, documentsNode?.Current);

                    if (Conf.VariantLoadConfiguration == VariantLoadConf.Full)
                    {
                        LoadTabularList(configurationDocuments.TabularList, documentsNode?.Current);

                        LoadTriggerFunctions(configurationDocuments.TriggerFunctions, documentsNode?.Current);

                        LoadSpendFunctions(configurationDocuments.SpendFunctions, documentsNode?.Current);

                        LoadForms(configurationDocuments.Forms, documentsNode?.Current);
                    }
                }
        }

        /// <summary>
        /// Завантаження регістрів інформації
        /// </summary>
        /// <param name="Conf">Конфігурація</param>
        /// <param name="xPathDocNavigator">Навігатор</param>
        private static void LoadRegistersInformation(Configuration Conf, XPathNavigator? xPathDocNavigator)
        {
            //Регістри відомостей
            XPathNodeIterator? registerInformationNode = xPathDocNavigator?.Select("/Configuration/RegistersInformation/RegisterInformation");
            if (registerInformationNode != null)
                while (registerInformationNode!.MoveNext())
                {
                    string? name = registerInformationNode.Current?.SelectSingleNode("Name")?.Value ?? throw new Exception("Не задана назва регістру відомостей");
                    string fullName = registerInformationNode.Current?.SelectSingleNode("FullName")?.Value ?? "";
                    string table = registerInformationNode.Current?.SelectSingleNode("Table")?.Value ?? "";
                    string desc = registerInformationNode.Current?.SelectSingleNode("Desc")?.Value ?? "";

                    ConfigurationRegistersInformation configurationRegistersInformation = new(name, fullName, table, desc);
                    Conf.RegistersInformation.Add(configurationRegistersInformation.Name, configurationRegistersInformation);

                    XPathNavigator? dimensionFieldsNode = registerInformationNode.Current?.SelectSingleNode("DimensionFields");
                    if (dimensionFieldsNode != null)
                        LoadFields(configurationRegistersInformation.DimensionFields, dimensionFieldsNode, "RegisterInformation");

                    XPathNavigator? resourcesFieldsNode = registerInformationNode.Current?.SelectSingleNode("ResourcesFields");
                    if (resourcesFieldsNode != null)
                        LoadFields(configurationRegistersInformation.ResourcesFields, resourcesFieldsNode, "RegisterInformation");

                    XPathNavigator? propertyFieldsNode = registerInformationNode.Current?.SelectSingleNode("PropertyFields");
                    if (propertyFieldsNode != null)
                        LoadFields(configurationRegistersInformation.PropertyFields, propertyFieldsNode, "RegisterInformation");

                    if (Conf.VariantLoadConfiguration == VariantLoadConf.Full)
                    {
                        LoadTabularList(configurationRegistersInformation.TabularList, registerInformationNode?.Current);

                        LoadForms(configurationRegistersInformation.Forms, registerInformationNode?.Current);
                    }
                }
        }

        /// <summary>
        /// Завантаження документів які можуть робити проводки в регістрі
        /// </summary>
        /// <param name="allowDocumentSpend">Колекція документів</param>
        /// <param name="xPathDocNavigator">Навігатор</param>
        private static void LoadAllowDocumentSpendRegisterAccumulation(List<string> allowDocumentSpend, XPathNavigator? xPathDocNavigator)
        {
            XPathNodeIterator? allowDocumentSpendNodes = xPathDocNavigator?.Select("AllowDocumentSpend/Name");
            if (allowDocumentSpendNodes != null)
                while (allowDocumentSpendNodes.MoveNext())
                {
                    string? name = allowDocumentSpendNodes.Current?.Value;
                    if (name != null) allowDocumentSpend.Add(name);
                }
        }

        /// <summary>
        /// Завантаження регістрів накопичення
        /// </summary>
        /// <param name="Conf">Конфігурація</param>
        /// <param name="xPathDocNavigator">Навігатор</param>
        private static void LoadRegistersAccumulation(Configuration Conf, XPathNavigator? xPathDocNavigator)
        {
            //Регістри накопичення
            XPathNodeIterator? registerAccumulationNode = xPathDocNavigator?.Select("/Configuration/RegistersAccumulation/RegisterAccumulation");
            if (registerAccumulationNode != null)
                while (registerAccumulationNode!.MoveNext())
                {
                    string? name = registerAccumulationNode.Current?.SelectSingleNode("Name")?.Value ?? throw new Exception("Не задана назва регістру накопичення");
                    string fullName = registerAccumulationNode.Current?.SelectSingleNode("FullName")?.Value ?? "";
                    string table = registerAccumulationNode.Current?.SelectSingleNode("Table")?.Value ?? "";
                    string type = registerAccumulationNode.Current?.SelectSingleNode("Type")?.Value ?? "";
                    string desc = registerAccumulationNode.Current?.SelectSingleNode("Desc")?.Value ?? "";
                    string noSummary = registerAccumulationNode.Current?.SelectSingleNode("NoSummary")?.Value ?? "";

                    //Тип регістру
                    TypeRegistersAccumulation typeRegistersAccumulation = type switch
                    {
                        "Residues" => TypeRegistersAccumulation.Residues,
                        "Turnover" => TypeRegistersAccumulation.Turnover,
                        _ => throw new Exception("Не оприділений тип регістру")
                    };

                    ConfigurationRegistersAccumulation configurationRegistersAccumulation =
                        new(name, fullName, table, typeRegistersAccumulation, desc) { NoSummary = noSummary == "1" };

                    Conf.RegistersAccumulation.Add(configurationRegistersAccumulation.Name, configurationRegistersAccumulation);

                    XPathNavigator? dimensionFieldsNode = registerAccumulationNode.Current?.SelectSingleNode("DimensionFields");
                    if (dimensionFieldsNode != null)
                        LoadFields(configurationRegistersAccumulation.DimensionFields, dimensionFieldsNode, "RegisterAccumulation");

                    XPathNavigator? resourcesFieldsNode = registerAccumulationNode.Current?.SelectSingleNode("ResourcesFields");
                    if (resourcesFieldsNode != null)
                        LoadFields(configurationRegistersAccumulation.ResourcesFields, resourcesFieldsNode, "RegisterAccumulation");

                    XPathNavigator? propertyFieldsNode = registerAccumulationNode.Current?.SelectSingleNode("PropertyFields");
                    if (propertyFieldsNode != null)
                        LoadFields(configurationRegistersAccumulation.PropertyFields, propertyFieldsNode, "RegisterAccumulation");

                    LoadAllowDocumentSpendRegisterAccumulation(configurationRegistersAccumulation.AllowDocumentSpend, registerAccumulationNode.Current);

                    LoadTabularParts(Conf, configurationRegistersAccumulation.TabularParts, registerAccumulationNode.Current);

                    if (Conf.VariantLoadConfiguration == VariantLoadConf.Full)
                    {
                        LoadQueryList(configurationRegistersAccumulation.QueryBlockList, registerAccumulationNode.Current);
                        LoadTabularList(configurationRegistersAccumulation.TabularList, registerAccumulationNode.Current);
                        LoadForms(configurationRegistersAccumulation.Forms, registerAccumulationNode.Current);

                        XPathNavigator? printBalancesNode = registerAccumulationNode.Current?.SelectSingleNode("PrintBalances");
                        if (printBalancesNode != null)
                            LoadTabularList(configurationRegistersAccumulation.TabularListPrintBalances, printBalancesNode);
                    }
                }
        }

        /// <summary>
        /// Завантаження списку запитів
        /// </summary>
        /// <param name="queryBlockList">Колекція запитів</param>
        /// <param name="xPathDocNavigator">Навігатор</param>
        private static void LoadQueryList(Dictionary<string, ConfigurationQueryBlock> queryBlockList, XPathNavigator? xPathDocNavigator)
        {
            XPathNodeIterator? nodeQueryBlock = xPathDocNavigator?.Select("QueryBlockList/QueryBlock");
            if (nodeQueryBlock != null)
                while (nodeQueryBlock!.MoveNext())
                {
                    string? name = nodeQueryBlock.Current?.SelectSingleNode("Name")?.Value ?? throw new Exception("Не задана назва запиту");
                    bool finalCalculation = (nodeQueryBlock.Current?.SelectSingleNode("FinalCalculation")?.Value ?? "") == "1";

                    ConfigurationQueryBlock QueryBlock = new(name, finalCalculation);
                    queryBlockList.Add(QueryBlock.Name, QueryBlock);

                    XPathNodeIterator? nodeQuery = nodeQueryBlock?.Current?.Select("Query");
                    if (nodeQuery != null)
                        while (nodeQuery.MoveNext())
                        {
                            string key = nodeQuery.Current?.GetAttribute("key", "") ?? "";
                            string query = nodeQuery.Current?.Value ?? "";

                            QueryBlock.Query.Add(key, query);
                        }
                }
        }

        #endregion

        #region Save (Збереження конфігурації в ХМЛ файл)

        /// <summary>
        /// Збереження конфігурації в ХМЛ файл
        /// </summary>
        /// <param name="pathToConf">Шлях до ХМЛ файлу конфігурації</param>
        /// <param name="Conf">Конфігурація</param>
        public static void Save(string pathToConf, Configuration Conf)
        {
            //Перерахувати документи які роблять рухи по регістрах накопичення
            Conf.CalculateAllowDocumentSpendForRegistersAccumulation();

            XmlDocument xmlConfDocument = new();
            xmlConfDocument.AppendChild(xmlConfDocument.CreateXmlDeclaration("1.0", "utf-8", ""));

            XmlElement rootNode = xmlConfDocument.CreateElement("Configuration");
            xmlConfDocument.AppendChild(rootNode);

            SaveConfigurationInfo(Conf, xmlConfDocument, rootNode);

            SaveConstantsBlock(Conf, Conf.ConstantsBlock, xmlConfDocument, rootNode);

            SaveDirectories(Conf, Conf.Directories, xmlConfDocument, rootNode);

            SaveDocuments(Conf, Conf.Documents, xmlConfDocument, rootNode);

            SaveEnums(Conf.Enums, xmlConfDocument, rootNode);

            SaveJournals(Conf, Conf.Journals, xmlConfDocument, rootNode);

            SaveRegistersInformation(Conf, Conf.RegistersInformation, xmlConfDocument, rootNode);

            SaveRegistersAccumulation(Conf, Conf.RegistersAccumulation, xmlConfDocument, rootNode);

            xmlConfDocument.Save(pathToConf);
        }

        /// <summary>
        /// Збереження загальної інформації
        /// </summary>
        /// <param name="Conf">Конфігурація</param>
        /// <param name="xmlConfDocument">Документ</param>
        /// <param name="rootNode">Корінна вітка</param>
        private static void SaveConfigurationInfo(Configuration Conf, XmlDocument xmlConfDocument, XmlElement rootNode)
        {
            XmlElement nodeName = xmlConfDocument.CreateElement("Name");
            nodeName.InnerText = Conf.Name;
            rootNode.AppendChild(nodeName);

            XmlElement nodeSubtitle = xmlConfDocument.CreateElement("Subtitle");
            nodeSubtitle.InnerText = Conf.Subtitle;
            rootNode.AppendChild(nodeSubtitle);

            XmlElement nodeNameSpaceGeneratedCode = xmlConfDocument.CreateElement("NameSpaceGeneratedCode");
            nodeNameSpaceGeneratedCode.InnerText = Conf.NameSpaceGeneratedCode;
            rootNode.AppendChild(nodeNameSpaceGeneratedCode);

            XmlElement nodeNameSpace = xmlConfDocument.CreateElement("NameSpace");
            nodeNameSpace.InnerText = Conf.NameSpace;
            rootNode.AppendChild(nodeNameSpace);

            XmlElement nodeAuthor = xmlConfDocument.CreateElement("Author");
            nodeAuthor.InnerText = Conf.Author;
            rootNode.AppendChild(nodeAuthor);

            if (!string.IsNullOrEmpty(Conf.Desc))
            {
                XmlElement nodeDesc = xmlConfDocument.CreateElement("Desc");
                nodeDesc.InnerText = Conf.Desc;
                rootNode.AppendChild(nodeDesc);
            }

            XmlElement nodeDateTimeSave = xmlConfDocument.CreateElement("DateTimeSave");
            nodeDateTimeSave.InnerText = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            rootNode.AppendChild(nodeDateTimeSave);

            XmlElement nodeDictTSearch = xmlConfDocument.CreateElement("DictTSearch");
            nodeDictTSearch.InnerText = !string.IsNullOrEmpty(Conf.DictTSearch) ? Conf.DictTSearch : DefaultDictTSearch;
            rootNode.AppendChild(nodeDictTSearch);

            XmlElement nodeGtkVersion = xmlConfDocument.CreateElement("GtkVersion");
            nodeGtkVersion.InnerText = Conf.GtkLibVersion.ToString();
            rootNode.AppendChild(nodeGtkVersion);
        }

        /// <summary>
        /// Зберегти блоки констант
        /// </summary>
        /// <param name="Conf">Конфігурація</param>
        /// <param name="ConfConstantsBlocks">Блок</param>
        /// <param name="xmlConfDocument">Документ</param>
        /// <param name="rootNode">Корінна вітка</param>
        private static void SaveConstantsBlock(Configuration Conf, Dictionary<string, ConfigurationConstantsBlock> ConfConstantsBlocks, XmlDocument xmlConfDocument, XmlElement rootNode)
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

                if (!string.IsNullOrEmpty(ConfConstantsBlock.Value.Desc))
                {
                    XmlElement nodeDesc = xmlConfDocument.CreateElement("Desc");
                    nodeDesc.InnerText = ConfConstantsBlock.Value.Desc;
                    rootConstantsBlock.AppendChild(nodeDesc);
                }

                SaveConstants(Conf, ConfConstantsBlock.Value.Constants, xmlConfDocument, rootConstantsBlock);
            }
        }

        /// <summary>
        /// Зберегти константи
        /// </summary>
        /// <param name="Conf">Конфігурація</param>
        /// <param name="ConfConstants">Колекція констант</param>
        /// <param name="xmlConfDocument">Документ</param>
        /// <param name="rootNode">Корінна вітка</param>
        private static void SaveConstants(Configuration Conf, Dictionary<string, ConfigurationConstants> ConfConstants, XmlDocument xmlConfDocument, XmlElement rootNode)
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

                if (!string.IsNullOrEmpty(ConfConstant.Value.Desc))
                {
                    XmlElement nodeDesc = xmlConfDocument.CreateElement("Desc");
                    nodeDesc.InnerText = ConfConstant.Value.Desc;
                    rootConstant.AppendChild(nodeDesc);
                }

                XmlElement nodeType = xmlConfDocument.CreateElement("Type");
                nodeType.InnerText = ConfConstant.Value.Type;
                rootConstant.AppendChild(nodeType);

                if (ConfConstant.Value.Type == "pointer" || ConfConstant.Value.Type == "enum")
                {
                    XmlElement nodePointer = xmlConfDocument.CreateElement("Pointer");
                    nodePointer.InnerText = ConfConstant.Value.Pointer;
                    rootConstant.AppendChild(nodePointer);
                }

                SaveTabularParts(Conf, ConfConstant.Value.TabularParts, xmlConfDocument, rootConstant);
            }
        }

        /// <summary>
        /// Зберегти довідники
        /// </summary>
        /// <param name="Conf">Конфігурація</param>
        /// <param name="ConfDirectories">Колекція довідників</param>
        /// <param name="xmlConfDocument">Документ</param>
        /// <param name="rootNode">Корінна вітка</param>
        private static void SaveDirectories(Configuration Conf, Dictionary<string, ConfigurationDirectories> ConfDirectories, XmlDocument xmlConfDocument, XmlElement rootNode)
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

                XmlElement nodeDirectoryFullName = xmlConfDocument.CreateElement("FullName");
                nodeDirectoryFullName.InnerText = ConfDirectory.Value.FullName;
                nodeDirectory.AppendChild(nodeDirectoryFullName);

                XmlElement nodeDirectoryTable = xmlConfDocument.CreateElement("Table");
                nodeDirectoryTable.InnerText = ConfDirectory.Value.Table;
                nodeDirectory.AppendChild(nodeDirectoryTable);

                if (!string.IsNullOrEmpty(ConfDirectory.Value.Desc))
                {
                    XmlElement nodeDirectoryDesc = xmlConfDocument.CreateElement("Desc");
                    nodeDirectoryDesc.InnerText = ConfDirectory.Value.Desc.Trim();
                    nodeDirectory.AppendChild(nodeDirectoryDesc);
                }

                XmlElement nodeDirectoryAutoNum = xmlConfDocument.CreateElement("AutoNum");
                nodeDirectoryAutoNum.InnerText = ConfDirectory.Value.AutomaticNumeration ? "1" : "0";
                nodeDirectory.AppendChild(nodeDirectoryAutoNum);

                XmlElement nodeDirectoryVersionsHistory = xmlConfDocument.CreateElement("VersionsHistory");
                nodeDirectoryVersionsHistory.InnerText = ConfDirectory.Value.VersionsHistory ? "1" : "0";
                nodeDirectory.AppendChild(nodeDirectoryVersionsHistory);

                XmlElement nodeDirectoryType = xmlConfDocument.CreateElement("Type");
                nodeDirectoryType.InnerText = ConfDirectory.Value.TypeDirectory.ToString();
                nodeDirectory.AppendChild(nodeDirectoryType);

                if (ConfDirectory.Value.TypeDirectory == ConfigurationDirectories.TypeDirectories.HierarchyInAnotherDirectory)
                {
                    XmlElement nodeDirectoryPointerFolders = xmlConfDocument.CreateElement("PointerFolders");
                    nodeDirectoryPointerFolders.InnerText = ConfDirectory.Value.PointerFolders_HierarchyInAnotherDirectory;
                    nodeDirectory.AppendChild(nodeDirectoryPointerFolders);
                }
                else if (ConfDirectory.Value.TypeDirectory == ConfigurationDirectories.TypeDirectories.Hierarchical)
                {
                    XmlElement nodeDirectoryParentField = xmlConfDocument.CreateElement("ParentField");
                    nodeDirectoryParentField.InnerText = ConfDirectory.Value.ParentField_Hierarchical;
                    nodeDirectory.AppendChild(nodeDirectoryParentField);

                    XmlElement nodeDirectoryIconTree = xmlConfDocument.CreateElement("IconTree");
                    nodeDirectoryIconTree.InnerText = ConfDirectory.Value.IconTree_Hierarchical;
                    nodeDirectory.AppendChild(nodeDirectoryIconTree);

                    XmlElement nodeDirectoryAllowedContent = xmlConfDocument.CreateElement("AllowedContent");
                    nodeDirectoryAllowedContent.InnerText = ConfDirectory.Value.AllowedContent_Hierarchical.ToString();
                    nodeDirectory.AppendChild(nodeDirectoryAllowedContent);

                    //Для типу вмісту Папки та елементи потрібне поле ЦеПапка
                    if (ConfDirectory.Value.AllowedContent_Hierarchical == ConfigurationDirectories.HierarchicalContentType.FoldersAndElements)
                    {
                        XmlElement nodeDirectoryIsFolderField = xmlConfDocument.CreateElement("IsFolderField");
                        nodeDirectoryIsFolderField.InnerText = ConfDirectory.Value.IsFolderField_Hierarchical;
                        nodeDirectory.AppendChild(nodeDirectoryIsFolderField);
                    }
                }

                if (!string.IsNullOrEmpty(ConfDirectory.Value.DirectoryOwner_Subordination))
                {
                    XmlElement nodeDirectoryOwner = xmlConfDocument.CreateElement("DirectoryOwner");
                    nodeDirectoryOwner.InnerText = ConfDirectory.Value.DirectoryOwner_Subordination;
                    nodeDirectory.AppendChild(nodeDirectoryOwner);

                    XmlElement nodePointerFieldOwner = xmlConfDocument.CreateElement("PointerFieldOwner");
                    nodePointerFieldOwner.InnerText = ConfDirectory.Value.PointerFieldOwner_Subordination;
                    nodeDirectory.AppendChild(nodePointerFieldOwner);
                }

                SavePredefinedFields(ConfigurationDirectories.GetPredefinedFields(), xmlConfDocument, nodeDirectory);

                SaveFields(ConfDirectory.Value.Fields, xmlConfDocument, nodeDirectory, "Directory");

                SaveTabularParts(Conf, ConfDirectory.Value.TabularParts, xmlConfDocument, nodeDirectory);

                SaveTabularList(ConfDirectory.Value.Fields, ConfDirectory.Value.TabularList, xmlConfDocument, nodeDirectory);

                SaveTriggerFunctions(ConfDirectory.Value.TriggerFunctions, xmlConfDocument, nodeDirectory);

                SaveForms(Conf, ConfDirectory.Value.Fields, ConfDirectory.Value.TabularParts, ConfDirectory.Value.Forms, xmlConfDocument, nodeDirectory);
            }
        }

        /// <summary>
        /// Збереження попередньо визначених полів
        /// </summary>
        /// <param name="predefinedFields">Масив полів</param>
        /// <param name="xmlConfDocument">документ</param>
        /// <param name="rootNode">Корінна вітка</param>
        private static void SavePredefinedFields(ConfigurationPredefinedField[] predefinedFields, XmlDocument xmlConfDocument, XmlElement rootNode)
        {
            /*
            Попередньо визначені поля
            */

            XmlElement nodePredefinedFields = xmlConfDocument.CreateElement("PredefinedFields");
            rootNode.AppendChild(nodePredefinedFields);

            foreach (ConfigurationPredefinedField predefinedField in predefinedFields)
            {
                XmlElement nodePredefinedField = xmlConfDocument.CreateElement("PredefinedField");
                nodePredefinedFields.AppendChild(nodePredefinedField);

                XmlElement nodePredefinedFieldName = xmlConfDocument.CreateElement("Name");
                nodePredefinedFieldName.InnerText = predefinedField.NameInTable;
                nodePredefinedField.AppendChild(nodePredefinedFieldName);

                XmlElement nodePredefinedFieldNameInTable = xmlConfDocument.CreateElement("NameInTable");
                nodePredefinedFieldNameInTable.InnerText = predefinedField.NameInTable;
                nodePredefinedField.AppendChild(nodePredefinedFieldNameInTable);

                XmlElement nodePredefinedFieldType = xmlConfDocument.CreateElement("Type");
                nodePredefinedFieldType.InnerText = predefinedField.Type;
                nodePredefinedField.AppendChild(nodePredefinedFieldType);

                XmlElement nodePredefinedFieldIsPrimaryKey = xmlConfDocument.CreateElement("IsPrimaryKey");
                nodePredefinedFieldIsPrimaryKey.InnerText = predefinedField.IsPrimaryKey ? "1" : "0";
                nodePredefinedField.AppendChild(nodePredefinedFieldIsPrimaryKey);

                XmlElement nodePredefinedFieldIsIndex = xmlConfDocument.CreateElement("IsIndex");
                nodePredefinedFieldIsIndex.InnerText = predefinedField.IsIndex ? "1" : "0";
                nodePredefinedField.AppendChild(nodePredefinedFieldIsIndex);

                XmlElement nodePredefinedFieldIsNotNull = xmlConfDocument.CreateElement("IsNotNull");
                nodePredefinedFieldIsNotNull.InnerText = predefinedField.IsNotNull ? "1" : "0";
                nodePredefinedField.AppendChild(nodePredefinedFieldIsNotNull);
            }
        }

        /// <summary>
        /// Збереження полів
        /// </summary>
        /// <param name="fields">Колекція полів</param>
        /// <param name="xmlConfDocument">Документ</param>
        /// <param name="rootNode">Корінна вітка</param>
        /// <param name="parentName">Власник полів</param>
        public static void SaveFields(Dictionary<string, ConfigurationField> fields, XmlDocument xmlConfDocument, XmlElement rootNode, string parentName = "")
        {
            XmlElement nodeFields = xmlConfDocument.CreateElement("Fields");
            rootNode.AppendChild(nodeFields);

            foreach (KeyValuePair<string, ConfigurationField> field in fields)
            {
                XmlElement nodeField = xmlConfDocument.CreateElement("Field");
                nodeFields.AppendChild(nodeField);

                XmlElement nodeFieldName = xmlConfDocument.CreateElement("Name");
                nodeFieldName.InnerText = field.Key;
                nodeField.AppendChild(nodeFieldName);

                XmlElement nodeFullName = xmlConfDocument.CreateElement("FullName");
                nodeFullName.InnerText = string.IsNullOrEmpty(field.Value.FullName) ? CreateFullName(field.Key) : field.Value.FullName;
                nodeField.AppendChild(nodeFullName);

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

                if (!string.IsNullOrEmpty(field.Value.Desc))
                {
                    XmlElement nodeFieldDesc = xmlConfDocument.CreateElement("Desc");
                    nodeFieldDesc.InnerText = field.Value.Desc.Trim();
                    nodeField.AppendChild(nodeFieldDesc);
                }

                if (parentName == "Directory" || parentName == "Document" || parentName == "RegisterInformation")
                {
                    XmlElement nodeFieldIsPresentation = xmlConfDocument.CreateElement("IsPresentation");
                    nodeFieldIsPresentation.InnerText = field.Value.IsPresentation ? "1" : "0";
                    nodeField.AppendChild(nodeFieldIsPresentation);
                }

                XmlElement nodeFieldIsIndex = xmlConfDocument.CreateElement("IsIndex");
                nodeFieldIsIndex.InnerText = field.Value.IsIndex ? "1" : "0";
                nodeField.AppendChild(nodeFieldIsIndex);

                XmlElement nodeFieldIsFullTextSearch = xmlConfDocument.CreateElement("IsFullTextSearch");
                nodeFieldIsFullTextSearch.InnerText = field.Value.IsFullTextSearch ? "1" : "0";
                nodeField.AppendChild(nodeFieldIsFullTextSearch);

                XmlElement nodeFieldIsSearch = xmlConfDocument.CreateElement("IsSearch");
                nodeFieldIsSearch.InnerText = field.Value.IsSearch ? "1" : "0";
                nodeField.AppendChild(nodeFieldIsSearch);

                XmlElement nodeFieldIsExport = xmlConfDocument.CreateElement("IsExport");
                nodeFieldIsExport.InnerText = field.Value.IsExport ? "1" : "0";
                nodeField.AppendChild(nodeFieldIsExport);

                //
                // Додаткові поля які залежать від типу
                //

                if (field.Value.Type == "string")
                {
                    XmlElement nodeFieldMultiline = xmlConfDocument.CreateElement("Multiline");
                    nodeFieldMultiline.InnerText = field.Value.Multiline ? "1" : "0";
                    nodeField.AppendChild(nodeFieldMultiline);
                }
                else if (field.Value.Type == "integer")
                {
                    XmlElement nodeFieldAutomaticNumbering = xmlConfDocument.CreateElement("AutomaticNumbering");
                    nodeFieldAutomaticNumbering.InnerText = field.Value.AutomaticNumbering ? "1" : "0";
                    nodeField.AppendChild(nodeFieldAutomaticNumbering);
                }
                else if (field.Value.Type == "composite_pointer")
                {
                    //Не використовувати довідники
                    if (field.Value.CompositePointerNotUseDirectories)
                    {
                        XmlElement nodeFieldNotUseDirectories = xmlConfDocument.CreateElement("CompositePointerNotUseDirectories");
                        nodeFieldNotUseDirectories.InnerText = field.Value.CompositePointerNotUseDirectories ? "1" : "0";
                        nodeField.AppendChild(nodeFieldNotUseDirectories);
                    }

                    //Не використовувати документи
                    if (field.Value.CompositePointerNotUseDocuments)
                    {
                        XmlElement nodeFieldNotUseDocuments = xmlConfDocument.CreateElement("CompositePointerNotUseDocuments");
                        nodeFieldNotUseDocuments.InnerText = field.Value.CompositePointerNotUseDocuments ? "1" : "0";
                        nodeField.AppendChild(nodeFieldNotUseDocuments);
                    }

                    void Add(string nameAllowBlock, List<string> listAllow)
                    {
                        if (listAllow.Count == 0) return;

                        XmlElement nodeAllow = xmlConfDocument.CreateElement(nameAllowBlock);
                        nodeField.AppendChild(nodeAllow);

                        foreach (string name in listAllow)
                        {
                            XmlElement nodeName = xmlConfDocument.CreateElement("Name");
                            nodeName.InnerText = name;
                            nodeAllow.AppendChild(nodeName);
                        }
                    }

                    Add("CompositePointerAllowDirectories", field.Value.CompositePointerAllowDirectories);
                    Add("CompositePointerAllowDocuments", field.Value.CompositePointerAllowDocuments);
                }
            }
        }

        /// <summary>
        /// Збереження табличних частин
        /// </summary>
        /// <param name="Conf">Конфігурація</param>
        /// <param name="tabularParts">Колекція табличних частин</param>
        /// <param name="xmlConfDocument">Документ</param>
        /// <param name="rootNode">Корінна вітка</param>
        public static void SaveTabularParts(Configuration Conf, Dictionary<string, ConfigurationTablePart> tabularParts, XmlDocument xmlConfDocument, XmlElement rootNode)
        {
            XmlElement nodeTabularParts = xmlConfDocument.CreateElement("TabularParts");
            rootNode.AppendChild(nodeTabularParts);

            foreach (KeyValuePair<string, ConfigurationTablePart> tablePart in tabularParts)
            {
                XmlElement nodeTablePart = xmlConfDocument.CreateElement("TablePart");
                nodeTabularParts.AppendChild(nodeTablePart);

                XmlElement nodeTablePartName = xmlConfDocument.CreateElement("Name");
                nodeTablePartName.InnerText = tablePart.Key;
                nodeTablePart.AppendChild(nodeTablePartName);

                XmlElement nodeTablePartFullName = xmlConfDocument.CreateElement("FullName");
                nodeTablePartFullName.InnerText = string.IsNullOrEmpty(tablePart.Value.FullName) ? CreateFullName(tablePart.Key) : tablePart.Value.FullName;
                nodeTablePart.AppendChild(nodeTablePartFullName);

                XmlElement nodeTablePartTable = xmlConfDocument.CreateElement("Table");
                nodeTablePartTable.InnerText = tablePart.Value.Table;
                nodeTablePart.AppendChild(nodeTablePartTable);

                if (!string.IsNullOrEmpty(tablePart.Value.Desc))
                {
                    XmlElement nodeTablePartDesc = xmlConfDocument.CreateElement("Desc");
                    nodeTablePartDesc.InnerText = tablePart.Value.Desc;
                    nodeTablePart.AppendChild(nodeTablePartDesc);
                }

                XmlElement nodeVersionsHistory = xmlConfDocument.CreateElement("VersionsHistory");
                nodeVersionsHistory.InnerText = tablePart.Value.VersionsHistory ? "1" : "0";
                nodeTablePart.AppendChild(nodeVersionsHistory);

                SavePredefinedFields(ConfigurationTablePart.GetPredefinedFields(), xmlConfDocument, nodeTablePart);

                SaveFields(tablePart.Value.Fields, xmlConfDocument, nodeTablePart, "TablePart");

                SaveTabularList(tablePart.Value.Fields, tablePart.Value.TabularList, xmlConfDocument, nodeTablePart);

                SaveTriggerTablePartFunctions(tablePart.Value.TriggerFunctions, xmlConfDocument, nodeTablePart);

                SaveForms(Conf, tablePart.Value.Fields, null, tablePart.Value.Forms, xmlConfDocument, nodeTablePart);
            }
        }

        /// <summary>
        /// Збереження табличних списків
        /// </summary>
        /// <param name="fields">Колекція полів</param>
        /// <param name="tabularLists">Колекція табличних списків</param>
        /// <param name="xmlConfDocument">Документ</param>
        /// <param name="rootNode">Корінна вітка</param>
        public static void SaveTabularList(Dictionary<string, ConfigurationField> fields, Dictionary<string, ConfigurationTabularList> tabularLists, XmlDocument xmlConfDocument, XmlElement rootNode)
        {
            /*
            fields - список полів довідника чи документу
            tabularLists - колекція  табличних списків
            */

            XmlElement nodeTabularLists = xmlConfDocument.CreateElement("TabularLists");
            rootNode.AppendChild(nodeTabularLists);

            foreach (KeyValuePair<string, ConfigurationTabularList> tabularList in tabularLists)
            {
                XmlElement nodeTabularList = xmlConfDocument.CreateElement("TabularList");
                nodeTabularLists.AppendChild(nodeTabularList);

                XmlElement nodeTabularListName = xmlConfDocument.CreateElement("Name");
                nodeTabularListName.InnerText = tabularList.Key;
                nodeTabularList.AppendChild(nodeTabularListName);

                if (!string.IsNullOrEmpty(tabularList.Value.Desc))
                {
                    XmlElement nodeTabularListDesc = xmlConfDocument.CreateElement("Desc");
                    nodeTabularListDesc.InnerText = tabularList.Value.Desc;
                    nodeTabularList.AppendChild(nodeTabularListDesc);
                }

                XmlElement nodeTabularListFields = xmlConfDocument.CreateElement("Fields");
                nodeTabularList.AppendChild(nodeTabularListFields);

                //Поля
                foreach (KeyValuePair<string, ConfigurationTabularListField> field in tabularList.Value.Fields)
                    if (fields.TryGetValue(field.Key, out ConfigurationField? fieldsItem))
                    {
                        XmlElement nodeTabularListField = xmlConfDocument.CreateElement("Field");
                        nodeTabularListFields.AppendChild(nodeTabularListField);

                        XmlElement nodeName = xmlConfDocument.CreateElement("Name");
                        nodeName.InnerText = field.Key;
                        nodeTabularListField.AppendChild(nodeName);

                        XmlElement nodeCaption = xmlConfDocument.CreateElement("Caption");
                        nodeCaption.InnerText = field.Value.Caption;
                        nodeTabularListField.AppendChild(nodeCaption);

                        XmlElement nodeSize = xmlConfDocument.CreateElement("Size");
                        nodeSize.InnerText = field.Value.Size.ToString();
                        nodeTabularListField.AppendChild(nodeSize);

                        XmlElement nodeSortNum = xmlConfDocument.CreateElement("SortNum");
                        nodeSortNum.InnerText = field.Value.SortNum.ToString();
                        nodeTabularListField.AppendChild(nodeSortNum);

                        XmlElement nodeSortField = xmlConfDocument.CreateElement("SortField");
                        nodeSortField.InnerText = field.Value.SortField.ToString();
                        nodeTabularListField.AppendChild(nodeSortField);

                        XmlElement nodeSortDirection = xmlConfDocument.CreateElement("SortDirection");
                        nodeSortDirection.InnerText = field.Value.SortDirection.ToString();
                        nodeTabularListField.AppendChild(nodeSortDirection);

                        XmlElement nodeFilterField = xmlConfDocument.CreateElement("FilterField");
                        nodeFilterField.InnerText = field.Value.FilterField.ToString();
                        nodeTabularListField.AppendChild(nodeFilterField);

                        #region Додаткова інформація для полегшення генерування коду

                        XmlElement nodeType = xmlConfDocument.CreateElement("Type");
                        nodeType.InnerText = fieldsItem.Type;
                        nodeTabularListField.AppendChild(nodeType);

                        if (fieldsItem.Type == "pointer" || fieldsItem.Type == "enum")
                        {
                            XmlElement nodePointer = xmlConfDocument.CreateElement("Pointer");
                            nodePointer.InnerText = fieldsItem.Pointer;
                            nodeTabularListField.AppendChild(nodePointer);
                        }
                        else if (fieldsItem.Type == "string")
                        {
                            XmlElement nodeFieldMultiline = xmlConfDocument.CreateElement("Multiline");
                            nodeFieldMultiline.InnerText = fieldsItem.Multiline ? "1" : "0";
                            nodeTabularListField.AppendChild(nodeFieldMultiline);
                        }

                        #endregion
                    }

                //Додаткові поля
                foreach (KeyValuePair<string, ConfigurationTabularListAdditionalField> field in tabularList.Value.AdditionalFields)
                {
                    XmlElement nodeTabularListAdditionalField = xmlConfDocument.CreateElement("AdditionalField");
                    nodeTabularListFields.AppendChild(nodeTabularListAdditionalField);

                    XmlElement nodeVisible = xmlConfDocument.CreateElement("Visible");
                    nodeVisible.InnerText = field.Value.Visible.ToString();
                    nodeTabularListAdditionalField.AppendChild(nodeVisible);

                    XmlElement nodeName = xmlConfDocument.CreateElement("Name");
                    nodeName.InnerText = field.Key;
                    nodeTabularListAdditionalField.AppendChild(nodeName);

                    XmlElement nodeCaption = xmlConfDocument.CreateElement("Caption");
                    nodeCaption.InnerText = field.Value.Caption;
                    nodeTabularListAdditionalField.AppendChild(nodeCaption);

                    XmlElement nodeSize = xmlConfDocument.CreateElement("Size");
                    nodeSize.InnerText = field.Value.Size.ToString();
                    nodeTabularListAdditionalField.AppendChild(nodeSize);

                    XmlElement nodeSortNum = xmlConfDocument.CreateElement("SortNum");
                    nodeSortNum.InnerText = field.Value.SortNum.ToString();
                    nodeTabularListAdditionalField.AppendChild(nodeSortNum);

                    XmlElement nodeType = xmlConfDocument.CreateElement("Type");
                    nodeType.InnerText = field.Value.Type;
                    nodeTabularListAdditionalField.AppendChild(nodeType);

                    XmlElement nodeValue = xmlConfDocument.CreateElement("Value");
                    nodeValue.AppendChild(xmlConfDocument.CreateCDataSection(field.Value.Value));
                    nodeTabularListAdditionalField.AppendChild(nodeValue);
                }
            }
        }

        /// <summary>
        /// Збереження табличних списків для журналу
        /// </summary>
        /// <param name="Conf">Конфігурація</param>
        /// <param name="fields">Колекція полів</param>
        /// <param name="tabularLists">Колекція табличних списків</param>
        /// <param name="xmlConfDocument">Документ</param>
        /// <param name="rootNode">Корінна вітка</param>
        private static void SaveJournalTabularList(Configuration Conf, Dictionary<string, ConfigurationJournalField> fields, Dictionary<string, ConfigurationTabularList> tabularLists, XmlDocument xmlConfDocument, XmlElement rootNode)
        {
            XmlElement nodeTabularLists = xmlConfDocument.CreateElement("TabularLists");
            rootNode.AppendChild(nodeTabularLists);

            foreach (KeyValuePair<string, ConfigurationTabularList> tabularList in tabularLists)
                if (Conf.Documents.TryGetValue(tabularList.Key, out ConfigurationDocuments? Doc))
                {
                    XmlElement nodeTabularList = xmlConfDocument.CreateElement("TabularList");
                    nodeTabularLists.AppendChild(nodeTabularList);

                    XmlElement nodeTabularListName = xmlConfDocument.CreateElement("Name");
                    nodeTabularListName.InnerText = tabularList.Key;
                    nodeTabularList.AppendChild(nodeTabularListName);

                    if (!string.IsNullOrEmpty(tabularList.Value.Desc))
                    {
                        XmlElement nodeTabularListDesc = xmlConfDocument.CreateElement("Desc");
                        nodeTabularListDesc.InnerText = tabularList.Value.Desc;
                        nodeTabularList.AppendChild(nodeTabularListDesc);
                    }

                    XmlElement nodeTabularListTable = xmlConfDocument.CreateElement("Table");
                    nodeTabularListTable.InnerText = Doc.Table;
                    nodeTabularList.AppendChild(nodeTabularListTable);

                    XmlElement nodeTabularListFields = xmlConfDocument.CreateElement("Fields");
                    nodeTabularList.AppendChild(nodeTabularListFields);

                    foreach (KeyValuePair<string, ConfigurationTabularListField> field in tabularList.Value.Fields)
                        if (fields.TryGetValue(field.Key, out ConfigurationJournalField? fieldsItem))
                        {
                            string docField = field.Value.DocField;

                            XmlElement nodeTabularListField = xmlConfDocument.CreateElement("Field");
                            nodeTabularListFields.AppendChild(nodeTabularListField);

                            XmlElement nodeName = xmlConfDocument.CreateElement("Name");
                            nodeName.InnerText = field.Key;
                            nodeTabularListField.AppendChild(nodeName);

                            XmlElement nodeDocField = xmlConfDocument.CreateElement("DocField");
                            nodeDocField.InnerText = docField;
                            nodeTabularListField.AppendChild(nodeDocField);

                            XmlElement nodeSqlType = xmlConfDocument.CreateElement("SqlType");
                            nodeSqlType.InnerText = fieldsItem.Type;
                            nodeTabularListField.AppendChild(nodeSqlType);

                            XmlElement nodeWherePeriod = xmlConfDocument.CreateElement("WherePeriod");
                            nodeWherePeriod.InnerText = fieldsItem.WherePeriod ? "1" : "0";
                            nodeTabularListField.AppendChild(nodeWherePeriod);

                            #region Додаткова інформація для полегшення генерування коду

                            //Поле документу
                            if (!string.IsNullOrEmpty(docField) && Doc.Fields.TryGetValue(docField, out ConfigurationField? DocFieldItem))
                            {
                                XmlElement nodeType = xmlConfDocument.CreateElement("Type");
                                nodeType.InnerText = DocFieldItem.Type;
                                nodeTabularListField.AppendChild(nodeType);

                                if (DocFieldItem.Type == "pointer" || DocFieldItem.Type == "enum")
                                {
                                    XmlElement nodePointer = xmlConfDocument.CreateElement("Pointer");
                                    nodePointer.InnerText = DocFieldItem.Pointer;
                                    nodeTabularListField.AppendChild(nodePointer);
                                }
                            }

                            #endregion
                        }
                }
        }

        /// <summary>
        /// Зберегти форми
        /// </summary>
        /// <param name="Conf">Конфігурація</param>
        /// <param name="fields">Колекція полів</param>
        /// <param name="tabularParts">Колекція табличних частин</param>
        /// <param name="forms">Колекція форм</param>
        /// <param name="xmlConfDocument">Документ</param>
        /// <param name="rootNode">Корінна вітка</param>
        private static void SaveForms(Configuration Conf, Dictionary<string, ConfigurationField> fields, Dictionary<string, ConfigurationTablePart>? tabularParts, Dictionary<string, ConfigurationForms> forms, XmlDocument xmlConfDocument, XmlElement rootNode)
        {
            XmlElement nodeForms = xmlConfDocument.CreateElement("Forms");
            rootNode.AppendChild(nodeForms);

            foreach (KeyValuePair<string, ConfigurationForms> form in forms)
            {
                XmlElement nodeForm = xmlConfDocument.CreateElement("Form");
                nodeForms.AppendChild(nodeForm);

                //Ознака зміни в даних форми, цей атрибут тільки записується але не зчитується
                XmlAttribute modifiedAttr = xmlConfDocument.CreateAttribute("Modified");
                modifiedAttr.Value = form.Value.Modified ? "1" : "0";
                nodeForm.Attributes.Append(modifiedAttr);

                XmlElement nodeName = xmlConfDocument.CreateElement("Name");
                nodeName.InnerText = form.Key;
                nodeForm.AppendChild(nodeName);

                if (!string.IsNullOrEmpty(form.Value.Desc))
                {
                    XmlElement nodeDesc = xmlConfDocument.CreateElement("Desc");
                    nodeDesc.InnerText = form.Value.Desc;
                    nodeForm.AppendChild(nodeDesc);
                }

                XmlElement nodeType = xmlConfDocument.CreateElement("Type");
                nodeType.InnerText = form.Value.Type.ToString();
                nodeForm.AppendChild(nodeType);

                XmlElement nodeGeneratedCode = SetZipAndBase64TextToNode(xmlConfDocument, "GeneratedCode", form.Value.GeneratedCode);
                nodeForm.AppendChild(nodeGeneratedCode);

                XmlElement nodeNotSaveToFile = xmlConfDocument.CreateElement("NotSaveToFile");
                nodeNotSaveToFile.InnerText = form.Value.NotSaveToFile ? "1" : "0";
                nodeForm.AppendChild(nodeNotSaveToFile);

                if (form.Value.Type == ConfigurationForms.TypeForms.List ||
                    form.Value.Type == ConfigurationForms.TypeForms.ListSmallSelect ||
                    form.Value.Type == ConfigurationForms.TypeForms.ListAndTree)
                {
                    XmlElement nodeTabularList = xmlConfDocument.CreateElement("TabularList");
                    nodeTabularList.InnerText = form.Value.TabularList.ToString();
                    nodeForm.AppendChild(nodeTabularList);
                }
                else if (form.Value.Type == ConfigurationForms.TypeForms.Element || form.Value.Type == ConfigurationForms.TypeForms.TablePart)
                {
                    SaveFormElementField(Conf, fields, form.Value.ElementFields, xmlConfDocument, nodeForm);

                    if (tabularParts != null)
                        SaveFormElementTablePart(tabularParts, form.Value.ElementTableParts, xmlConfDocument, nodeForm);

                    if (form.Value.Type == ConfigurationForms.TypeForms.TablePart)
                    {
                        XmlElement nodeIncludeIconColumn = xmlConfDocument.CreateElement("IncludeIconColumn");
                        nodeIncludeIconColumn.InnerText = form.Value.IncludeIconColumn ? "1" : "0";
                        nodeForm.AppendChild(nodeIncludeIconColumn);
                    }
                }
            }
        }

        /// <summary>
        /// Зберегти поля для форми
        /// </summary>
        /// <param name="Conf">Конфігурація</param>
        /// <param name="fields">Колекція полів</param>
        /// <param name="elementFields">Колекція полів в формі</param>
        /// <param name="xmlConfDocument">Документ</param>
        /// <param name="rootNode">Корінна вітка</param>
        public static void SaveFormElementField(Configuration Conf, Dictionary<string, ConfigurationField> fields, Dictionary<string, ConfigurationFormsElementField> elementFields, XmlDocument xmlConfDocument, XmlElement rootNode)
        {
            XmlElement nodeElementFields = xmlConfDocument.CreateElement("ElementFields");
            rootNode.AppendChild(nodeElementFields);

            foreach (KeyValuePair<string, ConfigurationFormsElementField> elementField in elementFields)
                if (fields.TryGetValue(elementField.Key, out ConfigurationField? fieldsItem))
                {
                    XmlElement nodeElementField = xmlConfDocument.CreateElement("Field");
                    nodeElementFields.AppendChild(nodeElementField);

                    XmlElement nodeFieldName = xmlConfDocument.CreateElement("Name");
                    nodeFieldName.InnerText = elementField.Key;
                    nodeElementField.AppendChild(nodeFieldName);

                    XmlElement nodeFieldCaption = xmlConfDocument.CreateElement("Caption");
                    nodeFieldCaption.InnerText = elementField.Value.Caption;
                    nodeElementField.AppendChild(nodeFieldCaption);

                    XmlElement nodeSize = xmlConfDocument.CreateElement("Size");
                    nodeSize.InnerText = elementField.Value.Size.ToString();
                    nodeElementField.AppendChild(nodeSize);

                    XmlElement nodeHeight = xmlConfDocument.CreateElement("Height");
                    nodeHeight.InnerText = elementField.Value.Height.ToString();
                    nodeElementField.AppendChild(nodeHeight);

                    XmlElement nodeSortNum = xmlConfDocument.CreateElement("SortNum");
                    nodeSortNum.InnerText = elementField.Value.SortNum.ToString();
                    nodeElementField.AppendChild(nodeSortNum);

                    XmlElement nodeReadOnly = xmlConfDocument.CreateElement("ReadOnly");
                    nodeReadOnly.InnerText = elementField.Value.ReadOnly ? "1" : "0";
                    nodeElementField.AppendChild(nodeReadOnly);

                    #region Додаткова інформація для полегшення генерування коду

                    XmlElement nodeType = xmlConfDocument.CreateElement("Type");
                    nodeType.InnerText = fieldsItem.Type;
                    nodeElementField.AppendChild(nodeType);

                    if (fieldsItem.Type == "pointer" || fieldsItem.Type == "enum")
                    {
                        XmlElement nodePointer = xmlConfDocument.CreateElement("Pointer");
                        nodePointer.InnerText = fieldsItem.Pointer;
                        nodeElementField.AppendChild(nodePointer);

                        //Тільки для вказівника
                        if (fieldsItem.Type == "pointer")
                        {
                            if (elementField.Value.MultipleSelect)
                            {
                                XmlElement nodeMultipleSelect = xmlConfDocument.CreateElement("MultipleSelect");
                                nodeMultipleSelect.InnerText = elementField.Value.MultipleSelect ? "1" : "0";
                                nodeElementField.AppendChild(nodeMultipleSelect);
                            }

                            (bool Result, string PointerGroup, string PointerType) = PointerParse(fieldsItem.Pointer, out Exception? _);
                            if (Result)
                            {
                                List<ConfigurationField> presetntationFields = [];

                                if (PointerGroup == "Довідники")
                                {
                                    if (Conf.Directories.TryGetValue(PointerType, out ConfigurationDirectories? configurationDirectories))
                                    {
                                        presetntationFields = configurationDirectories.GetPresentationFields();

                                        //Інформація про власника
                                        if (!string.IsNullOrEmpty(configurationDirectories.DirectoryOwner_Subordination))
                                        {
                                            XmlElement nodeDirectoryOwner = xmlConfDocument.CreateElement("DirectoryOwner");
                                            nodeDirectoryOwner.InnerText = configurationDirectories.DirectoryOwner_Subordination;
                                            nodeElementField.AppendChild(nodeDirectoryOwner);
                                        }
                                    }
                                }
                                else if (PointerGroup == "Документи")
                                    if (Conf.Documents.TryGetValue(PointerType, out ConfigurationDocuments? configurationDocuments))
                                        presetntationFields = configurationDocuments.GetPresentationFields();

                                XmlElement nodePresetntationFields = xmlConfDocument.CreateElement("PresetntationFields");
                                nodePresetntationFields.SetAttribute("Count", presetntationFields.Count.ToString());
                                nodeElementField.AppendChild(nodePresetntationFields);

                                foreach (ConfigurationField field in presetntationFields)
                                {
                                    XmlElement nodePresetntationField = xmlConfDocument.CreateElement("Field");
                                    nodePresetntationField.InnerText = field.Name;
                                    nodePresetntationFields.AppendChild(nodePresetntationField);
                                }
                            }
                        }
                    }
                    else if (fieldsItem.Type == "string")
                    {
                        XmlElement nodeFieldMultiline = xmlConfDocument.CreateElement("Multiline");
                        nodeFieldMultiline.InnerText = fieldsItem.Multiline ? "1" : "0";
                        nodeElementField.AppendChild(nodeFieldMultiline);
                    }
                    else if (fieldsItem.Type == "integer")
                    {
                        XmlElement nodeFieldAutomaticNumbering = xmlConfDocument.CreateElement("AutomaticNumbering");
                        nodeFieldAutomaticNumbering.InnerText = fieldsItem.AutomaticNumbering ? "1" : "0";
                        nodeElementField.AppendChild(nodeFieldAutomaticNumbering);
                    }

                    #endregion
                }
        }

        /// <summary>
        /// Зберегти табличні частини для форми
        /// </summary>
        /// <param name="tabularParts">Колекція табличних частин</param>
        /// <param name="elementTableParts">Колекція табличних частин у формі</param>
        /// <param name="xmlConfDocument">Документ</param>
        /// <param name="rootNode">Корінна вітка</param>
        public static void SaveFormElementTablePart(Dictionary<string, ConfigurationTablePart> tabularParts, Dictionary<string, ConfigurationFormsElementTablePart> elementTableParts, XmlDocument xmlConfDocument, XmlElement rootNode)
        {
            XmlElement nodeElementTableParts = xmlConfDocument.CreateElement("ElementTableParts");
            rootNode.AppendChild(nodeElementTableParts);

            foreach (KeyValuePair<string, ConfigurationFormsElementTablePart> elementTablePart in elementTableParts)
                if (tabularParts.TryGetValue(elementTablePart.Key, out ConfigurationTablePart? tabularPartsItem))
                {
                    XmlElement nodeElementTablePart = xmlConfDocument.CreateElement("TablePart");
                    nodeElementTableParts.AppendChild(nodeElementTablePart);

                    XmlElement nodeFieldName = xmlConfDocument.CreateElement("Name");
                    nodeFieldName.InnerText = elementTablePart.Key;
                    nodeElementTablePart.AppendChild(nodeFieldName);

                    XmlElement nodeFieldCaption = xmlConfDocument.CreateElement("Caption");
                    nodeFieldCaption.InnerText = elementTablePart.Value.Caption;
                    nodeElementTablePart.AppendChild(nodeFieldCaption);

                    XmlElement nodeSize = xmlConfDocument.CreateElement("Size");
                    nodeSize.InnerText = elementTablePart.Value.Size.ToString();
                    nodeElementTablePart.AppendChild(nodeSize);

                    XmlElement nodeHeight = xmlConfDocument.CreateElement("Height");
                    nodeHeight.InnerText = elementTablePart.Value.Height.ToString();
                    nodeElementTablePart.AppendChild(nodeHeight);

                    XmlElement nodeSortNum = xmlConfDocument.CreateElement("SortNum");
                    nodeSortNum.InnerText = elementTablePart.Value.SortNum.ToString();
                    nodeElementTablePart.AppendChild(nodeSortNum);
                }
        }

        /// <summary>
        /// Зберегти доступні регістри для документу
        /// </summary>
        /// <param name="allowRegisterAccumulation">Список доступних регістрів</param>
        /// <param name="xmlConfDocument">Документ</param>
        /// <param name="rootNode">Корінна вітка</param>
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

        /// <summary>
        /// Зберегти функції для тригерів
        /// </summary>
        /// <param name="triggerFunctions">Функції для тригерів</param>
        /// <param name="xmlConfDocument">Документ</param>
        /// <param name="rootNode">Корінна вітка</param>
        public static void SaveTriggerFunctions(ConfigurationTriggerFunctions triggerFunctions, XmlDocument xmlConfDocument, XmlElement rootNode)
        {
            XmlElement nodeTriggerFunctions = xmlConfDocument.CreateElement("TriggerFunctions");
            rootNode.AppendChild(nodeTriggerFunctions);

            void AddAttributeAction(XmlElement node, bool value)
            {
                XmlAttribute actionAttr = xmlConfDocument.CreateAttribute("Action");
                actionAttr.Value = value ? "1" : "0";
                node.Attributes.Append(actionAttr);
            }

            XmlElement nodeNew = xmlConfDocument.CreateElement("New");
            nodeNew.InnerText = triggerFunctions.New;
            AddAttributeAction(nodeNew, triggerFunctions.NewAction);
            nodeTriggerFunctions.AppendChild(nodeNew);

            XmlElement nodeCopying = xmlConfDocument.CreateElement("Copying");
            nodeCopying.InnerText = triggerFunctions.Copying;
            AddAttributeAction(nodeCopying, triggerFunctions.CopyingAction);
            nodeTriggerFunctions.AppendChild(nodeCopying);

            XmlElement nodeBeforeSave = xmlConfDocument.CreateElement("BeforeSave");
            nodeBeforeSave.InnerText = triggerFunctions.BeforeSave;
            AddAttributeAction(nodeBeforeSave, triggerFunctions.BeforeSaveAction);
            nodeTriggerFunctions.AppendChild(nodeBeforeSave);

            XmlElement nodeAfterSave = xmlConfDocument.CreateElement("AfterSave");
            nodeAfterSave.InnerText = triggerFunctions.AfterSave;
            AddAttributeAction(nodeAfterSave, triggerFunctions.AfterSaveAction);
            nodeTriggerFunctions.AppendChild(nodeAfterSave);

            XmlElement nodeSetDeletionLabel = xmlConfDocument.CreateElement("SetDeletionLabel");
            nodeSetDeletionLabel.InnerText = triggerFunctions.SetDeletionLabel;
            AddAttributeAction(nodeSetDeletionLabel, triggerFunctions.SetDeletionLabelAction);
            nodeTriggerFunctions.AppendChild(nodeSetDeletionLabel);

            XmlElement nodeBeforeDelete = xmlConfDocument.CreateElement("BeforeDelete");
            nodeBeforeDelete.InnerText = triggerFunctions.BeforeDelete;
            AddAttributeAction(nodeBeforeDelete, triggerFunctions.BeforeDeleteAction);
            nodeTriggerFunctions.AppendChild(nodeBeforeDelete);
        }

        /// <summary>
        /// Зберегти функції тригери для табличної частини
        /// </summary>
        /// <param name="triggerFunctions">Функції тригери</param>
        /// <param name="xmlConfDocument">Документ</param>
        /// <param name="rootNode">Корінна вітка</param>
        public static void SaveTriggerTablePartFunctions(ConfigurationTriggerTablePartFunctions triggerFunctions, XmlDocument xmlConfDocument, XmlElement rootNode)
        {
            XmlElement nodeTriggerFunctions = xmlConfDocument.CreateElement("TriggerFunctions");
            rootNode.AppendChild(nodeTriggerFunctions);

            void AddAttributeAction(XmlElement node, bool value)
            {
                XmlAttribute actionAttr = xmlConfDocument.CreateAttribute("Action");
                actionAttr.Value = value ? "1" : "0";
                node.Attributes.Append(actionAttr);
            }

            XmlElement nodeBeforeSave = xmlConfDocument.CreateElement("BeforeSave");
            nodeBeforeSave.InnerText = triggerFunctions.BeforeSave;
            AddAttributeAction(nodeBeforeSave, triggerFunctions.BeforeSaveAction);
            nodeTriggerFunctions.AppendChild(nodeBeforeSave);

            XmlElement nodeAfterSave = xmlConfDocument.CreateElement("AfterSave");
            nodeAfterSave.InnerText = triggerFunctions.AfterSave;
            AddAttributeAction(nodeAfterSave, triggerFunctions.AfterSaveAction);
            nodeTriggerFunctions.AppendChild(nodeAfterSave);
        }

        /// <summary>
        /// Зберегти функції для проведення
        /// </summary>
        /// <param name="spendFunctions">Функції для проведення</param>
        /// <param name="xmlConfDocument">Документ</param>
        /// <param name="rootNode">Корінна вітка</param>
        public static void SaveSpendFunctions(ConfigurationSpendFunctions spendFunctions, XmlDocument xmlConfDocument, XmlElement rootNode)
        {
            void AddAttributeAction(XmlElement node, bool value)
            {
                XmlAttribute actionAttr = xmlConfDocument.CreateAttribute("Action");
                actionAttr.Value = value ? "1" : "0";
                node.Attributes.Append(actionAttr);
            }

            XmlElement nodeSpendFunctions = xmlConfDocument.CreateElement("SpendFunctions");
            rootNode.AppendChild(nodeSpendFunctions);

            XmlElement nodeSpend = xmlConfDocument.CreateElement("Spend");
            nodeSpend.InnerText = spendFunctions.Spend;
            AddAttributeAction(nodeSpend, spendFunctions.SpendAction);
            nodeSpendFunctions.AppendChild(nodeSpend);

            XmlElement nodeClearSpend = xmlConfDocument.CreateElement("ClearSpend");
            nodeClearSpend.InnerText = spendFunctions.ClearSpend;
            AddAttributeAction(nodeClearSpend, spendFunctions.ClearSpendAction);
            nodeSpendFunctions.AppendChild(nodeClearSpend);
        }

        /// <summary>
        /// Зберегти перелічення
        /// </summary>
        /// <param name="enums">Колекція перелічень</param>
        /// <param name="xmlConfDocument">Документ</param>
        /// <param name="rootNode">Корінна вітка</param>
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

                if (!string.IsNullOrEmpty(enum_item.Value.Desc))
                {
                    XmlElement nodeEnumDesc = xmlConfDocument.CreateElement("Desc");
                    nodeEnumDesc.InnerText = enum_item.Value.Desc;
                    nodeEnum.AppendChild(nodeEnumDesc);
                }

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

        /// <summary>
        /// Зберегти журнали
        /// </summary>
        /// <param name="Conf">Конфігурація</param>
        /// <param name="journals">Колекція журналів</param>
        /// <param name="xmlConfDocument">Документ</param>
        /// <param name="rootNode">Корінна вітка</param>
        private static void SaveJournals(Configuration Conf, Dictionary<string, ConfigurationJournals> journals, XmlDocument xmlConfDocument, XmlElement rootNode)
        {
            XmlElement nodeJournals = xmlConfDocument.CreateElement("Journals");
            rootNode.AppendChild(nodeJournals);

            foreach (KeyValuePair<string, ConfigurationJournals> journal in journals)
            {
                XmlElement nodeJournal = xmlConfDocument.CreateElement("Journal");
                nodeJournals.AppendChild(nodeJournal);

                XmlElement nodeJournalName = xmlConfDocument.CreateElement("Name");
                nodeJournalName.InnerText = journal.Key;
                nodeJournal.AppendChild(nodeJournalName);

                if (!string.IsNullOrEmpty(journal.Value.Desc))
                {
                    XmlElement nodeJournalDesc = xmlConfDocument.CreateElement("Desc");
                    nodeJournalDesc.InnerText = journal.Value.Desc;
                    nodeJournal.AppendChild(nodeJournalDesc);
                }

                XmlElement nodeFields = xmlConfDocument.CreateElement("Fields");
                nodeJournal.AppendChild(nodeFields);

                foreach (KeyValuePair<string, ConfigurationJournalField> field in journal.Value.Fields)
                {
                    XmlElement nodeField = xmlConfDocument.CreateElement("Field");
                    nodeFields.AppendChild(nodeField);

                    XmlElement nodeFieldName = xmlConfDocument.CreateElement("Name");
                    nodeFieldName.InnerText = field.Value.Name;
                    nodeField.AppendChild(nodeFieldName);

                    XmlElement nodeFieldDesc = xmlConfDocument.CreateElement("Desc");
                    nodeFieldDesc.InnerText = field.Value.Desc;
                    nodeField.AppendChild(nodeFieldDesc);

                    XmlElement nodeFieldType = xmlConfDocument.CreateElement("Type");
                    nodeFieldType.InnerText = field.Value.Type;
                    nodeField.AppendChild(nodeFieldType);

                    XmlElement nodeFieldSort = xmlConfDocument.CreateElement("SortField");
                    nodeFieldSort.InnerText = field.Value.SortField ? "1" : "0";
                    nodeField.AppendChild(nodeFieldSort);

                    XmlElement nodeFieldWherePeriod = xmlConfDocument.CreateElement("WherePeriod");
                    nodeFieldWherePeriod.InnerText = field.Value.WherePeriod ? "1" : "0";
                    nodeField.AppendChild(nodeFieldWherePeriod);
                }

                SaveJournalAllowDocument(Conf, journal.Value.AllowDocuments, xmlConfDocument, nodeJournal);

                SaveJournalTabularList(Conf, journal.Value.Fields, journal.Value.TabularList, xmlConfDocument, nodeJournal);
            }
        }

        /// <summary>
        /// Зберегти список документів для журналу
        /// </summary>
        /// <param name="Conf">Конфігурація</param>
        /// <param name="allowDocument">Список доступних документів</param>
        /// <param name="xmlConfDocument">Документ</param>
        /// <param name="rootNode">Корінна вітка</param>
        private static void SaveJournalAllowDocument(Configuration Conf, List<string> allowDocument, XmlDocument xmlConfDocument, XmlElement rootNode)
        {
            XmlElement nodeAllowDocument = xmlConfDocument.CreateElement("AllowDocument");
            rootNode.AppendChild(nodeAllowDocument);

            foreach (string name in allowDocument)
            {
                XmlElement nodeItem = xmlConfDocument.CreateElement("Item");
                nodeAllowDocument.AppendChild(nodeItem);

                XmlElement nodeName = xmlConfDocument.CreateElement("Name");
                nodeName.InnerText = name;
                nodeItem.AppendChild(nodeName);

                if (Conf.Documents.TryGetValue(name, out ConfigurationDocuments? value))
                {
                    XmlElement nodeFullName = xmlConfDocument.CreateElement("FullName");
                    nodeFullName.InnerText = value.FullName;
                    nodeItem.AppendChild(nodeFullName);
                }
            }
        }

        /// <summary>
        /// Зберегти документи
        /// </summary>
        /// <param name="Conf">Конфігурація</param>
        /// <param name="ConfDocuments">Колекція документів</param>
        /// <param name="xmlConfDocument">Документ</param>
        /// <param name="rootNode">Корінна вітка</param>
        private static void SaveDocuments(Configuration Conf, Dictionary<string, ConfigurationDocuments> ConfDocuments, XmlDocument xmlConfDocument, XmlElement rootNode)
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

                XmlElement nodeDocumentFullName = xmlConfDocument.CreateElement("FullName");
                nodeDocumentFullName.InnerText = ConfDocument.Value.FullName;
                nodeDocument.AppendChild(nodeDocumentFullName);

                XmlElement nodeDocumentTable = xmlConfDocument.CreateElement("Table");
                nodeDocumentTable.InnerText = ConfDocument.Value.Table;
                nodeDocument.AppendChild(nodeDocumentTable);

                if (!string.IsNullOrEmpty(ConfDocument.Value.Desc))
                {
                    XmlElement nodeDocumentDesc = xmlConfDocument.CreateElement("Desc");
                    nodeDocumentDesc.InnerText = ConfDocument.Value.Desc;
                    nodeDocument.AppendChild(nodeDocumentDesc);
                }

                XmlElement nodeDocumentAutoNum = xmlConfDocument.CreateElement("AutoNum");
                nodeDocumentAutoNum.InnerText = ConfDocument.Value.AutomaticNumeration ? "1" : "0";
                nodeDocument.AppendChild(nodeDocumentAutoNum);

                XmlElement nodeDocumentVersionsHistory = xmlConfDocument.CreateElement("VersionsHistory");
                nodeDocumentVersionsHistory.InnerText = ConfDocument.Value.VersionsHistory ? "1" : "0";
                nodeDocument.AppendChild(nodeDocumentVersionsHistory);

                XmlElement nodeDocumentExportXml = xmlConfDocument.CreateElement("ExportXml");
                nodeDocumentExportXml.InnerText = ConfDocument.Value.ExportXml ? "1" : "0";
                nodeDocument.AppendChild(nodeDocumentExportXml);

                SavePredefinedFields(ConfigurationDocuments.GetPredefinedFields(), xmlConfDocument, nodeDocument);

                SaveFields(ConfDocument.Value.Fields, xmlConfDocument, nodeDocument, "Document");

                SaveTabularParts(Conf, ConfDocument.Value.TabularParts, xmlConfDocument, nodeDocument);

                SaveTabularList(ConfDocument.Value.Fields, ConfDocument.Value.TabularList, xmlConfDocument, nodeDocument);

                SaveAllowRegisterAccumulation(ConfDocument.Value.AllowRegisterAccumulation, xmlConfDocument, nodeDocument);

                SaveTriggerFunctions(ConfDocument.Value.TriggerFunctions, xmlConfDocument, nodeDocument);

                SaveSpendFunctions(ConfDocument.Value.SpendFunctions, xmlConfDocument, nodeDocument);

                SaveForms(Conf, ConfDocument.Value.Fields, ConfDocument.Value.TabularParts, ConfDocument.Value.Forms, xmlConfDocument, nodeDocument);
            }
        }

        /// <summary>
        /// Зберегти регістри інформації
        /// </summary>
        /// <param name="Conf">Конфігурація</param>
        /// <param name="ConfRegistersInformation">Колекція регістрів</param>
        /// <param name="xmlConfDocument">Документ</param>
        /// <param name="rootNode">Корінна вітка</param>
        private static void SaveRegistersInformation(Configuration Conf, Dictionary<string, ConfigurationRegistersInformation> ConfRegistersInformation, XmlDocument xmlConfDocument, XmlElement rootNode)
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

                XmlElement nodeRegisterFullName = xmlConfDocument.CreateElement("FullName");
                nodeRegisterFullName.InnerText = ConfRegisterInfo.Value.FullName;
                nodeRegister.AppendChild(nodeRegisterFullName);

                XmlElement nodeRegisterTable = xmlConfDocument.CreateElement("Table");
                nodeRegisterTable.InnerText = ConfRegisterInfo.Value.Table;
                nodeRegister.AppendChild(nodeRegisterTable);

                if (!string.IsNullOrEmpty(ConfRegisterInfo.Value.Desc))
                {
                    XmlElement nodeRegisterDesc = xmlConfDocument.CreateElement("Desc");
                    nodeRegisterDesc.InnerText = ConfRegisterInfo.Value.Desc;
                    nodeRegister.AppendChild(nodeRegisterDesc);
                }

                SavePredefinedFields(ConfigurationRegistersInformation.GetPredefinedFields(), xmlConfDocument, nodeRegister);

                XmlElement nodeDimensionFields = xmlConfDocument.CreateElement("DimensionFields");
                nodeRegister.AppendChild(nodeDimensionFields);

                SaveFields(ConfRegisterInfo.Value.DimensionFields, xmlConfDocument, nodeDimensionFields, "RegisterInformation");

                XmlElement nodeResourcesFields = xmlConfDocument.CreateElement("ResourcesFields");
                nodeRegister.AppendChild(nodeResourcesFields);

                SaveFields(ConfRegisterInfo.Value.ResourcesFields, xmlConfDocument, nodeResourcesFields, "RegisterInformation");

                XmlElement nodePropertyFields = xmlConfDocument.CreateElement("PropertyFields");
                nodeRegister.AppendChild(nodePropertyFields);

                SaveFields(ConfRegisterInfo.Value.PropertyFields, xmlConfDocument, nodePropertyFields, "RegisterInformation");

                Dictionary<string, ConfigurationField> AllFields = CombineAllFieldForRegister(
                    ConfRegisterInfo.Value.DimensionFields.Values,
                    ConfRegisterInfo.Value.ResourcesFields.Values,
                    ConfRegisterInfo.Value.PropertyFields.Values);

                SaveTabularList(AllFields, ConfRegisterInfo.Value.TabularList, xmlConfDocument, nodeRegister);

                SaveForms(Conf, AllFields, null, ConfRegisterInfo.Value.Forms, xmlConfDocument, nodeRegister);
            }
        }

        /// <summary>
        /// Зберегти список документів які роблять рухи по регістру
        /// </summary>
        /// <param name="allowDocumentSpend">Список документів</param>
        /// <param name="xmlConfDocument">Документ</param>
        /// <param name="rootNode">Корінна вітка</param>
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

        /// <summary>
        /// Зберегти регістри накопичення
        /// </summary>
        /// <param name="Conf">Конфігурація</param>
        /// <param name="ConfRegistersAccumulation">Колекція регістрів</param>
        /// <param name="xmlConfDocument">Документ</param>
        /// <param name="rootNode">Корінна вітка</param>
        private static void SaveRegistersAccumulation(Configuration Conf, Dictionary<string, ConfigurationRegistersAccumulation> ConfRegistersAccumulation, XmlDocument xmlConfDocument, XmlElement rootNode)
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

                XmlElement nodeRegisterFullName = xmlConfDocument.CreateElement("FullName");
                nodeRegisterFullName.InnerText = ConfRegisterAccml.Value.FullName;
                nodeRegister.AppendChild(nodeRegisterFullName);

                XmlElement nodeRegisterTable = xmlConfDocument.CreateElement("Table");
                nodeRegisterTable.InnerText = ConfRegisterAccml.Value.Table;
                nodeRegister.AppendChild(nodeRegisterTable);

                XmlElement nodeRegisterType = xmlConfDocument.CreateElement("Type");
                nodeRegisterType.InnerText = ConfRegisterAccml.Value.TypeRegistersAccumulation.ToString();
                nodeRegister.AppendChild(nodeRegisterType);

                if (!string.IsNullOrEmpty(ConfRegisterAccml.Value.Desc))
                {
                    XmlElement nodeRegisterDesc = xmlConfDocument.CreateElement("Desc");
                    nodeRegisterDesc.InnerText = ConfRegisterAccml.Value.Desc;
                    nodeRegister.AppendChild(nodeRegisterDesc);
                }

                XmlElement nodeRegisterNoSummary = xmlConfDocument.CreateElement("NoSummary");
                nodeRegisterNoSummary.InnerText = ConfRegisterAccml.Value.NoSummary ? "1" : "0";
                nodeRegister.AppendChild(nodeRegisterNoSummary);

                SavePredefinedFields(ConfigurationRegistersAccumulation.GetPredefinedFields(), xmlConfDocument, nodeRegister);

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

                SaveTabularParts(Conf, ConfRegisterAccml.Value.TabularParts, xmlConfDocument, nodeRegister);

                SaveQueryBlockList(ConfRegisterAccml.Value.QueryBlockList, xmlConfDocument, nodeRegister);

                Dictionary<string, ConfigurationField> AllFields = CombineAllFieldForRegister(
                    ConfRegisterAccml.Value.DimensionFields.Values,
                    ConfRegisterAccml.Value.ResourcesFields.Values,
                    ConfRegisterAccml.Value.PropertyFields.Values);

                SaveTabularList(AllFields, ConfRegisterAccml.Value.TabularList, xmlConfDocument, nodeRegister);

                XmlElement nodePrintBalances = xmlConfDocument.CreateElement("PrintBalances");
                nodeRegister.AppendChild(nodePrintBalances);

                SaveTabularList(AllFields, ConfRegisterAccml.Value.TabularListPrintBalances, xmlConfDocument, nodePrintBalances);

                SaveForms(Conf, AllFields, null, ConfRegisterAccml.Value.Forms, xmlConfDocument, nodeRegister);
            }
        }

        /// <summary>
        /// Зберегти блоки запитів
        /// </summary>
        /// <param name="queryBlockList">Колеція блоків запитів</param>
        /// <param name="xmlConfDocument">Документ</param>
        /// <param name="rootNode">Корінна вітка</param>
        private static void SaveQueryBlockList(Dictionary<string, ConfigurationQueryBlock> queryBlockList, XmlDocument xmlConfDocument, XmlElement rootNode)
        {
            XmlElement nodeQueryBlockList = xmlConfDocument.CreateElement("QueryBlockList");
            rootNode.AppendChild(nodeQueryBlockList);

            foreach (ConfigurationQueryBlock queryBlock in queryBlockList.Values)
            {
                XmlElement nodeQueryBlock = xmlConfDocument.CreateElement("QueryBlock");
                nodeQueryBlockList.AppendChild(nodeQueryBlock);

                XmlElement nodeQueryBlockName = xmlConfDocument.CreateElement("Name");
                nodeQueryBlockName.InnerText = queryBlock.Name;
                nodeQueryBlock.AppendChild(nodeQueryBlockName);

                XmlElement nodeQueryBlockFinalCalculation = xmlConfDocument.CreateElement("FinalCalculation");
                nodeQueryBlockFinalCalculation.InnerText = queryBlock.FinalCalculation ? "1" : "0";
                nodeQueryBlock.AppendChild(nodeQueryBlockFinalCalculation);

                int position = 0;

                foreach (KeyValuePair<string, string> query in queryBlock.Query)
                {
                    XmlElement nodeQuery = xmlConfDocument.CreateElement("Query");
                    nodeQuery.SetAttribute("position", position++.ToString());
                    nodeQuery.SetAttribute("key", query.Key);
                    nodeQuery.AppendChild(xmlConfDocument.CreateCDataSection(query.Value));
                    nodeQueryBlock.AppendChild(nodeQuery);
                }
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
                string? dirName = Path.GetDirectoryName(pathToConf) ?? throw new Exception($"Не вдалось отримати шлях до папки із шляху конфігурації: {pathToConf}");
                string fileNewName = Path.GetFileNameWithoutExtension(pathToConf) + DateTime.Now.ToString("ddMMyyyyHHmmssFFFFFFF") + ".xml";
                string pathToCopyConf = Path.Combine(dirName, fileNewName);

                File.Copy(pathToConf, pathToCopyConf);

                if (!string.IsNullOrEmpty(oldCopyConf))
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
                string? dirName = Path.GetDirectoryName(pathToConf) ?? throw new Exception($"Не вдалось отримати шлях до папки із шляху конфігурації: {pathToConf}");
                string fileTempName = Path.GetFileNameWithoutExtension(pathToConf) + Guid.NewGuid().ToString().Replace("-", "") + ".xml";
                string pathToTempConf = Path.Combine(dirName, fileTempName);

                if (!string.IsNullOrEmpty(oldTempConf) && File.Exists(oldTempConf))
                    File.Delete(oldTempConf);

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
            if (File.Exists(pathToTempConf))
            {
                File.Copy(pathToTempConf, pathToConf, true);
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
            string? dirName = Path.GetDirectoryName(pathToConf) ?? throw new Exception($"Не вдалось отримати шлях до папки із шляху конфігурації: {pathToConf}");
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
        public static void GeneratedCode(string pathToConf, string pathToTemplate, string pathToSaveCode)
        {
            XslCompiledTransform xsltCodeGnerator = new();
            xsltCodeGnerator.Load(pathToTemplate);

            xsltCodeGnerator.Transform(pathToConf, pathToSaveCode);
        }

        /// <summary>
        /// Створення одного файлу з декількох
        /// </summary>
        /// <param name="pathToInformationSchemaXML">Шлях до схеми бази даних</param>
        /// <param name="сonfigurationFileName">Шлях до нової конфігурації</param>
        /// <param name="secondConfigurationFileName">Шлях до старої конфігурації</param>
        /// <param name="pathToSaveXml">Куди зберегти</param>
        public static void CreateOneFileForComparison(string pathToInformationSchemaXML,
            string сonfigurationFileName, string secondConfigurationFileName, string pathToSaveXml)
        {
            XmlWriterSettings settings = new() { NewLineChars = "\r\n", Indent = true, Encoding = Encoding.UTF8 };

            XmlWriter xmlWriter = XmlWriter.Create(pathToSaveXml, settings);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("root");

            //
            //InformationSchema
            //

            XmlDocument InformationSchemaDoc = new();
            InformationSchemaDoc.Load(pathToInformationSchemaXML);

            XmlNode? rootInformationSchema = InformationSchemaDoc.SelectSingleNode("InformationSchema");
            string OuterXmlInformationSchema = rootInformationSchema?.OuterXml ?? "";

            xmlWriter.WriteRaw(OuterXmlInformationSchema);
            xmlWriter.Flush();

            //
            //сonfiguration
            //

            XmlDocument сonfigurationDoc = new();
            сonfigurationDoc.Load(сonfigurationFileName);

            XmlNode? rootConfiguration = сonfigurationDoc.SelectSingleNode("Configuration");
            string OuterXmlConfiguration = rootConfiguration?.OuterXml ?? "";

            xmlWriter.WriteStartElement("NewConfiguration");
            xmlWriter.WriteRaw(OuterXmlConfiguration);
            xmlWriter.WriteEndElement();
            xmlWriter.Flush();

            //
            //secondConfiguration
            //

            XmlDocument secondConfigurationDoc = new();
            secondConfigurationDoc.Load(secondConfigurationFileName);

            XmlNode? rootSecondConfiguration = secondConfigurationDoc.SelectSingleNode("Configuration");
            string OuterXmlSecondConfiguration = rootSecondConfiguration?.OuterXml ?? "";

            xmlWriter.WriteStartElement("SecondConfiguration");
            xmlWriter.WriteRaw(OuterXmlSecondConfiguration);
            xmlWriter.WriteEndElement();
            xmlWriter.Flush();

            xmlWriter.WriteEndElement(); //root
            xmlWriter.Close();
        }

        /// <summary>
        /// Функція генерує ХМЛ файл на основі порівняння з базою даних та новою і старою конфігураціями
        /// </summary>
        /// <param name="pathOneFileForComparison">Шлях до ХМЛ згрупованого файлу</param>
        /// <param name="pathToTemplate">Шаблон</param>
        /// <param name="pathToSaveXml">Куди зберегти результати</param>
        public static void Comparison(string pathOneFileForComparison, string pathToTemplate, string pathToSaveXml)
        {
            XslCompiledTransform xsltCodeGnerator = new();

            xsltCodeGnerator.Load(pathToTemplate, new XsltSettings(false, false), null);

            XsltArgumentList xsltArgumentList = new();

            FileStream fileStream = new(pathToSaveXml, FileMode.Create);

            xsltCodeGnerator.Transform(pathOneFileForComparison, xsltArgumentList, fileStream);

            fileStream.Close();
        }

        /// <summary>
        /// Функція генерує ХМЛ файл з SQL запитами на основі файлу порівняння конфігурацій.
        /// </summary>
        /// <param name="pathToXML">Шлях до ХМЛ файлу, який згенерований функцією Comparison</param>
        /// <param name="pathToTemplate">Шлях до шаблону</param>
        /// <param name="pathToSaveCode">Шлях файлу куди буде збережений вихідний ХМЛ файл</param>
        /// <param name="replacementColumn">Параметр для шаблону (чи потрібно заміщати стовпчики)</param>
        public static void ComparisonAnalizeGeneration(string pathToXML, string pathToTemplate, string pathToSaveCode)
        {
            XslCompiledTransform xsltCodeGnerator = new();
            xsltCodeGnerator.Load(pathToTemplate, new XsltSettings(true, true), null);

            XsltArgumentList xsltArgumentList = new();
            xsltArgumentList.AddParam("KeyUID", "", DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss"));

            FileStream fileStream = new(pathToSaveCode, FileMode.Create);

            xsltCodeGnerator.Transform(pathToXML, xsltArgumentList, fileStream);

            fileStream.Close();
        }

        /// <summary>
        /// Трансформація ХМЛ документу
        /// </summary>
        /// <param name="xmlDoc">ХМЛ документ</param>
        /// <param name="pathToTemplate">Шаблон</param>
        /// <param name="arguments">Аргументи</param>
        /// <returns>Повертає результат трансформації</returns>
        public static string? Transform(XmlDocument xmlDoc, string pathToTemplate, Dictionary<string, object>? arguments)
        {
            XPathNavigator? navigator = xmlDoc.CreateNavigator();
            string result = "";

            if (navigator != null)
            {
                XslCompiledTransform xsltCodeGnerator = new();
                xsltCodeGnerator.Load(pathToTemplate, new XsltSettings(true, true), null);

                XsltArgumentList? xsltArgumentList = null;

                if (arguments != null)
                {
                    xsltArgumentList = new XsltArgumentList();
                    foreach (KeyValuePair<string, object> argument in arguments)
                        xsltArgumentList.AddParam(argument.Key, "", argument.Value);
                }

                using TextWriter writer = new StringWriter();
                xsltCodeGnerator.Transform(navigator, xsltArgumentList, writer);
                result = writer.ToString() ?? "";
            }

            return result;
        }

        /// <summary>
        /// Функція зчитує згенерований список SQL запитів функцією ComparisonAnalizeGeneration
        /// </summary>
        /// <param name="pathToXML">Шлях до файлу який згенерувала функція ComparisonAnalizeGeneration</param>
        /// <returns>Список SQL запитів</returns>
        public static List<string> ListComparisonSql(string pathToXML)
        {
            List<string> slqList = [];

            XPathDocument xPathDoc = new(pathToXML);
            XPathNavigator xPathDocNavigator = xPathDoc.CreateNavigator();

            XPathNodeIterator? sqlNodes = xPathDocNavigator.Select("/root/sql");
            if (sqlNodes != null)
                while (sqlNodes.MoveNext())
                {
                    string sqlText = sqlNodes.Current?.Value ?? "";

                    if (!string.IsNullOrEmpty(sqlText))
                        slqList.Add(sqlText);
                }

            return slqList;
        }

        #endregion

        #region Enums

        /// <summary>
        /// Варіант пошуку вказівників
        /// </summary>
        public enum VariantWorkSearchForPointers
        {
            /// <summary>
            /// Інформація
            /// </summary>
            Info,

            /// <summary>
            /// Таблиця
            /// </summary>
            Tables
        }

        /// <summary>
        /// Варіант завантаження конфігурації
        /// </summary>
        public enum VariantLoadConf
        {
            /// <summary>
            /// Повна загрузка (для конфігуратора)
            /// </summary>
            Full,

            /// <summary>
            /// Тільки необхідні компоненти (для готової програми)
            /// </summary>
            Small
        }

        /// <summary>
        /// Версія Gtk
        /// </summary>
        public enum GtkVersion
        {
            Gtk3,

            Gtk4
        }

        #endregion
    }
}