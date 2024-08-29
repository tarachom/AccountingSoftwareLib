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

/*

Масове перепроведення документів

*/

using Gtk;
using System.Reflection;
using AccountingSoftware;

namespace InterfaceGtk
{
    public abstract class PageService : Box
    {
        private Kernel Kernel { get; set; }
        private string NameSpageProgram { get; set; }
        private string NameSpageCodeGeneration { get; set; }
        private Assembly ExecutingAssembly { get; } = Assembly.GetCallingAssembly();

        protected PeriodControl Період = new PeriodControl();
        LogMessage Лог = new LogMessage();

        public PageService(Kernel kernel, string nameSpageProgram, string nameSpageCodeGeneration) : base(Orientation.Vertical, 0)
        {
            Kernel = kernel;
            NameSpageProgram = nameSpageProgram;
            NameSpageCodeGeneration = nameSpageCodeGeneration;

            Box vBox = new Box(Orientation.Vertical, 0);
            PackStart(vBox, false, false, 5);

            //Сторінки
            {
                StackSwitcher stackSwitcher = new() { Stack = new Stack() };

                //Stack
                {
                    Box hBox = new Box(Orientation.Horizontal, 0) { Halign = Align.Center };
                    hBox.PackStart(stackSwitcher, false, false, 5);

                    vBox.PackStart(hBox, false, false, 5);
                }

                //Switcher
                {
                    Box hBox = new Box(Orientation.Horizontal, 0);
                    hBox.PackStart(stackSwitcher.Stack, true, true, 5);

                    vBox.PackStart(hBox, true, true, 5);
                }

                stackSwitcher.Stack.AddTitled(ОчисткаПоміченихНаВидалення(), "ОчисткаПоміченихНаВидалення", "Очистка помічених на видалення");
                stackSwitcher.Stack.AddTitled(ПроведенняДокументів(), "ПроведенняДокументів", "Проведення документів");
            }

            //Для виводу результатів
            PackStart(Лог, true, true, 0);

            ShowAll();
        }

        public async ValueTask SetValue()
        {
            await BeforeSetValue();
        }

        Box ПроведенняДокументів()
        {
            Box vBox = new Box(Orientation.Vertical, 0);

            //Період
            {
                Період.Changed = PeriodChanged;

                Box hBox = new Box(Orientation.Horizontal, 0);
                hBox.PackStart(Період, false, false, 5);

                vBox.PackStart(hBox, false, false, 5);
            }

            Button bSpendTheDocument = new Button("Перепровести документи");
            Button bStop = new Button("Зупинити") { Sensitive = false };
            Button bClear = new Button("Очистити");

            //Кнопки
            {
                Box hBox = new Box(Orientation.Horizontal, 0);
                hBox.PackStart(bSpendTheDocument, false, false, 5);
                hBox.PackStart(bStop, false, false, 5);
                hBox.PackEnd(bClear, false, false, 5);

                vBox.PackStart(hBox, false, false, 5);
            }

            void ButtonSensitive(bool sensitive)
            {
                bSpendTheDocument.Sensitive = sensitive;
                bStop.Sensitive = !sensitive;
                bClear.Sensitive = sensitive;
            }

            CancellationTokenSource? CancellationToken = null;

            bSpendTheDocument.Clicked += async (object? sender, EventArgs args) =>
            {
                ButtonSensitive(false);
                await SpendTheDocument(CancellationToken = new CancellationTokenSource(), () => { ButtonSensitive(true); });
            };

            bStop.Clicked += (object? sender, EventArgs args) => { CancellationToken?.Cancel(); };
            bClear.Clicked += (object? sender, EventArgs args) => { Лог.ClearMessage(); };

            return vBox;
        }

        Box ОчисткаПоміченихНаВидалення()
        {
            Box vBox = new Box(Orientation.Vertical, 0);

            Button bClearDeletionLabel = new Button("Очистити помічені на видалення");
            Button bStop = new Button("Зупинити") { Sensitive = false };
            Button bClear = new Button("Очистити");

            //Кнопки
            {
                Box hBox = new Box(Orientation.Horizontal, 0);
                hBox.PackStart(bClearDeletionLabel, false, false, 5);
                hBox.PackStart(bStop, false, false, 5);
                hBox.PackEnd(bClear, false, false, 5);

                vBox.PackStart(hBox, false, false, 5);
            }

            void ButtonSensitive(bool sensitive)
            {
                bClearDeletionLabel.Sensitive = sensitive;
                bStop.Sensitive = !sensitive;
                bClear.Sensitive = sensitive;
            }

            CancellationTokenSource? CancellationToken = null;

            bClearDeletionLabel.Clicked += async (object? sender, EventArgs args) =>
            {
                ButtonSensitive(false);
                await ClearDeletionLabel(CancellationToken = new CancellationTokenSource(), () => { ButtonSensitive(true); });
            };

            bStop.Clicked += (object? sender, EventArgs args) => { CancellationToken?.Cancel(); };
            bClear.Clicked += (object? sender, EventArgs args) => { Лог.ClearMessage(); };

            return vBox;
        }

        #region Функції

        Widget? CreateCompositControl(string caption, UuidAndText uuidAndText)
        {
            object? compositControlInstance = ExecutingAssembly.CreateInstance($"{NameSpageProgram}.CompositePointerControl");
            if (compositControlInstance != null)
            {
                dynamic compositControl = compositControlInstance;

                compositControl.Caption = caption;
                compositControl.ClearSensetive = false;
                compositControl.TypeSelectSensetive = false;
                compositControl.Pointer = uuidAndText;

                return compositControl;
            }
            else
                return null;
        }

        #endregion

        #region Virtual & Abstract Function

        protected virtual async ValueTask BeforeSetValue() { await ValueTask.FromResult(true); }

        protected abstract void PeriodChanged();

        #endregion

        #region ПроведенняДокументів

        async ValueTask SpendTheDocument(CancellationTokenSource cancellationToken, System.Action CallBack)
        {
            object? journalSelectInstance = ExecutingAssembly.CreateInstance($"{NameSpageCodeGeneration}.Журнали.JournalSelect");
            if (journalSelectInstance != null)
            {
                dynamic journalSelect = journalSelectInstance;

                int counterDocs = 0;
                DateTime dateTimeCurrDoc = DateTime.MinValue.Date;

                Лог.CreateMessage($"Період з <b>{Період.DateStartControl.ПочатокДня()}</b> по <b>{Період.DateStopControl.КінецьДня()}</b>", LogMessage.TypeMessage.Info);

                Box hBoxFindDoc = Лог.CreateMessage($"Пошук проведених документів:", LogMessage.TypeMessage.Info, true);
                if (await journalSelect.Select(Період.DateStartControl.ПочатокДня(), Період.DateStopControl.КінецьДня(), null, true))
                {
                    Лог.AppendMessage(hBoxFindDoc, $"знайдено {journalSelect.Count()} документів");
                    while (journalSelect.MoveNext())
                        if (journalSelect.Current != null)
                        {
                            if (cancellationToken!.IsCancellationRequested)
                                break;

                            //Список документів які ігноруються при обрахунку регістрів накопичення
                            if (dateTimeCurrDoc != journalSelect.Current.DocDate.Date)
                            {
                                dateTimeCurrDoc = journalSelect.Current.DocDate.Date;
                                await Kernel.DataBase.SpetialTableRegAccumTrigerDocIgnoreClear(Kernel.User, Kernel.Session);
                                Лог.CreateMessage($"{dateTimeCurrDoc.ToString("dd-MM-yyyy")}", LogMessage.TypeMessage.None);
                            }

                            await Kernel.DataBase.SpetialTableRegAccumTrigerDocIgnoreAdd(Kernel.User, Kernel.Session, journalSelect.Current.UnigueID.UGuid, journalSelect.Current.DocName);

                            DocumentObject? doc = await journalSelect.GetDocumentObject(true);
                            if (doc != null)
                            {
                                Box hBox = Лог.CreateMessage($"Проведення <b>{journalSelect.Current.DocName}</b>", LogMessage.TypeMessage.Info);

                                //Для документу викликається функція проведення
                                if (await ((dynamic)doc).SpendTheDocument(journalSelect.Current.SpendDate))
                                {
                                    //Документ проведений ОК
                                    Лог.AppendMessage(hBox, "Проведено");

                                    counterDocs++;
                                }
                                else
                                {
                                    //Документ НЕ проведений Error
                                    Лог.AppendMessage(hBox, "Помилка", LogMessage.TypeMessage.Error);

                                    //Додатково вивід помилок у це вікно
                                    SelectRequest_Record record = await Kernel.DataBase.SpetialTableMessageErrorSelect(doc.UnigueID, 1);

                                    string msg = "";
                                    foreach (Dictionary<string, object> row in record.ListRow)
                                        msg += "<i>" + row["message"].ToString() + "</i>";

                                    Лог.CreateMessage(msg, LogMessage.TypeMessage.None, true);
                                    Лог.CreateWidget(CreateCompositControl("Документи:", journalSelect.Current.GetBasis()), LogMessage.TypeMessage.None, true);
                                    Лог.CreateMessage("Проведення документів перервано!", LogMessage.TypeMessage.Info, true);

                                    break;
                                }
                            }
                        }

                    await Kernel.DataBase.SpetialTableRegAccumTrigerDocIgnoreClear(Kernel.User, Kernel.Session);

                    Лог.CreateEmptyMsg();
                    Лог.CreateMessage($"Проведено документів: {counterDocs}", LogMessage.TypeMessage.Info, true);
                }
                else
                    Лог.AppendMessage(hBoxFindDoc, $"документів не знайдено", LogMessage.TypeMessage.Error);

                CallBack.Invoke();
                Лог.CreateMessage($"Обробку завершено!", LogMessage.TypeMessage.None, true);

                await Task.Delay(1000);
                Лог.CreateEmptyMsg();
            }
        }

        #endregion

        #region ОчисткаПоміченихНаВидалення

        async ValueTask ClearDeletionLabel(CancellationTokenSource cancellationToken, System.Action CallBack)
        {
            //Шаблон запиту для вибірки елементів помічених на видалення
            string querySelectDeletion = "SELECT uid FROM @TABLE WHERE deletion_label = true";

            Лог.CreateMessage($"<b>Обробка довідників</b>", LogMessage.TypeMessage.Info);
            foreach (ConfigurationDirectories confDirectories in Kernel.Conf.Directories.Values)
            {
                if (cancellationToken!.IsCancellationRequested)
                    break;

                Box hBoxInfo = Лог.CreateMessage($"Довідник <b>{confDirectories.Name}</b>", LogMessage.TypeMessage.Info);
                var recordResult = await Kernel.DataBase.SelectRequest(querySelectDeletion.Replace("@TABLE", confDirectories.Table));
                if (recordResult.ListRow.Count > 0)
                {
                    Лог.AppendMessage(hBoxInfo, $"Помічених на видалення: {recordResult.ListRow.Count}", LogMessage.TypeMessage.Ok);
                    List<ConfigurationDependencies> listDependencies = Kernel.Conf.SearchDependencies("Довідники." + confDirectories.Name); //Пошук залежностей

                    object? directoryObjectInstance = ExecutingAssembly.CreateInstance($"{NameSpageCodeGeneration}.Довідники.{confDirectories.Name}_Objest");
                    if (directoryObjectInstance != null)
                    {
                        dynamic directoryObject = directoryObjectInstance;
                        foreach (Dictionary<string, object> row in recordResult.ListRow)
                        {
                            UnigueID unigueID = new(row["uid"]);
                            if (await directoryObject.Read(unigueID))
                            {
                                string nameObj = await directoryObject.GetPresentation();
                                if (await SearchDependencies(listDependencies, unigueID.UGuid, nameObj) == 0)
                                {
                                    await directoryObject.Delete();
                                    Лог.CreateMessage(" --> Видалено: " + nameObj, LogMessage.TypeMessage.Ok);
                                }
                            }
                        }
                    }
                }
            }

            Лог.CreateEmptyMsg();

            Лог.CreateMessage($"<b>Обробка документів</b>", LogMessage.TypeMessage.Info);
            foreach (ConfigurationDocuments confDocuments in Kernel.Conf.Documents.Values)
            {
                if (cancellationToken!.IsCancellationRequested)
                    break;

                Box hBoxInfo = Лог.CreateMessage($"Документ <b>{confDocuments.Name}</b>", LogMessage.TypeMessage.Info);
                var recordResult = await Kernel.DataBase.SelectRequest(querySelectDeletion.Replace("@TABLE", confDocuments.Table));
                if (recordResult.ListRow.Count > 0)
                {
                    Лог.AppendMessage(hBoxInfo, $"Помічених на видалення: {recordResult.ListRow.Count}", LogMessage.TypeMessage.Ok);

                    //Залежності
                    List<ConfigurationDependencies> listDependencies = Kernel.Conf.SearchDependencies("Документи." + confDocuments.Name);

                    object? documentObjectInstance = ExecutingAssembly.CreateInstance($"{NameSpageCodeGeneration}.Документи.{confDocuments.Name}_Objest");
                    if (documentObjectInstance != null)
                    {
                        dynamic documentObject = documentObjectInstance;
                        foreach (Dictionary<string, object> row in recordResult.ListRow)
                        {
                            UnigueID unigueID = new(row["uid"]);
                            if (await documentObject.Read(unigueID))
                            {
                                string nameObj = await documentObject.GetPresentation();
                                if (await SearchDependencies(listDependencies, unigueID.UGuid, nameObj) == 0)
                                {
                                    await documentObject.Delete();
                                    Лог.CreateMessage(" --> Видалено: " + nameObj, LogMessage.TypeMessage.Ok);
                                }
                            }
                        }
                    }
                }
            }

            CallBack.Invoke();

            Лог.CreateEmptyMsg();
            Лог.CreateMessage($"Обробку завершено!", LogMessage.TypeMessage.None, true);

            await Task.Delay(1000);
            Лог.CreateEmptyMsg();
        }

        async ValueTask<long> SearchDependencies(List<ConfigurationDependencies> listDependencies, Guid uid, string name)
        {
            long allCountDependencies = 0;
            bool existUse = false;

            if (listDependencies.Count > 0)
            {
                Dictionary<string, object> paramQuery = new() { { "uid", uid } };

                foreach (ConfigurationDependencies dependence in listDependencies) //Обробка залежностей
                {
                    string query = "";

                    if (dependence.ConfigurationGroupLevel == ConfigurationDependencies.GroupLevel.Object)
                        query = $"SELECT uid FROM {dependence.Table} WHERE {dependence.Field} = @uid LIMIT 3";
                    else if (dependence.ConfigurationGroupLevel == ConfigurationDependencies.GroupLevel.TablePart)
                        query = $"SELECT DISTINCT owner AS uid FROM {dependence.Table} WHERE {dependence.Field} = @uid LIMIT 3";
                    else if (dependence.ConfigurationGroupLevel == ConfigurationDependencies.GroupLevel.TablePartWithoutOwner)
                        // Для табличних частин Константи та Регістри Накопичення вибирається тільки 1 елемент як факт наявності запису
                        query = $"SELECT uid FROM {dependence.Table} WHERE {dependence.Field} = @uid LIMIT 1";

                    var recordResult = await Kernel.DataBase.SelectRequest(query, paramQuery);
                    if (recordResult.ListRow.Count > 0)
                    {
                        if (!existUse)
                        {
                            Лог.CreateMessage($"{name}. <u>Використовується:</u>", LogMessage.TypeMessage.Error);
                            existUse = true;
                        }

                        allCountDependencies += recordResult.ListRow.Count;
                        Лог.CreateMessage(
                            $"<i>{dependence.ConfigurationGroupName}.{dependence.ConfigurationObjectName}" +
                            (dependence.ConfigurationGroupLevel == ConfigurationDependencies.GroupLevel.TablePart ||
                             dependence.ConfigurationGroupLevel == ConfigurationDependencies.GroupLevel.TablePartWithoutOwner ? $", таблична частина {dependence.ConfigurationTablePartName}" : "") +
                            (dependence.ConfigurationGroupName != "Константи" ? $", поле {dependence.ConfigurationFieldName}" : "") +
                            "</i>", LogMessage.TypeMessage.None);

                        foreach (Dictionary<string, object> row in recordResult.ListRow)
                            if (dependence.ConfigurationGroupName == "Довідники" || dependence.ConfigurationGroupName == "Документи")
                            {
                                Widget? composit = CreateCompositControl("", new UuidAndText((Guid)row["uid"], $"{dependence.ConfigurationGroupName}.{dependence.ConfigurationObjectName}"));
                                Лог.CreateWidget(composit, LogMessage.TypeMessage.None, false);
                            }
                    }
                }
            }

            return allCountDependencies;
        }

        #endregion
    }
}