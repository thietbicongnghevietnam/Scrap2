using OfficeOpenXml;
using ScrapSystem.Api.Application.DTOs.MaterialName;
using ScrapSystem.Api.Application.DTOs.ScrapDetailDtos;
using ScrapSystem.Api.Application.DTOs.ScrapDtos;
using ScrapSystem.Api.Domain.Models;
using System.Data;
using System.IO;
using System.Reflection;
using Serilog;
using ScrapSystem.Api.Application.DTOs.VerifyDataDtos;
using ScrapSystem.Api.Application.DTOs.AppendixDtos;
using static System.Runtime.InteropServices.JavaScript.JSType;
using ScrapSystem.Api.Application.Common;
using OfficeOpenXml.Style;
using System;
using Microsoft.AspNetCore.Routing.Template;
using ScrapSystem.Api.Application.DTOs.LabelListDtos;
using ScrapSystem.Api.Application.Response;
using SixLabors.Fonts.Unicode;

namespace ScrapSystem.Api.Utilities;
public class ExcelHelper
{
    public ExcelHelper()
    {

    }

    public List<VerificationResult> ExcelFileToolToDataTable(IFormFile file)
    {
        try
        {
            using (var stream = file.OpenReadStream())
            using (var package = new ExcelPackage(stream))
            {
                var result = new List<VerificationResult>();

                var worksheet = package.Workbook.Worksheets[1];

                if (worksheet == null)
                    throw new ArgumentException("One or more required worksheets are missing.");

                int startRow = 4;
                if (worksheet?.Dimension == null) return result;
                int endRow = worksheet.Dimension.End.Row;
                for (int row = startRow; row <= endRow; row++)
                {
                    string material = worksheet.Cells[row, 3].Text;
                    if (string.IsNullOrWhiteSpace(material))
                        continue;

                    float quantity = 0;
                    float.TryParse(worksheet.Cells[row, 4].Text.Replace("-", string.Empty), out quantity);

                    var dr = new VerificationResult();
                    dr.Material = material;
                    dr.Sloc = worksheet.Cells[row, 2].Text;
                    dr.Qty = quantity;
                    dr.Sanction = worksheet.Cells[row, 16].Text;
                    dr.Section = worksheet.Cells[row, 15].Text;
                    result.Add(dr);
                }

                return result;
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error converting Excel to DataTable: " + ex.Message, ex);
        }
    }

    public List<VerificationResult> ExcelSAPToDataTable(IFormFile file)
    {

        try
        {
            using (var stream = file.OpenReadStream())
            using (var package = new ExcelPackage(stream))
            {
                var result = new List<VerificationResult>();

                var worksheet = package.Workbook.Worksheets[0];
                var shortage = package.Workbook.Worksheets[1];
                var rohs = package.Workbook.Worksheets[2];

                if (worksheet == null || shortage == null || rohs == null)
                    throw new ArgumentException("One or more required worksheets are missing.");


                AddSheetSAPToTable(worksheet, result, 13, 16);
                AddSheetSAPToTable(shortage, result, 12, 16);
                AddSheetSAPToTable(rohs, result, 12, 16);

                return result;
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error converting Excel to DataTable: " + ex.Message, ex);
        }
    }

    private void AddSheetSAPToTable(ExcelWorksheet sheet, List<VerificationResult> lst, int slocCol, int qtyCol)
    {
        if (sheet?.Dimension == null) return;
        int startRow = 2;
        int endRow = sheet.Dimension.End.Row;
        for (int row = startRow; row <= endRow; row++)
        {
            string material = sheet.Cells[row, 4].Text;
            if (string.IsNullOrWhiteSpace(material))
                continue;

            double quantity = 0;
            double.TryParse(sheet.Cells[row, qtyCol].Text.Replace("-", string.Empty), out quantity);

            var dr = new VerificationResult();
            dr.Material = material;
            dr.Sloc = sheet.Cells[row, slocCol].Text;
            dr.Qty = quantity;
            lst.Add(dr);
        }
    }


    /// <summary>
    /// Import Excel file (.xlsx) vào DataTable
    /// </summary>
    public (ScrapDto, List<ScrapDetailDto>) ExcelIssueOutToScrap(IFormFile file,string sanction, string section, int startRow = 15)
    //public (ScrapDto, List<ScrapDetailDto>,string tensacntion, string tensection) ExcelIssueOutToScrap(IFormFile file,string sanction, string section, int startRow = 15)
    {
        ScrapDto scrap = new ScrapDto();
        List<ScrapDetailDto> scrapDetalDtos = new List<ScrapDetailDto>();
        string sanction_new ="";
        string section_new = "";

        try
        {
            using (var stream = file.OpenReadStream())
            
            //code new 13.08.2025
            using (var package = new ExcelPackage(stream))
            {
                var worksheetData1 = package.Workbook.Worksheets[0];
                //var worksheetData = package.Workbook.Worksheets[1];

                //var worksheetData1 = package.Workbook.Worksheets["Form B"];
                //var worksheet = package.Workbook.Worksheets["Input"];  // se khong co typeupload => chi 1 form B duy nhat
                //string typeupload = worksheet.Cells[8, 6].Text;  // se khong co typeupload => chi 1 form B duy nhat
                string subType = worksheetData1.Cells[12, 20].Text; // worksheetData1.Cells[6, 3].Text;
                string date = DateTime.Now.ToString();// worksheetData1.Cells[8, 3].Text;
                string type = worksheetData1.Cells[12, 21].Text;// worksheetData1.Cells[10, 3].Text;

                scrap.Sanction = worksheetData1.Cells[12, 16].Text;// sanction;
                scrap.Section = worksheetData1.Cells[12, 18].Text;//section;
                scrap.SubType = subType;
                scrap.MoveType = type; // Quy tac cot Movetype se dua vao rule cua LOG ********** => cho ra du lieu cot phu luc
                scrap.IssueOutDate = DateTime.Parse(date);

                //gan lai so sanction => lay gia tri dung trong file excel
                sanction_new = worksheetData1.Cells[12, 16].Text;
                section_new = worksheetData1.Cells[12, 18].Text;

                //var worksheetData1 = package.Workbook.Worksheets["Form B"];
                //for (int row = startRow; row <= worksheetData1.Dimension.End.Row; row++)
                //for (int row = 16; row <= worksheetData1.Dimension.End.Row; row++)      //bat dau tu dong 16
                for (int row = 12; row <= worksheetData1.Dimension.End.Row; row++)      //bat dau tu dong 12
                {
                    if (scrap.Sanction == "" || scrap.Section == "")
                    {
                        //thoat khoi vong lap => khong cho insert
                        break;
                    }                   
                    else
                    {
                        //if (row == 58) 
                        //{
                        //    string materila = worksheetData1.Cells[row, 6].Text;
                        //}

                        var scrapDetail1 = new ScrapDetailDto();
                        int.TryParse(worksheetData1.Cells[row, 1].Text, out int stt);

                        if (stt == 0) return (scrap, scrapDetalDtos);
                        //if (stt == 0) return (scrap, scrapDetalDtos,"","");

                        scrapDetail1.Plant = worksheetData1.Cells[row, 2].Text;
                        scrapDetail1.Sloc = worksheetData1.Cells[row, 3].Text;
                        scrapDetail1.CostCenter = worksheetData1.Cells[row, 4].Text;
                        scrapDetail1.NameCost = worksheetData1.Cells[row, 5].Text;
                        scrapDetail1.Material = worksheetData1.Cells[row, 6].Text;
                        scrapDetail1.Qty = float.TryParse(worksheetData1.Cells[row, 7].Text, out float quantity) ? quantity : 0;
                        scrapDetail1.UnitPrice = decimal.TryParse(worksheetData1.Cells[row, 8].Text, out decimal unitPrice) ? unitPrice : 0;
                        scrapDetail1.Amount = decimal.TryParse(worksheetData1.Cells[row, 9].Text, out decimal amount) ? amount : 0;

                        //cot reason doi lai
                        scrapDetail1.Reason = worksheetData1.Cells[row, 17].Text;
                        //them cot remark   19.12.2025
                        scrapDetail1.Remark = worksheetData1.Cells[row, 12].Text;

                        scrapDetail1.Pallet = worksheetData1.Cells[row, 15].Text;           //12.10.2025 theo mau issue out moi
                        scrapDetail1.Noid = stt;         //12.10.2025 theo mau issue out moi
                        //scrapDetail1.Barcode = sanction_new + ";" + worksheetData1.Cells[row, 15].Text + ";" + section_new;         //12.10.2025 theo mau issue out moi
                        scrapDetail1.Barcode = sanction_new + ";" + worksheetData1.Cells[row, 15].Text + ";" + section_new + ";" + subType;

                        //========= // them moi cac cot 25.10.2025 //=====================
                        //scrapDetail1.Vendor = worksheetData1.Cells[row, 13].Text;
                        scrapDetail1.Vendor = worksheetData1.Cells[row, 34].Text;

                        scrapDetail1.issue_out_sloc = worksheetData1.Cells[row, 14].Text;
                        scrapDetail1.SoTK = worksheetData1.Cells[row, 22].Text;
                        //scrapDetail1.NgayTK = worksheetData1.Cells[row, 13].Text;
                        var cellValue = worksheetData1.Cells[row, 23].Text;
                        if (DateTime.TryParse(cellValue, out DateTime parsedDate))
                        {
                            scrapDetail1.NgayTK = parsedDate;
                        }
                        else
                        {
                            scrapDetail1.NgayTK = null;
                        }

                        scrapDetail1.Sotaisan = worksheetData1.Cells[row, 28].Text;
                        scrapDetail1.BookValue = worksheetData1.Cells[row, 29].Text;
                        scrapDetail1.ControlNo = worksheetData1.Cells[row, 30].Text;
                        scrapDetail1.Category = worksheetData1.Cells[row, 31].Text;
                        scrapDetail1.Currency = worksheetData1.Cells[row, 32].Text;
                        scrapDetail1.Picture = "";
                        scrapDetail1.MVT = subType;     //them cot MVT

                        scrapDetail1.TypeName = worksheetData1.Cells[row, 19].Text; // cot TYPE
                        scrapDetail1.MoveType = worksheetData1.Cells[row, 21].Text; // cot MoveType

                        scrapDetail1.UnitPriceAC = decimal.TryParse(worksheetData1.Cells[row, 10].Text, out decimal UnitPriceAC) ? UnitPriceAC : 0;
                        scrapDetail1.AmountAC = decimal.TryParse(worksheetData1.Cells[row, 11].Text, out decimal AmountAC) ? AmountAC : 0;

                        //check dieu kien vendor
                        string ck_MVT = worksheetData1.Cells[row, 19].Text;
                        int index = ck_MVT.IndexOf('.');
                        string ck_vendor = index >= 0 ? ck_MVT.Substring(0, index) : ck_MVT;

                        //string[] stringTypeID = { "2", "3", "4", "5", "8", "21" }; // Những Type sẽ phải điền Vendorcode
                        if (ck_vendor == "2" || ck_vendor == "3" || ck_vendor == "4" || ck_vendor == "5" || ck_vendor == "8" || ck_vendor == "9" || ck_vendor == "19")  //bo 21
                        {
                            if (scrapDetail1.Vendor == "")
                            {
                                //scrapDetail1.Vendor = worksheetData1.Cells[row, 13].Text;
                                //truong hop vendor null => khong cho nhap
                                break;
                            }
                        }

                        //tinh ra code phu luc
                        //scrapDetail1.Phuluc = worksheetData1.Cells[row, 27].Text;  //**** Quy tac cot Movetype se dua vao rule cua LOG ********** => cho ra du lieu cot phu luc
                        if (type == "25.Mold scrap")
                        {
                            scrapDetail1.Phuluc = "4";
                        }
                        else if (type == "3.Material shortage")
                        {
                            scrapDetail1.Phuluc = "x";
                        }
                        else
                        {
                            if (subType == "551" || subType == "201")
                            {
                                scrapDetail1.Phuluc = "1";
                            }
                            else if (subType == "555" || subType == "556" || subType == "559" || subType == "560")
                            {
                                scrapDetail1.Phuluc = "2";
                            }
                            else if (subType == "Recycle" || subType == "Tool/FA" || subType == "Mold")
                            {
                                scrapDetail1.Phuluc = "3";
                            }
                        }

                        
                        if (scrapDetail1.Plant == "" && scrapDetail1.Material == "")
                        {
                            break;
                        }                        
                        else
                        {
                            scrapDetalDtos.Add(scrapDetail1);
                        }
                    }                    
                }

                //theo ly thuyet dang khong dung format theo form A ******
                //if (typeupload == "Form A")
                //{
                //    var worksheetData = package.Workbook.Worksheets["Form A"];
                //    for (int row = startRow; row <= worksheetData.Dimension.End.Row; row++)
                //    {
                //        var scrapDetail = new ScrapDetailDto();
                //        int.TryParse(worksheetData.Cells[row, 1].Text, out int stt);
                //        if (stt == 0) return (scrap, scrapDetalDtos);
                //        scrapDetail.Plant = worksheetData.Cells[row, 2].Text;
                //        scrapDetail.Sloc = worksheetData.Cells[row, 3].Text;
                //        scrapDetail.CostCenter = worksheetData.Cells[row, 4].Text;
                //        scrapDetail.NameCost = worksheetData.Cells[row, 5].Text;
                //        scrapDetail.Material = worksheetData.Cells[row, 6].Text;
                //        scrapDetail.Qty = float.TryParse(worksheetData.Cells[row, 7].Text, out float quantity) ? quantity : 0;
                //        scrapDetail.UnitPrice = decimal.TryParse(worksheetData.Cells[row, 8].Text, out decimal unitPrice) ? unitPrice : 0;
                //        scrapDetail.Amount = decimal.TryParse(worksheetData.Cells[row, 9].Text, out decimal amount) ? amount : 0;
                //        scrapDetail.Reason = worksheetData.Cells[row, 10].Text;

                //        scrapDetail.Pallet = worksheetData.Cells[row, 13].Text;         //12.10.2025 theo mau issue out moi
                //        scrapDetail.Noid = stt;         //12.10.2025 theo mau issue out moi
                //        scrapDetail.Barcode = sanction+";"+ worksheetData.Cells[row, 13].Text+";"+ section;         //12.10.2025 theo mau issue out moi


                //        if (scrapDetail.Plant == "" && scrapDetail.Material == "")
                //        {
                //            break;
                //        }
                //        else
                //        {
                //            scrapDetalDtos.Add(scrapDetail);
                //        }
                //    }
                //}
                //else
                //{
                //    //from B
                //}
            }

            
        }
        catch (Exception ex)
        {
            Log.Error($"Error while processing Excel file: {ex.Message}", file.FileName);
            throw;
        }
        return (scrap, scrapDetalDtos);
        //return (scrap, scrapDetalDtos,sanction_new, section_new);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="file"></param>
    /// <param name="startRow"></param>
    /// <returns></returns>
    public List<MaterialNameDto> ExcelToMaterialName(IFormFile file, int startRow = 3)
    {
        List<MaterialNameDto> materialNameDtos = new List<MaterialNameDto>();
        try
        {
            using (var stream = file.OpenReadStream())
            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets[0];

                for (int row = startRow; row <= worksheet.Dimension.End.Row; row++)
                {
                    var materialNameDto = new MaterialNameDto();
                    if (string.IsNullOrEmpty(worksheet.Cells[row, 2].Text.Trim())) return materialNameDtos;

                    materialNameDto.Material = worksheet.Cells[row, 2].Text.Trim();
                    materialNameDto.EnglishName = worksheet.Cells[row, 3].Text.Trim();
                    materialNameDto.VietnameseName = worksheet.Cells[row, 4].Text.Trim();
                    materialNameDto.Unit = worksheet.Cells[row, 5].Text.Trim();
                    materialNameDto.UnitEcus = worksheet.Cells[row, 6].Text.Trim();

                    materialNameDtos.Add(materialNameDto);

                }
            }

        }
        catch (Exception)
        {

            throw;
        }

        return materialNameDtos;
    }

    /// <summary>
    /// Export DataTable thành file Excel (.xlsx)
    /// </summary>
    public Task<byte[]> ExportDataTableToExcel(List<AppendixDto> data, int appendix)
    {
        if (data == null || !data.Any())
            throw new ArgumentException("Data list cannot be null or empty.");

        string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppendixFormat.xlsx");

        if (!File.Exists(templatePath))
            throw new FileNotFoundException("Template file not found.", templatePath);

        var appendixData = new List<AppendixDto>();
        try
        {
            switch (appendix)
            {
                case (int)Appendix.APPENDIX1:
                    appendixData = data.Where(x => Commons.appendix1.Contains(x.MoveType)).ToList();
                    break;
                case (int)Appendix.APPENDIX2:
                    appendixData = data.Where(x => Commons.appendix2.Contains(x.MoveType)).ToList();
                    break;
                case (int)Appendix.APPENDIX3:
                    appendixData = data.Where(x => x.MoveType == null).ToList();
                    break;
                default:
                    break;
            }
            using (var package = new ExcelPackage(new FileInfo(templatePath)))
            {
                var worksheet = package.Workbook.Worksheets[appendix - 1];
                if (appendix == 3)
                    GenerateAppendixSheet3(worksheet, appendixData);
                else
                    GenerateAppendixSheet(worksheet, appendixData);
                return package.GetAsByteArrayAsync();
            }
        }
        catch (Exception)
        {

            throw;
        }
    }

    public void GenerateAppendixSheet3(ExcelWorksheet worksheet, List<AppendixDto> data)
    {
        try
        {
            if (worksheet == null)
                throw new InvalidOperationException("No worksheets found in the template file.");

            if (data.Any())
            {
                int halbStartRow = 4;
                worksheet.InsertRow(halbStartRow, data.Count);
                for (int i = 0; i < data.Count; i++)
                {
                    worksheet.Cells[i + halbStartRow, 1].Value = i + 1;
                    worksheet.Cells[i + halbStartRow, 3].Value = data[i].VietNameseName;
                    worksheet.Cells[i + halbStartRow, 4].Value = data[i].Qty;
                    worksheet.Cells[i + halbStartRow, 5].Value = data[i].Unit;
                    worksheet.Cells[i + halbStartRow, 2].Value = data[i].Material;
                }
                var range = worksheet.Cells[halbStartRow, 1, halbStartRow + data.Count, 9];
                range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to export data to Excel: {ex.Message}", ex);
        }
    }

    public void GenerateAppendixSheet(ExcelWorksheet worksheet, List<AppendixDto> data)
    {
        try
        {
            var halb = data.Where(x => x.SubType?.ToUpper() == Commons.HALB).ToList();
            var roh = data.Where(x => x.SubType?.ToUpper() == Commons.ROH).ToList();

            if (worksheet == null)
                throw new InvalidOperationException("No worksheets found in the template file.");
            int halbStartRow = 6;

            if (halb.Any())
            {
                worksheet.InsertRow(halbStartRow, halb.Count);
                for (int i = 0; i < halb.Count; i++)
                {
                    worksheet.Cells[i + halbStartRow, 1].Value = i + 1;
                    worksheet.Cells[i + halbStartRow, 2].Value = halb[i].Material;
                    worksheet.Cells[i + halbStartRow, 3].Value = halb[i].VietNameseName;
                    worksheet.Cells[i + halbStartRow, 4].Value = halb[i].Qty;
                    worksheet.Cells[i + halbStartRow, 5].Value = halb[i].Unit;
                }
            }

            if (roh.Any())
            {
                int rohStartRow = 10 + halb.Count;
                worksheet.InsertRow(rohStartRow, roh.Count);
                for (int i = 0; i < roh.Count; i++)
                {
                    worksheet.Cells[i + rohStartRow, 1].Value = i + 1;
                    worksheet.Cells[i + rohStartRow, 2].Value = roh[i].Material;
                    worksheet.Cells[i + rohStartRow, 3].Value = roh[i].VietNameseName;
                    worksheet.Cells[i + rohStartRow, 4].Value = roh[i].Qty;
                    worksheet.Cells[i + rohStartRow, 5].Value = roh[i].Unit;
                }
            }
            var range = worksheet.Cells[halbStartRow, 1, halbStartRow + data.Count, 5];
            range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to export data to Excel: {ex.Message}", ex);
        }
    }

    public bool PrintLabelList(List<LabelListMasterDto> masters, List<LabelListDetailDto> details)
    {
        if (masters == null || !masters.Any())
            throw new ArgumentException("Data list cannot be null or empty.");

        string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LabelListFormat.xlsx");

        if (!File.Exists(templatePath))
            throw new FileNotFoundException("Template file not found.", templatePath);

        var appendixData = new List<AppendixDto>();
        int startRow = 3;
        try
        {
            foreach (var master in masters)
            {
                using (var package = new ExcelPackage(new FileInfo(templatePath)))
                {

                    var worksheet = package.Workbook.Worksheets[0];
                    if (worksheet == null)
                        throw new InvalidOperationException("No worksheets found in the template file.");
                    worksheet.Cells[1, 2].Value = $"Label hàng hủy tháng {master.IssueOutDate.Month}.{master.IssueOutDate.Year} ";
                    worksheet.Cells[2, 3].Value = "Section: " + master.Section;
                    worksheet.Cells[2, 4].Value = "Sanction: " + master.Sanction;
                    worksheet.Cells[3, 3].Value = "Pallet: " + master.Pallet;
                    int i = 0;
                    foreach (var item in details)
                    {
                        worksheet.Cells[startRow + i, 2].Value = i + 1;
                        worksheet.Cells[startRow + i, 3].Value = item.Material;
                        worksheet.Cells[startRow + i, 4].Value = item.EnglishName;
                        worksheet.Cells[startRow + i, 5].Value = item.QtyActual;
                        worksheet.Cells[startRow + i, 6].Value = item.Pallet;
                        i++;
                    }

                    
                }
            }
            return true;
        }
        catch (Exception)
        {

            throw;
        }
    }
}
