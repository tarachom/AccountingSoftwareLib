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
/// ДокументЖурналПовний
/// 
/// Основа для журналів документів певного виду (Повний)
/// </summary>
public abstract class DocumentFormJournalFull : DocumentFormJournalBase
{
    public DocumentFormJournalFull(NotebookFunction? notebookFunc) : base(notebookFunc)
    {
        AddToolbar();
    }

    #region Virtual & Abstract Function

    /// <summary>
    /// Меню друк
    /// </summary>
    protected virtual NameValue<Action<UnigueID[]>>[]? SetPrintMenu() { return null; }

    /// <summary>
    /// Меню експорт
    /// </summary>
    protected virtual NameValue<Action<UnigueID[]>>[]? SetExportMenu() { return null; }

    /// <summary>
    /// Меню ввести на основі
    /// </summary>
    protected virtual NameValue<Action<UnigueID[]>>[]? SetEnterDocumentBasedMenu() { return null; }

    /// <summary>
    /// Історія версій
    /// </summary>
    /// <param name="unigueID">Вибрані елементи</param>
    protected virtual async ValueTask VersionsHistory(UnigueID[] unigueID) { await ValueTask.FromResult(true); }

    #endregion

    #region Grid



    #endregion

    #region Toolbar

    void AddToolbar()
    {
        {
            Button button = Button.NewFromIconName("document-open-recent");
            button.MarginEnd = 5;
            button.TooltipText = "Ввести на основі";
            button.OnClicked += (_, _) => CreatePopoverMenu(button, SetEnterDocumentBasedMenu());
            HBoxToolbarTop.Append(button);
        }

        {
            Button button = Button.NewFromIconName("document-print");
            button.MarginEnd = 5;
            button.TooltipText = "Друк";
            button.OnClicked += (_, _) => CreatePopoverMenu(button, SetPrintMenu());
            HBoxToolbarTop.Append(button);
        }

        {
            Button button = Button.NewFromIconName("process-working");
            button.MarginEnd = 5;
            button.TooltipText = "Експорт";
            button.OnClicked += (_, _) => CreatePopoverMenu(button, SetExportMenu());
            HBoxToolbarTop.Append(button);
        }

        {
            Button button = Button.NewFromIconName("zoom-in");
            button.MarginEnd = 5;
            button.TooltipText = "Версії";
            button.OnClicked += OnVersionsHistory;
            HBoxToolbarTop.Append(button);
        }
    }

    async void OnVersionsHistory(Button button, EventArgs args)
    {
        await VersionsHistory(GetGetSelectionUnigueID());
    }

    #endregion
}
