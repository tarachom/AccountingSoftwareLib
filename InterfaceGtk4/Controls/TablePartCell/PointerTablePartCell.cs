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
/// 
/// </summary>
public abstract class PointerTablePartCell : Box
{
    Box hBox;
    Entry entryPresentation;
    Button buttonSelect;

    public PointerTablePartCell()
    {
        SetOrientation(Orientation.Vertical);

        hBox = New(Orientation.Horizontal, 0);
        hBox.Hexpand = hBox.Vexpand = true;

        Append(hBox);
        AddCssClass("pointer");

        entryPresentation = Entry.New();
        entryPresentation.Editable = false;
        entryPresentation.Hexpand = true;
        entryPresentation.AddCssClass("entry");

        /*GestureClick gesture = GestureClick.New();
        entryPresentation.AddController(gesture);
        gesture.OnPressed += (_, args) =>
        {
            if (args.NPress >= 1)
                OnActivate?.Invoke();
            //else if (buttonSelect != null && args.NPress >= 2)
                //Select(buttonSelect, new());
        };*/

        EventControllerKey contrKey = EventControllerKey.New();
        entryPresentation.AddController(contrKey);
        contrKey.OnKeyReleased += (_, args) =>
        {
            if (buttonSelect != null && (args.Keyval == (uint)Key.KP_Enter || args.Keyval == (uint)Key.Return))
                Select(buttonSelect, new());
            else if (args.Keyval == (uint)Key.Delete)
                Clear();
        };

        hBox.Append(entryPresentation);

        //Select
        {
            buttonSelect = Button.New();
            buttonSelect.Child = Image.NewFromPixbuf(Icon.ForInformation.Grid);
            buttonSelect.OnClicked += (_, _) => OnActivate?.Invoke();
            buttonSelect.OnClicked += Select;
            buttonSelect.TooltipText = "Вибрати";
            buttonSelect.AddCssClass("button");
            hBox.Append(buttonSelect);
        }

        //Clear
        /*{
            buttonClear = Button.New();
            buttonClear.Child = Image.NewFromPixbuf(Icon.ForButton.Clean);
            buttonClear.OnClicked += Clear;
            buttonClear.AddCssClass("button");
            //buttonClear.Visible = false;
            hBox.Append(buttonClear);
        }*/

        /*entryPresentation.OnNotify += (_, args) =>
        {
            if (args.Pspec.GetName() == "cursor-position")
                OnActivate?.Invoke();
        };*/
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
    /// 
    /// </summary>
    public Action? OnActivate { get; set; }

    /// <summary>
    /// Відображення
    /// </summary>
    protected string Presentation
    {
        get => entryPresentation.GetText();
        set
        {
            entryPresentation.SetText(value);
            entryPresentation.TooltipText = value;
        }
    }
}