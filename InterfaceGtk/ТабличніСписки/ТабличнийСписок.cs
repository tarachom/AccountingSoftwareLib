

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
            if (treeView.Data.ContainsKey("Where"))
                treeView.Data["Where"] = null;
        }
    }
}