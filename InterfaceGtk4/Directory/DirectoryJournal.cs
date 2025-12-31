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
using Gdk;
using AccountingSoftware;

namespace InterfaceGtk4;

/// <summary>
/// Основа для журналів довідників певного виду
/// </summary>
public abstract class DirectoryJournal : FormJournal
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
    public override Gio.ListStore Store { get; } = Gio.ListStore.New(DirectoryRow.GetGType());

    /// <summary>
    /// Функція зворотнього виклику при виборі
    /// </summary>
    public Action<UnigueID>? CallBack_OnSelectPointer { get; set; }

    /// <summary>
    /// Верхній набір меню
    /// </summary>
    protected Box ToolbarTop { get; } = New(Orientation.Horizontal, 0);

    /// <summary>
    /// Верхній бок для додаткових кнопок
    /// </summary>
    protected Box HBoxTop { get; } = New(Orientation.Horizontal, 0);

    /// <summary>
    /// Пошук
    /// </summary>
    protected SearchControl Search { get; } = new();

    /// <summary>
    /// Фільтр
    /// </summary>
    public FilterControl Filter { get; } = new();

    public DirectoryJournal(NotebookFunction? notebook) : base(notebook)
    {
        //Кнопки
        HBoxTop.MarginBottom = 6;
        Append(HBoxTop);

        //Пошук
        {
            Search.MarginEnd = 2;
            Search.Select = async x =>
            {
                Filter.IsFiltered = false;
                SetSearch(x);

                PagesClear();
                await LoadRecords();
            };
            Search.Clear = async () =>
            {
                WhereList = null;

                PagesClear();
                await LoadRecords();
            };
            HBoxTop.Append(Search);
        }

        //Фільтр
        {
            Filter.MarginEnd = 2;
            Filter.Select = async () =>
            {
                PagesClear();
                await LoadRecords();
            };
            Filter.Clear = async () =>
            {
                WhereList = null;

                PagesClear();
                await LoadRecords();
            };
            Filter.FillFilterList = FillFilter;
        }

        Toolbar();

        MultiSelection model = MultiSelection.New(Store);
        model.OnSelectionChanged += GridOnSelectionChanged;

        Grid.Model = model;
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
                        Delete();
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

        ScrollGrid.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
        ScrollGrid.SetChild(Grid);
        ScrollGrid.Vexpand = ScrollGrid.Hexpand = true;
        Append(ScrollGrid);

        ScrollPages.SetPolicy(PolicyType.Automatic, PolicyType.Never);
        ScrollPages.SetChild(HBoxPages);
        Append(ScrollPages);
    }

    public override async ValueTask SetValue()
    {
        DefaultGrabFocus();
        await BeforeSetValue();

        await LoadRecords();
        RunUpdateRecords();
    }

    #region Virtual & Abstract Function

    protected abstract ValueTask OpenPageElement(bool IsNew, UnigueID? unigueID = null);
    protected abstract ValueTask SetDeletionLabel(UnigueID unigueID);
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

    /// <summary>
    /// Меню друк
    /// </summary>
    protected virtual NameValue<Action<UnigueID[]>>[]? SetPrintMenu() { return null; }

    /// <summary>
    /// Меню експорт
    /// </summary>
    protected virtual NameValue<Action<UnigueID[]>>[]? SetExportMenu() { return null; }

    /// <summary>
    /// Історія версій
    /// </summary>
    /// <param name="unigueID">Вибрані елементи</param>
    protected virtual async ValueTask VersionsHistory(UnigueID[] unigueID) { await ValueTask.FromResult(true); }

    #endregion

    #region Grid

    /// <summary>
    /// При активації
    /// </summary>
    /// <param name="position">Позиція</param>
    async ValueTask GridOnActivate(uint position)
    {
        MultiSelection model = (MultiSelection)Grid.Model;
        if (model.GetObject(position) is Row row)
            if (DirectoryPointerItem == null)
                await OpenPageElement(false, row.UnigueID);
            else
            {
                CallBack_OnSelectPointer?.Invoke(row.UnigueID);

                Notebook?.ClosePage(this.GetName());
                PopoverParent?.Hide();
            }
    }

    #endregion

    #region Toolbar

    void Toolbar()
    {
        ToolbarTop.MarginBottom = 6;
        Append(ToolbarTop);

        {
            Button button = Button.NewFromIconName("new");
            button.MarginEnd = 5;
            button.TooltipText = "Додати";
            button.OnClicked += OnAdd;
            ToolbarTop.Append(button);
        }

        {
            Button button = Button.NewFromIconName("edit");
            button.MarginEnd = 5;
            button.TooltipText = "Редагувати";
            button.OnClicked += OnEdit;
            ToolbarTop.Append(button);
        }

        {
            Button button = Button.NewFromIconName("refresh");
            button.MarginEnd = 5;
            button.TooltipText = "Оновити";
            button.OnClicked += OnRefresh;
            ToolbarTop.Append(button);
        }

        {
            Button button = Button.NewFromIconName("copy");
            button.MarginEnd = 5;
            button.TooltipText = "Копіювати";
            button.OnClicked += OnCopy;
            ToolbarTop.Append(button);
        }

        {
            Button button = Button.NewFromIconName("delete");
            button.MarginEnd = 5;
            button.TooltipText = "Видалити";
            button.OnClicked += OnDelete;
            ToolbarTop.Append(button);
        }

        {
            Button button = Button.NewFromIconName("view-sort-descending");
            button.MarginEnd = 5;
            button.TooltipText = "Фільтр";
            button.OnClicked += OnFilter;
            ToolbarTop.Append(button);
        }

        {
            Button button = Button.NewFromIconName("document-print");
            button.MarginEnd = 5;
            button.TooltipText = "Друк";
            button.OnClicked += (_, _) => CreatePopoverMenu(button, SetPrintMenu());
            ToolbarTop.Append(button);
        }

        {
            Button button = Button.NewFromIconName("process-working");
            button.MarginEnd = 5;
            button.TooltipText = "Експорт";
            button.OnClicked += (_, _) => CreatePopoverMenu(button, SetExportMenu());
            ToolbarTop.Append(button);
        }

        {
            Button button = Button.NewFromIconName("zoom-in");
            button.MarginEnd = 5;
            button.TooltipText = "Версії";
            button.OnClicked += OnVersionsHistory;
            ToolbarTop.Append(button);
        }
    }

    async void OnAdd(Button button, EventArgs args)
    {
        await OpenPageElement(true);
    }

    async void Edit()
    {
        foreach (Row row in GetSelection())
            await OpenPageElement(false, row.UnigueID);
    }

    async void OnEdit(Button button, EventArgs args)
    {
        Edit();
    }

    async void OnRefresh(Button sender, EventArgs args)
    {
        PagesClear();
        await LoadRecords();
    }

    async void OnCopy(Button button, EventArgs args)
    {
        List<Row> rows = GetSelection();
        if (rows.Count > 0)
            //!!! треба додати app and window і міняти курсор коли йде копіювання або вивести якесь вікно із статусом
            Message.Request(null, null, "Копіювання", "Копіювати вибрані елементи?", async YN =>
            {
                if (YN == Message.YesNo.Yes)
                {
                    foreach (Row row in rows)
                        SelectPointerItem = await Copy(row.UnigueID);

                    PagesClear();
                    await LoadRecords();
                }
            });
    }

    void Delete()
    {
        List<Row> rows = GetSelection();
        if (rows.Count > 0)
            //!!! треба додати app and window і міняти курсор коли йде видалення або вивести якесь вікно із статусом
            Message.Request(null, null, "Відмітка для видалення", "Встановити або зняти відмітку для видалення для вибраних елементів?", async YN =>
            {
                if (YN == Message.YesNo.Yes)
                {
                    foreach (Row row in rows)
                        await SetDeletionLabel(row.UnigueID);
                }
            });
    }

    async void OnDelete(Button button, EventArgs args)
    {
        Delete();
    }

    void OnFilter(Button button, EventArgs args)
    {
        if (!Filter.IsFilterCreated)
            Filter.CreatePopover(button);

        Filter.PopoverParent?.Show();
    }

    async void OnVersionsHistory(Button button, EventArgs args)
    {
        await VersionsHistory(GetGetSelectionUnigueID());
    }

    #endregion
}
