using CmsApi.Helpers;
using CmsApi.Http.Handlers.Interfaces;
using CmsApi.Services.Interfaces;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Caching.Memory;
using SkiaSharp;
using System.IO;
using System.Text;
using Tesseract;
using UglyToad.PdfPig;

namespace CmsApi.Services.Implementations
{
    public class DocumentService : IDocumentService
    {
        private readonly ICmsHttpHandler _cmsHttpHandler;
        private static readonly MemoryCache _attachments = new(new MemoryCacheOptions());



        public DocumentService(ICmsHttpHandler cmsHttpHandler)
        {
            _cmsHttpHandler = cmsHttpHandler;
        }

        public async Task<byte[]?> GetDocument(string attachmentId)
        {
            if (_attachments.TryGetValue(attachmentId, out byte[] bytes))
                return bytes;

            var result = await _cmsHttpHandler.GetDocumentAsBase64Async(attachmentId);

            if(result.Result.Content == null)
            {
                return null;
            }

            var resultConvert = Base64FileTypeDetector.DetectContentType(result.Result.Content);

            bytes = resultConvert.Item2;

            _attachments.Set(
                attachmentId,
                bytes,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12)
                });

            return bytes;
        }

        public string ExtractText(byte[] pdfBytes)
        {
            using var stream = new MemoryStream(pdfBytes);
            using var document = UglyToad.PdfPig.PdfDocument.Open(stream);

            // Əvvəl yoxla görək PDF-də text varmı
            bool hasText = document.GetPages().Any(p => p.Letters.Count > 0);

            if (hasText)
            {
                var sb = new StringBuilder();

                foreach (var page in document.GetPages())
                {
                    sb.AppendLine(page.Text);
                }

                var text = sb.ToString();

                if (LooksCorrupted(text))
                {
                    return ExtractTextWithOcr(pdfBytes);
                }

                return text;

            }

            // Burada artıq OCR işləməlidir
            return ExtractTextWithOcr(pdfBytes);
        }

        private bool LooksCorrupted(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return true;

            int suspiciousChars = text.Count(c =>
                c == '$' ||
                c == '§' ||
                c == '�');

            int badChars = text.Count(c =>
                c == 'п' ||
                c == 'ч' ||
                c == 'э' ||
                c == 'ё' ||
                c == 'ї');

            // Encoding/OCR problemi əlamətləri
            if (suspiciousChars > 5)
                return true;

            if (badChars > 20)
                return true;

            return false;
        }

        //private string ExtractTextWithOcr(byte[] pdfBytes)
        //{
        //    try
        //    {

        //        var sb = new StringBuilder();

        //        int pageCount;
        //        using (var countStream = new MemoryStream(pdfBytes))
        //        {
        //            pageCount = PDFtoImage.Conversion.GetPageCount(countStream);
        //        }

        //        using var engine = new TesseractEngine(@"./tessdata", "aze+eng", EngineMode.Default);

        //        for (int i = 0; i < pageCount; i++)
        //        {
        //            using var pageStream = new MemoryStream(pdfBytes); // fresh, undisposed stream per page

        //            using SKBitmap bitmap = PDFtoImage.Conversion.ToImage(
        //                pageStream,
        //                page: i,
        //                options: new PDFtoImage.RenderOptions(Dpi: 300));

        //            using var pngStream = new MemoryStream();
        //            bitmap.Encode(pngStream, SKEncodedImageFormat.Png, 100);
        //            pngStream.Seek(0, SeekOrigin.Begin);

        //            using var pix = Pix.LoadFromMemory(pngStream.ToArray());
        //            using var page = engine.Process(pix);

        //            sb.AppendLine(page.GetText());
        //        }

        //        return sb.ToString();
        //    }
        //    catch (Exception ex)
        //    {

        //        throw;
        //    }
        //}

        private string ExtractTextWithOcr(byte[] pdfBytes)
        {
            var sb = new StringBuilder();

            int pageCount;
            using (var countStream = new MemoryStream(pdfBytes))
            {
                pageCount = PDFtoImage.Conversion.GetPageCount(countStream);
            }

            using var engine = new TesseractEngine(@"./tessdata", "aze+eng", EngineMode.Default);
            engine.DefaultPageSegMode = PageSegMode.SingleBlock;

            // OSD engine BİR DƏFƏ yaradılır, hər səhifə üçün yox — çox baha proses idi
            using var osdEngine = new TesseractEngine(@"./tessdata", "osd", EngineMode.TesseractOnly);

            for (int i = 0; i < pageCount; i++)
            {
                using var pageStream = new MemoryStream(pdfBytes);
                using SKBitmap rawBitmap = PDFtoImage.Conversion.ToImage(
                    pageStream,
                    page: i,
                    options: new PDFtoImage.RenderOptions(Dpi: 400));

                using SKBitmap processedBitmap = PreprocessForOcr(rawBitmap);

                using var pngStream = new MemoryStream();
                // Quality 100 lazımsızdır — 1-bit/gray şəkil üçün fərq yoxdur, sadəcə yavaşladır
                processedBitmap.Encode(pngStream, SKEncodedImageFormat.Png, 90);
                pngStream.Seek(0, SeekOrigin.Begin);

                using var pix = Pix.LoadFromMemory(pngStream.ToArray());
                using var correctedPix = CorrectOrientation(osdEngine, pix);

                using var page = engine.Process(correctedPix);
                sb.AppendLine(page.GetText());
            }

            return sb.ToString();
        }

        /// <summary>
        /// Boz tona çevirmə — unsafe pointer ilə birbaşa piksel buferinə giriş.
        /// GetPixel/SetPixel-dən 50-100 dəfə sürətlidir.
        /// </summary>
        private unsafe SKBitmap PreprocessForOcr(SKBitmap source)
        {
            // Mənbəni sürətli daxil olmaq üçün standart formata gətir (əgər deyilsə)
            using SKBitmap normalized = source.ColorType == SKColorType.Rgba8888
                ? source
                : source.Copy(SKColorType.Rgba8888);

            int w = normalized.Width;
            int h = normalized.Height;

            var grayBitmap = new SKBitmap(w, h, SKColorType.Gray8, SKAlphaType.Opaque);

            byte* srcPtr = (byte*)normalized.GetPixels().ToPointer();
            byte* dstPtr = (byte*)grayBitmap.GetPixels().ToPointer();
            int srcStride = normalized.RowBytes;
            int dstStride = grayBitmap.RowBytes;

            for (int y = 0; y < h; y++)
            {
                byte* srcRow = srcPtr + y * srcStride;
                byte* dstRow = dstPtr + y * dstStride;
                for (int x = 0; x < w; x++)
                {
                    byte r = srcRow[x * 4 + 0];
                    byte g = srcRow[x * 4 + 1];
                    byte b = srcRow[x * 4 + 2];
                    dstRow[x] = (byte)(0.299 * r + 0.587 * g + 0.114 * b);
                }
            }

            return ApplyAdaptiveThreshold(grayBitmap, blockSize: 25, c: 10);
        }

        /// <summary>
        /// Adaptiv threshold — unsafe pointer + integral image, GetPixel istifadə etmir.
        /// </summary>
        private unsafe SKBitmap ApplyAdaptiveThreshold(SKBitmap grayBitmap, int blockSize, int c)
        {
            int w = grayBitmap.Width;
            int h = grayBitmap.Height;
            var result = new SKBitmap(w, h, SKColorType.Gray8, SKAlphaType.Opaque);
            int half = blockSize / 2;

            byte* grayPtr = (byte*)grayBitmap.GetPixels().ToPointer();
            int grayStride = grayBitmap.RowBytes;

            // Integral image
            var integral = new long[h + 1, w + 1];
            for (int y = 0; y < h; y++)
            {
                byte* row = grayPtr + y * grayStride;
                long rowSum = 0;
                for (int x = 0; x < w; x++)
                {
                    rowSum += row[x];
                    integral[y + 1, x + 1] = integral[y, x + 1] + rowSum;
                }
            }

            byte* dstPtr = (byte*)result.GetPixels().ToPointer();
            int dstStride = result.RowBytes;

            for (int y = 0; y < h; y++)
            {
                int y0 = Math.Max(0, y - half);
                int y1 = Math.Min(h - 1, y + half);
                byte* grayRow = grayPtr + y * grayStride;
                byte* dstRow = dstPtr + y * dstStride;

                for (int x = 0; x < w; x++)
                {
                    int x0 = Math.Max(0, x - half);
                    int x1 = Math.Min(w - 1, x + half);

                    long sum = integral[y1 + 1, x1 + 1] - integral[y0, x1 + 1]
                             - integral[y1 + 1, x0] + integral[y0, x0];
                    int count = (y1 - y0 + 1) * (x1 - x0 + 1);
                    double mean = (double)sum / count;

                    dstRow[x] = grayRow[x] < (mean - c) ? (byte)0 : (byte)255;
                }
            }

            return result;
        }

        private Pix CorrectOrientation(TesseractEngine osdEngine, Pix pix)
        {
            try
            {
                using var page = osdEngine.Process(pix, PageSegMode.OsdOnly);

                page.DetectBestOrientationAndScript(
                    out int orientation,
                    out float orientConf,
                    out string script,
                    out float scriptConf);

                if (orientation == 0) return pix.Clone();

                float angle = orientation switch
                {
                    90 => (float)(Math.PI / 2),
                    180 => (float)Math.PI,
                    270 => (float)(3 * Math.PI / 2),
                    _ => 0
                };

                return pix.Rotate(angle);
            }
            catch
            {
                return pix.Clone();
            }
        }
    }
}
