
using Gtk;
using AccountingSoftware;

namespace InterfaceGtk
{
    public static class ПеріодДляЖурналу
    {
        public enum ТипПеріоду
        {
            ВесьПеріод = 1,
            Особливий,
            Рік,
            ПівРоку,
            Квартал,
            ДваМісяці,
            Місяць,
            Тиждень,
            ТриДні,
            ДваДні,
            День
        }

        static string ТипПеріоду_Alias(ТипПеріоду value)
        {
            return value switch
            {
                ТипПеріоду.ВесьПеріод => "Весь період",
                ТипПеріоду.Особливий => "Особливий",
                ТипПеріоду.Рік => "Рік",
                ТипПеріоду.ПівРоку => "Пів року",
                ТипПеріоду.Квартал => "Три місяці",
                ТипПеріоду.ДваМісяці => "Два місяці",
                ТипПеріоду.Місяць => "Місяць",
                ТипПеріоду.Тиждень => "Тиждень",
                ТипПеріоду.ТриДні => "Три дні",
                ТипПеріоду.ДваДні => "Два дні",
                ТипПеріоду.День => "День",
                _ => ""
            };
        }

        public static ComboBoxText СписокВідбірПоПеріоду()
        {
            ComboBoxText сomboBox = new ComboBoxText();

            foreach (ТипПеріоду value in Enum.GetValues<ТипПеріоду>())
                сomboBox.Append(value.ToString(), ТипПеріоду_Alias(value));

            return сomboBox;
        }

        public static DateTime? ДатаПочатокЗПеріоду(ТипПеріоду типПеріоду)
        {
            DateTime? dateTime = типПеріоду switch
            {
                ТипПеріоду.Рік => DateTime.Now.AddYears(-1),
                ТипПеріоду.ПівРоку => DateTime.Now.AddMonths(-6),
                ТипПеріоду.Квартал => DateTime.Now.AddMonths(-3),
                ТипПеріоду.ДваМісяці => DateTime.Now.AddMonths(-2),
                ТипПеріоду.Місяць => DateTime.Now.AddMonths(-1),
                ТипПеріоду.Тиждень => DateTime.Now.AddDays(-7),
                ТипПеріоду.ТриДні => DateTime.Now.AddDays(-2),
                ТипПеріоду.ДваДні => DateTime.Now.AddDays(-1),
                ТипПеріоду.День => DateTime.Now,
                _ => null
            };

            return dateTime != null ? dateTime.Value.Date : null;
        }

        public static Where? ВідбірПоПеріоду(string fieldWhere, ТипПеріоду типПеріоду, DateTime? start = null, DateTime? stop = null)
        {
            if (типПеріоду == ТипПеріоду.Особливий)
            {
                if (start != null && stop != null)
                {
                    string start_format = start.Value.ToString("yyyy-MM-dd 00:00:00");
                    string stop_format = stop.Value.ToString("yyyy-MM-dd 23:59:59");

                    return new Where(fieldWhere, Comparison.BETWEEN, $"'{start_format}' AND '{stop_format}'", true);
                }
                else
                    return null;
            }
            else
            {
                DateTime? dateTime = ДатаПочатокЗПеріоду(типПеріоду);
                return dateTime != null ? new Where(fieldWhere, Comparison.QT_EQ, dateTime.Value) : null;
            }
        }
    }
}