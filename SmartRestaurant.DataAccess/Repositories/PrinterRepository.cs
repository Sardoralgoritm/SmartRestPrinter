using Microsoft.EntityFrameworkCore;
using SmartRestaurant.DataAccess.Data;
using SmartRestaurant.DataAccess.Interfaces;
using SmartRestaurant.Domain.Entities;

namespace SmartRestaurant.DataAccess.Repositories;

public class PrinterRepository : GenericRepository<Printer>, IPrinterRepository
{
    public PrinterRepository(AppDbContext context) : base(context)
    { }

    //private readonly IDbContextFactory<AppDbContext> _contextFactory;
    //public PrinterRepository(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
    //{
    //    _contextFactory = contextFactory;
    //}
}
