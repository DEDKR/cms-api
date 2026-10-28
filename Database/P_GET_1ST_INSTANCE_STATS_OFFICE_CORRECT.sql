USE [CMS_DB];
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

-- Office is determined ONLY by TBL_CASE_ANALYSIS_RESULT.OFFICE_KEY_CODE.
-- Court and result come from the same mapped TBL_CASES row.
-- @BY_COURT=0: one row per regional office.
-- @BY_COURT=1: one row per real office+court combination.
-- First 10 output fields preserve the existing frontend contract.
-- Last 2 fields are stable identifiers for parent/child UI mapping.
CREATE OR ALTER PROCEDURE dbo.P_GET_1ST_INSTANCE_STATS
    @FROM DATE = NULL,
    @TO DATE = NULL,
    @OFFICE_KEY_CODE NVARCHAR(50) = NULL,
    @BY_COURT BIT = 0,
    @COURT_TYPE_ID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH OfficeCaseMap AS
    (
        SELECT DISTINCT
            A.OFFICE_KEY_CODE,
            A.CASE_ID
        FROM dbo.TBL_CASE_ANALYSIS_RESULT A
        WHERE A.OFFICE_KEY_CODE IS NOT NULL
          AND (@OFFICE_KEY_CODE IS NULL OR A.OFFICE_KEY_CODE = @OFFICE_KEY_CODE)
    ),
    OfficeCases AS
    (
        SELECT DISTINCT
            M.OFFICE_KEY_CODE,
            C.ID AS CASE_ID,
            DC.COURT_ID,
            DC.COURT_NAME,
            C.RESULT
        FROM OfficeCaseMap M
        INNER JOIN dbo.TBL_CASES C
            ON C.ID = M.CASE_ID
        INNER JOIN dbo.DIC_COURTS DC
            ON DC.COURT_ID = C.COURT_ID
        LEFT JOIN dbo.DIC_COURT_TYPES CT
            ON CT.COURT_TYPE_ID = DC.COURT_TYPE_ID
        WHERE DC.COURT_TYPE_ID NOT IN (56, 91, 63, 64)
          AND (@COURT_TYPE_ID IS NULL OR DC.COURT_TYPE_ID = @COURT_TYPE_ID)
          AND (CT.COURT_LEVEL_ID = 1 OR CT.COURT_LEVEL_ID IS NULL)
          AND (@FROM IS NULL OR C.CREATED_AT >= @FROM)
          AND (@TO IS NULL OR C.CREATED_AT < DATEADD(DAY, 1, @TO))
    ),
    CourtStats AS
    (
        SELECT
            OC.OFFICE_KEY_CODE,
            OC.COURT_ID,
            OC.COURT_NAME,
            COUNT(*) AS TOTAL_COURTS,
            SUM(CASE WHEN OC.RESULT IN
                (N'təmin edildi', N'İddia - təmin edildi')
                THEN 1 ELSE 0 END) AS APPROVED_CLAIMS,
            SUM(CASE WHEN OC.RESULT IN
                (N'rədd edildi', N'İddia - rədd edildi')
                THEN 1 ELSE 0 END) AS REJECTED_CLAIMS,
            SUM(CASE WHEN OC.RESULT = N'qismən təmin edildi'
                THEN 1 ELSE 0 END) AS PARTIAL_CLAIMS,
            SUM(CASE WHEN OC.RESULT IN
                (
                    N'İcraata xitam verildi - cavabdeh tərəfindən iddiaçının tələbi təmin olunduqda və işin mahiyyəti üzrə mübahisə artıq mövcud olmadıqda',
                    N'İcraata xitam verildi - iddiaçı iddiadan imtina etdikdə',
                    N'İcraata xitam verildi - iddiaçı məhkəmə icraatından ardıcıl olaraq yayındığı halda iddia geri götürülmüş sayıldıqda',
                    N'İcraata xitam verildi - tərəflərdən biri olan şəxs öldükdən sonra mübahisə edilən hüquq münasibəti hüquq varisliyinə yol verilmədikdə',
                    N'İddia tələbləri birləşdirildiyindən işin icraatına xitam verildi'
                ) THEN 1 ELSE 0 END) AS REJECTED_BY_CASE_TERMINATION,
            SUM(CASE WHEN OC.RESULT = N'İddia mümkün sayılmadı'
                THEN 1 ELSE 0 END) AS NOT_CONSIDERED_CLAIMS,
            SUM(CASE WHEN OC.RESULT = N'Aidiyyət mübahisələndirildi və iş apellyasiya məhkəməsinə göndərildi'
                THEN 1 ELSE 0 END) AS RESPONDENT_APPEALS
        FROM OfficeCases OC
        GROUP BY OC.OFFICE_KEY_CODE, OC.COURT_ID, OC.COURT_NAME
    ),
    OfficeCourtNames AS
    (
        SELECT
            T.OFFICE_KEY_CODE,
            STRING_AGG(CAST(T.COURT_NAME AS NVARCHAR(MAX)), N', ')
                WITHIN GROUP (ORDER BY T.COURT_NAME) AS COURT_NAMES
        FROM (
            SELECT DISTINCT OFFICE_KEY_CODE, COURT_NAME
            FROM CourtStats
            WHERE COURT_NAME IS NOT NULL
        ) T
        GROUP BY T.OFFICE_KEY_CODE
    )
    SELECT
        RO.OFFICE_NAME AS REGIONAL_OFFICE_NAME,
        CASE WHEN @BY_COURT = 1
             THEN COALESCE(S.COURT_NAME, N'Məhkəmə təyin edilməyib')
             ELSE COALESCE(N.COURT_NAMES, N'Məhkəmə təyin edilməyib')
        END AS COURT_NAME,

        COALESCE(SUM(S.TOTAL_COURTS), 0) AS TOTAL_COURTS,

        -- Cəmi = ONLY the five RESULT categories below.
        COALESCE(SUM(
            S.APPROVED_CLAIMS + S.REJECTED_CLAIMS + S.PARTIAL_CLAIMS
            + S.REJECTED_BY_CASE_TERMINATION + S.NOT_CONSIDERED_CLAIMS
        ), 0) AS TOTAL_BY_RESULTS,
        COALESCE(SUM(S.APPROVED_CLAIMS), 0) AS APPROVED_CLAIMS,
        COALESCE(SUM(S.REJECTED_CLAIMS), 0) AS REJECTED_CLAIMS,
        COALESCE(SUM(S.PARTIAL_CLAIMS), 0) AS PARTIAL_CLAIMS,
        COALESCE(SUM(S.REJECTED_BY_CASE_TERMINATION), 0) AS REJECTED_BY_CASE_TERMINATION,
        COALESCE(SUM(S.NOT_CONSIDERED_CLAIMS), 0) AS NOT_CONSIDERED_CLAIMS,
        COALESCE(SUM(S.RESPONDENT_APPEALS), 0) AS RESPONDENT_APPEALS,

        RO.KEY_CODE AS REGIONAL_OFFICE_KEY_CODE,
        CASE WHEN @BY_COURT = 1 THEN S.COURT_ID ELSE NULL END AS COURT_ID
    FROM config.DIC_REGIONAL_OFFICES RO
    LEFT JOIN CourtStats S
        ON S.OFFICE_KEY_CODE = RO.KEY_CODE
    LEFT JOIN OfficeCourtNames N
        ON N.OFFICE_KEY_CODE = RO.KEY_CODE
    WHERE (@OFFICE_KEY_CODE IS NULL OR RO.KEY_CODE = @OFFICE_KEY_CODE)
    GROUP BY
        RO.KEY_CODE,
        RO.OFFICE_NAME,
        CASE WHEN @BY_COURT = 1 THEN S.COURT_ID ELSE NULL END,
        CASE WHEN @BY_COURT = 1
             THEN COALESCE(S.COURT_NAME, N'Məhkəmə təyin edilməyib')
             ELSE COALESCE(N.COURT_NAMES, N'Məhkəmə təyin edilməyib')
        END
    ORDER BY RO.KEY_CODE, COURT_NAME;
END;
GO

-- Verification: get the office key (e.g. RO_12), then compare both views.
-- EXEC dbo.P_GET_1ST_INSTANCE_STATS @OFFICE_KEY_CODE = N'RO_12', @BY_COURT = 0;
-- EXEC dbo.P_GET_1ST_INSTANCE_STATS @OFFICE_KEY_CODE = N'RO_12', @BY_COURT = 1;

-- Independent inspection of the true office / court association:
-- SELECT A.OFFICE_KEY_CODE, DC.COURT_ID, DC.COURT_NAME,
--        COUNT(DISTINCT C.ID) AS CASES_COUNT
-- FROM (SELECT DISTINCT CASE_ID, OFFICE_KEY_CODE
--       FROM dbo.TBL_CASE_ANALYSIS_RESULT) A
-- JOIN dbo.TBL_CASES C ON C.ID = A.CASE_ID
-- JOIN dbo.DIC_COURTS DC ON DC.COURT_ID = C.COURT_ID
-- WHERE A.OFFICE_KEY_CODE = N'RO_12'
--   AND DC.COURT_TYPE_ID NOT IN (56,91,63,64)
-- GROUP BY A.OFFICE_KEY_CODE, DC.COURT_ID, DC.COURT_NAME
-- ORDER BY CASES_COUNT DESC;
