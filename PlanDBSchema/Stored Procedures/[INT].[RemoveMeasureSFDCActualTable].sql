-- =============================================
-- Author:		Mitesh Vaishnav
-- Create date: 08/20/2016
-- Description:	Sp will Remove temp table for MEasure SFDC actuals data 
-- =============================================
ALTER PROCEDURE [INT].[RemoveMeasureSFDCActualTable]
(
	@MeasureSFDCActualTableName NVARCHAR(255)
)
AS
BEGIN
	DECLARE @CustomQuery NVARCHAR(MAX)
	DECLARE @TableNameWithoutSchema NVARCHAR(100)

	SET @CustomQuery='IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA=''dbo'' AND TABLE_NAME='''+@MeasureSFDCActualTableName+''')
							BEGIN
							DROP TABLE '+@MeasureSFDCActualTableName+';
							END'
			EXEC(@CustomQuery)


END

