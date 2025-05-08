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
        ListBox listBoxField = new ListBox();
        Notebook notebookTablePart = new Notebook() { Scrollable = true, BorderWidth = 0, ShowBorder = false, TabPos = PositionType.Top };
        CompositePointerControl pointerObj;
        Box hBoxInfo = new Box(Orientation.Horizontal, 5);

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
            pointerObj.Caption = pointerGroup switch { "Довідники" => "Довідник", "Документи" => "Документ", _ => "Об'єкт" } + " " + pointerFullName;

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

                    if (dictionaryFields.Count > 0)
                        foreach (var Field in Fields.Values)
                        {
                            switch (Field.Type)
                            {
                                case "string":
                                case "any_pointer":
                                    {
                                        if (Field.Multiline)
                                        {
                                            TextView text = new TextView();

                                            ScrolledWindow scroll = new ScrolledWindow() { ShadowType = ShadowType.In, WidthRequest = 500, HeightRequest = 300 };
                                            scroll.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
                                            scroll.Add(text);

                                            if (dictionaryFields.TryGetValue(Field.NameInTable, out string? value))
                                                text.Buffer.Text = value;

                                            AppendField(Field.FullName, scroll);
                                        }
                                        else
                                        {
                                            Entry entry = new Entry() { WidthRequest = 500 };
                                            if (dictionaryFields.TryGetValue(Field.NameInTable, out string? value))
                                                entry.Text = value;

                                            AppendField(Field.FullName, entry);
                                        }
                                        break;
                                    }
                                case "date":
                                case "datetime":
                                    {
                                        DateTimeControl dateTime = new DateTimeControl() { OnlyDate = Field.Type == "date" };
                                        if (dictionaryFields.TryGetValue(Field.NameInTable, out string? value))
                                            dateTime.Value = DateTime.Parse(value);

                                        AppendField(Field.FullName, dateTime);
                                        break;
                                    }
                                case "time":
                                    {
                                        TimeControl time = new TimeControl();
                                        if (dictionaryFields.TryGetValue(Field.NameInTable, out string? value))
                                            time.Value = TimeSpan.Parse(value);

                                        AppendField(Field.FullName, time);
                                        break;
                                    }
                                case "integer":
                                    {
                                        IntegerControl numeric = new IntegerControl();
                                        if (dictionaryFields.TryGetValue(Field.NameInTable, out string? value))
                                            numeric.Value = int.Parse(value);

                                        AppendField(Field.FullName, numeric);
                                        break;
                                    }
                                case "numeric":
                                    {
                                        NumericControl numeric = new NumericControl();
                                        if (dictionaryFields.TryGetValue(Field.NameInTable, out string? value))
                                            numeric.Value = decimal.Parse(value);

                                        AppendField(Field.FullName, numeric);
                                        break;
                                    }
                                case "boolean":
                                    {
                                        CheckButton check = new CheckButton(Field.FullName);
                                        if (dictionaryFields.TryGetValue(Field.NameInTable, out string? value))
                                            check.Active = bool.Parse(value);

                                        AppendField("", check);
                                        break;
                                    }
                                case "enum":
                                    {
                                        ComboBoxTextControl comboBox = new ComboBoxTextControl();

                                        string[] searchNameSplit = Field.Pointer.Split(["."], StringSplitOptions.None);
                                        foreach (ConfigurationEnumField enumField in Kernel.Conf.Enums[searchNameSplit[1]].Fields.Values)
                                            comboBox.Append(enumField.Value.ToString(), enumField.Desc);

                                        if (dictionaryFields.TryGetValue(Field.NameInTable, out string? value))
                                            comboBox.ActiveId = value;

                                        AppendField(Field.FullName, comboBox);
                                        break;
                                    }
                                case "pointer":
                                case "composite_pointer":
                                    {
                                        CompositePointerControl pointer = CreateCompositControl();
                                        if (dictionaryFields.TryGetValue(Field.NameInTable, out string? value))
                                            if (Field.Type == "pointer")
                                                pointer.Pointer = new UuidAndText(Guid.Parse(value), Field.Pointer);
                                            else
                                            {
                                                string[] UuidNameSplit = value.Split([":"], StringSplitOptions.None);
                                                if (UuidNameSplit.Length == 2)
                                                    pointer.Pointer = new UuidAndText(Guid.Parse(UuidNameSplit[0]), UuidNameSplit[1]);
                                            }

                                        AppendField(Field.FullName, pointer);
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
                {
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
                        foreach (var row in recordResult.ListRow.Where(x => x.TablePart == tablePart.Table).OrderBy(x => x.DateWrite))
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
                                                value = value.Replace("\n", " ");
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
            }
        }

        void AppendField(string caption, Widget widget)
        {
            Box hBox = new Box(Orientation.Horizontal, 0) { Halign = Align.End };
            Box vBox = new Box(Orientation.Vertical, 0);
            vBox.PackStart(hBox, false, false, 5);

            //Заголовок
            hBox.PackStart(new Label(caption), false, false, 2);

            //Віджет
            hBox.PackStart(widget, false, false, 2);

            listBoxField.Add(new ListBoxRow() { vBox });
            listBoxField.ShowAll();
        }

        /*void AppendTablePart(string caption, Widget widget)
        {
            Box vBox = new Box(Orientation.Vertical, 0);

            //Заголовок
            {
                Box hBox = new Box(Orientation.Horizontal, 0);
                hBox.PackStart(new Label(caption), false, false, 2);
                vBox.PackStart(hBox, false, false, 2);
            }

            //Віджет
            {
                Box hBox = new Box(Orientation.Horizontal, 0);
                hBox.PackStart(widget, true, true, 2);
                vBox.PackStart(hBox, true, true, 2);
            }

            listBoxTablePart.Add(new ListBoxRow() { vBox });
            listBoxTablePart.ShowAll();
        }*/
    }
}