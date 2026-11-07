using System;
using System.Linq;
using System.Windows;

namespace ShoeStore_AO
{
    public partial class AddEditOrderWindow : Window
    {
        private Orders _currentOrder;
        private bool _isEdit;

        public AddEditOrderWindow(Orders order)
        {
            InitializeComponent();

            using (var db = new ShoeStore_AOEntities1())
            {
                
                CmbUser.ItemsSource = db.Users.ToList();
                CmbStatus.ItemsSource = db.OrderStatuses.ToList();
                CmbPickup.ItemsSource = db.PickupPoints.ToList();
            }

            if (order == null)
            {
                
                _isEdit = false;
                _currentOrder = new Orders
                {
                    OrderDate = DateTime.Today,
                    PickupCode = "0000" 
                };
            }
            else
            {
                
                _isEdit = true;
                _currentOrder = order;
            }

            DataContext = _currentOrder;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
           
            if (_currentOrder.UserID == 0 || _currentOrder.StatusID == 0 || _currentOrder.PickupPointID == 0)
            {
                MessageBox.Show("Заполните клиента, статус и пункт выдачи!");
                return;
            }

            if (string.IsNullOrWhiteSpace(_currentOrder.PickupCode))
            {
                MessageBox.Show("Введите код получения!");
                return;
            }

            using (var db = new ShoeStore_AOEntities1())
            {
                try
                {
                    if (!_isEdit)
                    {
                        db.Orders.Add(_currentOrder);
                    }
                    else
                    {
                        var ex = db.Orders.Find(_currentOrder.OrderID);
                        if (ex != null)
                        {
                            ex.UserID = _currentOrder.UserID;
                            ex.StatusID = _currentOrder.StatusID;
                            ex.PickupPointID = _currentOrder.PickupPointID;
                            ex.PickupCode = _currentOrder.PickupCode;
                            ex.OrderDate = _currentOrder.OrderDate;
                            ex.DeliveryDate = _currentOrder.DeliveryDate;
                        }
                    }

                    db.SaveChanges();
                    MessageBox.Show("Заказ успешно сохранен!");
                    DialogResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка");
                }
            }
        }
    }
}