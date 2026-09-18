using CmsApi.DTOs.CaseDtos;
using CmsApi.Services.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CmsApi.Services.Implementations
{
    public class CasePdfGenerator : ICasePdfGenerator
    {
        public byte[] Generate(CaseDetailDto caseDetail)
        {
            ArgumentNullException.ThrowIfNull(caseDetail);

            return Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);

                    page.DefaultTextStyle(x =>
                        x.FontSize(9)
                         .FontFamily("Lato"));

                    page.Header()
                        .Element(container => ComposeHeader(container, caseDetail));

                    page.Content()
                        .PaddingVertical(15)
                        .Column(column =>
                        {
                            column.Spacing(15);

                            ComposeCaseInfo(column, caseDetail.CaseView);

                            ComposeWarnings(column, caseDetail.Warnings);

                            ComposeJudges(column, caseDetail.Judges);

                            ComposeParties(column, caseDetail.Parties);

                            ComposeMeetings(column, caseDetail.RelatedMeetingDtos);

                            ComposeDocuments(column, caseDetail.Documents);

                            ComposeAppeals(column, caseDetail.Appeals);

                            ComposeHistory(column, caseDetail.CaseHistories);

                            ComposeRelatedCases(column, caseDetail.RelatedCases);

                            ComposeNotifications(column, caseDetail.Notifications);

                            ComposeCaseCodes(column, caseDetail.CaseCodes);
                        });

                    page.Footer()
                        .Element(ComposeFooter);
                });
            })
            .GeneratePdf();
        }

        // =========================================================
        // HEADER
        // =========================================================

        private static void ComposeHeader(
            IContainer container,
            CaseDetailDto dto)
        {
            container
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten1)
                .PaddingBottom(10)
                .Row(row =>
                {
                    row.RelativeItem().Column(column =>
                    {
                        column.Item()
                            .Text("MƏHKƏMƏ İŞİ")
                            .Bold()
                            .FontSize(16);

                        column.Item()
                            .PaddingTop(3)
                            .Text($"İş №: {Value(dto.CaseView?.CaseNo)}")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken1);
                    });

                    row.ConstantItem(120)
                        .AlignRight()
                        .AlignMiddle()
                        .Text(DateTime.Now.ToString("dd.MM.yyyy"))
                        .FontSize(9)
                        .FontColor(Colors.Grey.Darken1);
                });
        }

        // =========================================================
        // ОСНОВНАЯ ИНФОРМАЦИЯ
        // =========================================================

        private static void ComposeCaseInfo(
            ColumnDescriptor column,
            CaseDetailViewDto? item)
        {
            SectionTitle(column, "ÜMUMİ MƏLUMAT");

            if (item is null)
            {
                EmptyMessage(column);
                return;
            }

            column.Item()
                .Border(0.5f)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(10)
                .Column(info =>
                {
                    info.Spacing(7);

                    Field(info, "İş nömrəsi", item.CaseNo);
                    Field(info, "İşin növü", item.CaseType);
                    Field(info, "İcra növü", item.ExecType);
                    Field(info, "Status", item.CaseStatus);

                    Field(info, "Məhkəmə", item.Court);
                    Field(info, "Məhkəmə instansiyası", item.CourtLevelName);
                    Field(info, "Hakim", item.Judge);

                    Field(info, "Daxil olma tarixi", Date(item.EnterDate));
                    Field(info, "İl", item.Year?.ToString());

                    Field(info, "Ərazi idarəsi", item.TerritorialOffice);
                    Field(info, "İşin predmeti", item.CaseSubject);
                    Field(info, "Nəticə", item.Result);
                });
        }

        // =========================================================
        // WARNINGS
        // =========================================================

        private static void ComposeWarnings(
            ColumnDescriptor column,
            List<CaseWarnings>? warnings)
        {
            if (warnings is not { Count: > 0 })
                return;

            SectionTitle(column, $"XƏBƏRDARLIQLAR ({warnings.Count})");

            foreach (var warning in warnings)
            {
                column.Item()
                    .ShowEntire()
                    .Border(0.5f)
                    .BorderColor(Colors.Grey.Lighten1)
                    .Padding(8)
                    .Column(x =>
                    {
                        x.Spacing(4);

                        x.Item().Text(text =>
                        {
                            text.Span("Növ: ").SemiBold();
                            text.Span(Value(warning.Type));
                        });

                        x.Item()
                            .Text(Value(warning.Message));

                        x.Item()
                            .Text(
                                warning.IsResolved
                                    ? $"Həll olunub: {Date(warning.ResolvedDate)}"
                                    : "Həll olunmayıb")
                            .FontSize(8)
                            .FontColor(Colors.Grey.Darken1);

                        x.Item()
                            .Text($"Yaradılma tarixi: {Date(warning.CreatedDate)}")
                            .FontSize(8)
                            .FontColor(Colors.Grey.Darken1);
                    });
            }
        }

        // =========================================================
        // JUDGES
        // =========================================================

        private static void ComposeJudges(
            ColumnDescriptor column,
            List<CaseJudgeDto>? judges)
        {
            if (judges is not { Count: > 0 })
                return;

            SectionTitle(column, $"HAKİMLƏR ({judges.Count})");

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(35);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    HeaderCell(header.Cell(), "№");
                    HeaderCell(header.Cell(), "Hakim");
                    HeaderCell(header.Cell(), "Növ");
                });

                for (var i = 0; i < judges.Count; i++)
                {
                    var judge = judges[i];

                    BodyCell(table.Cell(), (i + 1).ToString());
                    BodyCell(table.Cell(), judge.Name);
                    BodyCell(table.Cell(), judge.Type);
                }
            });
        }

        // =========================================================
        // PARTIES
        // =========================================================

        private static void ComposeParties(
            ColumnDescriptor column,
            List<CasePartyDto>? parties)
        {
            if (parties is not { Count: > 0 })
                return;

            SectionTitle(column, $"İŞ ÜZRƏ TƏRƏFLƏR ({parties.Count})");

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(35);
                    columns.RelativeColumn();
                    columns.RelativeColumn(2);
                });

                table.Header(header =>
                {
                    HeaderCell(header.Cell(), "№");
                    HeaderCell(header.Cell(), "Tərəfin növü");
                    HeaderCell(header.Cell(), "Ad / təşkilat");
                });

                for (var i = 0; i < parties.Count; i++)
                {
                    var party = parties[i];

                    BodyCell(table.Cell(), (i + 1).ToString());
                    BodyCell(table.Cell(), party.Type);
                    BodyCell(table.Cell(), party.Name);
                }
            });
        }

        // =========================================================
        // MEETINGS
        // =========================================================

        private static void ComposeMeetings(
            ColumnDescriptor column,
            List<CaseRelatedMeetingDto>? meetings)
        {
            if (meetings is not { Count: > 0 })
                return;

            SectionTitle(column, $"MƏHKƏMƏ İCLASLARI ({meetings.Count})");

            foreach (var meeting in meetings)
            {
                column.Item()
                    .ShowEntire()
                    .Border(0.5f)
                    .BorderColor(Colors.Grey.Lighten2)
                    .Padding(8)
                    .Column(x =>
                    {
                        x.Spacing(5);

                        Field(x, "Tarix", DateTimeValue(meeting.MeetingDate));
                        Field(x, "Məhkəmə", meeting.Court);
                        Field(x, "Hakim", meeting.Judge);
                        Field(x, "İclasın növü", meeting.MeetingType);
                        Field(x, "Status", meeting.Status);
                    });
            }
        }

        // =========================================================
        // DOCUMENTS
        // =========================================================

        private static void ComposeDocuments(
            ColumnDescriptor column,
            List<CaseDocuments>? documents)
        {
            if (documents is not { Count: > 0 })
                return;

            SectionTitle(column, $"SƏNƏDLƏR ({documents.Count})");

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(30);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.ConstantColumn(70);
                });

                table.Header(header =>
                {
                    HeaderCell(header.Cell(), "№");
                    HeaderCell(header.Cell(), "Sənəd");
                    HeaderCell(header.Cell(), "Status");
                    HeaderCell(header.Cell(), "Fayl");
                    HeaderCell(header.Cell(), "Tarix");
                });

                for (var i = 0; i < documents.Count; i++)
                {
                    var document = documents[i];

                    BodyCell(table.Cell(), (i + 1).ToString());
                    BodyCell(table.Cell(), document.DocTypeName);
                    BodyCell(table.Cell(), document.Status);
                    BodyCell(table.Cell(), document.Attachment?.FileName);
                    BodyCell(table.Cell(), Date(document.InsertDate));
                }
            });
        }

        // =========================================================
        // APPEALS
        // =========================================================

        private static void ComposeAppeals(
            ColumnDescriptor column,
            List<CaseAppealDto>? appeals)
        {
            if (appeals is not { Count: > 0 })
                return;

            SectionTitle(column, $"ŞİKAYƏTLƏR / APELLYASİYA ({appeals.Count})");

            foreach (var appeal in appeals)
            {
                column.Item()
                    .Border(0.5f)
                    .BorderColor(Colors.Grey.Lighten2)
                    .Padding(9)
                    .Column(x =>
                    {
                        x.Spacing(5);

                        Field(x, "Status", appeal.Status);

                        Field(
                            x,
                            "Sənədin növü",
                            appeal.OtherDocumentTypeName);

                        Field(
                            x,
                            "Sənəd nömrəsi",
                            appeal.OtherDocumentNumber);

                        Field(
                            x,
                            "Sənədin daxil olma tarixi",
                            Date(appeal.OtherDocumentEnterDate));

                        Field(
                            x,
                            "Qərarın növü",
                            appeal.DecisionTypeName);

                        Field(
                            x,
                            "Qərar sənədinin nömrəsi",
                            appeal.DecisitonDocumentNumber);

                        Field(
                            x,
                            "Qərarın daxil olma tarixi",
                            Date(appeal.DecisionEnterDate));

                        Field(
                            x,
                            "Göndərən orqan",
                            appeal.SendedOrgan);

                        if (appeal.AppealParties is { Count: > 0 })
                        {
                            Field(
                                x,
                                "Şikayət edən tərəflər",
                                string.Join(", ",
                                    appeal.AppealParties
                                        .Where(p => !string.IsNullOrWhiteSpace(p))));
                        }
                    });
            }
        }

        // =========================================================
        // HISTORY
        // =========================================================

        private static void ComposeHistory(
            ColumnDescriptor column,
            List<CaseHistoryDto>? history)
        {
            if (history is not { Count: > 0 })
                return;

            SectionTitle(column, $"İŞİN TARİXÇƏSİ ({history.Count})");

            foreach (var item in history)
            {
                column.Item()
                    .BorderLeft(2)
                    .BorderColor(Colors.Grey.Lighten1)
                    .PaddingLeft(10)
                    .PaddingVertical(5)
                    .Column(x =>
                    {
                        x.Spacing(4);

                        x.Item()
                            .Text(Value(item.CaseNo))
                            .SemiBold()
                            .FontSize(10);

                        Field(x, "Məhkəmə", item.Court);
                        Field(x, "Hakim", item.Judge);
                        Field(x, "Status", item.Status);
                        Field(x, "Nəticə", item.Result);

                        Field(x, "Daxil olma", Date(item.EnterDate));
                        Field(x, "Qərar tarixi", Date(item.DecisionDate));
                        Field(x, "Nəticə tarixi", Date(item.ResultDate));
                    });
            }
        }

        // =========================================================
        // RELATED CASES
        // =========================================================

        private static void ComposeRelatedCases(
            ColumnDescriptor column,
            List<CaseListDto>? cases)
        {
            if (cases is not { Count: > 0 })
                return;

            SectionTitle(column, $"ƏLAQƏLİ İŞLƏR ({cases.Count})");

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(30);
                    columns.RelativeColumn();
                    columns.RelativeColumn(2);
                    columns.RelativeColumn();
                    columns.ConstantColumn(70);
                });

                table.Header(header =>
                {
                    HeaderCell(header.Cell(), "№");
                    HeaderCell(header.Cell(), "İş №");
                    HeaderCell(header.Cell(), "Məhkəmə");
                    HeaderCell(header.Cell(), "Status");
                    HeaderCell(header.Cell(), "Tarix");
                });

                for (var i = 0; i < cases.Count; i++)
                {
                    var item = cases[i];

                    BodyCell(table.Cell(), (i + 1).ToString());
                    BodyCell(table.Cell(), item.CaseNo);
                    BodyCell(table.Cell(), item.CourtName);
                    BodyCell(table.Cell(), item.CaseStatus);
                    BodyCell(table.Cell(), Date(item.EnterDate));
                }
            });
        }

        // =========================================================
        // NOTIFICATIONS
        // =========================================================

        private static void ComposeNotifications(
            ColumnDescriptor column,
            List<CaseNotificationDto>? notifications)
        {
            if (notifications is not { Count: > 0 })
                return;

            SectionTitle(
                column,
                $"BİLDİRİŞLƏR ({notifications.Count})");

            foreach (var notification in notifications)
            {
                column.Item()
                    .BorderBottom(0.5f)
                    .BorderColor(Colors.Grey.Lighten2)
                    .PaddingVertical(7)
                    .Column(x =>
                    {
                        x.Spacing(4);

                        x.Item().Row(row =>
                        {
                            row.RelativeItem()
                                .Text(Value(notification.StatusName))
                                .SemiBold();

                            row.ConstantItem(100)
                                .AlignRight()
                                .Text(DateTimeValue(notification.InsertDate))
                                .FontSize(8)
                                .FontColor(Colors.Grey.Darken1);
                        });

                        if (!string.IsNullOrWhiteSpace(notification.Court))
                        {
                            x.Item()
                                .Text(notification.Court)
                                .FontSize(8)
                                .FontColor(Colors.Grey.Darken1);
                        }

                        if (!string.IsNullOrWhiteSpace(notification.Content))
                        {
                            x.Item()
                                .Text(notification.Content);
                        }

                        if (!string.IsNullOrWhiteSpace(notification.Result))
                        {
                            x.Item().Text(text =>
                            {
                                text.Span("Nəticə: ").SemiBold();
                                text.Span(notification.Result);
                            });
                        }
                    });
            }
        }

        // =========================================================
        // CASE CODES
        // =========================================================

        private static void ComposeCaseCodes(
            ColumnDescriptor column,
            List<CaseCode>? codes)
        {
            if (codes is not { Count: > 0 })
                return;

            SectionTitle(column, "HÜQUQİ NORMALAR");

            column.Item()
                .Border(0.5f)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(8)
                .Column(x =>
                {
                    x.Spacing(5);

                    foreach (var code in codes)
                    {
                        x.Item().Text(text =>
                        {
                            if (!string.IsNullOrWhiteSpace(code.Chapter))
                            {
                                text.Span(code.Chapter)
                                    .SemiBold();
                            }

                            if (!string.IsNullOrWhiteSpace(code.ArticleNo))
                            {
                                if (!string.IsNullOrWhiteSpace(code.Chapter))
                                    text.Span(" — ");

                                text.Span($"Maddə {code.ArticleNo}");
                            }

                            if (string.IsNullOrWhiteSpace(code.Chapter) &&
                                string.IsNullOrWhiteSpace(code.ArticleNo))
                            {
                                text.Span("-");
                            }
                        });
                    }
                });
        }

        // =========================================================
        // COMMON DESIGN
        // =========================================================

        private static void SectionTitle(
            ColumnDescriptor column,
            string title)
        {
            column.Item()
                .Background(Colors.Grey.Lighten3)
                .BorderLeft(3)
                .BorderColor(Colors.Grey.Darken1)
                .PaddingVertical(6)
                .PaddingHorizontal(9)
                .Text(title)
                .Bold()
                .FontSize(11);
        }

        private static void Field(
            ColumnDescriptor column,
            string label,
            string? value)
        {
            // Не показываем пустые поля вообще.
            if (string.IsNullOrWhiteSpace(value))
                return;

            column.Item().Row(row =>
            {
                row.ConstantItem(135)
                    .Text(label)
                    .SemiBold()
                    .FontColor(Colors.Grey.Darken2);

                row.RelativeItem()
                    .Text(value);
            });
        }

        private static void HeaderCell(
            IContainer container,
            string text)
        {
            container
                .Background(Colors.Grey.Lighten3)
                .Border(0.5f)
                .BorderColor(Colors.Grey.Lighten1)
                .Padding(5)
                .Text(text)
                .SemiBold()
                .FontSize(8);
        }

        private static void BodyCell(
            IContainer container,
            string? text)
        {
            container
                .BorderBottom(0.5f)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(5)
                .Text(Value(text))
                .FontSize(8);
        }

        private static void EmptyMessage(ColumnDescriptor column)
        {
            column.Item()
                .Padding(10)
                .AlignCenter()
                .Text("Məlumat yoxdur")
                .Italic()
                .FontColor(Colors.Grey.Medium);
        }

        // =========================================================
        // NULL / FORMAT HELPERS
        // =========================================================

        private static string Value(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? "-"
                : value.Trim();
        }

        private static string? Date(DateTime? date)
        {
            return date?.ToString("dd.MM.yyyy");
        }

        private static string Date(DateTime date)
        {
            return date.ToString("dd.MM.yyyy");
        }

        private static string? DateTimeValue(DateTime? date)
        {
            return date?.ToString("dd.MM.yyyy HH:mm");
        }

        private static string DateTimeValue(DateTime date)
        {
            return date.ToString("dd.MM.yyyy HH:mm");
        }

        // =========================================================
        // FOOTER
        // =========================================================

        private static void ComposeFooter(IContainer container)
        {
            container
                .BorderTop(0.5f)
                .BorderColor(Colors.Grey.Lighten2)
                .PaddingTop(7)
                .Row(row =>
                {
                    row.RelativeItem()
                        .Text($"Yaradılma tarixi: {DateTime.Now:dd.MM.yyyy HH:mm}")
                        .FontSize(7)
                        .FontColor(Colors.Grey.Medium);

                    row.RelativeItem()
                        .AlignRight()
                        .Text(text =>
                        {
                            text.DefaultTextStyle(x =>
                                x.FontSize(8)
                                 .FontColor(Colors.Grey.Darken1));

                            text.Span("Səhifə ");
                            text.CurrentPageNumber();
                            text.Span(" / ");
                            text.TotalPages();
                        });
                });
        }
    }
}