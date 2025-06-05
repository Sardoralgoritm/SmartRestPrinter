using Microsoft.EntityFrameworkCore.Storage;

namespace SmartRestaurant.DataAccess.Interfaces;

public interface IUnitOfWork : IDisposable
{
    ICategoryRepository Categories { get; }
    IProductRepository Products { get; }
    ITableRepository Tables { get; }
    IOrderRepository Orders { get; }
    IOrderItemRepository OrderItems { get; }
    IUserRepository Users { get; }
    IPrinterRepository Printers { get; }
    ITableCategoryRepository TableCategories { get; }

    Task<IDbContextTransaction> BeginTransactionAsync();
    Task<int> SaveChangesAsync();
    void ClearTracking();
}

