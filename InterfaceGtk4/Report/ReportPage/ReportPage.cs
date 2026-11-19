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

Звіт сторінка

*/

using Gtk;
using GObject;
using AccountingSoftware;

namespace InterfaceGtk4;

/// <summary>
/// Сторінка звіту
/// </summary>
[Subclass<Box>("ReportPage")]
[Template<AssemblyResource>("ReportPage.ui")]
public abstract partial class ReportPage : Box
{
    [Connect("button_action")] Button buttonAction;
    [Connect("column_view")] ColumnView columnView;

    public abstract Kernel Kernel { get; init; }
    Gio.ListStore Store { get; } = Gio.ListStore.New(ReportRow.GetGType());

    partial void Initialize()
    {
        columnView.Model = MultiSelection.New(Store);
    }

    /// <summary>
    /// Налаштування колонок
    /// </summary>
    public List<ReportPageColumnSettings> ColumnSettings { get; set; } = [];

    /// <summary>
    /// Створення колонок із налаштувань
    /// </summary>
    public void Columns()
    {
        //Створення колонок
        foreach (var columnSettings in ColumnSettings)
        {
            SignalListItemFactory factory = SignalListItemFactory.New();
            factory.OnSetup += (_, args) =>
            {
                if (args.Object is not ListItem listItem) return;
                listItem.Child = columnSettings.Cell();
            };
            factory.OnBind += (_, args) =>
            {
                if (args.Object is not ListItem listItem) return;
                if (listItem.Child is not IReportCell cell) return;
                if (listItem.Item is not ReportRow row) return;
                cell.SetValue(columnSettings.Value(row));
            };
            factory.OnUnbind += (_, args) =>
            {
                if (args.Object is not ListItem listItem) return;
                if (listItem.Child is not IReportCell cell) return;
                cell.SetValue(null);
            };

            ColumnViewColumn column = ColumnViewColumn.New(columnSettings.Title, factory);
            column.Resizable = true;

            columnView.AppendColumn(column);
        }

        { /* Пуста колонка для заповнення вільного простору */
            ColumnViewColumn column = ColumnViewColumn.New(null, null);
            column.Resizable = true;
            column.Expand = true;
            columnView.AppendColumn(column);
        }
    }

    /// <summary>
    /// Запит
    /// </summary>
    public string Query { get; set; } = "";

    /// <summary>
    /// Параметри запиту
    /// </summary>
    public Dictionary<string, object>? ParamQuery { get; set; } = null;

    /// <summary>
    /// Вибірка даних
    /// </summary>
    public async Task Select()
    {
        RecordResult = await Kernel.DataBase.SelectRequest(Query, ParamQuery);
    }

    /// <summary>
    /// Результат виконання запиту
    /// </summary>
    public SelectRequest_Record RecordResult { get; set; } = new();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="rows"></param>
    public void Fill(List<GObject.Object> rows) =>
        Store.Splice(0, Store.GetNItems(), [.. rows], (uint)rows.Count);
}