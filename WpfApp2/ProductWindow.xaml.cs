using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity;
using System.Data.Entity.Validation;

namespace WpfApp2
{
    public partial class ProductWindow : Window
    {
        private SportsAppEntities db = new SportsAppEntities();
        private Products currentProduct;
        private bool isEditMode = false;

        public ProductWindow()
        {
            InitializeComponent();
            isEditMode = false;
            Title = "Добавление товара";
            LoadCategories();
        }

        public ProductWindow(Products product) : this()
        {
            isEditMode = true;
            Title = "Редактирование товара";

            if (product != null && product.Id > 0)
            {
                currentProduct = db.Products.Find(product.Id);
            }

            LoadProductData();
        }

        private void LoadCategories()
        {
            try
            {
                var categories = db.Categories.OrderBy(c => c.Name).ToList();

                cmbCategory.Items.Clear();

                foreach (var category in categories)
                {
                    cmbCategory.Items.Add(category.Name);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}");
            }
        }

        private void LoadProductData()
        {
            if (currentProduct == null) return;

            try
            {
                txtName.Text = currentProduct.Name ?? "";
                txtPrice.Text = currentProduct.Price.ToString();
                txtQuantity.Text = currentProduct.Quantity.ToString();
                txtManufacturer.Text = currentProduct.Manufacturer ?? "";
                txtArticle.Text = currentProduct.Article ?? "";
                txtDescription.Text = currentProduct.Description ?? "";
                txtWeight.Text = currentProduct.Weight?.ToString() ?? "";
                txtSize.Text = currentProduct.Size ?? "";
                txtColor.Text = currentProduct.Color ?? "";
                txtMaterial.Text = currentProduct.Material ?? "";

                if (currentProduct.Categories != null)
                {
                    cmbCategory.Text = currentProduct.Categories.Name;
                }
                else if (currentProduct.CategoryId > 0)
                {
                    var category = db.Categories.FirstOrDefault(c => c.Id == currentProduct.CategoryId);
                    if (category != null)
                    {
                        cmbCategory.Text = category.Name;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите наименование товара", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(cmbCategory.Text))
            {
                MessageBox.Show("Введите категорию товара", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbCategory.Focus();
                return false;
            }

            string priceText = txtPrice.Text.Trim().Replace(" ", "");
            if (string.IsNullOrWhiteSpace(priceText))
            {
                MessageBox.Show("Введите цену товара", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPrice.Focus();
                return false;
            }

            if (!decimal.TryParse(priceText, out decimal price))
            {
                MessageBox.Show("Введите корректную цену (например: 99.99 или 100)", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPrice.Focus();
                return false;
            }

            if (price < 0)
            {
                MessageBox.Show("Цена не может быть отрицательной", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPrice.Focus();
                return false;
            }

            string quantityText = txtQuantity.Text.Trim();
            if (string.IsNullOrWhiteSpace(quantityText))
            {
                MessageBox.Show("Введите количество товара", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtQuantity.Focus();
                return false;
            }

            if (!int.TryParse(quantityText, out int quantity))
            {
                MessageBox.Show("Введите корректное количество (целое число)", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtQuantity.Focus();
                return false;
            }

            if (quantity < 0)
            {
                MessageBox.Show("Количество не может быть отрицательным", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtQuantity.Focus();
                return false;
            }

            return true;
        }

        private string GetStatusByQuantity(int quantity)
        {
            if (quantity <= 0)
                return "Нет в наличии";
            else if (quantity < 5)
                return "Мало";
            else
                return "В наличии";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInput())
                    return;

                string categoryName = cmbCategory.Text.Trim();

                var category = db.Categories.FirstOrDefault(c => c.Name.ToLower() == categoryName.ToLower());

                if (category == null)
                {
                    var result = MessageBox.Show($"Категория \"{categoryName}\" не найдена.\nСоздать новую категорию?",
                        "Создание категории", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        category = new Categories
                        {
                            Name = categoryName,
                            Description = null
                        };
                        db.Categories.Add(category);
                        db.SaveChanges();

                        LoadCategories();
                    }
                    else
                    {
                        return;
                    }
                }

                decimal price = decimal.Parse(txtPrice.Text.Trim().Replace(" ", ""));
                int quantity = int.Parse(txtQuantity.Text.Trim());
                string status = GetStatusByQuantity(quantity);

                decimal? weight = null;
                if (!string.IsNullOrWhiteSpace(txtWeight.Text))
                {
                    if (decimal.TryParse(txtWeight.Text.Trim().Replace(" ", ""), out decimal w))
                    {
                        weight = w;
                    }
                }

                if (isEditMode && currentProduct != null)
                {
                    currentProduct.Name = txtName.Text.Trim();
                    currentProduct.CategoryId = category.Id;
                    currentProduct.Price = price;
                    currentProduct.Quantity = quantity;
                    currentProduct.Status = status;
                    currentProduct.Manufacturer = string.IsNullOrWhiteSpace(txtManufacturer.Text) ? null : txtManufacturer.Text.Trim();
                    currentProduct.Article = string.IsNullOrWhiteSpace(txtArticle.Text) ? null : txtArticle.Text.Trim();
                    currentProduct.Description = string.IsNullOrWhiteSpace(txtDescription.Text) ? null : txtDescription.Text.Trim();
                    currentProduct.Weight = weight;
                    currentProduct.Size = string.IsNullOrWhiteSpace(txtSize.Text) ? null : txtSize.Text.Trim();
                    currentProduct.Color = string.IsNullOrWhiteSpace(txtColor.Text) ? null : txtColor.Text.Trim();
                    currentProduct.Material = string.IsNullOrWhiteSpace(txtMaterial.Text) ? null : txtMaterial.Text.Trim();

                    db.SaveChanges();

                    MessageBox.Show("Товар успешно обновлен", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var newProduct = new Products
                    {
                        Name = txtName.Text.Trim(),
                        CategoryId = category.Id,
                        Price = price,
                        Quantity = quantity,
                        Status = status,
                        AddedDate = DateTime.Now,
                        Manufacturer = string.IsNullOrWhiteSpace(txtManufacturer.Text) ? null : txtManufacturer.Text.Trim(),
                        Article = string.IsNullOrWhiteSpace(txtArticle.Text) ? null : txtArticle.Text.Trim(),
                        Description = string.IsNullOrWhiteSpace(txtDescription.Text) ? null : txtDescription.Text.Trim(),
                        Weight = weight,
                        Size = string.IsNullOrWhiteSpace(txtSize.Text) ? null : txtSize.Text.Trim(),
                        Color = string.IsNullOrWhiteSpace(txtColor.Text) ? null : txtColor.Text.Trim(),
                        Material = string.IsNullOrWhiteSpace(txtMaterial.Text) ? null : txtMaterial.Text.Trim()
                    };

                    db.Products.Add(newProduct);
                    db.SaveChanges();

                    MessageBox.Show("Товар успешно добавлен", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }

                DialogResult = true;
                Close();
            }
            catch (DbEntityValidationException ex)
            {
                string errorMessage = "Ошибка валидации:\n";
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        errorMessage += $"Поле: {validationError.PropertyName}, Ошибка: {validationError.ErrorMessage}\n";
                    }
                }
                MessageBox.Show(errorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
            {
                string errorMessage = "Ошибка базы данных:\n";
                var innerEx = ex.InnerException;
                while (innerEx != null)
                {
                    errorMessage += innerEx.Message + "\n";
                    innerEx = innerEx.InnerException;
                }

                if (errorMessage.Contains("UNIQUE") || errorMessage.Contains("duplicate"))
                {
                    MessageBox.Show("Категория с таким названием уже существует.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show(errorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Отменить изменения?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                DialogResult = false;
                Close();
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