using System.Data;
using CmsApi.DB;
using CmsApi.DTOs.AnalysisDtos;
using CmsApi.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace CmsApi.Repositories.Implementations;

public sealed class CaseAnalysisStatisticsRepository : ICaseAnalysisStatisticsRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CaseAnalysisStatisticsRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<CaseAnalysisStatisticsDto> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateMsSqlConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand("[dbo].[P_GET_CASE_ANALYSIS_STATISTICS]", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 60
        };

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var result = new CaseAnalysisStatisticsDto();

        // ResultSet 1: ümumi göstəricilər.
        if (await reader.ReadAsync(cancellationToken))
        {
            result.TotalCases = reader.GetInt32(reader.GetOrdinal("TOTAL_CASES"));
            result.TotalAnalyzed = reader.GetInt32(reader.GetOrdinal("TOTAL_ANALIZED"));
            result.TotalAnalyzeHasResult = reader.GetInt32(reader.GetOrdinal("TOTAL_ANALIZE_HAS_RESULT"));
            result.BothResult = reader.GetInt32(reader.GetOrdinal("BOTH_RESULT"));
            result.CaseSubjectIsNotFindings = reader.GetInt32(reader.GetOrdinal("CASE_SUBJECT_IS_NOT_FINDINGS"));
            result.CaseOfficeIsNotFindings = reader.GetInt32(reader.GetOrdinal("CASE_OFFICE_IS_NOT_FINDINGS"));
            result.TotalErrorCount = reader.GetInt32(reader.GetOrdinal("TOTAL_ERROR_COUNT"));
        }

        // ResultSet 2: xəta mesajları və sayları.
        if (await reader.NextResultAsync(cancellationToken))
        {
            var idOrdinal = reader.GetOrdinal("WarningMessageId");
            var messageOrdinal = reader.GetOrdinal("MESSAGE");
            var countOrdinal = reader.GetOrdinal("WarningCount");

            while (await reader.ReadAsync(cancellationToken))
            {
                result.Warnings.Add(new CaseAnalysisWarningStatisticDto
                {
                    WarningMessageId = reader.GetInt32(idOrdinal),
                    Message = reader.IsDBNull(messageOrdinal) ? string.Empty : reader.GetString(messageOrdinal),
                    WarningCount = reader.GetInt32(countOrdinal)
                });
            }
        }

        return result;
    }
}
