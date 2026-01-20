using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Tracker.Web.Services.Interfaces;
using Tracker.Web.ViewModels;

namespace Tracker.Web.Services;

public class ExcelExportService : IExcelExportService
{
    private static readonly Dictionary<string, (string Header, Func<EnhancementExportRow, object?> Getter)> ColumnMappings = new()
    {
        ["serviceArea"] = ("Service Area", r => r.ServiceAreaName),
        ["workIdDescription"] = ("Work ID / Description", r => $"{r.WorkId} - {r.Description}"),
        ["workId"] = ("Work ID", r => r.WorkId),
        ["description"] = ("Description", r => r.Description),
        ["status"] = ("Status", r => r.Status),
        ["estimatedHours"] = ("Est. Hours", r => r.EstimatedHours),
        ["estimatedStartDate"] = ("Est. Start", r => r.EstimatedStartDate),
        ["estimatedEndDate"] = ("Est. End", r => r.EstimatedEndDate),
        ["estimationNotes"] = ("Est. Notes", r => r.EstimationNotes),
        ["sponsors"] = ("Sponsors", r => r.Sponsors),
        ["spocs"] = ("Infy SPOC", r => r.Spocs),
        ["resources"] = ("Resources", r => r.Resources),
        ["returnedHours"] = ("Ret. Hours", r => r.ReturnedHours),
        ["startDate"] = ("Start Date", r => r.StartDate),
        ["endDate"] = ("End Date", r => r.EndDate),
        ["infStatus"] = ("INF Status", r => r.InfStatus),
        ["serviceLine"] = ("Service Line", r => r.ServiceLine),
        ["infServiceLine"] = ("INF Service Line", r => r.InfServiceLine),
        ["notes"] = ("Notes", r => r.Notes),
        ["timeW1"] = ("Time W1", r => r.TimeW1),
        ["timeW2"] = ("Time W2", r => r.TimeW2),
        ["timeW3"] = ("Time W3", r => r.TimeW3),
        ["timeW4"] = ("Time W4", r => r.TimeW4),
        ["timeW5"] = ("Time W5", r => r.TimeW5),
        ["timeW6"] = ("Time W6", r => r.TimeW6),
        ["timeW7"] = ("Time W7", r => r.TimeW7),
        ["timeW8"] = ("Time W8", r => r.TimeW8),
        ["timeW9"] = ("Time W9", r => r.TimeW9),
        ["createdAt"] = ("Created", r => r.CreatedAt),
        ["createdBy"] = ("Created By", r => r.CreatedBy),
        ["modifiedAt"] = ("Modified", r => r.ModifiedAt),
        ["modifiedBy"] = ("Modified By", r => r.ModifiedBy)
    };

    public byte[] ExportToExcel(List<EnhancementExportRow> enhancements, List<string> columns, bool includeServiceAreaColumn = false)
    {
        using var stream = new MemoryStream();
        
        using (var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
        {
            var workbookPart = document.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();
            
            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());
            
            var sheets = workbookPart.Workbook.AppendChild(new Sheets());
            var sheet = new Sheet
            {
                Id = workbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = "Report"
            };
            sheets.Append(sheet);
            
            var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>()!;
            
            // Build effective column list
            var effectiveColumns = new List<string>();
            if (includeServiceAreaColumn)
            {
                effectiveColumns.Add("serviceArea");
            }
            
            // Filter out non-exportable columns (like select, actions)
            effectiveColumns.AddRange(columns.Where(c => 
                c != "select" && 
                c != "actions" && 
                ColumnMappings.ContainsKey(c)));
            
            // Add header row
            var headerRow = new Row { RowIndex = 1 };
            uint colIndex = 1;
            foreach (var col in effectiveColumns)
            {
                if (ColumnMappings.TryGetValue(col, out var mapping))
                {
                    headerRow.Append(CreateCell(GetColumnName(colIndex), 1, mapping.Header, CellValues.String));
                    colIndex++;
                }
            }
            sheetData.Append(headerRow);
            
            // Add data rows
            uint rowIndex = 2;
            foreach (var item in enhancements)
            {
                var dataRow = new Row { RowIndex = rowIndex };
                colIndex = 1;
                
                foreach (var col in effectiveColumns)
                {
                    if (ColumnMappings.TryGetValue(col, out var mapping))
                    {
                        var value = mapping.Getter(item);
                        var cell = CreateCellForValue(GetColumnName(colIndex), rowIndex, value);
                        dataRow.Append(cell);
                        colIndex++;
                    }
                }
                
                sheetData.Append(dataRow);
                rowIndex++;
            }
            
            // Auto-fit columns (approximate)
            var columnsList = new Columns();
            for (uint i = 1; i <= (uint)effectiveColumns.Count; i++)
            {
                columnsList.Append(new Column
                {
                    Min = i,
                    Max = i,
                    Width = 15,
                    BestFit = true,
                    CustomWidth = true
                });
            }
            worksheetPart.Worksheet.InsertAt(columnsList, 0);
            
            workbookPart.Workbook.Save();
        }
        
        return stream.ToArray();
    }

    public string GetExportFilename(string reportName)
    {
        var safeName = string.Join("_", reportName.Split(Path.GetInvalidFileNameChars()));
        return $"{safeName}_{DateTime.Now:yyyy-MM-dd_HHmmss}.xlsx";
    }

    private static Cell CreateCell(string columnName, uint rowIndex, string value, CellValues dataType)
    {
        return new Cell
        {
            CellReference = $"{columnName}{rowIndex}",
            CellValue = new CellValue(value),
            DataType = dataType
        };
    }

    private static Cell CreateCellForValue(string columnName, uint rowIndex, object? value)
    {
        if (value == null)
        {
            return new Cell
            {
                CellReference = $"{columnName}{rowIndex}",
                CellValue = new CellValue(""),
                DataType = CellValues.String
            };
        }

        return value switch
        {
            DateTime dt => new Cell
            {
                CellReference = $"{columnName}{rowIndex}",
                CellValue = new CellValue(dt.ToString("yyyy-MM-dd")),
                DataType = CellValues.String
            },
            decimal d => new Cell
            {
                CellReference = $"{columnName}{rowIndex}",
                CellValue = new CellValue(d.ToString()),
                DataType = CellValues.Number
            },
            int i => new Cell
            {
                CellReference = $"{columnName}{rowIndex}",
                CellValue = new CellValue(i.ToString()),
                DataType = CellValues.Number
            },
            _ => new Cell
            {
                CellReference = $"{columnName}{rowIndex}",
                CellValue = new CellValue(value.ToString() ?? ""),
                DataType = CellValues.String
            }
        };
    }

    private static string GetColumnName(uint columnIndex)
    {
        // Convert 1-based column index to Excel column name (A, B, C, ..., AA, AB, ...)
        string result = "";
        while (columnIndex > 0)
        {
            columnIndex--;
            result = (char)('A' + columnIndex % 26) + result;
            columnIndex /= 26;
        }
        return result;
    }
}
