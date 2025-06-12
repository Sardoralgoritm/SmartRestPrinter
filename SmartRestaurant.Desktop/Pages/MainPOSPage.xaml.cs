using Microsoft.Extensions.DependencyInjection;
using SmartRestaurant.BusinessLogic.Services;
using SmartRestaurant.BusinessLogic.Services.Auth.Concrete;
using SmartRestaurant.BusinessLogic.Services.OrderItems.Concrete;
using SmartRestaurant.BusinessLogic.Services.OrderItems.DTOs;
using SmartRestaurant.BusinessLogic.Services.Orders.Concrete;
using SmartRestaurant.BusinessLogic.Services.Orders.DTOs;
using SmartRestaurant.BusinessLogic.Services.Printers.Concrete;
using SmartRestaurant.BusinessLogic.Services.Products.Concrete;
using SmartRestaurant.BusinessLogic.Services.Products.DTOs;
using SmartRestaurant.BusinessLogic.Services.TableCategories.Concrete;
using SmartRestaurant.BusinessLogic.Services.Tables;
using SmartRestaurant.BusinessLogic.Services.Tables.Concrete;
using SmartRestaurant.BusinessLogic.Services.Tables.DTOs;
using SmartRestaurant.Desktop.Components;
using SmartRestaurant.Desktop.Helpers.ScrollCustomization;
using SmartRestaurant.Desktop.Helpers.Session;
using SmartRestaurant.Desktop.Service;
using SmartRestaurant.Desktop.Windows.Auth;
using SmartRestaurant.Desktop.Windows.Extensions;
using SmartRestaurant.Desktop.Windows.Orders;
using SmartRestaurant.Domain.Const;
using SmartRestaurant.Domain.Entities;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static SkiaSharp.HarfBuzz.SKShaper;

namespace SmartRestaurant.Desktop.Pages;

/// <summary>
/// Interaction logic for MainPOSPage.xaml
/// </summary>
public partial class MainPOSPage : Page
{
    private readonly ITableService _tableService;
    private readonly IOrderService _orderService;
    private readonly IOrderItemService _orderItemService;
    private readonly ICategoryService _categoryService;
    private readonly ITableCategoryService _tableCategoryService;
    private readonly IProductService _productService;
    private readonly IAuthService _authService;

    private TableComponent? selectedTable = null;
    private TableDto? _selectedTable;
    private OrderDto? _currentOrder;
    private List<OrderItemDto> _previousItems = new();
    private List<OrderItemDto> _newItems = new();
    private Dictionary<Guid, OrderSummaryItem> _orderItemComponents = new();
    private Dictionary<Guid, OrderSummaryItem> _previousOrderItemComponents = new();
    private bool TakeAway = false;
    private double TotalPrice = 0;
    private NumberFormatInfo nfi;
    private CancellationTokenSource _searchCts = null!;

    public double PaymentPrice = 0;
    private string? _lotteryPhoneNumber;

    public MainPOSPage()
    {
        InitializeComponent();

        _tableService = App.ServiceProvider.GetRequiredService<ITableService>();
        _orderService = App.ServiceProvider.GetRequiredService<IOrderService>();
        _orderItemService = App.ServiceProvider.GetRequiredService<IOrderItemService>();
        _categoryService = App.ServiceProvider.GetRequiredService<ICategoryService>();
        _productService = App.ServiceProvider.GetRequiredService<IProductService>();
        _authService = App.ServiceProvider.GetRequiredService<IAuthService>();
        _tableCategoryService = App.ServiceProvider.GetRequiredService<ITableCategoryService>();

        nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        nfi.NumberGroupSeparator = " ";
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        await RefreshPageDataAsync();

        this.DisableTouchScrollBounce();
    }

    public void AddOrderItem(ProductDto product)
    {
        if (TakeAway == false && selectedTable == null)
        {
            NotificationManager.ShowNotification(NotificationWindow.MessageType.Warning,
                                "Iltimos, buyurtma uchun stol tanlang yoki olib ketish tugmasini bosing!",
                                NotificationWindow.NotificationPosition.TopCenter, 4000);
            return;
        }

        var existing = _newItems.FirstOrDefault(x => x.ProductId == product.Id);
        TotalPrice += product.Price;
        txt_totalPrice.Text = TotalPrice.ToString("#,0.##", nfi);

        if (TakeAway == false)
        {
            btnSendToKitchen.Visibility = Visibility.Visible;
            btnPay.Visibility = Visibility.Collapsed;
        }

        if (existing != null)
        {
            existing.Quantity += 1;

            // UI componentdagi quantity ni yangilash
            if (_orderItemComponents.TryGetValue(existing.ProductId, out var comp))
            {
                comp.UpdateQuantity(existing.Quantity);
            }
        }
        else
        {
            var newItem = new OrderItemDto
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                ProductName = product.Name,
                ProductPrice = product.Price,
                Quantity = 1,
                PrinterName = product.PrinterName
            };

            _newItems.Add(newItem);

            var itemComp = new OrderSummaryItem();
            itemComp.btnBossCancel.Visibility = Visibility.Collapsed;
            itemComp.QuantityDecreased += OrderItem_QuantityDecreased;
            itemComp.SeedData(newItem);

            _orderItemComponents[product.Id] = itemComp;
            spNewOrders.Children.Add(itemComp);
        }

        if (_newItems.Count > 0)
        {
            btnbooking.Visibility = Visibility.Collapsed;
            transfer.Visibility = Visibility.Collapsed;
        }

        if (_selectedTable != null)
        {
            if (_selectedTable.Status == TableStatus.Busy && _newItems.Count > 0)
            {
                btnReSendToKitchen.Visibility = Visibility.Collapsed;
            }
        }
    }

    private void OrderItem_QuantityDecreased(object? sender, Guid productId)
    {
        if (_newItems.FirstOrDefault(x => x.ProductId == productId) is { } item)
        {
            item.Quantity--;

            if (item.Quantity <= 0)
            {
                _newItems.Remove(item);
                if (_orderItemComponents.TryGetValue(productId, out var component))
                {
                    spNewOrders.Children.Remove(component);
                    TotalPrice -= item.ProductPrice;
                    _orderItemComponents.Remove(productId);

                    if (spNewOrders.Children.Count == 0 && _selectedTable != null && _selectedTable.Status == TableStatus.Busy)
                    {
                        btnSendToKitchen.Visibility = Visibility.Collapsed;
                        btnPay.Visibility = Visibility.Visible;
                        btnReSendToKitchen.Visibility = Visibility.Visible;
                    }
                }
            }
            else
            {
                // UI ni yangilaymiz
                if (_orderItemComponents.TryGetValue(productId, out var comp))
                {
                    comp.UpdateQuantity(item.Quantity);
                    TotalPrice -= item.ProductPrice;
                }
            }
        }

        txt_totalPrice.Text = TotalPrice.ToString("#,0.##", nfi);
    }

    private async Task PreviousOrderItemQuantityDecreased(Guid orderItemId)
    {
        var order = await _orderService.GetOpenOrderByTableId(_selectedTable!.Id);
        if (_previousItems.FirstOrDefault(x => x.Id == orderItemId) is { } item)
        {
            item.Quantity--;
            var printer = new PrintService();
            printer.PrintCancelledItemChek(order!.TableName, item.ProductName, 1, item.Quantity, item.PrinterName);

            if (item.Quantity <= 0)
            {
                _previousItems.Remove(item);
                if (_previousOrderItemComponents.TryGetValue(orderItemId, out var component))
                {
                    spPreviousOrders.Children.Remove(component);
                    TotalPrice -= item.ProductPrice;
                    _previousOrderItemComponents.Remove(orderItemId);
                    if (spPreviousOrders.Children.Count == 0 && _selectedTable != null && _selectedTable.Status == TableStatus.Busy)
                    {
                        btnPay.Visibility = Visibility.Visible;
                        _previousItems.Clear();
                        _previousOrderItemComponents.Clear();
                        var forDeleteOrder = new EditOrderDto
                        {
                            OrderId = order!.Id,
                            ClosedUserId = SessionManager.CurrentUserId,
                            ClientPhoneNumber = _lotteryPhoneNumber ?? "",
                            Items = _previousItems
                        };
                        await _orderService.CloseOrderAndFreeTable(forDeleteOrder, _selectedTable!.Id);
                        btnReSendToKitchen.Visibility = Visibility.Collapsed;
                        st_queue.Visibility = Visibility.Collapsed;
                        lblPrevious.Visibility = Visibility.Collapsed;
                        txt_total.Visibility = Visibility.Collapsed;
                        txt_totalPrice.Visibility = Visibility.Collapsed;
                        selectedTable = null;
                        _selectedTable = null;
                        await LoadTableCategoriesAsync();
                    }
                    else
                    {
                        await _orderItemService.DeleteAsync(item.Id);
                        await _orderService.UpdateTotalPrice(order!.Id, TotalPrice);
                    }
                }
            }
            else
            {
                // UI ni yangilaymiz
                if (_previousOrderItemComponents.TryGetValue(orderItemId, out var comp))
                {
                    comp.UpdateQuantity(item.Quantity);
                    TotalPrice -= item.ProductPrice;
                }
                await _orderItemService.UpdateAsync(item);
                await _orderService.UpdateTotalPrice(order!.Id, TotalPrice);
            }
        }
        txt_totalPrice.Text = TotalPrice.ToString("#,0.##", nfi);
    }

    private async Task LoadTableCategoriesAsync()
    {
        var categories = await _tableCategoryService.GetAllAsync();
        spTableCategoryButtons.Children.Clear();

        RadioButton firstRb = null;

        foreach (var category in categories)
        {
            var rb = new RadioButton
            {
                Content = category.Name,
                Tag = category.Id,
                Style = (Style)FindResource("RadioCategoryStyle"),
            };

            spTableCategoryButtons.Children.Add(rb);

            if (firstRb == null)
                firstRb = rb;
        }

        if (firstRb != null)
        {
            firstRb.IsChecked = true;
            foreach (RadioButton rb in spTableCategoryButtons.Children.OfType<RadioButton>())
            {
                rb.Checked += async (s, e) =>
                {
                    if (HasPendingOrder())
                    {
                        if (!MessageBoxManager.ShowConfirmation("Buyurtma hali yakunlanmagan. Davom etsangiz, hozirgi buyurtma o'chiriladi. Davom etasizmi?"))
                        {
                            ((RadioButton)s).IsChecked = false;
                            return;
                        }
                    }
                    AllCollapsed();
                    var id = (Guid)((RadioButton)s).Tag;
                    await LoadTablesAsync(id);
                };
            }

            var firstCategoryId = (Guid)firstRb.Tag;
            await LoadTablesAsync(firstCategoryId);
        }
    }

    private async Task LoadCategoriesAsync()
    {
        var categories = await _categoryService.GetAllAsync();
        spCategoryButtons.Children.Clear();

        foreach (var category in categories)
        {
            var rb = new RadioButton
            {
                Content = category.Name,
                Tag = category.Id,
                Style = (Style)FindResource("RadioCategoryStyle"), 
            };

            rb.Checked += async (s, e) =>
            {
                var id = (Guid)((RadioButton)s).Tag;
                tb_search_Product.Clear();
                await LoadProductsByCategory(id);
            };

            spCategoryButtons.Children.Add(rb);
        }


        if (spCategoryButtons.Children.Count > 0 && spCategoryButtons.Children[0] is RadioButton firstRb)
        {
            firstRb.IsChecked = true;
        }

        if (categories.Count == 0)
        {
            EmptyData.Visibility = Visibility.Visible;
            Loader.Visibility = Visibility.Collapsed;
        }
    }

    private async Task CategoryButton_Click(object sender, RoutedEventArgs e)
    {
        foreach (var child in spCategoryButtons.Children)
        {
            if (child is Button btn)
                btn.Style = (Style)FindResource("FilterButton");
        }

        if (sender is Button selectedBtn)
        {
            selectedBtn.Style = (Style)FindResource("SelectedFilterButton");

            // 💡 Kerak bo‘lsa filter qilinayotgan Category Id:
            var categoryId = (Guid)selectedBtn.Tag;

            // Mahsulotlar ro‘yxatini shu yerda filter qilishing mumkin
            await LoadProductsByCategory(categoryId);
        }
    }

    private async Task LoadProductsByCategory(Guid categoryId)
    {
        spProducts.Children.Clear();
        EmptyData.Visibility = Visibility.Collapsed;
        Loader.Visibility = Visibility.Visible;

        var option = new ProductSortFilterOptions
        {
            CategoryId = categoryId,
            IsActive = true

        };
        var products = await _productService.GetAllAsync(option);

        Loader.Visibility = Visibility.Collapsed;
        if(products.Count > 0)
        {
            foreach (var product in products)
            {
                var prod = new ProductCard();
                prod.SetData(product);
                prod.SetProductImage(product.ImagePath);
                prod.OnAddClicked += AddOrderItem;
                spProducts.Children.Add(prod);
            }
        }
        else
            EmptyData.Visibility = Visibility.Visible;
    }

    private void AllCollapsed()
    {
        txt_total.Visibility = Visibility.Collapsed;
        TotalPrice = 0;
        txt_totalPrice.Visibility = Visibility.Collapsed;
        btnReSendToKitchen.Visibility = Visibility.Collapsed;
        transfer.Visibility = Visibility.Collapsed;
        st_queue.Visibility = Visibility.Collapsed;
    }

    private async Task LoadTablesAsync(Guid categoryId)
    {
        wpTables.Children.Clear();
        _newItems.Clear();
        _orderItemComponents.Clear();
        spNewOrders.Children.Clear();
        _previousItems.Clear();
        lblPrevious.Visibility = Visibility.Collapsed;
        scrollPreviousOrders.Visibility = Visibility.Collapsed;
        lblAdditional.Visibility = Visibility.Collapsed;
        btnSendToKitchen.Visibility = Visibility.Collapsed;
        btnPay.Visibility = Visibility.Collapsed;

        var option = new TableSortFilterOption
        {
            CategoryId = categoryId
        };

        var tables = await _tableService.GetAllAsync(option);
        foreach (var table in tables)
        {
            var tableItem = new TableComponent();
            tableItem.SeedData(table);

            tableItem.MouseLeftButtonUp += async (s, e) =>
            {
                if (HasPendingOrder())
                {
                    if (!MessageBoxManager.ShowConfirmation("Buyurtma hali yakunlanmagan. Davom etsangiz, hozirgi buyurtma o‘chiriladi. Davom etasizmi?"))
                    {
                        return;
                    }
                }

                _orderItemComponents.Clear();
                if (selectedTable != null)
                    selectedTable.SetSelected(false);

                selectedTable = tableItem;
                tableItem.SetSelected(true);
                btnTakeAway.Tag = null;
                TakeAway = false;

                txt_total.Visibility = Visibility.Visible;
                txt_totalPrice.Text = TotalPrice.ToString("#,0.##", nfi);
                txt_totalPrice.Visibility = Visibility.Visible;

                var dto = tableItem.DataContext as TableDto;
                if (dto == null) return;

                if (dto.Status == TableStatus.Reserved)
                {
                    st_queue.Visibility = Visibility.Collapsed;
                    txt_totalPrice.Visibility = Visibility.Collapsed;
                    txt_total.Visibility = Visibility.Collapsed;
                    lblPrevious.Visibility = Visibility.Collapsed;
                    scrollPreviousOrders.Visibility = Visibility.Collapsed;
                    btnReSendToKitchen.Visibility = Visibility.Collapsed;
                    lblAdditional.Visibility = Visibility.Collapsed;
                    btnPay.Visibility = Visibility.Collapsed;
                    btnSendToKitchen.Visibility = Visibility.Collapsed;
                    btnbooking.Visibility = Visibility.Collapsed;
                    transfer.Visibility = Visibility.Collapsed;

                    bool cancel = MessageBoxManager.ShowConfirmation(
                        "Stol bandligini bekor qilmoqchimisiz?");
                    if (cancel)
                    {
                        dto.Status = TableStatus.Free;
                        var res = await _tableService.UpdateAsync(dto);
                        if (res)
                        {
                            NotificationManager.ShowNotification(NotificationWindow.MessageType.Success,
                                                "Stol bandligi bekor qilindi",
                                                NotificationWindow.NotificationPosition.TopRight);
                            btnbooking.Visibility = Visibility.Collapsed;
                            _selectedTable = null;
                            selectedTable = null;
                        }
                        else
                        {
                            NotificationManager.ShowNotification(NotificationWindow.MessageType.Error,
                                "Stol bandligini bekor qilishda xatolik yuz berdi",
                                NotificationWindow.NotificationPosition.TopRight);
                        }
                        await LoadTableCategoriesAsync();
                        return;
                    }
                    else
                    {
                        txt_totalPrice.Visibility = Visibility.Visible;
                        txt_total.Visibility = Visibility.Visible;
                        TotalPrice = 0;
                        btnbooking.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    TotalPrice = 0;
                }

                await LoadOrderSummaryForTable(dto);
            };

            wpTables.Children.Add(tableItem);
        }
    }

    private async Task LoadOrderSummaryForTable(TableDto table)
    {
        _selectedTable = table;
        _currentOrder = null;
        _previousItems.Clear();
        _newItems.Clear();
        spPreviousOrders.Children.Clear();
        spNewOrders.Children.Clear();

        lblPrevious.Visibility = Visibility.Collapsed;
        scrollPreviousOrders.Visibility = Visibility.Collapsed;
        lblAdditional.Visibility = Visibility.Collapsed;
        btnSendToKitchen.Visibility = Visibility.Collapsed;
        btnPay.Visibility = Visibility.Collapsed;

        if (table.Status == TableStatus.Busy)
        {
            _currentOrder = await _orderService.GetOpenOrderByTableId(table.Id);
            if (_currentOrder is not null)
            {
                foreach (var item in _currentOrder.Items)
                {
                    _previousItems.Add(item);

                    var itemCard = new OrderSummaryItem();
                    itemCard.SeedData(item);

                    itemCard.btnCancel.Visibility = Visibility.Collapsed;
                    itemCard.btnBossCancel.Visibility = Visibility.Visible;

                    itemCard.BossPasswordEntered += async (s, password) =>
                    {
                        var isBoss = _authService.CheckBossPassword(password);
                        if (!isBoss)
                        {
                            NotificationManager.ShowNotification(NotificationWindow.MessageType.Error, "Parol noto‘g‘ri yoki sizda huquq yo‘q!");
                            return;
                        }

                        await PreviousOrderItemQuantityDecreased(item.Id);
                        NotificationManager.ShowNotification(NotificationWindow.MessageType.Success, "Mahsulot bekor qilindi.");
                    };

                    _previousOrderItemComponents[item.Id] = itemCard;
                    spPreviousOrders.Children.Add(itemCard);
                    TotalPrice += item.TotalPrice;
                }

                if (_previousItems.Count > 0)
                {
                    lblPrevious.Visibility = Visibility.Visible;
                    scrollPreviousOrders.Visibility = Visibility.Visible;
                }

                lblqueue.Text = _currentOrder.QueueNumber.ToString();
                lblAdditional.Visibility = Visibility.Visible;
            }

            lblPrevious.Visibility = Visibility.Visible;
            btnPay.Visibility = Visibility.Visible;
            btnbooking.Visibility = Visibility.Collapsed;
            ScrollSpNewOredrs.MaxHeight = 200;
            btnReSendToKitchen.Visibility = Visibility.Visible;
            transfer.Visibility = Visibility.Visible;
            lblAdditional.Text = "Qo'shimcha mahsulotlar";
            st_queue.Visibility = Visibility.Visible;
        }
        else if (table.Status == TableStatus.Free)
        {
            lblAdditional.Visibility = Visibility.Visible;
            btnSendToKitchen.Visibility = Visibility.Visible;
            btnbooking.Visibility = Visibility.Visible;
            ScrollSpNewOredrs.MaxHeight = 400;
            btnReSendToKitchen.Visibility = Visibility.Collapsed;
            lblAdditional.Text = "Mahsulotlar";
            st_queue.Visibility = Visibility.Collapsed;
            transfer.Visibility = Visibility.Collapsed;
        }
        else
        {
            lblAdditional.Visibility = Visibility.Visible;
            btnSendToKitchen.Visibility = Visibility.Visible;
            ScrollSpNewOredrs.MaxHeight = 400;
            btnReSendToKitchen.Visibility = Visibility.Collapsed;
            lblAdditional.Text = "Mahsulotlar";
            st_queue.Visibility = Visibility.Collapsed;
        }

        txt_totalPrice.Text = TotalPrice.ToString("#,0.##", nfi);
    }

    private void Takeaway_Click(object sender, RoutedEventArgs e)
    {
        if (HasPendingOrder())
        {
            if (!MessageBoxManager.ShowConfirmation("Buyurtma hali yakunlanmagan. Davom etsangiz, hozirgi buyurtma o‘chiriladi. Davom etasizmi?"))
            {
                return;
            }
        }

        _newItems.Clear();
        _orderItemComponents.Clear();
        spNewOrders.Children.Clear();
        lblPrevious.Visibility = Visibility.Collapsed;
        lblAdditional.Visibility = Visibility.Visible;
        TotalPrice = 0;
        txt_totalPrice.Text = TotalPrice.ToString("#,0.##", nfi);
        txt_total.Visibility = Visibility.Visible;
        txt_totalPrice.Visibility = Visibility.Visible;
        btnReSendToKitchen.Visibility = Visibility.Collapsed;
        lblAdditional.Text = "Mahsulotlar";
        st_queue.Visibility = Visibility.Collapsed;
        transfer.Visibility = Visibility.Collapsed;

        if (selectedTable != null)
        {
            selectedTable.SetSelected(false);
            btnSendToKitchen.Visibility = Visibility.Collapsed;
            btnbooking.Visibility = Visibility.Collapsed;

            _previousItems.Clear();
            _newItems.Clear();
            spPreviousOrders.Children.Clear();
            spNewOrders.Children.Clear();
            lblPrevious.Visibility = Visibility.Collapsed;
        }

        lblAdditional.Visibility = Visibility.Visible;
        btnPay.Visibility = Visibility.Visible;
        btnTakeAway.Tag = "Selected";
        TakeAway = true;
    }

    private async void btnbooking_Click(object sender, RoutedEventArgs e)
    {
        _selectedTable!.Status = TableStatus.Reserved;
        TotalPrice = 0;
        var result = await _tableService.UpdateAsync(_selectedTable);
        if (result)
        {
            NotificationManager.ShowNotification(NotificationWindow.MessageType.Success,
                "Stol muvaffaqiyatli band qilindi!",
                NotificationWindow.NotificationPosition.TopRight);
            btnbooking.Visibility = Visibility.Collapsed;
            
            await LoadTableCategoriesAsync(); // UI yangilash
        }
        else
        {
            NotificationManager.ShowNotification(NotificationWindow.MessageType.Error,
                "Stolni band qilishda xatolik yuz berdi.",
                NotificationWindow.NotificationPosition.TopRight);
        }
        _selectedTable = null;
        selectedTable = null;
        txt_total.Visibility = Visibility.Collapsed;
        txt_totalPrice.Visibility = Visibility.Collapsed;
    }

    private Dictionary<string, List<OrderItemDto>> GroupOrdersByPrinter(List<OrderItemDto> items)
    {
        var printerGroups = new Dictionary<string, List<OrderItemDto>>();

        foreach (var item in items)
        {
            if (!string.IsNullOrEmpty(item.PrinterName) && item.PrinterName != "Printersiz")
            {
                if (!printerGroups.ContainsKey(item.PrinterName))
                {
                    printerGroups[item.PrinterName] = new List<OrderItemDto>();
                }
                printerGroups[item.PrinterName].Add(item);
            }
        }

        return printerGroups;
    }

    private async void btnSendToKitchen_Click(object sender, RoutedEventArgs e)
    {
        if (selectedTable == null)
        {
            NotificationManager.ShowNotification(
                NotificationWindow.MessageType.Warning,
                "Iltimos, stolni tanlang!",
                NotificationWindow.NotificationPosition.TopRight);
            return;
        }

        if (_newItems.Count == 0)
        {
            NotificationManager.ShowNotification(
                NotificationWindow.MessageType.Warning,
                "Iltimos, hech bo‘lmaganda bitta mahsulot tanlang!",
                NotificationWindow.NotificationPosition.TopRight);
            return;
        }

        var tableDto = (selectedTable.DataContext as TableDto)!;

        if (tableDto.Status == TableStatus.Free || tableDto.Status == TableStatus.Reserved)
        {
            var dto = new AddOrderDto
            {
                TableId = tableDto.Id,
                TableName = tableDto.Name,
                Items = _newItems,
                TransactionId = DateTimeUzb.UZBTime.ToString("ddMMyyyy"),
                OrderedUserId = SessionManager.CurrentUserId
            };

            var result = await _orderService.CreateAsync(dto);

            if (result.Item1)
            {
                var order_price = dto.Items.Sum(x => x.ProductPrice);

                var printerGroups = GroupOrdersByPrinter(dto.Items);

                foreach (var printerGroup in printerGroups)
                {
                    if (printerGroup.Value.Count > 0)
                    {
                        PrintService printService = new PrintService();
                        printService.PrintChefChek(
                            new AddOrderDto { Items = printerGroup.Value, TableName = tableDto.Name },
                            result.Item2,
                            printerGroup.Key // printer nomi
                        );
                    }
                }

                NotificationManager.ShowNotification(
                    NotificationWindow.MessageType.Success,
                    "Buyurtma oshpazga yuborildi!",
                    NotificationWindow.NotificationPosition.TopRight);

                spNewOrders.Children.Clear();
                _orderItemComponents.Clear();
                _newItems.Clear();
                selectedTable.SetSelected(false);
                selectedTable = null;
                txt_total.Visibility = Visibility.Collapsed;
                txt_totalPrice.Visibility = Visibility.Collapsed;
                await LoadTableCategoriesAsync();
            }
            else
            {
                NotificationManager.ShowNotification(
                    NotificationWindow.MessageType.Error,
                    "Buyurtmani saqlashda xatolik yuz berdi!",
                    NotificationWindow.NotificationPosition.TopRight);
            }
        }
        else if (tableDto.Status == TableStatus.Busy)
        {
            st_queue.Visibility = Visibility.Collapsed;
            var order = await _orderService.GetOpenOrderByTableId(tableDto.Id);

            if (order == null)
            {
                NotificationManager.ShowNotification(
                    NotificationWindow.MessageType.Error,
                    "Mavjud ochiq buyurtma topilmadi!",
                    NotificationWindow.NotificationPosition.TopRight);
                return;
            }

            var newOrderItems = _newItems.Select(i => new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = i.ProductId,
                ProductPrice = i.ProductPrice,
                Quantity = i.Quantity
            }).ToList();

            var success = await _orderItemService.AddManyAsync(newOrderItems);

            if (success)
            {
                var res = await _orderService.UpdateTotalPrice(order.Id, order.TotalPrice + _newItems.Sum(i => i.TotalPrice));

                if (res)
                {
                    var kitchenOrders = GroupOrdersByPrinter(_newItems);

                    foreach (var printerGroup in kitchenOrders)
                    {
                        if (printerGroup.Value.Count > 0)
                        {
                            PrintService printService = new PrintService();
                            printService.PrintChefChek(
                                new AddOrderDto { Items = printerGroup.Value, TableName = order.TableName },
                                order.QueueNumber,
                                printerGroup.Key // printer nomi
                            );
                        }
                    }

                    NotificationManager.ShowNotification(
                    NotificationWindow.MessageType.Success,
                    "Buyurtma oshpazga yuborildi!",
                    NotificationWindow.NotificationPosition.TopRight);

                }

                // UI tozalash
                spNewOrders.Children.Clear();
                _orderItemComponents.Clear();
                _newItems.Clear();
                selectedTable.SetSelected(false);
                selectedTable = null;
                txt_total.Visibility = Visibility.Collapsed;
                txt_totalPrice.Visibility = Visibility.Collapsed;
                await LoadTableCategoriesAsync();
            }
            else
            {
                NotificationManager.ShowNotification(
                    NotificationWindow.MessageType.Error,
                    "Yangi mahsulotlarni qo‘shishda xatolik!",
                    NotificationWindow.NotificationPosition.TopRight);
            }
        }
    }

    private void btnPay_Click(object sender, RoutedEventArgs e)
    {
        if (TakeAway && _newItems.Count == 0)
        {
            NotificationManager.ShowNotification(
                NotificationWindow.MessageType.Warning,
                "Iltimos, hech bo‘lmaganda bitta mahsulot tanlang!",
                NotificationWindow.NotificationPosition.TopRight);
            return;
        }

        if (LotteryManager.IsLotteryModeEnabled)
        {
            var phoneWindow = new EnterPhoneWindow();
            var result = phoneWindow.ShowDialog();

            if (result == true)
            {
                _lotteryPhoneNumber = phoneWindow.PhoneNumber;
            }
            else
            {
                return;
            }
        }

        ColculatorWindow colculatorWindow = new ColculatorWindow();
        colculatorWindow.TakenAwayChanged += ColculatorWindow_TakenAwayChanged;
        colculatorWindow.CheckPrinted += CheckPrint;
        colculatorWindow.OrderPrice = TotalPrice;
        colculatorWindow.ShowDialog();
        colculatorWindow.TakenAwayChanged -= ColculatorWindow_TakenAwayChanged;
    }

    private async void CheckPrint(object? sender, EventArgs e)
    {
        if (_selectedTable == null)
        {
            NotificationManager.ShowNotification(
                NotificationWindow.MessageType.Warning,
                "Stol tanlanmagan!",
                NotificationWindow.NotificationPosition.TopRight);
            return;
        }

        var openOrder = await _orderService.GetOpenOrderByTableId(_selectedTable.Id);
        if (openOrder is null)
        {
            NotificationManager.ShowNotification(
                NotificationWindow.MessageType.Error,
                "Ushbu stol uchun ochiq buyurtma topilmadi!",
                NotificationWindow.NotificationPosition.TopRight);
            return;
        }

        PrintService printService = new PrintService();
        printService.PrintUserChek(openOrder, openOrder.TotalPrice, TotalPrice);
    }

    private async void ColculatorWindow_TakenAwayChanged(object? sender, string? password)
    {
        if (TakeAway)
        {
            // 🟠 Olib ketish (Takeaway) uchun buyurtma yaratish
            var takeawayTable = await _tableService.GetTakeAwayAsync();

            if (takeawayTable is null)
            {
                NotificationManager.ShowNotification(
                    NotificationWindow.MessageType.Error,
                    "Olib ketish stoli topilmadi!",
                    NotificationWindow.NotificationPosition.TopRight);
                return;
            }

            var dto = new AddOrderDto
            {
                TableId = takeawayTable.Id,
                Items = _newItems,
                TableName = "Olib ketish",
                TransactionId = DateTimeUzb.UZBTime.ToString("ddMMyyyy"),
                ClientPhoneNumber = _lotteryPhoneNumber ?? "",
                OrderedUserId = SessionManager.CurrentUserId,
                ClosedUserId = SessionManager.CurrentUserId
            };

            if (password != null)
            {
                var isBoss = _authService.CheckBossPassword(password);
                if (!isBoss)
                {
                    NotificationManager.ShowNotification(NotificationWindow.MessageType.Error, "Parol noto‘g‘ri yoki sizda huquq yo‘q!");
                    return;
                }

                if (sender is ColculatorWindow colculatorWindow)
                {
                    colculatorWindow.Close();
                }

                dto.Status = OrderStatus.Free;
                PaymentPrice = TotalPrice;
            }
            else
            {
                dto.Status = OrderStatus.Closed;
            }

            var result = await _orderService.CreateAsync(dto);

            if (result.Item1)
            {
                var order_price = dto.Items.Sum(x => x.TotalPrice);
                var kitchenOrders = GroupOrdersByPrinter(dto.Items);
                PrintService printService = new PrintService();

                foreach (var printerGroup in kitchenOrders)
                {
                    if (printerGroup.Value.Count > 0)
                    {
                        printService.PrintChefChek(
                            new AddOrderDto { Items = printerGroup.Value, TableName = dto.TableName },
                            result.Item2,
                            printerGroup.Key // printer nomi
                        );
                    }
                }

                if (LotteryManager.IsLotteryModeEnabled && !string.IsNullOrEmpty(_lotteryPhoneNumber))
                {
                    printService.PrintLotteryChek(
                        phoneNumber: _lotteryPhoneNumber,
                        totalPrice: order_price,
                        transactionId: dto.TransactionId + _orderService.GetTodayQueueNumber().ToString("D4")
                    );
                }

                if (dto.Status != OrderStatus.Free)
                {
                    printService.PrintUserChek(new OrderDto { Items = _newItems,
                        TableName = dto.TableName,
                        TransactionId = dto.TransactionId + _orderService.GetTodayQueueNumber().ToString("D4") },
                        order_price, PaymentPrice);
                }

                NotificationManager.ShowNotification(
                    NotificationWindow.MessageType.Success,
                    "Buyurtma qabul qilindi!",
                    NotificationWindow.NotificationPosition.TopRight);
            }
            else
            {
                NotificationManager.ShowNotification(
                    NotificationWindow.MessageType.Error,
                    "Buyurtma yaratishda xatolik yuz berdi.",
                    NotificationWindow.NotificationPosition.TopRight);
            }
        }
        else
        {
            if (_selectedTable == null)
            {
                NotificationManager.ShowNotification(
                    NotificationWindow.MessageType.Warning,
                    "Stol tanlanmagan!",
                    NotificationWindow.NotificationPosition.TopRight);
                return;
            }

            var openOrder = await _orderService.GetOpenOrderByTableId(_selectedTable.Id);
            if (openOrder is null)
            {
                NotificationManager.ShowNotification(
                    NotificationWindow.MessageType.Error,
                    "Ushbu stol uchun ochiq buyurtma topilmadi!",
                    NotificationWindow.NotificationPosition.TopRight);
                return;
            }

            var updateOrder = new EditOrderDto
            {
                OrderId = openOrder.Id,
                ClosedUserId = SessionManager.CurrentUserId,
                ClientPhoneNumber = _lotteryPhoneNumber ?? "",
                Items = _previousItems
            };

            if (password != null)
            {
                var isBoss = _authService.CheckBossPassword(password);
                if (!isBoss)
                {
                    NotificationManager.ShowNotification(NotificationWindow.MessageType.Error, "Parol noto‘g‘ri yoki sizda huquq yo‘q!");
                    return;
                }

                if (sender is ColculatorWindow colculatorWindow)
                {
                    colculatorWindow.Close();
                }
                updateOrder.Status = OrderStatus.Free;
                PaymentPrice = TotalPrice;
            }
            else
            {
                updateOrder.Status = OrderStatus.Closed;
            }

            var result = await _orderService.CloseOrderAndFreeTable(updateOrder, _selectedTable.Id);


            if (result)
            {
                PrintService printService = new PrintService();

                if (LotteryManager.IsLotteryModeEnabled && !string.IsNullOrEmpty(_lotteryPhoneNumber))
                {
                    printService.PrintLotteryChek(
                        phoneNumber: _lotteryPhoneNumber,
                        totalPrice: openOrder.TotalPrice,
                        transactionId: openOrder.TransactionId
                    );
                }

                //if (updateOrder.Status != OrderStatus.Free)
                //{
                //    printService.PrintUserChek(openOrder, openOrder.TotalPrice, PaymentPrice);
                //}

                NotificationManager.ShowNotification(
                    NotificationWindow.MessageType.Success,
                    "Buyurtma to‘landi va stol bo‘shatildi!",
                    NotificationWindow.NotificationPosition.TopRight);

                selectedTable?.SetSelected(false);
            }
            else
            {
                NotificationManager.ShowNotification(
                    NotificationWindow.MessageType.Error,
                    "Buyurtmani yopishda xatolik yuz berdi!",
                    NotificationWindow.NotificationPosition.TopRight);
            }

        }


        spNewOrders.Children.Clear();
        _orderItemComponents.Clear();
        spPreviousOrders.Children.Clear();
        lblPrevious.Visibility = Visibility.Collapsed;
        scrollPreviousOrders.Visibility = Visibility.Collapsed;
        txt_total.Visibility = Visibility.Collapsed;
        txt_totalPrice.Visibility = Visibility.Collapsed;
        lblAdditional.Visibility = Visibility.Collapsed;
        btnPay.Visibility = Visibility.Collapsed;
        btnSendToKitchen.Visibility = Visibility.Collapsed;
        btnReSendToKitchen.Visibility = Visibility.Collapsed;
        _selectedTable = null;
        selectedTable = null;
        btnTakeAway.Tag = null;
        TakeAway = false;
        st_queue.Visibility = Visibility.Collapsed;

        await LoadTableCategoriesAsync();
    }

    private async void btnResSendToKitchen_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedTable == null || selectedTable == null)
        {
            NotificationManager.ShowNotification(
                NotificationWindow.MessageType.Error,
                "Stol tanlanmagan!",
                NotificationWindow.NotificationPosition.TopRight);
            return;
        }

        if (_previousItems.Count == 0)
        {
            NotificationManager.ShowNotification(
                NotificationWindow.MessageType.Warning,
                "Oshpaz uchun mahsulot yo'q!",
                NotificationWindow.NotificationPosition.TopRight);
            return;
        }

        var tableDto = (selectedTable.DataContext as TableDto);

        if (tableDto == null)
        {
            NotificationManager.ShowNotification(
                NotificationWindow.MessageType.Error,
                "Stol ma'lumotlari topilmadi!",
                NotificationWindow.NotificationPosition.TopRight);
            return;
        }

        var order = await _orderService.GetOpenOrderByTableId(tableDto.Id);

        if (order == null)
        {
            NotificationManager.ShowNotification(
                NotificationWindow.MessageType.Error,
                "Ochiq buyurtma topilmadi!",
                NotificationWindow.NotificationPosition.TopRight);
            return;
        }

        var kitchenOrders = GroupOrdersByPrinter(_previousItems);
        if (kitchenOrders.Count > 0)
        {
            foreach (var printerGroup in kitchenOrders)
            {
                if (printerGroup.Value.Count > 0)
                {
                    PrintService printService = new PrintService();
                    printService.PrintChefChek(
                        new AddOrderDto { Items = printerGroup.Value, TableName = order.TableName },
                        order.QueueNumber,
                        printerGroup.Key // printer nomi
                    );
                }
            }
            NotificationManager.ShowNotification(
                NotificationWindow.MessageType.Success,
                "Oshpazga buyurtma yuborildi!",
                NotificationWindow.NotificationPosition.TopRight);
        }
        else
        {
            NotificationManager.ShowNotification(
                NotificationWindow.MessageType.Warning,
                "Oshpaz uchun mahsulot yo'q!",
                NotificationWindow.NotificationPosition.TopRight);
        }
    }

    private void logout_button_Click(object sender, RoutedEventArgs e)
    {
        if (HasPendingOrder())
        {
            NotificationManager.ShowNotification(
                NotificationWindow.MessageType.Warning,
                "Buyurtma hali yakunlanmagan. Iltimis, buyurtmani yakunlang.",
                NotificationWindow.NotificationPosition.TopCenter, 4000);

            return;
        }

        if (MessageBoxManager.ShowConfirmation("Akkauntdan chiqishni xohlaysizmi?"))
        {
            SessionManager.Reset();

            var loginWindow = new LoginWindow();
            loginWindow.Show();

            var currentWindow = Window.GetWindow(this);
            currentWindow?.Close();
        }
    }

    private async void tb_search_ProductTextChanged(object sender, TextChangedEventArgs e)
    {
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        var token = _searchCts.Token;

        string keyword = tb_search_Product.Text.Trim();

        try
        {
            await Task.Delay(500, token);
        }
        catch { return; }

        if (token.IsCancellationRequested) return;

        if (string.IsNullOrWhiteSpace(keyword))
        {
            if (spCategoryButtons.Children.OfType<RadioButton>().FirstOrDefault(x => x.IsChecked == true) is RadioButton selected)
            {
                var catId = (Guid)selected.Tag;
                await LoadProductsByCategory(catId);
            }
            return;
        }

        spProducts.Children.Clear();
        Loader.Visibility = Visibility.Visible;
        EmptyData.Visibility = Visibility.Collapsed;

        var result = await _productService.GetAllAsync(new ProductSortFilterOptions
        {
            Name = keyword,
            IsActive = true
        });

        Loader.Visibility = Visibility.Collapsed;

        if (result.Count > 0)
        {
            foreach (var product in result)
            {
                var prod = new ProductCard();
                prod.SetData(product);
                prod.SetProductImage(product.ImagePath);
                prod.OnAddClicked += AddOrderItem;
                spProducts.Children.Add(prod);
            }
        }
        else
        {
            EmptyData.Visibility = Visibility.Visible;
        }
    }

    public bool HasPendingOrder()
    {
        return _newItems.Count > 0;
    }

    private void tb_search_Product_Loaded(object sender, RoutedEventArgs e)
    {
        var textBox = sender as TextBox;
        var clearButton = textBox?.Template?.FindName("ClearButton", textBox) as Button;

        if (clearButton != null)
        {
            clearButton.Click += ClearButton_Click;
        }
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        tb_search_Product.Clear();
        tb_search_Product.Focus();

        if (spCategoryButtons.Children.Count > 0 && spCategoryButtons.Children[0] is RadioButton firstRb)
        {
            firstRb.IsChecked = true;
        }
    }

    private void transfer_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedTable == null)
        {
            NotificationManager.ShowNotification(
                NotificationWindow.MessageType.Error,
                "Stol tanlanmagan!",
                NotificationWindow.NotificationPosition.TopRight);
            return;
        }

        var table = new TableTransferWindow(_selectedTable);
        table.tableIdChanged += transferClicked;
        table.ShowDialog();
    }

    private async void transferClicked(object? sender, Guid newTableId)
    {
        var order = await _orderService.GetOpenOrderByTableId(_selectedTable!.Id);
        if (order == null)
        {
            NotificationManager.ShowNotification(NotificationWindow.MessageType.Error, "Bunday buyurtma topilmadi!");
            return;
        }

        var result = await _orderService.ChangeTableIdAsync(order.Id, _selectedTable.Id, newTableId);

        if (result)
        {
            NotificationManager.ShowNotification(NotificationWindow.MessageType.Success, "Buyurtma boshqa stolga ko'chirildi!");
        }

        await RefreshPageDataAsync();
    }

    private async Task RefreshPageDataAsync()
    {
        await LoadTableCategoriesAsync();

        await LoadCategoriesAsync();

        AllCollapsed();

        lblAdditional.Visibility = Visibility.Visible;

        user_name.Text = SessionManager.FirstName + " | " + SessionManager.Role;
    }
}
