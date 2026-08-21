namespace CmsApi.DTOs.Dashboard
{
    public class CaseTotalByYearStatisticDto
    {
        public int Year { get; set; }
        public int Month { get; set; }

        public int FirstInstance { get; set; }
        public int Appeal { get; set; }
        public int Cassation { get; set; }

        public int TotalMonths { get; set; }

        public int TotalFirstInstance { get; set; }
        public int TotalAppeal { get; set; }
        public int TotalCassation { get; set; }
    }
}
