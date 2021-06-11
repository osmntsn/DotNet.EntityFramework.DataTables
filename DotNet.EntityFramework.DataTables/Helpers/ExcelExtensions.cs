using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DataTables.AspNetCore
{
    public static class ExcelExtensions
    {
        public static bool IsNumber(this object value)
        {
            return value is sbyte
                    || value is byte
                    || value is short
                    || value is ushort
                    || value is int
                    || value is uint
                    || value is long
                    || value is ulong
                    || value is float
                    || value is double
                    || value is decimal;
        }

        private static IXLWorksheet AddWorkSheetFromQuery<T>(this XLWorkbook wb, IQueryable<T> Query, string TableName, SortedDictionary<string, string> HeaderNames = null, bool AddTotalRow = false, bool AddAverageRow = false, string[] exludeColums = null)
        {
            exludeColums = exludeColums ?? new string[0];
            var worksheet = wb.Worksheets.Add("Sayfa1");
            int rowIndex = 3;
            int columnIndex = 1;
            bool HeaderAdded = false;
            if (HeaderNames == null)
                HeaderNames = new SortedDictionary<string, string>();

            int TotalHeaderCount = Query.ElementType.GetProperties().Where(x => !exludeColums.Contains(x.Name) && !x.Name.StartsWith("hidden")).Count();

            foreach (var item in Query.ToList())
            {
                if (!HeaderAdded)
                {
                    var headerRange = worksheet.Range(rowIndex - 2, columnIndex, rowIndex - 2, columnIndex + item.GetType().GetProperties().Count() - 1);
                    headerRange.Merge();
                    headerRange.Value = TableName;
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    foreach (var property in item.GetType().GetProperties().Where(x => !exludeColums.Contains(x.Name) && !x.Name.StartsWith("hidden")))
                    {
                        worksheet.Cell(rowIndex - 1, columnIndex).Value = HeaderNames.ContainsKey(property.Name.ToString()) ? HeaderNames[property.Name.ToString()] : property.Name.ToString();
                        worksheet.Cell(rowIndex - 1, columnIndex).Style.Font.Bold = true;
                        columnIndex++;
                    }
                    columnIndex = 1;
                    HeaderAdded = true;
                }

                foreach (var property in item.GetType().GetProperties().Where(x => !exludeColums.Contains(x.Name) && !x.Name.StartsWith("hidden")))
                {

                    var test = ExtensionMethods.GetPropValueObject(item, property.Name);
                    worksheet.Cell(rowIndex, columnIndex).Value = ExtensionMethods.GetPropValueObject(item, property.Name);
                    columnIndex++;
                }
                columnIndex = 1;
                rowIndex++;

            }
            worksheet.Columns().AdjustToContents();
            rowIndex = 3;
            //Toplam Satırı Ekle
            if (AddTotalRow)
            {
                worksheet.Cell(worksheet.RowsUsed().Count() + 1, columnIndex).Value = "Toplam";
                worksheet.Cell(worksheet.RowsUsed().Count(), columnIndex).Style.Font.Bold = true;
                int UsedRowNumber = worksheet.RowsUsed().Count();
                for (int j = 0; j < TotalHeaderCount - 1; j++)
                {
                    if (IsNumber(worksheet.Cell(UsedRowNumber - 1, columnIndex + 1 + j).Value))
                    {
                        string formula = "=SUM(" + worksheet.Column(columnIndex + 1 + j).ColumnLetter() + rowIndex + ":" +
                             (worksheet.Column(j + 2).ColumnLetter() + (UsedRowNumber - 1)) +
                        ")";

                        worksheet.Cell(UsedRowNumber, columnIndex + 1 + j)
                            .SetFormulaA1(formula);
                        worksheet.Cell(UsedRowNumber, columnIndex + 1 + j).Style.Font.Bold = true;
                    }
                }
            }
            rowIndex = 3;
            //Ortalama Satırı Ekle
            if (AddAverageRow)
            {
                worksheet.Cell(worksheet.RowsUsed().Count() + 1, columnIndex).Value = "Ortalama";
                worksheet.Cell(worksheet.RowsUsed().Count(), columnIndex).Style.Font.Bold = true;
                int UsedRowNumber = worksheet.RowsUsed().Count();
                for (int j = 0; j < TotalHeaderCount - 1; j++)
                {
                    string formula = "=AVERAGE(" + worksheet.Column(columnIndex + 1 + j).ColumnLetter() + rowIndex + ":" +
                        (worksheet.Column(j + 2).ColumnLetter() + (AddTotalRow ? UsedRowNumber - 2 : UsedRowNumber - 1)) +
                    ")";

                    worksheet.Cell(UsedRowNumber, columnIndex + 1 + j)
                        .SetFormulaA1(formula);
                    worksheet.Cell(UsedRowNumber, columnIndex + 1 + j).Style.Font.Bold = true;
                }
            }
            return worksheet;
        }
        public static IActionResult ToExcelFile<T>(this IQueryable<T> Query, string fileName, string TableName, SortedDictionary<string, string> HeaderNames = null, bool AddTotalRow = false, bool AddAverageRow = false, string[] exludeColumnsList = null)
        {
            var workbook = new XLWorkbook();
            workbook.AddWorkSheetFromQuery(Query, TableName, HeaderNames, AddTotalRow, AddAverageRow, exludeColums: exludeColumnsList);
            return new FileContentResult(GetBytes(workbook), "application/vnd.ms-excel") { FileDownloadName = fileName + ".xlsx" };
        }

        private static byte[] GetBytes(XLWorkbook excelWorkbook)
        {
            using (MemoryStream fs = new MemoryStream())
            {
                excelWorkbook.SaveAs(fs);
                fs.Position = 0;
                return fs.ToArray();
            }
        }

        public static XLWorkbook ToExcelFileWithGroup<T, TValue>(this IQueryable<T> Query, string fileName, string TableName, string GroupByName, string EqualName, string EqualHeaderName, TValue EqualExampleValue, bool AddTotalRow = false, bool AddAverageRow = false)
        {
            var workbook = new XLWorkbook();
            int k = 0;

            T exampleItem = Query.FirstOrDefault();
            if (exampleItem != null)
            {
                IXLWorksheet[] worksheets = new IXLWorksheet[exampleItem.GetType().GetProperties().Count() - 2];
                foreach (var prop in exampleItem.GetType().GetProperties().Where(x => x.Name != GroupByName && x.Name != EqualName))
                {
                    worksheets[k] = workbook.Worksheets.Add(prop.Name);
                    k++;
                }
                //workbook.Worksheets.Add("Sayfa1");
                int rowIndex = 3;
                int columnIndex = 1;
                bool HeaderAdded = false;

                //başlıkları ata
                List<string> HeaderNames = Query.Select(GroupByName, "").Distinct().OrderBy(x => x).ToList();


                foreach (var item in HeaderNames)
                {
                    for (int i = 0; i < worksheets.Length; i++)
                    {

                        worksheets[i].Cell(rowIndex - 1, columnIndex + 1).Value = item;
                        worksheets[i].Cell(rowIndex - 1, columnIndex + 1).Style.Font.Bold = true;
                        worksheets[i].Cell(rowIndex - 1, columnIndex + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }
                    columnIndex++;

                }
                columnIndex = 1;
                List<TValue> ValueList = Query.Select(EqualName, EqualExampleValue).Distinct().ToList();
                SortedList<TValue, TValue> sortedValueList = new SortedList<TValue, TValue>();
                ValueList.ForEach(x => sortedValueList.Add(x, x));

                foreach (var item in sortedValueList)
                {
                    for (int i = 0; i < worksheets.Length; i++)
                    {
                        worksheets[i].Cell(rowIndex, 1).Value = item.Value;
                    }
                    rowIndex++;
                }
                rowIndex = 3;
                //Veriyi Ata
                foreach (var item in Query.AsEnumerable())
                {
                    int rowIndexCalc = sortedValueList.IndexOfKey((TValue)ExtensionMethods.GetPropValueObject(item, EqualName)) + 3;
                    int columnIndexCalc = HeaderNames.IndexOf((ExtensionMethods.GetPropValue(item, GroupByName))) + 2;
                    k = 0;
                    foreach (var property in item.GetType().GetProperties())
                    {

                        if (property.Name.ToLowerInvariant() != GroupByName.ToLowerInvariant() && property.Name.ToLowerInvariant() != EqualName.ToLowerInvariant())
                        {
                            var test = ExtensionMethods.GetPropValueObject(item, property.Name);
                            worksheets[k].Cell(rowIndexCalc, columnIndexCalc).Value = ExtensionMethods.GetPropValueObject(item, property.Name);
                            k++;
                        }
                    }
                }


                for (int i = 0; i < worksheets.Length; i++)
                {
                    //Başlık ekle
                    var headerRange = worksheets[i].Range(rowIndex - 2, columnIndex, rowIndex - 2, columnIndex + HeaderNames.Count - 1);
                    headerRange.Merge();
                    headerRange.Value = TableName;
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    worksheets[i].Cell(rowIndex - 1, 1).Value = EqualHeaderName;
                    worksheets[i].Cell(rowIndex - 1, 1).Style.Font.Bold = true;
                    worksheets[i].Cell(rowIndex - 1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    //Toplam Satırı Ekle
                    if (AddTotalRow)
                    {
                        worksheets[i].Cell(worksheets[i].RowsUsed().Count() + 1, columnIndex).Value = "Toplam";
                        worksheets[i].Cell(worksheets[i].RowsUsed().Count(), columnIndex).Style.Font.Bold = true;
                        int UsedRowNumber = worksheets[i].RowsUsed().Count();
                        for (int j = 0; j < HeaderNames.Count; j++)
                        {
                            string formula = "=SUM(" + worksheets[i].Column(columnIndex + 1 + j).ColumnLetter() + rowIndex + ":" +
                                 (worksheets[i].Column(j + 2).ColumnLetter() + (UsedRowNumber - 1)) +
                            ")";

                            worksheets[i].Cell(UsedRowNumber, columnIndex + 1 + j)
                                .SetFormulaA1(formula);
                            worksheets[i].Cell(UsedRowNumber, columnIndex + 1 + j).Style.Font.Bold = true;
                        }
                    }

                    //Ortalama Satırı Ekle
                    if (AddAverageRow)
                    {
                        worksheets[i].Cell(worksheets[i].RowsUsed().Count() + 1, columnIndex).Value = "Ortalama";
                        worksheets[i].Cell(worksheets[i].RowsUsed().Count(), columnIndex).Style.Font.Bold = true;
                        int UsedRowNumber = worksheets[i].RowsUsed().Count();
                        for (int j = 0; j < HeaderNames.Count; j++)
                        {
                            string formula = "=AVERAGE(" + worksheets[i].Column(columnIndex + 1 + j).ColumnLetter() + rowIndex + ":" +
                                (worksheets[i].Column(j + 2).ColumnLetter() + (AddTotalRow ? UsedRowNumber - 2 : UsedRowNumber - 1)) +
                            ")";

                            worksheets[i].Cell(UsedRowNumber, columnIndex + 1 + j)
                                .SetFormulaA1(formula);
                            worksheets[i].Cell(UsedRowNumber, columnIndex + 1 + j).Style.Font.Bold = true;
                        }
                    }

                    //Hücreleri düzenle

                    var wsRange = worksheets[i].Range(rowIndex - 2, columnIndex, worksheets[i].RowsUsed().Count(), columnIndex + HeaderNames.Count);
                    wsRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                    wsRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheets[i].Columns().AdjustToContents();
                }
            }
            else
            {
                workbook.AddWorksheet("Sayfa1");
            }

            return workbook;
        }
    }
}

