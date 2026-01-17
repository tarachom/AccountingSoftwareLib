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
    protected abstract Widget Document_PointerControl(DocumentPointer documentPointer);

    protected void AddDocumentToForm(DocumentPointer documentPointer)
    {
        CreateField(this, null, Document_PointerControl(documentPointer));
    }

    /*protected virtual void ДодатиБлокНаФорму(string blockName, TreeView treeView)
    {
        Box vBox = new Box(Orientation.Vertical, 0);

        Expander expander = new Expander(blockName) { Expanded = true };
        expander.Add(vBox);

        ScrolledWindow scrollTree = new ScrolledWindow();
        scrollTree.SetPolicy(PolicyType.Automatic, PolicyType.Never);
        scrollTree.Add(treeView);

        Box hBox = new Box(Orientation.Horizontal, 0);
        hBox.PackStart(scrollTree, true, true, 5);
        vBox.PackStart(hBox, false, false, 5);

        PackStart(expander, false, false, 10);

        ShowAll();
    }*/

    protected virtual void ДодатиБлокНаФорму(string blockName, Widget form)
    {
        Box vBox = New(Orientation.Vertical, 0);

        Expander expander = Expander.New(blockName);
        expander.Expanded = true;
        expander.SetChild(vBox);

        Box hBox = New(Orientation.Horizontal, 0);
        hBox.Append(form);
        vBox.Append(hBox);

        Append(expander);
    }
}