using System.Windows.Controls;

namespace PROShine.Controls
{
    internal class ButtonTab : TabItem
    {
        public ButtonTab()
        {
            Header = new ButtonTabHeader();
        }
    }
}