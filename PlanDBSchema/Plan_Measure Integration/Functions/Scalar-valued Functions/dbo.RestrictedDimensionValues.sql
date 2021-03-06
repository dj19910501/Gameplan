IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'RestrictedDimensionValues') AND xtype IN (N'FN', N'IF', N'TF'))
    DROP FUNCTION RestrictedDimensionValues
GO


CREATE FUNCTION [RestrictedDimensionValues] 
(
	@DimensionId INT,
	@UserId UNIQUEIDENTIFIER = '14D7D588-CF4D-46BE-B4ED-A74063B67D66',
	@RoleId UNIQUEIDENTIFIER = '504F5E26-2208-44C2-A78F-4BDF4BAB703F'
)
RETURNS NVARCHAR(MAX) AS
BEGIN
		-- Restrict dimension values as per UserId and RoleId
		DECLARE @RestrictedDimensionValues NVARCHAR(MAX)
		
		IF EXISTS(SELECT TOP 1 DimensionValue FROM User_RestrictedDimensionValues WHERE DimensionId = @Dimensionid AND UserId = @UserId)
		BEGIN
			SELECT @RestrictedDimensionValues = COALESCE(@RestrictedDimensionValues + ',' ,'') + DimensionValue 
			--SELECT @RestrictedDimensionValues = COALESCE(@RestrictedDimensionValues + ''',''' ,'') + DimensionValue 
			FROM User_RestrictedDimensionValues WHERE DimensionId = @Dimensionid AND UserId = @UserId
		END
		ELSE 
		BEGIN
			--SELECT @RestrictedDimensionValues = COALESCE(@RestrictedDimensionValues + ''',''' ,'') + DimensionValue 
			
			SELECT @RestrictedDimensionValues = COALESCE(@RestrictedDimensionValues + ',' ,'') + DimensionValue 
			FROM Role_RestrictedDimensionValues WHERE DimensionId = @Dimensionid AND RoleId = @RoleId
		END
	--SET @RestrictedDimensionValues = ''''+ @RestrictedDimensionValues+''''
	RETURN @RestrictedDimensionValues

END

GO
