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

namespace InterfaceGtk3;

/// <summary>
/// Основа для класів:
///     ДовідникЖурнал, 
///     ДовідникШвидкийВибір, 
///     ДокументЖурнал, 
///     Журнал, 
///     РегістриВідомостейЖурнал, 
///     РегістриНакопиченняЖурнал,
///     РегістриНакопиченняЖурнал_СпрощенийРежим
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

    /// <summary>
    /// Режим який вказує що форма використовується як елемент в іншій формі 
    /// (наприклад дерево використовується в ішому журналі)
    /// </summary>
    public bool CompositeMode { get; set; } = false;

    public ФормаЖурнал()
    {
        TreeViewGrid.Selection.Mode = SelectionMode.Multiple;
        TreeViewGrid.ActivateOnSingleClick = true;

        ScrollTree.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);

        ScrollPages.SetPolicy(PolicyType.Automatic, PolicyType.Never);
        ScrollPages.Add(HBoxPages);
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
    /// Фокус за стандартом
    /// </summary>
    public virtual void DefaultGrabFocus() { TreeViewGrid.GrabFocus(); }

    /// <summary>
    /// Завантаження списку
    /// </summary>
    public abstract ValueTask LoadRecords();

    /// <summary>
    /// Завантаження списку при пошуку
    /// </summary>
    public virtual async ValueTask LoadRecords_OnSearch(string searchText) { await ValueTask.FromResult(true); }

    /// <summary>
    /// Фільтер
    /// </summary>
    public virtual async ValueTask LoadRecords_OnFilter() { await ValueTask.FromResult(true); }

    /// <summary>
    /// Для дерева
    /// </summary>
    /// <returns></returns>
    public virtual async ValueTask LoadRecords_OnTree() { await ValueTask.FromResult(true); }

    #endregion

    #region ToolBar

    protected void ToolButtonSensitive(object? sender, bool sensitive)
    {
        if (sender != null && sender is ToolButton button)
            button.Sensitive = sensitive;
    }

    #endregion

    #region  TreeView

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

    /// <summary>
    /// Додає сторінки для TreeViewGrid
    /// </summary>
    /// <param name="settings">Налаштування</param>
    protected void AddPages(Сторінки.Налаштування settings)
    {
        ТабличнийСписок.Сторінки(TreeViewGrid, settings);
    }

    /// <summary>
    /// Скидає налаштування сторінок для TreeViewGrid
    /// При наступному виводі даних в TreeViewGrid це призведе до повторного перерахунку кількості сторінок
    /// </summary>
    void ClearPages()
    {
        ТабличнийСписок.ОчиститиСторінки(TreeViewGrid);
    }

    /// <summary>
    /// Вивід сторінок для TreeViewGrid.
    /// Інформація про кількість сторінок береться із самого TreeViewGrid
    /// </summary>
    /// <param name="funcLoadRecords">Функція яка спрацьовує при виборі сторінки</param>
    void PagesShow(Func<ValueTask>? funcLoadRecords = null)
    {
        const int offset = 5;

        //Очищення
        foreach (var widget in HBoxPages.Children)
            HBoxPages.Remove(widget);

        Сторінки.Налаштування? settings = ТабличнийСписок.ОтриматиСторінки(TreeViewGrid);
        if (settings != null && settings.Record.Pages > 1)
        {
            HBoxPages.PackStart(new Label("<b>Сторінки:</b> ") { UseMarkup = true }, false, false, 2);

            bool writeSpace = false;
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

    protected async ValueTask BeforeLoadRecords()
    {
        Notebook? notebook = NotebookFunction.GetNotebookFromWidget(this);
        SpinnerOn(notebook);

        async ValueTask PageNavigation()
        {
            SpinnerOn(notebook);

            await LoadRecords();
            PagesShow(PageNavigation);

            SpinnerOff(notebook);
        }

        ClearPages();
        await LoadRecords();
        PagesShow(PageNavigation);

        SpinnerOff(notebook);
    }

    protected async ValueTask BeforeLoadRecords_OnSearch(string searchText)
    {
        Notebook? notebook = NotebookFunction.GetNotebookFromWidget(this);
        SpinnerOn(notebook);

        async ValueTask PageNavigation()
        {
            SpinnerOn(notebook);

            await LoadRecords_OnSearch(searchText);
            PagesShow(PageNavigation);

            SpinnerOff(notebook);
        }

        ClearPages();
        await LoadRecords_OnSearch(searchText);
        PagesShow(PageNavigation);

        SpinnerOff(notebook);
    }

    protected async ValueTask BeforeLoadRecords_OnFilter()
    {
        Notebook? notebook = NotebookFunction.GetNotebookFromWidget(this);
        SpinnerOn(notebook);

        async ValueTask PageNavigation()
        {
            SpinnerOn(notebook);

            await LoadRecords_OnFilter();
            PagesShow(PageNavigation);

            SpinnerOff(notebook);
        }

        ClearPages();
        await LoadRecords_OnFilter();
        PagesShow(PageNavigation);

        SpinnerOff(notebook);
    }

    protected async ValueTask BeforeLoadRecords_OnTree()
    {
        Notebook? notebook = NotebookFunction.GetNotebookFromWidget(this);
        SpinnerOn(notebook);

        async ValueTask PageNavigation()
        {
            SpinnerOn(notebook);

            await LoadRecords_OnTree();
            PagesShow(PageNavigation);

            SpinnerOff(notebook);
        }

        ClearPages();
        await LoadRecords_OnTree();
        PagesShow(PageNavigation);

        SpinnerOff(notebook);
    }

    #endregion
}