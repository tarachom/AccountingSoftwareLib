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

namespace InterfaceGtk4;

/// <summary>
/// 
/// </summary>
public class PointerTablePartCell : Box
{
    Box hBox;
    Entry entryPresentation;
    Button buttonSelect;
    Button buttonClear;

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
        hBox.Append(entryPresentation);

        //Select
        {
            buttonSelect = Button.New();
            buttonSelect.Child = Image.NewFromPixbuf(Icon.ForButton.Find);
            buttonSelect.OnClicked += Select;
            buttonSelect.AddCssClass("button");
            //buttonSelect.Visible = false;
            hBox.Append(buttonSelect);
        }

        //Clear
        {
            buttonClear = Button.New();
            buttonClear.Child = Image.NewFromPixbuf(Icon.ForButton.Clean);
            buttonClear.OnClicked += Clear;
            buttonClear.AddCssClass("button");
            //buttonClear.Visible = false;
            hBox.Append(buttonClear);
        }

        /*
        entryPresentation.OnNotify += (_, args) =>
        {
            string name = args.Pspec.GetName();
            Console.WriteLine(name);
            if (name == "has-focus")
            {
                //buttonSelect?.Visible = false;
                //buttonClear?.Visible = false;
            }

            if (name == "cursor-position")
            {
                buttonSelect?.Visible = true;
                buttonClear?.Visible = true;
            }
        };
        */
    }

    #region Virtual Function

    protected virtual void Select(Button button, EventArgs args) { }
    protected virtual void Clear(Button button, EventArgs args) { }

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
        get => entryPresentation.GetText();
        set
        {
            entryPresentation.SetText(value);
            entryPresentation.TooltipText = value;
        }
    }
}