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

using Gtk;
using AccountingSoftware;

namespace InterfaceGtk
{
    /// <summary>
    /// Основа для класів:
    ///             ДовідникЖурнал, 
    ///             ДовідникШвидкийВибір, 
    ///             ДокументЖурнал, 
    ///             Журнал, 
    ///             РегістриВідомостейЖурнал, 
    ///             РегістриНакопиченняЖурнал
    /// </summary>
    public abstract class ФормаЖурнал : Форма
    {
        /// <summary>
        /// Вспливаюче вікно власник в який поміщена ФормаЖурнал
        /// </summary>
        public Popover? PopoverParent { get; set; }

        /// <summary>
        /// Елемент на який треба спозиціонувати список при обновленні
        /// </summary>
        public UnigueID? SelectPointerItem { get; set; }

        /// <summary>
        /// Дерево
        /// </summary>
        protected TreeView TreeViewGrid = new TreeView();

        public ФормаЖурнал()
        {
            TreeViewGrid.Selection.Mode = SelectionMode.Multiple;
            TreeViewGrid.ActivateOnSingleClick = true;
        }

        /// <summary>
        /// Функція повертає список UnigueID виділених рядків дерева
        /// </summary>
        public List<UnigueID> GetSelectedRows()
        {
            List<UnigueID> unigueIDList = [];

            if (TreeViewGrid.Selection.CountSelectedRows() != 0)
                foreach (TreePath itemPath in TreeViewGrid.Selection.GetSelectedRows())
                {
                    TreeViewGrid.Model.GetIter(out TreeIter iter, itemPath);
                    unigueIDList.Add(new UnigueID((string)TreeViewGrid.Model.GetValue(iter, 1)));
                }
                
            return unigueIDList;
        }

        #region Virtual & Abstract Function

        /// <summary>
        /// Присвоєння значень
        /// </summary>
        public abstract ValueTask SetValue();

        /// <summary>
        /// Завантаження списку
        /// </summary>
        protected abstract ValueTask LoadRecords();

        /// <summary>
        /// Завантаження списку про пошуку
        /// </summary>
        protected abstract ValueTask LoadRecords_OnSearch(string searchText);

        #endregion
    }
}