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

using GdkPixbuf;
using Gtk;

namespace InterfaceGtk4;

/// <summary>
/// Клітинка табличної частини - Іконка
/// </summary>
public class ImageTablePartCell : Box
{
    Box hBox;
    Image img = Image.NewFromPixbuf(null);

    public ImageTablePartCell()
    {
        SetOrientation(Orientation.Vertical);

        hBox = New(Orientation.Horizontal, 0);
        hBox.Valign = hBox.Halign = Align.Center;
        hBox.Vexpand = true;
        hBox.Append(img);

        Append(hBox);
        AddCssClass("base");
    }

    public void SetImage(Pixbuf? pixbuf)
    {
        img.SetFromPixbuf(pixbuf);
    }

    public static ImageTablePartCell NewForPixbuf(Pixbuf? pixbuf)
    {
        ImageTablePartCell img = new();
        img.SetImage(pixbuf);

        return img;
    }
}