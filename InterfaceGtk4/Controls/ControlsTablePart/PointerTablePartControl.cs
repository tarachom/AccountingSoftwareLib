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

public class PointerTablePartControl : Box
{
    Box hBox;
    Entry entryText;

    Button buttonSelect;
    Button buttonClear;

    public PointerTablePartControl()
    {
        SetOrientation(Orientation.Vertical);

        hBox = New(Orientation.Horizontal, 0);
        hBox.Hexpand = hBox.Vexpand = true;

        Append(hBox);
        AddCssClass("pointer");

        entryText = Entry.New();
        entryText.Editable = false;
        entryText.Hexpand = true;
        entryText.AddCssClass("entry");

        /*entryText.OnNotify += (_, args) =>
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
        };*/

        hBox.Append(entryText);

        //Select
        {
            buttonSelect = Button.New();
            buttonSelect.Child = Image.NewFromPixbuf(Icon.ForButton.Find);
            buttonSelect.OnClicked += OpenSelect;
            buttonSelect.AddCssClass("button");
            //buttonSelect.Visible = false;
            hBox.Append(buttonSelect);
        }

        //Clear
        {
            buttonClear = Button.New();
            buttonClear.Child = Image.NewFromPixbuf(Icon.ForButton.Clean);
            buttonClear.OnClicked += OnClear;
            buttonClear.AddCssClass("button");
            //buttonClear.Visible = false;
            hBox.Append(buttonClear);
        }
    }

    #region Virtual Function

    protected virtual void OpenSelect(Button button, EventArgs args) { }
    protected virtual void OnClear(Button button, EventArgs args) { }

    #endregion

    /// <summary>
    /// Функція яка викликається перед відкриттям вибору
    /// </summary>
    public Action? BeforeClickOpenFunc { get; set; }

    /// <summary>
    /// Функція яка викликається після вибору.
    /// </summary>
    public Action? AfterSelectFunc { get; set; }

    /// <summary>
    /// Функція яка викликається після очищення.
    /// </summary>
    public Action? AfterClearFunc { get; set; }

    /// <summary>
    /// 
    /// </summary>
    protected string Presentation
    {
        get => entryText.GetText();
        set
        {
            entryText.SetText(value);
            entryText.TooltipText = value;
        }
    }
}