using System;
using System.IO;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using CafeApp.Models;
using System.Collections.Generic;

namespace CafeApp.PDF
{
    public class PdfService
    {
        private BaseFont _baseFont;

        public PdfService()
        {
            // Инициализируем шрифт с поддержкой кириллицы
            try
            {
                // Пробуем разные пути к шрифтам Arial
                string[] fontPaths = {
                    @"C:\Windows\Fonts\arial.ttf",
                    @"C:\Windows\Fonts\arialuni.ttf", 
                    @"C:\Windows\Fonts\times.ttf",
                    @"C:\Windows\Fonts\cour.ttf"
                };

                foreach (string fontPath in fontPaths)
                {
                    if (File.Exists(fontPath))
                    {
                        _baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                        break;
                    }
                }

                // Если ни один шрифт не найден, используем встроенный Helvetica (будет без кириллицы)
                if (_baseFont == null)
                {
                    _baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                }
            }
            catch (Exception)
            {
                // Fallback на стандартный шрифт
                _baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            }
        }

        private Font CreateFont(float size, int style = Font.NORMAL)
        {
            return new Font(_baseFont, size, style);
        }

        public string GenerateOrdersReceivedReport(List<OrderReportData> orders, string shiftInfo, string filePath)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    Document document = new Document(PageSize.A4.Rotate());
                    PdfWriter writer = PdfWriter.GetInstance(document, fs);
                    
                    document.Open();
                    
                    // Создаем шрифты
                    Font titleFont = CreateFont(16, Font.BOLD);
                    Font headerFont = CreateFont(10, Font.BOLD);
                    Font cellFont = CreateFont(9);
                    Font infoFont = CreateFont(10);
                    Font dateFont = CreateFont(10, Font.ITALIC);
                    
                    // Заголовок
                    Paragraph title = new Paragraph("Report on orders received - " + shiftInfo, titleFont);
                    title.Alignment = Element.ALIGN_CENTER;
                    title.SpacingAfter = 20f;
                    document.Add(title);
                    
                    // Дата генерации
                    Paragraph date = new Paragraph("Generated: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm"), dateFont);
                    date.Alignment = Element.ALIGN_RIGHT;
                    date.SpacingAfter = 10f;
                    document.Add(date);
                    
                    // Общая информация
                    Paragraph info = new Paragraph($"Orders: {orders.Count}\nTotal amount: {orders.Sum(o => o.TotalAmount):C}", infoFont);
                    info.SpacingAfter = 10f;
                    document.Add(info);
                    
                    // Таблица
                    PdfPTable table = new PdfPTable(6);
                    table.WidthPercentage = 100;
                    table.SetWidths(new float[] { 1, 2, 1, 2, 2, 2 });
                    
                    // Заголовки таблицы с явным стилем
                    AddHeaderCell(table, "N Order", headerFont);
                    AddHeaderCell(table, "Waiter", headerFont);
                    AddHeaderCell(table, "Table", headerFont);
                    AddHeaderCell(table, "Created at", headerFont);
                    AddHeaderCell(table, "Payment type", headerFont);
                    AddHeaderCell(table, "Count", headerFont);
                    // Данные
                    foreach (var order in orders)
                    {
                        AddCell(table, order.OrderId.ToString(), cellFont);
                        AddCell(table, order.WaiterName ?? "", cellFont);
                        AddCell(table, order.TableId.ToString(), cellFont);
                        AddCell(table, order.OrderDate.ToString("dd.MM.yyyy HH:mm"), cellFont);
                        AddCell(table, GetStatusText(order.Status), cellFont);
                        AddCell(table, order.TotalAmount.ToString("C"), cellFont);
                    }
                    
                    // Итоговая сумма
                    decimal totalAmount = orders.Sum(o => o.TotalAmount);
                    AddCell(table, "Amount:", headerFont, 5);
                    AddCell(table, totalAmount.ToString("C"), headerFont);
                    
                    document.Add(table);
                    document.Close();
                }
                
                return filePath;
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"A:\debug.log", 
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - ERROR in PDF generation: {ex.Message}\n{ex.StackTrace}\n");
                throw;
            }
        }
        
        public string GeneratePaidOrdersReport(List<OrderReportData> orders, string shiftInfo, string filePath)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    Document document = new Document(PageSize.A4.Rotate());
                    PdfWriter writer = PdfWriter.GetInstance(document, fs);
                    
                    document.Open();
                    
                    // Создаем шрифты
                    Font titleFont = CreateFont(16, Font.BOLD);
                    Font headerFont = CreateFont(10, Font.BOLD);
                    Font cellFont = CreateFont(9);
                    Font infoFont = CreateFont(10);
                    Font dateFont = CreateFont(10, Font.ITALIC);
                    
                    // Заголовок
                    Paragraph title = new Paragraph("Report on paid orders - " + shiftInfo, titleFont);
                    title.Alignment = Element.ALIGN_CENTER;
                    title.SpacingAfter = 20f;
                    document.Add(title);
                    
                    // Дата генерации
                    Paragraph date = new Paragraph("Generated: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm"), dateFont);
                    date.Alignment = Element.ALIGN_RIGHT;
                    date.SpacingAfter = 10f;
                    document.Add(date);
                    
                    // Общая информация
                    var paymentStats = orders.GroupBy(o => o.PaymentType)
                                           .Select(g => new { Type = g.Key, Count = g.Count(), Sum = g.Sum(o => o.TotalAmount) })
                                           .ToList();
                    
                    string statsText = $"Total paid orders: {orders.Count}\n" +
                                     $"Total revenue: {orders.Sum(o => o.TotalAmount):C}\n\n" +
                                     "Summary of payments:\n";
                    
                    foreach (var stat in paymentStats)
                    {
                        statsText += $"{GetPaymentMethodText(stat.Type)}: {stat.Count} order, {stat.Sum:C}\n";
                    }
                    
                    Paragraph info = new Paragraph(statsText, infoFont);
                    info.SpacingAfter = 10f;
                    document.Add(info);
                    
                    // Таблица заказов
                    PdfPTable table = new PdfPTable(6);
                    table.WidthPercentage = 100;
                    table.SetWidths(new float[] { 1, 2, 1, 2, 2, 2 });
                    
                    // Заголовки таблицы
                    AddHeaderCell(table, "N Order", headerFont);
                    AddHeaderCell(table, "Waiter", headerFont);
                    AddHeaderCell(table, "Table", headerFont);
                    AddHeaderCell(table, "Created at", headerFont);
                    AddHeaderCell(table, "Payment type", headerFont);
                    AddHeaderCell(table, "Count", headerFont);
                    
                    // Данные
                    foreach (var order in orders)
                    {
                        AddCell(table, order.OrderId.ToString(), cellFont);
                        AddCell(table, order.WaiterName ?? "", cellFont);
                        AddCell(table, order.TableId.ToString(), cellFont);
                        AddCell(table, order.OrderDate.ToString("dd.MM.yyyy HH:mm"), cellFont);
                        AddCell(table, GetPaymentMethodText(order.PaymentType), cellFont);
                        AddCell(table, order.TotalAmount.ToString("C"), cellFont);
                    }
                    
                    // Итоговая сумма
                    decimal totalAmount = orders.Sum(o => o.TotalAmount);
                    AddCell(table, "Amount:", headerFont, 5);
                    AddCell(table, totalAmount.ToString("C"), headerFont);
                    
                    document.Add(table);
                    document.Close();
                }
                
                return filePath;
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"A:\debug.log", 
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - ERROR in PDF generation: {ex.Message}\n{ex.StackTrace}\n");
                throw;
            }
        }
        
        // Вспомогательные методы для добавления ячеек
        private void AddHeaderCell(PdfPTable table, string text, Font font)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font));
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.Padding = 5;
            cell.BorderWidth = 1;
            table.AddCell(cell);
        }
        
        private void AddCell(PdfPTable table, string text, Font font, int colspan = 1)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font));
            cell.Padding = 4;
            cell.BorderWidth = 1;
            if (colspan > 1)
            {
                cell.Colspan = colspan;
            }
            table.AddCell(cell);
        }
        
        private string GetStatusText(string status)
        {
            return status switch
            {
                "created" => "Создан",
                "in_progress" => "В работе",
                "completed" => "Завершен",
                "paid" => "Оплачен",
                "cancelled" => "Отменен",
                _ => status
            };
        }
        
        private string GetPaymentMethodText(string paymentMethod)
        {
            return paymentMethod switch
            {
                "cash" => "Наличные",
                "card" => "Карта",
                "online" => "Онлайн",
                _ => paymentMethod
            };
        }
    }
}