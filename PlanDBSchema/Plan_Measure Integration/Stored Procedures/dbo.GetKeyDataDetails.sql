IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'GetKeyDataDetails') AND xtype IN (N'P'))
    DROP PROCEDURE GetKeyDataDetails
GO


-- ==========================================================
-- Author:		Nandish Shah
-- Create date: 12/14/2015
-- Description:	Add Magnitude Value in KeyData 
-- ==========================================================
CREATE PROCEDURE [dbo].[GetKeyDataDetails]
@KeyDataId INT, 
@DimensionTableName NVARCHAR(100), 
@StartDate DATE='1-1-1900', 
@EndDate DATE='1-1-2100', 
@CompStartDate DATE='1-1-1900', 
@CompEndDate DATE='1-1-2100', 
@DateField nvarchar(100)=null, 
@FilterValues NVARCHAR(max)=null,
@ViewByValue nvarchar(15)=null,
@UserId UNIQUEIDENTIFIER = '14D7D588-CF4D-46BE-B4ED-A74063B67D66',
@RoleId UNIQUEIDENTIFIER = '504F5E26-2208-44C2-A78F-4BDF4BAB703F'
AS
BEGIN
	DECLARE @KeyDataActual FLOAT,
			@KeyDataComp FLOAT,
			@TempValue FLOAT,
			@FormattedKeyDataActual NVARCHAR(50),
			@FormattedKeyDataTooltip NVARCHAR(50),
			@FormattedKeyDataComp NVARCHAR(50),
			@KDActualOutput NVARCHAR(50),
			@KDCompOutput NVARCHAR(50),
			@CompDataTooltip NVARCHAR(50),
			@ComparisonColor NVARCHAR(20),
			@ComparisonDiffValue FLOAT;
	
	-- get and set properties of key data
	DECLARE @KeyDataName NVARCHAR(50),
			@MeasureId INT,
			@Prefix NVARCHAR(10),
			@Suffix NVARCHAR(10),
			@DisplayIfZero NVARCHAR(20),
			@NoOfDecimal INT,
			@ComparisonType NVARCHAR(10),
			@IsComparisonDisplay BIT,
			@IsIndicatorDisplay BIT,
			@IsGoalDisplayForFilterCriteria BIT,
			@DateRangeGoalOption INT,
			@MagnitudeValue nvarchar(10),
			@HelpTextId int
	
	SELECT  @KeyDataName = KeyDataName,
			@MeasureId = MeasureId,
			@Prefix = Prefix ,
			@Suffix = Suffix,
			@DisplayIfZero = DisplayIfZero,
			@NoOfDecimal = (CASE WHEN ISNULL(NoOfDecimal , -1) < -1 THEN 2 ELSE NoOfDecimal END),
			@ComparisonType = ISNULL(ComparisonType,'ABSOLUTE'),
			@IsComparisonDisplay = ISNULL(IsComparisonDisplay,0),
			@IsIndicatorDisplay = ISNULL(IsIndicatorDisplay,0),
			@IsGoalDisplayForFilterCriteria = ISNULL(IsGoalDisplayForFilterCriteria,0),
			@DateRangeGoalOption = DateRangeGoalOption,
			@MagnitudeValue = (CASE WHEN LOWER(ISNULL(MagnitudeValue,0)) = 'k' THEN 3 WHEN LOWER(ISNULL(MagnitudeValue,0)) = 'm' THEN 6 WHEN LOWER(ISNULL(MagnitudeValue,0)) = 'b' THEN 9 WHEN LOWER(ISNULL									(MagnitudeValue,0)) = 't' THEN 12 WHEN LOWER(ISNULL(MagnitudeValue,0)) = 'q' THEN 15 WHEN (LOWER(ISNULL(MagnitudeValue,0)) IN ('3','6','9','12','15')) THEN MagnitudeValue ELSE 0 END),
			@HelpTextId = ISNULL(HelpTextId, 0) 
	FROM KeyData WHERE id = @KeyDataId

	-- Get actual key data value
	SET @TempValue = 0;
	EXECUTE KeyDataGet @KeyDataId,@DimensionTableName,@StartDate,@EndDate,@DateField,@FilterValues,@ViewByValue,@UserId,@RoleId,@TempValue OUTPUT;
	SET @KeyDataActual = @TempValue;

	DECLARE @MagSymbol nvarchar(10)
	SET @MagSymbol = (CASE WHEN @MagnitudeValue = '3' THEN 'k' WHEN @MagnitudeValue = '6' THEN 'M' WHEN @MagnitudeValue = '9' THEN 'B' WHEN @MagnitudeValue = '12' THEN 'T' WHEN @MagnitudeValue = '15' THEN 'Q' ELSE '0' END)
	
	IF(ISNUMERIC(@KeyDataActual) = 1)
	BEGIN
		BEGIN /* Region of Actual Key Data Value */
			-- Replace DisplayIfZero value 
			IF(ISNULL(@DisplayIfZero,'') != '' AND @KeyDataActual = 0)
			BEGIN
				SET @FormattedKeyDataActual = @DisplayIfZero
			END
			ELSE
			BEGIN
				-- Replace actual key data value with measure output value from MeasureOutputValue table
				SET @KDActualOutput = dbo.GetMeasureOutputValue(@KeyDataActual,@MeasureId)
				IF(ISNUMERIC(@KDActualOutput) = 1)
				BEGIN
					-- Format key data value as per NoOfDecimal property

					DECLARE @StrFormat NVARCHAR(20) = ''
					DECLARE @TooltipStrFormat NVARCHAR(20) = ''
					DECLARE @MagDivVal NVARCHAR(20) = ''
					DECLARE @MagDivValActual FLOAT
					IF(@MagnitudeValue != 0)
					begin
						SET @MagDivVal = '1' + REPLACE(STR('0',CAST(@MagnitudeValue AS int)),' ','0')
						SET @MagDivValActual = CAST(@KDActualOutput AS FLOAT) / CAST(@MagDivVal AS float)

						IF(ISNULL(@NoOfDecimal,-1) = -1)
						BEGIN
							IF (CHARINDEX('.', CAST(@MagDivValActual AS FLOAT)) > 0)
							BEGIN
								SET @StrFormat = REPLACE(STR('0',2),' ','0')
							END
							IF (CHARINDEX('.', CAST(@KDActualOutput AS FLOAT)) > 0)
							BEGIN
								SET @TooltipStrFormat = REPLACE(STR('0',2),' ','0')
							END
						END
						ELSE
						BEGIN
							SET @StrFormat = REPLACE(STR('0',@NoOfDecimal),' ','0')
							SET @TooltipStrFormat = REPLACE(STR('0',@NoOfDecimal),' ','0')
						END

						IF(ISNULL(@StrFormat,'') != '')
						begin
							SET @StrFormat = '#,##0.' + @StrFormat 
							SET @TooltipStrFormat = '#,##0.' + @TooltipStrFormat 
							end
						ELSE
						begin
							SET @StrFormat = '#,##0'
							SET @TooltipStrFormat = '#,##0'
						end
						
						SET @FormattedKeyDataActual = FORMAT(@MagDivValActual,@StrFormat) + @MagSymbol
						SET @FormattedKeyDataTooltip = FORMAT(@KeyDataActual,@TooltipStrFormat)
					end
					else
					begin
						IF(ISNULL(@NoOfDecimal,-1) = -1)
						BEGIN
							IF (CHARINDEX('.', CAST(@KDActualOutput AS FLOAT)) > 0)
							BEGIN
								SET @StrFormat = REPLACE(STR('0',2),' ','0')
							END
						END
						ELSE
						BEGIN
							SET @StrFormat = REPLACE(STR('0',@NoOfDecimal),' ','0')
						END

						IF(ISNULL(@StrFormat,'') != '')
						begin
							SET @StrFormat = '#,##0.' + @StrFormat 
							end
						ELSE
						begin
							SET @StrFormat = '#,##0'
						end
						SET @FormattedKeyDataActual = FORMAT(CAST(@KDActualOutput AS FLOAT),@StrFormat)
						SET @FormattedKeyDataTooltip = FORMAT(@KeyDataActual,@StrFormat)
					end
				END
				ELSE
				BEGIN
					SET @FormattedKeyDataActual = @KDActualOutput
				END
			END
			-- Add prefix and suffix to key data
			SET @FormattedKeyDataActual = CASE WHEN ISNULL(@Prefix,'') = '' THEN '' ELSE @Prefix + ' ' END + @FormattedKeyDataActual +
					       				  CASE WHEN ISNULL(@Suffix,'') = '' THEN '' ELSE ' ' + @Suffix END 	

			SET @FormattedKeyDataTooltip = CASE WHEN ISNULL(@Prefix,'') = '' THEN '' ELSE @Prefix + ' ' END + @FormattedKeyDataTooltip +
											CASE WHEN ISNULL(@Suffix,'') = '' THEN '' ELSE ' ' + @Suffix END 	
		END /* Region of Actual Key Data Value */

		BEGIN /* Region of Key Data Comparison Value */
			IF(@IsComparisonDisplay = 1)
			BEGIN
				-- Get comparison value for key data
				SET @TempValue = 0;
				EXECUTE KeyDataGet @KeyDataId,@DimensionTableName,@CompStartDate,@CompEndDate,@DateField,@FilterValues,@ViewByValue,@UserId,@RoleId ,@TempValue OUTPUT;
				SET @KeyDataComp = @TempValue;

				IF(ISNUMERIC(@KeyDataComp) = 1)
				BEGIN
					-- Set color of the comparison value
					IF(@KeyDataActual < @KeyDataComp)
						SET @ComparisonColor = 'red'
					ELSE
						SET @ComparisonColor = 'green'

					-- Get comparison value of key data
					IF(UPPER(@ComparisonType) = 'RATE')
					BEGIN
						IF(@KeyDataComp = 0)
							SET @ComparisonDiffValue = 100;
						ELSE IF(@KeyDataActual = @KeyDataComp)
						BEGIN
							SET @ComparisonDiffValue = 0;
							--SET @ComparisonColor = 'blue'
						END
						ELSE
							SET @ComparisonDiffValue = ((@KeyDataActual - @KeyDataComp) * 100) / @KeyDataComp;
					END
					ELSE 
					BEGIN
						SET @ComparisonDiffValue = @KeyDataActual - @KeyDataComp;
					END
					IF(@ComparisonDiffValue < 0)
						SET @ComparisonDiffValue = @ComparisonDiffValue * -1;


					-- Format key data comparison value to default decimal format
					DECLARE @StrCompFormat NVARCHAR(20) = ''
					DECLARE @TooltipStrCompFormat NVARCHAR(20) = ''

					DECLARE @MagDivValComp FLOAT
					IF(@MagnitudeValue != 0)
					BEGIN
						SET @MagDivValComp = CAST(@ComparisonDiffValue AS FLOAT) / CAST(@MagDivVal AS float)

						IF(ISNULL(@NoOfDecimal,-1) = -1)
						BEGIN
							IF (CHARINDEX('.', CAST(@MagDivValComp AS FLOAT)) > 0)
							BEGIN
								SET @StrCompFormat = REPLACE(STR('0',2),' ','0')
							END
							IF (CHARINDEX('.', CAST(@KDActualOutput AS FLOAT)) > 0)
							BEGIN
								SET @TooltipStrCompFormat = REPLACE(STR('0',2),' ','0')
							END
						END
						ELSE
						BEGIN
							SET @StrCompFormat = REPLACE(STR('0',@NoOfDecimal),' ','0')
							SET @TooltipStrCompFormat = REPLACE(STR('0',@NoOfDecimal),' ','0')
						END

						IF(ISNULL(@StrCompFormat,'') != '')
						begin
							SET @StrCompFormat = '#,##0.' + @StrCompFormat 
							SET @TooltipStrCompFormat = '#,##0.' + @TooltipStrCompFormat 
							end
						ELSE
						begin
							SET @StrCompFormat = '#,##0'
							SET @TooltipStrCompFormat = '#,##0'
						end

						SET @FormattedKeyDataComp = FORMAT(@MagDivValComp,@StrCompFormat) + @MagSymbol
						SET @CompDataTooltip = FORMAT(@ComparisonDiffValue, @TooltipStrCompFormat)
					END
					ELSE
						BEGIN
						IF(ISNULL(@NoOfDecimal,-1) = -1)
						BEGIN
							IF (CHARINDEX('.', CAST(@ComparisonDiffValue AS FLOAT)) > 0)
							BEGIN
								SET @StrCompFormat = REPLACE(STR('0',2),' ','0')
							END
						END
						ELSE
						BEGIN
							SET @StrCompFormat = REPLACE(STR('0',@NoOfDecimal),' ','0')
						END

						IF(ISNULL(@StrCompFormat,'') != '')
						begin
							SET @StrCompFormat = '#,##0.' + @StrCompFormat 
							end
						ELSE
						begin
							SET @StrCompFormat = '#,##0'
						end
						SET @FormattedKeyDataComp = FORMAT(@ComparisonDiffValue, @StrCompFormat)
						SET @CompDataTooltip = FORMAT(@ComparisonDiffValue, @StrCompFormat)
					END
					IF(UPPER(@ComparisonType) = 'RATE')
					BEGIN
						SET @FormattedKeyDataComp = @FormattedKeyDataComp + '%'
						SET @CompDataTooltip = CAST(@ComparisonDiffValue AS nvarchar(50)) + '%'
						SET @CompDataTooltip = FORMAT(@ComparisonDiffValue, @StrCompFormat) + '%'
					END
					ELSE
					BEGIN
						-- Add prefix and suffix to key data comparison
						SET @FormattedKeyDataComp = CASE WHEN ISNULL(@Prefix,'') = '' THEN '' ELSE @Prefix + ' ' END + @FormattedKeyDataComp +
												CASE WHEN ISNULL(@Suffix,'') = '' THEN '' ELSE ' ' + @Suffix END
						
						SET @CompDataTooltip = CASE WHEN ISNULL(@Prefix,'') = '' THEN '' ELSE @Prefix + ' ' END + @CompDataTooltip +
												CASE WHEN ISNULL(@Suffix,'') = '' THEN '' ELSE ' ' + @Suffix END
					END
				END
			END
		END /* Region of Key Data Comparison Value */
	END
	ELSE 
	BEGIN
		SET @KeyDataActual = 0
		SET @FormattedKeyDataActual = @DisplayIfZero
		SET @FormattedKeyDataActual = CASE WHEN ISNULL(@Prefix,'') = '' THEN '' ELSE @Prefix + ' ' END + @FormattedKeyDataActual +
				       				  CASE WHEN ISNULL(@Suffix,'') = '' THEN '' ELSE ' ' + @Suffix END 	
	END

	SELECT @KeyDataName AS KeyDataName,
		   @KeyDataActual AS KeyDataActual, 
		   @FormattedKeyDataActual AS FormattedKeyDataValue, 
		   @FormattedKeyDataTooltip AS FormattedKeyDataTooltip,
		   @FormattedKeyDataComp AS ComparisonValue, 
		   @IsComparisonDisplay AS IsComparisonDisplay,
		   @CompDataTooltip AS  ComparisonTooltip,	
		   @ComparisonColor AS ComparisonColor,
		   @IsIndicatorDisplay AS IsIndicatorDisplay,		
		   @IsGoalDisplayForFilterCriteria AS IsGoalDisplayForFilterCriteria,
		   @DateRangeGoalOption AS DateRangeGoalOption,
		   @HelpTextId AS HelpTextId 
END