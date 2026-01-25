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
using AccountingSoftware;
using System.Reflection;

namespace InterfaceGtk4;

/// <summary>
/// Контрол для вибору з типом який потрібно вибрати.
/// В конфігураторі можна задати обмеження для вибору типу (всі довідники чи документи або певні довідники чи документи).
/// </summary>
public abstract class CompositePointerControl : PointerControl
{
    Kernel Kernel { get; set; }
    string NamespaceProgram { get; set; }
    string NamespaceCodeGeneration { get; set; }
    NotebookFunction? NotebookFunc { get; set; }
    Assembly CallingAssembly { get; } = Assembly.GetCallingAssembly();
    event EventHandler<UuidAndText>? PointerChanged;

    public CompositePointerControl(Kernel kernel, string namespaceProgram, string namespaceCodeGeneration, NotebookFunction? notebookFunc)
    {
        Kernel = kernel;
        NamespaceProgram = namespaceProgram;
        NamespaceCodeGeneration = namespaceCodeGeneration;
        NotebookFunc = notebookFunc;

        PointerChanged += OnPointerChanged;

        pointer = new UuidAndText();
        WidthPresentation = 300;
        Caption = "Основа:";

        Button bTypeInfo = Button.NewFromIconName("go-down");
        bTypeInfo.MarginStart = 2;
        bTypeInfo.OnClicked += OnTypeInfo;
        Append(bTypeInfo);
    }

    #region Virtual & Abstract Function

    protected abstract ValueTask<CompositePointerPresentation_Record> CompositePointerPresentation(UuidAndText uuidAndText);

    #endregion

    /// <summary>
    /// Вказівник
    /// </summary>
    public UuidAndText Pointer
    {
        get => pointer;
        set
        {
            pointer = value;
            PointerChanged?.Invoke(this, pointer);
        }
    }
    UuidAndText pointer;

    /// <summary>
    /// При зміні вказівника
    /// </summary>
    protected async void OnPointerChanged(object? _, UuidAndText pointer)
    {
        if (pointer != null)
        {
            CompositePointerPresentation_Record record = await CompositePointerPresentation(pointer);

            Presentation = record.result;
            PointerName = record.pointer;
            TypeCaption = record.type;
        }
        else
            Presentation = PointerName = TypeCaption = "";
    }

    /// <summary>
    /// Документи або Довідники
    /// </summary>
    public string PointerName { get; private set; } = "";

    /// <summary>
    /// Назва обєкту як в конфігурації
    /// </summary>
    public string TypeCaption { get; private set; } = "";

    /// <summary>
    /// Прив'язка до типу даних в конфігурації щоб задіяти додаткові налаштування
    /// Потрібно задавати повний шлях до поля. 
    /// Наприклад: 
    ///     Довідники.Номенклатура.Основа або 
    ///     Документи.Поступлення.НаОснові
    /// </summary>
    public string BoundConfType { get; set; } = "";

    protected override void OpenSelect(Button button, EventArgs args)
    {
        //Якщо вибраний тип тоді відкриваю журнал
        if (PointerName == "Документи" || PointerName == "Довідники")
        {
            object? page;

            try
            {
                page = CallingAssembly.CreateInstance($"{NamespaceProgram}.{TypeCaption}_ШвидкийВибір");
            }
            catch (Exception ex)
            {
                Message.Error(NotebookFunc?.BasicForm?.Application, NotebookFunc?.BasicForm, ex.Message);
                return;
            }

            if (page != null)
            {
                Popover popover = Popover.New();
                popover.SetParent(button);
                popover.WidthRequest = 800;
                popover.HeightRequest = 400;
                BeforeClickOpenFunc?.Invoke();
                popover.SetChild((Widget)page);

                //Прив'язка popover до сторінки
                page.GetType().GetProperty("PopoverParent")?.SetValue(page, popover);

                //Елемент який потрібно виділити в списку
                page.GetType().GetProperty("SelectPointerItem")?.SetValue(page, pointer.UnigueID());

                //Вибір дозволено коли TypeSelectSensetive == true
                if (TypeSelectSensetive)
                {
                    string propertyName = PointerName switch { "Документи" => "DocumentPointerItem", "Довідники" => "DirectoryPointerItem", _ => "" };

                    //Елемент для вибору
                    page.GetType().GetProperty(propertyName)?.SetValue(page, pointer.UnigueID());

                    //Функція зворотнього виклику при виборі
                    Action<UnigueID>? callBackAction = x =>
                    {
                        Pointer = new UuidAndText(x.UGuid, GetBasisName());
                        AfterSelectFunc?.Invoke();
                    };

                    page.GetType().GetProperty("CallBack_OnSelectPointer")?.SetValue(page, callBackAction);
                }

                popover.Show();

                //Заповнення сторінки даними після відкриття
                page.GetType().InvokeMember("SetValue", BindingFlags.InvokeMethod, null, page, null);
            }
        }
        else if (TypeSelectSensetive)
            //Якщо тип не вибраний - тоді відкриваю вікно для вибору
            SelectType(button);
    }

    protected override void OnClear(Button button, EventArgs args) => Pointer = new UuidAndText(GetBasisName());

    protected virtual void OnTypeInfo(Button button, EventArgs args)
    {
        Box vBox = New(Orientation.Vertical, 0);

        Popover popover = Popover.New();
        popover.MarginTop = popover.MarginEnd = popover.MarginBottom = popover.MarginStart = 5;
        popover.SetParent(button);
        popover.SetChild(vBox);

        //Для вибору типу
        Box hBoxSelectType = New(Orientation.Horizontal, 0);
        hBoxSelectType.MarginBottom = 5;
        vBox.Append(hBoxSelectType);

        //Заголовок
        Label labelCaption = Label.New("<b>Тип даних</b>");
        labelCaption.UseMarkup = true;

        Box hBoxCaption = New(Orientation.Horizontal, 0);
        hBoxCaption.MarginBottom = 5;
        hBoxCaption.Halign = Align.Center;
        hBoxCaption.Append(labelCaption);
        vBox.Append(hBoxCaption);

        //Інформація про тип даних
        Label labelInfo = Label.New(null);
        labelInfo.MarginEnd = 5;

        Box hBoxInfo = New(Orientation.Horizontal, 0);
        hBoxInfo.Halign = Align.Center;
        hBoxInfo.Append(labelInfo);
        vBox.Append(hBoxInfo);

        //Лінія
        Separator separator = Separator.New(Orientation.Horizontal);
        separator.MarginTop = separator.MarginBottom = 10;
        vBox.Append(separator);

        //Кнопки
        {
            Box hBoxSelect = New(Orientation.Horizontal, 0);
            hBoxSelect.Halign = Align.Center;
            vBox.Append(hBoxSelect);

            //Вибір типу
            Button bSelect = Button.NewWithLabel("Вибрати");
            bSelect.MarginEnd = 10;
            bSelect.Sensitive = TypeSelectSensetive;
            bSelect.OnClicked += (_, _) =>
            {
                //Внутрішня функція - видалити бокс вибору типу
                void ClearChild()
                {
                    Widget? child = hBoxSelectType.GetFirstChild();
                    if (child != null) hBoxSelectType.Remove(child);
                }

                //Додаткова перевірка і очищення зразу піля натиснення кнопки
                ClearChild();

                //Бокс вибору типу
                Box box = BoxSelectType(() =>
                {
                    //Оновлення інформації
                    Info();

                    //Очищення після вибору
                    ClearChild();
                });

                hBoxSelectType.Append(box);
            };
            hBoxSelect.Append(bSelect);

            //Очищення
            Button bClear = Button.NewWithLabel("Скинути");
            bClear.Sensitive = TypeSelectSensetive;
            bClear.OnClicked += (_, _) => { Pointer = new UuidAndText(); Info(); };
            hBoxSelect.Append(bClear);
        }

        popover.Show();

        //Внутрішня функція
        void Info()
        {
            if (PointerName == "Документи" || PointerName == "Довідники")
            {
                Type? objectConst = CallingAssembly.GetType($"{NamespaceCodeGeneration}.{PointerName}.{TypeCaption}_Const");
                string? typeCaption = objectConst?.GetField("FULLNAME")?.GetValue(null)?.ToString();

                labelInfo.SetText($"{PointerName}: {typeCaption ?? TypeCaption}");
            }
            else
                labelInfo.SetText("Тип даних не заданий");
        }

        Info();
    }

    /// <summary>
    /// Доступність задання типу і можливість вибору. 
    /// Ікнакше тільки перегляд і відкриття
    /// </summary>
    public bool TypeSelectSensetive { get; set; } = true;

    /// <summary>
    /// Функція для стоворення списків вибору типу
    /// </summary>
    /// <param name="CallBackSelect">Функція відображення вибраного типу</param>
    /// <returns>Вертикальний Box</returns>
    Box BoxSelectType(Action? CallBackSelect = null)
    {
        Box vBoxContainer = New(Orientation.Vertical, 0);

        //Заголовок
        Label labelCaption = Label.New("<b>Вибір типу даних</b>");
        labelCaption.UseMarkup = true;

        Box hBoxCaption = New(Orientation.Horizontal, 0);
        hBoxCaption.Halign = Align.Center;
        hBoxCaption.MarginBottom = 5;
        hBoxCaption.Append(labelCaption);
        vBoxContainer.Append(hBoxCaption);

        Box hBox = New(Orientation.Horizontal, 0);
        vBoxContainer.Append(hBox);

        //Списки доступних для вибору типів
        List<string> AllowDirectories = [], AllowDocuments = [];
        bool NotUseDirectories = false;
        bool NotUseDocuments = false;

        //Обробка прив'язаного типу
        if (!string.IsNullOrEmpty(BoundConfType))
        {
            string[] bound_conf_type = BoundConfType.Split(".", StringSplitOptions.None);
            if (bound_conf_type.Length == 3)
            {
                string group = bound_conf_type[0], element = bound_conf_type[1], field = bound_conf_type[2];

                //Внутрішня функція
                void Fill(ConfigurationField configurationField)
                {
                    NotUseDirectories = configurationField.CompositePointerNotUseDirectories;
                    if (!NotUseDirectories) AllowDirectories = configurationField.CompositePointerAllowDirectories;
                    NotUseDocuments = configurationField.CompositePointerNotUseDocuments;
                    if (!NotUseDocuments) AllowDocuments = configurationField.CompositePointerAllowDocuments;
                }

                if (group == "Довідники")
                {
                    if (Kernel.Conf.Directories.TryGetValue(element, out ConfigurationDirectories? configurationDirectories) &&
                        configurationDirectories.Fields.TryGetValue(field, out ConfigurationField? configurationField))
                        Fill(configurationField);
                }
                else if (group == "Документи")
                    if (Kernel.Conf.Documents.TryGetValue(element, out ConfigurationDocuments? configurationDocuments) &&
                        configurationDocuments.Fields.TryGetValue(field, out ConfigurationField? configurationField))
                        Fill(configurationField);
            }
        }

        //Внутрішня функція - обробка вибору
        void Select(string p, string t)
        {
            //Спроба зберегти значення якщо при виборі не змінився тип
            Guid uuid = Guid.Empty;
            if (p == PointerName && t == TypeCaption && Pointer.Uuid != Guid.Empty)
                uuid = Pointer.Uuid;

            PointerName = p;
            TypeCaption = t;

            Pointer = new UuidAndText(uuid, GetBasisName());

            CallBackSelect?.Invoke();
        }

        //Внутрішня функція - заповнення списку
        void AddToList(ListBox listBox, string pinterName, string name, string fullName)
        {
            ListBoxRow row = ListBoxRow.New();
            row.Name = name;

            Label label = Label.New(fullName);
            label.Halign = Align.Start;
            row.SetChild(label);

            listBox.Append(row);

            if ((PointerName == "Документи" || PointerName == "Довідники") && PointerName == pinterName && name == TypeCaption)
                listBox.SelectRow(row);
        }

        //Довідники
        {
            string pinterName = "Довідники";

            Box vBox = New(Orientation.Vertical, 0);
            vBox.MarginEnd = 10;
            hBox.Append(vBox);

            Label label = Label.New(pinterName);
            label.MarginBottom = 5;
            vBox.Append(label);

            GestureClick gesture = GestureClick.New();

            ListBox list = ListBox.New();
            list.SelectionMode = SelectionMode.Single;
            list.AddController(gesture);

            gesture.OnPressed += (_, args) =>
            {
                ListBoxRow? selectedRow = list.GetSelectedRow();
                if (args.NPress >= 2 && selectedRow != null)
                    Select(pinterName, selectedRow.GetName());
            };

            ScrolledWindow scroll = ScrolledWindow.New();
            scroll.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
            scroll.WidthRequest = 300;
            scroll.HeightRequest = 250;
            scroll.HasFrame = true;
            scroll.SetChild(list);

            vBox.Append(scroll);

            if (!NotUseDirectories)
                if (AllowDirectories.Count != 0)
                {
                    foreach (string name in AllowDirectories)
                        if (Kernel.Conf.Directories.TryGetValue(name, out ConfigurationDirectories? directories))
                            AddToList(list, pinterName, directories.Name, directories.FullName);
                }
                else
                    foreach (ConfigurationDirectories directories in Kernel.Conf.Directories.Values)
                        AddToList(list, pinterName, directories.Name, directories.FullName);
        }

        //Документи
        {
            string pinterName = "Документи";

            Box vBox = New(Orientation.Vertical, 0);
            hBox.Append(vBox);

            Label label = Label.New(pinterName);
            label.MarginBottom = 5;
            vBox.Append(label);

            GestureClick gesture = GestureClick.New();

            ListBox list = ListBox.New();
            list.SelectionMode = SelectionMode.Single;
            list.AddController(gesture);

            gesture.OnPressed += (_, args) =>
            {
                ListBoxRow? selectedRow = list.GetSelectedRow();
                if (args.NPress >= 2 && selectedRow != null)
                    Select(pinterName, selectedRow.GetName());
            };

            ScrolledWindow scroll = ScrolledWindow.New();
            scroll.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
            scroll.WidthRequest = 300;
            scroll.HeightRequest = 250;
            scroll.HasFrame = true;
            scroll.SetChild(list);

            vBox.Append(scroll);

            if (!NotUseDocuments)
                if (AllowDocuments.Count != 0)
                {
                    foreach (string name in AllowDocuments)
                        if (Kernel.Conf.Documents.TryGetValue(name, out ConfigurationDocuments? documents))
                            AddToList(list, pinterName, documents.Name, documents.FullName);
                }
                else
                    foreach (ConfigurationDocuments documents in Kernel.Conf.Documents.Values)
                        AddToList(list, pinterName, documents.Name, documents.FullName);
        }

        return vBoxContainer;
    }

    /// <summary>
    /// Відкриває спливаюче вікно для вибору типу даних
    /// </summary>
    /// <param name="button">Кнопка привязки спливаючого вікна</param>
    void SelectType(Button button)
    {
        Popover popover = Popover.New();
        popover.MarginTop = popover.MarginEnd = popover.MarginBottom = popover.MarginStart = 2;
        popover.SetParent(button);

        popover.SetChild(BoxSelectType(() => popover.Hide()));
        popover.Show();
    }

    /// <summary>
    /// Функція формує назву
    /// </summary>
    string GetBasisName() => !string.IsNullOrEmpty(PointerName) ? PointerName + "." + TypeCaption : "";
}