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

using AccountingSoftware;
using Gtk;

namespace InterfaceGtk
{
    public static class NotebookFunction
    {
        public const string DataKey_HistorySwitchList = "history_switch_list";
        public const string DataKey_ParentNotebook = "parent_notebook";
        public const string DataKey_AfterClosePageFunc = "after_close_page_func";
        public const string DataKey_ObjectChangeEvents = "object_change_events";
        public const string DataKey_LockObjectPageFunc = "lock_object_page_func";

        /// <summary>
        /// Функція створює блокнот з верхнім положенням вкладок
        /// </summary>
        /// <param name="history_switch_list">Збереження історії переключення вкладок</param>
        /// <returns></returns>
        public static Notebook CreateNotebook(bool history_switch_list = true)
        {
            Notebook notebook = new Notebook()
            {
                Scrollable = true,
                //EnablePopup = true,
                BorderWidth = 0,
                ShowBorder = false,
                TabPos = PositionType.Top,
            };

            notebook.KeyReleaseEvent += OnNotebookKeyReleaseEvent;

            if (history_switch_list)
            {
                //Список для збереження історії переключення вкладок
                notebook.Data.Add(DataKey_HistorySwitchList, new List<string>());

                //Обробка переключення вкладок
                notebook.SwitchPage += (sender, args) =>
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

            //Структура для функцій реакцій на зміни об'єктів
            notebook.Data.Add(DataKey_ObjectChangeEvents,
                new Dictionary<GroupObjectChangeEvents, List<(string codePage, Func<List<ObjectChanged>, ValueTask> func, string[] pointersType)>>
                {
                    { GroupObjectChangeEvents.Directory, [] },
                    { GroupObjectChangeEvents.Document, [] }
                }
            );

            //Структура для функцій розблокування об'єктів
            notebook.Data.Add(DataKey_LockObjectPageFunc, new Dictionary<string, Func<ValueTask>>());

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
        /// <param name="notClosePage">Забрати можливість закривати сторінку</param>
        public static void CreateNotebookPage(Notebook? notebook, string tabName, Func<Widget>? pageWidget, bool insertPage = false,
            System.Action? beforeOpenPageFunc = null, System.Action? afterClosePageFunc = null, bool notClosePage = false)
        {
            if (notebook != null)
            {
                int numPage;
                string codePage = Guid.NewGuid().ToString();

                ScrolledWindow scroll = new ScrolledWindow() { Name = codePage };
                scroll.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);

                Box hBoxLabel = CreateLabelPageWidget(notebook, tabName, codePage, notClosePage);

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
        /// <param name="notClosePage">Забрати можливість закривати сторінку</param>
        /// <returns>НBox</returns>
        static Box CreateLabelPageWidget(Notebook? notebook, string caption, string codePage, bool notClosePage = false)
        {
            Box hBox = new Box(Orientation.Horizontal, 0);

            Box hBoxForSpinner = new Box(Orientation.Horizontal, 0) { Name = "BoxForSpinner" };
            hBoxForSpinner.PackStart(new Image(Іконки.ДляКнопок.Doc), false, false, 0);

            //Ico / BoxForSpinner
            hBox.PackStart(hBoxForSpinner, false, false, 0);

            //Текст
            Label label = new Label { Name = "Caption", Text = SubstringPageName(caption), TooltipText = caption };
            hBox.PackStart(label, false, false, 3);

            if (!notClosePage)
            {
                //Лінк закриття сторінки
                LinkButton lbClose = new LinkButton("Close", "")
                {
                    Image = new Image(Іконки.ДляКнопок.Clean),
                    AlwaysShowImage = true,
                    Name = codePage,
                    TooltipText = "Закрити"
                };

                lbClose.Clicked += (sender, args) => CloseNotebookPageToCode(notebook, codePage);

                hBox.PackEnd(lbClose, false, false, 0);
            }

            hBox.ShowAll();

            return hBox;
        }

        /// <summary>
        /// Закрити сторінку блокноту по коду
        /// </summary>
        /// <param name="notebook">Блокнот</param>
        /// <param name="codePage">Код</param>
        public static void CloseNotebookPageToCode(Notebook? notebook, string codePage)
        {
            notebook?.Foreach(
                async wg =>
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

                        //Очищення вказівників на функції реакції на зміни об'єктів
                        var objectChangeEvents = GetDataObjectChangeEvents(notebook);
                        if (objectChangeEvents != null)
                        {
                            foreach (var item in objectChangeEvents.Values)
                                item.RemoveAll(x => x.codePage == codePage);
                        }

                        //Розблокування об'єкту після закриття сторінки шляхом виклику відповідної функції
                        var lockObjectPageFunc = GetDataLockObjectFunc(notebook);
                        if (lockObjectPageFunc != null && lockObjectPageFunc.TryGetValue(codePage, out Func<ValueTask>? unlockFunc))
                        {
                            await unlockFunc.Invoke();
                            lockObjectPageFunc.Remove(codePage);
                        }

                        notebook.DetachTab(wg);

                        GC.Collect();
                    }
                });
        }

        /// <summary>
        /// Відображення спінера або іконки
        /// </summary>
        /// <param name="notebook">Блокнот</param>
        /// <param name="active">Спінер якшо true, Іконка якщо false</param>
        /// <param name="codePage">Код сторінки</param>
        public static void SpinnerNotebookPageToCode(Notebook? notebook, bool active, string codePage)
        {
            notebook?.Foreach(
                wg =>
                {
                    if (wg.Name == codePage)
                    {
                        foreach (Widget children in ((Box)notebook.GetTabLabel(wg)).Children)
                            if (children is Box boxForSpiner && boxForSpiner.Name == "BoxForSpinner")
                            {
                                foreach (var item in boxForSpiner.Children)
                                    boxForSpiner.Remove(item);

                                if (active)
                                    boxForSpiner.PackStart(new Spinner() { Active = true }, false, false, 0);
                                else
                                    boxForSpiner.PackStart(new Image(Іконки.ДляКнопок.Doc), false, false, 0);

                                boxForSpiner.ShowAll();
                                break;
                            }
                    }
                });
        }

        /// <summary>
        /// Перейменувати сторінку по коду
        /// </summary>
        /// <param name="notebook">Блокнот</param>
        /// <param name="caption">Нова назва</param>
        /// <param name="codePage">Код</param>
        public static void RenameNotebookPageToCode(Notebook? notebook, string caption, string codePage)
        {
            notebook?.Foreach(
                wg =>
                {
                    if (wg.Name == codePage)
                    {
                        foreach (Widget children in ((Box)notebook.GetTabLabel(wg)).Children)
                            if (children is Label label && label.Name == "Caption")
                            {
                                label.Text = SubstringPageName(caption);
                                label.TooltipText = caption;
                                break;
                            }
                    }
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
                wg =>
                {
                    if (wg.Name == codePage)
                    {
                        notebook.CurrentPage = counter;

                        //Передати фокус
                        if (notebook.CurrentPageWidget is ScrolledWindow scroll)
                            if (scroll.Child is Viewport viewport)
                                viewport.Child?.GetType().GetMethod("DefaultGrabFocus")?.Invoke(viewport.Child, null);
                    }

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
                wg =>
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
        public static string SubstringPageName(string pageName)
        {
            return pageName.Length >= 23 ? pageName[..20] : pageName;
        }

        static void OnNotebookKeyReleaseEvent(object sender, KeyReleaseEventArgs args)
        {
            Notebook notebook = (Notebook)sender;

            if (notebook.IsFocus)
                switch (args.Event.Key)
                {
                    case Gdk.Key.Escape:
                        {
                            Widget widget = notebook.CurrentPageWidget;
                            if (widget != null)
                                //Пошук в назві вкладки LinkButton для закриття сторінки
                                foreach (Widget children in ((Box)notebook.GetTabLabel(widget)).Children)
                                    if (children is LinkButton lbClose && lbClose.Uri == "Close")
                                    {
                                        CloseNotebookPageToCode(notebook, lbClose.Name);
                                        break;
                                    }
                            break;
                        }
                }
        }

        #region ObjectChangeEvents

        /*

        Цей блок використовується для прив'язки до блокнота функцій оновлення журналів документів та довідників після змін об'єктів.
        Прив'язка функції відбувається до сторінки блокноту.
        Коли сторінка блокноту закривається прив'язана функція видаляється.

        ConnectingToKernelObjectChangeEvents викликається після запуску програми один раз для ініціалізації.

        AddChangeFunc викликається із журналу довідника чи документу для привязки функції.
        AddChangeFuncJournal викликається із журналу документів для прив'язки функції.

        */

        /// <summary>
        /// Підключення до подій зміни об’єкта
        /// </summary>
        /// <param name="notebook">Блокнот</param>
        /// <param name="kernel">Ядро</param>
        public static void ConnectingToKernelObjectChangeEvents(Notebook notebook, Kernel kernel)
        {
            //Внутрішня функція для виклику функцій реакції на зміни об'єктів
            async void InvokeObjectChangeEvents(Notebook notebook, GroupObjectChangeEvents group, Dictionary<string, List<ObjectChanged>> directoryOrDocument)
            {
                var objectChangeEvents = GetDataObjectChangeEvents(notebook);
                if (objectChangeEvents != null && objectChangeEvents[group].Count > 0)
                    try
                    {
                        foreach (var (_, func, pointersType) in objectChangeEvents[group])
                        {
                            List<ObjectChanged> listChanged = [];
                            foreach (string pointerType in pointersType)
                                if (directoryOrDocument.TryGetValue(pointerType, out var value))
                                    listChanged.AddRange(value);

                            if (listChanged.Count > 0)
                                await func.Invoke(listChanged);
                        }
                    }
                    catch (Exception) { }
            }

            //Зміни в довідниках
            kernel.DirectoryObjectChanged += (_, directory) => InvokeObjectChangeEvents(notebook, GroupObjectChangeEvents.Directory, directory);

            //Зміни в документах
            kernel.DocumentObjectChanged += (_, document) => InvokeObjectChangeEvents(notebook, GroupObjectChangeEvents.Document, document);
        }

        /// <summary>
        /// Функція повертає колекцію функцій реакції на зміни об'єктів
        /// </summary>
        static Dictionary<GroupObjectChangeEvents, List<(string codePage, Func<List<ObjectChanged>, ValueTask> func, string[] pointersType)>>? GetDataObjectChangeEvents(Notebook? notebook)
        {
            var object_change_events = notebook?.Data[DataKey_ObjectChangeEvents];
            return object_change_events != null ? (Dictionary<GroupObjectChangeEvents, List<(string, Func<List<ObjectChanged>, ValueTask>, string[])>>)object_change_events : null;
        }

        /// <summary>
        /// Добавлення функції реакції на зміни об'єктів
        /// </summary>
        /// <param name="notebook">Блокнот</param>
        /// <param name="codePage">Код сторінки</param>
        /// <param name="func">Функція</param>
        /// <param name="pointerPattern">Фільтр по типу даних</param>
        public static void AddChangeFunc(Notebook? notebook, string codePage, Func<List<ObjectChanged>, ValueTask> func, string pointerPattern)
        {
            var (_, pointerGroup, pointerType) = Configuration.PointerParse(pointerPattern, out Exception? ex);

            GroupObjectChangeEvents group = pointerGroup switch
            {
                "Довідники" => GroupObjectChangeEvents.Directory,
                "Документи" => GroupObjectChangeEvents.Document,
                _ => throw ex ?? new Exception("Тільки 'Довідники' або 'Документи'")
            };

            var objectChangeEvents = GetDataObjectChangeEvents(notebook);
            objectChangeEvents?[group].Add((codePage, func, [pointerType]));
        }

        /// <summary>
        /// Добавлення функції реакції на зміни об'єктів для журналу документів
        /// </summary>
        /// <param name="notebook">Блокнот</param>
        /// <param name="codePage">Код сторінки</param>
        /// <param name="func">Функція</param>
        /// <param name="typeDocs">Типи документів які належать журналу</param>
        public static void AddChangeFuncJournal(Notebook? notebook, string codePage, Func<List<ObjectChanged>, ValueTask> func, string[] allowDocument)
        {
            var objectChangeEvents = GetDataObjectChangeEvents(notebook);
            objectChangeEvents?[GroupObjectChangeEvents.Document].Add((codePage, func, allowDocument));
        }

        enum GroupObjectChangeEvents
        {
            Directory,
            Document
        }

        #endregion

        #region Lock Element

        static Dictionary<string, Func<ValueTask>>? GetDataLockObjectFunc(Notebook? notebook)
        {
            var lock_object_page_func = notebook?.Data[DataKey_LockObjectPageFunc];
            return lock_object_page_func != null ? (Dictionary<string, Func<ValueTask>>)lock_object_page_func : null;
        }

        /// <summary>
        /// Додати функцію блокування та розблокування об'єкта
        /// </summary>
        /// <param name="notebook">Блокнот</param>
        /// <param name="codePage">Код сторінки</param>
        /// <param name="lockFunc">Функція блокування</param>
        /// <param name="unlockFunc">Функція розблокування</param>
        public static async ValueTask AddLockObjectFunc(Notebook? notebook, string codePage, AccountingSoftware.Object accountingObject)
        {
            //Функція розблокування після закриття сторінки
            var lockObjectPageFunc = GetDataLockObjectFunc(notebook);
            lockObjectPageFunc?.Add(codePage, accountingObject.UnLock);

            //Блокування
            await accountingObject.Lock();
        }

        #endregion
    }
}