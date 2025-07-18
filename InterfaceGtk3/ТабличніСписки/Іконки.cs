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

using Gdk;

namespace InterfaceGtk3.Іконки;

public static class ДляФорми
{
    public static string General = $"{AppContext.BaseDirectory}images/form.ico";
    public static string Configurator = $"{AppContext.BaseDirectory}images/configurator.ico";
}

public static class ДляКнопок
{
    public static Pixbuf Find = new Pixbuf($"{AppContext.BaseDirectory}images/find.png");
    public static Pixbuf Clean = new Pixbuf($"{AppContext.BaseDirectory}images/clean.png");
    public static Pixbuf Doc = new Pixbuf($"{AppContext.BaseDirectory}images/doc.png");
}

public static class ДляІнформування
{
    public static Pixbuf Ok = new Pixbuf($"{AppContext.BaseDirectory}images/16/ok.png");
    public static Pixbuf Error = new Pixbuf($"{AppContext.BaseDirectory}images/16/error.png");

    public static Pixbuf Info = new Pixbuf($"{AppContext.BaseDirectory}images/16/info.png");
    public static Pixbuf Lock = new Pixbuf($"{AppContext.BaseDirectory}images/16/lock.png");
    public static Pixbuf Key = new Pixbuf($"{AppContext.BaseDirectory}images/16/key.png");
    public static Pixbuf Synchronizing = new Pixbuf($"{AppContext.BaseDirectory}images/16/synchronizing.png");
}

public static class ДляІнформуванняВеликі
{
    public static Pixbuf Error = new Pixbuf($"{AppContext.BaseDirectory}images/error.png");
    public static Pixbuf Ok = new Pixbuf($"{AppContext.BaseDirectory}images/ok.png");
}

public static class ДляТабличногоСписку
{
    public static Pixbuf Normal = new Pixbuf($"{AppContext.BaseDirectory}images/doc.png");
    public static Pixbuf Delete = new Pixbuf($"{AppContext.BaseDirectory}images/doc_delete.png");
}

public static class ДляДерева
{
    public static Pixbuf Normal = new Pixbuf($"{AppContext.BaseDirectory}images/folder.png");
    public static Pixbuf Delete = new Pixbuf($"{AppContext.BaseDirectory}images/folder_delete.png");
}