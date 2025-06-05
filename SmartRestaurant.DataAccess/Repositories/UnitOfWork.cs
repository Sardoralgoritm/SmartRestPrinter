using Microsoft.EntityFrameworkCore.Storage;
using SmartRestaurant.DataAccess.Data;
using SmartRestaurant.DataAccess.Interfaces;

namespace SmartRestaurant.DataAccess.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public ICategoryRepository Categories { get; }
    public IProductRepository Products { get; }
    public ITableRepository Tables { get; }
    public IOrderRepository Orders { get; }
    public IOrderItemRepository OrderItems { get; }
    public IUserRepository Users { get; }
    public IPrinterRepository Printers { get; }
    public ITableCategoryRepository TableCategories { get; }


    public UnitOfWork(
        AppDbContext context,
        ICategoryRepository categories,
        IProductRepository products,
        ITableRepository tables,
        IOrderRepository orders,
        IOrderItemRepository orderItems,
        IUserRepository users,
        IPrinterRepository printer,
        ITableCategoryRepository tableCategories)
    {
        _context = context;
        Categories = categories;
        Products = products;
        Tables = tables;
        Orders = orders;
        OrderItems = orderItems;
        Users = users;
        Printers = printer;
        TableCategories = tableCategories;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    public Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return _context.Database.BeginTransactionAsync();
    }

    public void ClearTracking()
    {
        _context.ChangeTracker.Clear();
    }
}

