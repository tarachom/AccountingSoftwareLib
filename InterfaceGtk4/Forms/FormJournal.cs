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
/// Основа для класів:
///     DocumentJournalBase (ДокументЖурналБазовий),
///     DirectoryJournalBase (ДовідникЖурналБазовий),
///     
///     Журнал, 
///     РегістриВідомостейЖурнал, 
///     РегістриНакопиченняЖурнал,
///     РегістриНакопиченняЖурнал_СпрощенийРежим
/// </summary>
public abstract class FormJournal : Form
{
    /// <summary>
    /// Вспливаюче вікно власник даної форми (у випадку якщо форма поміщена у Popover)
    /// </summary>
    public Popover? PopoverParent { get; set; } //Може треба запхати в Form !!!

    /// <summary>
    /// Елемент на який треба спозиціонувати список при обновленні
    /// </summary>
    public UnigueID? SelectPointerItem { get; set; }

    /// <summary>
    /// Табличний список
    /// </summary>
    public ColumnView Grid { get; } = ColumnView.New(null);

    /// <summary>
    /// Дані для табличного списку
    /// </summary>
    public abstract Gio.ListStore Store { get; }

    /// <summary>
    /// Відбори
    /// </summary>
    public List<Where>? WhereList { get; set; }

    /// <summary>
    /// Прокрутка дерева
    /// </summary>
    protected ScrolledWindow ScrollGrid { get; } = ScrolledWindow.New();

    /// <summary>
    /// Прокрутка для сторінок
    /// </summary>
    protected ScrolledWindow ScrollPages { get; } = ScrolledWindow.New();

    /// <summary>
    /// Бокс для сторінок
    /// </summary>
    protected Box HBoxPages { get; } = New(Orientation.Horizontal, 0);

    /// <summary>
    /// Інформування про стан відборів
    /// </summary>
    protected Label TypeWhereStateInfo { get; } = Label.New(null);

    //protected ProgressBar Progress { get; } = ProgressBar.New(); //На майбтнє зробити відображення стану копіювання чи іншого процесу

    /// <summary>
    /// Режим який вказує що форма використовується як елемент в іншій формі 
    /// (наприклад дерево використовується в ішому журналі)
    /// </summary>
    public bool CompositeMode { get; set; } = false;

    public FormJournal(NotebookFunction? notebookFunc) : base(notebookFunc)
    {
        //Не переміщати стовпчики
        Grid.Reorderable = false;
        Grid.AccessibleRole = AccessibleRole.Table;

        EventControllerKey contrKey = EventControllerKey.New();
        Grid.AddController(contrKey);
        contrKey.OnKeyReleased += async (sender, args) =>
        {
            //Оновлення сторінки
            if (args.Keyval == (uint)Key.F5)
                await Refresh();
        };

        //Оновлення даних в таблиці
        OnEnqueueRecordsChangedQueue += async (_, _) => await UpdateRecords();

        //Відображення поточного стану відборів (пошук, фільт і т.д)
        OnTypeWhereStateChanged += (_, args) => TypeWhereStateChanged(args);
    }

    #region Function

    /// <summary>
    /// Перед початком завантаження даних в Store (на початку LoadRecords)
    /// </summary>
    public void BeforeLoadRecords()
    {
        if (PopoverParent == null)
            NotebookFunc?.SpinnerOn(GetName());
    }

    /// <summary>
    /// Функція викликається після завантаження даних в Store (в кінці LoadRecords)
    /// </summary>
    /// <param name="selectPosition">Позиція елемента який треба виділити</param>
    public void AfterLoadRecords(uint selectPosition = 0)
    {
        if (PopoverParent == null)
            NotebookFunc?.SpinnerOff(GetName());

        PagesShow();

        //Позиціювання на останньому елементі вибірки у випадку Pages.StartingPosition.End
        if (selectPosition == 0 && Store.GetNItems() > 0 && SelectPointerItem == null &&
            PageStartingPosition == Pages.StartingPosition.End && PagesSettings.CurrentPage == PagesSettings.Record.Pages)
            selectPosition = Store.GetNItems();

        //Виділення рядка
        if (selectPosition > 0)
        {
            Grid.Model.SelectItem(selectPosition - 1, false);
            Grid.Vadjustment?.Upper += 0.1;
        }
    }

    /// <summary>
    /// Прокрутка
    /// </summary>
    /// <param name="selectPosition"></param>
    protected void ScrollTo(uint selectPosition)
    {
        uint rowCount = Store.GetNItems();
        if (rowCount > 0 && Grid.Vadjustment != null)
        {
            //Видима частина
            double pageSize = Grid.Vadjustment.PageSize;

            //Максимальне значення
            double upper = Grid.Vadjustment.Upper;

            if (pageSize > 0 && upper > 0 && upper >= pageSize)
            {
                //Висота одного рядка
                double rowHeidth = upper / rowCount;

                //Висота для потрібної позиції
                double value = rowHeidth * selectPosition;

                //Розмір половини видимої частини
                double pageSizePart = pageSize / 2;

                if (value > pageSizePart)
                    Task.Run(async () =>
                    {
                        //Вимушена затримка, щоб все промалювалося
                        await Task.Delay(100);

                        //Позиціювання потрібного рядка посередині
                        Grid.Vadjustment.SetValue(value - pageSizePart);
                    });
            }
        }
    }

    /// <summary>
    /// При виділенні елементів в таблиці
    /// </summary>
    protected void GridOnSelectionChanged(SelectionModel sender, SelectionModel.SelectionChangedSignalArgs args)
    {
        Bitset selection = Grid.Model.GetSelection();

        //Коли виділений один рядок
        if (selection.GetMinimum() == selection.GetMaximum())
        {
            uint position = selection.GetMaximum();
            RowJournal? row = (RowJournal?)Store.GetObject(position);
            if (row != null) SelectPointerItem = row.UnigueID;
        }
    }

    /// <summary>
    /// Функція повертає список вибраних елементів
    /// </summary>
    /// <returns>Список вибраних елементів якщо є вибрані, або пустий список</returns>
    public List<RowJournal> GetSelection()
    {
        List<RowJournal> rows = [];

        MultiSelection model = (MultiSelection)Grid.Model;
        Bitset selection = model.GetSelection();

        for (uint i = selection.GetMinimum(); i <= selection.GetMaximum(); i++)
            if (model.IsSelected(i) && model.GetObject(i) is RowJournal row)
                rows.Add(row);

        return rows;
    }

    /// <summary>
    /// Функція повертає ІД виділених рядків які бере з функції GetSelection()
    /// </summary>
    /// <returns>Масив ІД виділених рядків</returns>
    public UnigueID[] GetGetSelectionUnigueID() => [.. GetSelection().Select(x => x.UnigueID)];

    /// <summary>
    /// Відкриває Popover із списком лінків. Використовується для меню
    /// </summary>
    /// <param name="widget">Прикріплення Popover</param>
    /// <param name="links">Масив лінків</param>
    public void CreatePopoverMenu(Widget widget, NameValue<Action<UnigueID[]>>[]? links)
    {
        if (links != null)
        {
            Popover popover = Popover.New();
            popover.SetParent(widget);
            popover.Position = PositionType.Bottom;
            //popover.MarginTop = popover.MarginEnd = popover.MarginBottom = popover.MarginStart = 2;

            ListBox list = ListBox.New();
            list.SelectionMode = SelectionMode.None;
            list.Hexpand = true;

            popover.SetChild(list);

            foreach (var item in links)
            {
                ListBoxRow row = ListBoxRow.New();
                Box hBox = New(Orientation.Horizontal, 0);

                //Картинка на початку елемента меню
                Image image = Image.NewFromIconName("doc");
                image.MarginStart = 5;
                hBox.Append(image);

                //Лінк
                LinkButton link = LinkButton.New("");
                link.Label = item.Name;
                link.TooltipText = link.Label;
                link.Halign = Align.Start;
                link.Hexpand = true;
                link.OnActivateLink += (_, _) =>
                {
                    item.Value?.Invoke(GetGetSelectionUnigueID());
                    return true;
                };
                hBox.Append(link);

                row.SetChild(hBox);
                list.Append(row);
            }

            popover.Show();
        }
    }

    /// <summary>
    /// Оновлення таблиці і скидання всіх відборів
    /// </summary>
    protected async ValueTask Refresh()
    {
        TypeWhereState = TypeWhere.Standart;
        WhereList = null;

        PagesClear();
        await LoadRecords();
    }

    #endregion

    #region UpdateRecords

    /// <summary>
    /// Черга для змінених об'єктів. Сюди поміщаються списки порцій List<ObjectChanged> змінених об'єктів
    /// </summary>
    public Queue<List<ObjectChanged>> RecordsChangedQueue { get; } = [];

    /// <summary>
    /// Блокування черги
    /// </summary>
    public readonly Lock Loсked = new();

    /// <summary>
    /// Подія яка виникає після поміщення змінених об'єктів у чергу
    /// </summary>
    event EventHandler? OnEnqueueRecordsChangedQueue;

    /// <summary>
    /// Привязка функції для відслідковування зміни об'єктів
    /// </summary>
    protected void RunUpdateRecords()
    {
        string codePage = PopoverParent != null ? Guid.NewGuid().ToString() : GetName();

        //При закритті PopoverParent
        PopoverParent?.OnHide += (_, _) =>
        {
            NotebookFunc?.RemoveChangeFunc(codePage);
            GC.Collect();
        };

        NotebookFunc?.AddChangeFunc(codePage,
            //Записи які були змінені
            records =>
            {
                //Нові записи зразу поміщаються у список
                List<ObjectChanged> added = records.FindAll(x => x.Type == TypeObjectChanged.Add);
                List<ObjectChanged> filtered = added;

                //та видаляються
                if (added.Count > 0)
                {
                    records.RemoveAll(x => x.Type == TypeObjectChanged.Add);

                    //Додатково треба буде перерахувати сторінки
                    //PagesClear();
                }

                //Видалення записів із Store які були видалені
                if (records.Count > 0)
                {
                    List<ObjectChanged> delete = records.FindAll(x => x.Type == TypeObjectChanged.Delete);
                    if (delete.Count > 0)
                    {
                        foreach (var record in delete)
                            for (uint i = 0; i < Store.GetNItems(); i++)
                            {
                                RowJournal? row = (RowJournal?)Store.GetObject(i);
                                if (row != null && row.UnigueID.UGuid.Equals(record.Uid))
                                {
                                    Store.Remove(i);
                                    break;
                                }
                            }

                        records.RemoveAll(x => x.Type == TypeObjectChanged.Delete);

                        //Додатково треба буде перерахувати сторінки
                        PagesClear();
                    }
                }

                //Перевірка чи присутні у таблиці записи які були змінені
                if (records.Count > 0)
                    for (uint i = 0; i < Store.GetNItems(); i++)
                    {
                        if (records.Count == 0)
                            break;

                        RowJournal? row = (RowJournal?)Store.GetObject(i);
                        if (row != null)
                        {
                            ObjectChanged? obj = records.Find(x => x.Uid.Equals(row.UnigueID.UGuid));
                            if (obj != null)
                            {
                                filtered.Add(obj);
                                records.RemoveAll(x => x.Uid.Equals(row.UnigueID.UGuid));
                            }
                        }
                    }

                if (filtered.Count > 0)
                {
                    //Добавлення в чергу
                    lock (Loсked)
                    {
                        RecordsChangedQueue.Enqueue(filtered);
                    }

                    OnEnqueueRecordsChangedQueue?.Invoke(null, new());
                }
            },
            //Відбір по типу як задано в конфігураторі Довідники.<Назва> або Документи.<Назва>
            TypeName);
    }

    #endregion

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
    public virtual void DefaultGrabFocus() { Grid.GrabFocus(); }

    /// <summary>
    /// Завантаження списку
    /// </summary>
    public abstract ValueTask LoadRecords();

    /// <summary>
    /// Оновлення списку
    /// </summary>
    public abstract ValueTask UpdateRecords();

    /// <summary>
    /// Відображення стану відборів у Label TypeWhereStateInfo
    /// </summary>
    protected virtual void TypeWhereStateChanged(TypeWhere typeWhereState)
    {
        TypeWhereStateInfo.SetText(typeWhereState switch
        {
            TypeWhere.Standart => "Звичайний",
            TypeWhere.Filter => "Застосовано фільтр",
            TypeWhere.Search => "Застосовано пошук",
            _ => ""
        });
    }

    #endregion

    #region Pages

    /// <summary>
    /// Налаштування для обчислення і виводу сторінок
    /// </summary>
    Pages.Settings PagesSettings { get; } = new();

    /// <summary>
    /// Розмір сторінки
    /// </summary>
    public int PageSize
    {
        get => PagesSettings.PageSize;
        set => PagesSettings.PageSize = value;
    }

    /// <summary>
    /// Початкове позиціювання (на початок чи кінець)
    /// </summary>
    public Pages.StartingPosition PageStartingPosition
    {
        get => PagesSettings.Position;
        set => PagesSettings.Position = value;
    }

    protected void PagesClear() => PagesSettings.Clear();

    public void SetPagesSettings(int size, Pages.StartingPosition position = Pages.StartingPosition.Start)
    {
        PageSize = size;
        PageStartingPosition = position;
    }

    /// <summary>
    /// Розбивка на сторінки
    /// </summary>
    /// <param name="splitSelectToPagesFunc">Функція для обчислення сторінок</param>
    /// <param name="querySelect">Вибірка для задання меж</param>
    /// <param name="unigueID">Вибраний елемент для позиціювання вибіоки</param>
    /// <returns></returns>
    public async ValueTask SplitPages(Func<UnigueID? /* Вибраний елемент */, int /* Розмір вибірки */, ValueTask<SplitSelectToPages_Record /* Результат */>> splitSelectToPagesFunc, Query querySelect, UnigueID? unigueID)
    {
        if (!PagesSettings.Calculated)
        {
            //Обчислення розміру вибірки
            PagesSettings.Record = await splitSelectToPagesFunc.Invoke(unigueID, PagesSettings.PageSize);
            PagesSettings.Calculated = true;

            if (unigueID != null && PagesSettings.Record.CurrentPage > 0)
                PagesSettings.CurrentPage = PagesSettings.Record.CurrentPage;
            else if (PagesSettings.Position == Pages.StartingPosition.End)
                PagesSettings.CurrentPage = PagesSettings.Record.Pages;
        }

        if (PagesSettings.Calculated && PagesSettings.Record.Result)
        {
            querySelect.Limit = PagesSettings.Record.PageSize;
            querySelect.Offset = PagesSettings.Record.PageSize * (PagesSettings.CurrentPage - 1);
        }
    }

    /// <summary>
    /// Вивід сторінок
    /// </summary>
    public void PagesShow()
    {
        const int offset = 5;

        //Очищення
        Widget? child = HBoxPages.GetFirstChild();
        while (child != null)
        {
            Widget? next = child.GetNextSibling();
            HBoxPages.Remove(child);
            child = next;
        }

        if (PagesSettings.Record.Pages >= 1)
        {
            Label labelCaption = Label.New("<b>Сторінки:</b> ");
            labelCaption.UseMarkup = true;
            labelCaption.MarginEnd = 5;

            HBoxPages.Append(labelCaption);

            bool writeSpace = false;
            for (int i = 1; i <= PagesSettings.Record.Pages; i++)
                if (i == PagesSettings.CurrentPage)
                {
                    Label labelPage = Label.New($"<b>{i}</b>");
                    labelPage.UseMarkup = true;
                    labelPage.MarginStart = labelPage.MarginEnd = 20;

                    HBoxPages.Append(labelPage);
                }
                else if (i == 1 || i == PagesSettings.Record.Pages || (i > PagesSettings.CurrentPage - offset && i < PagesSettings.CurrentPage + offset))
                {
                    LinkButton link = LinkButton.New(i.ToString());
                    link.Name = i.ToString();
                    link.OnActivateLink += (_, _) =>
                    {
                        async void f() => await LoadRecords();
                        PagesSettings.CurrentPage = int.Parse(link.Name);
                        HBoxPages.Sensitive = false;
                        f();
                        return true;
                    };

                    HBoxPages.Append(link);
                    writeSpace = false;
                }
                else if (!writeSpace)
                {
                    Label labeSpace = Label.New("..");
                    labeSpace.UseMarkup = true;
                    labeSpace.MarginStart = labeSpace.MarginEnd = 20;

                    HBoxPages.Append(labeSpace);
                    writeSpace = true;
                }
        }

        HBoxPages.Sensitive = true;
    }

    #endregion

    #region Key

    /// <summary>
    /// Назва типу як задано в конфігураторі
    /// </summary>
    public string TypeName { get; set; } = "";

    /// <summary>
    /// Додатковий ключ форми журналу для налаштувань
    /// Використовується для ідентифікації форми яка відкрита наприклад із звіту
    /// </summary>
    public string? KeyForSetting { get; set; }

    /// <summary>
    /// Ключ форми - текстовий ключ з типу та додаткового ключа
    /// </summary>
    public string FormKey
    {
        get => TypeName + (!string.IsNullOrEmpty(KeyForSetting) ? $".{KeyForSetting}" : "");
    }

    #endregion

    #region TypeWhere

    /// <summary>
    /// Стан відбору
    /// </summary>
    public TypeWhere TypeWhereState
    {
        get => typeWhereState;
        protected set
        {
            TypeWhere oldTypeWhereState = typeWhereState;
            typeWhereState = value;

            if (oldTypeWhereState != typeWhereState)
                OnTypeWhereStateChanged?.Invoke(null, typeWhereState);
        }
    }
    TypeWhere typeWhereState = TypeWhere.Standart;

    /// <summary>
    /// Подія яка виникає після зміни стану відбору
    /// </summary>
    event EventHandler<TypeWhere>? OnTypeWhereStateChanged;

    /// <summary>
    /// Типи відборів
    /// </summary>
    public enum TypeWhere
    {
        /// <summary>
        /// Стандартний
        /// </summary>
        Standart,

        /// <summary>
        /// Фільтрування
        /// </summary>
        Filter,

        /// <summary>
        /// Пошук
        /// </summary>
        Search
    }

    #endregion
}