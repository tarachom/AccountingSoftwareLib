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
        ListBox listBox = new ListBox();

        public СпільніФорми_ІсторіяЗміниДаних_Елемент(Kernel kernel) : base()
        {
            Kernel = kernel;

            //Список
            {
                Box hBox = new Box(Orientation.Horizontal, 0);
                PackStart(hBox, false, false, 5);

                hBox.PackStart(listBox, false, false, 5);
            }

            ShowAll();
        }

        #region Virtual & Abstract Function

        protected abstract CompositePointerControl CreateCompositControl();

        #endregion

        public async ValueTask Load(Guid versionID, UuidAndText obj)
        {
            var (result, pointerGroup, pointerType) = Configuration.PointerParse(obj.Text, out Exception? _);
            if (!result) return;

            (Dictionary<string, ConfigurationField> Fields, Dictionary<string, ConfigurationTablePart> TabularParts) = pointerGroup switch
            {
                "Довідники" => (Kernel.Conf.Directories[pointerType].Fields, Kernel.Conf.Directories[pointerType].TabularParts),
                "Документи" => (Kernel.Conf.Documents[pointerType].Fields, Kernel.Conf.Documents[pointerType].TabularParts),
                _ => throw new Exception("")
            };

            SelectVersionsHistoryItem_Record recordResult = await Kernel.DataBase.SpetialTableObjectVersionsHistorySelect(versionID, obj);
            if (recordResult.Result)
            {
                Dictionary<string, string> dictionaryFields = recordResult.GetDictionaryFields();

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

                                    Append(Field.FullName, scroll);
                                }
                                else
                                {
                                    Entry entry = new Entry() { WidthRequest = 500 };
                                    if (dictionaryFields.TryGetValue(Field.NameInTable, out string? value))
                                        entry.Text = value;

                                    Append(Field.FullName, entry);
                                }
                                break;
                            }
                        case "date":
                        case "datetime":
                            {
                                DateTimeControl dateTime = new DateTimeControl() { OnlyDate = Field.Type == "date" };
                                if (dictionaryFields.TryGetValue(Field.NameInTable, out string? value))
                                    dateTime.Value = DateTime.Parse(value);

                                Append(Field.FullName, dateTime);
                                break;
                            }
                        case "time":
                            {
                                TimeControl time = new TimeControl();
                                if (dictionaryFields.TryGetValue(Field.NameInTable, out string? value))
                                    time.Value = TimeSpan.Parse(value);

                                Append(Field.FullName, time);
                                break;
                            }
                        case "integer":
                            {
                                IntegerControl numeric = new IntegerControl();
                                if (dictionaryFields.TryGetValue(Field.NameInTable, out string? value))
                                    numeric.Value = int.Parse(value);

                                Append(Field.FullName, numeric);
                                break;
                            }
                        case "numeric":
                            {
                                NumericControl numeric = new NumericControl();
                                if (dictionaryFields.TryGetValue(Field.NameInTable, out string? value))
                                    numeric.Value = decimal.Parse(value);

                                Append(Field.FullName, numeric);
                                break;
                            }
                        case "boolean":
                            {
                                CheckButton check = new CheckButton(Field.FullName);
                                if (dictionaryFields.TryGetValue(Field.NameInTable, out string? value))
                                    check.Active = bool.Parse(value);

                                Append("", check);
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

                                Append(Field.FullName, comboBox);
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

                                Append(Field.FullName, pointer);
                                break;
                            }
                    }
                }
            }
        }

        public void Append(string caption, Widget widget)
        {
            Box hBox = new Box(Orientation.Horizontal, 0) { Halign = Align.End };
            Box vBox = new Box(Orientation.Vertical, 0);
            vBox.PackStart(hBox, false, false, 5);

            //Заголовок
            hBox.PackStart(new Label(caption) , false, false, 2);

            //Віджет
            hBox.PackStart(widget, false, false, 2);

            listBox.Add(new ListBoxRow() { vBox });
            listBox.ShowAll();
        }
    }
}