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
            ObjectVersionsHistory
        ];
    }
}