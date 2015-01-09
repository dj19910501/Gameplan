DECLARE @MRPDB VARCHAR(50) = 'MRPDev'
DECLARE @BDSAuth VARCHAR(50) = 'BDSAuthDev'
DECLARE @SQLString nvarchar(max)=''
DECLARE @ClientId uniqueidentifier
Declare @IntegrationTypeId int
Declare @PermissionCode varchar(50)='MQL'
Declare @ZebraCode varchar(50) = 'ZBR'
Declare @EloquaCode varchar(50) = 'Eloqua'


		SET @SQLString = N' IF NOT EXISTS(Select ClientIntegrationPermissionId from '+ @MRPDB + '.dbo.[Client_Integration_Permission] Where ClientID = (SElect TOP 1 ClientID from '+ @BDSAuth +'.[dbo].[Client] where Code ='''+@ZebraCode+''') and 
							IntegrationTypeId = (Select TOP 1 IntegrationTypeId from IntegrationType where Code='''+@EloquaCode+''') and
							PermissionCode = '''+@PermissionCode+''')  
							BEGIN '

		SET @SQLString = @SQLString + N' INSERT INTO '+ @MRPDB + '.dbo.[Client_Integration_Permission] Values(
				(SElect TOP 1 ClientID from '+ @BDSAuth +'.[dbo].[Client] where Code ='''+@ZebraCode+'''),
				(Select TOP 1 IntegrationTypeId from IntegrationType where Code='''+@EloquaCode+'''),
				'''+@PermissionCode+''', (SElect TOP 1 ClientID from '+ @BDSAuth +'.[dbo].[Client] where Code ='''+@ZebraCode+'''),GETDATE())
				
				END
				'
Exec(@SQLString)