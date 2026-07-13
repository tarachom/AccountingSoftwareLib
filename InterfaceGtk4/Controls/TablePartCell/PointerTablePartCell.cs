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

        /*
        GestureClick gesture = GestureClick.New();
        entryPresentation.AddController(gesture);
        gesture.OnPressed += (_, args) =>
        {
            if (buttonSelect != null && args.NPress >= 2)
                Select(buttonSelect, new());
        };

        EventControllerKey contrKey = EventControllerKey.New();
        entryPresentation.AddController(contrKey);
        contrKey.OnKeyReleased += (_, args) =>
        {
            if (buttonSelect != null && (args.Keyval == (uint)Key.KP_Enter || args.Keyval == (uint)Key.Return))
                Select(buttonSelect, new());
            else if (args.Keyval == (uint)Key.Delete)
                Clear();
        };
        */

        {
            /*
            // 1. Створюємо звичайний Popover (не PopoverMenu)
            var popover = Popover.New();
            popover.SetParent(hBox);

            // 2. Створюємо контейнер для "пунктів меню"
            var box =  Box.New(Orientation.Vertical, 0);
            popover.SetChild(box);

            // 3. Створюємо "пункт меню" як звичайну кнопку
            var clearButton = Button.NewWithLabel("Очистити");
            clearButton.HasFrame = false; // Робимо її плоскою, щоб була схожа на пункт меню

            // Прямий обробник події кліку
            clearButton.OnClicked += (s, e) =>
            {
                label.SetText(""); // Очищаємо текст
                popover.Popdown(); // Закриваємо меню після натискання
            };

            box.Append(clearButton);

            // 4. Додаємо жест для виклику меню правою кнопкою
            GestureClick gesture = GestureClick.New();
            gesture.Button = 3;
            gesture.OnPressed += (s, e) =>
            {
                var rect = new Gdk.Rectangle
                {
                    X = (int)e.X,
                    Y = (int)e.Y,
                    Width = 1,
                    Height = 1
                };
                popover.SetPointingTo(rect);
                popover.Popup();
            };

            hBox.AddController(gesture);
*/

            /*
            // 1. Створюємо модель меню
            var menuModel = Gio.Menu.New();
            menuModel.Append("Очистити поле", "app.clear_text");

            // 2. Створюємо PopoverMenu на основі моделі
            var popover = PopoverMenu.NewFromModel(menuModel);
            popover.SetParent(label); // Прив'язуємо до поля
            popover.HasArrow = true;

            GestureClick gesture = GestureClick.New();
            gesture.Button = 3; // 1 - ліва, 2 - середня, 3 - права

            // 4. Обробка натискання
            gesture.OnPressed += (s, e) =>
            {
                // Встановлюємо координати, де з'явиться меню (там, де клікнули)
                var rect = new Gdk.Rectangle
                {
                    X = (int)e.X,
                    Y = (int)e.Y,
                    Width = 1,
                    Height = 1
                };

                Console.WriteLine(rect);

                popover.SetPointingTo(rect);
                popover.Popup();
            };

            hBox.AddController(gesture);

            // 5. Додаємо логіку для дії
            var clearAction = SimpleAction.New("clear_text", null);
            clearAction.OnActivate += (s, e) =>
            {
                entry.Text = "";
            };
            application.AddAction(clearAction);
            */
        }

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