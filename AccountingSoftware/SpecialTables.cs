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

namespace AccountingSoftware
{
    /// <summary>
    /// Назви спеціальних таблиць
    /// </summary>
    public static class SpecialTables
    {
        /// <summary>
        /// Константи
        /// </summary>
        public static readonly string Constants = "tab_constants";

        /// <summary>
        /// Тригери для розрахунку регістрів накопичення
        /// </summary>
        public static readonly string RegAccumTriger = "tab_special_regaccum_triger";

        /// <summary>
        /// Список документів які ігноруються при обробці тригерів розрахунку регістрів накопичення
        /// </summary>
        public static readonly string RegAccumTrigerDocIgnore = "tab_special_regaccum_triger_docignore";

        /// <summary>
        /// Заблоковані обєкти (Довідники, Документи)
        /// </summary>
        public static readonly string LockedObject = "tab_special_locked_object";

        /// <summary>
        /// Користувічі
        /// </summary>
        public static readonly string Users = "tab_special_users";

        /// <summary>
        /// Активні користувачі
        /// </summary>
        public static readonly string ActiveUsers = "tab_special_active_users";

        /// <summary>
        /// Повнотекстовий пошук
        /// </summary>
        public static readonly string FullTextSearch = "tab_special_full_text_search";

        /// <summary>
        /// Помилки устарівша (!!! Не використовувати)
        /// </summary>
        [Obsolete("Не використовувати. Нова tab_special_message_error_or_info")]
        public static readonly string MessageErrorOld = "tab_special_message_error";

        /// <summary>
        /// Помилки та інформація
        /// </summary>
        public static readonly string MessageError = "tab_special_message_error_or_info";

        /// <summary>
        /// Тригери оновлення об’єктів (Довідники, Документи)
        /// </summary>
        public static readonly string ObjectUpdateTriger = "tab_special_object_update_triger";

        /// <summary>
        /// Історія змін обєктів
        /// </summary>
        public static readonly string ObjectVersionsHistory = "tab_special_object_versions_history";

        /// <summary>
        /// Історія змін табличних частин
        /// </summary>
        public static readonly string TablePartVersionsHistory = "tab_special_tablepart_versions_history";

        /// <summary>
        /// Хеш-коди для історії змін табличних частин
        /// </summary>
        public static readonly string TablePartVersionsHashData = "tab_special_tablepart_versions_hashdata";

        /// <summary>
        /// Список
        /// </summary>
        public static readonly string[] SpecialTablesList =
        [
            RegAccumTriger,
            RegAccumTrigerDocIgnore,
            LockedObject,
            Users,
            ActiveUsers,
            FullTextSearch,
            MessageError,
            ObjectUpdateTriger,
            ObjectVersionsHistory,
            TablePartVersionsHistory,
            TablePartVersionsHashData
        ];
    }

    /// <summary>
    /// Назви спеціальних функцій
    /// </summary>
    public static class SpecialFunc
    {
        /// <summary>
        /// Представлення для композитного типу даних. 
        /// 
        /// Функція повертає текстове представлення для вкзаного типу даних і унікального ідентифікатора.
        /// В залежності від типу даних відбувається вибірка відповідних полів які відмічені галочкою Представлення в конфігураторі з відповідних таблиць.  
        /// </summary>
        public static readonly string CompisitePresentation = "func_special_composite_presentation";

        /*
        В базі даних створюються дві функції з назвою func_special_composite_presentation але з різними параметрами:

            1.

            func_special_composite_presentation(data uuidtext)

                Параметр типу uuidtext (в C# це UuidAndText). Містить в собі унікальний ідентифікатор та тип даних. 
                Тип даних задається наступним чином: 'Довідники.Номенклатура' або 'Документи.ПрибутковаНакладна'.
                Наприклад: (483508ea-4c20-495e-8a8f-941978243826,Довідники.СтруктураПідприємства) або 
                           (db9b90ef-9f06-4765-a98a-bdb640903543,Довідники.Номенклатура)

                Якщо потрібно зробити відбір запитом тоді пишеться наступним чином:
                    SELECT * FROM table
                    WHERE field = '(db9b90ef-9f06-4765-a98a-bdb640903543,Довідники.Номенклатура)'::uuidtext
           
                    або

                    SELECT * FROM table
                    WHERE (field).text = 'Довідники.Номенклатура'

                    або 

                    SELECT * FROM table
                    WHERE (field).uuid = 'db9b90ef-9f06-4765-a98a-bdb640903543'

                    або 

                    SELECT * FROM table
                    WHERE (field).text = 'Довідники.Номенклатура' AND (field).uuid = 'db9b90ef-9f06-4765-a98a-bdb640903543'


            2.

            func_special_composite_presentation(row_uid uuid, conf_type nametext)

                Два параметри uuid та nametext. Перший це унікальний ідентифікатор а другий це тип даних, зразу розділений на два поля
                перше це одне із двох 'Довідники' або 'Документи'. Друге це назва довідника чи документу, так як він заданий в конфігурації.
                Наприклад (Довідники,Номенклатура) або (Документи,БухгалтерськаОперація)
            
                Якщо потрібно зробити відбір запитом тоді пишеться наступним чином:
                    SELECT * FROM table
                    WHERE field = '(Документи,БухгалтерськаОперація)'::nametext

                    або

                    SELECT * FROM table
                    WHERE (field).name = 'Документи'

                    або

                    SELECT * FROM table
                    WHERE (field).text = 'БухгалтерськаОперація'
                    
                    або

                     SELECT * FROM table
                    WHERE (field).name = 'Документи' AND (field).text = 'БухгалтерськаОперація'
        
        */
    }
}