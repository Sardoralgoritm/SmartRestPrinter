using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmartRestaurant.BusinessLogic.Services;
using SmartRestaurant.BusinessLogic.Services.Auth.Concrete;
using SmartRestaurant.BusinessLogic.Services.OrderItems.Concrete;
using SmartRestaurant.BusinessLogic.Services.Orders.Concrete;
using SmartRestaurant.BusinessLogic.Services.Printers.Concrete;
using SmartRestaurant.BusinessLogic.Services.Products.Concrete;
using SmartRestaurant.BusinessLogic.Services.TableCategories.Concrete;
using SmartRestaurant.BusinessLogic.Services.Tables.Concrete;
using SmartRestaurant.BusinessLogic.Services.Users.Concrete;
using SmartRestaurant.DataAccess.Data;
using SmartRestaurant.DataAccess.Interfaces;
using SmartRestaurant.DataAccess.Repositories;
using SmartRestaurant.Desktop.Helpers.Security;
using SmartRestaurant.Domain.Const;
using SmartRestaurant.Domain.Entities;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SmartRestaurant.Desktop;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly IHost _host;
    public static IServiceProvider ServiceProvider { get; private set; }

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
                config.AddJsonFile("core.dll", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                IConfiguration configuration = context.Configuration;

                // Shifrlangan connection string’ni o‘qiymiz va AES orqali ochamiz
                var encryptedConn = configuration.GetConnectionString("DefaultConnection");
                var decryptedConn = AesEncryptionHelper.Decrypt(encryptedConn);

                services.AddDbContext<AppDbContext>(options =>
                    options.UseNpgsql(decryptedConn));

                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

                services.AddScoped<IUnitOfWork, UnitOfWork>();
                services.AddScoped<ICategoryRepository, CategoryRepository>();
                services.AddScoped<IProductRepository, ProductRepository>();
                services.AddScoped<ITableRepository, TableRepository>();
                services.AddScoped<IOrderRepository, OrderRepository>();
                services.AddScoped<IOrderItemRepository, OrderItemRepository>();
                services.AddScoped<IUserRepository, UserRepository>();
                services.AddScoped<IPrinterRepository, PrinterRepository>();
                services.AddScoped<ITableCategoryRepository, TableCategoryRepository>();

                services.AddScoped<ITableService, TableService>();
                services.AddScoped<IOrderService, OrderService>();
                services.AddScoped<IOrderItemService, OrderItemService>();
                services.AddScoped<ICategoryService, CategoryService>();
                services.AddScoped<IProductService, ProductService>();
                services.AddScoped<IAuthService, AuthService>();
                services.AddScoped<IUserService, UserService>();
                services.AddScoped<IPrinterService, PrinterService>();
                services.AddScoped<ITableCategoryService, TableCategoryService>();

                ServiceProvider = services.BuildServiceProvider();
            })
            .Build();

        ServiceProvider = _host.Services;

        // Bazani yaratish (birinchi ishga tushishda)
        using (var scope = ServiceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated(); // yoki db.Database.Migrate();
        }
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        //EventManager.RegisterClassHandler(
        //    typeof(Window),
        //    UIElement.ManipulationBoundaryFeedbackEvent,
        //    new EventHandler<ManipulationBoundaryFeedbackEventArgs>((sender, args) => args.Handled = true)
        //);

        //EventManager.RegisterClassHandler(
        //    typeof(Window),
        //    Window.LoadedEvent,
        //    new RoutedEventHandler((sender, args) =>
        //    {
        //        if (sender is Window window)
        //        {
        //            Stylus.SetIsPressAndHoldEnabled(window, false);
        //            Stylus.SetIsFlicksEnabled(window, false);
        //        }
        //    })
        //);

        if (!LicenseService.IsLicenseValid())
        {
            MessageBox.Show("Bu dastur faqat ruxsat berilgan kompyuterda ishlaydi!",
                "Litsenziya xatosi",
                MessageBoxButton.OK,
                MessageBoxImage.Error
             );
            Current.Shutdown();
            return;
        }

        using var scope = ServiceProvider!.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await EnsureTakeawayTableExistsAsync(context);
        await EnsureDefaultUserExistsAsync(context);
    }

    private async Task EnsureTakeawayTableExistsAsync(AppDbContext context)
    {
        var exists = await context.Tables.AnyAsync(t => t.Name.ToLower().StartsWith("takeaway"));
        if (!exists)
        {
            context.Tables.Add(new Table
            {
                Id = Guid.NewGuid(),
                Name = "Takeaway 0",
                Status = TableStatus.Free,
                TableCategoryId = null
            });

            await context.SaveChangesAsync();
        }
    }

    private async Task EnsureDefaultUserExistsAsync(AppDbContext context)
    {
        var exists = await context.Users.Where(u => !u.IsDeleted).AnyAsync(u => u.Role == Roles.BOSS);
        if (!exists)
        {
            var salt = BCrypt.Net.BCrypt.GenerateSalt();
            var hash = BCrypt.Net.BCrypt.HashPassword("12345678" + salt);

            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Admin",
                LastName = "Admin",
                PhoneNumber = "+998000000000",
                Role = Roles.BOSS,
                PasswordHash = hash,
                PasswordSalt = salt,
                ImageUrl = ""
            };

            try
            {
                context.Users.Add(user);
                await context.SaveChangesAsync();
            }
            catch (Exception)
            {
                context.ChangeTracker.Clear();
            }
            
        }
    }
}
