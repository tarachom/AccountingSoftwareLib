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

public abstract class CompositePointerControl : PointerControl
{
    Kernel Kernel { get; set; }
    string NameSpageProgram { get; set; }
    string NameSpageCodeGeneration { get; set; }
    NotebookFunction? NotebookFunc { get; set; }
    Assembly ExecutingAssembly { get; } = Assembly.GetCallingAssembly();
    event EventHandler<UuidAndText>? PointerChanged;

    public CompositePointerControl(Kernel kernel, string nameSpageProgram, string nameSpageCodeGeneration, NotebookFunction? notebookFunc)
    {
        Kernel = kernel;
        NameSpageProgram = nameSpageProgram;
        NameSpageCodeGeneration = nameSpageCodeGeneration;
        NotebookFunc = notebookFunc;

        PointerChanged += OnPointerChanged;

        pointer = new UuidAndText();
        WidthPresentation = 300;
        Caption = "Основа:";

        Button bTypeInfo = Button.NewFromIconName("go-down");
        bTypeInfo.MarginEnd = 1;
        bTypeInfo.OnClicked += OnTypeInfo;
        Append(bTypeInfo);
    }

    #region Virtual & Abstract Function

    protected abstract ValueTask<CompositePointerPresentation_Record> CompositePointerPresentation(UuidAndText uuidAndText);

    #endregion

    UuidAndText pointer;
    public UuidAndText Pointer
    {
        get => pointer;
        set
        {
            pointer = value;
            PointerChanged?.Invoke(this, pointer);
        }
    }

    protected async void OnPointerChanged(object? sender, UuidAndText pointer)
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
        if (PointerName == "Документи" || PointerName == "Довідники")
        {
            object? listPage;

            try
            {
                listPage = ExecutingAssembly.CreateInstance($"{NameSpageProgram}.{TypeCaption}");
            }
            catch (Exception ex)
            {
                Message.Error(null, null, ex.Message);
                return;
            }

            if (listPage != null)
            {
                //Елемент який потрібно виділити в списку
                listPage.GetType().GetProperty("SelectPointerItem")?.SetValue(listPage, pointer.UnigueID());

                //Вибір дозволено коли TypeSelectSensetive == true
                if (TypeSelectSensetive)
                {
                    //Елемент для вибору
                    if (PointerName == "Документи")
                        listPage.GetType().GetProperty("DocumentPointerItem")?.SetValue(listPage, pointer.UnigueID());
                    else
                        listPage.GetType().GetProperty("DirectoryPointerItem")?.SetValue(listPage, pointer.UnigueID());

                    //Функція зворотнього виклику при виборі
                    listPage.GetType().GetProperty("CallBack_OnSelectPointer")?.SetValue(listPage, (UnigueID selectPointer) => Pointer = new UuidAndText(selectPointer.UGuid, GetBasisName()));
                }

                //Заголовок журналу з константи конфігурації
                string listName = "Список";
                {
                    Type? documentConst = ExecutingAssembly.GetType($"{NameSpageCodeGeneration}.{PointerName}.{TypeCaption}_Const");
                    if (documentConst != null)
                        listName = documentConst.GetField("FULLNAME")?.GetValue(null)?.ToString() ?? listName;
                }

                NotebookFunc?.CreatePage(listName, () => (Widget)listPage);

                listPage.GetType().InvokeMember("SetValue", BindingFlags.InvokeMethod, null, listPage, null);
            }
        }
        else if (TypeSelectSensetive)
            ВибірТипуДаних(button);
    }

    protected override void OnClear(Button button, EventArgs args) => Pointer = new UuidAndText(GetBasisName());

    protected virtual void OnTypeInfo(Button button, EventArgs args)
    {
        Popover popoverOpenInfo = Popover.New();
        popoverOpenInfo.SetParent(button);
        popoverOpenInfo.Position = PositionType.Bottom;
        popoverOpenInfo.MarginTop = popoverOpenInfo.MarginEnd = popoverOpenInfo.MarginBottom = popoverOpenInfo.MarginStart = 5;

        Box vBoxContainer = New(Orientation.Vertical, 0);

        Box hBoxCaption = New(Orientation.Horizontal, 0);
        hBoxCaption.MarginEnd = 5;
        vBoxContainer.Append(hBoxCaption);

        Label label = Label.New("<b>Тип даних</b>");
        label.UseMarkup = true;
        label.Halign = Align.Center;
        hBoxCaption.Append(label);

        Label labelInfo = Label.New(null);
        labelInfo.MarginEnd = 5;
        labelInfo.Halign = Align.Center;

        void Info()
        {
            if (PointerName == "Документи" || PointerName == "Довідники")
            {
                string typeCaption = TypeCaption;

                Type? objectConst = ExecutingAssembly.GetType($"{NameSpageCodeGeneration}.{PointerName}.{TypeCaption}_Const");
                if (objectConst != null)
                    typeCaption = objectConst.GetField("FULLNAME")?.GetValue(null)?.ToString() ?? typeCaption;

                labelInfo.SetText($"{PointerName}: {typeCaption}");
            }
            else
                labelInfo.SetText("Тип даних не заданий");
        }

        //Інформація про тип даних
        {
            Box hBoxInfo = New(Orientation.Horizontal, 0);
            hBoxInfo.MarginBottom = 5;
            hBoxInfo.Append(labelInfo);
            vBoxContainer.Append(hBoxInfo);

            Info();
        }

        //Кнопка вибору типу
        {
            Box hBoxSelect = New(Orientation.Horizontal, 0);
            hBoxSelect.MarginEnd = 10;
            vBoxContainer.Append(hBoxSelect);

            Button bSelectType = Button.NewWithLabel("Вибрати");
            bSelectType.MarginEnd = 5;
            bSelectType.Sensitive = TypeSelectSensetive;
            bSelectType.OnClicked += (_, _) =>
            {
                ВибірТипуДаних(button, popoverOpenInfo, Info);
                //popoverOpenInfo.Hide();
            };

            hBoxSelect.Append(bSelectType);

            Button bClearType = Button.NewWithLabel("Скинути");
            bClearType.MarginEnd = 5;
            bClearType.Sensitive = TypeSelectSensetive;
            bClearType.Halign = Align.End;
            bClearType.OnClicked += (_, _) =>
            {
                Pointer = new UuidAndText();
                Info();
            };

            hBoxSelect.Append(bClearType);
        }

        popoverOpenInfo.SetChild(vBoxContainer);
        popoverOpenInfo.Show();
    }

    /// <summary>
    /// Доступність задання типу і можливість вибору. Ікнакше тільки перегляд і відкриття
    /// </summary>
    public bool TypeSelectSensetive { get; set; } = true;

    /// <summary>
    /// Відкриває спливаюче вікно для вибору типу даних
    /// </summary>
    /// <param name="button">Кнопка привязки спливаючого вікна</param>
    void ВибірТипуДаних(Button button, Popover? parent = null, Action? CallBackSelect = null)
    {
        Popover PopoverSelect = Popover.New();
        PopoverSelect.SetParent(button);
        PopoverSelect.Position = PositionType.Bottom;
        PopoverSelect.MarginTop = PopoverSelect.MarginEnd = PopoverSelect.MarginBottom = PopoverSelect.MarginStart = 2;

        void Select(string p, string t)
        {
            PointerName = p;
            TypeCaption = t;
            Pointer = new UuidAndText(GetBasisName());

            CallBackSelect?.Invoke();
            PopoverSelect.Hide();
        }

        Box vBoxContainer = New(Orientation.Vertical, 0);

        Box hBoxCaption = New(Orientation.Horizontal, 0);

        Label labelCaption = Label.New("<b>Вибір типу даних</b>");
        labelCaption.UseMarkup = true;
        labelCaption.Halign = Align.Center;
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

        void AddToList(ListBox listBox, string name, string fullName)
        {
            ListBoxRow row = ListBoxRow.New();
            row.Name = name;

            Label label = Label.New(fullName);
            label.Halign = Align.Start;
            row.SetChild(label);

            listBox.Append(row);
        }

        //Довідники
        {
            Box vBox = New(Orientation.Vertical, 0);
            hBox.Append(vBox);

            Label label = Label.New("Довідники");
            vBox.Append(label);

            GestureClick gesture = GestureClick.New();

            ListBox listBox = ListBox.New();
            listBox.AddController(gesture);

            gesture.OnPressed += (_, args) =>
            {
                ListBoxRow? selectedRow = listBox.GetSelectedRow();
                if (args.NPress >= 2 && selectedRow != null)
                    Select("Довідники", selectedRow.GetName());
            };

            ScrolledWindow scrollList = ScrolledWindow.New();
            scrollList.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
            scrollList.WidthRequest = 300;
            scrollList.HeightRequest = 400;
            scrollList.HasFrame = true;
            scrollList.SetChild(listBox);

            vBox.Append(scrollList);

            if (!NotUseDirectories)
                if (AllowDirectories.Count != 0)
                {
                    foreach (string name in AllowDirectories)
                        if (Kernel.Conf.Directories.TryGetValue(name, out ConfigurationDirectories? directories))
                            AddToList(listBox, directories.Name, directories.FullName);
                }
                else
                    foreach (ConfigurationDirectories directories in Kernel.Conf.Directories.Values)
                        AddToList(listBox, directories.Name, directories.FullName);
        }

        //Документи
        {
            Box vBox = New(Orientation.Vertical, 0);
            hBox.Append(vBox);

            Label label = Label.New("Документи");
            vBox.Append(label);

            GestureClick gesture = GestureClick.New();

            ListBox listBox = ListBox.New();
            listBox.AddController(gesture);

            gesture.OnPressed += (_, args) =>
            {
                ListBoxRow? selectedRow = listBox.GetSelectedRow();
                if (args.NPress >= 2 && selectedRow != null)
                    Select("Документи", selectedRow.GetName());
            };

            ScrolledWindow scrollList = ScrolledWindow.New();
            scrollList.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
            scrollList.WidthRequest = 300;
            scrollList.HeightRequest = 400;
            scrollList.HasFrame = true;
            scrollList.SetChild(listBox);

            vBox.Append(scrollList);

            if (!NotUseDocuments)
                if (AllowDocuments.Count != 0)
                {
                    foreach (string name in AllowDocuments)
                        if (Kernel.Conf.Documents.TryGetValue(name, out ConfigurationDocuments? documents))
                            AddToList(listBox, documents.Name, documents.FullName);
                }
                else
                    foreach (ConfigurationDocuments documents in Kernel.Conf.Documents.Values)
                        AddToList(listBox, documents.Name, documents.FullName);
        }

        PopoverSelect.SetChild(vBoxContainer);
        PopoverSelect.Show();

        parent?.Hide();
    }

    /// <summary>
    /// Функція формує назву
    /// </summary>
    string GetBasisName() => !string.IsNullOrEmpty(PointerName) ? PointerName + "." + TypeCaption : "";
}