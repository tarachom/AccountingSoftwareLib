

using Gtk;
using AccountingSoftware;

namespace InterfaceGtk
{
    public abstract class ТабличнийСписок
    {
        public static void ДодатиВідбір(TreeView treeView, Where where, bool clear_all_before_add = false)
        {
            if (!treeView.Data.ContainsKey("Where"))
                treeView.Data.Add("Where", new List<Where>() { where });
            else
            {
                if (clear_all_before_add)
                    treeView.Data["Where"] = new List<Where>() { where };
                else
                {
                    object? value = treeView.Data["Where"];
                    if (value == null)
                        treeView.Data["Where"] = new List<Where>() { where };
                    else
                        ((List<Where>)value).Add(where);
                }
            }
        }

        public static void ДодатиВідбір(TreeView treeView, List<Where> where, bool clear_all_before_add = false)
        {
            if (!treeView.Data.ContainsKey("Where"))
                treeView.Data.Add("Where", where);
            else
            {
                if (clear_all_before_add)
                    treeView.Data["Where"] = where;
                else
                {
                    object? value = treeView.Data["Where"];
                    if (value == null)
                        treeView.Data["Where"] = where;
                    else
                    {
                        var list = (List<Where>)value;
                        foreach (Where item in where)
                            list.Add(item);
                    }
                }
            }
        }

        public static void ОчиститиВідбір(TreeView treeView)
        {
            treeView.Data["Where"] = null;
        }

        /// <summary>
        /// Додати у список фільтру новий елемент
        /// </summary>
        /// <param name="listBox">Список</param>
        /// <param name="caption">Заголовок</param>
        /// <param name="widget">Віджет</param>
        /// <param name="sw">Переключатель</param>
        public static void ДодатиЕлементВФільтр(ListBox listBox, string caption, Widget widget, Switch sw)
        {
            Box vBox = new Box(Orientation.Vertical, 0);
            Box hBox = new Box(Orientation.Horizontal, 0);
            vBox.PackStart(hBox, false, false, 5);
            hBox.PackStart(new Label(caption), false, false, 5);

            Box vBoxSw = new Box(Orientation.Vertical, 0) { Valign = Align.Center };
            hBox.PackEnd(vBoxSw, false, false, 0);
            Box hBoxSw = new Box(Orientation.Horizontal, 0) { Halign = Align.Center };
            vBoxSw.PackStart(hBoxSw, false, false, 0);
            hBoxSw.PackStart(sw, false, false, 5);

            hBox.PackEnd(widget, false, false, 5);

            listBox.Add(new ListBoxRow() { vBox });
        }

        #region Сторінки

        public static void Сторінки(TreeView treeView, Сторінки.Налаштування settings)
        {
            if (!treeView.Data.ContainsKey("Pages"))
                treeView.Data.Add("Pages", settings);
            else
                treeView.Data["Pages"] = settings;
        }

        public static Сторінки.Налаштування? ОтриматиСторінки(TreeView treeView)
        {
            var pages = treeView.Data["Pages"];
            return pages != null ? (Сторінки.Налаштування)pages : null;
        }

        public static void ОчиститиСторінки(TreeView treeView)
        {
            var pages = treeView.Data["Pages"];
            if (pages != null) ((Сторінки.Налаштування)pages).Clear();
        }

        public static async ValueTask ЗаповнитиСторінки(Func<UnigueID?, int, ValueTask<SplitSelectToPages_Record>> splitSelectToPagesFunc,
            Сторінки.Налаштування settings, Query querySelect, UnigueID? unigueID)
        {
            if (!settings.Calculated)
            {
                settings.Record = await splitSelectToPagesFunc.Invoke(unigueID, settings.PageSize);
                settings.Calculated = true;

                if (unigueID != null && settings.Record.CurrentPage > 0)
                    settings.CurrentPage = settings.Record.CurrentPage;
                //Для журналів документів при відкритті ставиться остання сторінка
                else if (settings.Тип == InterfaceGtk.Сторінки.ТипЖурналу.Документи)
                    settings.CurrentPage = settings.Record.Pages;
            }

            if (settings.Calculated && settings.Record.Result)
            {
                querySelect.Limit = settings.Record.PageSize;
                querySelect.Offset = settings.Record.PageSize * (settings.CurrentPage - 1);
            }
        }

        #endregion
    }
}