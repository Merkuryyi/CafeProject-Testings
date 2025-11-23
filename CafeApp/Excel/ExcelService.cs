using ClosedXML.Excel;
using System;
using System.IO;
using CafeApp.Models;

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

                    // Заголовки таблицы
                    worksheet.Cell("A9").Value = "№";
                    worksheet.Cell("B9").Value = "Наименование блюда";
                    worksheet.Cell("C9").Value = "Кол-во";
                    worksheet.Cell("D9").Value = "Цена";
                    worksheet.Cell("E9").Value = "Сумма";

                    // Стиль заголовков таблицы
                    var headerRange = worksheet.Range("A9:E9");
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Данные блюд
                    int row = 10;
                    int itemNumber = 1;
                    
                    foreach (var item in receiptOrder.Items)
                    {
                        worksheet.Cell($"A{row}").Value = itemNumber;
                        worksheet.Cell($"B{row}").Value = item.DishName;
                        worksheet.Cell($"C{row}").Value = item.Quantity;
                        worksheet.Cell($"D{row}").Value = item.Price;
                        worksheet.Cell($"E{row}").Value = item.TotalPrice;
                        
                        // Форматирование чисел
                        worksheet.Cell($"D{row}").Style.NumberFormat.Format = "#,##0.00\" ₽\"";
                        worksheet.Cell($"E{row}").Style.NumberFormat.Format = "#,##0.00\" ₽\"";
                        
                        // Выравнивание
                        worksheet.Cell($"A{row}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Cell($"C{row}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Cell($"D{row}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        worksheet.Cell($"E{row}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        
                        row++;
                        itemNumber++;
                    }

                    // Итоговая сумма
                    worksheet.Cell($"D{row}").Value = "ИТОГО:";
                    worksheet.Cell($"D{row}").Style.Font.Bold = true;
                    worksheet.Cell($"E{row}").Value = receiptOrder.TotalAmount;
                    worksheet.Cell($"E{row}").Style.Font.Bold = true;
                    worksheet.Cell($"E{row}").Style.NumberFormat.Format = "#,##0.00\" ₽\"";

                    // Настройка ширины колонок
                    worksheet.Column("A").Width = 5;
                    worksheet.Column("B").Width = 35;
                    worksheet.Column("C").Width = 10;
                    worksheet.Column("D").Width = 12;
                    worksheet.Column("E").Width = 15;

                    // Границы для таблицы
                    var tableRange = worksheet.Range($"A9:E{row}");
                    tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Создаем папку для сохранения
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
    }
}