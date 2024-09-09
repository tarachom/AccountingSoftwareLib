/*
Copyright (C) 2019-2024 TARAKHOMYN YURIY IVANOVYCH
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

using Gtk;
using AccountingSoftware;

namespace InterfaceGtk
{
    /// <summary>
    /// Основа для класів:
    ///      ДовідникЕлемент, 
    ///      ДокументЕлемент, 
    ///      РегістриВідомостейЕлемент
    /// </summary>
    public abstract class ФормаЕлемент : Форма
    {
        /// <summary>
        /// Чи це новий елемент
        /// </summary>
        public bool IsNew { get; set; } = true;

        /// <summary>
        /// ІД елементу
        /// </summary>
        public UnigueID? UnigueID { get; set; }

        /// <summary>
        /// Назва         
        /// </summary>
        public string Caption { get; set; } = "";

        /// <summary>
        /// Функція зворотнього виклику для перевантаження списку
        /// </summary>
        public Action<UnigueID?>? CallBack_LoadRecords { get; set; }

        public ФормаЕлемент() { }

        #region Event Function

        public void UnigueIDChanged(object? _, UnigueID unigueID)
        {
            UnigueID = unigueID;
        }
        
        public void CaptionChanged(object? _, string caption)
        {
            Caption = caption;
        }

        #endregion

        #region Abstract Function

        /// <summary>
        /// Присвоєння значень
        /// </summary>
        public abstract void SetValue();

        /// <summary>
        /// Зчитування значень
        /// </summary>
        protected abstract void GetValue();

        /// <summary>
        /// Збереження
        /// </summary>
        protected abstract ValueTask<bool> Save();

        #endregion
    }
}