using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data.Entity;

namespace WpfApp2
{
    public partial class MainWindow : Window
    {
        private SportsAppEntities db = new SportsAppEntities();
        private Users currentUser;

        public MainWindow()
        {
            InitializeComponent();
            LoadProducts();
        }

        public MainWindow(Users user) : this()
        {
            currentUser = user;
            if (txtUserInfo != null)
            {
                txtUserInfo.Text = user.Email ?? "Пользователь";
            }
        }

        private void LoadProducts()
        {
            try
            {
                var products = db.Products
                    .Include(p => p.Categories)
                    .Select(p => new
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Category = p.Categories != null ? p.Categories.Name : "Без категории",
                        Price = p.Price,
                        Quantity = p.Quantity,
                        Status = p.Quantity > 0 ? "В наличии" : "Нет в наличии",
                        AddedDate = p.AddedDate
                    })
                    .ToList();

                dgProducts.ItemsSource = products;
                txtTotalItems.Text = products.Count.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки товаров: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ProductWindow productWindow = new ProductWindow();
                if (productWindow.ShowDialog() == true)
                {
                    LoadProducts();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dynamic selectedProduct = dgProducts.SelectedItem;
                if (selectedProduct == null)
                {
                    MessageBox.Show("Выберите товар для редактирования",
                        "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int productId = selectedProduct.Id;

                using (var editDb = new SportsAppEntities())
                {
                    var product = editDb.Products.Find(productId);
                    if (product != null)
                    {
                        ProductWindow productWindow = new ProductWindow(product);
                        if (productWindow.ShowDialog() == true)
                        {
                            LoadProducts();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка редактирования: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dynamic selectedProduct = dgProducts.SelectedItem;
                if (selectedProduct == null)
                {
                    MessageBox.Show("Выберите товар для удаления",
                        "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show("Вы действительно хотите удалить выбранный товар?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    int productId = selectedProduct.Id;
                    var product = db.Products.Find(productId);

                    if (product != null)
                    {
                        db.Products.Remove(product);
                        db.SaveChanges();
                        LoadProducts();

                        MessageBox.Show("Товар успешно удален", "Информация",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                string searchText = txtSearch.Text.ToLower();

                if (string.IsNullOrWhiteSpace(searchText))
                {
                    LoadProducts();
                    return;
                }

                var filteredProducts = db.Products
                    .Where(p => p.Name.ToLower().Contains(searchText))
                    .Select(p => new
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Category = p.Categories != null ? p.Categories.Name : "Без категории",
                        Price = p.Price,
                        Quantity = p.Quantity,
                        Status = p.Quantity > 0 ? "В наличии" : "Нет в наличии",
                        AddedDate = p.AddedDate
                    })
                    .ToList();

                dgProducts.ItemsSource = filteredProducts;
                txtTotalItems.Text = filteredProducts.Count.ToString();
            }
            catch
            {
                LoadProducts();
            }
        }

        private void dgProducts_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            btnEdit_Click(sender, null);
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы действительно хотите выйти из системы?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (db != null)
            {
                db.Dispose();
            }
            base.OnClosed(e);
        }
    }
}