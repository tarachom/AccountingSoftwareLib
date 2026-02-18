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

using Gtk;
using Gdk;
using AccountingSoftware;

namespace InterfaceGtk4;

/// <summary>
/// ДовідникФормаЖурналБазовий
/// 
/// Основа для класів:
///     DirectoryFormJournalFull (ДовідникФормаЖурналПовний),
///     DirectoryFormJournalSmall (ДовідникФормаЖурналМіні),
///     DirectoryFormJournalBaseTree (ДовідникФормаЖурналБазовийДерево)
/// </summary>
public abstract class DirectoryFormJournalBase : FormJournal
{
    /// <summary>
    /// Для вибору і позиціювання
    /// </summary>
    public UnigueID? DirectoryPointerItem
    {
        get => directoryPointerItem;
        set => SelectPointerItem = directoryPointerItem = value;
    }
    UnigueID? directoryPointerItem;

    /// <summary>
    /// Перевизначення сховища для нового типу даних 
    /// </summary>
    public override Gio.ListStore Store { get; } = Gio.ListStore.New(DirectoryRowJournal.GetGType());

    /// <summary>
    /// Відбори по родичу або інший постійний відбір
    /// </summary>
    public List<Where>? ParentWhereList { get; set; }

    /// <summary>
    /// Відбори по власнику або інший постійний відбір.
    /// Використовується функція яка повертає список для відбору щоб відбір був актуальний
    /// </summary>
    public Func<List<Where>>? OwnerWhereListFunc { get; set; }

    /// <summary>
    /// Функція зворотнього виклику при виборі
    /// </summary>
    public Action<UnigueID>? CallBack_OnSelectPointer { get; set; }

    /// <summary>
    /// Верхній бокc для пошуку та додаткових кнопок
    /// </summary>
    protected Box HBoxTop { get; } = New(Orientation.Horizontal, 0);

    /// <summary>
    /// Верхній набір меню
    /// </summary>
    protected Box HBoxToolbarTop { get; } = New(Orientation.Horizontal, 0);

    /// <summary>
    /// Пошук
    /// </summary>
    protected SearchControl Search { get; } = new();

    /// <summary>
    /// Фільтр
    /// </summary>
    public FilterControl Filter { get; } = new();

    /// <summary>
    /// Панель з двох блоків
    /// </summary>
    protected Paned HPanedTable = Paned.New(Orientation.Horizontal);

    /// <summary>
    /// Переключатель використання ієрархії для довідника з деревом
    /// </summary>
    public Switch UseHierarchy { get; } = Switch.New();

    public DirectoryFormJournalBase(NotebookFunction? notebookFunc) : base(notebookFunc)
    {
        //Кнопки
        HBoxTop.MarginBottom = 6;
        Append(HBoxTop);

        //Пошук
        {
            Search.MarginEnd = 2;
            Search.Select = async x =>
            {
                TypeWhereState = TypeWhere.Search;
                SetSearch(x);

                PagesClear();
                await LoadRecords();
            };
            Search.Clear = async () =>
            {
                TypeWhereState = TypeWhere.Standart;
                WhereList = null;

                PagesClear();
                await LoadRecords();
            };
            HBoxTop.Append(Search);
        }

        //Інформування про стан відборів
        TypeWhereStateInfo.MarginStart = 8;
        TypeWhereStateInfo.MarginEnd = 10;
        HBoxTop.Append(TypeWhereStateInfo);

        //Фільтр
        {
            Filter.MarginEnd = 2;
            Filter.Select = async () =>
            {
                TypeWhereState = TypeWhere.Filter;

                PagesClear();
                await LoadRecords();
            };
            Filter.Clear = async () =>
            {
                TypeWhereState = TypeWhere.Standart;
                WhereList = null;

                PagesClear();
                await LoadRecords();
            };
            Filter.FillFilterList = FillFilter;
        }

        CreateToolbar();
        GridModel();

        Grid.OnActivate += async (_, args) => await GridOnActivate(args.Position);

        EventControllerKey contrKey = EventControllerKey.New();
        Grid.AddController(contrKey);
        contrKey.OnKeyReleased += async (sender, args) =>
        {
            switch (args.Keyval)
            {
                //Помітка на видалення
                case (uint)Key.Delete:
                    {
                        await Delete();
                        break;
                    }
                //Новий
                case (uint)Key.Insert:
                    {
                        await OpenPageElement(true);
                        break;
                    }
            }
        };

        Box vBoxStart = New(Orientation.Vertical, 0);

        ScrollGrid.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
        ScrollGrid.SetChild(Grid);
        ScrollGrid.Vexpand = ScrollGrid.Hexpand = true;
        vBoxStart.Append(ScrollGrid);

        ScrollPages.SetPolicy(PolicyType.Automatic, PolicyType.Never);
        ScrollPages.SetChild(HBoxPages);
        vBoxStart.Append(ScrollPages);

        HPanedTable.SetStartChild(vBoxStart);
        HPanedTable.SetShrinkStartChild(false);
        HPanedTable.SetShrinkEndChild(false);

        Append(HPanedTable);

        //Прокрутка до виділеного рядка
        Grid.Vadjustment?.OnChanged += (sender, args) =>
        {
            Bitset selection = Grid.Model.GetSelection();
            if (selection.GetSize() > 0 && ScrollToEnable)
                ScrollTo(selection.GetMaximum());
        };
    }

    /// <summary>
    /// Встановлення моделі
    /// </summary>
    protected override void GridModel()
    {
        //Модель
        MultiSelection model = MultiSelection.New(Store);
        model.OnSelectionChanged += GridOnSelectionChanged;

        Grid.Model = model;
    }

    /// <summary>
    /// Перша функція яка викликається після створення форми
    /// </summary>
    public override async ValueTask SetValue()
    {
        DefaultGrabFocus();
        await BeforeSetValue();

        //Для композитного режиму дані не загружаються
        if (!CompositeMode)
            await LoadRecords();

        RunUpdateRecords();
    }

    #region Virtual & Abstract Function

    /// <summary>
    /// Відкрити елемент
    /// </summary>
    /// <param name="IsNew">Чи це новий?</param>
    /// <param name="unigueID">Ід об'єкту</param>
    /// <returns></returns>
    protected abstract ValueTask OpenPageElement(bool IsNew, UnigueID? unigueID = null);

    /// <summary>
    /// Помітка на видалення
    /// </summary>
    /// <param name="unigueID">Ід об'єкту</param>
    /// <returns></returns>
    protected abstract ValueTask SetDeletionLabel(UnigueID unigueID);

    /// <summary>
    /// Копіювання
    /// </summary>
    /// <param name="unigueID">Ід об'єкту</param>
    /// <returns></returns>
    protected abstract ValueTask<UnigueID?> Copy(UnigueID unigueID);

    /// <summary>
    /// Функція зворотнього виклику для перевантаження списку
    /// </summary>
    /// <param name="selectPointer">Елемент на якому треба спозиціонувати список</param>
    protected virtual async void CallBack_LoadRecords(UnigueID? selectPointer)
    {
        SelectPointerItem = selectPointer;
        await LoadRecords();
    }

    /// <summary>
    /// Формування відборів для пошуку
    /// </summary>
    /// <param name="searchText">Текст для пошуку</param>
    protected abstract void SetSearch(string searchText);

    /// <summary>
    /// Заповнити поля для фільтру
    /// </summary>
    /// <param name="filterControl">Контрол Фільтр</param>
    protected virtual void FillFilter(FilterControl filterControl) { }

    #endregion

    #region Grid

    /// <summary>
    /// При активації
    /// </summary>
    /// <param name="position">Позиція</param>
    protected virtual async ValueTask GridOnActivate(uint position)
    {
        MultiSelection model = (MultiSelection)Grid.Model;
        if (model.GetObject(position) is RowJournal row)
            if (DirectoryPointerItem == null)
                await OpenPageElement(false, row.UnigueID);
            else
            {
                CallBack_OnSelectPointer?.Invoke(row.UnigueID);

                NotebookFunc?.ClosePage(GetName());
                PopoverParent?.Hide();
            }
    }

    #endregion

    #region Toolbar

    void CreateToolbar()
    {
        HBoxToolbarTop.MarginBottom = 6;
        Append(HBoxToolbarTop);

        //Новий
        {
            Button button = Button.NewFromIconName("new");
            button.MarginEnd = 5;
            button.TooltipText = "Додати";
            button.OnClicked += OnAdd;
            HBoxToolbarTop.Append(button);
        }

        //Редагувати
        {
            Button button = Button.NewFromIconName("edit");
            button.MarginEnd = 5;
            button.TooltipText = "Редагувати";
            button.OnClicked += OnEdit;
            HBoxToolbarTop.Append(button);
        }

        {
            Button button = Button.NewFromIconName("refresh");
            button.MarginEnd = 5;
            button.TooltipText = "Оновити";
            button.OnClicked += OnRefresh;
            HBoxToolbarTop.Append(button);
        }

        {
            Button button = Button.NewFromIconName("copy");
            button.MarginEnd = 5;
            button.TooltipText = "Копіювати";
            button.OnClicked += OnCopy;
            HBoxToolbarTop.Append(button);
        }

        {
            Button button = Button.NewFromIconName("delete");
            button.MarginEnd = 5;
            button.TooltipText = "Видалити";
            button.OnClicked += OnDelete;
            HBoxToolbarTop.Append(button);
        }

        {
            Button button = Button.NewFromIconName("view-sort-descending");
            button.MarginEnd = 5;
            button.TooltipText = "Фільтр";
            button.OnClicked += OnFilter;
            HBoxToolbarTop.Append(button);
        }

        {
            Separator separator = Separator.New(Orientation.Vertical);
            separator.MarginStart = 5;
            separator.MarginEnd = 10;
            HBoxToolbarTop.Append(separator);
        }
    }

    async void OnAdd(Button button, EventArgs args)
    {
        await OpenPageElement(true);
    }

    async void Edit()
    {
        foreach (RowJournal row in GetSelection())
            await OpenPageElement(false, row.UnigueID);
    }

    async void OnEdit(Button button, EventArgs args)
    {
        Edit();
    }

    async void OnRefresh(Button sender, EventArgs args)
    {
        await Refresh();
    }

    async void OnCopy(Button button, EventArgs args)
    {
        List<RowJournal> rows = GetSelection();
        if (rows.Count > 0)
        {
            foreach (RowJournal row in rows)
                SelectPointerItem = await Copy(row.UnigueID);

            PagesClear();
            await LoadRecords();
        }

        /*
        Message.Request(BasicApp, BasicForm, "Копіювання", "Копіювати вибрані елементи?", async YN =>
        {
            if (YN == Message.YesNo.Yes)
            {
                foreach (RowJournal row in rows)
                    SelectPointerItem = await Copy(row.UnigueID);

                PagesClear();
                await LoadRecords();
            }
        });
        */
    }

    async ValueTask Delete()
    {
        List<RowJournal> rows = GetSelection();
        if (rows.Count > 0)
        {
            foreach (RowJournal row in rows)
                await SetDeletionLabel(row.UnigueID);
        }

        /*
        Message.Request(BasicApp, BasicForm, "Відмітка для видалення", "Встановити або зняти відмітку для видалення для вибраних елементів?", async YN =>
        {
            if (YN == Message.YesNo.Yes)
            {
                foreach (RowJournal row in rows)
                    await SetDeletionLabel(row.UnigueID);
            }
        });
        */
    }

    async void OnDelete(Button button, EventArgs args)
    {
        await Delete();
    }

    void OnFilter(Button button, EventArgs args)
    {
        if (!Filter.IsFilterCreated)
            Filter.CreatePopover(button);

        Filter.PopoverParent?.Show();
    }

    #endregion

    #region ForTree

    /// <summary>
    /// Форма відкрита для вибору
    /// </summary>
    public bool OpenSelect { get; set; }

    /// <summary>
    /// Відкрита папка.
    /// Використовується при загрузці дерева щоб приховати вітку.
    /// Актуально у випадку вибору родича, щоб не можна було вибрати у якості родича відкриту папку
    /// </summary>
    public UnigueID? OpenFolder { get; set; }

    /// <summary>
    /// Вставити пустий рядок в дерево
    /// Потрібно для композитного типу, коли дерево вкладається у інший довідник і слугує для відбору по родичу.
    /// </summary>
    public bool InsertEmptyFirstRow { get; set; }

    #endregion

    #region ForCompositeMode

    /// <summary>
    /// Додати переключатель використання ієрархії на форму
    /// </summary>
    protected void AddSwitchUseHierarchy()
    {
        Box vBox = Box.New(Orientation.Vertical, 0);
        vBox.MarginStart = 20;
        vBox.Valign = Align.Center;
        HBoxToolbarTop.Append(vBox);

        Box hBox = Box.New(Orientation.Horizontal, 0);
        vBox.Append(hBox);

        Label label = Label.New("Без ієрархії");
        label.MarginEnd = 5;

        hBox.Append(label);
        hBox.Append(UseHierarchy);

        UseHierarchy.OnStateSet += (_, args) =>
        {
            async void reloadFunc()
            {
                PagesClear();
                await LoadRecords();
            }
            reloadFunc();

            return false;
        };
    }

    #endregion
}
