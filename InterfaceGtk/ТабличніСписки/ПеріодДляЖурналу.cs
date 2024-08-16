
using Gtk;
using AccountingSoftware;

namespace InterfaceGtk
{
    public static class ПеріодДляЖурналу
    {
        public enum ТипПеріоду
        {
            ВесьПеріод = 1,
            ЗПочаткуРоку = 2,
            Квартал = 6,
            ЗМинулогоМісяця = 7,
            Місяць = 8,
            ЗПочаткуМісяця = 3,
            ЗПочаткуТижня = 4,
            ПоточнийДень = 5,
            ДваДні = 9,
            ТриДні = 10
        }

        static string ТипПеріоду_Alias(ТипПеріоду value)
        {
            return value switch
            {
                ТипПеріоду.ВесьПеріод => "Весь період",
                ТипПеріоду.ЗПочаткуРоку => "Рік (з початку року)",
                ТипПеріоду.Квартал => "Квартал (три місяці)",
                ТипПеріоду.ЗМинулогоМісяця => "Два місяці (з 1 числа)",
                ТипПеріоду.Місяць => "Місяць",
                ТипПеріоду.ЗПочаткуМісяця => "Місяць (з 1 числа)",
                ТипПеріоду.ЗПочаткуТижня => "Тиждень",
                ТипПеріоду.ПоточнийДень => "День",
                ТипПеріоду.ДваДні => "Два дні",
                ТипПеріоду.ТриДні => "Три дні",
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

        public static Where? ВідбірПоПеріоду(string fieldWhere, ТипПеріоду типПеріоду)
        {
            switch (типПеріоду)
            {
                case ТипПеріоду.ЗПочаткуРоку:
                    return new Where(fieldWhere, Comparison.QT_EQ, new DateTime(DateTime.Now.Year, 1, 1));
                case ТипПеріоду.Квартал:
                    {
                        DateTime ДатаТриМісцяНазад = DateTime.Now.AddMonths(-3);
                        return new Where(fieldWhere, Comparison.QT_EQ, new DateTime(ДатаТриМісцяНазад.Year, ДатаТриМісцяНазад.Month, 1));
                    }
                case ТипПеріоду.ЗМинулогоМісяця:
                    {
                        DateTime ДатаМісцьНазад = DateTime.Now.AddMonths(-1);
                        return new Where(fieldWhere, Comparison.QT_EQ, new DateTime(ДатаМісцьНазад.Year, ДатаМісцьНазад.Month, 1));
                    }
                case ТипПеріоду.Місяць:
                    return new Where(fieldWhere, Comparison.QT_EQ, DateTime.Now.AddMonths(-1));
                case ТипПеріоду.ЗПочаткуМісяця:
                    return new Where(fieldWhere, Comparison.QT_EQ, new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1));
                case ТипПеріоду.ЗПочаткуТижня:
                    {
                        DateTime СімДнівНазад = DateTime.Now.AddDays(-7);
                        return new Where(fieldWhere, Comparison.QT_EQ, new DateTime(СімДнівНазад.Year, СімДнівНазад.Month, СімДнівНазад.Day));
                    }
                case ТипПеріоду.ДваДні:
                    {
                        DateTime ДваДніНазад = DateTime.Now.AddDays(-1);
                        return new Where(fieldWhere, Comparison.QT_EQ, new DateTime(ДваДніНазад.Year, ДваДніНазад.Month, ДваДніНазад.Day));
                    }
                case ТипПеріоду.ТриДні:
                    {
                        DateTime ТриДніНазад = DateTime.Now.AddDays(-2);
                        return new Where(fieldWhere, Comparison.QT_EQ, new DateTime(ТриДніНазад.Year, ТриДніНазад.Month, ТриДніНазад.Day));
                    }
                case ТипПеріоду.ПоточнийДень:
                    return new Where(fieldWhere, Comparison.QT_EQ, new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day));
                default:
                    return null;
            }
        }
    }
}