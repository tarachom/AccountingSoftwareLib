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

Діалогове повідомлення

*/

using Gtk;

namespace InterfaceGtk4;

public class Message
{
    static MessageDialog Create(Application? app, Window? win, string text, string secondaryText = "")
    {
        return new()
        {
            TransientFor = win,
            Application = app,
            Modal = true,
            Valign = Align.Center,
            Halign = Align.Center,
            Text = text,
            SecondaryText = secondaryText
        };
    }

    public static void Info(Application? app, Window? win, string text, string secondaryText = "")
    {
        MessageDialog message = Create(app, win, text, secondaryText);
        message.AddButton("Закрити", 1);

        message.OnResponse += (_, arrg) =>
        {
            message.Hide();
            message.Destroy();
        };

        message.Show();
    }

    public static void Error(Application? app, Window? win, string text, string secondaryText = "")
    {
        MessageDialog message = Create(app, win, text, secondaryText);
        message.AddButton("Закрити", 1);

        message.OnResponse += (_, arrg) =>
        {
            message.Hide();
            message.Destroy();
        };

        message.Show();
    }

    /// <summary>
    /// Повідомлення запит Так/Ні
    /// </summary>
    /// <param name="pwin">Вікно власник</param>
    /// <param name="message">Текст</param>
    /// <returns>Так або Ні</returns>
    public static void Request(Application? app, Window? win, string text, string secondaryText = "", Action<int>? callBackResponse = null)
    {
        MessageDialog message = Create(app, win, text, secondaryText);
        message.AddButton("Так", 1);
        message.AddButton("Ні", 2);

        message.OnResponse += (_, arrg) =>
        {
            callBackResponse?.Invoke(arrg.ResponseId);

            message.Hide();
            message.Destroy();
        };

        message.Show();
    }
}