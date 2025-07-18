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

/*

Форма для перегляду великого тексту в табличній частині

*/

using Gtk;

namespace InterfaceGtk3;

public class СпільніФорми_РедагуванняТексту : ФормаСпливаючеВікно
{
    ScrolledWindow scroll = new ScrolledWindow() { ShadowType = ShadowType.In, WidthRequest = 600, HeightRequest = 400 };
    TextView text = new TextView() { WrapMode = WrapMode.Word };
    public System.Action<string>? CallBack_Update = null;

    public СпільніФорми_РедагуванняТексту()
    {
        //Текст
        {
            Box hBox = new Box(Orientation.Horizontal, 0);
            PackStart(hBox, true, true, 0);

            scroll.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
            scroll.Add(text);

            hBox.PackStart(scroll, true, true, 0);
        }

        //Кнопка
        {
            Box hBox = new Box(Orientation.Horizontal, 0);
            PackStart(hBox, false, false, 2);

            Button bSave = new Button("Зберегти");
            bSave.Clicked += (sender, args) =>
            {
                CallBack_Update?.Invoke(this.Text);
                PopoverParent?.Hide();
            };
            hBox.PackStart(bSave, false, false, 0);
        }
    }

    public string Text
    {
        get
        {
            return text.Buffer.Text;
        }
        set
        {
            text.Buffer.Text = value;
        }
    }

    public WrapMode Wrap
    {
        set
        {
            text.WrapMode = value;
        }
    }

    public int Height
    {
        set
        {
            scroll.HeightRequest = value;
        }
    }

    public int Width
    {
        set
        {
            scroll.WidthRequest = value;
        }
    }
}