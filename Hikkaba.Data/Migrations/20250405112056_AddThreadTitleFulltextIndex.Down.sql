IF  EXISTS (SELECT * FROM sys.fulltext_indexes fti WHERE fti.object_id = OBJECT_ID(N'[dbo].[Threads]'))
    BEGIN
        ALTER FULLTEXT INDEX ON [dbo].[Threads] DISABLE
    END
GO

IF  EXISTS (SELECT * FROM sys.fulltext_indexes fti WHERE fti.object_id = OBJECT_ID(N'[dbo].[Threads]'))
    BEGIN
        DROP FULLTEXT INDEX ON [dbo].[Threads]
    END
GO
