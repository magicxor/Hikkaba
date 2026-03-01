IF NOT EXISTS (SELECT * FROM sys.fulltext_catalogs WHERE [name] = 'FTCHikkaba')
    BEGIN
        CREATE FULLTEXT CATALOG FTCHikkaba AS DEFAULT;
    END
GO

IF NOT EXISTS (SELECT * FROM sys.fulltext_indexes fti WHERE fti.object_id = OBJECT_ID(N'[dbo].[Posts]'))
    BEGIN
        CREATE FULLTEXT INDEX ON dbo.Posts(MessageText) KEY INDEX PK_Posts ON FTCHikkaba WITH CHANGE_TRACKING AUTO;
    END
GO
