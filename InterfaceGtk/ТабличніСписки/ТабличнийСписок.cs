

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

        /// <summary>
        /// Для довідників, документів та регістрів
        /// </summary>
        /// <param name="splitSelectToPagesFunc"></param>
        /// <param name="settings"></param>
        /// <param name="querySelect"></param>
        /// <param name="unigueID"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Для журналів
        /// </summary>
        /// <param name="splitSelectToPagesForJournalFunc"></param>
        /// <param name="settings"></param>
        /// <param name="query"></param>
        /// <param name="paramQuery"></param>
        /// <returns></returns>
        public static async ValueTask<string> ЗаповнитиСторінки(Func<string, Dictionary<string, object>, int, ValueTask<SplitSelectToPages_Record>> splitSelectToPagesForJournalFunc,
            Сторінки.Налаштування settings, string query, Dictionary<string, object> paramQuery)
        {
            if (!settings.Calculated)
            {
                settings.Record = await splitSelectToPagesForJournalFunc.Invoke(query, paramQuery, settings.PageSize);
                settings.Calculated = true;

                settings.CurrentPage = settings.Record.Pages;
            }

            if (settings.Calculated && settings.Record.Result && settings.CurrentPage > 0)
            {
                string Limit = $"\n\nLIMIT {settings.Record.PageSize}\n";
                string Offset = $"OFFSET {settings.Record.PageSize * (settings.CurrentPage - 1)}\n";

                return query + Limit + Offset;
            }
            else
                return query; //??
        }

        #endregion
    }
}