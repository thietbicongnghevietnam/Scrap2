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
    {
        ScrapDto scrap = new ScrapDto();
        List<ScrapDetailDto> scrapDetalDtos = new List<ScrapDetailDto>();
        try
        {
            using (var stream = file.OpenReadStream())
            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets[0];
                var worksheetData = package.Workbook.Worksheets[1];

                string subType = worksheet.Cells[6, 3].Text;
                string date = worksheet.Cells[8, 3].Text;
                string type = worksheet.Cells[10, 3].Text;

                scrap.Sanction = sanction;
                scrap.Section = section;
                scrap.SubType = subType;
                scrap.MoveType = type;
                scrap.IssueOutDate = DateTime.Parse(date);

                for (int row = startRow; row <= worksheetData.Dimension.End.Row; row++)
                {


                    var scrapDetail = new ScrapDetailDto();

                    int.TryParse(worksheetData.Cells[row, 1].Text, out int stt);

                    if (stt == 0) return (scrap, scrapDetalDtos);
                    scrapDetail.Plant = worksheetData.Cells[row, 2].Text;
                    scrapDetail.Sloc = worksheetData.Cells[row, 3].Text;
                    scrapDetail.CostCenter = worksheetData.Cells[row, 4].Text;
                    scrapDetail.NameCost = worksheetData.Cells[row, 5].Text;
                    scrapDetail.Material = worksheetData.Cells[row, 6].Text;
                    scrapDetail.Qty = float.TryParse(worksheetData.Cells[row, 7].Text, out float quantity) ? quantity : 0;
                    scrapDetail.UnitPrice = decimal.TryParse(worksheetData.Cells[row, 8].Text, out decimal unitPrice) ? unitPrice : 0;
                    scrapDetail.Amount = decimal.TryParse(worksheetData.Cells[row, 9].Text, out decimal amount) ? amount : 0;
                    scrapDetail.Reason = worksheetData.Cells[row, 10].Text;
                    scrapDetalDtos.Add(scrapDetail);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Error while processing Excel file: {ex.Message}", file.FileName);
            throw;
        }
        return (scrap, scrapDetalDtos);
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
