
using Gdk;

namespace InterfaceGtk.Іконки
{
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
}