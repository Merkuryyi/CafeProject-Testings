using ClosedXML.Excel;
using System;
using System.IO;
using CafeApp.Models;
using System.Collections.Generic;
using System.Linq;

namespace CafeApp.Excel
{
    public class ExcelService 
    {
        public string GenerateReceiptOrder(ReceiptOrder receiptOrder)
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Кассовый чек");

                    worksheet.Cell("A1").Value = "КАССОВЫЙ ЧЕК";
                    worksheet.Cell("A1").Style.Font.Bold = true;
                    worksheet.Cell("A1").Style.Font.FontSize = 16;
                    worksheet.Range("A1:E1").Merge();

                    worksheet.Cell("A3").Value = $"Номер заказа: {receiptOrder.OrderId}";
                    worksheet.Cell("A4").Value = $"Дата: {receiptOrder.OrderDate:dd.MM.yyyy HH:mm}";
                    worksheet.Cell("A5").Value = $"Стол: №{receiptOrder.TableId}";
                    worksheet.Cell("A6").Value = $"Официант: {receiptOrder.WaiterName}";
                    worksheet.Cell("A7").Value = $"Способ оплаты: {receiptOrder.PaymentType}";
                    
                    worksheet.Cell("A9").Value = "№";
                    worksheet.Cell("B9").Value = "Наименование блюда";
                    worksheet.Cell("C9").Value = "Кол-во";
                    worksheet.Cell("D9").Value = "Цена";
                    worksheet.Cell("E9").Value = "Сумма";

                    var headerRange = worksheet.Range("A9:E9");
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    
                    int row = 10;
                    int itemNumber = 1;
                    
                    foreach (var item in receiptOrder.Items)
                    {
                        worksheet.Cell($"A{row}").Value = itemNumber;
                        worksheet.Cell($"B{row}").Value = item.DishName;
                        worksheet.Cell($"C{row}").Value = item.Quantity;
                        worksheet.Cell($"D{row}").Value = item.Price;
                        worksheet.Cell($"E{row}").Value = item.TotalPrice;
                     
                        worksheet.Cell($"D{row}").Style.NumberFormat.Format = "#,##0.00\" ₽\"";
                        worksheet.Cell($"E{row}").Style.NumberFormat.Format = "#,##0.00\" ₽\"";
                        
                        worksheet.Cell($"A{row}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Cell($"C{row}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Cell($"D{row}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        worksheet.Cell($"E{row}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        
                        row++;
                        itemNumber++;
                    }

                    worksheet.Cell($"D{row}").Value = "ИТОГО:";
                    worksheet.Cell($"D{row}").Style.Font.Bold = true;
                    worksheet.Cell($"E{row}").Value = receiptOrder.TotalAmount;
                    worksheet.Cell($"E{row}").Style.Font.Bold = true;
                    worksheet.Cell($"E{row}").Style.NumberFormat.Format = "#,##0.00\" ₽\"";

                    worksheet.Column("A").Width = 5;
                    worksheet.Column("B").Width = 35;
                    worksheet.Column("C").Width = 10;
                    worksheet.Column("D").Width = 12;
                    worksheet.Column("E").Width = 15;

                    var tableRange = worksheet.Range($"A9:E{row}");
                    tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                    if (!Directory.Exists(downloadsPath))
                        Directory.CreateDirectory(downloadsPath);
                        
                    string fileName = $"Кассовый чек №{receiptOrder.OrderId}_{receiptOrder.OrderDate:yyyyMMdd_HHmm}.xlsx";
                    string filePath = Path.Combine(downloadsPath, fileName);

                    workbook.SaveAs(filePath);

                    return filePath;
                }
            }
            catch (Exception ex)
            {
                string logPath = @"A:\Инженерно-техническая поддержка сопровождения ИС\debug.log";
                string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - ERROR generating Excel: {ex.Message}\n{ex.StackTrace}\n";
                File.AppendAllText(logPath, errorMessage);
                return null;
            }
        }

        public string GenerateOrdersReceivedReport(List<OrderReportData> orders, string shiftInfo, string filePath)
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Полученные заказы");

                    worksheet.Cell("A1").Value = "ОТЧЕТ О ПОЛУЧЕННЫХ ЗАКАЗАХ";
                    worksheet.Cell("A1").Style.Font.Bold = true;
                    worksheet.Cell("A1").Style.Font.FontSize = 16;
                    worksheet.Range("A1:H1").Merge();

                    worksheet.Cell("A2").Value = $"Смена: {shiftInfo}";
                    worksheet.Cell("A3").Value = $"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}";
                    worksheet.Cell("A4").Value = $"Всего заказов: {orders.Count}";
                    worksheet.Cell("A5").Value = $"Общая сумма: {orders.Sum(o => o.TotalAmount):C}";

                    int row = 7;
                    worksheet.Cell($"A{row}").Value = "№ заказа";
                    worksheet.Cell($"B{row}").Value = "Дата/время";
                    worksheet.Cell($"C{row}").Value = "Стол";
                    worksheet.Cell($"D{row}").Value = "Официант";
                    worksheet.Cell($"E{row}").Value = "Статус";
                    worksheet.Cell($"F{row}").Value = "Способ оплаты";
                    worksheet.Cell($"G{row}").Value = "Гостей";
                    worksheet.Cell($"H{row}").Value = "Сумма";

                    var headerRange = worksheet.Range($"A{row}:H{row}");
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    row++;
                    foreach (var order in orders)
                    {
                        worksheet.Cell($"A{row}").Value = order.OrderId;
                        worksheet.Cell($"B{row}").Value = order.OrderDate;
                        worksheet.Cell($"C{row}").Value = order.TableId;
                        worksheet.Cell($"D{row}").Value = order.WaiterName;
                        worksheet.Cell($"E{row}").Value = GetStatusText(order.Status);
                        worksheet.Cell($"F{row}").Value = GetPaymentMethodText(order.PaymentType);
                        worksheet.Cell($"G{row}").Value = order.CustomerCount;
                        worksheet.Cell($"H{row}").Value = order.TotalAmount;

                        worksheet.Cell($"B{row}").Style.NumberFormat.Format = "dd.MM.yyyy HH:mm";
                        worksheet.Cell($"H{row}").Style.NumberFormat.Format = "#,##0.00\" ₽\"";

                        row++;

                        if (order.Items != null && order.Items.Any())
                        {
                            worksheet.Cell($"B{row}").Value = "Блюда:";
                            worksheet.Cell($"B{row}").Style.Font.Bold = true;
                            row++;

                            foreach (var item in order.Items)
                            {
                                worksheet.Cell($"C{row}").Value = item.DishName;
                                worksheet.Cell($"F{row}").Value = item.Quantity;
                                worksheet.Cell($"G{row}").Value = item.Price;
                                worksheet.Cell($"H{row}").Value = item.TotalPrice;

                                worksheet.Cell($"G{row}").Style.NumberFormat.Format = "#,##0.00\" ₽\"";
                                worksheet.Cell($"H{row}").Style.NumberFormat.Format = "#,##0.00\" ₽\"";

                                row++;
                            }
                            row++; 
                        }
                    }

                    worksheet.Cell($"G{row}").Value = "ИТОГО:";
                    worksheet.Cell($"G{row}").Style.Font.Bold = true;
                    worksheet.Cell($"H{row}").Value = orders.Sum(o => o.TotalAmount);
                    worksheet.Cell($"H{row}").Style.Font.Bold = true;
                    worksheet.Cell($"H{row}").Style.NumberFormat.Format = "#,##0.00\" ₽\"";

                    worksheet.Columns().AdjustToContents();

                    var dataRange = worksheet.Range($"A7:H{row}");
                    dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    workbook.SaveAs(filePath);
                    return filePath;
                }
            }
            catch (Exception ex)
            {
                LogError("GenerateOrdersReceivedReport", ex);
                return null;
            }
        }

        public string GeneratePaidOrdersReport(List<OrderReportData> orders, string shiftInfo, string filePath)
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Выручка");

                    worksheet.Cell("A1").Value = "ОТЧЕТ О ВЫРУЧКЕ";
                    worksheet.Cell("A1").Style.Font.Bold = true;
                    worksheet.Cell("A1").Style.Font.FontSize = 16;
                    worksheet.Range("A1:H1").Merge();

                    worksheet.Cell("A2").Value = $"Смена: {shiftInfo}";
                    worksheet.Cell("A3").Value = $"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}";
                    worksheet.Cell("A4").Value = $"Всего оплаченных заказов: {orders.Count}";
                    worksheet.Cell("A5").Value = $"Общая выручка: {orders.Sum(o => o.TotalAmount):C}";

                    var paymentStats = orders.GroupBy(o => o.PaymentType)
                                           .Select(g => new { Type = g.Key, Count = g.Count(), Sum = g.Sum(o => o.TotalAmount) })
                                           .ToList();

                    int row = 7;
                    worksheet.Cell($"A{row}").Value = "Сводка по оплатам:";
                    worksheet.Cell($"A{row}").Style.Font.Bold = true;
                    row++;

                    foreach (var stat in paymentStats)
                    {
                        worksheet.Cell($"A{row}").Value = $"{GetPaymentMethodText(stat.Type)}:";
                        worksheet.Cell($"B{row}").Value = $"{stat.Count} заказов";
                        worksheet.Cell($"C{row}").Value = stat.Sum;
                        worksheet.Cell($"C{row}").Style.NumberFormat.Format = "#,##0.00\" ₽\"";
                        row++;
                    }
                    row++;

                    worksheet.Cell($"A{row}").Value = "№ заказа";
                    worksheet.Cell($"B{row}").Value = "Дата/время";
                    worksheet.Cell($"C{row}").Value = "Стол";
                    worksheet.Cell($"D{row}").Value = "Официант";
                    worksheet.Cell($"E{row}").Value = "Способ оплаты";
                    worksheet.Cell($"F{row}").Value = "Гостей";
                    worksheet.Cell($"G{row}").Value = "Сумма";

                    var headerRange = worksheet.Range($"A{row}:G{row}");
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                    row++;
                    foreach (var order in orders)
                    {
                        worksheet.Cell($"A{row}").Value = order.OrderId;
                        worksheet.Cell($"B{row}").Value = order.OrderDate;
                        worksheet.Cell($"C{row}").Value = order.TableId;
                        worksheet.Cell($"D{row}").Value = order.WaiterName;
                        worksheet.Cell($"E{row}").Value = GetPaymentMethodText(order.PaymentType);
                        worksheet.Cell($"F{row}").Value = order.CustomerCount;
                        worksheet.Cell($"G{row}").Value = order.TotalAmount;

                        worksheet.Cell($"B{row}").Style.NumberFormat.Format = "dd.MM.yyyy HH:mm";
                        worksheet.Cell($"G{row}").Style.NumberFormat.Format = "#,##0.00\" ₽\"";
                        row++;
                    }

                    worksheet.Cell($"F{row}").Value = "ВСЕГО:";
                    worksheet.Cell($"F{row}").Style.Font.Bold = true;
                    worksheet.Cell($"G{row}").Value = orders.Sum(o => o.TotalAmount);
                    worksheet.Cell($"G{row}").Style.Font.Bold = true;
                    worksheet.Cell($"G{row}").Style.NumberFormat.Format = "#,##0.00\" ₽\"";

                    worksheet.Columns().AdjustToContents();

                    var dataRange = worksheet.Range($"A7:G{row}");
                    dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    workbook.SaveAs(filePath);
                    return filePath;
                }
            }
            catch (Exception ex)
            {
                LogError("GeneratePaidOrdersReport", ex);
                return null;
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

        private void LogError(string method, Exception ex)
        {
            string logPath = @"A:\Инженерно-техническая поддержка сопровождения ИС\debug.log";
            string errorMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - ERROR in {method}: {ex.Message}\n{ex.StackTrace}\n";
            File.AppendAllText(logPath, errorMessage);
        }
    }
}