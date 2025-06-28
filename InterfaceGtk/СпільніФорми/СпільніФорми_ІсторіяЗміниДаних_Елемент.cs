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

namespace InterfaceGtk
{
    public abstract class СпільніФорми_ІсторіяЗміниДаних_Елемент : Форма
    {
        Kernel Kernel { get; set; }

        CompositePointerControl pointerObj;
        Box hBoxInfo = new Box(Orientation.Horizontal, 5);

        ListBox listBoxField = new ListBox();
        Notebook notebookTablePart = new Notebook() { Scrollable = true, BorderWidth = 0, ShowBorder = false, TabPos = PositionType.Top };

        public СпільніФорми_ІсторіяЗміниДаних_Елемент(Kernel kernel) : base()
        {
            Kernel = kernel;

            //Обєкт
            {
                Box hBox = new Box(Orientation.Horizontal, 0);
                PackStart(hBox, false, false, 10);

                hBox.PackStart(pointerObj = CreateCompositControl(), false, false, 2);

                //Додаткова інформація
                hBox.PackStart(hBoxInfo, false, false, 2);
            }

            //Список
            {
                Box hBox = new Box(Orientation.Horizontal, 0);
                PackStart(hBox, false, false, 5);

                Box vBoxExpander = new Box(Orientation.Vertical, 5);
                vBoxExpander.PackStart(listBoxField, false, false, 5);

                Expander expander = new Expander("Поля") { Expanded = true };
                expander.Add(vBoxExpander);

                hBox.PackStart(expander, false, false, 5);
            }

            //Табличні частини
            {
                Box hBox = new Box(Orientation.Horizontal, 0);
                PackStart(hBox, false, false, 5);

                hBox.PackStart(notebookTablePart, true, true, 5);
            }

            ShowAll();
        }

        #region Virtual & Abstract Function

        protected abstract ValueTask<CompositePointerPresentation_Record> CompositePointerPresentation(UuidAndText uuidAndText);
        protected abstract CompositePointerControl CreateCompositControl();

        #endregion

        public async ValueTask Load(Guid versionID, UuidAndText obj)
        {
            var (result, pointerGroup, pointerType) = Configuration.PointerParse(obj.Text, out Exception? _);
            if (!result) return;

            (Dictionary<string, ConfigurationField> Fields, Dictionary<string, ConfigurationTablePart> TabularParts, string pointerFullName) = pointerGroup switch
            {
                "Довідники" => (Kernel.Conf.Directories[pointerType].Fields, Kernel.Conf.Directories[pointerType].TabularParts, Kernel.Conf.Directories[pointerType].FullName),
                "Документи" => (Kernel.Conf.Documents[pointerType].Fields, Kernel.Conf.Documents[pointerType].TabularParts, Kernel.Conf.Documents[pointerType].FullName),
                _ => throw new Exception("")
            };

            pointerObj.Pointer = obj;
            pointerObj.Caption = "Об'єкт:";

            SpinnerOn(NotebookFunction.GetNotebookFromWidget(this));

            //Поля
            {
                SelectVersionsHistoryItem_Record recordResult = await Kernel.DataBase.SpetialTableObjectVersionsHistorySelect(versionID, obj);
                if (recordResult.Result)
                {
                    //Додаткова інформація
                    {
                        hBoxInfo.PackStart(new Label($"<b>Дата запису:</b> {recordResult.DateWrite}") { UseMarkup = true, UseUnderline = false }, false, false, 5);
                        hBoxInfo.PackStart(new Label($"<b>Користувач:</b> {recordResult.UserName}") { UseMarkup = true, UseUnderline = false }, false, false, 5);
                        hBoxInfo.ShowAll();
                    }

                    Dictionary<string, string> dictionaryFields = recordResult.GetDictionaryFields();
                    Dictionary<string, string> dictionaryPreviousFields = recordResult.GetDictionaryPreviousFields();

                    if (dictionaryFields.Count > 0)
                        foreach (var Field in Fields.Values)
                        {
                            dictionaryFields.TryGetValue(Field.NameInTable, out string? value);

                            bool valueChanged = false;
                            if (dictionaryPreviousFields.TryGetValue(Field.NameInTable, out string? previous_value))
                                if (value != null && value != previous_value) valueChanged = true;

                            switch (Field.Type)
                            {
                                case "string":
                                case "any_pointer":
                                case "string[]":
                                case "integer[]":
                                case "numeric[]":
                                case "uuid[]":
                                    {
                                        if (Field.Multiline)
                                        {
                                            TextView text = new TextView();
                                            text.Buffer.Text = value;

                                            ScrolledWindow scroll = new ScrolledWindow() { ShadowType = ShadowType.In, WidthRequest = 500, HeightRequest = 300 };
                                            scroll.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
                                            scroll.Add(text);

                                            AppendField(Field.FullName, scroll, valueChanged, previous_value);
                                        }
                                        else
                                        {
                                            Entry entry = new Entry() { WidthRequest = 500, Text = value };

                                            AppendField(Field.FullName, entry, valueChanged, previous_value);
                                        }
                                        break;
                                    }
                                case "date":
                                case "datetime":
                                    {
                                        DateTimeControl dateTime = new DateTimeControl() { OnlyDate = Field.Type == "date" };
                                        if (value != null) dateTime.Value = DateTime.Parse(value);

                                        AppendField(Field.FullName, dateTime, valueChanged, previous_value);
                                        break;
                                    }
                                case "time":
                                    {
                                        TimeControl time = new TimeControl();
                                        if (value != null) time.Value = TimeSpan.Parse(value);

                                        AppendField(Field.FullName, time, valueChanged, previous_value);
                                        break;
                                    }
                                case "integer":
                                    {
                                        IntegerControl numeric = new IntegerControl();
                                        if (value != null) numeric.Value = int.Parse(value);

                                        AppendField(Field.FullName, numeric, valueChanged, previous_value);
                                        break;
                                    }
                                case "numeric":
                                    {
                                        NumericControl numeric = new NumericControl();
                                        if (value != null) numeric.Value = decimal.Parse(value);

                                        AppendField(Field.FullName, numeric, valueChanged, previous_value);
                                        break;
                                    }
                                case "boolean":
                                    {
                                        CheckButton check = new CheckButton(Field.FullName);
                                        if (value != null) check.Active = bool.Parse(value);

                                        AppendField("", check, valueChanged, previous_value);
                                        break;
                                    }
                                case "enum":
                                    {
                                        ComboBoxTextControl comboBox = new ComboBoxTextControl();
                                        string previous_value_presentation = "";

                                        string[] searchNameSplit = Field.Pointer.Split(["."], StringSplitOptions.None);
                                        foreach (ConfigurationEnumField enumField in Kernel.Conf.Enums[searchNameSplit[1]].Fields.Values)
                                        {
                                            comboBox.Append(enumField.Value.ToString(), enumField.Desc);

                                            if (valueChanged && previous_value != null && previous_value != "0" && previous_value == enumField.Value.ToString())
                                                previous_value_presentation = enumField.Desc;
                                        }

                                        if (value != null) comboBox.ActiveId = value;

                                        AppendField(Field.FullName, comboBox, valueChanged, previous_value_presentation);
                                        break;
                                    }
                                case "pointer":
                                case "composite_pointer":
                                    {
                                        CompositePointerControl pointer = CreateCompositControl();
                                        string previous_value_presentation = "";
                                        if (value != null)
                                            if (Field.Type == "pointer")
                                            {
                                                pointer.Pointer = new UuidAndText(Guid.Parse(value), Field.Pointer);

                                                if (valueChanged && previous_value != null)
                                                    previous_value_presentation = (await CompositePointerPresentation(new UuidAndText(Guid.Parse(previous_value), Field.Pointer))).result;
                                            }
                                            else
                                            {
                                                {
                                                    string[] UuidNameSplit = value.Split([":"], StringSplitOptions.None);
                                                    if (UuidNameSplit.Length == 2)
                                                        pointer.Pointer = new UuidAndText(Guid.Parse(UuidNameSplit[0]), UuidNameSplit[1]);
                                                }

                                                if (valueChanged && previous_value != null)
                                                {
                                                    string[] UuidNameSplit = previous_value.Split([":"], StringSplitOptions.None);
                                                    if (UuidNameSplit.Length == 2)
                                                        previous_value_presentation = (await CompositePointerPresentation(new UuidAndText(Guid.Parse(UuidNameSplit[0]), UuidNameSplit[1]))).result;
                                                }
                                            }

                                        AppendField(Field.FullName, pointer, valueChanged, previous_value_presentation);
                                        break;
                                    }
                            }
                        }
                }
            }

            //Табличні частини
            {
                SelectVersionsHistoryTablePart_Record recordResult = await Kernel.DataBase.SpetialTableTablePartVersionsHistorySelect(versionID, obj);
                if (recordResult.Result)
                    foreach (ConfigurationTablePart tablePart in TabularParts.Values)
                    {
                        //Таблична частина в якій немає даних пропускається
                        if (!recordResult.ListRow.Where(x => x.TablePart == tablePart.Table).Any())
                            continue;

                        //Поля табличної частини
                        List<ConfigurationField> fieldList = [.. tablePart.Fields.Values];

                        //Model
                        Type[] types = new Type[fieldList.Count];
                        for (int i = 0; i < fieldList.Count; i++)
                            types[i] = typeof(string);

                        ListStore Store = new ListStore(types);
                        TreeView TreeViewGrid = new TreeView(Store);
                        TreeViewGrid.EnableGridLines = TreeViewGridLines.Both;

                        ScrolledWindow scroll = new ScrolledWindow() { ShadowType = ShadowType.In, HeightRequest = 600 };
                        scroll.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
                        scroll.Add(TreeViewGrid);

                        //Колонки
                        for (int i = 0; i < fieldList.Count; i++)
                        {
                            CellRendererText cell = new CellRendererText();
                            TreeViewColumn treeColumn = new TreeViewColumn(fieldList[i].FullName, cell, "text", i) { Resizable = true, MinWidth = 20 };
                            TreeViewGrid.AppendColumn(treeColumn);
                        }

                        notebookTablePart.AppendPage(scroll, new Label(NotebookFunction.SubstringPageName(tablePart.FullName)) { TooltipText = tablePart.FullName });
                        notebookTablePart.ShowAll();

                        //Дані
                        foreach (var row in recordResult.ListRow.Where(x => x.TablePart == tablePart.Table && x.Fields.Length > 0).OrderBy(x => x.DateWrite))
                        {
                            //Рядок
                            TreeIter iter = Store.Append();

                            //Поля
                            Dictionary<string, string> dictionaryFields = row.GetDictionaryFields();
                            for (int i = 0; i < fieldList.Count; i++)
                            {
                                var field = fieldList[i];
                                if (dictionaryFields.TryGetValue(field.NameInTable, out string? value))
                                {
                                    switch (field.Type)
                                    {
                                        case "string":
                                            {
                                                value = value.Replace("\n", " ").Replace("\r", "");
                                                value = value.Length > 100 ? value[..100] + " ..." : value;
                                                break;
                                            }
                                        case "enum":
                                            {
                                                string[] searchNameSplit = field.Pointer.Split(["."], StringSplitOptions.None);
                                                var enumItem = Kernel.Conf.Enums[searchNameSplit[1]].Fields.Values.Single(x => x.Value == int.Parse(value));
                                                if (enumItem != null) value = enumItem.Desc;
                                                break;
                                            }
                                        case "pointer":
                                            {
                                                var record = await CompositePointerPresentation(new UuidAndText(Guid.Parse(value), field.Pointer));
                                                value = record.result;
                                                break;
                                            }
                                    }
                                }
                                Store.SetValue(iter, i, value ?? "");
                            }
                        }

                    }
            }

            SpinnerOff(NotebookFunction.GetNotebookFromWidget(this));
        }

        void AppendField(string caption, Widget widget, bool valueChanged, string? previous_value = null)
        {
            Box hBox = new Box(Orientation.Horizontal, 0) { Halign = Align.End };
            Box vBox = new Box(Orientation.Vertical, 0);
            vBox.PackStart(hBox, false, false, 5);

            //Заголовок
            hBox.PackStart(new Label(caption), false, false, 2);

            //Віджет
            hBox.PackStart(widget, false, false, 2);

            //Змінений
            if (valueChanged)
            {
                Image image = new Image(Іконки.ДляІнформування.Error);
                Label label = new Label("<b>Змінено</b>") { UseMarkup = true };

                if (previous_value != null)
                    image.TooltipText = label.TooltipText = $"Було: \"{previous_value}\"";

                hBox.PackStart(image, false, false, 2);
                hBox.PackStart(label, false, false, 2);
            }

            listBoxField.Add(new ListBoxRow() { vBox });
            listBoxField.ShowAll();
        }
    }
}