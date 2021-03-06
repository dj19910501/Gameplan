IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'NightlyRefreshDatabase') AND xtype IN (N'P'))
    DROP PROCEDURE NightlyRefreshDatabase
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [NightlyRefreshDatabase] 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[_Activity_Value0]') AND TYPE IN (N'U'))
	DROP TABLE [dbo].[_Activity_Value0]

	IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[_Activity_Value1]') AND TYPE IN (N'U'))
	DROP TABLE [dbo].[_Activity_Value1]

	IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[_Activity_Value2]') AND TYPE IN (N'U'))
	DROP TABLE [dbo].[_Activity_Value2]

	DELETE FROM _Database;

	INSERT INTO [_Database] ([Email],[SalesRegion],[FirmType],[AUMLevel],[Marketable],[DateCreated])
	SELECT ec.[EmailAddress]
		,ec.[SalesRegion]
		,ec.[FirmType]
		, rel.Id
		, 'Marketable'
		,CAST(ec.[DateCreated] AS date)
	FROM raw_Eloqua_Contacts ec
	INNER JOIN rel_AUMs rel ON (ec.TDAEstimatedAUM >= CAST(rel.[MIN] as float) AND ec.TDAEstimatedAUM <= CAST(rel.[MAX] as float))
	;

	UPDATE _Database SET SalesRegion = '(blank)' WHERE SalesRegion = '';
	UPDATE _Database SET FirmType = '(blank)' WHERE FirmType = '';

	DELETE FROM _Activity
	WHERE ActivityDate >= '2015-10-28'
	;

	INSERT INTO _Activity ([Email],[ActivityType],[ActivityDate]) 
	SELECT Email, 'Created', CAST(DateCreated as date) FROM _Database
	WHERE DateCreated >= '2015-10-28';

	INSERT INTO _Activity ([Email],[ActivityType],[ActivityDate]) 
	SELECT EmailAddress, ActivityType, CAST(ActivityDate as date) FROM raw_eloqua_Activities
	WHERE ActivityType IN ('Bounceback', 'Unsubscribe')
	AND ActivityDate  >= '2015-10-28';

	UPDATE [_Database] SET Marketable = 'Unmarketable'
	WHERE Email IN (SELECT Email FROM _Activity WHERE ActivityType IN ('Bounceback', 'Unsubscribe'))
	;

	IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[_EmailActivityNew_Value0]') AND TYPE IN (N'U'))
	DROP TABLE [dbo].[_EmailActivityNew_Value0]

	DELETE FROM _EmailActivityNew WHERE SendDate >= '2015-10-28';
	--DBCC CHECKIDENT (_EmailActivityNew, reseed, SELECT MAX(ID) FROM _EmailActivityNew);

	INSERT INTO _EmailActivityNew ([SendDate],[ContactID],[EmailAddress],[AssetName], isOpened, isClickThrough, IsBounceback)
	SELECT CAST([ActivityDate] as date),[ContactID],[EmailAddress],[AssetName], 0, 0, 0
	FROM raw_eloqua_Activities
	WHERE ActivityType IN ('EmailSend')
	AND ActivityDate >= '2015-10-28'
	;

	UPDATE e1
	SET e1.isOpened = 1
	FROM _EmailActivityNew e1
	INNER JOIN raw_eloqua_Activities elq ON (e1.AssetName = elq.AssetName AND e1.EmailAddress = elq.EmailAddress)
	WHERE elq.ActivityType = 'EmailOpen'
	;

	UPDATE e1
	SET e1.isClickThrough = 1
	FROM _EmailActivityNew e1
	INNER JOIN raw_eloqua_Activities elq ON (e1.AssetName = elq.AssetName AND e1.EmailAddress = elq.EmailAddress)
	WHERE elq.ActivityType = 'EmailClickthrough'
	;

	UPDATE e1
	SET e1.IsBounceback = 1
	FROM _EmailActivityNew e1
	INNER JOIN raw_eloqua_Activities elq ON (e1.AssetName = elq.AssetName AND e1.EmailAddress = elq.EmailAddress)
	WHERE elq.ActivityType = 'Bounceback'
	;


END

GO
