/*
Copyright (C) 2019-2024 TARAKHOMYN YURIY IVANOVYCH
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

namespace InterfaceGtk
{
    public abstract class CompositePointerControl : PointerControl
    {
        event EventHandler<UuidAndText>? PointerChanged;

        #region Abstract

        protected abstract string NameSpageProgram { get; }
        protected abstract string NameSpageCodeGeneration { get; }
        protected abstract Kernel Kernel { get; }
        protected abstract Assembly ExecutingAssembly { get; }
        protected abstract ValueTask<CompositePointerPresentation_Record> CompositePointerPresentation(UuidAndText uuidAndText);
        protected abstract void CreateNotebookPage(string tabName, Func<Widget>? pageWidget);

        #endregion

        public CompositePointerControl()
        {
            PointerChanged += OnPointerChanged;

            pointer = new UuidAndText();
            WidthPresentation = 300;
            Caption = "Основа:";

            Button bTypeInfo = new Button(new Image(Stock.GoDown, IconSize.Menu));
            PackStart(bTypeInfo, false, false, 1);
            bTypeInfo.Clicked += OnTypeInfo;
        }

        UuidAndText pointer;
        public UuidAndText Pointer
        {
            get
            {
                return pointer;
            }
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

        protected override void OpenSelect(object? sender, EventArgs args)
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
                    Message.Error(null, ex.Message);
                    return;
                }

                if (listPage != null)
                {
                    //Елемент який потрібно виділити в списку
                    listPage.GetType().GetProperty("SelectPointerItem")?.SetValue(listPage, pointer.UnigueID());

                    //Елемент для вибору
                    if (PointerName == "Документи")
                        listPage.GetType().GetProperty("DocumentPointerItem")?.SetValue(listPage, pointer.UnigueID());
                    else
                        listPage.GetType().GetProperty("DirectoryPointerItem")?.SetValue(listPage, pointer.UnigueID());

                    //Функція зворотнього виклику при виборі
                    listPage.GetType().GetProperty("CallBack_OnSelectPointer")?.SetValue(listPage, (UnigueID selectPointer) =>
                    {
                        Pointer = new UuidAndText(selectPointer.UGuid, GetBasisName());
                    });

                    //Заголовок журналу з константи конфігурації
                    string listName = "Список";
                    {
                        Type? documentConst = ExecutingAssembly.GetType($"{NameSpageCodeGeneration}.{PointerName}.{TypeCaption}_Const");
                        if (documentConst != null)
                            listName = documentConst.GetField("FULLNAME")?.GetValue(null)?.ToString() ?? listName;
                    }

                    CreateNotebookPage(listName, () => { return (Widget)listPage; });

                    if (PointerName == "Документи")
                        listPage.GetType().InvokeMember("SetValue", BindingFlags.InvokeMethod, null, listPage, null);
                    else
                        listPage.GetType().InvokeMember("LoadRecords", BindingFlags.InvokeMethod, null, listPage, null);
                }
            }
            else
                ВибірТипуДаних((Button)sender!);
        }

        protected override void OnClear(object? sender, EventArgs args)
        {
            Pointer = new UuidAndText(GetBasisName());
        }

        protected virtual void OnTypeInfo(object? sender, EventArgs args)
        {
            Popover PopoverOpenInfo = new Popover((Button)sender!) { Position = PositionType.Bottom, BorderWidth = 2 };

            Box vBoxContainer = new Box(Orientation.Vertical, 0);

            Box hBoxCaption = new Box(Orientation.Horizontal, 0);
            hBoxCaption.PackStart(new Label("<b>Тип даних</b>") { UseMarkup = true, Halign = Align.Center }, true, false, 5);
            vBoxContainer.PackStart(hBoxCaption, false, false, 5);

            Label labelInfo = new Label() { Halign = Align.Center };

            void Info()
            {
                if (PointerName == "Документи" || PointerName == "Довідники")
                {
                    string typeCaption = TypeCaption;

                    Type? objectConst = ExecutingAssembly.GetType($"{NameSpageCodeGeneration}.{PointerName}.{TypeCaption}_Const");
                    if (objectConst != null)
                        typeCaption = objectConst.GetField("FULLNAME")?.GetValue(null)?.ToString() ?? typeCaption;

                    labelInfo.Text = $"{PointerName}: {typeCaption}";
                }
                else
                    labelInfo.Text = "Тип даних не заданий";
            }

            //Інформація про тип даних
            {
                Box hBoxInfo = new Box(Orientation.Horizontal, 0);
                hBoxInfo.PackStart(labelInfo, true, false, 5);
                vBoxContainer.PackStart(hBoxInfo, false, false, 5);

                Info();
            }

            //Кнопка вибору типу
            {
                Box hBoxSelect = new Box(Orientation.Horizontal, 0);
                vBoxContainer.PackStart(hBoxSelect, false, false, 10);

                Button bSelectType = new Button("Вибрати");
                bSelectType.Clicked += (object? sender, EventArgs args) =>
                {
                    ВибірТипуДаних(bSelectType, Info);
                };

                hBoxSelect.PackStart(bSelectType, false, false, 5);

                Button bClearType = new Button("Скинути");
                bClearType.Clicked += (object? sender, EventArgs args) =>
                {
                    Pointer = new UuidAndText();
                    Info();
                };

                hBoxSelect.PackEnd(bClearType, false, false, 5);
            }

            PopoverOpenInfo.Add(vBoxContainer);
            PopoverOpenInfo.ShowAll();
        }

        /// <summary>
        /// Відкриває спливаюче вікно для вибору типу даних
        /// </summary>
        /// <param name="button">Кнопка привязки спливаючого вікна</param>
        void ВибірТипуДаних(Button button, System.Action? CallBackSelect = null)
        {
            Popover PopoverSelect = new Popover(button) { Position = PositionType.Bottom, BorderWidth = 2 };

            void Select(string p, string t)
            {
                PointerName = p;
                TypeCaption = t;
                Pointer = new UuidAndText(Guid.Empty, GetBasisName());

                PopoverSelect.Hide();

                CallBackSelect?.Invoke();
            }

            Box vBoxContainer = new Box(Orientation.Vertical, 0);

            Box hBoxCaption = new Box(Orientation.Horizontal, 0);
            hBoxCaption.PackStart(new Label("<b>Вибір типу даних</b>") { UseMarkup = true, Halign = Align.Center }, true, false, 0);
            vBoxContainer.PackStart(hBoxCaption, false, false, 0);

            Box hBox = new Box(Orientation.Horizontal, 0);
            vBoxContainer.PackStart(hBox, false, false, 0);

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
                ListBoxRow row = new ListBoxRow() { Name = name };
                row.Add(new Label(fullName) { Halign = Align.Start });

                listBox.Add(row);
            }

            //Довідники
            {
                Box vBox = new Box(Orientation.Vertical, 0);
                hBox.PackStart(vBox, false, false, 2);

                vBox.PackStart(new Label("Довідники"), false, false, 2);

                ListBox listBox = new ListBox();
                listBox.ButtonPressEvent += (object? sender, ButtonPressEventArgs args) =>
                {
                    if (args.Event.Type == Gdk.EventType.DoubleButtonPress && listBox.SelectedRows.Length != 0)
                        Select("Довідники", listBox.SelectedRows[0].Name);
                };

                ScrolledWindow scrollList = new ScrolledWindow() { WidthRequest = 300, HeightRequest = 400, ShadowType = ShadowType.In };
                scrollList.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
                scrollList.Add(listBox);

                vBox.PackStart(scrollList, false, false, 2);

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
                Box vBox = new Box(Orientation.Vertical, 0);
                hBox.PackStart(vBox, false, false, 2);

                vBox.PackStart(new Label("Документи"), false, false, 2);

                ListBox listBox = new ListBox();
                listBox.ButtonPressEvent += (object? sender, ButtonPressEventArgs args) =>
                {
                    if (args.Event.Type == Gdk.EventType.DoubleButtonPress && listBox.SelectedRows.Length != 0)
                        Select("Документи", listBox.SelectedRows[0].Name);
                };

                ScrolledWindow scrollList = new ScrolledWindow() { WidthRequest = 300, HeightRequest = 400, ShadowType = ShadowType.In };
                scrollList.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
                scrollList.Add(listBox);

                vBox.PackStart(scrollList, false, false, 2);

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

            PopoverSelect.Add(vBoxContainer);
            PopoverSelect.ShowAll();
        }

        /// <summary>
        /// Функція формує назву
        /// </summary>
        string GetBasisName()
        {
            return !string.IsNullOrEmpty(PointerName) ? PointerName + "." + TypeCaption : "";
        }
    }
}