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

using Gdk;
using Gtk;

namespace InterfaceGtk4;

public static class NotebookFunction
{
    public const string DataKey_ObjectChangeEvents = "object_change_events";
    public const string DataKey_LockObjectPageFunc = "lock_object_page_func";


    /// <summary>
    /// Історія переключення вкладок для блокнотів
    /// Ключ - назва блокноту
    /// Список - коди сторінок
    /// </summary>
    static readonly Dictionary<string, List<string>> HistorySwitch = [];

    /// <summary>
    /// Функції які будуть викликані після закриття сторінки блокнотів
    /// Ключ - назва блокноту
    /// Список - код сторінки і функція
    /// </summary>
    static readonly Dictionary<string, Dictionary<string, Action>> AfterClosePageFunc = [];

    /// <summary>
    /// Основний блокнот для форми
    /// Може бути тільки один основний блокнот на даний момент
    /// </summary>
    static Notebook? generalNotebook;
    public static Notebook? GeneralNotebook
    {
        get
        {
            return generalNotebook;
        }
        set
        {
            if (generalNotebook is null)
                generalNotebook = value;
            else
                throw new Exception("Головний блокнот вже встановлений!");
        }
    }

    /// <summary>
    /// Функція створює блокнот з верхнім положенням вкладок
    /// </summary>
    /// <param name="historySwitchList">Збереження історії переключення вкладок</param>
    /// <param name="isGeneralNotebook">Чи це головний блокнот?</param>
    /// <returns>Notebook</returns>
    public static Notebook CreateNotebook(bool historySwitchList = true, bool isGeneralNotebook = false)
    {
        Notebook notebook = new()
        {
            Scrollable = true,
            ShowBorder = false,
            TabPos = PositionType.Top,
            Hexpand = true,
            Vexpand = true,
            Name = Guid.NewGuid().ToString()
        };

        EventControllerKey controller = EventControllerKey.New();
        notebook.AddController(controller);
        controller.OnKeyReleased += (sender, args) =>
        {
            Console.WriteLine(args.Keycode);

            if (notebook.IsFocus())
                if (args.Keycode == 9 || args.Keycode == 27) // Esc Console.WriteLine(char.ConvertToUtf32("\e", 0));
                {
                    Widget? wg = notebook.GetNthPage(notebook.GetCurrentPage());
                    if (wg != null)
                    {
                        Box? tabLabel = (Box?)notebook.GetTabLabel(wg);
                        if (tabLabel != null)
                        {
                            Widget? child = tabLabel.GetFirstChild();
                            while (child != null)
                            {
                                //Бокс для кнопки
                                if (child is Box box && box.Name == "Close")
                                {
                                    //Кнопка Закрити
                                    Widget? bClose = child.GetFirstChild();
                                    if (bClose != null)
                                    {
                                        string? codePage = bClose.Name;

                                        if (codePage != null)
                                            CloseNotebookPageToCode(notebook, codePage);
                                    }

                                    break;
                                }

                                child = child.GetNextSibling();
                            }
                        }
                    }
                }
        };

        //Встановлення головного блокнота
        if (isGeneralNotebook) GeneralNotebook = notebook;

        //Обробка переключення вкладок
        if (historySwitchList)
        {
            HistorySwitch.Add(notebook.Name, []);

            notebook.OnSwitchPage += (_, args) =>
            {
                string? currNotebookName = notebook.Name;
                string? currPageName = args.Page.Name;

                if (currNotebookName != null && currPageName != null && HistorySwitch.TryGetValue(currNotebookName, out List<string>? listPageName))
                {
                    listPageName.Remove(currPageName); // Поточна сторінка видаляється із списку, якщо вона там є
                    listPageName.Add(currPageName); // Поточна сторінка ставиться у кінець списку
                }
            };
        }

        /*
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
        */

        return notebook;
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
    /// <returns>Код сторінки</returns>
    public static string? CreateNotebookPage(Notebook? notebook, string tabName, Func<Widget>? pageWidget, bool insertPage = false,
        Action? beforeOpenPageFunc = null, Action? afterClosePageFunc = null, bool notClosePage = false)
    {
        if (notebook != null)
        {
            int numPage;
            string? currNotebookName = notebook.Name;
            string codePage = Guid.NewGuid().ToString();

            ScrolledWindow scroll = new() { Name = codePage };
            scroll.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);

            Box hBoxLabel = CreateTabLabelPageWidget(notebook, tabName, codePage, notClosePage);

            if (insertPage)
                numPage = notebook.InsertPage(scroll, hBoxLabel, notebook.GetCurrentPage());
            else
                numPage = notebook.AppendPage(scroll, hBoxLabel);

            if (pageWidget != null)
            {
                Widget widget = pageWidget.Invoke();
                widget.Name = codePage;
                scroll.Child = widget;
            }

            //Переміщення сторінок
            notebook.SetTabReorderable(scroll, true);

            //Додаткова функція яка викликається до відкриття сторінки блокноту
            beforeOpenPageFunc?.Invoke();

            //Додаткова функція яка викликається після закриття сторінки блокноту
            if (currNotebookName != null && afterClosePageFunc != null)
                if (AfterClosePageFunc.TryGetValue(currNotebookName, out Dictionary<string, Action>? pageNameAndFunc))
                    pageNameAndFunc.Add(codePage, afterClosePageFunc);
                else
                    AfterClosePageFunc.Add(currNotebookName, new Dictionary<string, Action>() { { codePage, afterClosePageFunc } });

            notebook.SetCurrentPage(numPage);
            notebook.GrabFocus();

            return codePage;
        }
        else
            return null;
    }

    public static string? CreateNotebookPage(string tabName, Func<Widget>? pageWidget, bool insertPage = false, Action? beforeOpenPageFunc = null, Action? afterClosePageFunc = null, bool notClosePage = false) => CreateNotebookPage(GeneralNotebook, tabName, pageWidget, insertPage, beforeOpenPageFunc, afterClosePageFunc, notClosePage);

    /// <summary>
    /// Заголовок сторінки блокноту
    /// </summary>
    /// <param name="caption">Заголовок</param>
    /// <param name="codePage">Код сторінки</param>
    /// <param name="notebook">Блокнот</param>
    /// <param name="notClosePage">Забрати можливість закривати сторінку</param>
    /// <returns>НBox</returns>
    static Box CreateTabLabelPageWidget(Notebook? notebook, string caption, string codePage, bool notClosePage = false)
    {
        Box hBox = Box.New(Orientation.Horizontal, 0);

        Box hBoxIconOrSpinner = Box.New(Orientation.Horizontal, 0);
        hBoxIconOrSpinner.Name = "BoxIconOrSpinner";
        hBoxIconOrSpinner.MarginEnd = 5;
        hBoxIconOrSpinner.Append(Image.NewFromIconName("doc"));

        //Ico / BoxIconOrSpinner
        hBox.Append(hBoxIconOrSpinner);

        //Текст
        Label label = Label.New(SubstringPageName(caption));
        label.Name = "Caption";
        label.TooltipText = caption;

        hBox.Append(label);

        if (!notClosePage)
        {
            Box vBoxClose = Box.New(Orientation.Vertical, 0);
            vBoxClose.Valign = vBoxClose.Halign = Align.Center;
            vBoxClose.Name = "Close";
            vBoxClose.MarginStart = 5;
            hBox.Append(vBoxClose);

            Button button = Button.New();
            button.Cursor = Cursor.NewFromName("hand", null);
            button.Child = Image.NewFromIconName("page-close");
            button.Name = codePage;
            button.TooltipText = "Закрити";
            button.OnClicked += (sender, args) => CloseNotebookPageToCode(notebook, codePage);

            vBoxClose.Append(button);
        }

        return hBox;
    }

    /// <summary>
    /// Закрити сторінку блокноту по коду
    /// </summary>
    /// <param name="notebook">Блокнот</param>
    /// <param name="codePage">Код</param>
    public static void CloseNotebookPageToCode(Notebook? notebook, string codePage)
    {
        for (int i = 0; i < notebook?.GetNPages(); i++)
        {
            Widget? wg = notebook?.GetNthPage(i);
            if (wg?.Name == codePage)
            {
                string? currNotebookName = notebook?.Name;
                if (currNotebookName != null)
                {
                    //Історія переключення сторінок
                    if (HistorySwitch.TryGetValue(currNotebookName, out List<string>? listPageName))
                    {
                        listPageName.Remove(codePage);

                        //Встановлення поточної сторінки, яка остання в списку
                        if (listPageName.Count > 0)
                            CurrentNotebookPageToCode(notebook, listPageName[^1]);
                    }

                    //Додаткова функція яка викликається після закриття сторінки блокноту
                    if (AfterClosePageFunc.TryGetValue(currNotebookName, out Dictionary<string, Action>? pageNameAndFunc))
                        if (pageNameAndFunc.TryGetValue(codePage, out Action? func))
                        {
                            func?.Invoke();
                            pageNameAndFunc.Remove(codePage);
                        }
                }

                /*
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
                */

                notebook?.DetachTab(wg);
                GC.Collect();

                break;
            }
        }
    }

    public static void CloseNotebookPageToCode(string codePage) => CloseNotebookPageToCode(GeneralNotebook, codePage);

    /// <summary>
    /// Відображення спінера або іконки
    /// </summary>
    /// <param name="notebook">Блокнот</param>
    /// <param name="active">Спінер якшо true, Іконка якщо false</param>
    /// <param name="codePage">Код сторінки</param>
    public static void SpinnerNotebookPageToCode(Notebook? notebook, bool active, string codePage)
    {
        for (int i = 0; i < notebook?.GetNPages(); i++)
        {
            Widget? wg = notebook?.GetNthPage(i);
            if (wg?.Name == codePage)
            {
                Box? tabLabel = (Box?)notebook?.GetTabLabel(wg);
                if (tabLabel != null)
                {
                    Widget? child = tabLabel.GetFirstChild();
                    while (child != null)
                    {
                        if (child is Box BoxIconOrSpinner && BoxIconOrSpinner.Name == "BoxIconOrSpinner")
                        {
                            Widget? firstChild = BoxIconOrSpinner.GetFirstChild();
                            if (firstChild != null) BoxIconOrSpinner.Remove(firstChild);

                            if (active)
                                BoxIconOrSpinner.Append(new Spinner() { Spinning = true });
                            else
                                BoxIconOrSpinner.Append(Image.NewFromIconName("doc"));

                            break;
                        }

                        child = child.GetNextSibling();
                    }
                }

                break;
            }
        }
    }

    public static void SpinnerNotebookPageToCode(bool active, string codePage) => SpinnerNotebookPageToCode(GeneralNotebook, active, codePage);

    /// <summary>
    /// Перейменувати сторінку по коду
    /// </summary>
    /// <param name="notebook">Блокнот</param>
    /// <param name="caption">Нова назва</param>
    /// <param name="codePage">Код</param>
    public static void RenameNotebookPageToCode(Notebook? notebook, string caption, string codePage)
    {
        for (int i = 0; i < notebook?.GetNPages(); i++)
        {
            Widget? wg = notebook?.GetNthPage(i);
            if (wg?.Name == codePage)
            {
                Box? tabLabel = (Box?)notebook?.GetTabLabel(wg);
                if (tabLabel != null)
                {
                    Widget? child = tabLabel.GetFirstChild();
                    while (child != null)
                    {
                        if (child is Label label && label.Name == "Caption")
                        {
                            label.SetText(SubstringPageName(caption));
                            label.TooltipText = caption;

                            break;
                        }

                        child = child.GetNextSibling();
                    }
                }

                break;
            }
        }
    }

    public static void RenameNotebookPageToCode(string caption, string codePage) => RenameNotebookPageToCode(GeneralNotebook, caption, codePage);

    /// <summary>
    /// Встановлення поточної сторінки по коду
    /// </summary>
    /// <param name="notebook">Блокнот</param>
    /// <param name="codePage">Код</param>
    public static void CurrentNotebookPageToCode(Notebook? notebook, string codePage)
    {
        for (int i = 0; i < notebook?.GetNPages(); i++)
        {
            Widget? wg = notebook?.GetNthPage(i);
            if (wg?.Name == codePage)
            {
                notebook?.SetCurrentPage(i);

                //Передати фокус
                if (wg is ScrolledWindow scroll)
                    if (scroll.Child is Viewport viewport)
                        viewport.Child?.GetType().GetMethod("DefaultGrabFocus")?.Invoke(viewport.Child, null);
            }
        }
    }

    public static void CurrentNotebookPageToCode(string codePage) => CurrentNotebookPageToCode(GeneralNotebook, codePage);

    /// <summary>
    /// Блокування чи розблокування поточної сторінки по коду
    /// </summary>
    /// <param name="notebook">Блокнот</param>
    /// <param name="codePage">Код</param>
    /// <param name="sensitive">Значення</param>
    public static void SensitiveNotebookPageToCode(Notebook? notebook, bool sensitive, string codePage)
    {
        for (int i = 0; i < notebook?.GetNPages(); i++)
        {
            Widget? wg = notebook?.GetNthPage(i);
            if (wg?.Name == codePage)
            {
                wg.Sensitive = sensitive;
                break;
            }
        }
    }

    public static void SensitiveNotebookPageToCode(bool sensitive, string codePage) => SensitiveNotebookPageToCode(GeneralNotebook, sensitive, codePage);

    /// <summary>
    /// Обрізати імя для сторінки
    /// </summary>
    /// <param name="pageName"></param>
    /// <returns></returns>
    public static string SubstringPageName(string pageName)
    {
        return pageName.Length >= 23 ? pageName[..20] : pageName;
    }
}