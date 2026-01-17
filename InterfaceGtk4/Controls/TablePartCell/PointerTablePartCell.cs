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
using Gdk;

namespace InterfaceGtk4;

/// <summary>
/// Клітинка табличної частини - Вказівник
/// </summary>
public abstract class PointerTablePartCell : Box
{
    Box hBox;
    Label label = Label.New(null);
    Button buttonSelect;

    public PointerTablePartCell()
    {
        SetOrientation(Orientation.Vertical);

        hBox = New(Orientation.Horizontal, 0);
        hBox.Hexpand = hBox.Vexpand = true;

        label.Halign = Align.Start;
        label.AddCssClass("text");

        ScrolledWindow scroll = ScrolledWindow.New();
        scroll.SetPolicy(PolicyType.External, PolicyType.Never);
        scroll.Hexpand = true;
        scroll.SetChild(label);
        hBox.Append(scroll);

        /*
        GestureClick gesture = GestureClick.New();
        entryPresentation.AddController(gesture);
        gesture.OnPressed += (_, args) =>
        {
            if (buttonSelect != null && args.NPress >= 2)
                Select(buttonSelect, new());
        };
        */

        /*EventControllerKey contrKey = EventControllerKey.New();
        entryPresentation.AddController(contrKey);
        contrKey.OnKeyReleased += (_, args) =>
        {
            if (buttonSelect != null && (args.Keyval == (uint)Key.KP_Enter || args.Keyval == (uint)Key.Return))
                Select(buttonSelect, new());
            else if (args.Keyval == (uint)Key.Delete)
                Clear();
        };*/

        //Select
        {
            buttonSelect = Button.New();
            buttonSelect.Child = Image.NewFromPixbuf(Icon.ForInformation.Grid);
            buttonSelect.OnClicked += Select;
            buttonSelect.TooltipText = "Вибрати";
            buttonSelect.AddCssClass("button");
            hBox.Append(buttonSelect);
        }

        Append(hBox);
        AddCssClass("pointer");
    }

    #region Virtual Function

    protected abstract void Select(Button button, EventArgs args);
    protected abstract void Clear();

    #endregion

    /// <summary>
    /// Функція яка викликається після вибору
    /// </summary>
    public Action? OnSelect { get; set; }

    /// <summary>
    /// Відображення
    /// </summary>
    protected string Presentation
    {
        get => label.GetText();
        set
        {
            label.SetText(value);
            label.TooltipText = value;
        }
    }
}