using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace PROShine
{
    public partial class InventoryView : UserControl
    {
        private GridViewColumnHeader _lastColumn;
        private ListSortDirection _lastDirection;

        public InventoryView()
        {
            InitializeComponent();
        }

        private void ItemsListViewHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);

            ListSortDirection direction = ListSortDirection.Ascending;
            if (column == _lastColumn && direction == _lastDirection)
            {
                direction = ListSortDirection.Descending;
            }

            ItemsListView.Items.SortDescriptions.Clear();
            ItemsListView.Items.SortDescriptions.Add(new SortDescription((string)column.Content, direction));

            _lastColumn = column;
            _lastDirection = direction;
        }
    }
}
