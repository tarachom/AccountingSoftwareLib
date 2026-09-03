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

using Gtk;
using AccountingSoftware;

namespace InterfaceGtk4;

/// <summary>
/// Вивід повідомлень
/// </summary>
[GObject.Subclass<Box>("CommonForms_ErrorOrInfoMessageOutput")]
[Template<AssemblyResource>("CommonForms_ErrorOrInfoMessageOutput.ui")]
public abstract partial class CommonForms_ErrorOrInfoMessageOutput : Box
{
    [Connect("button_clear")] protected Button buttonClear;
    [Connect("button_refresh")] protected Button buttonRefresh;
    [Connect("listbox_log")] protected ListBox listBoxLog;

    partial void Initialize()
    {
        buttonClear.OnClicked += async (_, _) =>
        {
            await Kernel.ClearAllMessages();
            await LoadRecords();
        };

        buttonRefresh.OnClicked += async (_, _) => await LoadRecords();
    }

    #region Virtual & Abstract Function

    public abstract Kernel Kernel { get; init; }
    public abstract CompositePointerControl CreateCompositeControl(string caption, UuidAndText uuidAndText);

    #endregion

    public async Task LoadRecords(UniqueID? objectUid = null, int? limit = null)
    {
        //Вибірка повідомлень
        SelectRequest_Record record = await Kernel.SelectMessages(objectUid, limit);

        //Очистка списку
        listBoxLog.RemoveAll();

        //Заповнення списку
        foreach (Dictionary<string, object> row in record.ListRow)
            listBoxLog.Append(CommonForms_ErrorOrInfoMessageOutputRow.NewWithData(row, this));
    }

    /// <summary>
    /// Видалення одного повідомлення
    /// </summary>
    /// <param name="child">Рядок</param>
    /// <param name="pkey">Ід запису</param>
    public async Task Remove(CommonForms_ErrorOrInfoMessageOutputRow child, int pkey)
    {
        await Kernel.RemoveMessage(pkey);
        listBoxLog.Remove(child);
    }
}