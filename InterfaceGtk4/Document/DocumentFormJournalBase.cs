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
/// ДокументФормаЖурналБазовий
/// 
/// Основа для класів:
///     DocumentFormJournalFull (ДокументФормаЖурналПовний),
///     DocumentFormJournalSmall (ДокументФормаЖурналМіні)
/// </summary>
[GObject.Subclass<FormJournal>]
public partial class DocumentFormJournalBase : FormJournal
{
    /// <summary>
    /// Для вибору і позиціювання
    /// </summary>
    public UniqueID? DocumentPointerItem
    {
        get => documentPointerItem;
        set => SelectPointerItem = documentPointerItem = value;
    }
    UniqueID? documentPointerItem = null;

    /// <summary>
    /// Перевизначення сховища для нового типу даних 
    /// </summary>
    public override Gio.ListStore Store { get; } = Gio.ListStore.New(DocumentRowJournal.GetGType());

    /// <summary>
    /// Функція зворотнього виклику при виборі
    /// </summary>
    public Action<UniqueID>? CallBack_OnSelectPointer { get; set; } = null;

    /// <summary>
    /// Верхній бок для додаткових кнопок
    /// </summary>
    protected Box HBoxTop { get; } = Box.New(Orientation.Horizontal, 0);

    /// <summary>
    /// Верхній набір меню
    /// </summary>
    protected Box HBoxToolbarTop { get; } = Box.New(Orientation.Horizontal, 0);

    /// <summary>
    /// Період
    /// </summary>
    public PeriodControl Period { get; } = PeriodControl.New();

    /// <summary>
    /// Пошук
    /// </summary>
    protected SearchControl Search { get; } = SearchControl.New();

    /// <summary>
    /// Фільтр
    /// </summary>
    public FilterControl Filter { get; } = FilterControl.NewWithUsePeriod(true);

    partial void Initialize()
    {
        if (GetType().Namespace == "InterfaceGtk4") return;

        //Кнопки
        HBoxTop.MarginBottom = 6;
        Append(HBoxTop);

        //Період
        Period.MarginEnd = 2;
        Period.Changed = async () =>
        {
            PagesClear();
            await LoadRecords();

            PeriodChanged();
        };
        HBoxTop.Append(Period);

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

        ScrollGrid.SetChild(Grid);
        ScrollGrid.Vexpand = ScrollGrid.Hexpand = true;
        Append(ScrollGrid);

        ScrollPages.SetChild(HBoxPages);
        Append(ScrollPages);
    }

    protected override void GridModel()
    {
        //Модель
        MultiSelection model = MultiSelection.New(Store);
        model.OnSelectionChanged += GridOnSelectionChanged;

        Grid.Model = model;
    }

    public override async ValueTask SetValue()
    {
        DefaultGrabFocus();
        await BeforeSetValue();

        RunUpdateRecords();
    }

    #region Virtual & Abstract Function

    /// <summary>
    /// При відкритті
    /// </summary>
    /// <param name="IsNew">Чи це новий?</param>
    /// <param name="uniqueID">Ід об'єкту</param>
    /// <returns></returns>
    protected virtual async ValueTask OpenPageElement(bool IsNew, UniqueID? uniqueID = null) => await ValueTask.FromResult(true);

    /// <summary>
    /// При встановленні помітки на видалення
    /// </summary>
    /// <param name="uniqueID">Ід об'єкту</param>
    /// <returns></returns>
    protected virtual async ValueTask SetDeletionLabel(UniqueID uniqueID) => await ValueTask.FromResult(true);

    /// <summary>
    /// При копіюванні об'єкту
    /// </summary>
    /// <param name="uniqueID">Ід об'єкту</param>
    /// <returns></returns>
    protected virtual async ValueTask<UniqueID?> Copy(UniqueID uniqueID) => await ValueTask.FromResult(UniqueID.NewEmpty());

    /// <summary>
    /// Функція зворотнього виклику для перевантаження списку
    /// </summary>
    /// <param name="selectPointer">Елемент на якому треба спозиціонувати список</param>
    protected virtual async void CallBack_LoadRecords(UniqueID? selectPointer)
    {
        SelectPointerItem = selectPointer;
        await LoadRecords();
    }

    /// <summary>
    /// Формування відборів для пошуку
    /// </summary>
    /// <param name="searchText">Текст для пошуку</param>
    protected virtual void SetSearch(string searchText) { }

    /// <summary>
    /// Заповнити поля для фільтру
    /// </summary>
    /// <param name="filterControl">Контрол Фільтр</param>
    protected virtual void FillFilter(FilterControl filterControl) { }

    /// <summary>
    /// При зміні періоду в контролі Period
    /// </summary>
    protected virtual void PeriodChanged() { }

    /// <summary>
    /// Провести / відмінити проведення документів
    /// </summary>
    /// <param name="uniqueID">Вибрані елементи</param>
    /// <param name="spendDoc">Провести / відмінити</param>
    protected virtual async ValueTask SpendTheDocument(UniqueID[] uniqueID, bool spendDoc) => await ValueTask.FromResult(true);

    /// <summary>
    /// Друк проводок
    /// </summary>
    /// <param name="uniqueID">Вибрані елементи</param>
    protected virtual void ReportSpendTheDocument(UniqueID[] uniqueID) { }

    #endregion

    #region Grid

    /// <summary>
    /// При активації
    /// </summary>
    /// <param name="position">Позиція</param>
    async ValueTask GridOnActivate(uint position)
    {
        MultiSelection model = (MultiSelection)Grid.Model;

        if (model.GetObject(position) is DocumentRowJournal row)
            if (DocumentPointerItem == null)
                await OpenPageElement(false, row.UniqueID);
            else
            {
                CallBack_OnSelectPointer?.Invoke(row.UniqueID);

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

        {
            Button button = Button.NewFromIconName("new");
            button.MarginEnd = 5;
            button.TooltipText = "Додати";
            button.OnClicked += OnAdd;
            HBoxToolbarTop.Append(button);
        }

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
            NameValue<Action<UniqueID[]>>[]? SubMenu()
            {
                return
                [
                    new ("Відкрити проводки", ReportSpendTheDocument),
                    new ("Провести", async x=> await SpendTheDocument(x, true)),
                    new ("Відмінити проведення", async x=> await SpendTheDocument(x, false)),
                ];
            }

            Button button = Button.NewFromIconName("edit-find");
            button.MarginEnd = 5;
            button.TooltipText = "Проводки";
            button.OnClicked += (_, _) => CreatePopoverMenu(button, SubMenu());
            HBoxToolbarTop.Append(button);
        }

        {
            Separator separator = Separator.New(Orientation.Vertical);
            separator.MarginStart = 5;
            separator.MarginEnd = 10;
            HBoxToolbarTop.Append(separator);
        }
    }

    async void OnAdd(Button button, EventArgs args) => await OpenPageElement(true);

    async void Edit()
    {
        foreach (UniqueID uniqueID in GetSelectionUnigueID())
            await OpenPageElement(false, uniqueID);
    }

    async void OnEdit(Button button, EventArgs args) => Edit();

    async void OnRefresh(Button sender, EventArgs args) => await Refresh();

    async void OnCopy(Button button, EventArgs args)
    {
        UniqueID[] rows = GetSelectionUnigueID();
        if (rows.Length > 0)
        {
            foreach (UniqueID uniqueID in rows)
                SelectPointerItem = await Copy(uniqueID);

            PagesClear();
            await LoadRecords();
        }

        /*
        Message.Request(BasicApp, BasicForm, "Копіювання", "Копіювати вибрані елементи?", async YN =>
        {
            if (YN == Message.YesNo.Yes)
            {
                foreach (RowJournal row in rows)
                    SelectPointerItem = await Copy(row.UniqueID);

                PagesClear();
                await LoadRecords();
            }
        });
        */
    }

    async ValueTask Delete()
    {
        UniqueID[] rows = GetSelectionUnigueID();
        if (rows.Length > 0)
        {
            foreach (UniqueID uniqueID in rows)
                await SetDeletionLabel(uniqueID);
        }

        /*
        Message.Request(BasicApp, BasicForm, "Відмітка для видалення", "Встановити або зняти відмітку для видалення для вибраних елементів?", async YN =>
        {
            if (YN == Message.YesNo.Yes)
            {
                foreach (RowJournal row in rows)
                    await SetDeletionLabel(row.UniqueID);
            }
        });
        */
    }

    async void OnDelete(Button button, EventArgs args) => await Delete();

    void OnFilter(Button button, EventArgs args)
    {
        if (!Filter.IsFilterCreated)
            Filter.CreatePopover(button);

        Filter.PopoverParent?.Show();
    }

    #endregion
}
