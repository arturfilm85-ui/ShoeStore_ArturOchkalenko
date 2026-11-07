using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ShoeStore_AO
{
    public partial class OrdersWindow : Window
    {
        private static AddEditOrderWindow _openedAddEditOrderWindow = null;

        public OrdersWindow()
        {
            InitializeComponent();

            if (!CurrentUser.IsAdmin)
            {
                BtnAddOrder.Visibility = Visibility.Collapsed;
                BtnDeleteOrder.Visibility = Visibility.Collapsed;
            }

            
            using (var db = new ShoeStore_AOEntities1())
            {
                ListOrders.ItemsSource = db.Orders
                    .Include("OrderStatuses")
                    .Include("PickupPoints")
                    .ToList();
            }
        }

        private void BtnAddOrder_Click(object sender, RoutedEventArgs e)
        {
            if (_openedAddEditOrderWindow != null) return;
            _openedAddEditOrderWindow = new AddEditOrderWindow(null);
            _openedAddEditOrderWindow.Closed += (s, a) => _openedAddEditOrderWindow = null;

            if (_openedAddEditOrderWindow.ShowDialog() == true)
            {
                
                using (var db = new ShoeStore_AOEntities1())
                {
                    ListOrders.ItemsSource = db.Orders.Include("OrderStatuses").Include("PickupPoints").ToList();
                }
            }
        }

        private void ListOrders_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!CurrentUser.IsAdmin || ListOrders.SelectedItem == null || _openedAddEditOrderWindow != null) return;
            _openedAddEditOrderWindow = new AddEditOrderWindow(ListOrders.SelectedItem as Orders);
            _openedAddEditOrderWindow.Closed += (s, a) => _openedAddEditOrderWindow = null;

            if (_openedAddEditOrderWindow.ShowDialog() == true)
            {
                using (var db = new ShoeStore_AOEntities1())
                {
                    ListOrders.ItemsSource = db.Orders.Include("OrderStatuses").Include("PickupPoints").ToList();
                }
            }
        }

        private void BtnDeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            if (!(ListOrders.SelectedItem is Orders sel))
            {
                MessageBox.Show("Сначала выберите заказ в списке!");
                return;
            }

            if (MessageBox.Show($"Удалить заказ №{sel.OrderID}?", "Вопрос",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                using (var db = new ShoeStore_AOEntities1())
                {
                    var orderToDelete = db.Orders.Find(sel.OrderID);
                    if (orderToDelete == null)
                    {
                        MessageBox.Show("Заказ не найден в базе данных.");
                        return;
                    }

                    var itemsInOrder = db.OrderItems.Where(i => i.OrderID == orderToDelete.OrderID).ToList();
                    if (itemsInOrder.Any())
                    {
                        db.OrderItems.RemoveRange(itemsInOrder);
                        db.SaveChanges();
                    }

                    db.Orders.Remove(orderToDelete);
                    db.SaveChanges();
                    MessageBox.Show("Заказ успешно удален!");
                }

                
                using (var db2 = new ShoeStore_AOEntities1())
                {
                    ListOrders.ItemsSource = db2.Orders.Include("OrderStatuses").Include("PickupPoints").ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}