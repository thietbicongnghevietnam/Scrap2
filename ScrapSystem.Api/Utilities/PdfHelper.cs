using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Xml.Linq;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using Serilog;
using SixLabors.ImageSharp.Processing;
using Microsoft.AspNetCore.Razor.TagHelpers;

public class PdfHelper
{
    private const double PageWidth = 595; // A4 width in points
    private const double PageHeight = 842; // A4 height in points
    private const double Margin = 0;
    private const double ColumnGap = 20;
    private const double ImageSpacing = 10;
    private const double MaxImageHeight = 150;
    private const double TableBorderWidth = 4;
    private const double ColumnPadding = 10;

    public static byte[] ExportImagesToPdf(List<string> group1Images, List<string> group2Images, string barcode, DateTime date)
    {        
        string group1Title = $"Before: {barcode}"; string group2Title = "After";
        using (var stream = new MemoryStream())
        {
            PdfDocument document = null;

            try
            {
                // Tạo document PDF
                document = new PdfDocument();
                document.Info.Title = "Bảng hình ảnh 2 cột";
                document.Info.Author = "PDF Image Exporter";

                // Tính toán kích thước cột
                double columnWidth = (PageWidth - 2 * Margin - ColumnGap) / 2;
                double leftColumnX = Margin;
                double rightColumnX = Margin + columnWidth + ColumnGap;

                // Tính toán vùng table
                double tableX = Margin;
                double tableY = Margin;
                double tableWidth = PageWidth - 2 * Margin;
                double tableHeight = PageHeight - 2 * Margin;

                // Font cho tiêu đề
                var titleFont = new XFont("Arial", 24, XFontStyle.Bold);
                var titleColumnFont = new XFont("Arial", 14, XFontStyle.Bold);
                var pageFont = new XFont("Arial", 10);

                // Tạo trang mới
                PdfPage headPage = document.AddPage();
                headPage.Width = PageWidth;
                headPage.Height = PageHeight;
                XGraphics headGfx = XGraphics.FromPdfPage(headPage);
                double lineHeight = titleFont.GetHeight();
                XRect layoutRect = new XRect(50, 50, headPage.Width - 2 * 50, headPage.Height - 2 * 50);
                DrawStringWordWrapCentered(headGfx, $"Danh sách hàng hủy tháng {date.Month}.{date.Year} (Normal)", titleFont, XBrushes.Black, layoutRect, lineHeight);              
                
                /// ///code new 01.08.2025
                int maxCount = Math.Max(group1Images.Count, group2Images.Count);

                PdfPage currentPage = null;
                XGraphics gfx = null;

                double leftColumnY = 0;
                double rightColumnY = 0;

                for (int i = 0; i < maxCount; i++)
                {
                    //string beforeImage = i < group1Images.Count ? AppDomain.CurrentDomain.BaseDirectory + "wwwroot" + group1Images[i] : null;
                    //string afterImage = i < group2Images.Count ? AppDomain.CurrentDomain.BaseDirectory + "wwwroot" + group2Images[i] : null;

                    //doi B&A
                    string afterImage = i < group1Images.Count ? AppDomain.CurrentDomain.BaseDirectory + "wwwroot" + group1Images[i] : null;
                    string beforeImage = i < group2Images.Count ? AppDomain.CurrentDomain.BaseDirectory + "wwwroot" + group2Images[i] : null;

                    double group1ImageHeight = 0;
                    double group2ImageHeight = 0;

                    // Tính chiều cao ảnh cột trái
                    if (i < group1Images.Count && File.Exists(beforeImage))
                    {
                        using (var image = XImage.FromFile(beforeImage))
                        {
                            double aspectRatio = (double)image.PixelWidth / image.PixelHeight;
                            group1ImageHeight = (columnWidth - 2 * ColumnPadding) / aspectRatio;
                            group1ImageHeight = Math.Min(group1ImageHeight, MaxImageHeight);
                        }
                    }

                    // Tính chiều cao ảnh cột phải
                    if (i < group2Images.Count && File.Exists(afterImage))
                    {
                        using (var image = XImage.FromFile(afterImage))
                        {
                            double aspectRatio = (double)image.PixelWidth / image.PixelHeight;
                            group2ImageHeight = (columnWidth - 2 * ColumnPadding) / aspectRatio;
                            group2ImageHeight = Math.Min(group2ImageHeight, MaxImageHeight);
                        }
                    }

                    // Kiểm tra có cần trang mới không
                    bool needNewPage = currentPage == null;

                    if (group1ImageHeight > 0 && leftColumnY + group1ImageHeight > PageHeight - Margin - 10)
                        needNewPage = true;

                    if (group2ImageHeight > 0 && rightColumnY + group2ImageHeight > PageHeight - Margin - 10)
                        needNewPage = true;

                    // Nếu cần thì tạo trang mới
                    if (needNewPage)
                    {
                        if (gfx != null)
                        {
                            gfx.Dispose();
                            gfx = null;
                        }

                        currentPage = document.AddPage();
                        currentPage.Width = PageWidth;
                        currentPage.Height = PageHeight;
                        gfx = XGraphics.FromPdfPage(currentPage);

                        // Vẽ khung, header, tiêu đề cột
                        DrawTableBorders(gfx, tableX, tableY, tableWidth, tableHeight, columnWidth, ColumnGap);

                        leftColumnY = Margin + 80;
                        rightColumnY = Margin + 80;

                        var headerRect = new XRect(leftColumnX, 0, PageWidth, 40);
                        gfx.DrawString("Photo Of Disposal", titleFont, XBrushes.Black, headerRect, XStringFormats.Center);

                        DrawColumnHeaders(gfx, leftColumnX + 10, rightColumnX, Margin + 40, columnWidth,
                                          group1Title, group2Title, titleColumnFont);
                    }

                    // Vẽ ảnh cột trái nếu có
                    if (group1ImageHeight > 0)
                    {
                        var imgHeight = DrawImageInColumn(gfx, beforeImage, leftColumnX + ColumnPadding, leftColumnY,
                                        columnWidth - 2 * ColumnPadding, $"{group1Title} - Ảnh {i + 1}", pageFont);
                        leftColumnY += imgHeight + ImageSpacing;
                    }

                    // Vẽ ảnh cột phải nếu có
                    if (group2ImageHeight > 0)
                    {
                        var imgHeight = DrawImageInColumn(gfx, afterImage, rightColumnX + ColumnPadding, rightColumnY,
                                        columnWidth - 2 * ColumnPadding, $"{group2Title} - Ảnh {i + 1}", pageFont);
                        rightColumnY += imgHeight + ImageSpacing;
                    }
                }

                if (gfx != null)
                {
                    gfx.Dispose();
                    gfx = null;
                }

                document.Save(stream, false);  // luu file 1 lan roi => bo qua

                // Tạo tên file theo barcode + timestamp
                //string fileName = $"{barcode}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                //string fileName = $"{barcode}_{DateTime.Now:yyyyMMdd}.pdf";
                //string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                //Directory.CreateDirectory(folderPath);
                //string filePath = Path.Combine(folderPath, fileName);
                //File.WriteAllBytes(filePath, stream.ToArray());
                // Đặt lại vị trí con trỏ về đầu stream để đọc lại
                stream.Position = 0;

                // Tạo tên file theo barcode + timestamp
                string fileName = $"{barcode}_{DateTime.Now:yyyyMMdd}.pdf";

                // Lấy thư mục Downloads của user
                string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                Directory.CreateDirectory(folderPath);
                string filePath = Path.Combine(folderPath, fileName);

                // Ghi dữ liệu stream ra file
                File.WriteAllBytes(filePath, stream.ToArray());

            }

            catch (Exception ex)
            {
                Log.Error(ex.Message);
                Console.WriteLine($"Lỗi khi xuất PDF: {ex.Message}");
            }

            return stream.ToArray();
        }

    }

    public static MemoryStream FixImageOrientation(string imagePath)
    {
        using var image = Image.Load(imagePath); 
        image.Mutate(x => x.AutoOrient()); 

        var ms = new MemoryStream();
        image.SaveAsJpeg(ms); 
        ms.Position = 0;

        return ms; 
    }

    private static void DrawStringWordWrapCentered(XGraphics gfx, string text, XFont font, XBrush brush, XRect rect, double lineHeight)
    {
        string[] words = text.Split(' ');
        string line = "";
        double y = rect.Top;
        var lines = new List<string>();

        // First pass: wrap text and store lines
        foreach (string word in words)
        {
            string testLine = (line + word).Trim();
            double width = gfx.MeasureString(testLine, font).Width;

            if (width < rect.Width)
            {
                line = testLine + " ";
            }
            else
            {
                lines.Add(line.Trim());
                line = word + " ";
            }
        }

        if (!string.IsNullOrWhiteSpace(line))
            lines.Add(line.Trim());

        // Optional: vertically center block
        double totalHeight = lines.Count * lineHeight;
        y = rect.Top + (rect.Height - totalHeight) / 2;

        // Draw each line centered
        foreach (string l in lines)
        {
            double lineWidth = gfx.MeasureString(l, font).Width;
            double x = rect.Left + (rect.Width - lineWidth) / 2;

            gfx.DrawString(l, font, brush, new XPoint(x, y));
            y += lineHeight;
        }
    }


    // Vẽ border table và phân cách cột
    private static void DrawTableBorders(XGraphics gfx, double tableX, double tableY, double tableWidth,
                                           double tableHeight, double columnWidth, double columnGap)
    {
        var borderPen = new XPen(XColors.Black, TableBorderWidth);


        double separatorX = tableX + columnWidth + columnGap / 2;
        double headerY = tableY + 35;

        // Vẽ đường phân cách giữa 2 cột
        gfx.DrawLine(borderPen, separatorX, headerY, separatorX, tableY + tableHeight);

        // Vẽ border ngoài của table
        gfx.DrawRectangle(borderPen, tableX, headerY, tableWidth, tableHeight - headerY);

    }

    // Vẽ tiêu đề cột với background
    private static void DrawColumnHeaders(XGraphics gfx, double leftColumnX, double rightColumnX,
                                        double headerY, double columnWidth, string leftTitle,
                                        string rightTitle, XFont titleFont)
    {
        // Vẽ tiêu đề cột trái
        var leftRect = new XRect(leftColumnX, headerY, columnWidth, 20);
        gfx.DrawString(leftTitle, titleFont, XBrushes.Black, leftRect, XStringFormats.CenterLeft);

        // Vẽ tiêu đề cột phải
        var rightRect = new XRect(rightColumnX, headerY, columnWidth, 20);
        gfx.DrawString(rightTitle, titleFont, XBrushes.Black, rightRect, XStringFormats.CenterLeft);
    }

    private static double DrawImageInColumn(XGraphics gfx, string imagePath, double x, double y,
                                        double columnWidth, string caption, XFont font)

    {
        XImage image = null;
        try
        {
            var imageStream = FixImageOrientation(imagePath);
            image = XImage.FromStream(() => imageStream);
            //image = XImage.FromFile(imagePath);

            // Tính toán kích thước hình ảnh để fit trong cột
            double imageWidth = columnWidth - 10;
            double imageHeight = MaxImageHeight;

            // Tính tỷ lệ để giữ nguyên aspect ratio
            double aspectRatio = (double)image.PixelWidth / image.PixelHeight;

            imageHeight = imageWidth / aspectRatio;

            // Căn giữa hình ảnh trong cột
            double imageX = x + (columnWidth - imageWidth) / 2;

            // Vẽ border cho hình ảnh
            var pen = new XPen(XColors.Gray, 1);
            gfx.DrawRectangle(pen, imageX - 2, y - 2, imageWidth + 4, imageHeight + 4);

            // Vẽ hình ảnh
            gfx.DrawImage(image, imageX, y, imageWidth, imageHeight);

            return imageHeight;

        }
        catch (Exception ex)
        {
            return MaxImageHeight;
        }
        finally
        {
            if (image != null)
            {
                image.Dispose();
            }
        }
    }


    //public static byte[] ExportImagesByFolders(string outputPath, string folder1Path, string folder2Path,
    //                                       string group1Title = "Thư mục 1", string group2Title = "Thư mục 2")
    //{
    //    var group1Images = new List<string>();
    //    var group2Images = new List<string>();

    //    if (Directory.Exists(folder1Path))
    //    {
    //        var extensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
    //        foreach (var ext in extensions)
    //        {
    //            group1Images.AddRange(Directory.GetFiles(folder1Path, $"*{ext}", SearchOption.TopDirectoryOnly));
    //        }
    //    }

    //    if (Directory.Exists(folder2Path))
    //    {
    //        var extensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
    //        foreach (var ext in extensions)
    //        {
    //            group2Images.AddRange(Directory.GetFiles(folder2Path, $"*{ext}", SearchOption.TopDirectoryOnly));
    //        }
    //    }

    //    var rs = ExportImagesToPdf(group1Images, group2Images, "barcocde", Da);

    //    return rs;
    //}



}

