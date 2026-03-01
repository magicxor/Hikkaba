IF NOT EXISTS (SELECT * FROM sys.fulltext_indexes fti WHERE fti.object_id = OBJECT_ID(N'[dbo].[Threads]'))
    BEGIN
        CREATE FULLTEXT INDEX ON dbo.Threads(Title) KEY INDEX PK_Threads ON FTCHikkaba WITH CHANGE_TRACKING AUTO;
    END
GO
