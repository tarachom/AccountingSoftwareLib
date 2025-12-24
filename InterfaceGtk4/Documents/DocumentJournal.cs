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

namespace InterfaceGtk4;

/// <summary>
/// Основа для журналів документів певного виду
/// </summary>
public abstract class DocumentJournal : FormJournal
{
    /// <summary>
    /// Для вибору і позиціювання
    /// </summary>
    public UnigueID? DocumentPointerItem
    {
        get => documentPointerItem;
        set => SelectPointerItem = documentPointerItem = value;
    }
    UnigueID? documentPointerItem;

    /// <summary>
    /// Перевизначення сховища для нового типу даних 
    /// </summary>
    public override Gio.ListStore Store { get; } = Gio.ListStore.New(DocumentRow.GetGType());

    /// <summary>
    /// Функція зворотнього виклику при виборі
    /// </summary>
    public Action<UnigueID>? CallBack_OnSelectPointer { get; set; }

    /// <summary>
    /// Верхній набір меню
    /// </summary>
    protected Box ToolbarTop = New(Orientation.Horizontal, 0);

    /// <summary>
    /// Верхній бок для додаткових кнопок
    /// </summary>
    protected Box HBoxTop = New(Orientation.Horizontal, 0);

    /// <summary>
    /// Період
    /// </summary>
    protected PeriodControl Period = new();

    /// <summary>
    /// Пошук
    /// </summary>
    SearchControl Search = new();

    /// <summary>
    /// Фільтр
    /// </summary>
    public FilterControl Filter { get; } = new(true);

    /// <summary>
    /// Додатковий ключ форми журналу для налаштувань
    /// Використовується для ідентифікації форми яка відкрита наприклад із звіту
    /// </summary>
    public string KeyForSetting { get; set; } = "";

    public DocumentJournal() : base()
    {
        //Кнопки
        HBoxTop.MarginBottom = 5;
        Append(HBoxTop);

        //Період
        Period.MarginEnd = 2;
        Period.Changed = PeriodChanged;
        HBoxTop.Append(Period);

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
            Filter.Період = Period;
        }

        Toolbar();

        MultiSelection model = MultiSelection.New(Store);
        model.OnSelectionChanged += OnGridSelectionChanged;

        Grid.Model = model;
        Grid.OnActivate += async (sender, args) =>
        {
            if (model.GetObject(args.Position) is Row row)
                await OpenPageElement(false, row.UnigueID);
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
    }

    #region Toolbar & Menu

    #endregion

    #region Virtual & Abstract Function

    //protected virtual Menu? ToolbarNaOsnoviSubMenu() { return null; }

    //protected virtual void PrintingSubMenu(Menu Menu) { }

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

    protected abstract void PeriodChanged();

    protected abstract ValueTask SpendTheDocument(UnigueID unigueID, bool spendDoc);

    protected abstract void ReportSpendTheDocument(UnigueID unigueID);

    protected virtual bool IsExportXML() { return false; } //Дозволити експорт

    protected virtual async ValueTask ExportXML(UnigueID unigueID, string pathToFolder) { await ValueTask.FromResult(true); }

    protected virtual bool IsExportExcel() { return false; } //Дозволити експорт

    protected virtual async ValueTask ExportExcel(UnigueID unigueID, string pathToFolder) { await ValueTask.FromResult(true); }

    protected virtual async ValueTask PrintingDoc(UnigueID unigueID) { await ValueTask.FromResult(true); }

    protected virtual async ValueTask VersionsHistory(UnigueID unigueID) { await ValueTask.FromResult(true); }

    #endregion

    #region Toolbar

    void Toolbar()
    {
        ToolbarTop.MarginBottom = 5;
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
            Button button = Button.NewFromIconName("edit-find");
            button.MarginEnd = 5;
            button.TooltipText = "Проводки";
            button.OnClicked += OnReportSpendTheDocument;
            ToolbarTop.Append(button);
        }

        {
            Button button = Button.NewFromIconName("document-print");
            button.MarginEnd = 5;
            button.TooltipText = "Друк";
            button.OnClicked += (_, _) =>
            {
                /*
                Menu menu = new();
                menu.Append("Edit", "on_edit");
                menu.Append("Delete", "on_edit");

                PopoverMenu pop = PopoverMenu.NewFromModel(menu);
                pop.Position = PositionType.Bottom;
                pop.SetParent(button);
                pop.Popup();
                */
            };
            ToolbarTop.Append(button);
        }

        {
            Button button = Button.NewFromIconName("process-working");
            button.MarginEnd = 5;
            button.TooltipText = "Експорт";
            button.OnClicked += (_, _) => { };
            ToolbarTop.Append(button);
        }
    }

    async void OnAdd(Button button, EventArgs args)
    {
        await OpenPageElement(true);
    }

    async void OnEdit(Button button, EventArgs args)
    {
        foreach (Row row in GetSelection())
            await OpenPageElement(false, row.UnigueID);
    }

    async void OnRefresh(Button sender, EventArgs args)
    {
        await LoadRecords();
    }

    async void OnCopy(Button button, EventArgs args)
    {
        List<Row> rows = GetSelection();
        if (rows.Count > 0)
            //!!! треба додати app and window і міняти курсор коли йде копіювання або вивести якесь вікно із статусом
            Message.Request(null, null, "Копіювання", "Копіювати вибрані елементи?", async yesNo =>
            {
                if (yesNo == Message.YesNo.Yes)
                {
                    foreach (Row row in rows)
                        SelectPointerItem = await Copy(row.UnigueID);

                    await LoadRecords();
                }
            });
    }

    async void OnDelete(Button button, EventArgs args)
    {
        List<Row> rows = GetSelection();
        if (rows.Count > 0)
            //!!! треба додати app and window і міняти курсор коли йде видалення або вивести якесь вікно із статусом
            Message.Request(null, null, "Відмітка для видалення", "Відмітити для видалення вибрані елементи?", async yesNo =>
            {
                if (yesNo == Message.YesNo.Yes)
                {
                    foreach (Row row in rows)
                        await SetDeletionLabel(row.UnigueID);

                    await LoadRecords();
                }
            });
    }

    void OnFilter(Button button, EventArgs args)
    {
        if (!Filter.IsFilterCreated)
            Filter.CreatePopover(button);

        Filter.PopoverParent?.Show();
    }

    void OnReportSpendTheDocument(Button button, EventArgs args)
    {

    }

    #endregion
}
