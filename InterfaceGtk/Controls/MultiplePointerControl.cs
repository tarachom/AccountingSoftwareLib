/*



*/

using Gtk;

namespace InterfaceGtk
{
    public abstract class MultiplePointerControl : PointerControl
    {
        public MultiplePointerControl()
        {
            Button bMultiple = new Button(new Image(Stock.GoDown, IconSize.Menu));
            PackStart(bMultiple, false, false, 1);
            bMultiple.Clicked += OnMultiple;
        }

        protected virtual void OnMultiple(object? sender, EventArgs args) { }
    }
}