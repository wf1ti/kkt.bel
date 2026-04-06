using System.Windows;
using System.Data.Entity;

namespace WpfApp2
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Database.SetInitializer(new DatabaseInitializer());

            try
            {
                using (var db = new SportsAppEntities())
                {
                    db.Database.Initialize(false);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(
                    "Не удалось инициализировать локальную базу данных.\n" + ex.Message,
                    "Ошибка базы данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
