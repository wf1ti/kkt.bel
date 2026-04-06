using System;
using System.Data.Entity;
using System.Linq;

namespace WpfApp2
{
    public class DatabaseInitializer : CreateDatabaseIfNotExists<SportsAppEntities>
    {
        protected override void Seed(SportsAppEntities context)
        {
            var defaultCategories = new[]
            {
                new Categories { Name = "Одежда", Description = "Спортивная одежда" },
                new Categories { Name = "Обувь", Description = "Спортивная обувь" },
                new Categories { Name = "Аксессуары", Description = "Инвентарь и аксессуары" }
            };

            context.Categories.AddRange(defaultCategories);

            var adminUser = new Users
            {
                Email = "admin@sports.local",
                PasswordHash = "admin123",
                Role = "admin",
                CreatedAt = DateTime.Now
            };
            context.Users.Add(adminUser);
            context.SaveChanges();

            var shoesCategoryId = context.Categories.First(c => c.Name == "Обувь").Id;

            context.Products.Add(new Products
            {
                Name = "Кроссовки Run Pro",
                CategoryId = shoesCategoryId,
                Price = 5990m,
                Quantity = 8,
                Status = "В наличии",
                AddedDate = DateTime.Now,
                Manufacturer = "SportsPro",
                Article = "RUN-PRO-001",
                Description = "Демо-товар, создан автоматически при первом запуске"
            });

            context.SaveChanges();
            base.Seed(context);
        }
    }
}
