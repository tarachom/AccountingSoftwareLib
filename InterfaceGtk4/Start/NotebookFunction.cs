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
    //public const string DataKey_ObjectChangeEvents = "object_change_events";
    //public const string DataKey_LockObjectPageFunc = "lock_object_page_func";


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
            if (notebook.IsFocus())
                if (args.Keyval == (uint)Key.Escape)
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
                                            CloseNotebookPage(notebook, codePage);
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
    /// 
    /// Код сторінки вкладається в назву віджета - widget.Name = codePage;
    /// Також код сторінки вкладається в назву основної прокрутки сторінки - scroll.Name = codePage;
    /// Функція повертає код сторінки.
    /// </summary>
    /// <param name="notebook">Блокнот</param>
    /// <param name="tabName">Назва сторінки, заголовок</param>
    /// <param name="pageWidget">Віджет для сторінки</param>
    /// <param name="insertPage">Вставити сторінку перед поточною</param>
    /// <param name="beforeOpenPageFunc">Функція яка викликається перед відкриттям сторінки блокноту</param>
    /// <param name="afterClosePageFunc">Функція яка викликається після закриття сторінки блокноту</param>
    /// <param name="noClosePage">Забрати можливість закривати сторінку</param>
    /// <returns>Код сторінки</returns>
    public static string CreateNotebookPage(Notebook? notebook, string tabName, Func<Widget>? pageWidget, bool insertPage = false,
        Action? beforeOpenPageFunc = null, Action? afterClosePageFunc = null, bool noClosePage = false)
    {
        if (notebook is null) throw new Exception("Блокнот не заданий, неможливо створити сторінку!");
        string notebookName = notebook.Name ?? throw new Exception("Для блокноту не задана назва!");

        int numPage;
        string codePage = Guid.NewGuid().ToString();

        ScrolledWindow scroll = new() { Name = codePage };
        scroll.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);

        Box hBoxLabel = CreateTabLabelPageWidget(notebook, tabName, codePage, noClosePage);

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

        //Додаткова функція яка викликається зразу після відкриття сторінки блокноту
        beforeOpenPageFunc?.Invoke();

        //Додаткова функція яка викликається після закриття сторінки блокноту
        if (afterClosePageFunc != null)
            if (AfterClosePageFunc.TryGetValue(notebookName, out Dictionary<string, Action>? codePageAndFunc))
                codePageAndFunc.Add(codePage, afterClosePageFunc);
            else
                AfterClosePageFunc.Add(notebookName, new Dictionary<string, Action>() { { codePage, afterClosePageFunc } });

        notebook.SetCurrentPage(numPage);
        notebook.GrabFocus();

        return codePage;
    }

    public static string CreateNotebookPage(string tabName, Func<Widget>? pageWidget, bool insertPage = false, Action? beforeOpenPageFunc = null, Action? afterClosePageFunc = null, bool notClosePage = false) =>
        CreateNotebookPage(GeneralNotebook, tabName, pageWidget, insertPage, beforeOpenPageFunc, afterClosePageFunc, notClosePage);

    public static string CreateNotebookPage(string tabName, Widget? pageWidget) =>
        CreateNotebookPage(GeneralNotebook, tabName, pageWidget != null ? () => pageWidget : null);

    /// <summary>
    /// Заголовок сторінки блокноту з кнопкою для закриття самої сторінки
    /// </summary>
    /// <param name="notebook">Блокнот</param>
    /// <param name="caption">Заголовок</param>
    /// <param name="codePage">Код сторінки</param>
    /// <param name="noClosePage">Забрати можливість закривати сторінку</param>
    /// <returns>НBox</returns>
    static Box CreateTabLabelPageWidget(Notebook notebook, string caption, string codePage, bool noClosePage = false)
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

        if (!noClosePage)
        {
            Box vBoxClose = Box.New(Orientation.Vertical, 0);
            vBoxClose.Valign = vBoxClose.Halign = Align.Center;
            vBoxClose.Name = "Close";
            vBoxClose.MarginStart = 5;
            hBox.Append(vBoxClose);

            Button button = Button.New();
            button.Cursor = Cursor.NewFromName("hand", null);
            button.Child = Image.NewFromIconName("window-close-symbolic");
            button.Name = codePage;
            button.TooltipText = "Закрити";
            button.OnClicked += (sender, args) => CloseNotebookPage(notebook, codePage);

            vBoxClose.Append(button);
        }

        return hBox;
    }

    /// <summary>
    /// Закрити сторінку блокноту по коду
    /// </summary>
    /// <param name="notebook">Блокнот</param>
    /// <param name="codePage">Код</param>
    public static void CloseNotebookPage(Notebook? notebook, string codePage)
    {
        if (notebook is null) throw new Exception("Блокнот не заданий, неможливо закрити сторінку!");
        string notebookName = notebook.Name ?? throw new Exception("Для блокноту не задана назва!");

        for (int i = 0; i < notebook.GetNPages(); i++)
        {
            Widget? wg = notebook.GetNthPage(i);
            if (wg?.Name == codePage)
            {
                //Історія переключення сторінок
                if (HistorySwitch.TryGetValue(notebookName, out List<string>? codePageList))
                {
                    codePageList.Remove(codePage);

                    //Встановлення поточної сторінки, яка остання в списку
                    if (codePageList.Count > 0)
                        CurrentNotebookPage(notebook, codePageList[^1]);
                }

                //Додаткова функція яка викликається після закриття сторінки блокноту
                if (AfterClosePageFunc.TryGetValue(notebookName, out Dictionary<string, Action>? codePageAndFunc))
                    if (codePageAndFunc.TryGetValue(codePage, out Action? func))
                    {
                        func?.Invoke();
                        codePageAndFunc.Remove(codePage);
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

                notebook.DetachTab(wg);
                GC.Collect();

                break;
            }
        }
    }

    public static void CloseNotebookPage(string codePage) => CloseNotebookPage(GeneralNotebook, codePage);

    /// <summary>
    /// Відображення спінера або іконки
    /// </summary>
    /// <param name="notebook">Блокнот</param>
    /// <param name="active">Спінер якшо true, Іконка якщо false</param>
    /// <param name="codePage">Код сторінки</param>
    public static void SpinnerNotebookPage(Notebook? notebook, bool active, string codePage)
    {
        if (notebook is null) throw new Exception("Блокнот не заданий, неможливо встановити спінер!");

        for (int i = 0; i < notebook.GetNPages(); i++)
        {
            Widget? wg = notebook.GetNthPage(i);
            if (wg?.Name == codePage)
            {
                Box? tabLabel = (Box?)notebook.GetTabLabel(wg);
                if (tabLabel != null)
                {
                    Widget? child = tabLabel.GetFirstChild();
                    while (child != null)
                    {
                        if (child is Box BoxIconOrSpinner && BoxIconOrSpinner.Name == "BoxIconOrSpinner")
                        {
                            Widget? firstChild = BoxIconOrSpinner.GetFirstChild();
                            if (firstChild != null) BoxIconOrSpinner.Remove(firstChild);

                            BoxIconOrSpinner.Append(active ? new Spinner() { Spinning = true } : Image.NewFromIconName("doc"));
                            break;
                        }

                        child = child.GetNextSibling();
                    }
                }

                break;
            }
        }
    }

    public static void SpinnerNotebookPage(bool active, string codePage) => SpinnerNotebookPage(GeneralNotebook, active, codePage);
    public static void SpinnerNotebookPage(string codePage, bool active = true) => SpinnerNotebookPage(GeneralNotebook, active, codePage);

    /// <summary>
    /// Перейменувати сторінку по коду
    /// </summary>
    /// <param name="notebook">Блокнот</param>
    /// <param name="caption">Нова назва</param>
    /// <param name="codePage">Код</param>
    public static void RenameNotebookPage(Notebook? notebook, string caption, string codePage)
    {
        if (notebook is null) throw new Exception("Блокнот не заданий, неможливо перейменувати сторінку!");

        for (int i = 0; i < notebook.GetNPages(); i++)
        {
            Widget? wg = notebook.GetNthPage(i);
            if (wg?.Name == codePage)
            {
                Box? tabLabel = (Box?)notebook.GetTabLabel(wg);
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

    public static void RenameNotebookPage(string caption, string codePage) => RenameNotebookPage(GeneralNotebook, caption, codePage);

    /// <summary>
    /// Встановлення поточної сторінки по коду
    /// </summary>
    /// <param name="notebook">Блокнот</param>
    /// <param name="codePage">Код</param>
    public static void CurrentNotebookPage(Notebook? notebook, string codePage)
    {
        if (notebook is null) throw new Exception("Блокнот не заданий, неможливо встановити поточну сторінку!");

        for (int i = 0; i < notebook.GetNPages(); i++)
        {
            Widget? wg = notebook.GetNthPage(i);
            if (wg?.Name == codePage)
            {
                notebook.SetCurrentPage(i);

                //Передати фокус
                if (wg is ScrolledWindow scroll && scroll.Child is Viewport viewport)
                    viewport.Child?.GetType().GetMethod("DefaultGrabFocus")?.Invoke(viewport.Child, null);
            }
        }
    }

    public static void CurrentNotebookPage(string codePage) => CurrentNotebookPage(GeneralNotebook, codePage);

    /// <summary>
    /// Блокування чи розблокування поточної сторінки по коду
    /// </summary>
    /// <param name="notebook">Блокнот</param>
    /// <param name="codePage">Код</param>
    /// <param name="sensitive">Значення</param>
    public static void SensitiveNotebookPage(Notebook? notebook, bool sensitive, string codePage)
    {
        if (notebook is null) throw new Exception("Блокнот не заданий, неможливо заблокувати/розблокувати поточну сторінку!");

        for (int i = 0; i < notebook.GetNPages(); i++)
        {
            Widget? wg = notebook.GetNthPage(i);
            if (wg?.Name == codePage)
            {
                wg.Sensitive = sensitive;
                break;
            }
        }
    }

    public static void SensitiveNotebookPage(bool sensitive, string codePage) => SensitiveNotebookPage(GeneralNotebook, sensitive, codePage);
    public static void SensitiveNotebookPage(string codePage, bool sensitive = true) => SensitiveNotebookPage(GeneralNotebook, sensitive, codePage);

    /// <summary>
    /// Обрізати ім'я для сторінки
    /// </summary>
    /// <param name="pageName">Назва</param>
    /// <returns>Обрізана назва</returns>
    public static string SubstringPageName(string pageName) => pageName.Length >= 23 ? pageName[..19] + " ..." : pageName;
}