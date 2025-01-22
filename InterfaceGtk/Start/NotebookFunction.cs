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

namespace InterfaceGtk
{
    public static class NotebookFunction
    {
        public const string DataKey_HistorySwitchList = "history_switch_list";
        public const string DataKey_ParentNotebook = "parent_notebook";
        public const string DataKey_AfterClosePageFunc = "after_close_page_func";

        /// <summary>
        /// Функція створює блокнок з верхнім положенням вкладок
        /// </summary>
        /// <param name="history_switch_list">Збереження історії переключення вкладок</param>
        /// <returns></returns>
        public static Notebook CreateNotebook(bool history_switch_list = true)
        {
            Notebook notebook = new Notebook()
            {
                Scrollable = true,
                EnablePopup = true,
                BorderWidth = 0,
                ShowBorder = false,
                TabPos = PositionType.Top,
            };

            if (history_switch_list)
            {
                //Список для збереження історії переключення вкладок
                notebook.Data.Add(DataKey_HistorySwitchList, new List<string>());

                //Обробка переключення вкладок
                notebook.SwitchPage += (object? sender, SwitchPageArgs args) =>
                {
                    string currPageUID = args.Page.Name;

                    var history_switch_list = notebook.Data[DataKey_HistorySwitchList];
                    if (history_switch_list != null)
                    {
                        List<string> historySwitchList = (List<string>)history_switch_list;
                        historySwitchList.Remove(currPageUID); // Поточна сторінка видаляється із списку, якщо вона там є
                        historySwitchList.Add(currPageUID); // Поточна сторінка ставиться у кінець списку
                    }
                };
            }

            return notebook;
        }

        /// <summary>
        /// Функція повертає блокнот із віджету
        /// </summary>
        /// <param name="widget">Віджет</param>
        /// <returns></returns>
        public static Notebook? GetNotebookFromWidget(Widget widget)
        {
            object? objNotebook = widget.Data.ContainsKey(DataKey_ParentNotebook) ? widget.Data[DataKey_ParentNotebook] : null;
            return objNotebook != null ? (Notebook)objNotebook : null;
        }

        /// <summary>
        /// Створити сторінку в блокноті.
        /// Код сторінки задається в назву віджета - widget.Name = codePage;
        /// </summary>
        /// <param name="notebook">Блокнот</param>
        /// <param name="tabName">Назва сторінки</param>
        /// <param name="pageWidget">Віджет для сторінки</param>
        /// <param name="insertPage">Вставити сторінку перед поточною</param>
        /// <param name="beforeOpenPageFunc">Функція яка викликається перед відкриттям сторінки блокноту</param>
        /// <param name="afterClosePageFunc">Функція яка викликається після закриття сторінки блокноту</param>
        public static void CreateNotebookPage(Notebook? notebook, string tabName, Func<Widget>? pageWidget, bool insertPage = false,
            System.Action? beforeOpenPageFunc = null, System.Action? afterClosePageFunc = null)
        {
            if (notebook != null)
            {
                int numPage;
                string codePage = Guid.NewGuid().ToString();

                ScrolledWindow scroll = new ScrolledWindow() { Name = codePage };
                scroll.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);

                Box hBoxLabel = CreateLabelPageWidget(notebook, tabName, codePage);

                if (insertPage)
                    numPage = notebook.InsertPage(scroll, hBoxLabel, notebook.CurrentPage);
                else
                    numPage = notebook.AppendPage(scroll, hBoxLabel);

                //Переміщення сторінок
                notebook.SetTabReorderable(scroll, true);

                if (pageWidget != null)
                {
                    Widget widget = pageWidget.Invoke();
                    scroll.Add(widget);

                    widget.Name = codePage;

                    //Додатковий вказівник на блокнот вкладаю у віджет
                    //Функція GetNotebookFromWidget() отримує вказівник на блокнот з віджету
                    widget.Data.Add(DataKey_ParentNotebook, notebook);
                }

                //Додаткова функція яка викликається до відкриття сторінки блокноту
                beforeOpenPageFunc?.Invoke();

                //Додаткова функція яка викликається після закриття сторінки блокноту
                if (afterClosePageFunc != null)
                    scroll.Data.Add(DataKey_AfterClosePageFunc, afterClosePageFunc);

                notebook.ShowAll();
                notebook.CurrentPage = numPage;
                notebook.GrabFocus();
            }
        }

        /// <summary>
        /// Заголовок сторінки блокноту
        /// </summary>
        /// <param name="caption">Заголовок</param>
        /// <param name="codePage">Код сторінки</param>
        /// <param name="notebook">Блокнот</param>
        /// <returns>НBox</returns>
        public static Box CreateLabelPageWidget(Notebook? notebook, string caption, string codePage)
        {
            Box hBoxLabel = new Box(Orientation.Horizontal, 0);

            //Ico
            hBoxLabel.PackStart(new Image(Іконки.ДляКнопок.Doc), false, false, 0);

            //Текст
            Label label = new Label { Text = SubstringPageName(caption), TooltipText = caption };
            hBoxLabel.PackStart(label, false, false, 3);

            //Лінк закриття сторінки
            LinkButton lbClose = new LinkButton("", "")
            {
                Image = new Image(Іконки.ДляКнопок.Clean),
                AlwaysShowImage = true,
                Name = codePage,
                TooltipText = "Закрити"
            };

            lbClose.Clicked += (object? sender, EventArgs args) => CloseNotebookPageToCode(notebook, codePage);

            hBoxLabel.PackEnd(lbClose, false, false, 0);
            hBoxLabel.ShowAll();

            return hBoxLabel;
        }

        /// <summary>
        /// Закрити сторінку блокноту по коду
        /// </summary>
        /// <param name="notebook">Блокнот</param>
        /// <param name="codePage">Код</param>
        public static void CloseNotebookPageToCode(Notebook? notebook, string codePage)
        {
            notebook?.Foreach(
                (Widget wg) =>
                {
                    if (wg.Name == codePage)
                    {
                        //Історія переключення сторінок
                        var history_switch_list = notebook.Data[DataKey_HistorySwitchList];
                        if (history_switch_list != null)
                        {
                            List<string> historySwitchList = (List<string>)history_switch_list;
                            historySwitchList.Remove(codePage);

                            if (historySwitchList.Count > 0)
                                CurrentNotebookPageToCode(notebook, historySwitchList[^1]);
                        }

                        //Додаткова функція яка викликається після закриття сторінки блокноту
                        var after_close_page_func = wg.Data[DataKey_AfterClosePageFunc];
                        if (after_close_page_func != null)
                        {
                            System.Action afterClosePageFunc = (System.Action)after_close_page_func;
                            afterClosePageFunc.Invoke();
                        }

                        notebook.DetachTab(wg);
                    }
                });
        }

        /// <summary>
        /// Перейменувати сторінку по коду
        /// </summary>
        /// <param name="notebook">Блокнот</param>
        /// <param name="name">Нова назва</param>
        /// <param name="codePage">Код</param>
        public static void RenameNotebookPageToCode(Notebook? notebook, string name, string codePage)
        {
            Box hBoxLabel = CreateLabelPageWidget(notebook, name, codePage);

            notebook?.Foreach(
                (Widget wg) =>
                {
                    if (wg.Name == codePage)
                        notebook.SetTabLabel(wg, hBoxLabel);
                });
        }

        /// <summary>
        /// Встановлення поточної сторінки по коду
        /// </summary>
        /// <param name="notebook">Блокнот</param>
        /// <param name="codePage">Код</param>
        public static void CurrentNotebookPageToCode(Notebook? notebook, string codePage)
        {
            int counter = 0;

            notebook?.Foreach(
                (Widget wg) =>
                {
                    if (wg.Name == codePage)
                        notebook.CurrentPage = counter;

                    counter++;
                });
        }

        /// <summary>
        /// Блокування чи розблокування поточної сторінки по коду
        /// </summary>
        /// <param name="notebook">Блокнот</param>
        /// <param name="codePage">Код</param>
        /// <param name="sensitive">Значення</param>
        public static void SensitiveNotebookPageToCode(Notebook? notebook, string codePage, bool sensitive)
        {
            notebook?.Foreach(
                (Widget wg) =>
                {
                    if (wg.Name == codePage)
                        wg.Sensitive = sensitive;
                });
        }

        /// <summary>
        /// Обрізати імя для сторінки
        /// </summary>
        /// <param name="pageName"></param>
        /// <returns></returns>
        static string SubstringPageName(string pageName)
        {
            return pageName.Length >= 20 ? pageName[..17] + "..." : pageName;
        }
    }
}