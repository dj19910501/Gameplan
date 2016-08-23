-- =============================================
-- Author:		Mitesh Vaishnav
-- Create date: 08/20/2016
-- Description:	Sp will create temp table for MEasure SFDC actuals data 
-- =============================================
ALTER PROCEDURE [INT].[GETMeasureClientDbName]
(
@ClientId nvarchar(36),
@AuthDatabaseName Nvarchar(1000)
)
AS
BEGIN
	
DECLARE @CustomQuery NVARCHAR(MAX)

		SET @CustomQuery='DECLARE @ApplicationId nvarchar(36)=''''
						  DECLARE @ClientConnection NVARCHAR(1000)=''''
						  DECLARE @DatabaseName NVARCHAR(1000)=''''
						
						  SELECT TOP 1 @ApplicationId=ApplicationId 
						  FROM '+@AuthDatabaseName+'.[Dbo].[Application] 
						  WHERE Code=''RPC''

						 SELECT TOP 1 @ClientConnection='+@AuthDatabaseName+'.[dbo].[DecryptString](EncryptedConnectionString) 
						 FROM '+@AuthDatabaseName+'.[dbo].[ClientDatabase] 
						 WHERE clientid='''+@ClientID+''' 
								AND ApplicationId=@ApplicationId
						IF (@ClientConnection IS NOT NULL AND @ClientConnection<>'''')
						BEGIN

						SELECT @DatabaseName=dimension FROM [dbo].fnSplitString(@ClientConnection,'';'') WHERE dimension like ''%initial catalog=%''
						SET @DatabaseName= REPLACE(@DatabaseName,''initial catalog='','''')
						END
						SELECT TOP 1 @DatabaseName AS DbName'

EXEC (@CustomQuery);

END



