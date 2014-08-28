--Execute this script on MRP database
IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Funnel')
BEGIN

	IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Funnel' AND COLUMN_NAME = 'Description')
	BEGIN

		UPDATE dbo.Funnel SET Description='I use marketing to generate leads annually from my website, blogs and social media efforts with an average deal size of #MarketingDealSize.' WHERE Title='Marketing'

	END

END