using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ShoeStore_AO
{
    public partial class MainMenuWindow : Window
    {
        private List<Products> _allProducts;
        private int? _supplierId = null;
        private bool? _sortAsc = null;
        private static AddEditTovarWindow _openedAddEditWindow = null;

        public MainMenuWindow()
        {
            InitializeComponent();
            TbUserInfo.Text = CurrentUser.IsGuest ? "Гость" : CurrentUser.FullName;

            
            if (!CurrentUser.CanSearch) FilterPanel.Visibility = Visibility.Collapsed;

            
            if (!CurrentUser.IsAdmin) AdminPanel.Visibility = Visibility.Collapsed;

            
            if (!CurrentUser.IsManager && !CurrentUser.IsAdmin)
                BtnOrders.Visibility = Visibility.Collapsed;

            LoadData();
        }

        private void LoadData()
        {
            using (var db = new ShoeStore_AOEntities1())
            {
                _allProducts = db.Products.Include("Categories").Include("Manufacturers").Include("Suppliers").ToList();
                var suppliers = new List<dynamic> { new { SupplierID = 0, SupplierName = "Все поставщики" } };
                suppliers.AddRange(db.Suppliers.Select(s => new { s.SupplierID, s.SupplierName }).ToList());

                CmbSupplier.ItemsSource = suppliers;
                CmbSupplier.DisplayMemberPath = "SupplierName";
                CmbSupplier.SelectedValuePath = "SupplierID";
                CmbSupplier.SelectedIndex = 0;
            }
            Filtr();
        }

        private void Filtr()
        {
            var searchList = _allProducts.AsEnumerable();

            if (_supplierId.HasValue && _supplierId.Value > 0)
                searchList = searchList.Where(x => x.SupplierID == _supplierId.Value);

            if (CurrentUser.CanSearch && !string.IsNullOrWhiteSpace(TbSearch.Text))
            {
                string q = TbSearch.Text.ToLower();
                searchList = searchList.Where(x =>
                    (x.ProductName ?? "").ToLower().Contains(q) ||
                    (x.Description ?? "").ToLower().Contains(q) ||
                    (x.Unit ?? "").ToLower().Contains(q) ||
                    (x.Categories?.CategoryName ?? "").ToLower().Contains(q) ||
                    (x.Manufacturers?.ManufacturerName ?? "").ToLower().Contains(q) ||
                    (x.Suppliers?.SupplierName ?? "").ToLower().Contains(q));
            }

            if (_sortAsc.HasValue)
                searchList = _sortAsc.Value ? searchList.OrderBy(x => x.StockQuantity) : searchList.OrderByDescending(x => x.StockQuantity);

            ListTovar.ItemsSource = searchList.ToList();
        }

        private void TbSearch_TextChanged(object sender, TextChangedEventArgs e) => Filtr();
        private void BtnSortAsc_Click(object sender, RoutedEventArgs e) { _sortAsc = true; Filtr(); }
        private void BtnSortDesc_Click(object sender, RoutedEventArgs e) { _sortAsc = false; Filtr(); }
        private void CmbSupplier_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbSupplier.SelectedValue == null) return;
            int id = Convert.ToInt32(CmbSupplier.SelectedValue);
            _supplierId = (id == 0) ? (int?)null : id;
            Filtr();
        }
        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            TbSearch.Text = ""; CmbSupplier.SelectedIndex = 0; _supplierId = null; _sortAsc = null; Filtr();
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            CurrentUser.UserID = 0; CurrentUser.FullName = "Гость"; CurrentUser.RoleName = "Гость";
            new AuthorizationWindow().Show(); this.Close();
        }

        private void BtnOrders_Click(object sender, RoutedEventArgs e) { new OrdersWindow().ShowDialog(); }

        private void BtnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            if (_openedAddEditWindow != null) return;
            _openedAddEditWindow = new AddEditTovarWindow(null);
            _openedAddEditWindow.Closed += (s, a) => _openedAddEditWindow = null;
            if (_openedAddEditWindow.ShowDialog() == true) LoadData();
        }

        private void ListTovar_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!CurrentUser.IsAdmin || ListTovar.SelectedItem == null || _openedAddEditWindow != null) return;
            _openedAddEditWindow = new AddEditTovarWindow(ListTovar.SelectedItem as Products);
            _openedAddEditWindow.Closed += (s, a) => _openedAddEditWindow = null;
            if (_openedAddEditWindow.ShowDialog() == true) LoadData();
        }

        private void BtnDeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (!(ListTovar.SelectedItem is Products sel)) { MessageBox.Show("Выберите товар!"); return; }
            using (var db = new ShoeStore_AOEntities1())
            {
                if (db.OrderItems.Any(o => o.ProductID == sel.ProductID))
                { MessageBox.Show("Нельзя удалить товар из заказов!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

                if (MessageBox.Show("Удалить?", "Вопрос", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    var del = db.Products.Find(sel.ProductID);
                    if (del != null)
                    {
                        if (!string.IsNullOrEmpty(del.Photo))
                        {
                            string p = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", del.Photo);
                            if (System.IO.File.Exists(p)) System.IO.File.Delete(p);
                        }
                        db.Products.Remove(del); db.SaveChanges(); LoadData();
                    }
                }
            }
        }
    }
}