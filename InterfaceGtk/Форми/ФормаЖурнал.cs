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
    ///             РегістриНакопиченняЖурнал,
    ///             РегістриНакопиченняЖурнал_СпрощенийРежим
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

        /// <summary>
        /// Прокрутка дерева
        /// </summary>
        protected ScrolledWindow ScrollTree = new ScrolledWindow() { ShadowType = ShadowType.In };

        /// <summary>
        /// Прокрутка для сторінок
        /// </summary>
        protected ScrolledWindow ScrollPages = new ScrolledWindow() { ShadowType = ShadowType.None };

        /// <summary>
        /// Бокс для сторінок
        /// </summary>
        protected Box HBoxPages = new Box(Orientation.Horizontal, 0);

        public ФормаЖурнал()
        {
            TreeViewGrid.Selection.Mode = SelectionMode.Multiple;
            TreeViewGrid.ActivateOnSingleClick = true;

            ScrollTree.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);

            ScrollPages.SetPolicy(PolicyType.Automatic, PolicyType.Never);
            ScrollPages.Add(HBoxPages);
        }

        /// <summary>
        /// Спрощений режим
        /// Прапорець який на даний момент ні на що не впривляє
        /// </summary>
        public bool LiteMode { get; set; } = false;

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
        /// Додаткова функція яка викликається із SetValue
        /// </summary>
        protected virtual async ValueTask BeforeSetValue() { await ValueTask.FromResult(true); }

        /// <summary>
        /// Завантаження списку
        /// </summary>
        public abstract ValueTask LoadRecords();

        /// <summary>
        /// Завантаження списку при пошуку
        /// </summary>
        public virtual async ValueTask LoadRecords_OnSearch(string searchText) { await ValueTask.FromResult(true); }

        /// <summary>
        /// Функція для повного обновлення списку
        /// </summary>
        /// <returns></returns>
        // public async ValueTask ReLoadRecords()
        // {
        //     ClearPages();
        //     await LoadRecords();
        // }

        #endregion

        #region ToolBar

        protected void ToolButtonSensitive(object? sender, bool sensitive)
        {
            if (sender != null && sender is ToolButton button)
                button.Sensitive = sensitive;
        }

        #endregion

        #region  TreeView

        protected void ClearPages()
        {
            ТабличнийСписок.ОчиститиСторінки(TreeViewGrid);
        }

        protected void PagesShow(Func<ValueTask>? funcLoadRecords = null)
        {
            const int offset = 5;
            bool writeSpace = false;

            //Очищення
            foreach (var widget in HBoxPages.Children)
                HBoxPages.Remove(widget);

            Сторінки.Налаштування? settings = ТабличнийСписок.ОтриматиСторінки(TreeViewGrid);
            if (settings != null && settings.Record.Pages > 1)
            {
                HBoxPages.PackStart(new Label("<b>Сторінки:</b> ") { UseMarkup = true }, false, false, 2);

                for (int i = 1; i <= settings.Record.Pages; i++)
                    if (i == settings.CurrentPage)
                        HBoxPages.PackStart(new Label($"<b>{i}</b>") { UseMarkup = true }, false, false, 19);
                    else if (i == 1 || i == settings.Record.Pages || (i > settings.CurrentPage - offset && i < settings.CurrentPage + offset))
                    {
                        LinkButton link = new LinkButton(i.ToString()) { Name = i.ToString() };
                        link.Clicked += async (sender, args) =>
                        {
                            settings.CurrentPage = int.Parse(link.Name);
                            if (funcLoadRecords != null) await funcLoadRecords.Invoke();
                        };

                        HBoxPages.PackStart(link, false, false, 0);
                        writeSpace = false;
                    }
                    else if (!writeSpace)
                    {
                        HBoxPages.PackStart(new Label("..") { UseMarkup = true }, false, false, 19);
                        writeSpace = true;
                    }
            }

            HBoxPages.ShowAll();
        }

        #endregion

        #region ScrolledWindow

        // async void OnValueChanged(object? sender, EventArgs args)
        // {
        //if (!ТабличнийСписок.СтанДоповнення(TreeViewGrid))
        /*if (ScrollTree.Vadjustment.Value == 0)
        {
            ТабличнийСписок.ТипДоповнення(TreeViewGrid, Прокручування.ТипДобавленняДаних.Зверху);
            await LoadRecords_OnScrolling();
        }
        else if (ScrollTree.Vadjustment.Upper == ScrollTree.Vadjustment.Value + ScrollTree.Vadjustment.PageSize)
        {
            ТабличнийСписок.ТипДоповнення(TreeViewGrid, Прокручування.ТипДобавленняДаних.Знизу);
            await LoadRecords_OnScrolling();
        }*/

        //     await ValueTask.FromResult(true);
        // }

        #endregion
    }
}