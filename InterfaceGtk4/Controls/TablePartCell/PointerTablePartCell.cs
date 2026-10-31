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
/// Клітинка табличної частини - Вказівник
/// </summary>
[GObject.Subclass<Box>]
public abstract partial class PointerTablePartCell : Box
{
    protected Box hBox = New(Orientation.Horizontal, 0);
    Label label = Label.New(null);
    Button buttonSelect = Button.New();
    Button buttonClear = Button.New();

    partial void Initialize()
    {
        SetOrientation(Orientation.Vertical);

        hBox.Hexpand = hBox.Vexpand = true;

        label.Halign = Align.Start;
        label.AddCssClass("text");

        ScrolledWindow scroll = ScrolledWindow.New();
        scroll.SetPolicy(PolicyType.External, PolicyType.Never);
        scroll.Hexpand = true;
        scroll.SetChild(label);
        hBox.Append(scroll);

        //Select
        {
            buttonSelect.Child = Image.NewFromPixbuf(Icon.ForButton.Find);
            buttonSelect.OnClicked += Select;
            buttonSelect.TooltipText = "Вибрати";
            buttonSelect.AddCssClass("button");
            hBox.Append(buttonSelect);
        }

        Append(hBox);
        AddCssClass("pointer");

        //Контекстне меню
        {
            Gio.SimpleActionGroup actionGroup = Gio.SimpleActionGroup.New();

            {
                var action = Gio.SimpleAction.New("select", null);
                action.OnActivate += (_, _) => Select(buttonSelect, new());
                actionGroup.AddAction(action);
            }

            {
                var action = Gio.SimpleAction.New("clear", null);
                action.OnActivate += (_, _) => Clear();
                actionGroup.AddAction(action);
            }

            InsertActionGroup("control", actionGroup); // Реєструємо групу дій

            Gio.Menu menuModel = Gio.Menu.New();
            menuModel.Append("Вибрати", "control.select");
            menuModel.Append("Очистити", "control.clear");

            PopoverMenu popover = PopoverMenu.NewFromModel(menuModel);
            popover.SetParent(this);

            GestureClick gesture = GestureClick.New();
            gesture.SetButton(Gdk.Constants.BUTTON_SECONDARY); // Права кнопка миші
            gesture.OnPressed += (g, args) =>
            {
                var rect = new Gdk.Rectangle
                {
                    X = (int)args.X,
                    Y = (int)args.Y,
                    Width = 1,
                    Height = 1
                };

                popover.SetPointingTo(rect);
                popover.Popup();
            };

            AddController(gesture);
        }
    }

    #region Virtual Function

    protected virtual void Select(Button button, EventArgs args) { }
    protected virtual void Clear() { }

    #endregion

    /// <summary>
    /// Функція яка викликається після вибору
    /// </summary>
    public Action? OnSelect { get; set; } = null;

    /// <summary>
    /// Функція яка викликається перед відкриттям вибору
    /// </summary>
    public Action? BeforeClickOpenFunc { get; set; } = null;

    /// <summary>
    /// Функція яка викликається після вибору.
    /// </summary>
    public Action? AfterSelectFunc { get; set; } = null;

    /// <summary>
    /// Функція яка викликається після очищення.
    /// </summary>
    public Action? AfterClearFunc { get; set; } = null;

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