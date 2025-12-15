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

using GdkPixbuf;
using Gtk;

namespace InterfaceGtk4.Icon;

public static class ForForm
{
    public static string General = $"{AppContext.BaseDirectory}images/form.ico";
    public static string Configurator = $"{AppContext.BaseDirectory}images/configurator.ico";
}

public static class ForButton
{
    public static Pixbuf? Find = Pixbuf.NewFromFile($"{AppContext.BaseDirectory}images/find.png");
    public static Pixbuf? Clean = Pixbuf.NewFromFile($"{AppContext.BaseDirectory}images/clean.png");
    public static Pixbuf? Doc = Pixbuf.NewFromFile($"{AppContext.BaseDirectory}images/doc.png");
}

public static class ForInformation
{
    public static Pixbuf? Ok = Pixbuf.NewFromFile($"{AppContext.BaseDirectory}images/16/ok.png");
    public static Pixbuf? Error = Pixbuf.NewFromFile($"{AppContext.BaseDirectory}images/16/error.png");

    public static Pixbuf? Info = Pixbuf.NewFromFile($"{AppContext.BaseDirectory}images/16/info.png");
    public static Pixbuf? Lock = Pixbuf.NewFromFile($"{AppContext.BaseDirectory}images/16/lock.png");
    public static Pixbuf? Key = Pixbuf.NewFromFile($"{AppContext.BaseDirectory}images/16/key.png");
    public static Pixbuf? Synchronizing = Pixbuf.NewFromFile($"{AppContext.BaseDirectory}images/16/synchronizing.png");
}

public static class ForInformationBig
{
    public static Pixbuf? Error = Pixbuf.NewFromFile($"{AppContext.BaseDirectory}images/error.png");
    public static Pixbuf? Ok = Pixbuf.NewFromFile($"{AppContext.BaseDirectory}images/ok.png");
}

public static class ForTabularLists
{
    public static Pixbuf? Normal = Pixbuf.NewFromFile($"{AppContext.BaseDirectory}images/doc.png");
    public static Pixbuf? Delete = Pixbuf.NewFromFile($"{AppContext.BaseDirectory}images/doc_delete.png");
}

public static class ForTree
{
    public static Pixbuf? Normal = Pixbuf.NewFromFile($"{AppContext.BaseDirectory}images/folder.png");
    public static Pixbuf? Delete = Pixbuf.NewFromFile($"{AppContext.BaseDirectory}images/folder_delete.png");
}