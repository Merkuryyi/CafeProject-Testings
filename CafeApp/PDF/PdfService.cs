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
        public string GenerateOrdersReceivedReport(List<OrderReportData> orders, string shiftInfo, string filePath)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    Document document = new Document(PageSize.A4.Rotate());
                    PdfWriter writer = PdfWriter.GetInstance(document, fs);
                    
                    document.Open();
                    
                    // Заголовок
                    Font titleFont = FontFactory.GetFont("Arial", 16, Font.BOLD);
                    Paragraph title = new Paragraph($"Отчет полученных заказов - {shiftInfo}", titleFont);
                    title.Alignment = Element.ALIGN_CENTER;
                    title.SpacingAfter = 20f;
                    document.Add(title);
                    
                    // Дата генерации
                    Font dateFont = FontFactory.GetFont("Arial", 10, Font.ITALIC);
                    Paragraph date = new Paragraph($"Сгенерировано: {DateTime.Now:dd.MM.yyyy HH:mm}", dateFont);
                    date.Alignment = Element.ALIGN_RIGHT;
                    date.SpacingAfter = 10f;
                    document.Add(date);
                    
                    // Общая информация
                    Font infoFont = FontFactory.GetFont("Arial", 10, Font.NORMAL);
                    Paragraph info = new Paragraph($"Всего заказов: {orders.Count}\nОбщая сумма: {orders.Sum(o => o.TotalAmount):C}", infoFont);
                    info.SpacingAfter = 10f;
                    document.Add(info);
                    
                    // Таблица
                    PdfPTable table = new PdfPTable(6);
                    table.WidthPercentage = 100;
                    table.SetWidths(new float[] { 1, 2, 1, 2, 2, 2 });
                    
                    // Заголовки таблицы
                    Font headerFont = FontFactory.GetFont("Arial", 10, Font.BOLD);
                    table.AddCell(new PdfPCell(new Phrase("№ заказа", headerFont)));
                    table.AddCell(new PdfPCell(new Phrase("Официант", headerFont)));
                    table.AddCell(new PdfPCell(new Phrase("Стол", headerFont)));
                    table.AddCell(new PdfPCell(new Phrase("Время создания", headerFont)));
                    table.AddCell(new PdfPCell(new Phrase("Статус", headerFont)));
                    table.AddCell(new PdfPCell(new Phrase("Сумма", headerFont)));
                    
                    // Данные
                    Font cellFont = FontFactory.GetFont("Arial", 9);
                    foreach (var order in orders)
                    {
                        table.AddCell(new PdfPCell(new Phrase(order.OrderId.ToString(), cellFont)));
                        table.AddCell(new PdfPCell(new Phrase(order.WaiterName ?? "", cellFont)));
                        table.AddCell(new PdfPCell(new Phrase(order.TableId.ToString(), cellFont)));
                        table.AddCell(new PdfPCell(new Phrase(order.OrderDate.ToString("dd.MM.yyyy HH:mm"), cellFont)));
                        table.AddCell(new PdfPCell(new Phrase(GetStatusText(order.Status), cellFont)));
                        table.AddCell(new PdfPCell(new Phrase(order.TotalAmount.ToString("C"), cellFont)));
                    }
                    
                    // Итоговая сумма
                    decimal totalAmount = orders.Sum(o => o.TotalAmount);
                    table.AddCell(new PdfPCell(new Phrase("ИТОГО:", headerFont)) { Colspan = 5 });
                    table.AddCell(new PdfPCell(new Phrase(totalAmount.ToString("C"), headerFont)));
                    
                    document.Add(table);
                    
                    // Добавляем детали по заказам, если есть
                    if (orders.Any(o => o.Items != null && o.Items.Any()))
                    {
                        document.NewPage();
                        
                        Paragraph detailsTitle = new Paragraph("Детали заказов", titleFont);
                        detailsTitle.Alignment = Element.ALIGN_CENTER;
                        detailsTitle.SpacingAfter = 20f;
                        document.Add(detailsTitle);
                        
                        foreach (var order in orders.Where(o => o.Items != null && o.Items.Any()))
                        {
                            // Заголовок заказа
                            Font orderFont = FontFactory.GetFont("Arial", 12, Font.BOLD);
                            Paragraph orderHeader = new Paragraph($"Заказ №{order.OrderId} - {order.WaiterName} - Стол {order.TableId}", orderFont);
                            orderHeader.SpacingAfter = 10f;
                            document.Add(orderHeader);
                            
                            // Таблица позиций заказа
                            PdfPTable itemsTable = new PdfPTable(4);
                            itemsTable.WidthPercentage = 100;
                            itemsTable.SetWidths(new float[] { 3, 1, 2, 2 });
                            
                            // Заголовки таблицы позиций
                            itemsTable.AddCell(new PdfPCell(new Phrase("Блюдо", headerFont)));
                            itemsTable.AddCell(new PdfPCell(new Phrase("Кол-во", headerFont)));
                            itemsTable.AddCell(new PdfPCell(new Phrase("Цена", headerFont)));
                            itemsTable.AddCell(new PdfPCell(new Phrase("Сумма", headerFont)));
                            
                            // Данные позиций
                            foreach (var item in order.Items)
                            {
                                itemsTable.AddCell(new PdfPCell(new Phrase(item.DishName ?? "", cellFont)));
                                itemsTable.AddCell(new PdfPCell(new Phrase(item.Quantity.ToString(), cellFont)));
                                itemsTable.AddCell(new PdfPCell(new Phrase(item.Price.ToString("C"), cellFont)));
                                itemsTable.AddCell(new PdfPCell(new Phrase(item.TotalPrice.ToString("C"), cellFont)));
                            }
                            
                            // Итог по заказу
                            itemsTable.AddCell(new PdfPCell(new Phrase("Итого по заказу:", headerFont)) { Colspan = 3 });
                            itemsTable.AddCell(new PdfPCell(new Phrase(order.TotalAmount.ToString("C"), headerFont)));
                            
                            document.Add(itemsTable);
                            document.Add(new Paragraph(" ")); // Пустая строка
                        }
                    }
                    
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
                    
                    // Заголовок
                    Font titleFont = FontFactory.GetFont("Arial", 16, Font.BOLD);
                    Paragraph title = new Paragraph($"Отчет оплаченных заказов - {shiftInfo}", titleFont);
                    title.Alignment = Element.ALIGN_CENTER;
                    title.SpacingAfter = 20f;
                    document.Add(title);
                    
                    // Дата генерации
                    Font dateFont = FontFactory.GetFont("Arial", 10, Font.ITALIC);
                    Paragraph date = new Paragraph($"Сгенерировано: {DateTime.Now:dd.MM.yyyy HH:mm}", dateFont);
                    date.Alignment = Element.ALIGN_RIGHT;
                    date.SpacingAfter = 10f;
                    document.Add(date);
                    
                    // Общая информация
                    Font infoFont = FontFactory.GetFont("Arial", 10, Font.NORMAL);
                    
                    // Статистика по способам оплаты
                    var paymentStats = orders.GroupBy(o => o.PaymentType)
                                           .Select(g => new { Type = g.Key, Count = g.Count(), Sum = g.Sum(o => o.TotalAmount) })
                                           .ToList();
                    
                    string statsText = $"Всего оплаченных заказов: {orders.Count}\n" +
                                     $"Общая выручка: {orders.Sum(o => o.TotalAmount):C}\n\n" +
                                     "Сводка по оплатам:\n";
                    
                    foreach (var stat in paymentStats)
                    {
                        statsText += $"{GetPaymentMethodText(stat.Type)}: {stat.Count} заказов, {stat.Sum:C}\n";
                    }
                    
                    Paragraph info = new Paragraph(statsText, infoFont);
                    info.SpacingAfter = 10f;
                    document.Add(info);
                    
                    // Таблица
                    PdfPTable table = new PdfPTable(7);
                    table.WidthPercentage = 100;
                    table.SetWidths(new float[] { 1, 2, 1, 2, 2, 2, 2 });
                    
                    // Заголовки таблицы
                    Font headerFont = FontFactory.GetFont("Arial", 10, Font.BOLD);
                    table.AddCell(new PdfPCell(new Phrase("№ заказа", headerFont)));
                    table.AddCell(new PdfPCell(new Phrase("Официант", headerFont)));
                    table.AddCell(new PdfPCell(new Phrase("Стол", headerFont)));
                    table.AddCell(new PdfPCell(new Phrase("Время создания", headerFont)));
                    table.AddCell(new PdfPCell(new Phrase("Время оплаты", headerFont)));
                    table.AddCell(new PdfPCell(new Phrase("Способ оплаты", headerFont)));
                    table.AddCell(new PdfPCell(new Phrase("Сумма", headerFont)));
                    
                    // Данные
                    Font cellFont = FontFactory.GetFont("Arial", 9);
                    foreach (var order in orders)
                    {
                        table.AddCell(new PdfPCell(new Phrase(order.OrderId.ToString(), cellFont)));
                        table.AddCell(new PdfPCell(new Phrase(order.WaiterName ?? "", cellFont)));
                        table.AddCell(new PdfPCell(new Phrase(order.TableId.ToString() ?? "", cellFont)));
                        table.AddCell(new PdfPCell(new Phrase(order.OrderDate.ToString("dd.MM.yyyy HH:mm"), cellFont)));
                        table.AddCell(new PdfPCell(new Phrase(GetPaymentMethodText(order.PaymentType), cellFont)));
                        table.AddCell(new PdfPCell(new Phrase(order.TotalAmount.ToString("C"), cellFont)));
                    }
                    
                    decimal totalAmount = orders.Sum(o => o.TotalAmount);
                    table.AddCell(new PdfPCell(new Phrase("ИТОГО:", headerFont)) { Colspan = 6 });
                    table.AddCell(new PdfPCell(new Phrase(totalAmount.ToString("C"), headerFont)));
                    
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