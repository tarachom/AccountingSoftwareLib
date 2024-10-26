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

Звіт сторінка

*/

using Gtk;
using AccountingSoftware;

namespace InterfaceGtk
{
    public abstract class ЗвітСторінка : Форма
    {
        Kernel Kernel { get; set; }
        TreeView? TreeViewGrid = null;

        /// <summary>
        /// Назва звіту
        /// </summary>
        public string ReportName { get; set; } = "";

        /// <summary>
        /// Заголовок
        /// </summary>
        public string Caption { get; set; } = "";

        /// <summary>
        /// Запит
        /// </summary>
        public string Query { get; set; } = "";

        /// <summary>
        /// Параметри запиту
        /// </summary>
        public Dictionary<string, object>? ParamQuery { get; set; } = null;

        /// <summary>
        /// Результат виконання запиту
        /// </summary>
        public SelectRequest_Record RecordResult { get; set; } = new SelectRequest_Record();

        /// <summary>
        /// Налаштування для колонок
        /// </summary>
        public Dictionary<string, ColumnsSettings> ColumnSettings { get; set; } = [];

        /// <summary>
        /// Додаткова інформація для звіту
        /// </summary>
        public Func<ValueTask<string>>? GetInfo { get; set; } = null;

        public ЗвітСторінка(Kernel kernel)
        {
            Kernel = kernel;
        }

        public record ColumnsSettings(string Caption = "", string DataColumn = "", string Type = "", float Xalign = 0, TreeCellDataFunc? Func = null)
        {
            public string Caption { get; set; } = Caption;
            public string DataColumn { get; set; } = DataColumn;
            public string Type { get; set; } = Type;
            public float Xalign { get; set; } = Xalign;
            public TreeCellDataFunc? Func { get; set; } = Func;
        }

        /// <summary>
        /// Вибірка даних
        /// </summary>
        public async ValueTask Select()
        {
            RecordResult = await Kernel.DataBase.SelectRequest(Query, ParamQuery);
        }

        /// <summary>
        /// Вигрузка даних у список
        /// </summary>
        /// <returns></returns>
        public List<string[]> FillList()
        {
            List<string> columnsList = RecordResult.ColumnsName.ToList();
            List<string[]> rows = [];

            Dictionary<string, ColumnsSettings> newColumnSettings = [];

            //Перевірка наявності колонки
            foreach (var columnSettings in ColumnSettings)
                if (columnsList.Exists((x) => x == columnSettings.Key))
                    newColumnSettings.Add(columnSettings.Key, columnSettings.Value);

            int colsLen = newColumnSettings.Count;

            //Заголовки колонок
            {
                string[] cols = new string[colsLen];
                rows.Add(cols);

                int counter = 0;
                foreach (var columnSettings in newColumnSettings)
                    cols[counter++] = columnSettings.Value.Caption;
            }

            //Дані
            foreach (Dictionary<string, object> row in RecordResult.ListRow)
            {
                string[] cols = new string[colsLen];
                rows.Add(cols);

                int counter = 0;
                foreach (var columnSettings in newColumnSettings)
                    cols[counter++] = row[columnSettings.Key]?.ToString() ?? "";
            }

            return rows;
        }

        /// <summary>
        /// Вигрузка даних у таблицю
        /// </summary>
        public void FillTreeView()
        {
            List<string> columnsList = RecordResult.ColumnsName.ToList();

            //Model
            Type[] types = new Type[columnsList.Count];
            for (int i = 0; i < columnsList.Count; i++)
                types[i] = typeof(string);

            ListStore listStore = new ListStore(types);

            TreeViewGrid = new TreeView(listStore);
            TreeViewGrid.EnableGridLines = TreeViewGridLines.Both;
            TreeViewGrid.ButtonPressEvent += ВідкритиДовідникАбоДокумент;
            TreeViewGrid.StyleContext.AddClass("report");

            for (int i = 0; i < columnsList.Count; i++)
            {
                string сolumnName = RecordResult.ColumnsName[i];
                if (ColumnSettings.TryGetValue(сolumnName, out ColumnsSettings? columnSettings))
                {
                    CellRendererText cell = new CellRendererText() { Xalign = columnSettings.Xalign };
                    TreeViewColumn treeColumn = new TreeViewColumn(columnSettings.Caption, cell, "text", i) { Alignment = columnSettings.Xalign, Resizable = true, MinWidth = 20 };
                    TreeViewGrid.AppendColumn(treeColumn);

                    //Привязка колонки з даними
                    if (!string.IsNullOrEmpty(columnSettings.DataColumn) && columnsList.Exists((x) => x == columnSettings.DataColumn))
                    {
                        treeColumn.Data.Add("Column", сolumnName);
                        treeColumn.Data.Add("ColumnDataType", columnSettings.Type);
                        treeColumn.Data.Add("ColumnDataNum", columnsList.FindIndex((x) => x == columnSettings.DataColumn));
                    }

                    //Функція обробки ячейки
                    if (columnSettings.Func != null)
                    {
                        treeColumn.Data.Add("CellDataFunc", сolumnName);
                        treeColumn.SetCellDataFunc(cell, columnSettings.Func);
                    }
                }
            }

            //Колонка пустишка для заповнення вільного простору
            TreeViewGrid.AppendColumn(new TreeViewColumn());

            //Заповнення даними
            foreach (Dictionary<string, object> row in RecordResult.ListRow)
            {
                string[] values = new string[columnsList.Count];
                for (int i = 0; i < columnsList.Count; i++)
                    values[i] = row[RecordResult.ColumnsName[i]]?.ToString() ?? "";

                listStore.AppendValues(values);
            }
        }

        /// <summary>
        /// Відобразити звіт як сторінку блокноту
        /// </summary>
        /// <param name="notebook">Блокнот</param>
        /// <param name="insertPage"></param>
        public async ValueTask View(Notebook? notebook, bool insertPage = false)
        {
            Box vBox = new Box(Orientation.Vertical, 0);

            //Toolbar
            {
                Toolbar toolbar = new Toolbar();

                //Обновити
                {
                    ToolButton button = new ToolButton(new Image(Stock.Refresh, IconSize.Menu), "Обновити") { TooltipText = "Обновити" };
                    toolbar.Add(button);
                    button.Clicked += async (object? sender, EventArgs args) =>
                    {
                        await Select();
                        FillTreeView();

                        Notebook? notebook = NotebookFunction.GetNotebookFromWidget(vBox);
                        await View(notebook, true);
                        NotebookFunction.CloseNotebookPageToCode(notebook, vBox.Name);
                    };
                }

                ToolItem separator = [new Separator(Orientation.Horizontal)];
                toolbar.Add(separator);

                //Зберегти в довіднику "Збережені звіти"
                {
                    ToolButton button = new ToolButton(new Image(Stock.Save, IconSize.Menu), "Зберегти в довіднику \"Збережені звіти\"") { TooltipText = "Зберегти в довіднику \"Збережені звіти\"" };
                    toolbar.Add(button);
                    button.Clicked += async (object? sender, EventArgs args) =>
                    {
                        button.Sensitive = false;

                        await Select();
                        await ЗберегтиЗвіт(this, FillList());
                    };
                }

                //Довідник "Збережені звіти"
                {
                    ToolButton button = new ToolButton(new Image(Stock.GoForward, IconSize.Menu), "Відкрити довідник \"Збережені звіти\"") { TooltipText = "Відкрити довідник \"Збережені звіти\"" };
                    toolbar.Add(button);
                    button.Clicked += async (object? sender, EventArgs args) => await ВідкритиЗбереженіЗвіти();
                }

                //PDF
                {
                    ToolButton button = new ToolButton(new Image(Stock.Print, IconSize.Menu), "Вигрузити в PDF файл") { TooltipText = "Вигрузити в PDF файл" };
                    toolbar.Add(button);
                    button.Clicked += async (object? sender, EventArgs args) =>
                    {
                        button.Sensitive = false;

                        await Select();
                        await ВигрузитиВФайл_PDF(this, FillList());
                    };
                }

                //Excel
                {
                    ToolButton button = new ToolButton(new Image(Stock.Convert, IconSize.Menu), "Вигрузити в Excel файл") { TooltipText = "Вигрузити в Excel файл" };
                    toolbar.Add(button);
                    button.Clicked += async (object? sender, EventArgs args) =>
                    {
                        button.Sensitive = false;

                        await Select();
                        await ВигрузитиВФайл_Excel(this, FillList());
                    };
                }

                vBox.PackStart(toolbar, false, false, 0);
            }

            //Інформаційний бокс
            if (GetInfo != null)
            {
                string infoText = await GetInfo.Invoke();
                if (!string.IsNullOrEmpty(infoText))
                {
                    Box hBoxCaption = new Box(Orientation.Horizontal, 0);
                    hBoxCaption.PackStart(new Label(infoText) { Wrap = true, UseMarkup = true, UseUnderline = false }, false, false, 2);
                    CreateField(vBox, null, hBoxCaption, Align.Start);
                }
            }

            //TreeView
            ScrolledWindow scroll = new ScrolledWindow() { ShadowType = ShadowType.In };
            scroll.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
            scroll.Add(TreeViewGrid);

            vBox.PackStart(scroll, true, true, 5);

            NotebookFunction.CreateNotebookPage(notebook, Caption, () => vBox, insertPage);
        }

        void ВідкритиДовідникАбоДокумент(object sender, ButtonPressEventArgs args)
        {
            if (args.Event.Type == Gdk.EventType.DoubleButtonPress)
            {
                TreeView treeView = (TreeView)sender;
                treeView.GetCursor(out TreePath itemPath, out TreeViewColumn treeColumn);

                if (treeColumn.Data.ContainsKey("Column"))
                {
                    treeView.Model.GetIter(out TreeIter iter, itemPath);

                    //Два ключі Column і ColumnDataNum йдуть в парі
                    int columnDataNum = int.Parse(treeColumn.Data["ColumnDataNum"]?.ToString() ?? "0");

                    //Тип даних (Документи.*, Документи.<Назва>, Довідники.*, Довідники.<Назва>)
                    string columnDataType = treeColumn.Data["ColumnDataType"]?.ToString() ?? "";

                    string valueDataCell = (string)treeView.Model.GetValue(iter, columnDataNum);

                    //
                    //Вміст колонки даних може бути двох видів:
                    // 1. Guid
                    // 2. Guid:Вид (Документу або Довідника)
                    // 

                    string uid, vyd = "";
                    if (valueDataCell.Contains(':'))
                    {
                        // 2. Guid:Вид
                        string[] uid_and_text = valueDataCell.Split(":");
                        uid = uid_and_text[0];
                        vyd = uid_and_text[1];
                    }
                    else
                    {
                        // 1. Guid
                        uid = valueDataCell;
                    }

                    if (!Guid.TryParse(uid, out _))
                        return;

                    UnigueID unigueID = new UnigueID(uid);
                    if (unigueID.IsEmpty())
                        return;

                    //
                    //Тип даних (Документи.*, Документи.<Назва>, Довідники.*, Довідники.<Назва>)
                    //

                    string pointer;
                    string type;
                    if (columnDataType.Contains('.'))
                    {
                        string[] pointer_and_type = columnDataType.Split(".");
                        pointer = pointer_and_type[0];
                        type = pointer_and_type[1];
                    }
                    else
                        return;

                    if ((type == "" || type == "*") && vyd != "")
                        type = vyd;

                    if (pointer == "Документи")
                        ВідкритиДокументВідповідноДоВиду(type, unigueID, ".Report");
                    else if (pointer == "Довідники")
                        ВідкритиДовідникВідповідноДоВиду(type, unigueID);
                }
            }
        }

        #region Virtual & Abstract Function

        protected abstract void ВідкритиДокументВідповідноДоВиду(string name, UnigueID? unigueID, string keyForSetting = "");
        protected abstract void ВідкритиДовідникВідповідноДоВиду(string name, UnigueID? unigueID);
        protected virtual async ValueTask ЗберегтиЗвіт(ЗвітСторінка звіт, List<string[]> rows) { await ValueTask.FromResult(true); }
        protected virtual async ValueTask ВідкритиЗбереженіЗвіти() { await ValueTask.FromResult(true); }
        protected virtual async ValueTask ВигрузитиВФайл_PDF(ЗвітСторінка звіт, List<string[]> rows) { await ValueTask.FromResult(true); }
        protected virtual async ValueTask ВигрузитиВФайл_Excel(ЗвітСторінка звіт, List<string[]> rows) { await ValueTask.FromResult(true); }

        #endregion

        #region ФункціяДляКолонки

        public static void ФункціяДляКолонкиБазоваДляЧисла(TreeViewColumn column, CellRenderer cell, ITreeModel model, TreeIter iter)
        {
            CellRendererText cellText = (CellRendererText)cell;
            if (column.Data.Contains("CellDataFunc"))
                if (float.TryParse(cellText.Text, out float result))
                {
                    if (result == 0)
                        cellText.Text = "";
                    else
                        cellText.Foreground = "green";
                }
        }

        public static void ФункціяДляКолонкиВідємнеЧислоЧервоним(TreeViewColumn column, CellRenderer cell, ITreeModel model, TreeIter iter)
        {
            CellRendererText cellText = (CellRendererText)cell;
            if (column.Data.Contains("CellDataFunc"))
                if (float.TryParse(cellText.Text, out float result))
                {
                    if (result == 0)
                        cellText.Text = "";
                    else
                        cellText.Foreground = (result >= 0) ? "green" : "red";
                }
        }

        #endregion
    }
}