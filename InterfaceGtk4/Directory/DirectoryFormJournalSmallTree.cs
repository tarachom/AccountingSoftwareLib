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
/// ДовідникЖурналМініДерево
/// 
/// Журнал довідників певного виду (Міні)
/// </summary>
public abstract class DirectoryFormJournalSmallTree : DirectoryFormJournalBaseTree
{
    public DirectoryFormJournalSmallTree(NotebookFunction? notebookFunc) : base(notebookFunc)
    {
        AddToolbar();
    }

    #region Virtual & Abstract Function

    protected abstract ValueTask OpenPageList(UnigueID? unigueID = null);

    #endregion

    #region Grid

    #endregion

    #region Toolbar

    /// <summary>
    /// Доповнити набір кнопок
    /// </summary>
    void AddToolbar()
    {
        {
            //Відкрити в блокноті
            {
                Button button = Button.NewFromIconName("go-up");
                button.MarginEnd = 5;
                button.TooltipText = "Відкрити";
                button.OnClicked += OnOpenPageList;
                HBoxToolbarTop.Append(button);
            }
        }
    }

    async void OnOpenPageList(Button button, EventArgs args)
    {
        await OpenPageList(SelectPointerItem);
    }

    #endregion
}
