using ESC_POS_USB_NET.Printer;
using SmartRestaurant.BusinessLogic.Services.Orders.DTOs;
using SmartRestaurant.Desktop.Helpers.Session;
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

        if (!string.IsNullOrEmpty(printerName))
        {
            printer.FullPaperCut();
            printer.PrintDocument();
        }
        else
            NotificationManager.ShowNotification(MessageType.Error, "Oshpaz uchun printer tanlanmagan.");
    }

    public void PrintCancelledItemChek(string tableName, string productName, int cancelledQuantity, double remainingQuantity, string printerName)
    {
        printer = new Printer(printerName, "UTF-8");
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        printer.AlignCenter();
        printer.Append(new byte[] { 0x1D, 0x21, 0x22 });
        printer.BoldMode("BEKOR QILINDI");
        printer.Append("\n");
        printer.Separator();

        // Stol nomi
        printer.AlignCenter();
        printer.DoubleWidth2();
        printer.BoldMode(tableName);
        printer.Append("\n");
        printer.Separator();

        // Bekor qilingan mahsulot
        printer.AlignLeft();
        printer.DoubleWidth2();
        printer.BoldMode("❌ BEKOR QILINDI:");
        printer.Append("\n");
        printer.Append($"{productName}");
        printer.Append("\n");
        printer.BoldMode($"Miqdori: {cancelledQuantity} ta");
        printer.Append("\n");
        printer.Separator();

        // Qolgan miqdor (agar bor bo'lsa)
        if (remainingQuantity > 0)
        {
            printer.AlignLeft();
            printer.DoubleWidth2();
            printer.BoldMode("✅ QOLGAN:");
            printer.Append("\n");
            printer.Append($"{productName}");
            printer.Append("\n");
            printer.BoldMode($"Miqdori: {remainingQuantity} ta");
            printer.Append("\n");
            printer.Separator();
        }
        else
        {
            printer.AlignCenter();
            printer.DoubleWidth2();
            printer.BoldMode("BUTUNLAY BEKOR QILINDI!");
            printer.Append("\n");
            printer.Separator();
        }

        // Vaqt va qo'shimcha ma'lumotlar
        printer.Append("\n");
        printer.AlignLeft();
        printer.NormalWidth();
        printer.Append($"Bekor qilish vaqti: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
        printer.Append("\n");
        printer.Append($"Kassir: {SessionManager.FirstName}");
        printer.Append("\n");

        if (!string.IsNullOrEmpty(printerName))
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

        // Logo
        var imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"logo.png");
        using (var image = new Bitmap(imagePath))
        {
            printer.AlignCenter();
            printer.Image(image);
        }

        // Header bo'limi
        printer.Separator();
        printer.Append("\n");
        printer.AlignCenter();
        printer.NormalWidth();
        printer.BoldMode("SMART RESTAURANT");
        printer.Append("\n");
        printer.BoldMode("Manzil: Urgut krug, SXF binosi, 1-qavat");
        printer.Append("\n");
        printer.BoldMode($"Tel: {PHONE_NUMBER}");
        printer.Append("\n");
        printer.Separator();

        // Buyurtma ma'lumotlari
        printer.Append("\n");
        printer.AlignCenter();
        printer.DoubleWidth2();
        printer.BoldMode(dto.TableName);
        printer.Append("\n");
        printer.NormalWidth();
        printer.Append($"Chek ID: {dto.TransactionId}");
        printer.Append("\n");
        printer.Separator();

        // Mahsulotlar ro'yxati
        printer.Append("\n");
        printer.AlignLeft();
        printer.BoldMode("MAHSULOT             MIQDOR   SUMMA");
        printer.Append("\n");
        printer.Separator();

        int c = 0;
        foreach (var item in dto.Items)
        {
            c++;
            string productName = item.ProductName.Length > 18
                ? item.ProductName.Substring(0, 15) + "..."
                : item.ProductName;

            string line = string.Format("{0,-18} {1,3} x {2,8:N0}",
                productName,
                item.Quantity,
                item.TotalPrice);

            printer.AlignLeft();
            printer.Append(line);
            if (dto.Items.Count != c)
            {
                printer.Append("\n");
            }
        }

        // Hisob-kitob bo'limi
        printer.Append("\n");
        printer.Separator();
        printer.Append("\n");
        printer.AlignLeft();
        printer.BoldMode(string.Format("{0,-20} {1,12:N0}", "JAMI SUMMA:", order_price));
        printer.Append("\n");
        printer.Append(string.Format("{0,-20} {1,12:N0}", "Berilgan summa:", payment_price));
        printer.Append("\n");
        printer.BoldMode(string.Format("{0,-20} {1,12:N0}", "QAYTIM:", payment_price - order_price));
        printer.Append("\n");
        printer.Separator();

        // Kassir va vaqt ma'lumotlari
        printer.Append("\n");
        printer.AlignLeft();
        printer.NormalWidth();
        printer.Append($"Sana: {DateTime.Now:yyyy-MM-dd HH:mm}");
        printer.Append("\n");
        printer.BoldMode($"Kassir: {SessionManager.FirstName} {SessionManager.LastName}");
        printer.Append("\n");
        printer.Separator();

        // Footer
        printer.Append("\n");
        printer.AlignCenter();
        printer.BoldMode("SMART PARTNERS");
        printer.Append("\n");
        printer.Append("Biz bilan biznesingizni avtomatlashtiring");
        printer.Append("\n");
        printer.BoldMode("Buyurtma uchun tel: +998 99 666 11 32");
        printer.Append("\n");
        printer.Append("Rahmat! Yana kutib qolamiz!");
        printer.Append("\n");
        printer.Append("\n");

        if (!string.IsNullOrEmpty(USER_PRINTER_NAME))
        {
            printer.FullPaperCut();
            printer.PrintDocument();
        }
        else
            NotificationManager.ShowNotification(MessageType.Error, "Kassa uchun printer tanlanmagan.");


        //printer = new Printer(USER_PRINTER_NAME, "UTF-8");
        //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        //var imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"logo.png");

        //using (var image = new Bitmap(imagePath))
        //{
        //    printer.AlignCenter();
        //    printer.Image(image);
        //}
        //printer.Separator();


        //printer.Append("\n");
        //printer.AlignCenter();
        //printer.NormalWidth();
        //printer.BoldMode("Manzil:");
        //printer.BoldMode("Urgut krug, SXF binosi, 1-qavat");
        //printer.Append("\n");
        //printer.Separator();
        //printer.Append("\n");
        //printer.BoldMode($"Buyurtma uchun tel: {PHONE_NUMBER}");
        //printer.Append("\n");
        //printer.Separator();
        //printer.Append("\n");


        //printer.AlignCenter();
        //printer.DoubleWidth2();
        //printer.BoldMode(dto.TableName);
        //printer.Append("\n");
        //printer.Separator();

        //int c = 0;
        //foreach (var item in dto.Items)
        //{
        //    c++;
        //    string text = $"{item.ProductName}";
        //    int strLength = 18 - text.Length;
        //    for (int i = 1; i <= strLength; i++)
        //    {
        //        text += " ";
        //    }
        //    string temp = $"  {item.Quantity} * ";
        //    text += temp;
        //    strLength = 1 - temp.Length;
        //    for (int i = 0; i < strLength; i++)
        //    {
        //        text += " ";
        //    }
        //    text += item.TotalPrice.ToString();
        //    printer.AlignLeft();
        //    printer.Append(text);

        //    if (dto.Items.Count != c)
        //    {
        //        printer.Append("\n");
        //    }
        //}

        //printer.Separator();

        //printer.Append("\n");
        //printer.AlignLeft();
        //printer.NormalWidth();
        //printer.Append($"Jami summa:       {order_price}");
        //printer.Append($"Berilgan summa:   {payment_price}");
        //printer.Append($"Qaytim summasi:   {payment_price - order_price}");
        //printer.Append("\n");
        //printer.Append($"Sana:  " + DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
        //printer.Append("ID:   " + dto.TransactionId);

        //printer.Append("\n");

        //printer.AlignCenter();
        //printer.Append("Smart Partners");
        //printer.Append("Biz bilan biznesingizni avtomatlashtiring.");
        //printer.Append("Murojat uchun tel: +998996661132");
        //printer.Append("\n");
        //printer.Append("\n");

        //if (!string.IsNullOrEmpty(USER_PRINTER_NAME))
        //{
        //    printer.FullPaperCut();
        //    printer.PrintDocument();
        //}
        //else
        //    NotificationManager.ShowNotification(MessageType.Error, "Kassa uchun printer tanlanmagan.");
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

        if (!string.IsNullOrEmpty(USER_PRINTER_NAME))
        {
            printer.FullPaperCut();
            printer.PrintDocument();
        }
        else
            NotificationManager.ShowNotification(MessageType.Error, "Kassa uchun printer tanlanmagan.");
    }
}