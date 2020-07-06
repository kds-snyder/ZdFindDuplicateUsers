using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using ZdFindDuplicateUsers.ZdModels;

namespace ZdFindDuplicateUsers.HelperFunctions
{
    public static class ExcelHelperFunctions
    {
        #region Utility functions
        /// <summary>
        /// Add cell with specified address to specified row
        /// </summary>
        /// <param name="row"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        private static Cell CreateCell(Row row, String address)
        {
            Cell cellResult;
            Cell refCell = null;

            // Cells must be in sequential order according to CellReference. 
            // Determine where to insert the new cell.
            foreach (Cell cell in row.Elements<Cell>())
            {
                if (string.Compare(cell.CellReference.Value, address, true) > 0)
                {
                    refCell = cell;
                    break;
                }
            }

            cellResult = new Cell();
            cellResult.CellReference = address;

            row.InsertBefore(cellResult, refCell);
            return cellResult;
        }

        public static void CreateExcelFile(string outputExcelFile, string sheetName = "mySheet")
        {
            // Create a spreadsheet document by supplying the filepath.
            // By default, AutoSave = true, Editable = true, and Type = xlsx.
            SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.
                Create(outputExcelFile, SpreadsheetDocumentType.Workbook);

            // Add a WorkbookPart to the document.
            WorkbookPart workbookpart = spreadsheetDocument.AddWorkbookPart();
            workbookpart.Workbook = new Workbook();

            // Add a WorksheetPart to the WorkbookPart.
            WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            // Add Sheets to the Workbook.
            Sheets sheets = spreadsheetDocument.WorkbookPart.Workbook.
                AppendChild<Sheets>(new Sheets());

            // Append a new worksheet and associate it with the workbook.
            Sheet sheet = new Sheet()
            {
                Id = spreadsheetDocument.WorkbookPart.
                GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = sheetName
            };
            sheets.Append(sheet);

            workbookpart.Workbook.Save();

            // Close the document.
            spreadsheetDocument.Close();
        }

        /// <summary>
        /// Return the row at the specified rowIndex located within
        /// the sheet data passed in via wsData. If the row does not
        /// exist, create it.
        /// </summary>
        /// <param name="wsData"></param>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        private static Row GetRow(SheetData wsData, UInt32 rowIndex)
        {
            var row = wsData.Elements<Row>().FirstOrDefault(r => r.RowIndex.Value == rowIndex);
            if (row == null)
            {
                row = new Row();
                row.RowIndex = rowIndex;
                wsData.Append(row);
            }
            return row;
        }

        /// <summary>
        /// Given an Excel address such as E5 or AB128, GetRowIndex
        /// parses the address and returns the row index.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        private static UInt32 GetRowIndex(string address)
        {
            string rowPart;
            UInt32 l;
            UInt32 result = 0;

            for (int i = 0; i < address.Length; i++)
            {
                if (UInt32.TryParse(address.Substring(i, 1), out l))
                {
                    rowPart = address.Substring(i, address.Length - i);
                    if (UInt32.TryParse(rowPart, out l))
                    {
                        result = l;
                        break;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Get or create the reference to a cell
        /// </summary>
        /// <param name="ws"></param>
        /// <param name="addressName">excel style cell address (like "B5")</param>
        /// <returns></returns>
        private static Cell InsertCellInWorksheet(Worksheet ws, string addressName)
        {
            SheetData sheetData = ws.GetFirstChild<SheetData>();
            Cell cell = null;

            UInt32 rowNumber = GetRowIndex(addressName);
            Row row = GetRow(sheetData, rowNumber);

            // If the cell you need already exists, return it.
            // If there is not a cell with the specified column name, insert one.  
            Cell refCell = row.Elements<Cell>().FirstOrDefault(c => c.CellReference.Value == addressName);
            if (refCell != null)
            {
                cell = refCell;
            }
            else
            {
                cell = CreateCell(row, addressName);
            }
            return cell;
        }

        /// <summary>
        /// Insert value in shared string object needed to insert in XML Later
        /// </summary>
        /// <param name="wbPart"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static int InsertSharedStringItem(WorkbookPart wbPart, string value)
        {
            int index = 0;
            bool found = false;

            SharedStringTablePart stringTablePart;
            if (wbPart.GetPartsOfType<SharedStringTablePart>().Count() > 0)
            {
                stringTablePart = wbPart.GetPartsOfType<SharedStringTablePart>().First();
            }
            else
            {
                stringTablePart = wbPart.AddNewPart<SharedStringTablePart>();
            }


            // If the part does not contain a SharedStringTable, create one.
            if (stringTablePart.SharedStringTable is null)
            {
                stringTablePart.SharedStringTable = new SharedStringTable();
            }

            // Iterate through all the items in the SharedStringTable. 
            // If the text already exists, return its index, otherwise add it.
            foreach (SharedStringItem item in stringTablePart.SharedStringTable.Elements<SharedStringItem>())
            {
                if (item.InnerText == value)
                {
                    found = true;
                    break;
                }
                index += 1;
            }

            if (!found)
            {
                stringTablePart.SharedStringTable.AppendChild(new SharedStringItem(new Text(value)));
                stringTablePart.SharedStringTable.Save();
            }

            return index;
        }

        /// <summary>
        /// Updates a cell value in a workbook sheet
        /// </summary>
        /// <param name="wbPart">WorkBookPart</param>
        /// <param name="sheetName">Workbook sheet to edit</param>
        /// <param name="colAddress">excel cell column address (e.g. A)</param>
        /// <param name="rowIndex">excel cell row index (e.g. 1)</param>
        /// <param name="value">value for cell</param>
        /// <param name="styleIndex"> styling to apply to cell - defaults to 0 (no style</param>)
        /// <param name="isString">string or number - defaults to true (string) </param>
        /// <returns></returns>
        public static bool UpdateCellValue(WorkbookPart wbPart, string sheetName, string colAddress, int rowIndex,
                                            string value, int styleIndex = 0,  bool isString = true)
        {
            string cellAddress = colAddress + rowIndex.ToString();
            bool updated = false;
            Sheet sheet = wbPart.Workbook.Descendants<Sheet>().FirstOrDefault((s) => s.Name == sheetName);

            if (sheet != null)
            {
                Worksheet ws = ((WorksheetPart)(wbPart.GetPartById(sheet.Id))).Worksheet;
                Cell cell = InsertCellInWorksheet(ws, cellAddress);

                if (isString)
                {
                    // Either retrieve the index of an existing string,
                    // or insert the string into the shared string table
                    // and get the index of the new item.
                    int stringIndex = InsertSharedStringItem(wbPart, value);

                    cell.CellValue = new CellValue(stringIndex.ToString());
                    cell.DataType = new EnumValue<CellValues>(CellValues.SharedString);
                }
                else
                {
                    cell.CellValue = new CellValue(value);
                    cell.DataType = new EnumValue<CellValues>(CellValues.Number);
                }
                if (styleIndex > 0)
                    cell.StyleIndex = Convert.ToUInt32(styleIndex);

                // Save the worksheet and workbook.
                ws.Save();
                wbPart.Workbook.Save();
                updated = true;
            }

            return updated;
        }
        #endregion Utility functions

        #region Output functions
        /// <summary>
        /// Outputs duplicated users to Excel file
        /// Columns are user name, email, role, and updated
        /// </summary>
        /// <param name="fileName">File to edit</param>
        /// <param name="sheetName">Workbook sheet to edit</param>
        /// <param name="duplicatedUsersGrouped">Grouped list of duplicated users</param>
        /// <param name="zdUsers">List of users</param>
        /// <returns></returns>
        public static void OutputDuplicatedUsersToExcel(string fileName, string sheetName, IOrderedEnumerable<IGrouping<string, ZdUser>> duplicatedUsersGrouped, IEnumerable<ZdUser> zdUsers)
        {
            string ColHeaderUserName = "User Name";
            string ColAddrUserName = "A";
            string ColHeaderEmail = "Email";
            string ColAddrEmail = "B";
            string ColHeaderRole = "Role";
            string ColAddrRole = "C";
            string ColHeaderUpdated = "Updated";
            string ColAddrUpdated = "D";
            string ColAddrFirst = ColAddrUserName;

            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite))
            {
                int rowIndex = 1;
                using (SpreadsheetDocument document = SpreadsheetDocument.Open(fs, true))
                {
                    WorkbookPart wbPart = document.WorkbookPart;

                    // Write counts
                    UpdateCellValue(wbPart, sheetName, ColAddrFirst, rowIndex,
                                    $"Total # user records: {zdUsers.Count()}, # duplicated users: {duplicatedUsersGrouped.Count()}");
                    rowIndex += 2;

                    // Write column headers
                    UpdateCellValue(wbPart, sheetName, ColAddrUserName, rowIndex, ColHeaderUserName);
                    UpdateCellValue(wbPart, sheetName, ColAddrEmail, rowIndex, ColHeaderEmail);
                    UpdateCellValue(wbPart, sheetName, ColAddrRole, rowIndex, ColHeaderRole);
                    UpdateCellValue(wbPart, sheetName, ColAddrUpdated, rowIndex, ColHeaderUpdated);

                    // Write the user data
                    bool firstLine;
                    foreach (var userGroup in duplicatedUsersGrouped)
                    {
                        firstLine = true;
                        foreach (var user in userGroup)
                        {
                            ++rowIndex;
                            if (firstLine)
                            {
                                UpdateCellValue(wbPart, sheetName, ColAddrUserName, rowIndex, user.Name);
                                firstLine = false;
                            }
                            UpdateCellValue(wbPart, sheetName, ColAddrEmail, rowIndex, user.Email);
                            UpdateCellValue(wbPart, sheetName, ColAddrRole, rowIndex, user.Role);
                            UpdateCellValue(wbPart, sheetName, ColAddrUpdated, rowIndex, user.UpdatedAt.ToString());

                        }
                    }

                }
            }

        }
        #endregion Output functions
    }
}
