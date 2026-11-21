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

/*



*/

using Gtk;
using Gdk;
using System.Text;

namespace InterfaceGtk4;

/// <summary>
/// Стандартний стиль для всієї програми
/// </summary>
public static class FunctionForStyle
{
    public static void LoadDefault()
    {
        Display? display = Display.GetDefault();
        if (display != null)
        {
            //Icon
            IconTheme iconTheme = IconTheme.GetForDisplay(display);
            iconTheme.AddSearchPath(Path.Combine(AppContext.BaseDirectory, "images"));

            //Css
            {
                CssProvider provider = CssProvider.New();
                using GLib.Bytes bytes = AssemblyResource.Load("Default.css");

                //Для старшої версії Gtk
                if (Gtk.Functions.CheckVersion(4, 12, 0) != null)
                {
                    string css = Encoding.UTF8.GetString(bytes.GetRegionSpan<byte>(0, bytes.GetSize()));
                    provider.LoadFromData(css, -1);
                }
                else
                {
                    //Метод для 4.12+
                    provider.LoadFromBytes(bytes);
                }

                StyleContext.AddProviderForDisplay(display, provider, Gtk.Constants.STYLE_PROVIDER_PRIORITY_APPLICATION);
            }
        }
    }
}