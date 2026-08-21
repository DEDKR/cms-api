using System.IO.Compression;
using System.Text;

namespace CmsApi.Helpers
{
    public static class Base64FileTypeDetector
    {
        public static (string, byte[]) DetectContentType(string base64)
        {
            var bytes = Convert.FromBase64String(base64);
            var type = DetectContentType(bytes);

            if (type == "application/zip")
            {
                using var ms = new MemoryStream(bytes);
                using var archive = new ZipArchive(ms, ZipArchiveMode.Read);

                var pdfEntry = archive.Entries
                    .FirstOrDefault(e => e.FullName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase));

                if (pdfEntry != null)
                {
                    using var pdfStream = pdfEntry.Open();
                    using var output = new MemoryStream();

                    pdfStream.CopyTo(output);

                    return ("application/pdf", output.ToArray());
                }

                throw new InvalidOperationException("ZIP faylının içində PDF tapılmadı.");
            }

            if (type != "application/pdf")
            {
                bytes = PdfConverter.ConvertToPdf(bytes, type);
                type = "application/pdf";
            }

            return (type, bytes);
        }

        private static string DetectContentType(byte[] fileBytes)
        {
            if (fileBytes == null || fileBytes.Length < 4)
                return "application/octet-stream";

            // IMAGE
            if (Match(fileBytes, 0xFF, 0xD8, 0xFF)) return "image/jpeg";
            if (Match(fileBytes, 0x89, 0x50, 0x4E, 0x47)) return "image/png";
            if (Match(fileBytes, 0x47, 0x49, 0x46, 0x38)) return "image/gif";
            if (Match(fileBytes, 0x42, 0x4D)) return "image/bmp";
            if (Match(fileBytes, 0x49, 0x49, 0x2A, 0x00) || Match(fileBytes, 0x4D, 0x4D, 0x00, 0x2A)) return "image/tiff";
            if (Match(fileBytes, 0x52, 0x49, 0x46, 0x46) && Encoding.ASCII.GetString(fileBytes, 8, 4) == "WEBP") return "image/webp";
            if (Match(fileBytes, 0x00, 0x00, 0x01, 0x00)) return "image/x-icon";
            if (Match(fileBytes, 0x3C, 0x73, 0x76, 0x67)) return "image/svg+xml";

            // DOCUMENTS
            if (Match(fileBytes, 0x25, 0x50, 0x44, 0x46)) return "application/pdf";
            if (Match(fileBytes, 0xD0, 0xCF, 0x11, 0xE0)) return "application/msword"; // old office

            // DOCX / XLSX / PPTX (ZIP container)
            if (Match(fileBytes, 0x50, 0x4B, 0x03, 0x04))
                return DetectZipBasedOfficeFile(fileBytes); // deeper inspection lazımdır

            // TEXT
            if (IsText(fileBytes)) return "text/plain";

            // AUDIO
            if (Match(fileBytes, 0x49, 0x44, 0x33)) return "audio/mpeg"; // mp3
            if (Match(fileBytes, 0xFF, 0xFB)) return "audio/mpeg";
            if (Match(fileBytes, 0x52, 0x49, 0x46, 0x46) && Encoding.ASCII.GetString(fileBytes, 8, 4) == "WAVE") return "audio/wav";
            if (Match(fileBytes, 0x4F, 0x67, 0x67, 0x53)) return "audio/ogg";
            if (Match(fileBytes, 0x66, 0x4C, 0x61, 0x43)) return "audio/flac";

            // VIDEO
            if (Match(fileBytes, 0x00, 0x00, 0x00, 0x18) && Encoding.ASCII.GetString(fileBytes, 4, 4) == "ftyp") return "video/mp4";
            if (Match(fileBytes, 0x1A, 0x45, 0xDF, 0xA3)) return "video/webm";
            if (Match(fileBytes, 0x52, 0x49, 0x46, 0x46) && Encoding.ASCII.GetString(fileBytes, 8, 4) == "AVI ") return "video/x-msvideo";

            // ARCHIVE
            if (Match(fileBytes, 0x52, 0x61, 0x72, 0x21)) return "application/x-rar-compressed";
            if (Match(fileBytes, 0x1F, 0x8B)) return "application/gzip";
            if (Match(fileBytes, 0x37, 0x7A, 0xBC, 0xAF, 0x27, 0x1C)) return "application/x-7z-compressed";
            if (Match(fileBytes, 0x75, 0x73, 0x74, 0x61, 0x72)) return "application/x-tar";

            // JSON / XML / HTML
            var textStart = Encoding.UTF8.GetString(fileBytes.Take(50).ToArray()).TrimStart();

            if (textStart.StartsWith("{") || textStart.StartsWith("["))
                return "application/json";

            if (textStart.StartsWith("<!DOCTYPE html") || textStart.StartsWith("<html"))
                return "text/html";

            if (textStart.StartsWith("<"))
                return "application/xml";

            return "application/octet-stream";
        }

        private static bool Match(byte[] bytes, params byte[] signature)
        {
            if (bytes.Length < signature.Length) return false;

            for (int i = 0; i < signature.Length; i++)
            {
                if (bytes[i] != signature[i]) return false;
            }

            return true;
        }

        private static bool IsText(byte[] bytes)
        {
            return bytes.Take(512).All(b =>
                b == 9 || b == 10 || b == 13 || (b >= 32 && b <= 126));
        }

        private static string DetectZipBasedOfficeFile(byte[] fileBytes)
        {
            try
            {
                using var stream = new MemoryStream(fileBytes);
                using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

                var entries = archive.Entries
                    .Select(x => x.FullName.Replace('\\', '/'))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                // DOCX
                if (entries.Contains("word/document.xml"))
                    return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

                // XLSX
                if (entries.Contains("xl/workbook.xml"))
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                // PPTX
                if (entries.Contains("ppt/presentation.xml"))
                    return "application/vnd.openxmlformats-officedocument.presentationml.presentation";

                // Sadə ZIP
                return "application/zip";
            }
            catch
            {
                return "application/zip";
            }
        }
    }
}
