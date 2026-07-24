using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using iTextSharp.text;
using iTextSharp.text.pdf;
using SkiaSharp;
using Svg.Skia;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using Paragraph = iTextSharp.text.Paragraph;

namespace CmsApi.Helpers
{
    public static class PdfConverter
    {
        public static byte[] ConvertToPdf(byte[] fileBytes, string contentType)
        {
            return contentType switch
            {
                // IMAGE
                "image/jpeg" => JpegToPdf(fileBytes),
                "image/png" => PngToPdf(fileBytes),
                "image/gif" => GifToPdf(fileBytes),
                "image/bmp" => BmpToPdf(fileBytes),
                "image/tiff" => TiffToPdf(fileBytes),
                "image/webp" => WebpToPdf(fileBytes),
                "image/x-icon" => IcoToPdf(fileBytes),
                "image/svg+xml" => SvgToPdf(fileBytes),

                // DOCUMENTS
                "application/pdf" => fileBytes, // Artıq PDF-dir, geri qaytar
                "application/msword" => DocxToPdf(fileBytes), // .doc
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => DocxToPdf(fileBytes), // .docx
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" => XlsxToPdf(fileBytes), // .xlsx
                "application/vnd.openxmlformats-officedocument.presentationml.presentation" => PptxToPdf(fileBytes), // .pptx

                // TEXT
                "text/plain" => TextToPdf(fileBytes),
                "application/json" => JsonToPdf(fileBytes),
                "application/xml" or "text/xml" => XmlToPdf(fileBytes),
                "text/html" => HtmlToPdf(fileBytes),

                // AUDIO
                "audio/mpeg" or "audio/mp3" => AudioToPdf(fileBytes, "audio.mp3"),
                "audio/wav" => AudioToPdf(fileBytes, "audio.wav"),
                "audio/ogg" => AudioToPdf(fileBytes, "audio.ogg"),
                "audio/flac" => AudioToPdf(fileBytes, "audio.flac"),

                // VIDEO
                "video/mp4" => VideoToPdf(fileBytes, "video.mp4"),
                "video/webm" => VideoToPdf(fileBytes, "video.webm"),
                "video/x-msvideo" => VideoToPdf(fileBytes, "video.avi"),

                // ARCHIVE
                "application/zip" => ArchiveToPdf(fileBytes, "archive.zip"),
                "application/x-rar-compressed" => ArchiveToPdf(fileBytes, "archive.rar"),
                "application/gzip" => ArchiveToPdf(fileBytes, "archive.gz"),
                "application/x-7z-compressed" => ArchiveToPdf(fileBytes, "archive.7z"),
                "application/x-tar" => ArchiveToPdf(fileBytes, "archive.tar"),

                // DEFAULT
                _ => throw new NotSupportedException($"Content type '{contentType}' PDF-ə çevrilə bilmir")
            };
        }

        public static byte[] ImageToPdf(byte[] imageBytes, string contentType)
        {
            using (var ms = new MemoryStream())
            {
                var document = new iTextSharp.text.Document();
                PdfWriter.GetInstance(document, ms);
                document.Open();

                try
                {
                    // JPEG və PNG birbaşa işləyir
                    if (contentType == "image/jpeg" || contentType == "image/png")
                    {
                        iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(imageBytes);
                        img.ScaleToFit(document.PageSize.Width - 50, document.PageSize.Height - 50);
                        img.Alignment = iTextSharp.text.Image.ALIGN_CENTER;
                        document.Add(img);
                    }
                    else
                    {
                        // Digər formatları PNG-ə çevir
                        byte[] pngBytes = ConvertImageToPng(imageBytes);
                        iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(pngBytes);
                        img.ScaleToFit(document.PageSize.Width - 50, document.PageSize.Height - 50);
                        img.Alignment = iTextSharp.text.Image.ALIGN_CENTER;
                        document.Add(img);
                    }
                }
                catch
                {
                    // Əgər iTextSharp tanımırsa, PNG-ə çevir
                    byte[] pngBytes = ConvertImageToPng(imageBytes);
                    iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(pngBytes);
                    img.ScaleToFit(document.PageSize.Width - 50, document.PageSize.Height - 50);
                    img.Alignment = iTextSharp.text.Image.ALIGN_CENTER;
                    document.Add(img);
                }

                document.Close();
                return ms.ToArray();
            }
        }

        // Helper: Hər hansı şəkli PNG-ə çevir
        private static byte[] ConvertImageToPng(byte[] imageBytes)
        {
            using (var inputStream = new MemoryStream(imageBytes))
            using (var outputStream = new MemoryStream())
            using (var bitmap = new Bitmap(inputStream))
            {
                bitmap.Save(outputStream, ImageFormat.Png);
                return outputStream.ToArray();
            }
        }

        // ========== JPEG -> PDF ==========
        public static byte[] JpegToPdf(byte[] jpegBytes)
        {
            return ImageToPdf(jpegBytes, "image/jpeg");
        }

        // ========== PNG -> PDF ==========
        public static byte[] PngToPdf(byte[] pngBytes)
        {
            return ImageToPdf(pngBytes, "image/png");
        }

        // ========== GIF -> PDF ==========
        public static byte[] GifToPdf(byte[] gifBytes)
        {
            return ImageToPdf(gifBytes, "image/gif");
        }

        // ========== BMP -> PDF ==========
        public static byte[] BmpToPdf(byte[] bmpBytes)
        {
            return ImageToPdf(bmpBytes, "image/bmp");
        }

        // ========== WEBP -> PDF ==========
        public static byte[] WebpToPdf(byte[] webpBytes)
        {
            return ImageToPdf(webpBytes, "image/webp");
        }

        // ========== ICO -> PDF ==========
        public static byte[] IcoToPdf(byte[] icoBytes)
        {
            return ImageToPdf(icoBytes, "image/x-icon");
        }

        // ========== TIFF -> PDF ==========
        public static byte[] TiffToPdf(byte[] tiffBytes)
        {
            using (var ms = new MemoryStream())
            {
                var document = new iTextSharp.text.Document();
                PdfWriter.GetInstance(document, ms);
                document.Open();

                using (var imgStream = new MemoryStream(tiffBytes))
                {
                    System.Drawing.Image tiffImage = System.Drawing.Image.FromStream(imgStream);
                    int frameCount = tiffImage.GetFrameCount(FrameDimension.Page);

                    for (int i = 0; i < frameCount; i++)
                    {
                        tiffImage.SelectActiveFrame(FrameDimension.Page, i);
                        using (var frameMs = new MemoryStream())
                        {
                            tiffImage.Save(frameMs, ImageFormat.Png);
                            iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(frameMs.ToArray());
                            img.ScaleToFit(document.PageSize.Width - 50, document.PageSize.Height - 50);
                            document.Add(img);
                            if (i < frameCount - 1) document.NewPage();
                        }
                    }
                }

                document.Close();
                return ms.ToArray();
            }
        }

        // ========== SVG -> PDF ==========
        public static byte[] SvgToPdf(byte[] svgBytes)
        {
            using (var ms = new MemoryStream())
            {
                var document = new iTextSharp.text.Document();
                PdfWriter.GetInstance(document, ms);
                document.Open();

                string svgContent = Encoding.UTF8.GetString(svgBytes);

                using (var svgStream = new MemoryStream(svgBytes))
                {
                    var svg = new SKSvg();
                    svg.Load(svgStream);

                    if (svg.Picture != null)
                    {
                        var bounds = svg.Picture.CullRect;
                        var info = new SKImageInfo((int)bounds.Width, (int)bounds.Height);

                        using (var surface = SKSurface.Create(info))
                        {
                            var canvas = surface.Canvas;
                            canvas.Clear(SKColors.White);
                            canvas.DrawPicture(svg.Picture);

                            using (var image = surface.Snapshot())
                            using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                            {
                                byte[] pngBytes = data.ToArray();
                                iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(pngBytes);
                                img.ScaleToFit(document.PageSize.Width - 50, document.PageSize.Height - 50);
                                img.Alignment = iTextSharp.text.Image.ALIGN_CENTER;
                                document.Add(img);
                            }
                        }
                    }
                }

                document.Close();
                return ms.ToArray();
            }
        }

        // ========== TEXT -> PDF ==========
        public static byte[] TextToPdf(byte[] textBytes)
        {
            string text = Encoding.UTF8.GetString(textBytes);
            using (var ms = new MemoryStream())
            {
                var document = new iTextSharp.text.Document();
                PdfWriter.GetInstance(document, ms);
                document.Open();

                var font = FontFactory.GetFont(FontFactory.COURIER, 10);
                var paragraph = new Paragraph(text, font);
                document.Add(paragraph);

                document.Close();
                return ms.ToArray();
            }
        }

        // ========== DOCX -> PDF ==========
        public static byte[] DocxToPdf(byte[] docxBytes)
        {
            using (var docxStream = new MemoryStream(docxBytes))
            using (var wordDoc = WordprocessingDocument.Open(docxStream, false))
            {
                var body = wordDoc.MainDocumentPart.Document.Body;
                string text = string.Join("\n",
                    body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>()
                        .Select(p => p.InnerText));

                using (var ms = new MemoryStream())
                {
                    var document = new iTextSharp.text.Document();
                    iTextSharp.text.pdf.PdfWriter.GetInstance(document, ms);
                    document.Open();

                    var paragraph = new iTextSharp.text.Paragraph(text);
                    document.Add(paragraph);

                    document.Close();
                    return ms.ToArray();
                }
            }
        }

        // ========== XLSX -> PDF ==========
        public static byte[] XlsxToPdf(byte[] xlsxBytes)
        {
            // Excel-i oxuyub PdfPTable ilə PDF-ə çevir
            // EPPlus və ya ClosedXML istifadə et
            using (var ms = new MemoryStream())
            {
                var document = new iTextSharp.text.Document();
                PdfWriter.GetInstance(document, ms);
                document.Open();

                var paragraph = new Paragraph("Excel məzmunu (parse lazımdır)");
                document.Add(paragraph);

                document.Close();
                return ms.ToArray();
            }
        }

        // ========== PPTX -> PDF ==========
        public static byte[] PptxToPdf(byte[] pptxBytes)
        {
            // Slide-ları oxu və hər slide-ı səhifə kimi PDF-ə əlavə et
            using (var ms = new MemoryStream())
            {
                var document = new iTextSharp.text.Document();
                PdfWriter.GetInstance(document, ms);
                document.Open();

                var paragraph = new Paragraph("PowerPoint məzmunu (parse lazımdır)");
                document.Add(paragraph);

                document.Close();
                return ms.ToArray();
            }
        }

        // ========== JSON -> PDF ==========
        public static byte[] JsonToPdf(byte[] jsonBytes)
        {
            string json = Encoding.UTF8.GetString(jsonBytes);
            return TextToPdf(jsonBytes); // formatlanmış versiya üçün JSON pretty print
        }

        // ========== XML -> PDF ==========
        public static byte[] XmlToPdf(byte[] xmlBytes)
        {
            string xml = Encoding.UTF8.GetString(xmlBytes);
            return TextToPdf(xmlBytes);
        }

        // ========== HTML -> PDF ==========
        public static byte[] HtmlToPdf(byte[] htmlBytes)
        {
            string html = Encoding.UTF8.GetString(htmlBytes);
            using (var ms = new MemoryStream())
            {
                var document = new iTextSharp.text.Document();
                PdfWriter writer = PdfWriter.GetInstance(document, ms);
                document.Open();

                // iTextSharp.tool.xml istifadə edərək HTML parse et
                using (var htmlStream = new MemoryStream(htmlBytes))
                {
                    iTextSharp.tool.xml.XMLWorkerHelper.GetInstance().ParseXHtml(writer, document, htmlStream, Encoding.UTF8);
                }

                document.Close();
                return ms.ToArray();
            }
        }

        // ========== MP3 -> PDF (metadata) ==========
        public static byte[] AudioToPdf(byte[] audioBytes, string fileName)
        {
            using (var ms = new MemoryStream())
            {
                var document = new iTextSharp.text.Document();
                PdfWriter.GetInstance(document, ms);
                document.Open();

                var paragraph = new Paragraph($"Audio File: {fileName}\nSize: {audioBytes.Length} bytes\n(Audio faylı PDF-də embed oluna bilməz, yalnız metadata)");
                document.Add(paragraph);

                document.Close();
                return ms.ToArray();
            }
        }

        // ========== VIDEO -> PDF (metadata) ==========
        public static byte[] VideoToPdf(byte[] videoBytes, string fileName)
        {
            using (var ms = new MemoryStream())
            {
                var document = new iTextSharp.text.Document();
                PdfWriter.GetInstance(document, ms);
                document.Open();

                var paragraph = new Paragraph($"Video File: {fileName}\nSize: {videoBytes.Length} bytes\n(Video PDF-ə birbaşa çevrilə bilməz)");
                document.Add(paragraph);

                document.Close();
                return ms.ToArray();
            }
        }

        // ========== ZIP/RAR/ARCHIVE -> PDF (list) ==========
        public static byte[] ArchiveToPdf(byte[] archiveBytes, string fileName)
        {
            using (var ms = new MemoryStream())
            {
                var document = new iTextSharp.text.Document();
                PdfWriter.GetInstance(document, ms);
                document.Open();

                var paragraph = new Paragraph($"Archive File: {fileName}\nSize: {archiveBytes.Length} bytes\n(Arxiv faylları üçün siyahı göstərilə bilər)");
                document.Add(paragraph);

                document.Close();
                return ms.ToArray();
            }
        }
    }
}
