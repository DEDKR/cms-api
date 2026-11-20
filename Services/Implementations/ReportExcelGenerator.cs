using CmsApi.DTOs.Reports.Appeals;
using CmsApi.DTOs.Reports.Cassasion;
using CmsApi.DTOs.Reports.FirstInstance;
using CmsApi.DTOs.Reports.Meeting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace CmsApi.Services.Implementations
{
    /// <summary>
    /// Builds an Excel sheet in the style of the supplied Azerbaijani statistical report:
    /// white cells, black thin table grid, centered wrapped text, merged multilevel
    /// headings, a single consolidated total row and the requested date interval.
    /// Uses built-in ZipArchive/OpenXML; no Excel package or license is required.
    /// </summary>
    public static class ReportExcelGenerator
    {
        private sealed class Metric
        {
            public string Key { get; }
            public string Caption { get; }
            public Metric(string key, string caption) { Key = key; Caption = caption; }
        }
        private sealed class Section
        {
            public string Kind { get; }
            public string Caption { get; }
            public Metric[] Metrics { get; }
            public Section(string kind, string caption, params Metric[] metrics)
            {
                Kind = kind; Caption = caption; Metrics = metrics;
            }
        }
        private sealed class Office
        {
            public string Code = "";
            public string Name = "";
            public string Courts = "";
            public Dictionary<string, long> Values = new Dictionary<string, long>(StringComparer.Ordinal);
        }

        private static readonly Section First = new Section("first-instance", "1-ci instansiya",
            new Metric("totalCourts", "Məhkəmələrdə qaldırılmış iddiaların sayı"),
            new Metric("totalByResults", "Cəmi"),
            new Metric("approvedClaims", "Təmin edilən iddia ərizələrinin sayı"),
            new Metric("rejectedClaims", "Təmin edilməyən (rədd edilən) iddia ərizələrinin sayı"),
            new Metric("partialClaims", "Qismən təmin edilən iddiaların sayı"),
            new Metric("rejectedByCaseTermination", "İş üzrə icraata xitam verilən iddia ərizələrinin sayı"),
            new Metric("notConsideredClaims", "Mümkün sayılmayan iddia ərizələrinin sayı"),
            new Metric("respondedAppeals", "Cavabdeh orqan tərəfindən apellyasiya şikayəti verilən məhkəmə işlərinin sayı"));
        private static readonly Section Appeal = new Section("appeal", "Apellyasiya",
            new Metric("total", "Cəmi"),
            new Metric("approved", "Təmin edilib"),
            new Metric("rejected", "Təmin edilməyib"),
            new Metric("partiallyApproved", "Qismən təmin edilib"));
        private static readonly Section Cassation = new Section("cassation", "Kassasiya",
            new Metric("total", "Cəmi"),
            new Metric("approved", "Təmin edilib"),
            new Metric("rejected", "Təmin edilməyib"),
            new Metric("partiallyApproved", "Qismən təmin edilib"));
        private static readonly Section Meetings = new Section("meetings", "Məhkəmə iclaslarında iştirak sayları",
            new Metric("total", "Məhkəmə iclaslarında iştirak sayları"));
        private static readonly Section[] All = { First, Appeal, Cassation, Meetings };

        public static byte[] Build(
            string kind,
            DateTime? from,
            DateTime? to,
            int? courtTypeId,
            string? courtTypeLabel,
            IReadOnlyList<FirstInstanceResponse>? firstRows,
            IReadOnlyList<AppealResponse>? appealRows,
            IReadOnlyList<CassasionResponse>? cassationRows,
            IReadOnlyList<MeetingResponse>? meetingRows)
        {
            Section[] selected = kind == "all" ? All : All.Where(s => s.Kind == kind).ToArray();
            if (selected.Length == 0) throw new ArgumentException("Unknown report kind", nameof(kind));

            // One row per regional office; NEVER sum an already aggregated office twice.
            var offices = new Dictionary<string, Office>(StringComparer.OrdinalIgnoreCase);
            var namesToKeys = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            Office GetOffice(string code, string name)
            {
                var normalizedName = (name ?? "").Trim();
                var normalizedCode = (code ?? "").Trim();
                var key = normalizedCode.Length > 0 ? "id:" + normalizedCode : "name:" + normalizedName;
                if (!offices.ContainsKey(key) && namesToKeys.TryGetValue(normalizedName, out var oldKey))
                    key = oldKey;
                if (!offices.TryGetValue(key, out var office))
                {
                    office = new Office { Code = normalizedCode, Name = normalizedName };
                    offices.Add(key, office);
                }
                if (string.IsNullOrEmpty(office.Code)) office.Code = normalizedCode;
                if (string.IsNullOrEmpty(office.Name)) office.Name = normalizedName;
                if (normalizedName.Length > 0) namesToKeys[normalizedName] = key;
                return office;
            }
            void Save(Office office, string section, string metric, long number)
            {
                office.Values[section + "." + metric] = number;
            }
            if (selected.Contains(First))
                foreach (var r in firstRows ?? Array.Empty<FirstInstanceResponse>())
                {
                    var o = GetOffice(r.RegionalOfficeKeyCode, r.RegionalOfficeName);
                    if (!string.IsNullOrWhiteSpace(r.CourtName) && r.CourtName != "Məhkəmə təyin edilməyib")
                        o.Courts = r.CourtName;
                    Save(o, First.Kind, "totalCourts", r.TotalCourts);
                    Save(o, First.Kind, "totalByResults", r.TotalByResults);
                    Save(o, First.Kind, "approvedClaims", r.ApprovedClaims);
                    Save(o, First.Kind, "rejectedClaims", r.RejectedClaims);
                    Save(o, First.Kind, "partialClaims", r.PartialClaims);
                    Save(o, First.Kind, "rejectedByCaseTermination", r.RejectedByCaseTermination);
                    Save(o, First.Kind, "notConsideredClaims", r.NotConsideredClaims);
                    Save(o, First.Kind, "respondedAppeals", r.RespondedAppeals);
                }
            if (selected.Contains(Appeal))
                foreach (var r in appealRows ?? Array.Empty<AppealResponse>())
                {
                    var o = GetOffice(r.RegionalOfficeKeyCode, r.RegionalOfficeName);
                    if (string.IsNullOrEmpty(o.Courts) && r.CourtName != "Məhkəmə təyin edilməyib") o.Courts = r.CourtName;
                    Save(o, Appeal.Kind, "total", r.Total);
                    Save(o, Appeal.Kind, "approved", r.Approved);
                    Save(o, Appeal.Kind, "rejected", r.Rejected);
                    Save(o, Appeal.Kind, "partiallyApproved", r.PartiallyApproved);
                }
            if (selected.Contains(Cassation))
                foreach (var r in cassationRows ?? Array.Empty<CassasionResponse>())
                {
                    var o = GetOffice(r.RegionalOfficeKeyCode, r.RegionalOfficeName);
                    if (string.IsNullOrEmpty(o.Courts) && r.CourtName != "Məhkəmə təyin edilməyib") o.Courts = r.CourtName;
                    Save(o, Cassation.Kind, "total", r.Total);
                    Save(o, Cassation.Kind, "approved", r.Approved);
                    Save(o, Cassation.Kind, "rejected", r.Rejected);
                    Save(o, Cassation.Kind, "partiallyApproved", r.PartiallyApproved);
                }
            if (selected.Contains(Meetings))
                foreach (var r in meetingRows ?? Array.Empty<MeetingResponse>())
                {
                    var o = GetOffice(r.RegionalOfficeKeyCode, r.RegionalOfficeName);
                    if (string.IsNullOrEmpty(o.Courts) && r.CourtName != "Məhkəmə təyin edilməyib") o.Courts = r.CourtName;
                    Save(o, Meetings.Kind, "total", r.Total);
                }

            var ordered = offices.Values
                .OrderBy(o => RegionalOfficeNumber(o.Code))
                .ThenBy(o => o.Code, StringComparer.OrdinalIgnoreCase)
                .ThenBy(o => o.Name, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            string title = kind == "all" ? "Məhkəmə hesabatlarının ümumi statistikası" : selected[0].Caption;
            var period = Period(from, to);
            var label = (courtTypeLabel ?? "").Trim();
            if (label.Length > 120) label = label.Substring(0, 120);
            var subtitle = "Tarix aralığı: " + period + "    |    Məhkəmə növü: " +
                (courtTypeId.HasValue && courtTypeId.Value > 0 ? (label.Length == 0 ? courtTypeId.Value.ToString(CultureInfo.InvariantCulture) : label) : "Hamısı");
            return Package(Worksheet(title + " — " + period, subtitle, selected, ordered));
        }

        private static int RegionalOfficeNumber(string code)
        {
            if (string.IsNullOrEmpty(code)) return int.MaxValue;
            var digits = new string(code.Where(char.IsDigit).ToArray());
            return int.TryParse(digits, out var n) ? n : int.MaxValue;
        }
        private static string Period(DateTime? from, DateTime? to)
        {
            if (from.HasValue && to.HasValue) return from.Value.ToString("dd.MM.yyyy") + " – " + to.Value.ToString("dd.MM.yyyy");
            if (from.HasValue) return from.Value.ToString("dd.MM.yyyy") + " tarixindən";
            if (to.HasValue) return to.Value.ToString("dd.MM.yyyy") + " tarixinədək";
            return "Bütün dövr";
        }
        private static string Col(int number)
        {
            var s = "";
            for (var n = number; n > 0; n = (n - 1) / 26)
                s = ((char)('A' + (n - 1) % 26)) + s;
            return s;
        }
        private static string Esc(string? input)
        {
            var sb = new StringBuilder();
            foreach (char ch in input ?? "")
            {
                if (ch == '&') sb.Append("&amp;");
                else if (ch == '<') sb.Append("&lt;");
                else if (ch == '>') sb.Append("&gt;");
                else if (ch == '"') sb.Append("&quot;");
                else if (ch == '\'') sb.Append("&apos;");
                else if (ch == '\r' || ch == '\n' || ch == '\t' || ch >= 32) sb.Append(ch);
            }
            return sb.ToString();
        }
        private static string Cell(int row, int col, string? text, int style)
        {
            var reference = Col(col) + row.ToString(CultureInfo.InvariantCulture);
            if (string.IsNullOrEmpty(text)) return $"<c r=\"{reference}\" s=\"{style}\"/>";
            return $"<c r=\"{reference}\" s=\"{style}\" t=\"inlineStr\"><is><t xml:space=\"preserve\">{Esc(text)}</t></is></c>";
        }
        private static string NumberCell(int row, int col, long value, int style, bool blankWhenZero)
        {
            var reference = Col(col) + row.ToString(CultureInfo.InvariantCulture);
            if (blankWhenZero && value == 0) return $"<c r=\"{reference}\" s=\"{style}\"/>";
            return $"<c r=\"{reference}\" s=\"{style}\"><v>{value.ToString(CultureInfo.InvariantCulture)}</v></c>";
        }

        private static string Worksheet(
            string title,
            string subtitle,
            Section[] sections,
            Office[] offices)
        {
            var metrics = sections
                .SelectMany(s => s.Metrics.Select(m =>
                    (section: s.Kind, metric: m)))
                .ToArray();

            var count = metrics.Length + 2;
            var end = Col(count);

            const int firstData = 7;

            var rows = new List<string>();

            var merges = new List<string>
    {
        $"A1:{end}1",
        $"A2:{end}2",
        "A4:A6",
        "B4:B6"
    };

            // MAIN TITLE
            var r1 = new StringBuilder(Cell(1, 1, title, 1));
            var r2 = new StringBuilder(Cell(2, 1, subtitle, 2));

            for (int c = 2; c <= count; c++)
            {
                r1.Append(Cell(1, c, null, 1));
                r2.Append(Cell(2, c, null, 2));
            }

            rows.Add(
                $"<row r=\"1\" ht=\"32\" customHeight=\"1\">{r1}</row>"
            );

            rows.Add(
                $"<row r=\"2\" ht=\"26\" customHeight=\"1\">{r2}</row>"
            );

            rows.Add("<row r=\"3\" ht=\"10\" customHeight=\"1\"/>");

            // TABLE HEADERS
            var headers = new Dictionary<(int row, int col), string>();

            for (int row = 4; row <= 6; row++)
            {
                for (int col = 1; col <= count; col++)
                {
                    headers[(row, col)] = "";
                }
            }

            headers[(4, 1)] = "Ərazi idarəsinin adı";
            headers[(4, 2)] = "Məhkəmə orqanının adı";

            var cursor = 3;

            foreach (var section in sections)
            {
                var last = cursor + section.Metrics.Length - 1;

                headers[(4, cursor)] = section.Caption;

                if (last > cursor)
                {
                    merges.Add($"{Col(cursor)}4:{Col(last)}4");
                }
                else
                {
                    merges.Add($"{Col(cursor)}4:{Col(cursor)}6");
                }

                if (section == Appeal || section == Cassation)
                {
                    headers[(5, cursor)] = "Cəmi";

                    merges.Add(
                        $"{Col(cursor)}5:{Col(cursor)}6"
                    );

                    if (last > cursor + 1)
                    {
                        headers[(5, cursor + 1)] =
                            "Nəticələr üzrə (ümumi)";

                        merges.Add(
                            $"{Col(cursor + 1)}5:{Col(last)}5"
                        );
                    }

                    for (int i = 1; i < section.Metrics.Length; i++)
                    {
                        headers[(6, cursor + i)] =
                            section.Metrics[i].Caption;
                    }
                }
                else if (section.Metrics.Length > 1)
                {
                    for (int i = 0; i < section.Metrics.Length; i++)
                    {
                        headers[(5, cursor + i)] =
                            section.Metrics[i].Caption;

                        merges.Add(
                            $"{Col(cursor + i)}5:{Col(cursor + i)}6"
                        );
                    }
                }

                cursor = last + 1;
            }

            // HEADER ROW HEIGHTS
            for (int row = 4; row <= 6; row++)
            {
                var sb = new StringBuilder();

                for (int col = 1; col <= count; col++)
                {
                    sb.Append(
                        Cell(row, col, headers[(row, col)], 3)
                    );
                }

                // Keep sufficient height for long headings
                var ht = row == 5 ? 88 : row == 6 ? 65 : 28;

                rows.Add(
                    $"<row r=\"{row}\" ht=\"{ht}\" customHeight=\"1\">{sb}</row>"
                );
            }

            // DATA ROWS
            foreach (var pair in offices.Select(
                (office, idx) => (office, row: idx + firstData)))
            {
                var sb = new StringBuilder();

                sb.Append(
                    Cell(pair.row, 1, pair.office.Name, 4)
                );

                sb.Append(
                    Cell(pair.row, 2, pair.office.Courts, 4)
                );

                for (int i = 0; i < metrics.Length; i++)
                {
                    var m = metrics[i];

                    pair.office.Values.TryGetValue(
                        m.section + "." + m.metric.Key,
                        out var number
                    );

                    var isTotal =
                        m.metric.Key == "total" ||
                        m.metric.Key == "totalCourts" ||
                        m.metric.Key == "totalByResults";

                    sb.Append(
                        NumberCell(
                            pair.row,
                            i + 3,
                            number,
                            4,
                            !isTotal
                        )
                    );
                }

                // Compact numeric rows, but preserve long court names
                int courtLineCount = Math.Max(
                    1,
                    (int)Math.Ceiling(
                        pair.office.Courts.Length / 46.0
                    )
                );

                int dataHeight = Math.Max(
                    24,
                    Math.Min(90, 10 + courtLineCount * 14)
                );

                rows.Add(
                    $"<row r=\"{pair.row}\" ht=\"{dataHeight}\" customHeight=\"1\">{sb}</row>"
                );
            }

            // TOTAL ROW
            var totalRow = firstData + offices.Length;

            {
                var sb = new StringBuilder();

                sb.Append(Cell(totalRow, 1, "Cəmi", 5));
                sb.Append(Cell(totalRow, 2, null, 5));

                for (int i = 0; i < metrics.Length; i++)
                {
                    var m = metrics[i];
                    long sum = 0;

                    foreach (var office in offices)
                    {
                        if (office.Values.TryGetValue(
                            m.section + "." + m.metric.Key,
                            out var number))
                        {
                            sum += number;
                        }
                    }

                    sb.Append(
                        NumberCell(
                            totalRow,
                            i + 3,
                            sum,
                            5,
                            false
                        )
                    );
                }

                rows.Add(
                    $"<row r=\"{totalRow}\" ht=\"26\" customHeight=\"1\">{sb}</row>"
                );
            }

            // COLUMN WIDTHS
            var cols = new StringBuilder();

            // A - Regional Office
            cols.Append(
                "<col min=\"1\" max=\"1\" width=\"29\" customWidth=\"1\"/>"
            );

            // B - Court Name (increased from 36 to 52)
            cols.Append(
                "<col min=\"2\" max=\"2\" width=\"52\" customWidth=\"1\"/>"
            );

            // C onward - numeric columns (fixed compact width)
            for (int i = 0; i < metrics.Length; i++)
            {
                cols.Append(
                    $"<col min=\"{i + 3}\" max=\"{i + 3}\" width=\"12\" customWidth=\"1\"/>"
                );
            }

            // BUILD XML
            const string ns =
                "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

            var xml = new StringBuilder(
                $"<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><worksheet xmlns=\"{ns}\">"
            );

            xml.Append(
                "<sheetPr><pageSetUpPr fitToPage=\"1\"/></sheetPr>"
            );

            xml.Append(
                "<dimension ref=\"A1:" + end + totalRow + "\"/>"
            );

            xml.Append(
                "<sheetViews><sheetView showGridLines=\"0\" workbookViewId=\"0\">" +
                "<pane ySplit=\"6\" topLeftCell=\"A7\" activePane=\"bottomLeft\" state=\"frozen\"/>" +
                "</sheetView></sheetViews>"
            );

            xml.Append(
                "<sheetFormatPr defaultRowHeight=\"20\"/>" +
                "<cols>" + cols + "</cols>" +
                "<sheetData>" + string.Concat(rows) + "</sheetData>"
            );

            xml.Append(
                "<mergeCells count=\"" + merges.Count + "\">" +
                string.Concat(
                    merges.Select(m =>
                        "<mergeCell ref=\"" + m + "\"/>"
                    )
                ) +
                "</mergeCells>"
            );

            xml.Append(
                "<printOptions horizontalCentered=\"1\"/>" +
                "<pageMargins left=\"0.2\" right=\"0.2\" top=\"0.4\" bottom=\"0.4\" header=\"0.2\" footer=\"0.2\"/>"
            );

            xml.Append(
                "<pageSetup orientation=\"landscape\" fitToWidth=\"1\" fitToHeight=\"0\"/>" +
                "</worksheet>"
            );

            return xml.ToString();
        }


        private const string Styles = """
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">

        <fonts count="4">
            <font>
                <sz val="10"/>
                <name val="Arial"/>
                <color rgb="FF000000"/>
            </font>
            <font>
                <b/>
                <sz val="14"/>
                <name val="Arial"/>
                <color rgb="FF000000"/>
            </font>
            <font>
                <sz val="10"/>
                <name val="Arial"/>
                <color rgb="FF000000"/>
            </font>
            <font>
                <b/>
                <sz val="9"/>
                <name val="Arial"/>
                <color rgb="FF000000"/>
            </font>
        </fonts>

        <fills count="3">
            <fill>
                <patternFill patternType="none"/>
            </fill>
            <fill>
                <patternFill patternType="gray125"/>
            </fill>
            <fill>
                <patternFill patternType="solid">
                    <fgColor rgb="FFFFFFFF"/>
                    <bgColor indexed="64"/>
                </patternFill>
            </fill>
        </fills>

        <borders count="2">
            <border>
                <left/>
                <right/>
                <top/>
                <bottom/>
                <diagonal/>
            </border>
            <border>
                <left style="thin">
                    <color rgb="FF000000"/>
                </left>
                <right style="thin">
                    <color rgb="FF000000"/>
                </right>
                <top style="thin">
                    <color rgb="FF000000"/>
                </top>
                <bottom style="thin">
                    <color rgb="FF000000"/>
                </bottom>
                <diagonal/>
            </border>
        </borders>

        <cellStyleXfs count="1">
            <xf numFmtId="0"
                fontId="0"
                fillId="2"
                borderId="0"
                applyFill="1"/>
        </cellStyleXfs>

        <cellXfs count="6">

            <!-- Style 0: Default -->
            <xf numFmtId="0"
                fontId="0"
                fillId="2"
                borderId="0"
                xfId="0"
                applyFill="1"/>

            <!-- Style 1: Main title -->
            <xf numFmtId="0"
                fontId="1"
                fillId="2"
                borderId="0"
                xfId="0"
                applyFill="1"
                applyAlignment="1">
                <alignment
                    vertical="center"
                    horizontal="center"/>
            </xf>

            <!-- Style 2: Subtitle and date interval -->
            <xf numFmtId="0"
                fontId="2"
                fillId="2"
                borderId="0"
                xfId="0"
                applyFill="1"
                applyAlignment="1">
                <alignment
                    vertical="center"
                    horizontal="left"
                    wrapText="1"/>
            </xf>

            <!-- Style 3: Table headers -->
            <xf numFmtId="0"
                fontId="3"
                fillId="2"
                borderId="1"
                xfId="0"
                applyFill="1"
                applyBorder="1"
                applyAlignment="1">
                <alignment
                    vertical="center"
                    horizontal="center"
                    wrapText="1"/>
            </xf>

            <!-- Style 4: Table data -->
            <xf numFmtId="3"
                fontId="0"
                fillId="2"
                borderId="1"
                xfId="0"
                applyFill="1"
                applyBorder="1"
                applyAlignment="1">
                <alignment
                    vertical="center"
                    horizontal="center"
                    wrapText="1"/>
            </xf>

            <!-- Style 5: Total row -->
            <xf numFmtId="3"
                fontId="3"
                fillId="2"
                borderId="1"
                xfId="0"
                applyFill="1"
                applyBorder="1"
                applyAlignment="1">
                <alignment
                    vertical="center"
                    horizontal="center"
                    wrapText="1"/>
            </xf>

        </cellXfs>

        <cellStyles count="1">
            <cellStyle
                name="Normal"
                xfId="0"
                builtinId="0"/>
        </cellStyles>

        </styleSheet>
        """;

        private static byte[] Package(string sheet)
        {
            using var stream = new MemoryStream();
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
            {
                void Add(string name, string content)
                {
                    var entry = archive.CreateEntry(name, CompressionLevel.Fastest);
                    using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false));
                    writer.Write(content);
                }
                Add("[Content_Types].xml", """
                <?xml version="1.0" encoding="UTF-8"?>
                <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types"><Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/><Default Extension="xml" ContentType="application/xml"/><Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/><Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/><Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/></Types>
                """);
                Add("_rels/.rels", """
                <?xml version="1.0" encoding="UTF-8"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/></Relationships>
                """);
                Add("xl/workbook.xml", """
                <?xml version="1.0" encoding="UTF-8"?>
                <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"><sheets><sheet name="Hesabat" sheetId="1" r:id="rId1"/></sheets></workbook>
                """);
                Add("xl/_rels/workbook.xml.rels", """
                <?xml version="1.0" encoding="UTF-8"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/><Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/></Relationships>
                """);
                Add("xl/styles.xml", Styles.TrimStart());
                Add("xl/worksheets/sheet1.xml", sheet);
            }
            return stream.ToArray();
        }
    }
}
