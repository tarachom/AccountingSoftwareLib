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

Спільна форма - Рух документу по регістрах

*/

using Gtk;
using AccountingSoftware;

namespace InterfaceGtk4;

/// <summary>
/// Рух документу по регістрах
/// </summary>
public abstract class CommonForms_DocumentMovementThroughRegisters(NotebookFunction? notebookFunc) : Form(notebookFunc)
{
    /// <summary>
    /// Повертає віджет для відображення документу
    /// </summary>
    /// <param name="documentPointer">Вказівник на документ</param>
    /// <returns>Віджет</returns>
    protected abstract Widget Document_PointerControl(DocumentPointer documentPointer);

    /// <summary>
    /// Додає контрол віджет для відображення документу
    /// </summary>
    /// <param name="documentPointer">Вказівник на документ</param>
    protected void AddDocument(DocumentPointer documentPointer)
    {
        CreateField(this, null, Document_PointerControl(documentPointer), Align.Start);
    }

    /// <summary>
    /// Добавляє блок даних на форму
    /// </summary>
    /// <param name="name">Назва</param>
    /// <param name="getForm">Функція яка повертає віджет</param>
    protected virtual async ValueTask AddForm(string name, Func<bool, ValueTask<Widget>> getForm)
    {
        Box vBox = New(Orientation.Vertical, 0);
        vBox.MarginTop = vBox.MarginBottom = 10;
        vBox.MarginStart = vBox.MarginEnd = 20;

        Expander expander = Expander.New(name);
        expander.Expanded = false;
        expander.MarginTop = expander.MarginBottom = 5;
        expander.SetChild(vBox);

        //Add link
        {
            Box hBox = New(Orientation.Horizontal, 0);
            hBox.MarginBottom = 5;
            vBox.Append(hBox);

            //Відкрити в новій вкладці
            CreateLink(hBox, "Відкрити окремо", async () => NotebookFunc?.CreatePage(name, await getForm.Invoke(false)));
        }

        //Add form
        {
            Box hBox = New(Orientation.Horizontal, 0);
            hBox.Append(await getForm.Invoke(true));
            vBox.Append(hBox);
        }

        Append(expander);
    }
}