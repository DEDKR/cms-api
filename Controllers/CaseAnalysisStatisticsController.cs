using CmsApi.DTOs.AnalysisDtos;
using CmsApi.DTOs.ApiDtos;
using CmsApi.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CmsApi.Controllers;

[ApiController]
[Route("api/Admin/analysis-statistics")]
[Authorize(Roles = "Admin")]
public sealed class CaseAnalysisStatisticsController : ControllerBase
{
    private readonly ICaseAnalysisStatisticsRepository _repository;

    public CaseAnalysisStatisticsController(ICaseAnalysisStatisticsRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<CaseAnalysisStatisticsDto>>> Get(CancellationToken cancellationToken)
    {
        var data = await _repository.GetStatisticsAsync(cancellationToken);
        return Ok(ApiResponse<CaseAnalysisStatisticsDto>.Ok(data));
    }
}
