using System.Windows.Controls;

namespace PROShine.Controls
{
    class ButtonTab : TabItem
    {
        public ButtonTab()
        {
            this.Header = new ButtonTabHeader();
        }
    }
}
