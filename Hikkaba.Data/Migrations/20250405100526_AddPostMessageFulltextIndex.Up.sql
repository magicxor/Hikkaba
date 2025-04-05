IF  EXISTS (SELECT * FROM sys.fulltext_indexes fti WHERE fti.object_id = OBJECT_ID(N'[dbo].[Posts]'))
    BEGIN
        ALTER FULLTEXT INDEX ON [dbo].[Posts] DISABLE
    END
GO

IF  EXISTS (SELECT * FROM sys.fulltext_indexes fti WHERE fti.object_id = OBJECT_ID(N'[dbo].[Posts]'))
    BEGIN
        DROP FULLTEXT INDEX ON [dbo].[Posts]
    END
GO

IF EXISTS (SELECT * FROM sys.fulltext_catalogs WHERE [name]='FTCHikkaba')
    BEGIN
        DROP FULLTEXT CATALOG FTCHikkaba
    END
GO

CREATE FULLTEXT CATALOG FTCHikkaba AS DEFAULT;

CREATE FULLTEXT INDEX ON dbo.Posts(MessageText) KEY INDEX PK_Posts ON FTCHikkaba WITH CHANGE_TRACKING AUTO;
