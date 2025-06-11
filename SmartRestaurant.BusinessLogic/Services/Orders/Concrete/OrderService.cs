using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SmartRestaurant.BusinessLogic.Extentions;
using SmartRestaurant.BusinessLogic.Services.OrderItems.DTOs;
using SmartRestaurant.BusinessLogic.Services.Orders.DTOs;
using SmartRestaurant.DataAccess.Interfaces;
using SmartRestaurant.Domain.Const;
using SmartRestaurant.Domain.Entities;
using SmartRestaurant.Domain.Models.PageResult;

namespace SmartRestaurant.BusinessLogic.Services.Orders.Concrete;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;

    public OrderService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public PagedResult<OrderDto> GetAll(OrderSortFilterOptions option)
    {
        var orders = _unitOfWork.Orders.GetAll()
                                       .Include(o => o.OrderItems.Where(oi => !oi.IsDeleted))
                                           .ThenInclude(oi => oi.Product)
                                               .ThenInclude(p => p.Category)
                                       .Include(o => o.Table)
                                       .Include(o => o.OrderedByUser)
                                       .Include(o => o.ClosedByUser)
                                       .Include(o => o.CanceledByUser).SortFilter(option);
        return orders.Select(o => (OrderDto)o).AsPagedResult(option);
    }

    public async Task<OrderDto?> GetByIdAsync(Guid id)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id);
        return order is null ? null : (OrderDto)order;
    }

    public async Task<(bool, int)> CreateAsync(AddOrderDto dto)
    {
        using var transaction = await _unitOfWork.BeginTransactionAsync();

        try
        {
            var order = new Order
            {
                Id = Guid.NewGuid(),
                TableId = dto.TableId,
                QueueNumber = GetTodayQueueNumber() + 1, // har kuni 1 dan boshlanadi
                Status = dto.Status ??= OrderStatus.Open,
                TotalPrice = dto.Items.Sum(i => i.TotalPrice),
                OrderItems = dto.Items.Select(i => new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = i.ProductId,
                    ProductPrice = i.ProductPrice,
                    Quantity = i.Quantity
                }).ToList(),
                TransactionId = dto.TransactionId + (GetTodayQueueNumber() + 1).ToString("D4"),
                OrderedByUserId = dto.OrderedUserId,
                ClosedByUserId = dto.ClosedUserId,
                CreatedAt = DateTimeUzb.UZBTime,
                UpdatedAt = DateTimeUzb.UZBTime,
            };

            var check = await _unitOfWork.Orders.AddAsync(order);
            if (check == false)
                return (false, 0);


            // Stol statusini Busy ga o‘zgartirish
            var table = await _unitOfWork.Tables.GetAll().FirstOrDefaultAsync(t => t.Id == dto.TableId);
            if (table is not null)
            {
                table.Status = TableStatus.Busy;
                await _unitOfWork.Tables.UpdateAsync(table);
                await _unitOfWork.SaveChangesAsync();
            }

            await transaction.CommitAsync();
            return (true, order.QueueNumber);
        }
        catch
        {
            await transaction.RollbackAsync();
            return (false, 0);
        }
    }

    public int GetTodayQueueNumber()
    {
        var today = DateTimeUzb.UZBTime;
        var ordersToday = _unitOfWork.Orders.GetAll();
        int maxQueue = ordersToday
            .Where(o => o.CreatedAt.Date == today.Date)
            .Max(o => (int?)o.QueueNumber) ?? 0;

        return maxQueue;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _unitOfWork.Orders.DeleteByIdAsync(id);
    }

    public async Task<OrderDto?> GetOpenOrderByTableId(Guid tableId)
    {
        var order = await _unitOfWork.Orders.GetOpenOrderWithProductsByTableIdAsync(tableId);
        if (order == null)
            return null;

        return (OrderDto)order;
    }

    public async Task<ReportOrderDto> GetReportOrder(OrderSortFilterOptions option)
    {
        var orders = await _unitOfWork.Orders.GetAll()
                                             .Include(o => o.OrderItems)
                                                .ThenInclude(OrderItems => OrderItems.Product)
                                                    .ThenInclude(p => p.Category)
                                             .SortFilter(option).ToListAsync();

        var result = new ReportOrderDto
        {
            TotalOrdersCount = orders.Count,
            TotalRevenue = orders.Sum(o => o.TotalPrice),
            TopOrderItems = orders
                            .SelectMany(o => o.OrderItems)
                            .GroupBy(oi => new
                            {
                                oi.ProductId,
                                oi.Product.Name,
                                oi.Product.Price,
                                CategoryName = oi.Product.Category.Name
                            })
                            .Select(g => new OrderItemDto
                            {
                                ProductId = g.Key.ProductId,
                                ProductName = g.Key.Name,
                                ProductPrice = g.Key.Price,
                                CategoryName = g.Key.CategoryName,
                                Quantity = g.Sum(x => x.Quantity)
                            })
                            .OrderByDescending(i => i.Quantity)
                            .Take(5)
                            .ToList()
        };

        return result;
    }

    public async Task<bool> CloseOrderAndFreeTable(EditOrderDto order, Guid tableId)
    {
        using var transaction = await _unitOfWork.BeginTransactionAsync();

        try
        {
            var existedOrder = await _unitOfWork.Orders.GetByIdAsync(order.OrderId);
            if (existedOrder is null)
                return false;

            var existingItems = _unitOfWork.OrderItems.GetAll().Where(x => x.OrderId == existedOrder.Id);

            if (existingItems.Any())
                _unitOfWork.OrderItems.RemoveRange(existingItems);

            var newItems = order.Items.Select(i => new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = existedOrder.Id,
                ProductId = i.ProductId,
                ProductPrice = i.ProductPrice,
                Quantity = i.Quantity
            }).ToList();

            if (newItems.Any())
            {
                await _unitOfWork.OrderItems.AddRangeAsync(newItems);

                existedOrder.Status = order.Status;
                existedOrder.ClosedByUserId = order.ClosedUserId;
                existedOrder.ClientPhoneNumber = order.ClientPhoneNumber;
                existedOrder.TotalPrice = order.Items.Sum(i => i.TotalPrice);
                existedOrder.UpdatedAt = DateTimeUzb.UZBTime;

                await _unitOfWork.Orders.UpdateAsync(existedOrder);
            }
            else
            {
                existedOrder.Status = OrderStatus.Closed;
                existedOrder.TotalPrice = 0;
                await _unitOfWork.Orders.DeleteAsync(existedOrder);
            }

            var table = await _unitOfWork.Tables.GetByIdAsync(tableId);
            if (table is null)
                return false;

            table.Status = TableStatus.Free;
            await _unitOfWork.Tables.UpdateAsync(table);

            var result = await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
            return result > 0;
        }
        catch
        {
            await transaction.RollbackAsync();
            return false;
        }
    }

    public async Task<bool> UpdateTotalPrice(Guid orderId, double totalPrice)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
        if (order is null) return false;

        order.TotalPrice = totalPrice;
        await _unitOfWork.Orders.UpdateAsync(order);
        return await _unitOfWork.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteOrderAndFreeTableAsync(Guid OrderId, Guid tableId)
    {
        using var transaction = await _unitOfWork.BeginTransactionAsync();

        try
        {
            var theOrder = _unitOfWork.Orders.GetAll().Include(o => o.OrderItems);
            var existed = await theOrder.FirstOrDefaultAsync(o => o.Id == OrderId);
            //var existed = await _unitOfWork.Orders.GetByIdAsync(OrderId);
            if (existed is null) return false;

            foreach (var item in existed.OrderItems)
            {
                await _unitOfWork.OrderItems.DeleteAsync(item);
            }

            existed.Status = OrderStatus.Closed;
            await _unitOfWork.Orders.DeleteAsync(existed);

            var table = await _unitOfWork.Tables.GetByIdAsync(tableId);
            if (table is null) return false;

            table.Status = TableStatus.Free;
            await _unitOfWork.Tables.UpdateAsync(table);

            var result = await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
            return result > 0;
        }
        catch
        {
            await transaction.RollbackAsync();
            return false;
        }
    }

    public async Task<bool> CancelOrderAsync(Guid orderId, Guid UserId)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order is null) return false;
            order.Status = OrderStatus.Canceled;
            order.CanceledAt = DateTimeUzb.UZBTime;
            order.CanceledByUserId = UserId;
            await _unitOfWork.Orders.UpdateAsync(order);
            return await _unitOfWork.SaveChangesAsync() > 0;
        }
        catch
        {
            return false;
        }
    }

    public PagedResult<OrderItemDto> GetAllReportOrder(OrderSortFilterOptions option)
    {
        var orders = _unitOfWork.Orders.GetAll()
                                             .Include(o => o.OrderItems)
                                                .ThenInclude(OrderItems => OrderItems.Product)
                                             .SortFilter(option);

        var result = orders.SelectMany(o => o.OrderItems)
                    .GroupBy(oi => new
                    {
                        oi.ProductId,
                        oi.Product.Name,
                        oi.Product.Price,
                    })
                    .Select(g => new OrderItemDto
                    {
                        ProductId = g.Key.ProductId,
                        ProductName = g.Key.Name,
                        ProductPrice = g.Key.Price,
                        Quantity = g.Sum(x => x.Quantity)
                    })
                    .OrderByDescending(i => i.Quantity).AsPagedResult(option);

        return result;
    }

    public async Task<bool> ChangeTableIdAsync(Guid orderId, Guid oldTableId, Guid newTableId)
    {
        using var transaction = await _unitOfWork.BeginTransactionAsync();

        try
        {
            var exitingOrder = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (exitingOrder == null) return false;

            exitingOrder.TableId = newTableId;
            await _unitOfWork.Orders.UpdateAsync(exitingOrder);

            var exitingTable = await _unitOfWork.Tables.GetByIdAsync(oldTableId);
            if (exitingTable is null) return false;
            exitingTable.Status = TableStatus.Free;
            await _unitOfWork.Tables.UpdateAsync(exitingTable);

            var newTable = await _unitOfWork.Tables.GetByIdAsync(newTableId);
            if (newTable is null) return false;
            newTable.Status = TableStatus.Busy;
            await _unitOfWork.Tables.UpdateAsync(newTable);

            var result = await _unitOfWork.SaveChangesAsync();
            transaction.Commit();
            return result > 0;
        }
        catch
        {
            await transaction.RollbackAsync();
            return false;
        }
    }

    public async Task<List<OrderItemDto>> GetAllReportOrderListAsync(OrderSortFilterOptions option)
    {
        var orders = _unitOfWork.Orders.GetAll()
                                             .Include(o => o.OrderItems)
                                                .ThenInclude(OrderItems => OrderItems.Product)
                                             .SortFilter(option);

        var result = await orders.SelectMany(o => o.OrderItems)
                    .GroupBy(oi => new
                    {
                        oi.ProductId,
                        oi.Product.Name,
                        oi.Product.Price,
                    })
                    .Select(g => new OrderItemDto
                    {
                        ProductId = g.Key.ProductId,
                        ProductName = g.Key.Name,
                        ProductPrice = g.Key.Price,
                        Quantity = g.Sum(x => x.Quantity)
                    })
                    .OrderByDescending(i => i.Quantity).ToListAsync();

        return result;
    }
}