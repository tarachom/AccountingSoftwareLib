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

Діалогове повідомлення

*/

using Gtk;

namespace InterfaceGtk4;

public class Message
{
    /// <summary>
    /// Ознака того, що поточна версія GTK є старішою за 4.10.0
    /// </summary>
    private static readonly bool IsLegacyGtk = Functions.CheckVersion(4, 10, 0) != null;

    #region Info

    /// <summary>
    /// Інформаційне повідомлення
    /// </summary>
    public static async Task Info(Window? win, string text, string? secondaryText = null)
    {
        if (IsLegacyGtk)
            await ShowLegacyDialog(win, text, secondaryText);
        else
            await ShowModernDialog(win, text, secondaryText);
    }

    #endregion

    #region Request (Так / Ні)

    /// <summary>
    /// Повідомлення запит Так/Ні
    /// </summary>
    public static async Task<YesNo> Request(Window? win, string text, string? secondaryText = null) =>
        IsLegacyGtk ? await RequestLegacy(win, text, secondaryText) : await RequestModern(win, text, secondaryText);

    #endregion

    #region Реалізація для версій нижче 4.10 (MessageDialog)

    static MessageDialog CreateLegacy(Window? win, string text, string? secondaryText = null)
    {
        MessageDialog md = MessageDialog.NewWithProperties([]);
        md.Application = win?.Application;
        md.TransientFor = win;
        md.Modal = true;
        md.Valign = Align.Center;
        md.Halign = Align.Center;
        md.Text = text;
        md.SecondaryText = secondaryText;

        return md;
    }

    private static Task<bool> ShowLegacyDialog(Window? win, string text, string? secondaryText)
    {
        var tcs = new TaskCompletionSource<bool>();
        MessageDialog message = CreateLegacy(win, text, secondaryText);
        message.AddButton("Закрити", 1);

        message.OnResponse += (_, _) =>
        {
            message.Hide();
            message.Destroy();
            tcs.TrySetResult(true);
        };

        message.Show();
        return tcs.Task;
    }

    static Task<YesNo> RequestLegacy(Window? win, string text, string? secondaryText)
    {
        var tcs = new TaskCompletionSource<YesNo>();
        MessageDialog message = CreateLegacy(win, text, secondaryText);
        Button buttonYes = (Button)message.AddButton("Так", (int)YesNo.Yes);
        message.AddButton("Ні", (int)YesNo.No);

        message.OnResponse += (_, arrg) =>
        {
            var res = (YesNo)arrg.ResponseId;
            message.Hide();
            message.Destroy();
            tcs.TrySetResult(res);
        };

        message.Show();
        buttonYes.GrabFocus();

        return tcs.Task;
    }

    #endregion

    #region Реалізація для версій 4.10+ (AlertDialog)

    static async Task<int> ShowModernDialog(Window? win, string text, string? secondaryText)
    {
        AlertDialog dialog = AlertDialog.NewWithProperties([]);
        dialog.SetMessage(text);

        if (!string.IsNullOrEmpty(secondaryText))
            dialog.SetDetail(secondaryText);

        dialog.SetButtons(["Закрити"]);
        dialog.SetDefaultButton(0);
        dialog.SetCancelButton(0);

        try
        {
            return await dialog.ChooseAsync(win);
        }
        catch
        {
            return 0;
        }
    }

    static async Task<YesNo> RequestModern(Window? win, string text, string? secondaryText)
    {
        AlertDialog dialog = AlertDialog.NewWithProperties([]);
        dialog.SetMessage(text);

        if (!string.IsNullOrEmpty(secondaryText))
            dialog.SetDetail(secondaryText);

        dialog.SetButtons(["Так", "Ні"]);
        dialog.SetDefaultButton(0);
        dialog.SetCancelButton(1);

        try
        {
            return await dialog.ChooseAsync(win) == 0 ? YesNo.Yes : YesNo.No;
        }
        catch
        {
            return YesNo.No;
        }
    }

    #endregion

    public enum YesNo
    {
        Yes = 1,
        No = 2
    }
}