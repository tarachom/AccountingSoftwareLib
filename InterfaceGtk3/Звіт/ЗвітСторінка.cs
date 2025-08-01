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

/*

Звіт сторінка

*/

using Gtk;
using AccountingSoftware;

namespace InterfaceGtk3;

public abstract class ЗвітСторінка(Kernel kernel) : Форма
{
    Kernel Kernel { get; set; } = kernel;
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
    /// Стиль для TreeViewGrid
    /// </summary>
    public string TreeStyleClass { get; set; } = "report";

    /// <summary>
    /// Параметри запиту
    /// </summary>
    public Dictionary<string, object>? ParamQuery { get; set; } = null;

    /// <summary>
    /// Результат виконання запиту
    /// </summary>
    public SelectRequest_Record RecordResult { get; set; } = new SelectRequest_Record();

    /// <summary>
    /// Додаткова інформація для звіту
    /// </summary>
    public Func<ValueTask<string>>? GetInfo { get; set; } = null;

    #region View

    /// <summary>
    /// Налаштування для колонок
    /// </summary>
    public Dictionary<string, ColumnsSettings> ColumnSettings { get; set; } = [];

    public record ColumnsSettings(string Caption = "", string DataColumn = "", string Type = "", float Xalign = 0, TreeCellDataFunc? Func = null,
        ФункціїДляДинамічногоВідкриття.TypeForm TypeOpenForm = ФункціїДляДинамічногоВідкриття.TypeForm.Journal)
    {
        /// <summary>
        /// Заголовок колонки
        /// </summary>
        public string Caption { get; set; } = Caption;

        /// <summary>
        /// Колонка з даними для даної колонки
        /// </summary>
        public string DataColumn { get; set; } = DataColumn;

        /// <summary>
        /// Тип даних клітинки
        /// </summary>
        public string Type { get; set; } = Type;

        /// <summary>
        /// Позиція тексту в клітинці (0 зліва, 0.5 середина 1 зправа)
        /// </summary>
        public float Xalign { get; set; } = Xalign;

        /// <summary>
        /// Функція для клітинки
        /// </summary>
        public TreeCellDataFunc? Func { get; set; } = Func;

        /// <summary>
        /// Тип форми який буде відкриватися коли заданий тип
        /// </summary>
        public ФункціїДляДинамічногоВідкриття.TypeForm TypeForm { get; set; } = TypeOpenForm;
    }

    #endregion

    #region PDF

    public Dictionary<string, PDFColumnsSettings> PDFColumnSettings { get; set; } = [];

    public record PDFColumnsSettings(string Caption = "", float Width = 1, TypePDFColumn TypeColumn = TypePDFColumn.Relative, float Xalign = 0, Func<object, string>? Func = null)
    {
        public string Caption { get; set; } = Caption;
        public float Width { get; set; } = Width;
        public TypePDFColumn Type { get; set; } = TypeColumn;
        public float Xalign { get; set; } = Xalign;
        public Func<object, string>? Func { get; set; } = Func;
    }

    public enum TypePDFColumn
    {
        Constant,
        Relative
    }

    #endregion

    #region Excel

    public Dictionary<string, ExcelColumnsSettings> ExcelColumnSettings { get; set; } = [];

    public record ExcelColumnsSettings(string Caption = "", string Type = "", float Width = 0, float Xalign = 0, Func<object, string>? Func = null)
    {
        public string Caption { get; set; } = Caption;

        /// <summary>
        /// Тип даних клітинки
        /// </summary>
        public string Type { get; set; } = Type;

        /// <summary>
        /// Ширина колонки, 0 - авторозмір
        /// </summary>
        public float Width { get; set; } = Width;

        /// <summary>
        /// 0 - зліва, 0.5 - середина, 1 - зправа
        /// </summary>
        public float Xalign { get; set; } = Xalign;

        /// <summary>
        /// 
        /// </summary>
        public Func<object, string>? Func { get; set; } = Func;
    }

    #endregion

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
    public List<string[]> FillList()
    {
        Dictionary<string, ColumnsSettings> newColumnSettings = [];
        List<string[]> rows = [];

        List<string> columns = [.. RecordResult.ColumnsName];

        //Перевірка наявності колонки
        foreach (var columnSettings in ColumnSettings)
            if (columns.Exists((x) => x == columnSettings.Key))
                newColumnSettings.Add(columnSettings.Key, columnSettings.Value);

        int columnsCount = newColumnSettings.Count;

        //Заголовки колонок
        {
            string[] cols = new string[columnsCount];
            rows.Add(cols);

            int counter = 0;
            foreach (var columnSettings in newColumnSettings)
                cols[counter++] = columnSettings.Value.Caption;
        }

        //Дані
        foreach (Dictionary<string, object> row in RecordResult.ListRow)
        {
            string[] cols = new string[columnsCount];
            rows.Add(cols);

            int counter = 0;
            foreach (var columnSettings in newColumnSettings)
                cols[counter++] = row[columnSettings.Key]?.ToString() ?? "";
        }

        return rows;
    }

    /// <summary>
    /// Функція повертає набір рядків із налаштуваннями для створення PDF
    /// </summary>
    public (Dictionary<string, PDFColumnsSettings> Settings, List<string[]> Rows) FillListForPDF()
    {
        Dictionary<string, PDFColumnsSettings> newPDFColumnSettings = [];
        List<string[]> rows = [];

        List<string> columns = [.. RecordResult.ColumnsName];

        //Перевірка наявності колонки з результатів запиту в налаштуваннях
        foreach (var columnSettings in PDFColumnSettings)
            if (columns.Exists((x) => x == columnSettings.Key))
                newPDFColumnSettings.Add(columnSettings.Key, columnSettings.Value);

        int columnsCount = newPDFColumnSettings.Count;

        //Дані
        foreach (Dictionary<string, object> row in RecordResult.ListRow)
        {
            string[] cols = new string[columnsCount];

            int counter = 0;
            foreach (var columnSettings in newPDFColumnSettings)
            {
                string value = row[columnSettings.Key]?.ToString() ?? "";
                if (columnSettings.Value.Func != null)
                    value = columnSettings.Value.Func(value);

                cols[counter++] = value;
            }

            rows.Add(cols);
        }

        return (newPDFColumnSettings, rows);
    }

    /// <summary>
    /// Функція повертає набір рядків із налаштуваннями для створення Excel
    /// </summary>
    public (Dictionary<string, ExcelColumnsSettings> Settings, List<string[]> Rows) FillListForExcel()
    {
        Dictionary<string, ExcelColumnsSettings> newExcelColumnSettings = [];
        List<string[]> rows = [];

        List<string> columns = [.. RecordResult.ColumnsName];

        //Перевірка наявності колонки з результатів запиту в налаштуваннях
        foreach (var columnSettings in ExcelColumnSettings)
            if (columns.Exists((x) => x == columnSettings.Key))
                newExcelColumnSettings.Add(columnSettings.Key, columnSettings.Value);

        int columnsCount = newExcelColumnSettings.Count;

        //Дані
        foreach (Dictionary<string, object> row in RecordResult.ListRow)
        {
            string[] cols = new string[columnsCount];

            int counter = 0;
            foreach (var columnSettings in newExcelColumnSettings)
            {
                string value = row[columnSettings.Key]?.ToString() ?? "";
                if (columnSettings.Value.Func != null)
                    value = columnSettings.Value.Func(value);

                cols[counter++] = value;
            }

            rows.Add(cols);
        }

        return (newExcelColumnSettings, rows);
    }

    /// <summary>
    /// Вигрузка даних у таблицю
    /// </summary>
    public void FillTreeView()
    {
        List<string> columnsList = [.. RecordResult.ColumnsName];

        //Model
        Type[] types = new Type[columnsList.Count];
        for (int i = 0; i < columnsList.Count; i++)
            types[i] = typeof(string);

        ListStore listStore = new ListStore(types);

        TreeViewGrid = new TreeView(listStore);
        TreeViewGrid.EnableGridLines = TreeViewGridLines.Both;
        TreeViewGrid.ButtonPressEvent += ВідкритиДовідникАбоДокумент;

        if (!string.IsNullOrEmpty(TreeStyleClass))
            TreeViewGrid.StyleContext.AddClass(TreeStyleClass);

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

                //Вказується тільки тип даних якщо немає привязки до колонки з даними
                if (!string.IsNullOrEmpty(columnSettings.Type) && string.IsNullOrEmpty(columnSettings.DataColumn))
                    treeColumn.Data.Add("ColumnDataType", columnSettings.Type);

                //Функція обробки ячейки
                if (columnSettings.Func != null)
                {
                    treeColumn.Data.Add("CellDataFunc", сolumnName);
                    treeColumn.SetCellDataFunc(cell, columnSettings.Func);
                }

                //Тип форми
                if (columnSettings.TypeForm != ФункціїДляДинамічногоВідкриття.TypeForm.Journal)
                    treeColumn.Data.Add("CellTypeOpenForm", columnSettings.TypeForm.ToString());
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

    public void FillTreeView2()
    {
        //У вигляді дерева. Групувати по колонках
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
                string label = "Оновити";

                ToolButton button = new ToolButton(new Image(Stock.Refresh, IconSize.Menu), label) { TooltipText = label };
                toolbar.Add(button);
                button.Clicked += async (sender, args) =>
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

            //PDF
            {
                string label = "Вигрузити в PDF файл";

                ToolButton button = new ToolButton(new Image(Stock.Print, IconSize.Menu), label) { TooltipText = label };
                toolbar.Add(button);
                button.Clicked += async (sender, args) =>
                {
                    button.Sensitive = false;

                    await Select();
                    await ВигрузитиВФайл_PDF(this, FillListForPDF());

                    button.Sensitive = true;
                };
            }

            //Excel
            {
                string label = "Вигрузити в Excel файл";

                ToolButton button = new ToolButton(new Image(Stock.Convert, IconSize.Menu), label) { TooltipText = label };
                toolbar.Add(button);
                button.Clicked += async (sender, args) =>
                {
                    button.Sensitive = false;

                    await Select();
                    await ВигрузитиВФайл_Excel(this, FillListForExcel());

                    button.Sensitive = true;
                };
            }

            AppendToolbar(toolbar);

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

        vBox.PackStart(scroll, true, true, 0);

        NotebookFunction.CreateNotebookPage(notebook, Caption, () => vBox, insertPage);
    }

    /// <summary>
    /// Відобразити звіт в боксі
    /// </summary>
    /// <param name="hBox">Бокс</param>
    /// <param name="heightRequest">Висота</param>
    /// <returns></returns>
    public async ValueTask ViewToBox(Box hBox, int heightRequest = 100)
    {
        Box vBox = new Box(Orientation.Vertical, 0);

        //Назва
        CreateField(vBox, Caption, null, Align.Start);

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
        ScrolledWindow scroll = new ScrolledWindow() { ShadowType = ShadowType.In, HeightRequest = heightRequest };
        scroll.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
        scroll.Add(TreeViewGrid);

        vBox.PackStart(scroll, true, true, 0);
        hBox.PackStart(vBox, true, true, 5);
        hBox.ShowAll();
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

                //
                //Тип форми
                //

                ФункціїДляДинамічногоВідкриття.TypeForm typeOpenForm = ФункціїДляДинамічногоВідкриття.TypeForm.Journal;
                string typeForm = treeColumn.Data["CellTypeOpenForm"]?.ToString() ?? "";
                if (!string.IsNullOrEmpty(typeForm))
                    typeOpenForm = Enum.Parse<ФункціїДляДинамічногоВідкриття.TypeForm>(typeForm);

                if (pointer == "Документи")
                    ВідкритиДокументВідповідноДоВиду(type, unigueID, ".Report", typeOpenForm);
                else if (pointer == "Довідники")
                    ВідкритиДовідникВідповідноДоВиду(type, unigueID, typeOpenForm);
            }
        }
    }

    #region Virtual & Abstract Function
    protected abstract void ВідкритиДокументВідповідноДоВиду(string name, UnigueID? unigueID, string keyForSetting = "", ФункціїДляДинамічногоВідкриття.TypeForm typeForm = ФункціїДляДинамічногоВідкриття.TypeForm.Journal);
    protected abstract void ВідкритиДовідникВідповідноДоВиду(string name, UnigueID? unigueID, ФункціїДляДинамічногоВідкриття.TypeForm typeForm = ФункціїДляДинамічногоВідкриття.TypeForm.Journal);

    protected virtual async void AppendToolbar(Toolbar toolbar) { await ValueTask.FromResult(true); }
    protected virtual async ValueTask ВигрузитиВФайл_PDF(ЗвітСторінка звіт, (Dictionary<string, PDFColumnsSettings> Settings, List<string[]> Rows) settingsAndRows) { await ValueTask.FromResult(true); }
    protected virtual async ValueTask ВигрузитиВФайл_Excel(ЗвітСторінка звіт, (Dictionary<string, ExcelColumnsSettings> Settings, List<string[]> Rows) settingsAndRows) { await ValueTask.FromResult(true); }

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

    public static string ФункціяДляКолонкиБазоваДляЧисла(object? value)
    {
        string? valueString = value?.ToString();
        if (string.IsNullOrEmpty(valueString)) return "";

        if (float.TryParse(valueString, out float result))
            return result == 0 ? "" : result.ToString();
        else
            return valueString ?? "";
    }

    #endregion
}