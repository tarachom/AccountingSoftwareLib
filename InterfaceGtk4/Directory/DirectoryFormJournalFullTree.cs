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
/// ДовідникЖурналПовнийДерево
/// 
/// Журнал довідників певного виду (Повний)
/// </summary>
public abstract class DirectoryFormJournalFullTree : DirectoryFormJournalBaseTree
{
    public DirectoryFormJournalFullTree(NotebookFunction? notebookFunc) : base(notebookFunc)
    {
        AddToolbar();
    }

    #region Virtual & Abstract Function

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
