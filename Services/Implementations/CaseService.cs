using CmsApi.DTOs.CaseDtos;
using CmsApi.Enums;
using CmsApi.Repositories.Interfaces;
using CmsApi.Services.Interfaces;
using DocumentFormat.OpenXml.Office2010.PowerPoint;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Text.RegularExpressions;

namespace CmsApi.Services.Implementations
{
    public class CaseService : ICaseService
    {
        private readonly IDocumentService _documentService;
        private readonly ICaseRepository _caseRepository;

        public CaseService(IDocumentService documentService, ICaseRepository caseRepository)
        {
            _documentService = documentService;
            _caseRepository = caseRepository;
        }

        public async Task AnalizeCase(long caseId)
        {
            string casno = string.Empty;
            try
            {

                // var caseResultFromDb = await _caseRepository.GetCaseOrStarterCaseAsync(caseId);

                var cases = await _caseRepository.CaseForAnalizesAsync();

                foreach (var caseItem in cases)
                {
                    bool isDocNull = false;

                    casno = caseItem.CaseNo;
                    caseId = caseItem.Id;


                    if (caseItem.CompletedCaseHasRequiredDocuments != null)
                    {
                        if ((bool)!caseItem.CompletedCaseHasRequiredDocuments)
                        {
                            var analizeRes = new CaseAnalysisFindingDto
                            {
                                CaseId = caseItem.Id,
                                IsResolved = false,
                                Type = 2,
                                WarningMessageId = (int)CaseAnalysisWarningType.FinalCourtActNotUploaded
                            };

                            await _caseRepository.InsertCaseAnalysisFindingAsync(analizeRes);
                        }
                    }

                    if (!caseItem.CaseHasAnalysisDocuments)
                    {
                        var analizeRes = new CaseAnalysisFindingDto
                        {
                            CaseId = caseItem.Id,
                            IsResolved = false,
                            Type = 2,
                            WarningMessageId = (int)CaseAnalysisWarningType.NoSuitableDocumentsForAnalysisFound
                        };

                        await _caseRepository.InsertCaseAnalysisFindingAsync(analizeRes);

                        continue;
                    }

                    var documents = await _caseRepository.CaseDocumentsAsync(caseItem.Id);

                    if (documents == null)
                    {
                        continue;
                    }

                    var content = string.Empty;

                    long docId = 0;
                    var orderedDocuments = documents
                        .Where(x => x.OtherDocTypeId == 9 ||
                                    x.OtherDocTypeId == 8 ||
                                    x.OtherDocTypeId == 229)
                        .OrderBy(x => x.OtherDocTypeId switch
                        {
                            9 => 1,
                            8 => 2,
                            229 => 3,
                            _ => 4
                        });

                    foreach (var item in orderedDocuments)
                    {
                        var document = await _documentService.GetDocument(item.Attachment.Ids);

                        docId = item.Id;

                        if (document == null)
                        {
                            var analizeRes = new CaseAnalysisFindingDto
                            {
                                CaseId = caseItem.Id,
                                IsResolved = false,
                                Type = 2,
                                WarningMessageId = (int)CaseAnalysisWarningType.DocumentCouldNotBeRetrievedFromSource
                            };

                            await _caseRepository.InsertCaseAnalysisFindingAsync(analizeRes);

                            isDocNull = true;

                            continue;
                        }

                        long fileSizeInBytes = document.Length;
                        double fileSizeInMb = fileSizeInBytes / 1024.0 / 1024.0;

                        var sened = _documentService.ExtractText(document);

                        var requestMatch = Regex.Match(
                          sened,
                          @"X\s*A?\s*(?:H|Z|J)?\s*[İIıi]?\s*(?:Ş|S|Z)?\s*E\s*D?\s*[İIıi]?\s*R\s*[ƏəEe]?\s*M\s*:?\s*(.+)",
                          RegexOptions.IgnoreCase | RegexOptions.Singleline);

                        string requestText = requestMatch.Success
                            ? requestMatch.Groups[1].Value.Trim()
                            : string.Empty;



                        if (!string.IsNullOrWhiteSpace(requestText))
                        {
                            string[] endPhrases =
                            {
                            "qərar qəbul edəsiniz.",
                            "qərar qəbul edəsiniz",
                            "qərar çıxarasınız.",
                            "qərar çıxarasınız",
                            "üzərinə qoyulsun.",
                            "üzərinə qoyulsun",
                            "qərar qəbuledəsiniz",
                            "qərar qəbuledəsiniz.",
                            "qərar qəbul edilsin.",
                            "qərar qəbul edilsin",
                            "qərardad qəbul edilsin.",
                            "qərardad qəbul edilsin",
                            "qərar qəbul nedəsiniz.",
                            "qərar qəbul nedəsiniz",
                            "məhkəmədən xahiş edirəm.",
                            "məhkəmədən xahiş edirəm",
                            "vəzifəsi qoyulsun.",
                            "vəzifəsi qoyulsun",
                            "qərar verəsiniz",
                            "qərar verəsiniz.",
                            "qərar çıxarılsın",
                            "qərar çıxarılsın.",
                            "qətnamə qəbul edəsiniz",
                            "qətnamə qəbul edəsiniz.",
                            "qətnamə qəbul edilsin.",
                            "qətnamə qəbul edilsin",
                            "qətnamə çıxarasınız.",
                            "qətnamə çıxarasınız",
                            "qərar qəbul olunsun",
                            "qərar qəbul olunsun."
                        };


                            string[] endPhrases2 =
                            {
                                "Qoşma",
                                "Qoşma Sənədlər.",
                                "Qoşma Sənədlər",
                                "Ərizəyə aşağıdakı sənədləri əlavə edirəm",
                                "Ərizəyə aşağıdakı sənədləri əlavə edirəm.",
                                "Ərizəyə əlavə sənədlərim.",
                                "Ərizəyə əlavə sənədlərim",
                                "Qoşma:",
                                "İmza:",
                                "İmza",
                                "İmza.",
                                "Dövlət rüsumunun ödənilməsi haqqında məlumat.",
                                "Dövlət rüsumunun ödənilməsi haqqında məlumat"
                            };



                            requestText = Regex.Replace(requestText, @"\s+", " ").Trim();

                            foreach (var marker in endPhrases)
                            {
                                int index = requestText.IndexOf(marker, StringComparison.OrdinalIgnoreCase);

                                if (index >= 0)
                                {
                                    content = requestText.Substring(0, index + marker.Length).Trim();
                                    docId = item.Id;
                                    break;
                                }
                            }
                            if (string.IsNullOrEmpty(content))
                            {

                                foreach (var marker in endPhrases2)
                                {
                                    int index = requestText.IndexOf(marker, StringComparison.OrdinalIgnoreCase);

                                    if (index >= 0)
                                    {
                                        content = requestText.Substring(0, index).Trim();
                                        docId = item.Id;
                                        break;
                                    }
                                }
                            }
                            if (string.IsNullOrEmpty(content))
                            {
                                content = await Analize(sened);
                            }

                            var key1 = await _caseRepository.GetKeyCodeByTextAsync(content, (int)KeywordGroup.ƏraziMənsubiyyəti);
                            var key2 = await _caseRepository.GetKeyCodeByTextAsync(content, (int)KeywordGroup.Əmlaknövü);

                            if (key1 == null || key2 == null)
                            {

                                content = Get1000Word(sened);

                                key1 = await _caseRepository.GetKeyCodeByTextAsync(content, (int)KeywordGroup.ƏraziMənsubiyyəti);
                                key2 = await _caseRepository.GetKeyCodeByTextAsync(content, (int)KeywordGroup.Əmlaknövü);
                            }
                            if (key1 == null || key2 == null)
                            {

                                content = await Analize(sened);

                                key1 = await _caseRepository.GetKeyCodeByTextAsync(content, (int)KeywordGroup.ƏraziMənsubiyyəti);
                                key2 = await _caseRepository.GetKeyCodeByTextAsync(content, (int)KeywordGroup.Əmlaknövü);
                            }

                            if (key1 == null)
                            {
                                var analizeRes = new CaseAnalysisFindingDto
                                {
                                    CaseId = caseItem.Id,
                                    IsResolved = false,
                                    Type = 2,
                                    WarningMessageId = (int)CaseAnalysisWarningType.EnforcementAuthorityNotFound
                                };

                                await _caseRepository.InsertCaseAnalysisFindingAsync(analizeRes);
                            }
                            else
                            {
                                await _caseRepository.UpdateCaseWarnings(caseId, (int)CaseAnalysisWarningType.EnforcementAuthorityNotFound);
                            }

                            if (key2 == null)
                            {
                                var analizeRes = new CaseAnalysisFindingDto
                                {
                                    CaseId = caseItem.Id,
                                    IsResolved = false,
                                    Type = 2,
                                    WarningMessageId = (int)CaseAnalysisWarningType.CaseSubjectNotFound
                                };

                                await _caseRepository.InsertCaseAnalysisFindingAsync(analizeRes);
                            }
                            else
                            {
                                await _caseRepository.UpdateCaseWarnings(caseId, (int)CaseAnalysisWarningType.CaseSubjectNotFound);

                            }


                            if (key1 != null || key2 != null)
                            {
                                var res = new CaseAnalysisResultDto
                                {

                                    CaseId = caseItem.Id,
                                    OfficeKeyCode = key1,
                                    CaseSubjectKeyCode = key2,
                                };
                                await _caseRepository.InsertCaseAnalysisResultAsync(res);

                                await _caseRepository.UpdateCaseWarnings(caseId, (int)CaseAnalysisWarningType.StatementOfClaimNotUploaded);
                                await _caseRepository.UpdateCaseWarnings(caseId, (int)CaseAnalysisWarningType.NoAnalyzableTextFound);
                                await _caseRepository.UpdateCaseWarnings(caseId, (int)CaseAnalysisWarningType.NoSuitableDocumentsForAnalysisFound);
                                await _caseRepository.UpdateCaseWarnings(caseId, (int)CaseAnalysisWarningType.DocumentCouldNotBeRetrievedFromSource);

                            }

                        }
                        else
                        {
                            content = await Analize(sened);

                            var key1 = await _caseRepository.GetKeyCodeByTextAsync(content, (int)KeywordGroup.ƏraziMənsubiyyəti);
                            var key2 = await _caseRepository.GetKeyCodeByTextAsync(content, (int)KeywordGroup.Əmlaknövü);

                            if (key1 == null || key2 == null)
                            {

                                content = Get1000Word(sened);

                                key1 = await _caseRepository.GetKeyCodeByTextAsync(content, (int)KeywordGroup.ƏraziMənsubiyyəti);
                                key2 = await _caseRepository.GetKeyCodeByTextAsync(content, (int)KeywordGroup.Əmlaknövü);
                            }

                            if (key1 == null || key2 == null)
                            {

                                content = await Analize(sened);

                                key1 = await _caseRepository.GetKeyCodeByTextAsync(content, (int)KeywordGroup.ƏraziMənsubiyyəti);
                                key2 = await _caseRepository.GetKeyCodeByTextAsync(content, (int)KeywordGroup.Əmlaknövü);
                            }

                            if (key1 == null)
                            {
                                var analizeRes = new CaseAnalysisFindingDto
                                {
                                    CaseId = caseItem.Id,
                                    IsResolved = false,
                                    Type = 2,
                                    WarningMessageId = (int)CaseAnalysisWarningType.EnforcementAuthorityNotFound
                                };

                                await _caseRepository.InsertCaseAnalysisFindingAsync(analizeRes);
                            }
                            else
                            {
                                await _caseRepository.UpdateCaseWarnings(caseId, (int)CaseAnalysisWarningType.EnforcementAuthorityNotFound);
                            }

                            if (key2 == null)
                            {
                                var analizeRes = new CaseAnalysisFindingDto
                                {
                                    CaseId = caseItem.Id,
                                    IsResolved = false,
                                    Type = 2,
                                    WarningMessageId = (int)CaseAnalysisWarningType.CaseSubjectNotFound
                                };

                                await _caseRepository.InsertCaseAnalysisFindingAsync(analizeRes);
                            }
                            else
                            {
                                await _caseRepository.UpdateCaseWarnings(caseId, (int)CaseAnalysisWarningType.CaseSubjectNotFound);
                            }

                            if (key1 != null || key2 != null)
                            {
                                var res = new CaseAnalysisResultDto
                                {

                                    CaseId = caseItem.Id,
                                    OfficeKeyCode = key1,
                                    CaseSubjectKeyCode = key2,

                                };
                                await _caseRepository.InsertCaseAnalysisResultAsync(res);

                                await _caseRepository.UpdateCaseWarnings(caseId, (int)CaseAnalysisWarningType.StatementOfClaimNotUploaded);
                                await _caseRepository.UpdateCaseWarnings(caseId, (int)CaseAnalysisWarningType.NoAnalyzableTextFound);
                                await _caseRepository.UpdateCaseWarnings(caseId, (int)CaseAnalysisWarningType.NoSuitableDocumentsForAnalysisFound);
                                await _caseRepository.UpdateCaseWarnings(caseId, (int)CaseAnalysisWarningType.DocumentCouldNotBeRetrievedFromSource);
                            }
                        }

                        if (!string.IsNullOrEmpty(content))
                        {
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(content))
                    {
                        var docde = new InsertCaseDocumentRawContentRequestDto
                        {
                            CaseNo = caseItem.CaseNo,
                            CourtLevelId = caseItem.CourtLevelId,
                            Content = content,
                            DocumentId = docId,
                        };

                        await _caseRepository.InsertCaseDocumentRawContentAsync(docde);
                    }
                    if(string.IsNullOrEmpty(content) && isDocNull == false)
                    {
                        var analizeRes = new CaseAnalysisFindingDto
                        {
                            CaseId = caseItem.Id,
                            IsResolved = false,
                            Type = 2,
                            WarningMessageId = (int)CaseAnalysisWarningType.NoAnalyzableTextFound
                        };

                        await _caseRepository.InsertCaseAnalysisFindingAsync(analizeRes);
                    }

                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private string Get1000Word(string content)
        {
            const int maxWords = 1000;

            if (string.IsNullOrWhiteSpace(content))
                return string.Empty;

            // "xahiş edirəm" ifadəsinin müxtəlif variantlarını tapır:
            // xahiş edirəm
            // xahis edirem
            // xahiş  edirəm
            // xahişedirəm
            // və s.
            const string markerPattern =
                @"X\s*A\s*H\s*[Iİiı]\s*Ş\s*E\s*D\s*[Iİiı]\s*R(?:\s*Ə\s*M|\s*İ\s*K)\s*:?\s*(.+)";



            var matches = Regex.Matches(
                 content,
                 markerPattern,
                 RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            if (matches.Count == 0)
                return string.Empty;

            // Sənəddəki SON "xahiş edirəm" variantını götür
            Match lastMatch = matches[matches.Count - 1];

            // "xahiş edirəm" də daxil olsun
            int endPosition = lastMatch.Index + lastMatch.Length;

            string textUntilEnd = content.Substring(0, endPosition);

            // Sözləri tap
            var words = Regex.Matches(textUntilEnd, @"\S+");

            if (words.Count <= maxWords)
                return textUntilEnd.Trim();

            // Son 1000 sözün başlanğıcını tap
            int startIndex = words[words.Count - maxWords].Index;

            return textUntilEnd
                .Substring(startIndex)
                .Trim();
        }


        private async Task<string> Analize(string text)
        {
            var pattern =
                @"[İIiı]\s*[Dd]\s*[Dd]\s*[İIiı]\s*[Aa]\s*[ƏəEe]\s*[Rr]\s*[İIiı]\s*[Zz]\s*[ƏəEe]\s*[SsŞş]\s*[İIiı]";

            var match = Regex.Match(
                text,
                pattern,
                RegexOptions.IgnoreCase);

            if (!match.Success)
                return string.Empty;

            var startPosition = match.Index;

            var afterText = text[startPosition..];

            var words = Regex
                .Split(afterText, @"\s+")
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Take(301);

            return string.Join(" ", words);
        }

       


    }
}