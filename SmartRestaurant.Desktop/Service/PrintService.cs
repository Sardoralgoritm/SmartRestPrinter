using ESC_POS_USB_NET.Printer;
using SmartRestaurant.BusinessLogic.Services.Orders.DTOs;
using SmartRestaurant.Desktop.Windows.Extensions;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Management;
using System.Text;
using static SmartRestaurant.Desktop.Windows.Extensions.NotificationWindow;

namespace SmartRestaurant.Desktop.Service;

public class PrintService : IDisposable
{
    private readonly string USER_PRINTER_NAME = Properties.Settings.Default.UserPrinterName;
    private readonly string PHONE_NUMBER = Properties.Settings.Default.PhoneNumber;
    Printer? printer;

    public void PrintChefChek(AddOrderDto dto, int queue, string printerName)
    {
        printer = new Printer(printerName, "UTF-8");
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        printer.AlignCenter();
        printer.Append(new byte[] { 0x1D, 0x21, 0x33 });
        printer.BoldMode(dto.TableName);
        printer.Append("\n");
        printer.Separator();

        int c = 0;
        foreach (var item in dto.Items)
        {
            c++;
            string text = $"{item.ProductName}";
            int strLength = 15 - text.Length;
            for (int i = 1; i <= strLength; i++)
            {
                text += " ";
            }
            string temp = $"   {item.Quantity} ta";
            text += temp;
            printer.AlignLeft();
            printer.DoubleWidth2();
            printer.Append(text);
            printer.Append("\n");
            if (dto.Items.Count != c)
            {
                printer.Separator();
            }
        }

        printer.Separator();
        
        printer.Append("\n");
        printer.AlignLeft();
        printer.DoubleWidth2();
        printer.Append($"Navbat raqami:   {queue}");
        printer.Append("\n");
        printer.NormalWidth();
        printer.Append($"Buyurtma vaqti:       " + DateTime.Now.ToString("yyyy-MM-dd HH:mm"));

        printer.Append("\n");

        if (!string.IsNullOrEmpty(printerName) && IsPrinterAvailable(printerName))
        {
            printer.FullPaperCut();
            printer.PrintDocument();
        }
        else
            NotificationManager.ShowNotification(MessageType.Error, "Oshpaz uchun printer tanlanmagan.");
    }

    public void PrintUserChek(OrderDto dto, double order_price, double payment_price)
    {
        printer = new Printer(USER_PRINTER_NAME, "UTF-8");
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"logo.png");

        using (var image = new Bitmap(imagePath))
        {
            printer.AlignCenter();
            printer.Image(image);
        }
        printer.Separator();


        printer.Append("\n");
        printer.AlignCenter();
        printer.NormalWidth();
        printer.BoldMode("Manzil:");
        printer.BoldMode("Urgut krug, SXF binosi, 1-qavat");
        printer.Append("\n");
        printer.Separator();
        printer.Append("\n");
        printer.BoldMode($"Buyurtma uchun tel: {PHONE_NUMBER}");
        printer.Append("\n");
        printer.Separator();
        printer.Append("\n");
        printer.AlignCenter();
        printer.DoubleWidth2();
        printer.BoldMode(dto.TableName);
        printer.Append("\n");
        printer.Separator();

        int c = 0;
        foreach (var item in dto.Items)
        {
            c++;
            string text = $"{item.ProductName}";
            int strLength = 18 - text.Length;
            for (int i = 1; i <= strLength; i++)
            {
                text += " ";
            }
            string temp = $"  {item.Quantity} * ";
            text += temp;
            strLength = 1 - temp.Length;
            for (int i = 0; i < strLength; i++)
            {
                text += " ";
            }
            text += item.TotalPrice.ToString();
            printer.AlignLeft();
            printer.Append(text);

            if (dto.Items.Count != c)
            {
                printer.Append("\n");
            }
        }

        printer.Separator();

        printer.Append("\n");
        printer.AlignLeft();
        printer.NormalWidth();
        printer.Append($"Jami summa:       {order_price}");
        printer.Append($"Berilgan summa:   {payment_price}");
        printer.Append($"Qaytim summasi:   {payment_price - order_price}");
        printer.Append("\n");
        printer.Append($"Sana:  " + DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
        printer.Append("ID:   " + dto.TransactionId);

        printer.Append("\n");

        printer.AlignCenter();
        printer.Append("Smart Partners");
        printer.Append("Biz bilan biznesingizni avtomatlashtiring.");
        printer.Append("Murojat uchun tel: +998996661132");
        printer.Append("\n");
        printer.Append("\n");

        if (!string.IsNullOrEmpty(USER_PRINTER_NAME) && IsPrinterAvailable(USER_PRINTER_NAME))
        {
            printer.FullPaperCut();
            printer.PrintDocument();
        }
        else
            NotificationManager.ShowNotification(MessageType.Error, "Kassa uchun printer tanlanmagan.");
    }

    public void Dispose()
         => GC.SuppressFinalize(this);

    public void PrintLotteryChek(string phoneNumber, double totalPrice, string transactionId)
    {
        var printer = new Printer(USER_PRINTER_NAME, "UTF-8");
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        printer.AlignCenter();
        printer.DoubleWidth2();
        printer.Append("LOTOREYA CHEKI");
        printer.Separator();

        printer.NormalWidth();
        printer.AlignLeft();
        printer.Append($"Telefon: {phoneNumber}");
        printer.Append($"Umumiy: {totalPrice} so'm");
        printer.Append($"ID: {transactionId}");

        printer.Separator();
        printer.AlignCenter();
        printer.Append("Chekni barabanga tashlang!");
        printer.Append("\n\n");

        if (!string.IsNullOrEmpty(USER_PRINTER_NAME) && IsPrinterAvailable(USER_PRINTER_NAME))
        {
            printer.FullPaperCut();
            printer.PrintDocument();
        }
        else
            NotificationManager.ShowNotification(MessageType.Error, "Kassa uchun printer tanlanmagan.");
    }

    private bool IsPrinterAvailable(string printerName)
    {
        try
        {
            string query = $"SELECT * FROM Win32_Printer WHERE Name = '{printerName.Replace("\\", "\\\\")}'";
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                var printers = searcher.Get();
                foreach (ManagementObject printer in printers)
                {
                    // Printer holatini tekshirish
                    var status = printer["PrinterStatus"];
                    var state = printer["PrinterState"];

                    // Agar printer offline yoki xato holatda bo'lsa
                    if (status != null && (int)status == 7) // Offline
                        return false;

                    return true;
                }
            }
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }
}