/*
Copyright (C) 2019-2025 TARAKHOMYN YURIY IVANOVYCH
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

namespace InterfaceGtk3;

public class ComboBoxTextControl : Box
{
    ComboBoxText comboBox = new ComboBoxText() { Sensitive = false };
    ToggleButton bToggle = new ToggleButton() { Image = new Image(Іконки.ДляІнформування.Key) };

    public ComboBoxTextControl() : base(Orientation.Horizontal, 0)
    {
        comboBox.ButtonPressEvent += (sender, arrg) =>
        {
            if (arrg.Event.Button == 3)
                PopUpContextMenu().Popup();
        };
        PackStart(comboBox, false, false, 1);

        bToggle.Clicked += (sender, arrg) => comboBox.Sensitive = bToggle.Active;
        PackStart(bToggle, false, false, 1);
    }

    public void Append(string id, string text)
    {
        comboBox.Append(id, text);
    }

    public string ActiveId
    {
        get
        {
            return comboBox.ActiveId;
        }
        set
        {
            comboBox.ActiveId = value;
            comboBox.TooltipText = comboBox.ActiveText;
        }
    }

    Menu PopUpContextMenu()
    {
        Menu menu = new Menu();

        {
            MenuItem item = new MenuItem("Очистити");
            item.Activated += (sender, arrg) => comboBox.Active = -1;
            menu.Append(item);
        }

        menu.ShowAll();

        return menu;
    }
}