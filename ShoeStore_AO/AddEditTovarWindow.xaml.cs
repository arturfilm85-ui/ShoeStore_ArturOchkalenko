using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace ShoeStore_AO
{
    public partial class AddEditTovarWindow : Window
    {
        private Products _currentProduct;
        private bool _isEdit;
        private string _oldPhotoFile;
        private string _imagesFolder;

        public AddEditTovarWindow(Products product)
        {
            InitializeComponent();
            _imagesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
            if (!Directory.Exists(_imagesFolder)) Directory.CreateDirectory(_imagesFolder);

            
            using (var db = new ShoeStore_AOEntities1())
            {
                CmbCategory.ItemsSource = db.Categories.ToList();
                CmbManufacturer.ItemsSource = db.Manufacturers.ToList();
                CmbSupplier.ItemsSource = db.Suppliers.ToList();
            }

            if (product == null)
            {
                _isEdit = false;
                _currentProduct = new Products { ProductID = GenerateNewProductID(), Unit = "шт.", Discount = 0, StockQuantity = 0 };
            }
            else
            {
                _isEdit = true;
                _currentProduct = product;
                _oldPhotoFile = product.Photo;
            }

            DataContext = _currentProduct;
            if (!string.IsNullOrEmpty(_currentProduct.Photo))
            {
                string fullPath = Path.Combine(_imagesFolder, _currentProduct.Photo);
                if (File.Exists(fullPath)) ImgPhoto.Source = new BitmapImage(new Uri(fullPath));
            }
        }

        private string GenerateNewProductID()
        {
            using (var db = new ShoeStore_AOEntities1())
            {
                var last = db.Products.OrderByDescending(p => p.ProductID).FirstOrDefault();
                if (last == null) return "P001";
                string digits = new string(last.ProductID.SkipWhile(c => !char.IsDigit(c)).ToArray());
                if (int.TryParse(digits, out int num)) return "P" + (num + 1).ToString("D3");
                return "P" + (db.Products.Count() + 1).ToString("D3");
            }
        }

        private void BtnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Filter = "Image files|*.jpg;*.jpeg;*.png;*.bmp;*.gif" };
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    BitmapImage bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(dialog.FileName);
                    bmp.DecodePixelWidth = 300;
                    bmp.DecodePixelHeight = 200;
                    bmp.EndInit();

                    string newFileName = $"{_currentProduct.ProductID}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(dialog.FileName)}";
                    string newPath = Path.Combine(_imagesFolder, newFileName);
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bmp));
                    using (var fs = File.Create(newPath)) encoder.Save(fs);

                    if (!string.IsNullOrEmpty(_oldPhotoFile))
                    {
                        string oldPath = Path.Combine(_imagesFolder, _oldPhotoFile);
                        if (File.Exists(oldPath)) File.Delete(oldPath);
                    }
                    _currentProduct.Photo = newFileName;
                    _oldPhotoFile = newFileName;
                    ImgPhoto.Source = new BitmapImage(new Uri(newPath));
                }
                catch (Exception ex) { MessageBox.Show("Ошибка загрузки: " + ex.Message); }
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_currentProduct.ProductName)) { MessageBox.Show("Введите наименование!"); return; }
            if (_currentProduct.Price < 0 || _currentProduct.StockQuantity < 0 || _currentProduct.Discount < 0) { MessageBox.Show("Отрицательные значения запрещены!"); return; }

            using (var db = new ShoeStore_AOEntities1())
            {
                try
                {
                    if (!_isEdit) db.Products.Add(_currentProduct);
                    else
                    {
                        var ex = db.Products.Find(_currentProduct.ProductID);
                        ex.ProductName = _currentProduct.ProductName; ex.Unit = _currentProduct.Unit;
                        ex.Price = _currentProduct.Price; ex.SupplierID = _currentProduct.SupplierID;
                        ex.ManufacturerID = _currentProduct.ManufacturerID; ex.CategoryID = _currentProduct.CategoryID;
                        ex.Discount = _currentProduct.Discount; ex.StockQuantity = _currentProduct.StockQuantity;
                        ex.Description = _currentProduct.Description; ex.Photo = _currentProduct.Photo;
                    }
                    db.SaveChanges(); MessageBox.Show("Сохранено!"); DialogResult = true; Close();
                }
                catch (Exception ex) { MessageBox.Show("Ошибка: " + ex.Message); }
            }
        }
    }
}