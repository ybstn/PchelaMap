using System;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;

namespace PchelaMap.Areas.Identity.Data
{
    public class XlxsCreation
    {
        public void XlxsCreationUsers(List<PchelaMapUser> _users, List<string> roles, List<PchelaMapUserTasks> _userstasks, string _path)
        {
            using (SpreadsheetDocument _UsersTable = SpreadsheetDocument.Create(_path, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart workbookPart = _UsersTable.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                SheetData sheetData = new SheetData();
                worksheetPart.Worksheet = new Worksheet(sheetData);

                Sheets sheets = _UsersTable.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());

                Sheet _sheet = new Sheet()
                {
                    Id = _UsersTable.WorkbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = "Users"
                };

                sheets.Append(_sheet);

                Row headerRow = new Row();
                List<string> _columns = new List<string>();
                foreach (string headers in ExportDataTypes.UserRow)
                {
                    Cell _cell = new Cell();
                    _cell.DataType = CellValues.String;
                    _cell.CellValue = new CellValue(headers);
                    headerRow.AppendChild(_cell);
                }
                sheetData.AppendChild(headerRow);
                int _index = 0;
                foreach (PchelaMapUser user in _users)
                {
                    Row userRow = new Row();

                    Cell NameCell = new Cell();
                    NameCell.DataType = CellValues.String;
                    NameCell.CellValue = new CellValue(user.Name);
                    Cell MailCell = new Cell();
                    MailCell.DataType = CellValues.String;
                    MailCell.CellValue = new CellValue(user.Email);
                    Cell PhoneCell = new Cell();
                    PhoneCell.DataType = CellValues.String;
                    PhoneCell.CellValue = new CellValue(user.PhoneNumber);
                    Cell AdressCell = new Cell();
                    AdressCell.DataType = CellValues.String;
                    AdressCell.CellValue = new CellValue(user.UserAdress);
                    Cell RegDate = new Cell();
                    RegDate.DataType = CellValues.String;
                    RegDate.CellValue = new CellValue(user.CreatedDateUtc);
                    Cell RefusedTasks = new Cell();
                    RefusedTasks.DataType = CellValues.String;
                    RefusedTasks.CellValue = new CellValue(user.uncompletedTasks.ToString());
                    Cell CreatedTasks = new Cell();
                    CreatedTasks.DataType = CellValues.String;
                    CreatedTasks.CellValue = new CellValue(user.Tasks.Count().ToString());
                    Cell TakenTasks = new Cell();
                    TakenTasks.DataType = CellValues.String;
                    TakenTasks.CellValue = new CellValue(_userstasks.Where(x => x.UserID == user.Id).Count().ToString());
                    Cell userPoints = new Cell();
                    userPoints.DataType = CellValues.String;
                    userPoints.CellValue = new CellValue(user.UserPoints.ToString());
                    Cell userRoles = new Cell();
                    userRoles.DataType = CellValues.String;
                    userRoles.CellValue = new CellValue(roles.ElementAt(_index));
                    userRow.AppendChild(NameCell);
                    userRow.AppendChild(MailCell);
                    userRow.AppendChild(PhoneCell);
                    userRow.AppendChild(AdressCell);
                    userRow.AppendChild(RegDate);
                    userRow.AppendChild(RefusedTasks);
                    userRow.AppendChild(CreatedTasks);
                    userRow.AppendChild(TakenTasks);
                    userRow.AppendChild(userPoints);
                    userRow.AppendChild(userRoles);
                    sheetData.AppendChild(userRow);
                    _index = _index++;
                }
                workbookPart.Workbook.Save();
            }
        }
        public void XlxsCreationTasks(List<PchelaMapTask> _tasks, List<PchelaMapUserTasks> _userstasks, List<PchelaMapUser> _users, string _path)
        {
            using (SpreadsheetDocument _UsersTable = SpreadsheetDocument.Create(_path, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart workbookPart = _UsersTable.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                SheetData sheetData = new SheetData();
                worksheetPart.Worksheet = new Worksheet(sheetData);

                Sheets sheets = _UsersTable.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());

                Sheet _sheet = new Sheet()
                {
                    Id = _UsersTable.WorkbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = "Users"
                };

                sheets.Append(_sheet);

                Row headerRow = new Row();
                List<string> _columns = new List<string>();
                foreach (string headers in ExportDataTypes.TaskRow)
                {
                    Cell _cell = new Cell();
                    _cell.DataType = CellValues.String;
                    _cell.CellValue = new CellValue(headers);
                    headerRow.AppendChild(_cell);
                }

                sheetData.AppendChild(headerRow);
                int _index = 0;
                foreach (PchelaMapTask task in _tasks)
                {
                    Row userRow = new Row();

                    Cell DescripCell = new Cell();
                    DescripCell.DataType = CellValues.String;
                    DescripCell.CellValue = new CellValue(task.Description);
                    Cell AdressCell = new Cell();
                    AdressCell.DataType = CellValues.String;
                    AdressCell.CellValue = new CellValue(task.Adress);
                    Cell UrgentCell = new Cell();
                    UrgentCell.DataType = CellValues.String;
                    if (task.Urgentable == 1)
                    {
                        UrgentCell.CellValue = new CellValue("срочное");
                    }
                    else
                    {
                        UrgentCell.CellValue = new CellValue("обычное");
                    }
                    Cell StatusCell = new Cell();
                    StatusCell.DataType = CellValues.String;
                    StatusCell.CellValue = new CellValue(GlobalStatusEditModel.GlobalTaskStatusDictionary[task.Status]);
                    Cell CreationDate = new Cell();
                    CreationDate.DataType = CellValues.String;
                    CreationDate.CellValue = new CellValue(task.CreatedDateUtc);
                    Cell ClosedDate = new Cell();
                    ClosedDate.DataType = CellValues.String;
                    ClosedDate.CellValue = new CellValue(task.ClosedDateUtc);
                    Cell UserCreated = new Cell();
                    UserCreated.DataType = CellValues.String;
                    UserCreated.CellValue = new CellValue(task.Name);
                    Cell UserTaken = new Cell();
                    UserTaken.DataType = CellValues.String;
                    Cell TakenDate = new Cell();
                    TakenDate.DataType = CellValues.String;
                    Cell DoneDate = new Cell();
                    DoneDate.DataType = CellValues.String;
                    if (_userstasks.Any(x => x.TaskID == task.id))
                    {
                        var userTakenId = _userstasks.FirstOrDefault(x => x.TaskID == task.id).UserID;
                        UserTaken.CellValue = new CellValue(_users.FirstOrDefault(x => x.Id == userTakenId).Name);
                        DoneDate.CellValue = new CellValue(_userstasks.FirstOrDefault(x => x.TaskID == task.id).DateDone);
                        TakenDate.CellValue = new CellValue(_userstasks.FirstOrDefault(x => x.TaskID == task.id).DateTaken);
                    }
                    else
                    {
                        UserTaken.CellValue = new CellValue("");
                        DoneDate.CellValue = new CellValue("");
                        TakenDate.CellValue = new CellValue("");
                    }

                    userRow.AppendChild(DescripCell);
                    userRow.AppendChild(AdressCell);
                    userRow.AppendChild(UrgentCell);
                    userRow.AppendChild(StatusCell);
                    userRow.AppendChild(CreationDate);
                    userRow.AppendChild(TakenDate);
                    userRow.AppendChild(DoneDate);
                    userRow.AppendChild(ClosedDate);
                    userRow.AppendChild(UserCreated);
                    userRow.AppendChild(UserTaken);
                    sheetData.AppendChild(userRow);
                    _index = _index++;
                }
                workbookPart.Workbook.Save();
            }
        }
    }
}
