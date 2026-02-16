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

/*

1. Очистка помічених на видалення
2. Масове перепроведення документів

*/

using Gtk;
using System.Reflection;
using AccountingSoftware;

namespace InterfaceGtk4;

public abstract class PageService : Form
{
    private Kernel Kernel { get; set; }
    private string NamespaceProgram { get; set; }
    private string NamespaceCodeGeneration { get; set; }
    private Assembly ExecutingAssembly { get; } = Assembly.GetCallingAssembly();

    protected PeriodControl Period = new();
    LogMessage Log = new();
    protected const string KeyForSettings = "PageService";

    public PageService(Kernel kernel, string namespaceProgram, string namespaceCodeGeneration, NotebookFunction? notebook) : base(notebook)
    {
        Kernel = kernel;
        NamespaceProgram = namespaceProgram;
        NamespaceCodeGeneration = namespaceCodeGeneration;

        Box vBox = New(Orientation.Vertical, 0);
        vBox.MarginBottom = 5;
        Append(vBox);

        //Сторінки
        {
            StackSwitcher stackSwitcher = StackSwitcher.New();
            stackSwitcher.Stack = Stack.New();

            //Stack
            {
                Box hBox = New(Orientation.Horizontal, 0);
                hBox.Halign = Align.Center;
                hBox.MarginBottom = 5;
                hBox.Append(stackSwitcher);

                vBox.Append(hBox);
            }

            //Switcher
            {
                Box hBox = New(Orientation.Horizontal, 0);
                hBox.Append(stackSwitcher.Stack);

                vBox.Append(hBox);
            }

            stackSwitcher.Stack.AddTitled(ОчисткаПоміченихНаВидалення(), "ОчисткаПоміченихНаВидалення", "Очистка помічених на видалення");
            stackSwitcher.Stack.AddTitled(ПроведенняДокументів(), "ПроведенняДокументів", "Проведення документів");
        }

        //Для виводу результатів
        Log.Vexpand = Log.Hexpand = true;
        Append(Log);
    }

    public async ValueTask SetValue()
    {
        await BeforeSetValue();
    }

    Box ПроведенняДокументів()
    {
        Box vBox = New(Orientation.Vertical, 0);

        ListBox listBoxAllowDoc = ListBox.New();
        listBoxAllowDoc.SelectionMode = SelectionMode.None;

        Popover? popoverAllowDoc = null;

        Button bFilterAllowDoc = Button.NewFromIconName("view-sort-descending");
        bFilterAllowDoc.MarginEnd = 5;

        Button bSpendTheDocument = Button.NewWithLabel("Перепровести документи");
        bSpendTheDocument.MarginEnd = 5;

        Button bStop = Button.NewWithLabel("Зупинити");
        bStop.MarginEnd = 5;
        bStop.Sensitive = false;

        Button bClear = Button.NewWithLabel("Очистити");
        bClear.MarginEnd = 5;

        //Період & Кнопки
        {
            Box hBox = New(Orientation.Horizontal, 0);
            hBox.MarginBottom = 5;

            Period.Changed = PeriodChanged;
            Period.MarginEnd = 5;
            hBox.Append(Period);

            //Фільтер типів документів
            hBox.Append(bFilterAllowDoc);

            //Кнопки
            hBox.Append(bSpendTheDocument);
            hBox.Append(bStop);
            hBox.Append(bClear);

            vBox.Append(hBox);
        }

        void ButtonSensitive(bool sensitive)
        {
            bSpendTheDocument.Sensitive = sensitive;
            bStop.Sensitive = !sensitive;
        }

        (string[] Filter, string[] Info)? CreateFilterAllowDoc()
        {
            List<string> filter = [];
            List<string> info = [];

            ListBoxRow? child = (ListBoxRow?)listBoxAllowDoc.GetFirstChild();
            while (child != null)
            {
                CheckButton? cb = (CheckButton?)child.GetFirstChild();
                if (cb != null && cb.Active)
                {
                    filter.Add(cb.GetName());
                    info.Add(cb.Label ?? "");
                }

                child = (ListBoxRow?)child.GetNextSibling();
            }

            return filter.Count != 0 ? ([.. filter], [.. info]) : null;
        }

        bFilterAllowDoc.OnClicked += (sender, args) =>
        {
            if (popoverAllowDoc == null)
            {
                //Список видів документів
                foreach (ConfigurationDocuments Document in Kernel.Conf.Documents.Values.OrderBy(x => x.Name))
                {
                    CheckButton checkButton = CheckButton.NewWithLabel(Document.FullName);
                    checkButton.Name = $"{Document.Name}:{Document.Table}";
                    listBoxAllowDoc.Append(checkButton);
                }

                ScrolledWindow scroll = ScrolledWindow.New();
                scroll.HeightRequest = 500;
                scroll.HasFrame = true;
                scroll.SetPolicy(PolicyType.Never, PolicyType.Automatic);
                scroll.SetChild(listBoxAllowDoc);

                popoverAllowDoc = Popover.New();
                popoverAllowDoc.SetParent(bFilterAllowDoc);
                popoverAllowDoc.Position = PositionType.Bottom;
                popoverAllowDoc.MarginTop = popoverAllowDoc.MarginEnd = popoverAllowDoc.MarginBottom = popoverAllowDoc.MarginStart = 2;
                popoverAllowDoc.SetChild(scroll);
            }

            popoverAllowDoc.Show();
        };

        CancellationTokenSource? CancellationToken = null;

        bSpendTheDocument.OnClicked += async (sender, args) =>
        {
            ButtonSensitive(false);
            await SpendTheDocument(CancellationToken = new CancellationTokenSource(), CreateFilterAllowDoc(), () => ButtonSensitive(true));
        };

        bStop.OnClicked += (sender, args) => CancellationToken?.Cancel();
        bClear.OnClicked += (sender, args) => Log.ClearMessage();

        return vBox;
    }

    Box ОчисткаПоміченихНаВидалення()
    {
        Box vBox = New(Orientation.Vertical, 0);

        Button bClearDeletionLabel = Button.NewWithLabel("Очистити помічені на видалення");
        bClearDeletionLabel.MarginEnd = 5;

        Button bStop = Button.NewWithLabel("Зупинити");
        bStop.Sensitive = false;
        bStop.MarginEnd = 5;

        Button bClear = Button.NewWithLabel("Очистити");
        bClear.MarginEnd = 5;

        //Кнопки
        {
            Box hBox = New(Orientation.Horizontal, 0);
            hBox.MarginEnd = 5;

            hBox.Append(bClearDeletionLabel);
            hBox.Append(bStop);
            hBox.Append(bClear);

            vBox.Append(hBox);
        }

        void ButtonSensitive(bool sensitive)
        {
            bClearDeletionLabel.Sensitive = sensitive;
            bStop.Sensitive = !sensitive;
        }

        CancellationTokenSource? CancellationToken = null;

        bClearDeletionLabel.OnClicked += async (sender, args) =>
        {
            ButtonSensitive(false);
            await ClearDeletionLabel(CancellationToken = new CancellationTokenSource(), () => ButtonSensitive(true));
        };

        bStop.OnClicked += (sender, args) => CancellationToken?.Cancel();
        bClear.OnClicked += (sender, args) => Log.ClearMessage();

        return vBox;
    }
    
    #region Virtual & Abstract Function

    protected abstract CompositePointerControl CreateCompositeControl(string caption, UuidAndText uuidAndText);

    protected virtual async ValueTask BeforeSetValue() { await ValueTask.FromResult(true); }

    protected abstract void PeriodChanged();

    #endregion

    #region ПроведенняДокументів

    async ValueTask SpendTheDocument(CancellationTokenSource cancellationToken, (string[] Filter, string[] Info)? filterAllowDoc, System.Action CallBack)
    {
        object? journalSelectInstance = ExecutingAssembly.CreateInstance($"{NamespaceCodeGeneration}.Журнали.JournalSelect");
        if (journalSelectInstance != null)
        {
            dynamic journalSelect = journalSelectInstance;

            int counterDocs = 0;
            DateTime dateTimeCurrDoc = DateTime.MinValue.Date;

            Log.CreateMessage($"Період з <b>{Period.DateStartControl.DayBeginning()}</b> по <b>{Period.DateStopControl.DayEnd()}</b>", LogMessage.TypeMessage.Info);

            //Вивід інформації про фільтр
            if (filterAllowDoc.HasValue)
                Log.CreateMessage($"Відбір документів наступних видів: <b>" + string.Join(", ", filterAllowDoc.Value.Info) + "</b>", LogMessage.TypeMessage.None, true);

            Box hBoxFindDoc = Log.CreateMessage($"Пошук проведених документів:", LogMessage.TypeMessage.Info, true);
            if (await journalSelect.Select(Period.DateStartControl.DayBeginning(), Period.DateStopControl.DayEnd(), filterAllowDoc.HasValue ? filterAllowDoc.Value.Filter : null, true))
            {
                Log.AppendMessage(hBoxFindDoc, $"знайдено {journalSelect.Count()} документів");
                while (journalSelect.MoveNext())
                    if (journalSelect.Current != null)
                    {
                        if (cancellationToken!.IsCancellationRequested) break;

                        //Список документів які ігноруються при обрахунку регістрів накопичення
                        if (dateTimeCurrDoc != journalSelect.Current.DocDate.Date)
                        {
                            dateTimeCurrDoc = journalSelect.Current.DocDate.Date;
                            await Kernel.DataBase.SpetialTableRegAccumTrigerDocIgnoreClear(Kernel.User, Kernel.Session);
                            Log.CreateMessage($"{dateTimeCurrDoc.ToString("dd-MM-yyyy")}", LogMessage.TypeMessage.None);
                        }

                        await Kernel.DataBase.SpetialTableRegAccumTrigerDocIgnoreAdd(Kernel.User, Kernel.Session, journalSelect.Current.UnigueID.UGuid, journalSelect.Current.DocName);

                        DocumentObject? doc = await journalSelect.GetDocumentObject(true);
                        if (doc != null)
                        {
                            //Для документу викликається функція проведення
                            if (await ((dynamic)doc).SpendTheDocument(journalSelect.Current.SpendDate))
                            {
                                //Документ проведений ОК
                                Log.AppendLine($"Проведено {journalSelect.Current.DocName}");

                                counterDocs++;
                            }
                            else
                            {
                                //Документ НЕ проведений Error
                                Log.CreateMessage($"Помилка! {journalSelect.Current.DocName}", LogMessage.TypeMessage.Error);

                                //Додатково вивід помилок у це вікно
                                SelectRequest_Record record = await Kernel.SelectMessages(doc.UnigueID, 1);

                                string msg = "";
                                foreach (Dictionary<string, object> row in record.ListRow)
                                    msg += "<i>" + row["message"].ToString() + "</i>";

                                Log.CreateMessage(msg, LogMessage.TypeMessage.None, true);
                                Log.CreateWidget(CreateCompositeControl("Документи:", journalSelect.Current.GetBasis()), LogMessage.TypeMessage.None, true);
                                Log.CreateMessage("Проведення документів перервано!", LogMessage.TypeMessage.Info, true);

                                break;
                            }
                        }
                    }

                await Kernel.DataBase.SpetialTableRegAccumTrigerDocIgnoreClear(Kernel.User, Kernel.Session);

                Log.CreateEmptyMsg();
                Log.CreateMessage($"Проведено документів: {counterDocs}", LogMessage.TypeMessage.Info, true);
            }
            else
                Log.AppendMessage(hBoxFindDoc, $"документів не знайдено", LogMessage.TypeMessage.Error);

            CallBack.Invoke();
            Log.CreateMessage($"Обробку завершено!", LogMessage.TypeMessage.None, true);

            await Task.Delay(1000);
            Log.CreateEmptyMsg();
        }
    }

    #endregion

    #region ОчисткаПоміченихНаВидалення

    async ValueTask ClearDeletionLabel(CancellationTokenSource cancellationToken, System.Action CallBack)
    {
        //Шаблон запиту для вибірки елементів помічених на видалення
        string querySelectDeletion = "SELECT uid FROM @TABLE WHERE deletion_label = true";

        Log.CreateMessage($"<b>Обробка довідників</b>", LogMessage.TypeMessage.Info);
        foreach (ConfigurationDirectories confDirectories in Kernel.Conf.Directories.Values)
        {
            if (cancellationToken!.IsCancellationRequested) break;

            Box hBoxInfo = Log.CreateMessage($"Довідник <b>{confDirectories.FullName}</b>", LogMessage.TypeMessage.Info);
            var recordResult = await Kernel.DataBase.SelectRequest(querySelectDeletion.Replace("@TABLE", confDirectories.Table));
            if (recordResult.ListRow.Count > 0)
            {
                Log.AppendMessage(hBoxInfo, $"Помічених на видалення: {recordResult.ListRow.Count}", LogMessage.TypeMessage.Ok);
                List<ConfigurationDependencies> listDependencies = Kernel.Conf.SearchDependencies("Довідники." + confDirectories.Name); //Пошук залежностей

                object? directoryObjectInstance = ExecutingAssembly.CreateInstance($"{NamespaceCodeGeneration}.Довідники.{confDirectories.Name}_Objest");
                if (directoryObjectInstance != null)
                {
                    dynamic directoryObject = directoryObjectInstance;
                    foreach (Dictionary<string, object> row in recordResult.ListRow)
                    {
                        if (cancellationToken!.IsCancellationRequested) break;

                        UnigueID unigueID = new(row["uid"]);
                        if (await directoryObject.Read(unigueID))
                        {
                            string nameObj = await directoryObject.GetPresentation();
                            if (await SearchDependencies(listDependencies, unigueID.UGuid, nameObj) == 0)
                            {
                                await directoryObject.Delete();
                                Log.AppendLine("Видалено: " + nameObj);
                            }
                        }
                    }
                }
            }
        }

        Log.CreateEmptyMsg();

        Log.CreateMessage($"<b>Обробка документів</b>", LogMessage.TypeMessage.Info);
        foreach (ConfigurationDocuments confDocuments in Kernel.Conf.Documents.Values)
        {
            if (cancellationToken!.IsCancellationRequested) break;

            Box hBoxInfo = Log.CreateMessage($"Документ <b>{confDocuments.FullName}</b>", LogMessage.TypeMessage.Info);
            var recordResult = await Kernel.DataBase.SelectRequest(querySelectDeletion.Replace("@TABLE", confDocuments.Table));
            if (recordResult.ListRow.Count > 0)
            {
                Log.AppendMessage(hBoxInfo, $"Помічених на видалення: {recordResult.ListRow.Count}", LogMessage.TypeMessage.Ok);

                //Залежності
                List<ConfigurationDependencies> listDependencies = Kernel.Conf.SearchDependencies("Документи." + confDocuments.Name);

                object? documentObjectInstance = ExecutingAssembly.CreateInstance($"{NamespaceCodeGeneration}.Документи.{confDocuments.Name}_Objest");
                if (documentObjectInstance != null)
                {
                    dynamic documentObject = documentObjectInstance;
                    foreach (Dictionary<string, object> row in recordResult.ListRow)
                    {
                        if (cancellationToken!.IsCancellationRequested) break;

                        UnigueID unigueID = new(row["uid"]);
                        if (await documentObject.Read(unigueID))
                        {
                            string nameObj = await documentObject.GetPresentation();
                            if (await SearchDependencies(listDependencies, unigueID.UGuid, nameObj) == 0)
                            {
                                await documentObject.Delete();
                                Log.AppendLine("Видалено: " + nameObj);
                            }
                        }
                    }
                }
            }
        }

        CallBack.Invoke();

        Log.CreateEmptyMsg();
        Log.CreateMessage($"Обробку завершено!", LogMessage.TypeMessage.None, true);

        await Task.Delay(1000);
        Log.CreateEmptyMsg();
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
                        Log.CreateMessage($"{name}. <u>Використовується:</u>", LogMessage.TypeMessage.Error);
                        existUse = true;
                    }

                    allCountDependencies += recordResult.ListRow.Count;
                    Log.CreateMessage(
                        $"<i>{dependence.ConfigurationGroupName}.{dependence.ConfigurationObjectName}" +
                        (dependence.ConfigurationGroupLevel == ConfigurationDependencies.GroupLevel.TablePart ||
                         dependence.ConfigurationGroupLevel == ConfigurationDependencies.GroupLevel.TablePartWithoutOwner ? $", таблична частина {dependence.ConfigurationTablePartName}" : "") +
                        (dependence.ConfigurationGroupName != "Константи" ? $", поле {dependence.ConfigurationFieldName}" : "") +
                        "</i>", LogMessage.TypeMessage.None);

                    foreach (Dictionary<string, object> row in recordResult.ListRow)
                        if (dependence.ConfigurationGroupName == "Довідники" || dependence.ConfigurationGroupName == "Документи")
                        {
                            Widget? composit = CreateCompositeControl("", new UuidAndText((Guid)row["uid"], $"{dependence.ConfigurationGroupName}.{dependence.ConfigurationObjectName}"));
                            Log.CreateWidget(composit, LogMessage.TypeMessage.None, false);
                        }
                }
            }
        }

        return allCountDependencies;
    }

    #endregion
}