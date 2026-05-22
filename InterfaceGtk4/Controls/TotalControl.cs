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

TotalControl - контрол для виводу загальних підсумків в таб частинах документів

*/

using AccountingSoftware;
using Gtk;
using System.Globalization;

namespace InterfaceGtk4;

[GObject.Subclass<Box>]
public partial class TotalControl : Box
{
    event EventHandler<Model>? OnChanged;
    Label total = Label.New(null);
    NumberFormatInfo numberFormatUA = new CultureInfo("uk-UA", false).NumberFormat;

    partial void Initialize()
    {
        SetOrientation(Orientation.Horizontal);

        numberFormatUA.NumberGroupSeparator = " ";
        numberFormatUA.NumberDecimalDigits = 2;
        Console.WriteLine("Init");

        Label caption = Label.New("<b>Підсумки:</b> ");
        caption.UseMarkup = true;

        Append(caption);

        total.UseMarkup = true;
        Append(total);
    }

    /// <summary>
    /// Функція обчислення
    /// </summary>
    public Func<Model>? QuantifyFunc { get; set; } = null;

    /// <summary>
    /// Перерахувати
    /// </summary>
    public void Recount() => OnChanged?.Invoke(null, QuantifyFunc?.Invoke() ?? new Model());

    public static TotalControl New()
    {
        TotalControl control = NewWithProperties([]);
        control.OnChanged += (_, model) => control.total.SetMarkup(model.Format(control.numberFormatUA));

        return control;
    }

    public class Model
    {
        public string Template { get; set; } = "";
        public object[] Values { get; set; } = [];

        public Model() { }

        public Model(string template, params object[] values)
        {
            Template = template;
            Values = values;
        }

        public string Format(NumberFormatInfo provider)
        {
            //Копіювання в інший масив і одночасне форматування в залежності від типу
            object[] copy = new object[Values.Length];
            for (int i = 0; i < Values.Length; i++)
            {
                object value = Values[i];
                copy[i] = value switch
                {
                    int v => v.ToString("N", provider),
                    decimal v => v.ToString("N", provider),
                    float v => v.ToString("N", provider),
                    _ => value,
                };
            }

            return string.Format(Template, copy);
        }
    }
}