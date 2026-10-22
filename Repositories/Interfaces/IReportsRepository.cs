using CmsApi.DTOs.Reports.Appeals;
using CmsApi.DTOs.Reports.Cassasion;
using CmsApi.DTOs.Reports.FirstInstance;
using CmsApi.DTOs.Reports.Meeting;

namespace CmsApi.Repositories.Interfaces
{
    public interface IReportsRepository
    {
        Task<List<FirstInstanceResponse>> GetFirstInstanceReportAsync(FirstInstanceRequest request);
        Task<List<AppealResponse>> GetAppealReportAsync(AppealRequest request);
        Task<List<CassasionResponse>> GetCassasionReportAsync(CassasionRequest request);
        Task<List<MeetingResponse>> GetMeetingReportAsync(MeetingRequest request);
    }
}
