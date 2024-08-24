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
        protected LogMessage Лог = new LogMessage();

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

                stackSwitcher.Stack.AddTitled(ПроведенняДокументів(), "ПроведенняДокументів", "Проведення документів");
                stackSwitcher.Stack.AddTitled(ОчисткаПоміченихНаВидалення(), "ОчисткаПоміченихНаВидалення", "Очистка помічених на видалення");
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
                hBox.PackStart(bClear, false, false, 5);

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
                hBox.PackStart(bClear, false, false, 5);

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

        #region Virtual & Abstract Function

        protected virtual async ValueTask BeforeSetValue() { await ValueTask.FromResult(true); }

        protected abstract void PeriodChanged();

        protected abstract ValueTask SpendTheDocument(CancellationTokenSource cancellationToken, System.Action CallBack);

        protected abstract Widget CreateCompositControl(string caption, UuidAndText uuidAndText);

        #endregion

        #region ПроведенняДокументів

        public async ValueTask ДодатиДокументВСписокІгнорування(Guid document, string info)
        {
            await Kernel.DataBase.SpetialTableRegAccumTrigerDocIgnoreAdd(Kernel.User, Kernel.Session, document, info);
        }

        public async ValueTask ОчиститиСписокІгноруванняДокументів()
        {
            await Kernel.DataBase.SpetialTableRegAccumTrigerDocIgnoreClear(Kernel.User, Kernel.Session);
        }

        #endregion

        #region ОчисткаПоміченихНаВидалення

        protected async ValueTask ClearDeletionLabel(CancellationTokenSource cancellationToken, System.Action CallBack)
        {
            Лог.CreateMessage($"<b>Обробка довідників</b>", LogMessage.TypeMessage.Info);
            foreach (ConfigurationDirectories confDirectories in Kernel.Conf.Directories.Values)
            {
                if (cancellationToken!.IsCancellationRequested)
                    break;

                Box hBoxInfo = Лог.CreateMessage($"Довідник <b>{confDirectories.Name}</b>", LogMessage.TypeMessage.Info);

                var recordResult = await Kernel.DataBase.SelectRequest($"SELECT uid FROM {confDirectories.Table} WHERE deletion_label = true");
                if (recordResult.ListRow.Count > 0)
                {
                    Лог.AppendMessage(hBoxInfo, $"Помічених на видалення: {recordResult.ListRow.Count}", LogMessage.TypeMessage.Ok);

                    //Пошук залежностей
                    List<ConfigurationDependencies> listDependencies = Kernel.Conf.SearchDependencies("Довідники." + confDirectories.Name);

                    //Обєкт довідника
                    object? directoryObject = ExecutingAssembly.CreateInstance($"{NameSpageCodeGeneration}.Довідники.{confDirectories.Name}_Objest");
                    if (directoryObject != null)
                        foreach (Dictionary<string, object> row in recordResult.ListRow)
                        {
                            UnigueID unigueID = new(row["uid"]);
                            string nameObj = "";

                            object? objRead = directoryObject.GetType().InvokeMember("ReadSync", BindingFlags.InvokeMethod, null, directoryObject, [unigueID]);
                            if (objRead != null && (bool)objRead)
                            {
                                object? objName = directoryObject.GetType().InvokeMember("GetPresentationSync", BindingFlags.InvokeMethod, null, directoryObject, null);
                                if (objName != null) nameObj = (string)objName;

                                long allCountDependencies = await SearchDependencies(listDependencies, unigueID.UGuid, nameObj);
                                if (allCountDependencies == 0)
                                {
                                    directoryObject.GetType().InvokeMember("DeleteSync", BindingFlags.InvokeMethod, null, directoryObject, null);
                                    Лог.CreateMessage(" --> Видалено: " + nameObj, LogMessage.TypeMessage.Ok);
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

                var recordResult = await Kernel.DataBase.SelectRequest(@$"SELECT uid, docname FROM {confDocuments.Table} WHERE deletion_label = true");
                if (recordResult.ListRow.Count > 0)
                {
                    Лог.AppendMessage(hBoxInfo, $"Помічених на видалення: {recordResult.ListRow.Count}", LogMessage.TypeMessage.Ok);

                    //Пошук залежностей
                    List<ConfigurationDependencies> listDependencies = Kernel.Conf.SearchDependencies("Документи." + confDocuments.Name);

                    //Обєкт документу
                    object? documentObject = ExecutingAssembly.CreateInstance($"{NameSpageCodeGeneration}.Документи.{confDocuments.Name}_Objest");
                    if (documentObject != null)
                        foreach (Dictionary<string, object> row in recordResult.ListRow)
                        {
                            UnigueID unigueID = new(row["uid"]);
                            string nameObj = (string)row["docname"];

                            object? objRead = documentObject.GetType().InvokeMember("ReadSync", BindingFlags.InvokeMethod, null, documentObject, [unigueID, false]);
                            if (objRead != null && (bool)objRead)
                            {
                                long allCountDependencies = await SearchDependencies(listDependencies, unigueID.UGuid, nameObj);
                                if (allCountDependencies == 0)
                                {
                                    documentObject.GetType().InvokeMember("DeleteSync", BindingFlags.InvokeMethod, null, documentObject, null);
                                    Лог.CreateMessage(" --> Видалено: " + nameObj, LogMessage.TypeMessage.Ok);
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

                Лог.CreateMessage(name, LogMessage.TypeMessage.Error);
                
                //Обробка залежностей
                foreach (ConfigurationDependencies dependence in listDependencies)
                {
                    string query = "";

                    if (dependence.ConfigurationGroupLevel == ConfigurationDependencies.GroupLevel.Object)
                        query = $"SELECT uid FROM {dependence.Table} WHERE {dependence.Field} = @uid LIMIT 3";
                    else if (dependence.ConfigurationGroupLevel == ConfigurationDependencies.GroupLevel.TablePart)
                        query = $"SELECT DISTINCT owner AS uid FROM {dependence.Table} WHERE {dependence.Field} = @uid LIMIT 3";

                    var recordResult = await Kernel.DataBase.SelectRequest(query, paramQuery);
                    if (recordResult.ListRow.Count > 0)
                    {
                        if (!existUse)
                        {
                            Лог.CreateMessage("<u>Використовується:</u>", LogMessage.TypeMessage.None);
                            existUse = true;
                        }

                        allCountDependencies += recordResult.ListRow.Count;
                        Лог.CreateMessage($"<i>{dependence.ConfigurationGroupName}.{dependence.ConfigurationObjectName}" +
                            (dependence.ConfigurationGroupLevel == ConfigurationDependencies.GroupLevel.TablePart ? $", таблична частина {dependence.ConfigurationTablePartName}" : "") +
                            $", поле {dependence.ConfigurationFieldName}</i>", LogMessage.TypeMessage.None);

                        foreach (Dictionary<string, object> row in recordResult.ListRow)
                            if (dependence.ConfigurationGroupName == "Довідники" || dependence.ConfigurationGroupName == "Документи")
                            {
                                Widget composit = CreateCompositControl("", new UuidAndText((Guid)row["uid"], $"{dependence.ConfigurationGroupName}.{dependence.ConfigurationObjectName}"));
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