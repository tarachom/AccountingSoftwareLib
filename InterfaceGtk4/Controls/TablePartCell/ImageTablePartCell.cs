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
[GObject.Subclass<Box>]
public partial class ImageTablePartCell : Box
{
    Box hBox = New(Orientation.Horizontal, 0);
    Image img = Image.NewFromPixbuf(null);

    partial void Initialize()
    {
        SetOrientation(Orientation.Vertical);

        hBox.Valign = hBox.Halign = Align.Center;
        hBox.Vexpand = true;
        hBox.Append(img);

        Append(hBox);
        AddCssClass("base");
    }

    public static ImageTablePartCell New() => NewWithProperties([]);

    public static ImageTablePartCell NewFromPixbuf(Pixbuf? pixbuf)
    {
        ImageTablePartCell img = NewWithProperties([]);
        img.SetImage(pixbuf);

        return img;
    }

    public void SetImage(Pixbuf? pixbuf)
    {
        img.SetFromPixbuf(pixbuf);
    }

    /// <summary>
    /// Видаляє із hBox віджет img і замість нього ставить Spinner.
    /// 
    /// Використовується для вітки дерева в яку повинні завантажитись дані.
    /// Після завантаження даних, ця вітка видаляється
    /// </summary>
    public void SetSpinner()
    {
        Widget? firstChild = hBox.GetFirstChild();
        if (firstChild != null) hBox.Remove(firstChild);

        Spinner spinner = Spinner.New();
        spinner.Spinning = true;

        hBox.Append(spinner);
    }
}