/* Start - Added by Arpita Soni on 08/09/2016 for Ticket #2464 - Custom Alerts and Notifications */

-- Create table Alert_Rules
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Alert_Rules]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[Alert_Rules](
		[RuleId] [int] IDENTITY(1,1) NOT NULL,
		[RuleSummary] [nvarchar](max) NOT NULL,
		[EntityId] [int] NOT NULL,
		[EntityType] [nvarchar](50) NOT NULL,
		[Indicator] [nvarchar](50) NOT NULL,
		[IndicatorComparision] [nvarchar](10) NOT NULL,
		[IndicatorGoal] [int] NOT NULL,
		[CompletionGoal] [int] NOT NULL,
		[Frequency] [nvarchar](50) NOT NULL,
		[DayOfWeek] [tinyint] NULL,
		[DateOfMonth] [tinyint] NULL,
		[LastProcessingDate] [datetime] NULL,
		[UserId] [uniqueidentifier] NOT NULL,
		[ClientId] [uniqueidentifier] NOT NULL,
		[IsDisabled] [bit] NOT NULL DEFAULT 0,
		[CreatedDate] [datetime] NOT NULL,
		[CreatedBy] [uniqueidentifier] NOT NULL,
		[ModifiedDate] [datetime] NULL,
		[ModifiedBy] [uniqueidentifier] NULL,
	 CONSTRAINT [PK_Alert_Rules] PRIMARY KEY CLUSTERED 
	(
		[RuleId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO

-- Create table Alerts
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Alerts_Alert_Rules]') AND parent_object_id = OBJECT_ID(N'[dbo].[Alerts]'))
BEGIN
	ALTER TABLE [dbo].[Alerts] DROP CONSTRAINT [FK_Alerts_Alert_Rules]
END
GO
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Alerts]') AND type in (N'U'))
BEGIN
	DROP TABLE [dbo].[Alerts]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Alerts]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[Alerts](
		[AlertId] [int] IDENTITY(1,1) NOT NULL,
		[RuleId] [int] NOT NULL,
		[Description] [nvarchar](max) NOT NULL,
		[IsRead] [bit] NOT NULL DEFAULT 0,
		[ReadDate] [datetime] NULL,
		[UserId] [uniqueidentifier] NOT NULL,
		[CreatedDate] [datetime] NOT NULL,
		[DisplayDate] [datetime] NOT NULL,
	CONSTRAINT [PK_Alerts] PRIMARY KEY CLUSTERED 
	(
		[AlertId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO

-- Create FK FK_Alerts_Alert_Rules in table Alerts
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Alerts_Alert_Rules]') AND parent_object_id = OBJECT_ID(N'[dbo].[Alerts]'))
ALTER TABLE [dbo].[Alerts]  WITH CHECK ADD  CONSTRAINT [FK_Alerts_Alert_Rules] FOREIGN KEY([RuleId])
REFERENCES [dbo].[Alert_Rules] ([RuleId])
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Alerts_Alert_Rules]') AND parent_object_id = OBJECT_ID(N'[dbo].[Alerts]'))
ALTER TABLE [dbo].[Alerts] CHECK CONSTRAINT [FK_Alerts_Alert_Rules]
GO

-- Create table User_Notification_Messages
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[User_Notification_Messages]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[User_Notification_Messages](
		[NotificationId] [int] IDENTITY(1,1) NOT NULL,
		[ComponentName] [nvarchar](50) NOT NULL,
		[ComponentId] [int] NULL,
		[EntityId] [int] NULL,
		[Description] [nvarchar](250) NULL,
		[ActionName] [nvarchar](50) NULL,
		[IsRead] [bit] NOT NULL DEFAULT 0,
		[ReadDate] [datetime] NULL,
		[UserId] [uniqueidentifier] NULL,
		[RecipientId] [uniqueidentifier] NOT NULL,
		[CreatedDate] [datetime] NOT NULL,
		[ClientID] [uniqueidentifier] NULL,
	 CONSTRAINT [PK_User_Notification_Detail] PRIMARY KEY CLUSTERED 
	(
		[NotificationId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
END
GO

-- Alter column ObjectId
IF EXISTS(SELECT * FROM sys.columns WHERE [name] = 'ObjectId' AND [object_id] = OBJECT_ID(N'ChangeLog'))
BEGIN
	ALTER TABLE ChangeLog ALTER COLUMN ObjectId INT NULL
END
GO

-- Alter column ComponentId
IF EXISTS(SELECT * FROM sys.columns WHERE [name] = 'ComponentId' AND [object_id] = OBJECT_ID(N'ChangeLog'))
BEGIN
	ALTER TABLE ChangeLog ALTER COLUMN ComponentId INT NULL
END
GO

-- Alter column UserId
IF EXISTS(SELECT * FROM sys.columns WHERE [name] = 'UserId' AND [object_id] = OBJECT_ID(N'ChangeLog'))
BEGIN
	ALTER TABLE ChangeLog ALTER COLUMN UserId UNIQUEIDENTIFIER NULL
END
GO

IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = 'IsCurrency' AND [object_id] = OBJECT_ID(N'Measure'))
BEGIN
	ALTER TABLE dbo.Measure ADD IsCurrency BIT NULL
END
GO

/* End - Added by Arpita Soni on 08/09/2016 for Ticket #2464 - Custom Alerts and Notifications */

/* Added by devanshi for #2569: create alert rule*/
/****** Object:  View [dbo].[vClientWise_EntityList]    Script Date: 08/11/2016 10:56:10 AM ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vClientWise_EntityList]'))
DROP VIEW [dbo].[vClientWise_EntityList]
GO
/****** Object:  View [dbo].[vClientWise_EntityList]    Script Date: 08/11/2016 10:56:10 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vClientWise_EntityList]'))
EXEC dbo.sp_executesql @statement = N'


CREATE VIEW [dbo].[vClientWise_EntityList] AS
WITH AllPlans AS(
SELECT P.PlanId EntityId, P.Title EntityTitle, M.ClientId, ''Plan'' Entity, 1 EntityOrder 
FROM [Plan] P 
INNER JOIN Model M ON M.ModelId = P.ModelId AND P.IsDeleted = 0
WHERE  M.IsDeleted = 0
),
AllCampaigns AS
(
       SELECT P.PlanCampaignId EntityId, P.Title EntityTitle,C.ClientId, ''Campaign'' Entity, 2 EntityOrder 
       FROM Plan_Campaign P
              INNER JOIN AllPlans C ON P.PlanId = C.EntityId 
       WHERE P.IsDeleted = 0
),
AllProgram AS
(
       SELECT P.PlanProgramId EntityId, P.Title EntityTitle,C.ClientId, ''Program'' Entity, 3 EntityOrder 
       FROM Plan_Campaign_Program P
              INNER JOIN AllCampaigns C ON P.PlanCampaignId = C.EntityId 
       WHERE P.IsDeleted = 0
),
AllLinkedTactic as
(
SELECT P.LinkedTacticId 
       FROM Plan_Campaign_Program_Tactic P
              INNER JOIN AllProgram C ON P.PlanProgramId = C.EntityId 
       WHERE P.IsDeleted = 0 and P.Status in (''In-Progress'',''Approved'',''Complete'') and P.LinkedTacticId is not null
),
AllTactic AS
(
       SELECT P.PlanTacticId EntityId, P.Title EntityTitle,C.ClientId, ''Tactic'' Entity, 4 EntityOrder 
       FROM Plan_Campaign_Program_Tactic P
              INNER JOIN AllProgram C ON P.PlanProgramId = C.EntityId 
			  LEFT OUTER JOIN AllLinkedTactic L on P.PlanTacticId=L.LinkedTacticId
       WHERE P.IsDeleted = 0 and P.Status in (''In-Progress'',''Approved'',''Complete'') and L.LinkedTacticId is null
),
AllLineitem AS
(
       SELECT P.PlanLineItemId EntityId, P.Title EntityTitle, C.ClientId, ''LineItem'' Entity, 5 EntityOrder 
       FROM Plan_Campaign_Program_Tactic_LineItem P
              INNER JOIN AllTactic C ON P.PlanTacticId = C.EntityId 
       WHERE P.IsDeleted = 0 and P.LineItemTypeId is not null
)
SELECT * FROM AllPlans
UNION ALL 
SELECT * FROM AllCampaigns
UNION ALL 
SELECT * FROM AllProgram
UNION ALL 
SELECT * FROM AllTactic
UNION ALL 
SELECT * FROM AllLineitem

' 
GO
/*end*/

IF NOT EXISTS (SELECT * FROM sys.types WHERE is_table_type = 1 AND name = 'PreferredCurrenctDetails')
BEGIN
-- Create the data type
CREATE TYPE [dbo].[PreferredCurrenctDetails] AS TABLE(
	[StartDate] [datetime] NULL,
	[EndDate] [datetime] NULL,
	[Rate] [float] NULL
)

END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE  object_id = OBJECT_ID(N'DimensionBaseQuery') AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
  DROP FUNCTION [dbo].[DimensionBaseQuery]
GO

CREATE FUNCTION [dbo].[DimensionBaseQuery] 
( 
    @DIMENSIONTABLENAME NVARCHAR(1000), 
	@STARTDATE date='01/01/2014', 
	@ENDDATE date='12/31/2014',
	@ReportGraphId int ,
	@FilterValues nvarchar(MAX) =null ,
	@IsOnlyDateDimension bit =0,
	@IsDimension bit=0,
	@UserId UNIQUEIDENTIFIER = '14D7D588-CF4D-46BE-B4ED-A74063B67D66',
	@RoleId UNIQUEIDENTIFIER = '504F5E26-2208-44C2-A78F-4BDF4BAB703F',
	@RateList NVARCHAR(50) = ''
) 
RETURNS  NVARCHAR(MAX)
BEGIN
DECLARE @OrderByCount int=1
DECLARE @DimensionCount int=0
DECLARE @Dimensionid int,@DimensionIndex NVARCHAR(100),@IsDateDimension BIT, @i INT =3
DECLARE @ExcludeQuery nvarchar(MAX)
SET @ExcludeQuery=''

DECLARE @QueryToRun NVARCHAR(MAX) 
--Chek is only Date dimension then it will get values from dimension value table
	IF(@IsOnlyDateDimension=1 AND @FilterValues IS NULL)
	BEGIN
		SET @DimensionId=(SELECT TOP 1 DimensionId from ReportAxis where reportgraphid=@ReportGraphID )
		SET @QueryToRun='';
		IF(@IsDimension=0)
		BEGIN
			IF(@RateList = '' OR @RateList IS NULL)
			BEGIN
			SET @QueryToRun =  'SELECT ' + 'DISTINCT #COLUMNS# '  + ' FROM MeasureValue D1 INNER JOIN DimensionValue D3 ON D1.DimensionValue = d3.id and CAST(d3.DisplayValue AS DATE) between ''' + cast(@STARTDATE as nvarchar) + ''' and ''' + cast(@ENDDATE as nvarchar) + '''  AND d3.DimensionId =' + REPLACE(@DIMENSIONTABLENAME,'D','') 
			END
			ELSE
			BEGIN
			SET @QueryToRun =  'SELECT ' + 'DISTINCT #COLUMNS# '  + ' FROM MeasureValue D1 INNER JOIN DimensionValue D3 ON D1.DimensionValue = d3.id and CAST(d3.DisplayValue AS DATE) between ''' + cast(@STARTDATE as nvarchar) + ''' and ''' + cast(@ENDDATE as nvarchar) + '''  AND d3.DimensionId =' + REPLACE(@DIMENSIONTABLENAME,'D','') +' INNER JOIN '+@RateList+' rt ON  CAST(D3.DisplayValue AS DATE) between CAST(StartDate AS DATE) and CAST(EndDate AS DATE)' 
			END
		END
		ELSE
		BEGIN
			SET @QueryToRun ='SELECT '+'DISTINCT #COLUMNS# '+'  from dimensionvalue D3 where DimensionID=' + REPLACE(@DIMENSIONTABLENAME,'D','')+' and CAST(DisplayValue AS DATE) between '''+CAST(@STARTDATE AS NVARCHAR)+''' and '''+CAST(@ENDDATE AS NVARCHAR)+'''' -- INNER JOIN '+@RateList+' rt ON CAST(D3.DisplayValue AS DATE) between CAST(StartDate AS DATE) and CAST(EndDate AS DATE) order by ordervalue1
		END
	END
	ELSE
	BEGIN
		SET @QueryToRun= 'SELECT distinct '+' #COLUMNS# '+' FROM ' + @DIMENSIONTABLENAME + 'Value D1'
		SET @QueryToRun = @QueryToRun + ' INNER JOIN ' + @DIMENSIONTABLENAME + ' D2 ON D2.id = D1.' + @DIMENSIONTABLENAME 

		DECLARE Column_Cursor CURSOR FOR
		SELECT D.id,D.IsDateDimension,FD.cnt
			FROM dbo.fnSplitString(@DIMENSIONTABLENAME,'D') FD
			INNER JOIN Dimension D ON D.id = FD.dimension
		OPEN Column_Cursor
		FETCH NEXT FROM Column_Cursor
		INTO @Dimensionid,@IsDateDimension, @DimensionIndex
		WHILE @@FETCH_STATUS = 0
		BEGIN	    
			/* Start - Added by Arpita Soni for ticket #511 on 10/30/2015 */
			-- Restrict dimension values as per UserId and RoleId
			--Insertation Start 24/02/2016 Kausha Added if conditionDue to resolve performance issue we have removed unneccesary join for ticket no #729,#730
			IF(@IsDateDimension=1 OR ((SELECT COUNT(*) FROM ReportAxis WHERE ReportGraphID=@ReportGraphId AND DimensionId=@Dimensionid)>0))
			BEGIN
				DECLARE @RestrictedDimensionValues NVARCHAR(MAX)
		
				IF EXISTS(SELECT TOP 1 DimensionValue FROM User_RestrictedDimensionValues WHERE DimensionId = @Dimensionid AND UserId = @UserId)
				BEGIN
					SELECT @RestrictedDimensionValues = COALESCE(@RestrictedDimensionValues + ''',''' ,'') + DimensionValue 
					FROM User_RestrictedDimensionValues WHERE DimensionId = @Dimensionid AND UserId = @UserId
				END
				ELSE 
				BEGIN
					SELECT @RestrictedDimensionValues = COALESCE(@RestrictedDimensionValues + ''',''' ,'') + DimensionValue 
					FROM Role_RestrictedDimensionValues WHERE DimensionId = @Dimensionid AND RoleId = @RoleId
				END
				IF(CHARINDEX(',',@RestrictedDimensionValues) = 2)
				BEGIN
					SET @RestrictedDimensionValues = SUBSTRING(@RestrictedDimensionValues,4,LEN(@RestrictedDimensionValues))
				END
				/* End - Added by Arpita Soni for ticket #511 on 10/30/2015 */

				IF(@ExcludeQuery IS NOT NULL)
					SET @ExcludeQuery=@ExcludeQuery+' and '
				SET @ExcludeQuery=@ExcludeQuery+'D'+ CAST(@i  AS NVARCHAR) + '.DisplayValue not in (select Exclude FROM ReportGraphRowExclude WHERE ReportGraphId=' +cast(@ReportGraphId as nvarchar)+')'

				/* Start - Added by Arpita Soni for ticket #511 on 10/30/2015 */
				IF(ISNULL(@RestrictedDimensionValues,'') != '')
					SET @ExcludeQuery = @ExcludeQuery + ' AND ' + 'D'+ CAST(@i  AS NVARCHAR) + '.DisplayValue NOT IN (''' + ISNULL(@RestrictedDimensionValues,'') + ''')'
				/* End - Added by Arpita Soni for ticket #511 on 11/02/2015 */

				SET @QueryToRun = @QueryToRun + ' INNER JOIN DimensionValue D' + CAST(@i AS NVARCHAR) + ' ON D' + CAST(@i AS NVARCHAR) + '.id = D2.D' + CAST(@DimensionIndex AS NVARCHAR) 
																+ ' AND D'+ CAST(@i  AS NVARCHAR) +'.DimensionId = ' + CAST(@Dimensionid AS NVARCHAR) 

				IF(@IsDateDimension = 1)
				BEGIN
					SET @QueryToRun = @QueryToRun + ' and CAST(d' + CAST(@i  AS NVARCHAR) + '.DisplayValue AS DATE) between '''+cast(@STARTDATE as nvarchar)+''' and '''+cast(@ENDDATE as nvarchar)+''''
					IF(@RateList != '' AND @RateList IS NOT NULL)
					BEGIN
						SET @QueryToRun = @QueryToRun + ' INNER JOIN '+@RateList+' rt ON  CAST(d' + CAST(@i  AS NVARCHAR) + '.DisplayValue AS DATE) between CAST(StartDate AS DATE) and CAST(EndDate AS DATE)'
					END
					SET @OrderbyCount=(select count(id) from ReportAxis where ReportGraphId=@ReportGraphId and AxisName='X'and Dimensionid=@Dimensionid );
					SET @DimensionCount=(select count(id) from ReportAxis where ReportGraphId=@ReportGraphId );
					--Following code is written to identify order by value
					if(@OrderbyCount=0 and @DimensionCount>1)
						SET @OrderByCount=2
					else
						SET @OrderByCount=1
				END	   
				SET @RestrictedDimensionValues= ''
			END
			SET @i = @i + 1
			FETCH NEXT FROM Column_Cursor
			INTO @Dimensionid,@IsDateDimension, @DimensionIndex
		END
		Close Column_Cursor
		Deallocate Column_Cursor
		--Filters
		DECLARE @FilterCondition NVARCHAR(MAX);
		SET @FilterCondition = ''
		IF(@FilterValues IS NOT NULL AND @IsDimension=0)
		BEGIN
			--Insertation Start 28/07/2016 Kausha Moved function code here and following code will be executed when this function will caled for measure
			--SELECT @FilterCondition = ' ' +  ISNULL(KeyValue,'') FROM [dbo].[ExtractValueFromXML](@FilterValues,'D2',2);
			DECLARE @FilterString TABLE (KeyValue NVARCHAR(MAX))
			DECLARE @TableAlias NVARCHAR(MAX)='D2'
			DECLARE @IsGraph int=2
			Declare @XmlString XML=@FilterValues
			DECLARE @temp_Table TABLE (ID NVARCHAR(10) Index Ix1 clustered,Value NVARCHAR(1000))

			INSERT INTO @temp_Table
			SELECT data.col.value('(@ID)[1]', 'int'),data.col.value('(.)', 'INT') FROM @XmlString.nodes('(/filters/filter)') AS data(col);

			--Based on the Id, combines value comma saperated
			WITH ExtractValue AS(
				SELECT ID, Value = STUFF((
					SELECT ', ' + Value
					FROM @temp_Table b
					WHERE b.ID = a.ID
					FOR XML PATH('')
					), 1, 2, '')
				FROM @temp_Table a
				GROUP BY ID
			),
			_AllFilters AS(
			SELECT 1 Id,  
				Condition = CASE 
					WHEN @IsGraph= 0 THEN  'AND '+ @TableAlias +'.D' + ID + ' IN (' + Value + ')' 
					WHEN @IsGraph= 2 THEN  ' '+ @TableAlias +'.D' + ID + ' IN (' + Value + ')' 
					ELSE ' left outer join DimensionValue d' + CONVERT(NVARCHAR(20),(ID + 4)) + ' on ' +  @TableAlias + '.Id = d' + CONVERT(NVARCHAR(20),(ID + 4)) + '.id and d' + CONVERT(NVARCHAR(20),(ID + 4)) + '.id in (' + Value  +')'
				END 
			FROM ExtractValue
			)
			INSERT @FilterString (KeyValue)
			SELECT DISTINCT 
				STUFF	(
					(SELECT CASE WHEN @IsGraph = 2 THEN '#' ELSE ',' END + B.Condition FROM _AllFilters B WHERE B.Id = A.Id FOR XML PATH(''))
					,1,1,''	) Condition
					FROM
					_AllFilters A;
					--Insertation End  kausha 27/07/2016 Write function code here for extract xml to reduce time
			SELECT  @FilterCondition= ' ' +  ISNULL(KeyValue,'') FROM @FilterString	

		END
		SET @FilterCondition = ISNULL(@FilterCondition,'')
		IF(@FilterCondition != '')
		BEGIN
			SET @FilterCondition=' where'+@FilterCondition
			SET @FilterCondition =  REPLACE(@FilterCondition,'#',' AND ')
		END

		IF(@FilterCondition is null and @ExcludeQuery is not null)
			SET @ExcludeQuery=' where '+@ExcludeQuery
		--Exclude
		--Deletion Start: <17/02/2016> <Kausha> <Ticket #729,#730> - <Due to performance issue no need to pass filter and exclude in both dimension and measure table>
		--set @QueryToRun=@QueryToRun+'  '+@FilterCondition+@ExcludeQuery
		--Deletion End: <17/02/2016> <Kausha> <Ticket #729,#730>

		--Insertion Start: <17/02/2016> <Kausha> <Ticket #729,#730> - <Due to performance issue we have removed filter from dimension table and exculd from measure table>
		IF(@IsDimension=1)
			set @QueryToRun=@QueryToRun+'  '+@ExcludeQuery
		ELSE
			set @QueryToRun=@QueryToRun+'  '+@FilterCondition
		--Insertion End: <17/02/2016> <Kausha> <Ticket #729,#730>

		IF(@IsDimension=1)
			set @QueryToRun=@QueryToRun+' order by ordervalue'+CAST(@OrderByCount as nvarchar)

	END
RETURN @QueryToRun 
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE  object_id = OBJECT_ID(N'GetMeasuresForMeasureTable') AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
  DROP FUNCTION [dbo].[GetMeasuresForMeasureTable]
GO

CREATE FUNCTION [dbo].[GetMeasuresForMeasureTable] 
(   
	@ReportGraphID int,
    @MeasureId int,
	@RateList NVARCHAR(50) = ''
) 
RETURNS  nvarchar(1000)
BEGIN
declare @MeasureName nvarchar(100)
declare @AggrigationType nvarchar(100)
declare @Column nvarchar(max)
declare @DimensionTableMeasure nvarchar(max)
declare @SymbolType nvarchar(max)
declare @Count int =1
     

  set @Column=''
			set @Column=',SUM(d1.Rows) as Rows'
 DECLARE @MeasureCursor CURSOR
			SET @MeasureCursor = CURSOR FAST_FORWARD FOR SELECT Top 1 m.name,m.AggregationType,rc.SymbolType from reportgraphcolumn rc inner join measure m on    rc.reportgraphid=@ReportGraphId and m.id=@MeasureId
			OPEN @MeasureCursor
			FETCH NEXT FROM @MeasureCursor
			INTO @MeasureName,@AggrigationType,@SymbolType
			WHILE @@FETCH_STATUS = 0 
			BEGIN
			if(@AggrigationType='AVG')
			BEGIN
			IF(@SymbolType = '%'  )
			BEGIN
			set @Column=@Column+','+' ISNULL(ROUND((sum(D1.Value*d1.rows)/sum(d1.rows))*100,2),0) AS  ['+@MeasureName+']'			
			END
			ELSE
			BEGIN
				IF(@RateList = '' OR @RateList IS NULL)
				BEGIN
					set @Column=@Column+','+ 'ISNULL(ROUND(sum(D1.Value*d1.rows)/sum(d1.rows),2),0) AS ['+@MeasureName+']'
				END
				ELSE
				BEGIN
					SET @Column=@Column+','+ 'ISNULL(ROUND((sum(D1.Value*d1.rows)/sum(d1.rows))*avg(ISNULL(rt.Rate,0)),2),0) AS ['+@MeasureName+']'
				END
			END
			END
			ELSE
				IF(@RateList = '' OR @RateList IS NULL)
				BEGIN
					set @Column=@Column+','+'ISNULL(ROUND(SUM(d1.Value),2),0) as ['+@MeasureName+']'
				END
				ELSE
				BEGIN
					set @Column=@Column+','+'ISNULL(ROUND(SUM(d1.Value)* avg(ISNULL(rt.Rate,0)),2),0) as ['+@MeasureName+']'
				END
			--Add Rows
			set @Count=@count+1
			
			FETCH NEXT FROM @MeasureCursor
			INTO @MeasureName,@AggrigationType,@SymbolType
			END
			CLOSE @MeasureCursor
			DEALLOCATE @MeasureCursor
			return @Column
END
GO

IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'ReportGraphResultsNew') AND xtype IN (N'P'))
    DROP PROCEDURE ReportGraphResultsNew
GO


CREATE PROCEDURE [ReportGraphResultsNew]
@ReportGraphID INT, 
@DIMENSIONTABLENAME NVARCHAR(100), 
@STARTDATE date='1-1-1900', 
@ENDDATE date='1-1-2100', 
@DATEFIELD NVARCHAR(100)=null, 
@FilterValues NVARCHAR(MAX)=null,
@ViewByValue NVARCHAR(15),
@SubDashboardOtherDimensionTable INT = 0,
@SubDashboardMainDimensionTable INT = 0,
@DisplayStatSignificance NVARCHAR(15) = NULL,
@UserId UNIQUEIDENTIFIER = '14D7D588-CF4D-46BE-B4ED-A74063B67D66',
@RoleId UNIQUEIDENTIFIER = '504F5E26-2208-44C2-A78F-4BDF4BAB703F',
@Rate PreferredCurrenctDetails READONLY

AS
BEGIN
SET NOCOUNT ON;
DECLARE @DateDimensionID int

 --Dimension Table 
SET @DateDimensionId=(SELECT TOP 1 Dimension FROM dbo.fnSplitString(@DimensionTableName,'D') d1 INNER JOIN dimension d ON d1.dimension=d.id AND d.IsDateDimension=1)

--For issue #609
if @FilterValues='''<filters></filters>'''
BEGIN
	SET @FilterValues=null
END

Declare @CustomQuery NVARCHAR(MAX)
SET @CustomQuery=(SELECT ISNULL(CustomQuery,'') FROM ReportGraph where Id=@ReportGraphID)
IF(@CustomQuery!='')
BEGIN
EXEC CustomGraphQuery @ReportGraphID,@STARTDATE,@ENDDATE,@FilterValues,@ViewByValue,@DIMENSIONTABLENAME,@DateDimensionID,0,NULL,NULL,NULL,NULL,NULL,1,@Rate
END
ELSE


BEGIN
SET @DisplayStatSignificance=ISNULL(@DisplayStatSignificance,'')
--to do please add report id to all the table variables 
DECLARE @Dimensions NVARCHAR(MAX), @TempString NVARCHAR(MAX), @Count INT, @Measures NVARCHAR(1000), @GroupBy NVARCHAR(1000), @FirstTable NVARCHAR(MAX), @SymbolType NVARCHAR(50) ,@ConfidenceLevel FLOAT
DECLARE @SecondTable NVARCHAR(MAX)  DECLARE @MeasureCreateTableDimension NVARCHAR(MAX) DECLARE @MeasureSelectTableDimension NVARCHAR(1000), @IsDateOnYaixs BIT=0, @DimensionCount INT=0
DECLARE @UpdateTableCondition NVARCHAR(500),  @DimensionName1 NVARCHAR(50), @DimensionName2 NVARCHAR(50), @Measure NVARCHAR(50), @DimensionId1 INT, @DimensionId2 INT, @IsDateDimension BIT=0 ,@IsOnlyDateDimension BIT=0
DECLARE @FinalTable NVARCHAR(MAX), @MeasureCount INT DECLARE @SQL NVARCHAR(MAX), @Columns NVARCHAR(1000) DECLARE @BaseQuery NVARCHAR(MAX), @GType NVARCHAR(100)
DECLARE @RateList NVARCHAR(50) = ''

SET @FirstTable='';  SET @SymbolType=''; SET @Columns='' ; SET @MeasureCreateTableDimension='';SET @MeasureSelectTableDimension='';SET @UpdateTableCondition='';SET @BaseQuery='';SET @SecondTable=''
SET @SQL=''; SET @DimensionName1=''; SET @DimensionName2=''; SET @Measure=''; SET @GroupBy='';SET @FinalTable='';SET @TempString=''
if @FilterValues='<filters></filters>'
BEGIN
SET @FilterValues=null
END	
		
SELECT @GType =LOWER (GraphType),@ConfidenceLevel=ISNULL(ConfidenceLevel,0) FROM ReportGraph WHERE id = @ReportGraphID

 --Dimension Table 
SET @DateDimensionId=(SELECT TOP 1 Dimension FROM dbo.fnSplitString(@DimensionTableName,'D') d1 INNER JOIN dimension d ON d1.dimension=d.id AND d.IsDateDimension=1)

SELECT 
		@Dimensions						= selectdimension, 
		@Columns						= CreateTableColumn,
		@MeasureCreateTableDimension	= MeasureTableColumn, 
		@MeasureSelectTableDimension	= MeasureSelectColumn, 
		@UpdateTableCondition			= UpdateCondition,
		@GroupBy						= GroupBy,
		@DimensionCount					= CAST(Totaldimensioncount AS INT), 
		@DimensionName1					= DimensionName1, 
		@DimensionName2					= DimensionName2, 
		@DimensionId1					= CAST(DimensionId1 AS INT), 
		@DimensionId2					= CAST(DimensionId2 AS INT),
		@IsDateDimension				= CAST(IsDateDImensionExist AS BIT), 
		@IsDateOnYaixs					= CAST(IsDateOnYAxis AS BIT), 
		@IsOnlyDateDimension			= CAST(IsOnlyDateDImensionExist AS BIT) 
FROM dbo.GetDimensions(@ReportGraphID,@ViewByValue,@DIMENSIONTABLENAME,@STARTDATE,@ENDDATE,@GType,@SubDashboardOtherDimensionTable,@SubDashboardMainDimensionTable,@DisplayStatSignificance)

--Measure Table
 SELECT 
		@Measures	= SelectTableColumn, 
		@Columns	= @Columns + CreateTableColumn, 
		@Measure	= MeasureName,
		@SymbolType	= SymbolType 
FROM dbo.GetMeasures(@ReportGraphID,@ViewByValue,@DimensionCount,@GTYPe,@DisplayStatSignificance)


IF EXISTS (SELECT * FROM @Rate)
BEGIN
	IF OBJECT_ID('tempdb..#RateListMonthWise') IS NOT NULL
	BEGIN
		DROP TABLE #RateListMonthWise
	END
	CREATE TABLE #RateListMonthWise (StartDate DATE, EndDate DATE, Rate FLOAT)
	INSERT INTO #RateListMonthWise 
	SELECT CAST(StartDate AS DATE), CAST(EndDate AS DATE), Rate FROM @Rate
	
	IF EXISTS (SELECT * FROM Measure m 
				INNER JOIN ReportGraphColumn rgc ON (rgc.MeasureId = m.id AND rgc.ReportGraphId = @ReportGraphID) 
				WHERE ISNULL(m.IsCurrency, 0) = 1)
	BEGIN
		SET @RateList = '#RateListMonthWise'
	END
END

--1 means creating base query for dimension - only order by cluase will be added
SET @BaseQuery = dbo.DimensionBaseQuery(@DIMENSIONTABLENAME, @STARTDATE, @ENDDATE, @ReportGraphID, @FilterValues,@IsOnlyDateDimension, 1,@UserId,@RoleId,@RateList)
--print @basequery
--
SET @SQL = REPLACE(@BaseQuery,'#COLUMNS#',@Dimensions + @Measures)
--print @Columns
DECLARE @Table NVARCHAR(1000)
SET @Table = ''
SET @Table = 'DECLARE @DimensionTable'+CAST(@ReportGraphID AS NVARCHAR)+' TABLE('
SET @Table = @Table+' '+ @Columns + ')'
SET @FirstTable = @Table + 'INSERT INTO @DimensionTable'+CAST(@ReportGraphID AS NVARCHAR)+' ' + @SQL + ''; --insert in to dimension table

SET @BaseQuery= (dbo.DimensionBaseQuery(@DIMENSIONTABLENAME,@STARTDATE,@ENDDATE,@ReportGraphID,@FilterValues,@IsOnlyDateDimension,0,@UserId,@RoleId,@RateList))
--print @basequery
DECLARE @MeasureId INT, @AggregationType NVARCHAR(50), @MeasureName NVARCHAR(100)
SET @Table=' DECLARE @MeasureTable'+cast(@ReportGraphID AS NVARCHAR)+' table(' + ' ' + @MeasureCreateTableDimension + ',Rows float, Measure float'+')'

SET @SecondTable = @SecondTable + @Table

--print @MeasureSelectTableDimension + @Measures

DECLARE @MeasureCursor CURSOR
SET @MeasureCursor = CURSOR FAST_FORWARD FOR SELECT MeasureId,Measurename,AggregationType  FROM dbo.GetGraphMeasure(@ReportGraphID,@ViewByValue)
OPEN @MeasureCursor
FETCH NEXT FROM @MeasureCursor
INTO @MeasureId,@MeasureName,@AggregationType
WHILE @@FETCH_STATUS = 0 
BEGIN
  IF(@DimensionCount>1 AND LOWER(@GType)!='errorbar' AND LOWER(@GType)!='columnrange' AND LOWER(@GType)!='bullet' AND  LOWER(@DisplayStatSignificance)!='rate')
   SET @MeasureName='Measure_'+@MeasureName

	IF EXISTS(SELECT * FROM Measure WHERE id = @MeasureId AND ISNULL(IsCurrency, 0) = 1)
	BEGIN
		SET @Measures = dbo.GetMeasuresForMeasureTable(@ReportGraphID,@MeasureId,@RateList)
	END
	ELSE
	BEGIN
		SET @Measures = dbo.GetMeasuresForMeasureTable(@ReportGraphID,@MeasureId,'')
	END
	IF(@IsOnlyDateDImension=1 AND @FilterValues IS NULL )
	SET @Measures=REPLACE(@Measures,'rows','RecordCount')
	SET @SQL = REPLACE(@BaseQuery,'#COLUMNS#', @MeasureSelectTableDimension + @Measures )
	--SET SQL

	--In base query if there are row excluded or filter is passed as parameter then there is already where cluase so we have to check condition
	IF CHARINDEX('where',LOWER(@SQL)) <= 0
		SET @SQL=@SQL+' WHERE '
	ELSE
		SET @SQL=@SQL+' AND '
	
	SET @SQL = @SQL + ' D1.Measure=' + CAST(@MeasureId AS NVARCHAR) + ' GROUP BY ' + @GroupBy
			
	SET @SecondTable=@SecondTable+' INSERT INTO @MeasureTable'+cast(@ReportGraphID AS NVARCHAR)+' '+@SQL
	
		IF(LOWER(@GType)='errorbar' OR LOWER(@DisplayStatSignificance)='rate')	
		BEGIN
			DECLARE @ErrorbarQuery NVARCHAR(2000)='', @UpdateErrorbarQuery NVARCHAR(2000)=' ', @PopulationRate FLOAT, @NoOfTails FLOAT=2
			
			IF( @Confidencelevel < 0 OR @Confidencelevel > 1)
				SET @ConfidenceLevel=0

		IF(@DimensionCount = 1)
			BEGIN
			IF(LOWER(@AggregationType) = 'avg')
			BEGIN
			SET @PopulationRate=(SELECT ROUND((SUM(value*RecordCount)/SUM(RecordCount)),2) FROM MeasureValue WHERE DimensionValue in (SELECT id FROM DimensionValue where DimensionID in(select DIMENSIONID FROM reportaxis where ReportGraphId=@ReportGraphID ))and measure = @MeasureId)
				
			END
			ELSE
			BEGIN
				SET @PopulationRate=(SELECT ROUND(AVG(value),2) FROM MeasureValue WHERE DimensionValue IN (SELECT id FROM DimensionValue where DimensionID IN(SELECT DIMENSIONID FROM reportaxis where ReportGraphId=@ReportGraphID ))and measure = @MeasureId)
			END
		
			IF(@PopulationRate>1 OR @PopulationRate<0)
				SET @PopulationRate = 1;
				SET @ErrorbarQuery =' Update @DimensionTable'+cast(@ReportGraphID AS NVARCHAR)+' SET PopulationRate='+CAST(@PopulationRate AS NVARCHAR)
				SET @SecondTable = @SecondTable + @ErrorbarQuery + ';'
			END

		ELSE -- @DimensionCount = 2
			BEGIN
				DECLARE @ExistDimensionTableName NVARCHAR(100), @Pr_DimensionId INT, @SeriesDimension INT, @SeriesDimensionIndex INT, @YaxisIndex INT
				DECLARE @SeriesDimensionName NVARCHAR(500), @DateAdded bit =0, @IsDateDimensionOnYAxis INT, @Pr_IsDateDimensionExist INT
			
				SELECT @Pr_IsDateDimensionExist = COUNT(IsDateDimension) FROM Dimension WHERE Id IN (SELECT DimensionId FROM ReportAxis WHERE ReportGraphId = @ReportGraphID) and IsDateDimension=1;
				SET @SeriesDimension=(select dimensionid FROM ReportAxis WHERE  ReportGraphId = @ReportGraphID and axisname='Y')
				
				SET @SeriesDimensionName=(select columnname FROM Dimension where id=@SeriesDimension)
			
				SET @YaxisIndex=2
				SELECT @IsDateDimensionOnYAxis = COUNT(IsDateDimension) FROM Dimension WHERE Id IN (SELECT DimensionId FROM ReportAxis WHERE ReportGraphId = @ReportGraphID and axisname='Y') and IsDateDimension=1;
			
				DECLARE @DimensionCursor CURSOR
				SET @DimensionCursor = CURSOR FAST_FORWARD FOR SELECT dimensionid FROM   reportaxis where reportgraphid=@ReportGraphID order by dimensionid 
				OPEN @DimensionCursor
				FETCH NEXT FROM @DimensionCursor
				INTO @Pr_DimensionId
				WHILE @@FETCH_STATUS = 0
					BEGIN

						IF(@Pr_IsDateDimensionExist=0 and @DateDimensionId < @Pr_DimensionId and @DateAdded=0)
						BEGIN
							SET @ExistDimensionTableName=concat(@ExistDimensionTableName,'D'+CAST(@DateDimensionId as NVARCHAR)) 
							SET @DateAdded=1
						END

					SET @ExistDimensionTableName=concat(@ExistDimensionTableName,'D'+CAST(@Pr_DimensionId as NVARCHAR)) 
				FETCH NEXT FROM @DimensionCursor
				INTO @Pr_DimensionId
				END
				CLOSE @DimensionCursor
				DEALLOCATE @DimensionCursor	
					SET @SeriesDimensionIndex=1
				IF(@DateAdded=1)
				SET @SeriesDimensionIndex= (select cnt FROM dbo.fnSplitString(@DIMENSIONTABLENAME,'d') where Dimension=CAST(@SeriesDimension as NVARCHAR))
				ELSE
				IF((select Dimensionid from reportaxis where ReportGraphId=@ReportGraphID and UPPER(axisname)='Y')>(select Dimensionid from reportaxis where ReportGraphId=@ReportGraphID and UPPER(axisname)='X'))
				SET @SeriesDimensionIndex=2
				DECLARE @DisplayValue NVARCHAR(500), @Dimension NVARCHAR(50), @Populationratestring NVARCHAR(1000)
			
				IF(@YaxisIndex=1)
					SET @Dimension = @DimensionName1
				ELSE
					SET @Dimension = @DimensionName2
					SET @DisplayValue='DisplayValue'
					IF(@IsDateDimensionOnYAxis>0)
						SET @DisplayValue=' dimensionid='+CAST(@DateDimensionId as NVARCHAR)+' and CAST(dbo.GetDatePart(CAST(DisplayValue AS DATE),'''+@ViewByValue+''','''+CAST(@STARTDATE as NVARCHAR)+''','''+CAST(@Enddate as NVARCHAR)+''') as NVARCHAR) '
							IF(LOWER(@AggregationType)='avg')
							BEGIN
						
							SET @PopulationRateString='((select (SUM(Value*Rows)/SUM(rows)) FROM '+CAST(@ExistDimensionTableName as NVARCHAR)+'Value where '+CAST(@ExistDimensionTableName as NVARCHAR)+' in (select id FROM '+CAST(@ExistDimensionTableName as NVARCHAR)+' where d'+CAST(@SeriesDimensionIndex as NVARCHAR)+' in (select Id FROM Dimensionvalue where '+@DisplayValue+' = '+@Dimension+')) and Measure='+CAST(@MeasureId as NVARCHAR)+' ))'
						
							END
							ELSE
							BEGIN
							SET @PopulationRateString='((select AVG(value) FROM '+CAST(@ExistDimensionTableName as NVARCHAR)+'Value where '+CAST(@ExistDimensionTableName as NVARCHAR)+' in (select id FROM '+CAST(@ExistDimensionTableName as NVARCHAR)+' where d'+CAST(@SeriesDimensionIndex as NVARCHAR)+' in (select Id FROM Dimensionvalue where '+@DisplayValue+' = '+@Dimension+')) and Measure='+CAST(@MeasureId as NVARCHAR)+' ))'
								
							
							END
							SET @ErrorbarQuery=' UPDATE @DimensionTable'+CAST(@ReportGraphID AS NVARCHAR)+' SET PopulationRate='+@PopulationRateString
							SET @SecondTable=@SEcondTable+@ErrorbarQuery+';'
		END -- Error graph @DimensionCount End
		IF(LOWER(@DisplayStatSignificance)='rate')
		BEGIN
		IF(@SymbolType='%' )
			BEGIN
				SET @UpdateErrorbarQuery=',[LowerLimit-'+CAST(@ConfidenceLevel*100 AS NVARCHAR)+'%]=CAST(ROUND((2*CAST(PopulationRate as float)-(select dbo.ComputeErrorBar(PopulationRate,M.rows,'+CAST(@ConfidenceLevel AS NVARCHAR)+','+CAST(@NoOfTails AS NVARCHAR)+'))/M.rows)*100,2) as NVARCHAR)'+',[UpperLimit-'+CAST(@ConfidenceLevel*100 AS NVARCHAR)+'%]=CAST(ROUND((select dbo.ComputeErrorBar(PopulationRate,M.rows,'+CAST(@ConfidenceLevel AS NVARCHAR)+','+CAST(@NoOfTails AS NVARCHAR)+')/M.rows)*100,2) as NVARCHAR)'
			END
		ELSE
			BEGIN
				SET @UpdateErrorbarQuery=',[LowerLimit-'+CAST(@ConfidenceLevel*100 AS NVARCHAR)+'%]=CAST(ROUND((2*CAST(PopulationRate as float)-(select dbo.ComputeErrorBar(PopulationRate,M.rows,'+CAST(@ConfidenceLevel AS NVARCHAR)+','+CAST(@NoOfTails AS NVARCHAR)+'))/M.rows),2) as NVARCHAR)'+',[UpperLimit-'+CAST(@ConfidenceLevel*100 AS NVARCHAR)+'%]=CAST(ROUND((select dbo.ComputeErrorBar(PopulationRate,M.rows,'+CAST(@ConfidenceLevel AS NVARCHAR)+','+CAST(@NoOfTails AS NVARCHAR)+')/M.rows),2) as NVARCHAR)'
			END
		END
		ELSE IF(LOWER(@GType)='errorbar')
		BEGIN
	    IF(@SymbolType='%' )
			BEGIN
				SET @UpdateErrorbarQuery=',LowerLimit=CAST(ROUND((2*CAST(PopulationRate as float)-(select dbo.ComputeErrorBar(PopulationRate,M.rows,'+CAST(@ConfidenceLevel AS NVARCHAR)+','+CAST(@NoOfTails AS NVARCHAR)+'))/M.rows)*100,2) as NVARCHAR)'+',UpperLimit=CAST(ROUND((select dbo.ComputeErrorBar(PopulationRate,M.rows,'+CAST(@ConfidenceLevel AS NVARCHAR)+','+CAST(@NoOfTails AS NVARCHAR)+')/M.rows)*100,2) as NVARCHAR)'
			END
			ELSE
			BEGIN
				SET @UpdateErrorbarQuery=',LowerLimit=CAST(ROUND((2*CAST(PopulationRate as float)-(select dbo.ComputeErrorBar(PopulationRate,M.rows,'+CAST(@ConfidenceLevel AS NVARCHAR)+','+CAST(@NoOfTails AS NVARCHAR)+'))/M.rows),2) as NVARCHAR)'+',UpperLimit=CAST(ROUND((select dbo.ComputeErrorBar(PopulationRate,M.rows,'+CAST(@ConfidenceLevel AS NVARCHAR)+','+CAST(@NoOfTails AS NVARCHAR)+')/M.rows),2) as NVARCHAR)'
			END

		END -- Error graph complete
		END

		 IF(@gtype!='errorbar' AND LOWER(@DisplayStatSignificance)!='rate')
				SET @UpdateErrorBarQuery=' '
				SET @SecondTable=@SecondTable+' UPDATE @dimensionTable'+CAST(@ReportGraphID AS NVARCHAR)+' SET Rows=M.Rows,['+@MeasureName+']=M.[Measure]'+@UpdateErrorBarQuery+' FROM @dimensionTable'+cast(@ReportGraphID AS NVARCHAR)+' D INNER JOIN @MeasureTable'+CAST(@ReportGraphID AS NVARCHAR)+' M ON '+@UpdateTableCondition+' delete FROM @Measuretable'+CAST(@ReportGraphID as NVARCHAR)
			 
FETCH NEXT FROM @MeasureCursor
INTO @MeasureId,@MeasureName,@AggregationType
END
CLOSE @MeasureCursor
DEALLOCATE @MeasureCursor
DECLARE @SelectTable NVARCHAR(MAX)


IF(@DimensionCount>1 AND LOWER(@GType)!='errorbar' AND LOWER(@GType)!='columnrange' AND LOWER(@GType)!='bullet' AND LOWER(@DisplayStatSignificance)!='rate')
BEGIN

	DECLARE @DimensionValues NVARCHAR(MAX)
	IF(@IsDateOnYaixs=0)
		SELECT @DimensionValues = COALESCE(@DimensionValues + ', ' ,'') + '[' + DisplayValue + ']'  FROM DimensionValue WHERE DimensionID = @DimensionId2 AND DisplayValue NOT IN (SELECT Exclude FROM ReportGraphRowExclude WHERE ReportGraphID = @ReportGraphID) ORDER BY OrderValue
	ELSE
	BEGIN
			IF(@ViewByValue = 'Q') 
			BEGIN
				SELECT @DimensionValues = COALESCE(@DimensionValues + ', ' ,'') + DisplayValue FROM 
				(SELECT '[' + 'Q' + CAST(DATEPART(Q,CAST(DisplayValue AS DATE)) AS NVARCHAR) + '-' + CAST(DATEPART(YY,CAST(DisplayValue AS DATE)) AS NVARCHAR)  + ']' DisplayValue ,MIN(OrderValue) OrderValue  FROM DimensionValue WHERE DimensionID = @DimensionId2 AND DisplayValue NOT IN (SELECT Exclude FROM ReportGraphRowExclude WHERE ReportGraphID = @ReportGraphID) GROUP BY '[' + 'Q' + CAST(DATEPART(Q,CAST(DisplayValue AS DATE)) AS NVARCHAR) + '-' + CAST(DATEPART(YY,CAST(DisplayValue AS DATE)) AS NVARCHAR)  + ']') A
				ORDER BY OrderValue
			END
			ELSE IF(@ViewByValue = 'Y') 
			BEGIN
				SELECT @DimensionValues = COALESCE(@DimensionValues + ', ' ,'') + DisplayValue FROM 
				(SELECT '[' + CAST(DATEPART(YY,CAST(DisplayValue AS DATE)) AS NVARCHAR)  + ']' DisplayValue ,MIN(OrderValue) OrderValue  FROM DimensionValue WHERE DimensionID = @DimensionId2 AND DisplayValue NOT IN (SELECT Exclude FROM ReportGraphRowExclude WHERE ReportGraphID = @ReportGraphID) GROUP BY '[' + CAST(DATEPART(YY,CAST(DisplayValue AS DATE)) AS NVARCHAR)  + ']') A
				ORDER BY OrderValue
			END
			ELSE IF(@ViewByValue = 'M') 
			BEGIN
				SELECT @DimensionValues = COALESCE(@DimensionValues + ', ' ,'') + DisplayValue FROM 
				(SELECT '[' + SUBSTRING(DateName(MONTH,CAST(DisplayValue AS DATE)),0,4) + '-' + CAST(DATEPART(YY,CAST(DisplayValue AS DATE)) AS NVARCHAR) + ']' DisplayValue ,MIN(OrderValue) OrderValue  FROM DimensionValue WHERE DimensionID = @DimensionId2 AND DisplayValue NOT IN (SELECT Exclude FROM ReportGraphRowExclude WHERE ReportGraphID = @ReportGraphID) GROUP BY '[' + SUBSTRING(DateName(MONTH,CAST(DisplayValue AS DATE)),0,4) + '-' + CAST(DATEPART(YY,CAST(DisplayValue AS DATE)) AS NVARCHAR) + ']') A
				ORDER BY OrderValue
			END
			ELSE IF(@ViewByValue='W')
			BEGIN
				IF(YEAR(@STARTDATE)=YEAR(@ENDDATE))
				BEGIN
					SELECT @DimensionValues = COALESCE(@DimensionValues + ', ' ,'') + DisplayValue FROM 
					(SELECT '[' + LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(DisplayValue AS NVARCHAR)) - 6, CAST(DisplayValue AS NVARCHAR)) AS NVARCHAR),3) + ' ' + CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(DisplayValue AS NVARCHAR)) - 6, CAST(DisplayValue AS NVARCHAR)))) + ']' DisplayValue ,MIN(OrderValue) OrderValue  FROM DimensionValue WHERE DimensionID = @DimensionId2 AND DisplayValue NOT IN (SELECT Exclude FROM ReportGraphRowExclude WHERE ReportGraphID = @ReportGraphID) GROUP BY '[' + LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(DisplayValue AS NVARCHAR)) - 6, CAST(DisplayValue AS NVARCHAR)) AS NVARCHAR),3) + ' ' + CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(DisplayValue AS NVARCHAR)) - 6, CAST(DisplayValue AS NVARCHAR)))) + ']') A
					ORDER BY OrderValue
				END
				ELSE 
				BEGIN
					SELECT @DimensionValues = COALESCE(@DimensionValues + ', ' ,'') + DisplayValue FROM 
					(SELECT '[' + LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(DisplayValue AS NVARCHAR)) - 6, CAST(DisplayValue AS NVARCHAR)) AS NVARCHAR),3) + ' '+ CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(DisplayValue AS NVARCHAR)) - 6, CAST(DisplayValue AS NVARCHAR)))) + '-' + CONVERT(NVARCHAR,YEAR(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(DisplayValue AS NVARCHAR)) - 6, CAST(DisplayValue AS NVARCHAR)) AS NVARCHAR))) + ']' DisplayValue ,MIN(OrderValue) OrderValue  FROM DimensionValue WHERE DimensionID = @DimensionId2 AND DisplayValue NOT IN (SELECT Exclude FROM ReportGraphRowExclude WHERE ReportGraphID = @ReportGraphID) GROUP BY '[' + LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(DisplayValue AS NVARCHAR)) - 6, CAST(DisplayValue AS NVARCHAR)) AS NVARCHAR),3) + ' '+ CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(DisplayValue AS NVARCHAR)) - 6, CAST(DisplayValue AS NVARCHAR)))) + '-' + CONVERT(NVARCHAR,YEAR(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(DisplayValue AS NVARCHAR)) - 6, CAST(DisplayValue AS NVARCHAR)) AS NVARCHAR))) + ']') A
					ORDER BY OrderValue
				END
			END
			ELSE IF(@ViewByValue='FQ' OR @ViewByValue='FY' OR @ViewByValue='FM')
			BEGIN
				SELECT @DimensionValues = COALESCE(@DimensionValues + ', ' ,'') + DisplayValue FROM 
				(SELECT '[' + [dbo].[CalculateFiscalQuarterYear](@ViewByValue,CAST(DisplayValue AS DATETIME)) + ']' DisplayValue ,MIN(OrderValue) OrderValue  FROM DimensionValue WHERE DimensionID = @DimensionId2 AND DisplayValue NOT IN (SELECT Exclude FROM ReportGraphRowExclude WHERE ReportGraphID = @ReportGraphID) GROUP BY '[' + [dbo].[CalculateFiscalQuarterYear](@ViewByValue,CAST(DisplayValue AS DATETIME)) + ']') A
				ORDER BY OrderValue
			END
	END
	
	--SET @SelectTable='; SELECT '+ @DimensionName1 + ',' + @DimensionValues + ' FROM ( ' +
	SET @SelectTable='; SELECT * FROM ( ' +
	'SELECT ' + @DimensionName1 + ',' + @DimensionName2 + ',[' + @Measure + '],MAX(OrderValue1) OVER (PARTITION BY '+ @DimensionName1 +') OrderValue FROM @DimensionTable'+cast(@ReportGraphID AS NVARCHAR)+' ' + 
	') P PIVOT ('+
	'MAX(['+ @Measure +']) FOR ' + @DimensionName2 + ' IN ('+ @DimensionValues +')'+
	') AS PVT ORDER BY OrderValue' 
	END
	ELSE
	BEGIN
		DECLARE @OrderBy NVARCHAR(500) 
		IF(@DimensionCount>1)
			SET @OrderBy=' ORDER BY OrderValue1,OrderValue2'
		ELSE
			SET @OrderBy=' ORDER BY OrderValue1  '
		SET @SelectTable=';SELECT * FROM @DimensionTable'+CAST(@ReportGraphID AS NVARCHAR)+' '+@OrderBy
	END
	
	
	print 	@FirstTable
	print @SecondTable
	print @finaltable
	print @SelectTable
	EXEC(@FirstTable+@SecondTable+@finaltable+@SelectTable )
END

END
GO

IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'WebApiGetReportRawData') AND xtype IN (N'P'))
    DROP PROCEDURE WebApiGetReportRawData
GO


CREATE PROCEDURE [WebApiGetReportRawData]
(
	@Id INT, @TopOnly BIT = 1, @ViewBy NVARCHAR(2) = 'Q',@StartDate DATETIME= '1/1/1900', @EndDate DATETIME = '1/1/2100', @FilterValues NVARCHAR(MAX) = NULL, @Rate PreferredCurrenctDetails READONLY
)
AS 
BEGIN
	SET NOCOUNT ON;
	--Identify if it is a custom query or not
	DECLARE @CustomQuery NVARCHAR(MAX),@DrillDownCustomQuery NVARCHAR(MAX),@DrillDownXFilter NVARCHAR(MAX),@CustomFilter NVARCHAR(MAX), @GT NVARCHAR(100)
	SELECT TOP 1 
			@GT						= ISNULL(G.GraphType,''),
			@CustomQuery			= ISNULL(G.CustomQuery,''),
			@DrillDownCustomQuery	= ISNULL(G.DrillDownCustomQuery,''),
			@DrillDownXFilter		= ISNULL(G.DrillDownXFilter,''),
			@CustomFilter			= ISNULL(G.CustomFilter,'')
	FROM ReportGraph G 
		LEFT JOIN ReportAxis A			ON G.id = A.ReportGraphId 
		WHERE G.Id = @Id

	IF(@GT != 'bar' AND @GT != 'column' AND @GT != 'pie' AND @GT != 'donut' AND @GT != 'line' AND @GT != 'stackbar' AND @GT != 'stackcol' AND @GT != 'area' AND @GT != 'bubble' AND @GT != 'scatter' AND @GT != 'columnrange' AND @GT != 'negativearea' AND @GT != 'negativebar' AND @GT != 'negativecol' AND @GT != 'solidgauge' AND @GT != 'gauge')
	BEGIN
			SET @GT = 'Currently we are not supporting graph type "'+ @GT +'"  in Measure Report Web API'
			RAISERROR(@GT,16,1) 
	END
	ELSE
	BEGIN
		--Identify if there is only date dimension is configured and will return with the chart attribute
		DECLARE @IsDateDimensionOnly BIT = 0
		IF(@CustomQuery != '') --Need to get if date dimension is configured on x-axis or not, this can be happen only for 2 dimension configured
		BEGIN
			SET @IsDateDimensionOnly = 
				CASE WHEN (SELECT COUNT(*) FROM ReportAxis WHERE ReportGraphId = @Id ) = 1 THEN
						(SELECT COUNT(*) FROM ReportAxis  A
						INNER JOIN Dimension D ON D.id = A.Dimensionid AND D.IsDateDimension = 1 AND D.IsDeleted = 0
						WHERE ReportGraphId  = @Id)
				ELSE 0 
			END
		END

		--Need to get if date dimension is configured on x-axis or not, this can be happen only for 2 dimension configured
		DECLARE @DateOnX BIT = 0
		IF(@CustomQuery = '') 
		BEGIN
			SET @DateOnX = 
				CASE WHEN (SELECT COUNT(*) FROM ReportAxis WHERE ReportGraphId = @Id ) > 1 THEN
						(SELECT COUNT(*) FROM ReportAxis  A
						INNER JOIN Dimension D ON D.id = A.Dimensionid AND D.IsDateDimension = 1 AND D.IsDeleted = 0
						WHERE ReportGraphId  = @Id AND AxisNAme = 'X')
				ELSE 0 
			END
		END
	
		DECLARE @ColumnNames NVARCHAR(MAX) 
		SELECT @ColumnNames = COALESCE(@ColumnNames + ', ', '') +  AttributeKey FROM (SELECT DISTINCT  '[' + AttributeKey + ']' AttributeKey FROM ChartOptionAttribute)  A
		IF(@ColumnNames IS NULL OR @ColumnNames ='')
			SET @ColumnNames = 'X'

		DECLARE @Query NVARCHAR(MAX);
		SET @Query = '
		;WITH ReportAttribute AS (
		SELECT 
				Id,
				GraphType				=	ISNULL(GraphType,''''),
				IsLableDisplay			=	ISNULL(IsLableDisplay,0),
				IsLegendVisible			= 	ISNULL(IsLegendVisible,0),
				LegendPosition			=	ISNULL(LegendPosition,''right,middle,y''),
				IsDataLabelVisible		= 	ISNULL(IsDataLabelVisible,0),
				DataLablePosition		=	ISNULL(DataLablePosition,''''),
				DefaultRows				=	ISNULL(DefaultRows,10),
				ChartAttribute			=	ISNULL(ChartAttribute,''''),
				ConfidenceLevel			=	ConfidenceLevel,
				CustomQuery				=	ISNULL(CustomQuery,''''),
				IsSortByValue			=	ISNULL(IsSortByValue,0),
				SortOrder				=	ISNULL(SortOrder,''asc''),
				DrillDownCustomQuery	=	ISNULL(DrillDownCustomQuery,''''),
				DrillDownXFilter		=	ISNULL(DrillDownXFilter,''''),
				CustomFilter			=	ISNULL(CustomFilter,''''),
				TotalDecimalPlaces		=	(SELECT TOP 1 CASE WHEN ISNULL(TotalDecimalPlaces,-1) = -1 THEN ISNULL(G.TotalDecimalPlaces,-1) ELSE TotalDecimalPlaces END FROM ReportGraphColumn WHERE ReportGraphId = ' + CAST(@Id AS NVARCHAR) + '),
				MagnitudeValue			=	(SELECT TOP 1 CASE WHEN ISNULL(MagnitudeValue,'''') = '''' THEN ISNULL(G.MagnitudeValue,'''') ELSE MagnitudeValue END  FROM ReportGraphColumn WHERE ReportGraphId = ' + CAST(@Id AS NVARCHAR) + '),
				DimensionCount			=   CASE WHEN ISNULL(CustomQuery,'''') = ''''
												THEN (SELECT COUNT(*) FROM ReportAxis A WHERE A.ReportGraphId = ' + CAST(@Id AS NVARCHAR) + ')
											ELSE 
												CASE WHEN CHARINDEX(''#DIMENSIONGROUP#'',CustomQuery) <= 0
													THEN 1
													ELSE 2
												END
											END,
				SymbolType = (SELECT TOP 1 ISNULL(SymbolType,'''') FROM ReportGraphColumn WHERE ReportGraphId = ' + CAST(@Id AS NVARCHAR) + '),
				IsDateDimensionOnly		=   '+ CAST(@IsDateDimensionOnly AS NVARCHAR) +',
				DateOnX					=   '+ CAST(@DateOnX AS NVARCHAR) +'
				FROM ReportGraph G
				WHERE G.Id = ' + CAST(@Id AS NVARCHAR) + '
		),
		ExtendedAttribute AS
		(
	
			SELECT * FROM (
						SELECT  C1.ReportGraphId ReportGraphId1,C1.AttributeKey,C1.AttributeValue,C2.AttributeValue ColorSequenceNo FROM ChartOptionAttribute C1
						LEFT JOIN ChartOptionAttribute C2 ON C1.ReportGraphId = C2.ReportGraphId AND  C2.AttributeKey = ''ColorSequenceNumber''
						WHERE C1.ReportGraphId = ' + CAST(@Id AS NVARCHAR) + '
				) AS R
				PIVOT 
				(
					MIN(AttributeValue)
					FOR AttributeKey IN ( '+  @ColumnNames + ')
				) AS A
				
		)
		SELECT *,ChartColor = dbo.GetColor(E.ColorSequenceNo) FROM ReportAttribute R
		LEFT JOIN ExtendedAttribute E ON R.id = E.ReportGraphId1
		'
	
		--This dynamic query will returns all the attributes of chart
	
		EXEC(@Query)

	
		DECLARE @DateDimensionId INT;
		DECLARE @DimensionName VARCHAR(MAX);
		DECLARE @FilterXML NVARCHAR(MAX) = NULL
		DECLARE @FilterXMLString NVARCHAR(MAX) = NULL
		DECLARE @DimList NVARCHAR(MAX) = NULL
		DECLARE @ColDimLst NVARCHAR(MAX)
			IF(@CustomQuery != '') --In case of custom query is configured for the report
			BEGIN
			
				SELECT TOP 1 @DateDimensionId = DateD.id FROM ReportAxis A 
					INNER JOIN Dimension D ON D.id = A.Dimensionid AND D.IsDeleted = 0
					INNER JOIN Dimension DateD ON D.TableName = DateD.TableName and DateD.IsDateDimension = 1 AND DateD.IsDeleted = 0
				WHERE A.ReportGraphId = @Id

				IF(@GT = 'columnrange')
				BEGIN
					IF (@FilterValues IS NOT NULL AND @FilterValues != '')
					BEGIN
						SELECT @ColDimLst = COALESCE(@ColDimLst, '') + 'D' + CAST(DimensionId AS NVARCHAR) FROM ReportAxis where ReportGraphId = @Id
						SELECT @DimList = COALESCE(@DimList, '') + 'D' + LEFT(dimension, CHARINDEX(':',dimension)-1) FROM [dbo].[fnSplitString](@FilterValues, ',')
						SET @DimList = CONCAT(@DimList, 'D', CAST(@ColDimLst AS NVARCHAR(MAX)))
						SELECT @DimensionName = COALESCE(@DimensionName, '') + 'D' + CAST(DimensionId AS NVARCHAR(MAX)) FROM dbo.[IdentifyDimensions](@Id,1,@DimList)
					END
					ELSE
					BEGIN
						SELECT @DimensionName = COALESCE(@DimensionName, '') + 'D' + CAST(DimensionId AS NVARCHAR) FROM ReportAxis where ReportGraphId = @Id order by DimensionId asc
					END

					IF (@FilterValues IS NOT NULL AND @FilterValues != '' AND @DimensionName IS NOT NULL AND @DimensionName != '')
					BEGIN
						SET @FilterXMLString = (SELECT [dbo].[GetFilterXmlString] (@FilterValues, @DimensionName))
					END	
				END
				ELSE
				BEGIN
					IF (@FilterValues IS NOT NULL AND @FilterValues != '')
					BEGIN
						SELECT @DimList = COALESCE(@DimList, '') + 'D' + LEFT(dimension, CHARINDEX(':',dimension)-1) FROM [dbo].[fnSplitString](@FilterVAlues, ',')
						SET @DimList = CONCAT(@DimList, 'D', CAST(@DateDimensionId AS NVARCHAR(MAX)))
						SELECT @DimensionName = COALESCE(@DimensionName, '') + 'D' + CAST(DimensionId AS NVARCHAR) FROM dbo.[IdentifyDimensions](@Id,1,@DimList)
					END
					ELSE
					BEGIN
						SELECT @DimensionName = COALESCE(@DimensionName, '') + 'D' + CAST(DimensionId AS NVARCHAR) FROM dbo.[IdentifyDimensions](@Id,1,@DateDimensionId)
					END

					IF (@FilterValues IS NOT NULL AND @FilterValues != '' AND @DimensionName IS NOT NULL AND @DimensionName != '')
					BEGIN
						SET @FilterXMLString = (SELECT [dbo].[GetFilterXmlString] (@FilterValues, @DimensionName))
					END
				END

				IF (@FilterXMLString IS NOT NULL AND @FilterXMLString != '')
				BEGIN
					SELECT @FilterXML = CONCAT('''<filters>', @FilterXMLString, '</filters>''')
				END
			
				IF((CHARINDEX('#DIMENSIONGROUP#',@CustomQuery) > 0 OR CHARINDEX('#DIMENSIONWHERE#',@CustomQuery) > 0) AND ISNULL(@DateDimensionId,0) != 0 )  -- We must have one dimension (date) configured for the report
				BEGIN
					EXEC [CustomGraphQuery]  
						@ReportGraphID			= @Id, 
						@STARTDATE				= @StartDate, 
						@ENDDATE				= @EndDate,
						@FilterValues			= @FilterXML,
						@ViewByValue			= @ViewBy,
						@DimensionTableName		= '',
						@DateDimensionId		= @DateDimensionId,--this value must be pass , other wise CustomGraphQuery will throw an error
						@IsDrillDownData		= 0,
						@DrillRowValue			= NULL,
						@SortBy					= NULL,
						@SortDirection			= NULL,
						@PageSize				= NULL,
						@PageIndex				= NULL,
						@IsExportAll			= 0,
						@Rate                   = @Rate
				END
				ELSE IF(CHARINDEX('#DIMENSIONWHERE#',@CustomQuery) <= 0)
				BEGIN
					EXEC [CustomGraphQuery]  
						@ReportGraphID			= @Id, 
						@STARTDATE				= @StartDate, 
						@ENDDATE				= @EndDate,
						@FilterValues			= @FilterXML,
						@ViewByValue			= @ViewBy,
						@DimensionTableName		= '',
						@DateDimensionId		= '', --this value is not passed here
						@IsDrillDownData		= 0,
						@DrillRowValue			= NULL,
						@SortBy					= NULL,
						@SortDirection			= NULL,
						@PageSize				= NULL,
						@PageIndex				= NULL,
						@IsExportAll			= 0,
						@Rate                   = @Rate
				END
				ELSE 
				BEGIN
						RAISERROR('Date Dimension is not configured for Report ',16,1) 
				END
			END
			ELSE --In case of custom query is not configured for the report, but Dimension and Measure are configured
			BEGIN
					SELECT TOP 1 @DateDimensionId = DateD.id FROM ReportAxis A 
						INNER JOIN Dimension D ON D.id = A.Dimensionid AND D.IsDeleted = 0
						INNER JOIN Dimension DateD ON D.TableName = DateD.TableName and DateD.IsDateDimension = 1 AND DateD.IsDeleted = 0
					WHERE A.ReportGraphId = @Id

					IF(ISNULL(@DateDimensionId,0) != 0 )  -- We must have one dimension (date) configured for the report
					BEGIN
						IF(@GT = 'columnrange')
						BEGIN
							IF (@FilterValues IS NOT NULL AND @FilterValues != '')
							BEGIN
								SELECT @ColDimLst = COALESCE(@ColDimLst, '') + 'D' + CAST(DimensionId AS NVARCHAR) FROM ReportAxis where ReportGraphId = @Id
								SELECT @DimList = COALESCE(@DimList, '') + 'D' + LEFT(dimension, CHARINDEX(':',dimension)-1) FROM [dbo].[fnSplitString](@FilterVAlues, ',')
								SET @DimList = CONCAT(@DimList, 'D', CAST(@ColDimLst AS NVARCHAR(MAX)))
								SELECT @DimensionName = COALESCE(@DimensionName, '') + 'D' + CAST(DimensionId AS NVARCHAR(MAX)) FROM dbo.[IdentifyDimensions](@Id,1,@DimList)
							END
							ELSE
							BEGIN
								SELECT @DimensionName = COALESCE(@DimensionName, '') + 'D' + CAST(DimensionId AS NVARCHAR) FROM ReportAxis where ReportGraphId = @Id order by DimensionId asc
							END

							IF (@FilterValues IS NOT NULL AND @FilterValues != '' AND @DimensionName IS NOT NULL AND @DimensionName != '')
							BEGIN
								SET @FilterXMLString = (SELECT [dbo].[GetFilterXmlString] (@FilterValues, @DimensionName))
							END	
						END
						ELSE
						BEGIN
							IF (@FilterValues IS NOT NULL AND @FilterValues != '')
							BEGIN
								SELECT @DimList = COALESCE(@DimList, '') + 'D' + LEFT(dimension, CHARINDEX(':',dimension)-1) FROM [dbo].[fnSplitString](@FilterVAlues, ',')
								SET @DimList = CONCAT(@DimList, 'D', CAST(@DateDimensionId AS NVARCHAR(MAX)))
								SELECT @DimensionName = COALESCE(@DimensionName, '') + 'D' + CAST(DimensionId AS NVARCHAR(MAX)) FROM dbo.[IdentifyDimensions](@Id,1,@DimList)
							END
							ELSE
							BEGIN
								SELECT @DimensionName = COALESCE(@DimensionName, '') + 'D' + CAST(DimensionId AS NVARCHAR(MAX)) FROM dbo.[IdentifyDimensions](@Id,1,@DateDimensionId)
							END

							IF (@FilterValues IS NOT NULL AND @FilterValues != '' AND @DimensionName IS NOT NULL AND @DimensionName != '')
							BEGIN
								SET @FilterXMLString = (SELECT [dbo].[GetFilterXmlString] (@FilterValues, @DimensionName))
							END						
						END

						IF (@FilterXMLString IS NOT NULL AND @FilterXMLString != '')
						BEGIN
							SELECT @FilterXML = CONCAT('''<filters>', @FilterXMLString, '</filters>''')
						END
						
						EXEC [ReportGraphResultsNew]
							@ReportGraphID						= @Id, 
							@DIMENSIONTABLENAME					= @DimensionName, 
							@STARTDATE							= @StartDate, 
							@ENDDATE							= @EndDate, 
							@DATEFIELD							= @DateDimensionId, 
							@FilterValues						= @FilterXML,
							@ViewByValue						= @ViewBy,
							@SubDashboardOtherDimensionTable	= 0,
							@SubDashboardMainDimensionTable		= 0,
							@DisplayStatSignificance			= NULL,
							@UserId								= NULL,
							@RoleId								= NULL,
							@Rate                               = @Rate
					END
					ELSE
					BEGIN
							RAISERROR('Date Dimension is not configured for Report ',16,1) 
					END
			END

			--If chart type is Gauge than returns Measure Output values

			IF (@GT='gauge')
			BEGIN
					SELECT 
						LowerLimit,
						UpperLimit,
						Value 
					FROM MeasureOutputValue MOV
					INNER JOIN ReportGraphColumn RGC 
					ON MOV.MeasureId=RGC.MeasureId
					WHERE RGC.ReportGraphId=@Id

			END
		END
END
GO


IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'CustomGraphQuery') AND xtype IN (N'P'))
    DROP PROCEDURE CustomGraphQuery
GO

CREATE PROCEDURE [CustomGraphQuery]
@ReportGraphID INT, 
@STARTDATE date='1-1-1900', 
@ENDDATE date='1-1-2100',
@FilterValues NVARCHAR(MAX)=null,
@ViewByValue NVARCHAR(15),
@DimensionTableName NVARCHAR(100),
@DateDimensionId int,
@IsDrillDownData bit=0,
@DrillRowValue NVARCHAR(1000) = NULL,
@SortBy NVARCHAR(1000) = NULL,
@SortDirection NVARCHAR(1000) = NULL,
@PageSize INT = NULL,
@PageIndex INT = NULL,
@IsExportAll BIT=1,
@Rate PreferredCurrenctDetails READONLY
AS
BEGIN
	DECLARE @DIMENSIONGROUP NVARCHAR(MAX)
	DECLARE @DimensionIndex NVARCHAR(50)='d1.DimensionValue'
	DECLARE @DimensionId Nvarchar(20)=''
	DECLARE @WhereCondition NVARCHAR(2000)=' '
	DECLARE @DimensionValues NVARCHAR(MAX)
	DECLARE @Query NVARCHAR(MAX), @DrillDownCustomQuery NVARCHAR(MAX),@DrillDownXFilter NVARCHAR(MAX), @CustomFilter NVARCHAR(MAX)

	IF OBJECT_ID('tempdb..#RateListMonthWise') IS NOT NULL
	BEGIN
		DROP TABLE #RateListMonthWise
	END
	CREATE TABLE #RateListMonthWise (StartDate DATE, EndDate DATE, Rate FLOAT)
	IF EXISTS (SELECT * FROM @Rate)
	BEGIN
		INSERT INTO #RateListMonthWise 
		SELECT CAST(StartDate AS DATE), CAST(EndDate AS DATE), Rate FROM @Rate
	END
	
	SET @DIMENSIONGROUP=' '
	SET @DimensionId=CAST(@DateDimensionId as nvarchar)
	
	Select @Query=CustomQuery, @DrillDownCustomQuery=DrillDownCustomQuery, @DrillDownXFilter=DrillDownXFilter,@CustomFilter=CustomFilter from ReportGraph where Id=@ReportGraphID
	IF NOT EXISTS (SELECT * FROM @Rate)
	BEGIN
		SET @QUERY=REPLACE(LOWER(@Query),'rt.Rate','1')
	END
	SET @QUERY=REPLACE(@Query,'#STARTDATE#','''' +CAST(@STARTDATE AS NVARCHAR)+'''')
	SET @QUERY=REPLACE(@Query,'#ENDDATE#','''' +CAST(@ENDDATE AS NVARCHAR)+'''')

	IF(@IsDrillDownData = 0)
	BEGIN	
		SET @DIMENSIONGROUP = 'dbo.GetDatePart(CAST(CAST(DV.Value AS INT) AS DATETIME),''' + @ViewByValue+''',''' +CAST(@STARTDATE AS NVARCHAR)+''','''+CAST(@ENDDATE AS NVARCHAR)+''') ,DV.OrderValue '
	END
	ELSE
	BEGIN
		SET @DIMENSIONGROUP = 'dbo.GetDatePart(CAST(CAST(DV.Value AS INT) AS DATETIME),''' + @ViewByValue+''',''' +CAST(@STARTDATE AS NVARCHAR)+''','''+CAST(@ENDDATE AS NVARCHAR)+''') AS Column1,DV.OrderValue '
	END
	IF(@FilterValues IS NULL)
	BEGIN
	--Insertation Start Kuahsha This date filter will not affact to attribution query
	   IF CHARINDEX('getattributiontouchdata',LOWER(@Query)) <= 0
	   --Insertation End Kuahsha This date filter will not affact to attribution query
		SET @WhereCondition = ' WHERE D1.DimensionValue' + CAST(@DimensionId AS NVARCHAR) + ' IN (SELECT ID FROM DimensionValue WHERE CAST(CAST(DV.Value AS INT) AS DATETIME) BETWEEN ''' + CAST(@STARTDATE AS NVARCHAR) +''' AND ''' + CAST(@EndDate AS NVARCHAR) + ''')'
	END
	ELSE 
	BEGIN
	--Insertation Start Kuahsha This date filter will not affact to attribution query
	IF CHARINDEX('getattributiontouchdata',@Query) <= 0
	--Insertation end Kuahsha This date filter will not affact to attribution query
	   SET @WhereCondition = ' WHERE D1.DimensionValue' + CAST(@DimensionId AS NVARCHAR) + ' IN (SELECT ID FROM DimensionValue WHERE CAST(CAST(DV.Value AS INT) AS DATETIME) BETWEEN ''' + CAST(@STARTDATE AS NVARCHAR) +''' AND ''' + CAST(@EndDate AS NVARCHAR) + ''')'
	END
	Declare @DimensionValueField NVArchar(500)='DimensionValue'+@dimensionID
	IF(@ViewByValue = 'Q') 
	BEGIN
		SELECT @DimensionValues = COALESCE(@DimensionValues + ', ' ,'') + DisplayValue FROM 
		(SELECT '[' + 'Q' + CAST(DATEPART(Q,CAST(CAst(Value AS int) AS DATETIME)) AS NVARCHAR) + '-' + CAST(DATEPART(YY,CAST(CAST(Value AS INT) AS DATETIME)) AS NVARCHAR)  + ']' DisplayValue ,MIN(OrderValue) OrderValue 
			FROM DimensionValue  WHERE dimensionid=@DimensionId ANd CAST(CAst(Value AS int) AS DATETIME) Between @StartDate and @EndDate
			GROUP BY '[' + 'Q' + CAST(DATEPART(Q,CAST(CAST(Value AS Int) AS DATETIME)) AS NVARCHAR) + '-' + CAST(DATEPART(YY,CAST(CAST(Value AS Int) AS DATETIME)) AS NVARCHAR)  + ']') A
		ORDER BY OrderValue
	END
			
	ELSE IF(@ViewByValue = 'Y') 
	BEGIN
		SELECT @DimensionValues = COALESCE(@DimensionValues + ', ' ,'') + DisplayValue FROM 
		(SELECT '[' + CAST(DATEPART(YY,CAST(CAST(value AS INt) AS DATETIME)) AS NVARCHAR)  + ']' DisplayValue ,MIN(Ordervalue) OrderValue 
			FROM DimensionValue  WHERE dimensionid=@DimensionId ANd CAST(CAst(Value AS int) AS DATETIME) Between @StartDate and @EndDate
			GROUP BY '[' + CAST(DATEPART(YY,CAST(CAST(value AS INT) AS DATETIME)) AS NVARCHAR)  + ']') A
		ORDER BY OrderValue
	END
	ELSE IF(@ViewByValue = 'M') 
	BEGIN
		SELECT @DimensionValues = COALESCE(@DimensionValues + ', ' ,'') + DisplayValue FROM 
		(SELECT '[' + SUBSTRING(DateName(MONTH,CAST(CAST(VALUE AS INT) AS DATETIME)),0,4)
			+ '-' + CAST(DATEPART(YY,CAST(CAST(VALUE AS INT) AS DATETIME)) AS NVARCHAR) + ']' DisplayValue ,
		MIN(ORDERVALUE) OrderValue  
			FROM DimensionValue  WHERE dimensionid=@DimensionId ANd CAST(CAst(Value AS int) AS DATETIME) Between @StartDate and @EndDate
		GROUP BY '[' + SUBSTRING(DateName(MONTH,CAST(CAST(VALUE AS int) AS DATETIME)),0,4) + '-' + CAST(DATEPART(YY,CAST(CAST(CAST(VALUE AS INT) AS INT) AS DATETIME)) AS NVARCHAR) + ']') A
		ORDER BY OrderValue
	END
	ELSE IF(@ViewByValue='W')
	BEGIN
		IF(YEAR(@STARTDATE)=YEAR(@ENDDATE))
		BEGIN
			SELECT @DimensionValues = COALESCE(@DimensionValues + ', ' ,'') + DisplayValue FROM 
			(SELECT '[' + LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(CAST(VALUE AS INT) AS DATETIME)) - 6,
				CAST(CAST(Value AS INT) AS DATETIME)) AS NVARCHAR),3) + ' ' +
				CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(CAST(VALUE AS INT) AS DATETIME)) - 6, 
				CAST(CAST(VALUE AS INT) AS DATETIME)))) + ']' 
				DisplayValue ,MIN(ORDERVALUE) OrderValue 
				FROM DimensionValue  WHERE dimensionid=@DimensionId ANd CAST(CAst(Value AS int) AS DATETIME) Between @StartDate and @EndDate
				GROUP BY '[' + LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(CAST(VALUE AS INT) AS DATETIME)) - 6,
				CAST(CAST(VALUE AS INT) AS DATETIME)) AS NVARCHAR),3) + ' ' +
				CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(CAST(VALUE AS INT) AS DATETIME)) - 6,
					CAST(CAST(VALUE AS INT) AS DATETIME)))) + ']') A
			ORDER BY OrderValue
		END
		ELSE 
		BEGIN
			SELECT @DimensionValues = COALESCE(@DimensionValues + ', ' ,'') + DisplayValue FROM 
			(SELECT '[' + LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(CAST(Value AS INT) AS DATETIME)) - 6, CAST(CAST(Value AS INT) AS DATETIME)) AS NVARCHAR),3) 
			+ ' '+ CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(CAST(Value AS INT) AS DATETIME)) - 6, CAST(CAST(Value AS INT) AS DATETIME)))) + '-' 
			+ CONVERT(NVARCHAR,YEAR(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(CAST(Value AS INT) AS DATETIME)) - 6, CAST(CAST(Value AS INT) AS DATETIME)) AS NVARCHAR))) + ']'
				DisplayValue ,MIN(Id) OrderValue  
				FROM DimensionValue  WHERE dimensionid=@DimensionId ANd CAST(CAst(Value AS int) AS DATETIME) Between @StartDate and @EndDate
				GROUP BY '[' + LEFT(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(CAST(Value AS INT) AS DATETIME)) - 6, CAST(CAST(Value AS INT) AS DATETIME)) AS NVARCHAR),3) 
			+ ' '+ CONVERT(NVARCHAR,DATEPART(DD,DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(CAST(Value AS INT) AS DATETIME)) - 6, CAST(CAST(Value AS INT) AS DATETIME)))) + '-' 
			+ CONVERT(NVARCHAR,YEAR(CAST(DATEADD(dd, @@DATEFIRST - DATEPART(dw, CAST(CAST(Value AS INT) AS DATETIME)) - 6, CAST(CAST(Value AS INT) AS DATETIME)) AS NVARCHAR))) + ']') A
			ORDER BY OrderValue
		END
	END
	ELSE IF(@ViewByValue='FQ' OR @ViewByValue='FY' OR @ViewByValue='FM')
	BEGIN
		SELECT @DimensionValues = COALESCE(@DimensionValues + ', ' ,'') + DisplayValue FROM 
		(SELECT '[' + [dbo].[CalculateFiscalQuarterYear](@ViewByValue,CAST(CAST(VALUE AS INT) AS DATETIME)) + ']' DisplayValue ,
			MIN(OrderValue) OrderValue
			FROM DimensionValue  WHERE dimensionid=@DimensionId ANd CAST(CAst(Value AS int) AS DATETIME) Between @StartDate and @EndDate
			GROUP BY '[' + [dbo].[CalculateFiscalQuarterYear](@ViewByValue,CAST(CAST(VALUE AS INT) AS DATETIME)) + ']') A
		ORDER BY OrderValue
	END
	/* Modified by Arpita Soni for ticket #604 on 11/19/2015 */
	IF(@FilterValues IS NOT NULL AND (select IsICustomFilterApply from ReportGraph where id=@ReportGraphId)=1 )
	BEGIN
		DECLARE @XmlString XML
		SET @XmlString = @FilterValues;
  		DECLARE @TempValueTable TABLE (
						ID NVARCHAR(MAX),
						DimensionValueID NVARCHAR(MAX)
					)
		DECLARE @FilterValueTable TABLE (
						ID NVARCHAR(MAX),
						DimensionValueID NVARCHAR(MAX)
					)

 
		;WITH MyData AS (
			SELECT data.col.value('(@ID)[1]', 'int') Number
				,data.col.value('(.)', 'INT') DimensionValueId
			FROM @XmlString.nodes('(/filters/filter)') AS data(col)
		)
		Insert into @TempValueTable 
		SELECT 
			 D.id,
			 DV.Id
		FROM MyData 
		INNER JOIN DimensionValue DV ON DV.Id = MyData.DimensionValueId
		INNER JOIN Dimension D ON D.Id = DV.DimensionId
		--select * from @TempValueTable

		INSERT INTO @FilterValueTable SELECT  ID
			   ,'AND DimensionValue'+Id+' IN('+STUFF((SELECT ',' + CAST(DimensionValueId+'' AS VARCHAR(MAX)) [text()]
				 FROM @TempValueTable 
				 WHERE ID = t.ID
				 FOR XML PATH(''), TYPE)
				.value('.','NVARCHAR(MAX)'),1,1,' ')+')' List_Output
		FROM @TempValueTable t
		GROUP BY ID
		--select * from @FilterValueTable
		--1 step
		DECLARE @DelimitedString NVARCHAR(MAX)
		DECLARE @DimensionValueTemp NVARCHAR(MAX)
		DECLARE @DimensionIdTemp NVARCHAR(MAX)
		SET @DelimitedString =@CustomFilter
		DECLARE @FinalFilterString NVARCHAR(MAX)=' '
		DECLARE @FilterCursor CURSOR
		SET @FilterCursor = CURSOR FAST_FORWARD FOR SELECT id,dimensionvalueid from @FilterValueTable
		OPEN @FilterCursor
		FETCH NEXT FROM @FilterCursor
		INTO @DimensionIdTemp,@DimensionValueTemp
		WHILE @@FETCH_STATUS = 0
		BEGIN
		Declare @Flage INT=0
		Declare @Count INT=0
		Declare @Id NVARCHAR(1000)
		SET @FinalFilterString= @FinalFilterString+ ' '+ @DimensionValueTemp
		FETCH NEXT FROM @FilterCursor
		INTO @DimensionIdTemp,@DimensionValueTemp
		END
		CLOSE @FilterCursor
		DEALLOCATE @FilterCursor	
	END
	IF(@FinalFilterString IS NOT NULL)
	BEGIN
		SET @WhereCondition = @WhereCondition + @FinalFilterString
	END

	/* Modified by Arpita Soni for Ticket #669 on 12/29/2015 */
	DECLARE @fromRecord INT
	DECLARE @toRecord INT 
	
	IF(ISNULL(@PageIndex,-1) != -1 AND ISNULL(@PageSize,-1) != -1)
	BEGIN
		SET @fromRecord = (@PageSize * @PageIndex) + 1;
		SET @toRecord = (@PageSize * @PageIndex) + 10;
	END
	IF(ISNULL(@SortBy,'') = '')
	BEGIN
		SET @SortBy = 'SELECT NULL'
	END
	IF CHARINDEX('#DIMENSIONGROUP#',@Query) <= 0
	BEGIN
		IF(@IsDrillDownData = 0)
		BEGIN
			IF NOT EXISTS(SELECT * FROM #RateListMonthWise)
			BEGIN
				SET @QUERY=REPLACE(@Query,'#DIMENSIONWHERE#',' INNER Join  DimensionValue DV ON DV.id = D1.DimensionValue'+@DimensionId+'  AND DV.DimensionID = '+@DimensionID+' '+@WhereCondition)
			END
			ELSE
			BEGIN
			SET @QUERY=REPLACE(@Query,'#DIMENSIONWHERE#',' INNER Join  DimensionValue DV ON DV.id = D1.DimensionValue'+@DimensionId+'  AND DV.DimensionID = '+@DimensionID+' INNER JOIN #RateListMonthWise rt ON  CAST(DV.DisplayValue AS DATE) between CAST(rt.StartDate AS DATE) and CAST(rt.EndDate AS DATE) '+@WhereCondition)
			END
		END
		ELSE 
		BEGIN
			DECLARE @DrillDownWhere NVARCHAR(MAX)
			IF(@IsExportAll = 0)
			BEGIN
			SET @DrillDownWhere = ' AND ' + REPLACE(@DrillDownXFilter,'#XFILTER#',''''+@DrillRowValue+'''') 

			END
			SET @QUERY = REPLACE(@DrillDownCustomQuery,'#DIMENSIONWHERE#',' INNER Join  DimensionValue DV ON DV.id = D1.DimensionValue'+@DimensionId+'  AND DV.DimensionID = '+@DimensionID+' '+@WhereCondition+ISNULL(@DrillDownWhere,''))
			IF(ISNULL(@PageIndex,-1) != -1 AND ISNULL(@PageSize,-1) != -1)
			BEGIN
				SET @Query =' SELECT * FROM (SELECT ROW_NUMBER() OVER(ORDER BY ('+@SortBy+') '+@SortDirection+') as RowNumber,* FROM (' + @Query +
										' ) B ) A WHERE RowNumber BETWEEN '+CAST(@fromRecord AS NVARCHAR)+' AND '+CAST(@toRecord AS NVARCHAR) +
									'; SELECT COUNT(*) FROM (' + @Query +') C;'
			END
		END
		print @Query
		EXEC(@Query)
	END
	ELSE
	BEGIN
		IF(@IsDrillDownData = 0)
		BEGIN
			Declare @Table nvarchar(Max)='DECLARE @TABLE Table(Name NVARCHAR(1000),Measure Nvarchar(1000),Date Nvarchar(1000),OrderValue1 int)'
			DECLARE @SelectTable NVARCHAR(MAx) =' SELECT * FROM @Table'
			SET @SelectTable='; SELECT * FROM ( ' +
			'SELECT ' + 'Name' + ',' + 'Date' + ',[' + 'Measure' + '],MAX(OrderValue1) OVER (PARTITION BY '+ 'Name' +') OrderValue FROM @Table' + 
			') P PIVOT ('+
			'MAX(['+ 'Measure' +']) FOR ' + 'Date' + ' IN ('+ @DimensionValues +')'+
			') AS PVT ORDER BY OrderValue' 
	
			SET @QUERY=REPLACE(@Query,'#DIMENSIONGROUP#',''+@DIMENSIONGROUP)
			IF NOT EXISTS(SELECT * FROM #RateListMonthWise)
			BEGIN
				SET @QUERY=REPLACE(@Query,'#DIMENSIONWHERE#',' INNER JOIN DimensionValue DV ON DV.id = D1.DimensionValue'+@DimensionId+'  AND DV.DimensionID = '+@DimensionID+' '+@WhereCondition)
			END
			ELSE
			BEGIN
				SET @QUERY=REPLACE(@Query,'#DIMENSIONWHERE#',' INNER JOIN DimensionValue DV ON DV.id = D1.DimensionValue'+@DimensionId+'  AND DV.DimensionID = '+@DimensionID+' INNER JOIN #RateListMonthWise rt ON  CAST(DV.DisplayValue AS DATE) between CAST(rt.StartDate AS DATE) and CAST(rt.EndDate AS DATE) '+@WhereCondition)
			END
			EXEC(@Table+'INSERT INTO @TABLE '+@Query + '; '+@SelectTable)
		END
		ELSE 
		BEGIN
			SET @DrillDownWhere = ''
			IF(@IsExportAll = 0)
			BEGIN
				SET @DrillDownWhere = ' AND ' + REPLACE(@DrillDownXFilter,'#XFILTER#',''''+@DrillRowValue+'''') 
			END
			SET @DIMENSIONGROUP = REPLACE(@DIMENSIONGROUP,',DV.ORDERVALUE','')
			SET @DrillDownCustomQuery=REPLACE((@DrillDownCustomQuery),'#DIMENSIONGROUP#',''+@DIMENSIONGROUP)
			SET @DrillDownCustomQuery= REPLACE(@DrillDownCustomQuery,'#DIMENSIONWHERE#',' INNER Join  DimensionValue DV ON DV.id = D1.DimensionValue'+@DimensionId+'  AND DV.DimensionID = '+@DimensionID+' '+@WhereCondition+ISNULL(@DrillDownWhere,''))
			IF(ISNULL(@PageIndex,-1) != -1 AND ISNULL(@PageSize,-1) != -1)
			BEGIN	
				SET @DrillDownCustomQuery ='SELECT * FROM (SELECT ROW_NUMBER() OVER(ORDER BY ('+@SortBy+') '+@SortDirection+') as RowNumber,* FROM ('+ @DrillDownCustomQuery +
										') B ) A WHERE RowNumber BETWEEN '+ CAST(@fromRecord AS NVARCHAR)+' AND '+CAST(@toRecord AS NVARCHAR) +
										'; SELECT COUNT(*) FROM (' + @DrillDownCustomQuery +') C;'
			END
			
			EXEC(@DrillDownCustomQuery)
		END
	END
END
GO


   /* Start - Added by Komal Rawal on 08/16/2016 for Ticket #2468 -Notifications*/
   /*Note : Change variable @Createdby as required */
    Declare @Createdby uniqueidentifier = 'C55D12A8-79EC-4ADE-9E40-E595D7980248'

    IF (EXISTS(SELECT * FROM Notification WHERE NotificationInternalUseOnly = 'PlanIsUpdated'  and IsDeleted = 0 )) 
	Begin
	update Notification set IsDeleted = 1 where NotificationInternalUseOnly = 'PlanIsUpdated'
	End

	IF (EXISTS(SELECT * FROM Notification WHERE NotificationInternalUseOnly = 'ModelIsUpdated'  and IsDeleted = 0 )) 
	Begin
	update Notification set IsDeleted = 1 where NotificationInternalUseOnly = 'ModelIsUpdated'
	End

	IF (NOT EXISTS(SELECT * FROM Notification WHERE NotificationInternalUseOnly = 'TacticIsEdited')) 
	Begin
	 insert into Notification(NotificationInternalUseOnly,Title,[Description],NotificationType,EmailContent,IsDeleted,CreatedDate,CreatedBy,ModifiedDate,ModifiedBy,[Subject]) values('TacticIsEdited','When my tactic is edited',null,'AM','Dear [NameToBeReplaced],<br/><br/>Please note that following tactic has been edited.<br><br><table><tr><td>Tactic Name</td><td>:</td><td>[TacticNameToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Edited by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr><tr><td>URL</td><td>:</td><td>[URL]</td></tr></table><br><br>Thank You,<br>Hive9 Plan Admin',0,GETDATE(),@Createdby,null,null,'Plan :Tactic is edited')
	End

	IF (NOT EXISTS(SELECT * FROM Notification WHERE NotificationInternalUseOnly = 'TacticIsApproved')) 
	Begin
	 insert into Notification(NotificationInternalUseOnly,Title,[Description],NotificationType,EmailContent,IsDeleted,CreatedDate,CreatedBy,ModifiedDate,ModifiedBy,[Subject]) values('TacticIsApproved','When a tactic that I own / collaborate on is approved',null,'AM','Dear [NameToBeReplaced],<br/><br/>Please note that following tactic has been approved.<br><br><table><tr><td>Tactic Name</td><td>:</td><td>[TacticNameToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Approved by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr></table><br><br>Thank You,<br>Hive9 Plan Admin',0,GETDATE(),@Createdby,null,null,'Plan : Tactic is approved')
	End

	IF (NOT EXISTS(SELECT * FROM Notification WHERE NotificationInternalUseOnly = 'CommentAddedToTactic')) 
	Begin
	 insert into Notification(NotificationInternalUseOnly,Title,[Description],NotificationType,EmailContent,IsDeleted,CreatedDate,CreatedBy,ModifiedDate,ModifiedBy,[Subject]) values('CommentAddedToTactic','When a comment is added to a tactic I own / collaborate on',null,'AM','Dear [NameToBeReplaced],<br/><br/>Please note that comment is added to following tactic.<br><br><table><tr><td>Tactic Name</td><td>:</td><td>[TacticNameToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Added by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr></table><br><br>Thank You,<br>Hive9 Plan Admin',0,GETDATE(),@Createdby,null,null,'Plan: Comment added to tactic')
	End

	IF (NOT EXISTS(SELECT * FROM Notification WHERE NotificationInternalUseOnly = 'CampaignIsEdited')) 
	Begin
	 insert into Notification(NotificationInternalUseOnly,Title,[Description],NotificationType,EmailContent,IsDeleted,CreatedDate,CreatedBy,ModifiedDate,ModifiedBy,[Subject]) values('CampaignIsEdited','When my campaign is edited',null,'AM','Dear [NameToBeReplaced],<br/><br/>Please note that following campaign has been edited.<br><br><table><tr><td>Campaign Name</td><td>:</td><td>[CampaignNameToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Edited by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr><tr><td>URL</td><td>:</td><td>[URL]</td></tr></table><br><br>Thank You,<br>Hive9 Plan Admin',0,GETDATE(),@Createdby,null,null,'Plan : Campaign is edited')
	End

	IF (NOT EXISTS(SELECT * FROM Notification WHERE NotificationInternalUseOnly = 'ProgramIsEdited')) 
	Begin
	 insert into Notification(NotificationInternalUseOnly,Title,[Description],NotificationType,EmailContent,IsDeleted,CreatedDate,CreatedBy,ModifiedDate,ModifiedBy,[Subject]) values('ProgramIsEdited','When my program is edited',null,'AM','Dear [NameToBeReplaced],<br/><br/>Please note that following program has been edited.<br><br><table><tr><td>Program Name</td><td>:</td><td>[ProgramToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Edited by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr><tr><td>URL</td><td>:</td><td>[URL]</td></tr></table><br><br>Thank You,<br>Hive9 Plan Admin',0,GETDATE(),@Createdby,null,null,'Plan : program is edited')
	End

	IF (NOT EXISTS(SELECT * FROM Notification WHERE NotificationInternalUseOnly = 'TacticIsSubmitted')) 
	Begin
	 insert into Notification(NotificationInternalUseOnly,Title,[Description],NotificationType,EmailContent,IsDeleted,CreatedDate,CreatedBy,ModifiedDate,ModifiedBy,[Subject]) values('TacticIsSubmitted','When a tactic is submitted for my approval',null,'AM','Dear [NameToBeReplaced],<br/><br/>Please note that following tactic has been submitted for approval.<br><br><table><tr><td>Tactic Name</td><td>:</td><td>[TacticNameToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Submitted by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr><tr><td>URL</td><td>:</td><td>[URL]</td></tr></table><br><br>Thank You,<br>Hive9 Plan Admin',0,GETDATE(),@Createdby,null,null,'Plan: Tactic submitted for approval')
	End

	IF (NOT EXISTS(SELECT * FROM Notification WHERE NotificationInternalUseOnly = 'EntityOwnershipAssigned')) 
	Begin
	 insert into Notification(NotificationInternalUseOnly,Title,[Description],NotificationType,EmailContent,IsDeleted,CreatedDate,CreatedBy,ModifiedDate,ModifiedBy,[Subject]) values('EntityOwnershipAssigned','When I am assigned ownership of an entity',null,'AM','Dear [NameToBeReplaced],<br><br>[ModifierName] has made you the owner of following [EntityName].<br><br><table><tr><td>[EntityName]</td><td>:</td><td>[EntityTitle]</td></tr><tr><td>Plan</td><td>:</td><td>[planname]</td></tr><tr><td>URL</td><td>:</td><td>[URL]</td></tr></table><br>Thank You,<br>Hive9 Plan Admin',0,GETDATE(),@Createdby,null,null,'Plan : Ownership assigned of an entity')
	End
	
    IF (NOT EXISTS(SELECT * FROM Notification WHERE NotificationInternalUseOnly = 'CommentAddedToCampaign')) 
	Begin
	 insert into Notification(NotificationInternalUseOnly,Title,[Description],NotificationType,EmailContent,IsDeleted,CreatedDate,CreatedBy,ModifiedDate,ModifiedBy,[Subject]) values('CommentAddedToCampaign','When a comment is added to a campaign I own / collaborate on',null,'AM','Dear [NameToBeReplaced],<br/><br/>Please note that comment is added to following campaign.<br><br><table><tr><td>Campaign Name</td><td>:</td><td>[CampaignNameToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Added by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr></table><br><br>Thank You,<br>Hive9 Plan Admin',0,GETDATE(),@Createdby,null,null,'Plan: Comment added to campaign')
	End

	  IF (NOT EXISTS(SELECT * FROM Notification WHERE NotificationInternalUseOnly = 'CommentAddedToProgram')) 
	Begin
	 insert into Notification(NotificationInternalUseOnly,Title,[Description],NotificationType,EmailContent,IsDeleted,CreatedDate,CreatedBy,ModifiedDate,ModifiedBy,[Subject]) values('CommentAddedToProgram','When a comment is added to a program I own / collaborate on',null,'AM','Dear [NameToBeReplaced],<br/><br/>Please note that comment is added to following program.<br><br><table><tr><td>Program Name</td><td>:</td><td>[ProgramNameToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Added by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr></table><br><br>Thank You,<br>Hive9 Plan Admin',0,GETDATE(),@Createdby,null,null,'Plan: Comment added to program')
	End

	IF (NOT EXISTS(SELECT * FROM Notification WHERE NotificationInternalUseOnly = 'CampaignIsApproved')) 
	Begin
	 insert into Notification(NotificationInternalUseOnly,Title,[Description],NotificationType,EmailContent,IsDeleted,CreatedDate,CreatedBy,ModifiedDate,ModifiedBy,[Subject]) values('CampaignIsApproved','When a campaign that I own / collaborate on is approved',null,'AM','Dear [NameToBeReplaced],<br/><br/>Please note that following campaign has been approved.<br><br><table><tr><td>Campaign Name</td><td>:</td><td>[CampaignNameToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Approved by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr></table><br><br>Thank You,<br>Hive9 Plan Admin',0,GETDATE(),@Createdby,null,null,'Plan : Campaign is approved')
	End

	IF (NOT EXISTS(SELECT * FROM Notification WHERE NotificationInternalUseOnly = 'ProgramIsApproved')) 
	Begin
	 insert into Notification(NotificationInternalUseOnly,Title,[Description],NotificationType,EmailContent,IsDeleted,CreatedDate,CreatedBy,ModifiedDate,ModifiedBy,[Subject]) values('ProgramIsApproved','When a program that I own / collaborate on is approved',null,'AM','Dear [NameToBeReplaced],<br/><br/>Please note that following program has been approved.<br><br><table><tr><td>Program Name</td><td>:</td><td>[ProgramNameToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Approved by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr></table><br><br>Thank You,<br>Hive9 Plan Admin',0,GETDATE(),@Createdby,null,null,'Plan : Program is approved')
	End
	Go

	
/****** Object:  StoredProcedure [dbo].[SaveLogNoticationdata]    Script Date: 08/12/2016 17:52:34 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SaveLogNoticationdata]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[SaveLogNoticationdata]
GO
/****** Object:  StoredProcedure [dbo].[SaveLogNoticationdata]    Script Date: 08/12/2016 17:52:34 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SaveLogNoticationdata]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[SaveLogNoticationdata] AS' 
END
GO
ALTER PROCEDURE [dbo].[SaveLogNoticationdata]
@action nvarchar(50) = null,
@actionSuffix nvarchar(max) = null,
@componentId int = null,
@componentTitle nvarchar(256) = null,
@description nvarchar(50) = null,
@objectId int = null ,
@parentObjectId int =  null ,
@TableName nvarchar(50) = null,
@Userid uniqueidentifier,
@ClientId uniqueidentifier,
@UserName nvarchar(250) = null,
@RecipientIDs nvarchar(max) = null,
@EntityOwnerID nvarchar(max) = null
AS
BEGIN
Declare @InsertedCount int
Declare @ActivityMessageIds int
Declare @NotificationMessage nvarchar(250)
Declare @TacticIsApproved nvarchar(50) = 'TacticIsApproved'
Declare @ReportShared nvarchar(50) = 'ReportIsShared'
Declare @TacticEdited nvarchar(50) = 'TacticIsEdited'
Declare @CommentAddedToTactic nvarchar(50) = 'CommentAddedToTactic'
Declare @CampaignIsEdited nvarchar(50) = 'CampaignIsEdited'
Declare @ProgramIsEdited nvarchar(50) = 'ProgramIsEdited'
Declare @TacticIsSubmitted nvarchar(50) = 'TacticIsSubmitted'
Declare @CommentAddedToCampaign nvarchar(50) = 'CommentAddedToCampaign'
Declare @CommentAddedToProgram nvarchar(50) = 'CommentAddedToProgram'
Declare @CampaignIsApproved nvarchar(50) = 'CampaignIsApproved'
Declare @ProgramIsApproved nvarchar(50) = 'ProgramIsApproved'
Declare @OwnerChange nvarchar(50) = 'EntityOwnershipAssigned'
Declare @NotificationName nvarchar(50)

IF OBJECT_ID('tempdb..#tempNotificationdata') IS NOT NULL
Drop Table #tempNotificationdata
 
insert into ChangeLog(TableName,ObjectId,ParentObjectId,ComponentId,ComponentTitle,ComponentType,ActionName,ActionSuffix,[TimeStamp],UserId,IsDeleted,ClientId) 
values (@TableName,@objectId,@parentObjectId,@componentId,@componentTitle,@description,@action,@actionSuffix,GETDATE(),@Userid,0,@ClientId)

SELECT @InsertedCount=@@ROWCOUNT
DECLARE @ret int = CASE WHEN @InsertedCount = 0 THEN 0 ELSE 1 END
select @ret

if(@TableName <> 'Model')
BEGIN
	select * into #tempnotificationdata from 
	(select u.userid,u.notificationid,n.notificationinternaluseonly from user_notification as u join notification as n on
	 u.notificationid= n.notificationid where  n.notificationtype = 'AM'  and n.isdeleted = 0 and
	 userid in (SELECT Item From dbo.SplitString(@RecipientIDs,','))) as result

	IF (EXISTS(SELECT * FROM #tempNotificationdata WHERE NotificationInternalUseOnly = @TacticEdited and @action='updated' and (@description ='tactic' or @description ='tactic results'))) 
	Begin
		SET @NotificationMessage = 'Tactic '+ @componentTitle +' has been changed by ' + @UserName
	End

	IF (EXISTS(SELECT * FROM #tempNotificationdata WHERE  NotificationInternalUseOnly = @CampaignIsEdited and @action='updated' and  @description ='campaign' )) 
	Begin
		
	    SET @description = UPPER(LEFT(@description,1))+LOWER(SUBSTRING(@description,2,LEN(@description)))
	    SET @NotificationMessage = @description + ' '+ @componentTitle +' has been changed by ' + @UserName
		
	End

	IF (EXISTS(SELECT * FROM #tempNotificationdata WHERE  NotificationInternalUseOnly = @ProgramIsEdited  and @action='updated' and @description ='program')) 
	Begin
		
	    SET @description = UPPER(LEFT(@description,1))+LOWER(SUBSTRING(@description,2,LEN(@description)))
		SET @NotificationMessage = @description + ' '+ @componentTitle +' has been changed by ' + @UserName
	
	End

	if(@action='updated' and @Userid <> @EntityOwnerID and @NotificationMessage <> '' and (@description ='tactic' or @description ='tactic results' or @description ='campaign' or @description ='program'))
    Begin
			insert into User_Notification_Messages(ComponentName,ComponentId,EntityId,[Description],ActionName,IsRead,UserId,RecipientId,CreatedDate,ClientID)
			values(@TableName,@objectId,@componentId,@NotificationMessage,@action,0,@Userid,@EntityOwnerID,GETDATE(),@ClientId)
	End

	IF (EXISTS(SELECT * FROM #tempNotificationdata WHERE NotificationInternalUseOnly = @ReportShared and  @TableName ='Report' and @action='shared' )) 
	Begin
		select @NotificationMessage = @UserName +' has shared report with you '
		insert into User_Notification_Messages(ComponentName,ComponentId,EntityId,[Description],ActionName,IsRead,UserId,RecipientId,CreatedDate,ClientID)
		SELECT @TableName,@objectId,@componentId,@NotificationMessage,@action,0,@Userid,UserId,GETDATE(),@ClientId FROM #tempNotificationdata
		WHERE NotificationInternalUseOnly = @ReportShared and UserId <> @Userid
	End

	IF (EXISTS(SELECT * FROM #tempNotificationdata WHERE NotificationInternalUseOnly = @CommentAddedToTactic and @description ='tactic' and @action='commentadded' )) 
	Begin
	
	  SET @NotificationMessage =  @UserName +' has added comment to ' + @description + ' ' + @componentTitle 
	  SET  @NotificationName = @CommentAddedToTactic
	
	End

	 IF (EXISTS(SELECT * FROM #tempNotificationdata WHERE  NotificationInternalUseOnly = @CommentAddedToCampaign and @description ='campaign'and @action='commentadded' )) 
	Begin
	
	   SET @NotificationMessage =  @UserName +' has added comment to ' + @description + ' ' + @componentTitle 
       SET	@NotificationName = @CommentAddedToCampaign
	End

	IF (EXISTS(SELECT * FROM #tempNotificationdata WHERE NotificationInternalUseOnly = @CommentAddedToProgram and @description ='program'and @action='commentadded' )) 
	Begin
	
		SET @NotificationMessage =  @UserName +' has added comment to ' + @description + ' ' + @componentTitle 
	    SET  @NotificationName = @CommentAddedToProgram
	End

	if(@action='commentadded' and @NotificationMessage <> '' and (@description ='tactic' or @description ='campaign' or @description ='program'))
	begin
		insert into User_Notification_Messages(ComponentName,ComponentId,EntityId,[Description],ActionName,IsRead,UserId,RecipientId,CreatedDate,ClientID)
		SELECT @TableName,@objectId,@componentId,@NotificationMessage,@action,0,@Userid,UserId,GETDATE(),@ClientId FROM #tempNotificationdata
		WHERE NotificationInternalUseOnly = @NotificationName and UserId <> @Userid
    End

	IF (EXISTS(SELECT * FROM #tempNotificationdata WHERE NotificationInternalUseOnly = @TacticIsApproved and @action='approved' and @description ='tactic' )) 
	Begin
	    SET @description = UPPER(LEFT(@description,1))+LOWER(SUBSTRING(@description,2,LEN(@description)))
		SET @NotificationMessage = @description +' '+ @componentTitle +' has been approved by ' + @UserName
		SET	@NotificationName = @TacticIsApproved
	End

	IF (EXISTS(SELECT * FROM #tempNotificationdata WHERE  NotificationInternalUseOnly = @CampaignIsApproved  and @action='approved' and  @description ='campaign' )) 
	Begin
	    SET @description = UPPER(LEFT(@description,1))+LOWER(SUBSTRING(@description,2,LEN(@description)))
		SET @NotificationMessage = @description +' '+ @componentTitle +' has been approved by ' + @UserName
		SET	@NotificationName = @CampaignIsApproved
	End

	IF (EXISTS(SELECT * FROM #tempNotificationdata WHERE  NotificationInternalUseOnly = @ProgramIsApproved  and @action='approved' and @description ='program')) 
	Begin
	    SET @description = UPPER(LEFT(@description,1))+LOWER(SUBSTRING(@description,2,LEN(@description)))
		SET @NotificationMessage = @description +' '+ @componentTitle +' has been approved by ' + @UserName
		SET	@NotificationName = @ProgramIsApproved
	End

    if(@action='approved' and @NotificationMessage <> '' and (@description ='tactic' or @description ='campaign' or @description ='program'))
	begin
		insert into User_Notification_Messages(ComponentName,ComponentId,EntityId,[Description],ActionName,IsRead,UserId,RecipientId,CreatedDate,ClientID)
		SELECT @TableName,@objectId,@componentId,@NotificationMessage,@action,0,@Userid,UserId,GETDATE(),@ClientId FROM #tempNotificationdata
		WHERE NotificationInternalUseOnly = @NotificationName and UserId <> @Userid
    End

	IF (EXISTS(SELECT * FROM #tempNotificationdata WHERE NotificationInternalUseOnly = @TacticIsSubmitted and @description ='tactic' and @action='submitted' )) 
	Begin
		select @NotificationMessage = @UserName + ' has submitted tactic ' + @componentTitle +' for approval '
		
		insert into User_Notification_Messages(ComponentName,ComponentId,EntityId,[Description],ActionName,IsRead,UserId,RecipientId,CreatedDate,ClientID)
		SELECT @TableName,@objectId,@componentId,@NotificationMessage,@action,0,@Userid,UserId,GETDATE(),@ClientId FROM #tempNotificationdata
		WHERE NotificationInternalUseOnly = @TacticIsSubmitted and UserId <> @Userid
	End

	IF (EXISTS(SELECT * FROM #tempNotificationdata WHERE NotificationInternalUseOnly = @OwnerChange  and @action='ownerchanged' )) 
	Begin
		select @NotificationMessage = @UserName + ' has made you the owner of ' + @description + ' ' + @componentTitle
		if(@Userid <> @EntityOwnerID)
		Begin
			insert into User_Notification_Messages(ComponentName,ComponentId,EntityId,[Description],ActionName,IsRead,UserId,RecipientId,CreatedDate,ClientID)
			values(@TableName,@objectId,@componentId,@NotificationMessage,@action,0,@Userid,@EntityOwnerID,GETDATE(),@ClientId)
		End
	End

END

End



GO

	/* End - Added by Komal Rawal on 08/16/2016 for Ticket #2468 -Notifications*/

-- Add By Nishant Sheth
-- #2502 : Changes on export to csv
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExportToCSV]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[ExportToCSV] AS' 
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExportToCSV]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[ExportToCSV] AS' 
END
GO

ALTER PROCEDURE [dbo].[ExportToCSV]
@PlanId int=0
,@ClientId nvarchar(max)=''
,@HoneyCombids nvarchar(max)=null
,@CurrencyExchangeRate FLOAT=1
AS
BEGIN

SET NOCOUNT ON;

IF OBJECT_ID('tempdb..#tblPivot') IS NOT NULL
   DROP TABLE #tblPivot

IF OBJECT_ID('tempdb..#tblColName') IS NOT NULL
   DROP TABLE #tblColName

IF OBJECT_ID('tempdb..#tblCustomData') IS NOT NULL
   DROP TABLE #tblCustomData

IF OBJECT_ID('tempdb..#tbldynamicColumns') IS NOT NULL
	DROP TABLE #tbldynamicColumns

IF OBJECT_ID('tempdb..#tblHoneyCombIds') IS NOT NULL
	DROP TABLE #tblHoneyCombIds

IF OBJECT_ID('tempdb..#tblPlanHoneyComb') IS NOT NULL
	DROP TABLE #tblPlanHoneyComb

IF OBJECT_ID('tempdb..#tblCampaignHoneyComb') IS NOT NULL
	DROP TABLE #tblCampaignHoneyComb

IF OBJECT_ID('tempdb..#tblProgramHoneyComb') IS NOT NULL
	DROP TABLE #tblProgramHoneyComb

IF OBJECT_ID('tempdb..#tblTacticHoneyComb') IS NOT NULL
	DROP TABLE #tblTacticHoneyComb

IF OBJECT_ID('tempdb.dbo.#EntityValues') IS NOT NULL 
	DROP TABLE #EntityValues 

	SELECT Item into #tblHoneyCombIds From dbo.SplitString(@HoneyCombids,',') 

	SELECT REPLACE(Item,'Plan_','') as Item into #tblPlanHoneyComb FROM #tblHoneyCombIds WHERE Item like '%Plan%'

	SELECT REPLACE(Item,'Campaign_','') as Item into #tblCampaignHoneyComb FROM #tblHoneyCombIds WHERE Item like '%Campaign%'

	SELECT REPLACE(Item,'Program_','') as Item into #tblProgramHoneyComb FROM #tblHoneyCombIds WHERE Item like '%Program%'

	SELECT REPLACE(Item,'Tactic_','') as Item into #tblTacticHoneyComb FROM #tblHoneyCombIds WHERE Item like '%Tactic%'
	


DECLARE @Entityids nvarchar(max)=''
	
SELECT ROW_NUMBER() OVER(ORDER BY (SELECT 100)) AS ROWNUM,* into #tblPivot FROM
(
-- Plan Details
SELECT   NULL AS 'CustomFieldEntityId',[Section] = 'Plan',[Plan].PlanId  AS 'EntityId',CustomField.CustomFieldId AS'CustomFieldId',
NULL AS 'Value','Plan' AS'EntityType',[CustomField].Name AS 'ColName',0 As 'ParentId', [Plan].Title AS 'Plan',NULL AS 'Campaign',NULL AS 'Program',NULL AS 'Tactic',NULL AS 'LineItem'
,Convert(nvarchar(10),Campaign.StartDate,101) AS 'StartDate', Convert(nvarchar(10),Campaign.EndDate,101) AS 'EndDate',null As 'PlannedCost',null AS 'Type',null AS SFDCId,null AS EloquaId
,[Plan].CreatedBy AS 'CreatedBy'
,null AS 'TargetStageGoal'
,null AS 'MQL',null AS 'Revenue',null AS 'Owner'
,[Plan].ModelId AS 'ModelId'
,null As 'ExternalName'
, 'TextBox' As CustomFieldType
FROM [Plan] AS [Plan] WITH (NOLOCK) 
OUTER APPLY (SELECT PlanCampaignId,PlanId,StartDate,EndDate FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE [Plan].PlanId=[Campaign].PlanId AND Campaign.IsDeleted=0) Campaign 
OUTER APPLY (SELECT * FROM CustomField WHERE CustomField.ClientId=@ClientId AND CustomField.EntityType!='Budget' AND IsDeleted=0) [CustomField]
WHERE 
--[Plan].PlanId IN (@PlanId)
(CASE WHEN @HoneyCombids IS NULL THEN [Plan].PlanId END) IN (@PlanId)
OR (CASE WHEN @HoneyCombids IS NOT NULL THEN [Plan].PlanId END) IN (SELECT Item FROm #tblPlanHoneyComb)
UNION ALL
-- Campaign Details
SELECT DISTINCT CustomField_Entity.CustomFieldEntityId,[Section] = 'Campaign',[Campaign].PlanCampaignId As 'EntityId' ,CustomField.CustomFieldId,
CONVERT(NVARCHAR(800),CASE CustomFieldType.Name WHEN 'DropDownList' THEN (SELECT Value  FROM CustomFieldOption WHERE CustomFieldOptionId=CustomField_Entity.Value) ELSE CustomField_Entity.Value END) AS 'Value',
'Campaign' AS'EntityType',[CustomField].Name as 'ColName',[Plan].PlanId As 'ParentId'
, [Plan].Title AS 'Plan',[Campaign].Title AS 'Campaign',NULL AS 'Program',NULL AS 'Tactic',NULL AS 'LineItem'
,Convert(nvarchar(10),Campaign.StartDate,101) AS 'StartDate',Convert(nvarchar(10),Campaign.EndDate,101) AS 'EndDate',null As 'PlannedCost',null AS 'Type',Campaign.IntegrationInstanceCampaignId AS SFDCId,null AS EloquaId
,[Campaign].CreatedBy AS 'CreatedBy'
,null AS 'TargetStageGoal'
,null AS 'MQL',null AS 'Revenue',null AS 'Owner'
,[Plan].ModelId AS 'ModelId'
,null As 'ExternalName'
,IsNull(CustomFieldType.Name,'TextBox') as CustomFieldType
FROM [Plan] WITH (NOLOCK)
CROSS APPLY (SELECT PlanCampaignId,PlanId,Title,StartDate,EndDate,IntegrationInstanceCampaignId,CreatedBy FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE [Plan].PlanId=[Campaign].PlanId AND Campaign.IsDeleted=0) Campaign 
OUTER APPLY (SELECT * FROM CustomField_Entity AS CustomField_Entity WITH (NOLOCK) WHERE [Campaign].PlanCampaignId=CustomField_Entity.EntityId) CustomField_Entity
OUTER APPLY (SELECT * FROM CustomField WHERE  CustomField.ClientId=@ClientId AND CustomField.EntityType='Campaign' AND CustomField.CustomFieldId = CustomField_Entity.CustomFieldId AND IsDeleted=0) [CustomField]
OUTER APPLY (SELECT * FROM CustomFieldType WHERE CustomField.CustomFieldTypeId=CustomFieldType.CustomFieldTypeId) [CustomFieldType]
OUTER APPLY (SELECT * FROM CustomFieldOption WHERE CustomField.CustomFieldId=CustomFieldOption.CustomFieldId AND CustomFieldOption.IsDeleted=0) [CustomFieldOption]
WHERE 
(CASE WHEN @HoneyCombids IS NULL THEN [Plan].PlanId END) IN (@PlanId)
OR (CASE WHEN @HoneyCombids IS NOT NULL THEN Campaign.PlanCampaignId END)IN(SELECT item FROM #tblCampaignHoneyComb)
UNION ALL
-- Prgoram Details
SELECT DISTINCT CustomField_Entity.CustomFieldEntityId,[Section] = 'Program',[Program].PlanProgramId As 'EntityId',CustomField_Entity.CustomFieldId,
CONVERT(NVARCHAR(800),CASE [CustomFieldType].Name WHEN 'DropDownList' THEN (SELECT [CustomFieldOption].Value  FROM CustomFieldOption WHERE CustomFieldOptionId=CustomField_Entity.Value)  ELSE CustomField_Entity.Value END) AS 'Value',
'Program' AS'EntityType',[CustomField].Name as 'ColName',[Campaign].PlanCampaignId As 'ParentId'
, [Plan].Title AS 'Plan',[Campaign].Title AS 'Campaign',[Program].Title AS 'Program',NULL AS 'Tactic',NULL AS 'LineItem'
,Convert(nvarchar(10),Program.StartDate,101) AS 'StartDate',Convert(nvarchar(10),Program.EndDate,101) AS 'EndDate',NULL As 'PlannedCost',null AS 'Type',Program.IntegrationInstanceProgramId AS SFDCId,null AS EloquaId
,[Program].CreatedBy AS 'CreatedBy'
,null AS 'TargetStageGoal'
,null AS 'MQL',null AS 'Revenue',null AS 'Owner'
,[Plan].ModelId AS 'ModelId'
,null As 'ExternalName'
,IsNull([CustomFieldType].Name,'TextBox') as CustomFieldType
FROM [Plan] WITH (NOLOCK)
CROSS APPLY (SELECT PlanCampaignId,PlanId,Title FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE [Plan].PlanId=[Campaign].PlanId AND Campaign.IsDeleted=0 ) Campaign 
CROSS APPLY (SELECT PlanProgramId,PlanCampaignId,Title,StartDate,EndDate,IntegrationInstanceProgramId,CreatedBy FROM Plan_Campaign_Program AS Program WITH (NOLOCK) WHERE [Campaign].PlanCampaignId= Program.PlanCampaignId 
AND Program.IsDeleted=0 ) Program
OUTER APPLY (SELECT PlanTacticId,PlanProgramId,[Status],Title,TacticCustomName,StartDate,EndDate,Cost,TacticTypeId,IntegrationInstanceTacticId,IntegrationInstanceEloquaId,CreatedBy,StageId,ProjectedStageValue FROM Plan_Campaign_Program_Tactic AS Tactic WITH (NOLOCK) WHERE [Program].PlanProgramId=[Tactic].PlanProgramId AND Tactic.IsDeleted=0 ) Tactic
OUTER APPLY (SELECT * FROM CustomField_Entity AS CustomField_Entity WITH (NOLOCK) WHERE [Program].PlanProgramId=CustomField_Entity.EntityId ) CustomField_Entity
OUTER APPLY (SELECT * FROM CustomField WHERE  CustomField.ClientId=@ClientId AND CustomField.EntityType='Program' AND CustomField.CustomFieldId = CustomField_Entity.CustomFieldId AND IsDeleted=0) [CustomField]
OUTER APPLY (SELECT * FROM CustomFieldType WHERE CustomFieldType.CustomFieldTypeId=CustomField.CustomFieldTypeId) [CustomFieldType]
OUTER APPLY (SELECT * FROM CustomFieldOption WHERE CustomField.CustomFieldId=CustomFieldOption.CustomFieldId AND CustomFieldOption.IsDeleted=0) [CustomFieldOption]
WHERE
(CASE WHEN @HoneyCombids IS NULL THEN [Plan].PlanId END) IN (@PlanId)
OR (CASE WHEN @HoneyCombids IS NOT NULL THEN Program.PlanProgramId END)IN(SELECT item From #tblProgramHoneyComb)
UNION ALL
-- Tactic Details
SELECT DISTINCT CustomField_Entity.CustomFieldEntityId,[Section] = 'Tactic',[Tactic].PlanTacticId As 'EntityId',CustomField_Entity.CustomFieldId,
CONVERT(NVARCHAR(800),CASE [CustomFieldType].Name WHEN 'DropDownList' THEN (SELECT [CustomFieldOption].Value  FROM CustomFieldOption WHERE CustomFieldOptionId=CustomField_Entity.Value)  ELSE CustomField_Entity.Value END) AS 'Value'
,'Tactic' AS'EntityType',[CustomField].Name as 'ColName',[Program].PlanProgramId As 'ParentId'
, [Plan].Title AS 'Plan',[Campaign].Title AS 'Campaign',[Program].Title AS 'Program',[Tactic].Title AS 'Tactic',NULL AS 'LineItem'
,Convert(nvarchar(10),Tactic.StartDate,101) AS 'StartDate',Convert(nvarchar(10),Tactic.EndDate,101) AS 'EndDate',([Tactic].Cost*@CurrencyExchangeRate) As 'PlannedCost',[TacticType].Title AS 'Type',Tactic.IntegrationInstanceTacticId AS SFDCId,Tactic.IntegrationInstanceEloquaId AS EloquaId
,[Tactic].CreatedBy AS 'CreatedBy'
,CONVERT(NVARCHAR(MAX),[Tactic].ProjectedStageValue) +' '+ [Stage].Title As 'TargetStageGoal'
,null AS 'MQL',null AS 'Revenue',null AS 'Owner'
,[Plan].ModelId AS 'ModelId'
,[Tactic].TacticCustomName As 'ExternalName'
,IsNull([CustomFieldType].Name,'TextBox') as CustomFieldType
FROM [Plan] WITH (NOLOCK)
CROSS APPLY (SELECT PlanCampaignId,PlanId,Title FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE [Plan].PlanId=[Campaign].PlanId AND Campaign.IsDeleted=0 ) Campaign 
CROSS APPLY (SELECT PlanProgramId,PlanCampaignId,Title FROM Plan_Campaign_Program AS Program WITH (NOLOCK) WHERE [Campaign].PlanCampaignId= Program.PlanCampaignId AND Program.IsDeleted=0 ) Program
CROSS APPLY (SELECT PlanTacticId,PlanProgramId,[Status],Title,TacticCustomName,StartDate,EndDate,Cost,TacticTypeId,IntegrationInstanceTacticId,IntegrationInstanceEloquaId,CreatedBy,StageId,ProjectedStageValue FROM Plan_Campaign_Program_Tactic AS Tactic WITH (NOLOCK) WHERE 
[Program].PlanProgramId=[Tactic].PlanProgramId 
AND Tactic.IsDeleted=0 ) Tactic
OUTER APPLY (SELECT [StageId],[Title] FROM [Stage] WITH (NOLOCK) Where [Tactic].StageId=Stage.StageId AND  IsDeleted=0) Stage
OUTER APPLY (SELECT * FROM CustomField_Entity AS CustomField_Entity WITH (NOLOCK) WHERE [Tactic].PlanTacticId=CustomField_Entity.EntityId) CustomField_Entity
OUTER APPLY (SELECT * FROM CustomField WHERE  CustomField.ClientId=@ClientId AND CustomField.EntityType='Tactic' AND CustomField.CustomFieldId = CustomField_Entity.CustomFieldId AND IsDeleted=0) [CustomField]
OUTER APPLY (SELECT * FROM CustomFieldType WHERE CustomFieldType.CustomFieldTypeId=CustomField.CustomFieldTypeId) [CustomFieldType]
OUTER APPLY (SELECT * FROM CustomFieldOption WHERE CustomField.CustomFieldId=CustomFieldOption.CustomFieldId AND CustomFieldOption.IsDeleted=0 ) [CustomFieldOption]
OUTER APPLY (SELECT TacticTypeId,Title FROM TacticType AS TacticType WITH (NOLOCK) WHERE [Tactic].TacticTypeId=TacticType.TacticTypeId AND TacticType.IsDeleted=0) TacticType
WHERE 
(CASE WHEN @HoneyCombids IS NULL THEN [Plan].PlanId END) IN (@PlanId)
OR (CASE WHEN @HoneyCombids IS NOT NULL THEN Tactic.PlanTacticId END)IN(SELECT item From #tblTacticHoneyComb) 
UNION ALL
-- Line Item Details
SELECT DISTINCT CustomField_Entity.CustomFieldEntityId,[Section] = 'LineItem',[lineitem].PlanLineItemId As 'EntityId',CustomField_Entity.CustomFieldId,
CONVERT(NVARCHAR(800),CASE [CustomFieldType].Name WHEN 'DropDownList' THEN (SELECT [CustomFieldOption].Value  FROM CustomFieldOption WHERE CustomFieldOptionId=CustomField_Entity.Value)  ELSE CustomField_Entity.Value END) AS 'Value',
'LineItem' AS'EntityType',[CustomField].Name as 'ColName',[Tactic].PlanTacticId As 'ParentId'
, [Plan].Title AS 'Plan',[Campaign].Title AS 'Campaign',[Program].Title AS 'Program',[Tactic].Title AS 'Tactic',[lineitem].Title AS 'LineItem'
,NULL AS 'StartDate',NULL AS 'EndDate',([lineitem].Cost*@CurrencyExchangeRate) As 'PlannedCost',[LineItemType].Title As 'Type',null AS SFDCId,null AS EloquaId
,[lineitem].CreatedBy AS 'CreatedBy'
,null AS 'TargetStageGoal'
,null AS 'MQL',null AS 'Revenue',null AS 'Owner'
,[Plan].ModelId AS 'ModelId'
,null As 'ExternalName'
,IsNull([CustomFieldType].Name,'TextBox') as CustomFieldType
FROM [Plan] WITH (NOLOCK)
CROSS APPLY (SELECT PlanCampaignId,PlanId,Title FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE [Plan].PlanId=[Campaign].PlanId AND Campaign.IsDeleted=0 ) Campaign 
CROSS APPLY (SELECT PlanProgramId,PlanCampaignId,Title FROM Plan_Campaign_Program AS Program WITH (NOLOCK) WHERE [Campaign].PlanCampaignId= Program.PlanCampaignId AND Program.IsDeleted=0 ) Program
CROSS APPLY (SELECT PlanTacticId,PlanProgramId,Title FROM Plan_Campaign_Program_Tactic AS Tactic WITH (NOLOCK) WHERE [Program].PlanProgramId=[Tactic].PlanProgramId AND Tactic.IsDeleted=0 ) Tactic
CROSS APPLY (SELECT PlanLineItemId,PlanTacticId,Title,LineItemTypeId,Cost,CreatedBy FROM Plan_Campaign_Program_Tactic_LineItem AS lineitem WITH (NOLOCK) WHERE [Tactic].PlanTacticId=[lineitem].PlanTacticId AND lineitem.IsDeleted=0) lineitem
OUTER APPLY (SELECT * FROM CustomField_Entity AS CustomField_Entity WITH (NOLOCK) WHERE [lineitem].PlanLineItemId=CustomField_Entity.EntityId) CustomField_Entity
OUTER APPLY (SELECT * FROM CustomField WHERE  CustomField.ClientId=@ClientId AND CustomField.EntityType='Lineitem' AND CustomField.CustomFieldId = CustomField_Entity.CustomFieldId AND IsDeleted=0) [CustomField]
OUTER APPLY (SELECT * FROM CustomFieldType WHERE CustomFieldType.CustomFieldTypeId=CustomField.CustomFieldTypeId) [CustomFieldType]
OUTER APPLY (SELECT * FROM CustomFieldOption WHERE CustomField.CustomFieldId=CustomFieldOption.CustomFieldId AND CustomFieldOption.IsDeleted=0) [CustomFieldOption]
OUTER APPLY (SELECT LineItemTypeId,Title FROM LineItemType AS LineItemType WITH (NOLOCK) WHERE [lineitem].LineItemTypeId=LineItemType.LineItemTypeId AND LineItemType.IsDeleted=0) LineItemType
WHERE 
(CASE WHEN @HoneyCombids IS NULL THEN [Plan].PlanId END) IN (@PlanId)
) tblUnion

DECLARE   @ConcatString NVARCHAR(Max)=''

Declare @RowCount int , @Count int=1

SELECT ColName,ROW_NUMBER() OVER(ORDER BY (SELECT 100)) AS ROWNUM into #tblColName FROM (SELECT Distinct  ColName FROM #tblPivot WHERE ColName IS NOT NULL) tblColName

SET @RowCount=(SELECT COUNT(*) FROM #tblColName)
Declare @Delimeter varchar(5)=',';

CREATE TABLE #tblCustomData
(
ROWNUM INT,
Section NVARCHAR(MAX),
[Plan] NVARCHAR(MAX),		
Campaign NVARCHAR(MAX),
Program NVARCHAR(MAX),
Tactic NVARCHAR(MAX),
Lineitem NVARCHAR(MAX),
StartDate NVARCHAR(MAX),
EndDate NVARCHAR(MAX),
PlannedCost FLOAT,
[Type] NVARCHAR(MAX),
SFDCId NVARCHAR(MAX),
EloquaId NVARCHAR(MAX),
CustomFieldEntityId INT, 
CustomFieldId INT,
CreatedBy Uniqueidentifier,
TargetStageGoal NVARCHAR(MAX),
ModelId INT,
MQL FLOAT,
Revenue FLOAT,
[Owner] NVARCHAR(MAX),
ExternalName NVARCHAR(MAX),
EntityId INT,
EntityType NVARCHAR(MAX),
ParentId INT,
CustomFieldType NVARCHAR(MAX)
)

DECLARE @Colname nvarchar(max)=''
DECLARE @AlterTable nvarchar(max)=''
While @Count<=@RowCount
BEGIN

SELECT @Colname = REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(ColName,')','-'),'(','-'),'*','-'),'&','-'),'^','-'),'%','-'),'$','-'),'#','-'),'@','-'),'~','-'),'–','-') FROM #tblColName WHERE ROWNUM=@Count --This is to Special charachter En Dash replace with Hyphen in CustomField Name
SET @AlterTable +=' ALTER TABLE #tblCustomData ADD ['+@Colname+'] NVARCHAR(MAX) ';
SET @ConcatString= @ConcatString +'['+ @Colname +']'+@Delimeter ;


SET @Count=@Count+1;
END


IF @ConcatString=null OR @ConcatString=''
BEGIN
	SET @ConcatString='DummyCol '
	SET @AlterTable+=' ALTER TABLE #tblCustomData ADD DummyCol NVARCHAR(MAX) '
END

SELECT @ConcatString=LEFT(@ConcatString, LEN(@ConcatString) - 1)

EXEC(@AlterTable)
DECLARE @query nvarchar(max)

    SELECT @query = 
    'SELECT *  FROM
    (SELECT     
		ROW_NUMBER() OVER(ORDER BY (SELECT 100)) AS ROWNUM,
		Section,
		[Plan],		
		Campaign,
		Program,
		Tactic,
		Lineitem,
		StartDate,
		EndDate,
		PlannedCost,
		Type,
		SFDCId,
		EloquaId,
        CustomFieldEntityId, 
		CustomFieldId,
		CreatedBy,
		TargetStageGoal,
		ModelId,
		MQL,
		Revenue,
		Owner,
		ExternalName,
              EntityId,
			  EntityType,
			  ParentId,
			  ColName,
              CONVERT(NVARCHAR(MAX),Value) AS Value,
			  CustomFieldType
    FROM #tblPivot WITH (NOLOCK))X 
    PIVOT 
    (
        MIN(Value)
        for [ColName] in (' + @ConcatString + ')
    ) P  
	'
	
	INSERT INTO #tblCustomData EXEC SP_EXECUTESQL @query	
	DECLARE @CustomtblCount int
	DECLARE @initCustomCount int =1
		
	select ROW_NUMBER() OVER (ORDER BY (SELECT 100)) AS ROWNUM,name into #tbldynamicColumns from tempdb.sys.columns where object_id =
	object_id('tempdb..#tblCustomData');
	
	DECLARE @SqlStuff VARCHAR(max)='SELECT '
	SET @Count=1
	DECLARE @Val nvarchar(max)=''
	SELECT @RowCount = COUNT(*) FROM #tbldynamicColumns
	
	SET @Delimeter=',';	
	select * into #EntityValues from #tblCustomData WHERE 1=0
	-- Replace Stuff Logic
	-- ADD Columns into #EntityValues
	DECLARE @InsertStatement NVARCHAR(MAX)=''
	
	SET @AlterTable=''
	SET @Colname=''
	SET @Count=1
	SET @AlterTable+=' ALTER TABLE #EntityValues ADD Col_RowGroup NVARCHAR(MAX) '
	SET @InsertStatement=' INSERT INTO #EntityValues ('
	While @Count<=@RowCount
	BEGIN
		SELECT @Colname = name FROM #tbldynamicColumns WHERE ROWNUM=@Count
		SET @AlterTable +=' ALTER TABLE #EntityValues ADD [Col_'+(SELECT REPLACE(REPLACE(@Colname,' ','#'),'-','@'))+'] NVARCHAR(MAX) ';
		
		SET @InsertStatement+='['+@Colname+']'+@Delimeter
	SET @Count=@Count+1;
	END
	SET @InsertStatement+='Col_RowGroup) '
	--PRINT(@AlterTable)
	EXEC(@AlterTable)
	SET @InsertStatement+=' SELECT *,Col_RowGroup = ROW_NUMBER() OVER (PARTITION BY EntityId, EntityType,CustomFieldId ORDER BY (SELECT 100)) FROM #tblCustomData'
	--PRINT(@InsertStatement)
	EXEC(@InsertStatement)

	select ROW_NUMBER() OVER (ORDER BY (SELECT 100)) AS ROWNUM,name into #tblEntityColumns from tempdb.sys.columns where object_id = object_id('tempdb..#EntityValues');

	DECLARE @EntityRowCount INT=0
	SELECT  @EntityRowCount = COUNT(*) FROM #tblEntityColumns
	
	DECLARE @MergeData nvarchar(max)=''
	SET @MergeData=''

	-- Declare Dynamic Variables
	While @Count<=@EntityRowCount
	BEGIN
		IF(@Count=@EntityRowCount)
		BEGIN
			SET @Delimeter='';
		END
		SET @Val =''
		(SELECT @Val=name FROM #tblEntityColumns WHERE ROWNUM=@Count)
		SET @MergeData+=' DECLARE @Col_'+(SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))+' NVARCHAR(MAX) '
		SET @Count=@Count+1;
	END
	PRINT(@MergeData)
	-- END Dynamic Variables
	
	-- Update #EntityValues Tables row
	DECLARE @UpdateStatement NVARCHAR(MAX)=''
	SET @UpdateStatement=@MergeData+ '	UPDATE #EntityValues SET '
	SET @Count=1;
	SET @Delimeter=',';

	While @Count<=@EntityRowCount
	BEGIN
		IF(@Count=@EntityRowCount)
		BEGIN
			SET @Delimeter='';
		END
		SET @Val =''
		(SELECT @Val=name FROM #tblEntityColumns WHERE ROWNUM=@Count)
		IF CHARINDEX('Col_',@Val) > 0
		BEGIN
		IF (@Val!='Col_RowGroup' AND @Val!='Col_ROWNUM')
		BEGIN
		IF(@Val='Col_PlannedCost')
		BEGIN
			SET @UpdateStatement+='  @Col_'+(SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))+' = ['+(SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))+'] = CASE WHEN Col_RowGroup=1 THEN CONVERT(NVARCHAR(MAX),CAST(['+(SELECT REPLACE( REPLACE(REPLACE(@Val,'#',' '),'@','-'),'Col_',''))+'] AS decimal(38,2))) ELSE @Col_'+(SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))+'+'';''+ CONVERT(NVARCHAR(MAX),CAST(['+(SELECT REPLACE( REPLACE(REPLACE(@Val,'#',' '),'@','-'),'Col_',''))+'] AS decimal(38,2))) END'+@Delimeter
		END
		ELSE 
		BEGIN
			SET @UpdateStatement+='  @Col_'+(SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))+' = ['+(SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))+'] = CASE WHEN Col_RowGroup=1 THEN CONVERT(NVARCHAR(MAX),['+(SELECT REPLACE( REPLACE(REPLACE(@Val,'#',' '),'@','-'),'Col_',''))+']) ELSE @Col_'+(SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))+'+'';''+ CONVERT(NVARCHAR(MAX),['+(SELECT REPLACE( REPLACE(REPLACE(@Val,'#',' '),'@','-'),'Col_',''))+']) END'+@Delimeter
		END
		END
		END
		SET @Count=@Count+1;
	END

	EXEC(@UpdateStatement)
	
	-- Select With GroupBy
	SET @Count=1;
	SET @Delimeter=',';
	DECLARE @SelectGroup NVARCHAR(MAX)=''
	DECLARE @ActualColName NVARCHAR(MAX)=''
	SET @SelectGroup=' SELECT EntityId,EntityType'
	While @Count<=@EntityRowCount
	BEGIN
	SET @Val =''
		(SELECT @Val=name FROM #tblEntityColumns WHERE ROWNUM=@Count)
	IF CHARINDEX('Col_',@Val) > 0
	BEGIN
		IF (@Val!='Col_RowGroup' AND @Val!='Col_ROWNUM')
		BEGIN
			SET @ActualColName=REPLACE( REPLACE(REPLACE(@Val,'#',' '),'@','-'),'Col_','');
			IF (@ActualColName!='CustomFieldId' AND @ActualColName!='CustomFieldType' AND (@Val!='Col_EntityId' AND @Val!='Col_EntityType'))
			BEGIN
			IF(@ActualColName ='CustomFieldEntityId' OR @ActualColName='EntityId' OR @ActualColName='EndDate' OR @ActualColName='StartDate' OR @ActualColName='Plan' OR @ActualColName='Campaign' OR @ActualColName='Program' OR @ActualColName='Tactic' OR @ActualColName='LineItem' OR @ActualColName='EntityType' OR @ActualColName='ROWNUM' OR @ActualColName='PlannedCost' OR @ActualColName='Section' OR @ActualColName='Type' OR @ActualColName='EloquaId' OR @ActualColName='SFDCId' OR @ActualColName='ParentId' OR @ActualColName='CreatedBy' OR @ActualColName='TargetStageGoal' OR @ActualColName='ModelId' OR @ActualColName='ExternalName' OR @ActualColName='MQL' OR @ActualColName='Revenue' OR @ActualColName='Owner')
			BEGIN
				IF @ActualColName!='EndDate'
				BEGIN 
					SET @SelectGroup+= @Delimeter+'['+@ActualColName+'] = MIN(['+ @Val+']) ';
				END
				ELSE 
				BEGIN
					SET @SelectGroup+= @Delimeter+'['+@ActualColName+'] = MAX(['+ @Val+']) ';
				END
			END
			ELSE
			BEGIN
				SET @SelectGroup+=@Delimeter+' ['+@ActualColName+'] = MAX(['+@Val+']) ';
			END
			END
		END
	END
		SET @Count=@Count+1;
	END
	SET @SelectGroup+=' FROM #EntityValues GROUP BY EntityId,EntityType ORDER BY (CASE EntityType WHEN ''Plan'' THEN 1'
	SET @SelectGroup+=' WHEN ''Campaign'' THEN 2'
	SET @SelectGroup+=' WHEN ''Program'' THEN 3'
	SET @SelectGroup+=' WHEN ''Tactic'' THEN 4'
	SET @SelectGroup+=' WHEN ''Lineitem'' THEN 5'
	SET @SelectGroup+=' ELSE 6 END)';
	--PRINT(@SelectGroup)
	EXEC(@SelectGroup)
	
	-- End Update #EntityValues Tables row

	-- End Stuff Logic
	
--Modified By komal rawal if export is from honeycomb dont bring line item Custom fields
  IF (@HoneyCombids IS NULL)
  BEGIN
  SELECT Name FROM CustomField WHERE ClientId=@ClientId
AND IsDeleted=0
AND EntityType IN('Campaign','Program','Tactic','Lineitem')
ORDER BY (CASE EntityType WHEN 'Campaign' THEN 1
	WHEN 'Program' THEN 2
	WHEN 'Tactic' THEN 3
	WHEN 'Lineitem' THEN 4
	ELSE 5 END )
  END

  ELSE 
  BEGIN
  SELECT Name FROM CustomField WHERE ClientId=@ClientId
AND IsDeleted=0
AND EntityType IN('Campaign','Program','Tactic')
ORDER BY (CASE EntityType WHEN 'Campaign' THEN 1
	WHEN 'Program' THEN 2
	WHEN 'Tactic' THEN 3
	ELSE 4 END )
  END
--End
END


GO
-- End By Nishant Sheth

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'INT')
EXEC sys.sp_executesql N'CREATE SCHEMA [INT]'

GO

/****** Object:  StoredProcedure [dbo].[UpdateAlert_Notification]    Script Date: 08/17/2016 14:56:16 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateAlert_Notification]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateAlert_Notification]
GO
/****** Object:  StoredProcedure [dbo].[UpdateAlert_Notification]    Script Date: 08/17/2016 14:56:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateAlert_Notification]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[UpdateAlert_Notification] AS' 
END
GO
-- =============================================
-- Author:		Devanshi gandhi
-- Create date: 16-8-2016
-- Description:	method to update isready field for Alert and Notification
-- =============================================
ALTER PROCEDURE [dbo].[UpdateAlert_Notification]
	@UserId UniqueIdentifier,
	@Type nvarchar(100)

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	If(@Type='alert')
	Begin
		Update Alerts set IsRead=1 where UserId=@UserId
	End
	Else If(@Type='notification')
	Begin
		Declare @ReadDate datetime = GETDATE()
		Update user_notification_messages set IsRead=1 , ReadDate=@ReadDate where RecipientId=@UserId
	End
END

GO

------ NOTE: Execute 'Alerts' acitivity insert attached in BDSAuth DatabaseScript folder prior to execute below script and pick ApplicationActivityId by above BDSAuth script and replace @applicationActivityId variable value with picked value.

-- Add by Devanshi gandhi
-- Created Date: 08/19/2016
-- Desc: Insert 'Alerts' permission to Client_Activity table in Plan database.

------ START: Please modify the below variable value as per requirement.
Declare @clientId uniqueidentifier ='464EB808-AD1F-4481-9365-6AADA15023BD'
Declare @applicationActivityId int = 55  -- Set 'Alerts' application activity Id from Application_Activity table in BDSAuth db.
Declare @createdBy uniqueidentifier ='D3238077-161A-405F-8F0E-10F4D6E50631'
------------ END ------------ 

IF NOT EXISTS(Select 1 from Client_Activity where ClientId=@clientId and ApplicationActivityId=@applicationActivityId)
BEGIN
	INSERT INTO Client_Activity(ClientId,ApplicationActivityId,CreatedBy,CreatedDate) VALUES(@clientId,@applicationActivityId,@createdBy,GETDATE())
END
Go


/*===================================================================================
Added By : Arpita Soni on 08/20/2016
Ticket : #2475
Description: Backend logic to run alert rules and generate alerts based on conditions
===================================================================================*/

-- Create default constraint on CreatedDate on Alerts table
DECLARE @ConstraintName nvarchar(200)
SELECT @ConstraintName = Name FROM SYS.DEFAULT_CONSTRAINTS
WHERE PARENT_OBJECT_ID = OBJECT_ID('[dbo].[Alerts]')
AND PARENT_COLUMN_ID = (SELECT column_id FROM sys.columns
                        WHERE NAME = N'CreatedDate'
                        AND object_id = OBJECT_ID(N'[dbo].[Alerts]'))

IF @ConstraintName IS NULL
BEGIN
	ALTER TABLE Alerts ADD CONSTRAINT DF_Alerts_CreatedDate DEFAULT GETDATE() FOR CreatedDate
END
GO

-- DROP FUNCTION
IF EXISTS (SELECT * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[ProjectedValuesForPlans]') AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
  DROP FUNCTION [dbo].[ProjectedValuesForPlans]
GO

-- DROP FUNCTION
IF EXISTS (SELECT * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[GetTacticsForAllRuleEntities]') AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
  DROP FUNCTION [dbo].[GetTacticsForAllRuleEntities]
GO

IF EXISTS (SELECT * FROM sys.types WHERE is_table_type = 1 AND name = 'TacticForRuleEntities')
	DROP TYPE [dbo].[TacticForRuleEntities]
GO

-- Create User Defined Table Type
/****** Object:  UserDefinedTableType [dbo].[TacticForRuleEntities]    Script Date: 08/19/2016 06:37:10 PM ******/
IF NOT EXISTS (SELECT * FROM sys.types WHERE is_table_type = 1 AND name = 'TacticForRuleEntities')
CREATE TYPE [dbo].[TacticForRuleEntities] AS TABLE(
	[RuleId] [int] NULL,
	[EntityId] [int] NULL,
	[EntityType] [nvarchar](50) NULL,
	[Indicator] [nvarchar](50) NULL,
	[IndicatorTitle] [nvarchar](255) NULL,
	[IndicatorComparision] [nvarchar](10) NULL,
	[IndicatorGoal] [int] NULL,
	[CompletionGoal] [int] NULL,
	[Frequency] [nvarchar](50) NULL,
	[DayOfWeek] [tinyint] NULL,
	[DateOfMonth] [tinyint] NULL,
	[UserId] [uniqueidentifier] NULL,
	[ClientId] [uniqueidentifier] NULL,
	[EntityTitle] [nvarchar](255) NULL,
	[StartDate] [datetime] NULL,
	[EndDate] [datetime] NULL,
	[PercentComplete] [int] NULL,
	[ProjectedStageValue] [int] NULL,
	[ActualStageValue] [int] NULL,
	[CalculatedPercentGoal] [int] NULL
)
GO

-- ======================================================
-- Author:		Arpita Soni
-- Create date: 08/09/2016
-- Description:	Convert date differenct into percentage
-- ======================================================
-- DROP AND CREATE FUNCTION
IF EXISTS (SELECT * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[ConvertDateDifferenceToPercentage]') AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
  DROP FUNCTION [dbo].[ConvertDateDifferenceToPercentage]
GO
CREATE FUNCTION [dbo].[ConvertDateDifferenceToPercentage]
(
	@StartDate DATETIME,
	@EndDate DATETIME
)
RETURNS INT
AS
BEGIN
	DECLARE @Percent INT = (DATEDIFF(DAY, @StartDate,GETDATE()) * 100) / DATEDIFF(DAY, @StartDate,@EndDate)
	
	IF(@Percent > 100)
		SET @Percent = 100

	RETURN @Percent
END
GO

-- =================================================================================
-- Author:		Arpita Soni
-- Create date: 08/19/2016
-- Description:	Get entities from the rules which satisfy the entity completion goal
-- =================================================================================
-- DROP AND CREATE FUNCTION
IF EXISTS (SELECT * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[GetEntitiesReachedCompletionGoal]') AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
  DROP FUNCTION [dbo].[GetEntitiesReachedCompletionGoal]
GO
CREATE FUNCTION [dbo].[GetEntitiesReachedCompletionGoal]()
RETURNS @TempTable TABLE
(
    RuleId INT,
    EntityId INT,
	EntityType NVARCHAR(50),
	Indicator NVARCHAR(50),
	IndicatorComparision NVARCHAR(10),
	IndicatorGoal INT,
	CompletionGoal INT,
	Frequency NVARCHAR(50),
	DayOfWeek TINYINT,
	DateOfMonth TINYINT,
	UserId UNIQUEIDENTIFIER, 
	ClientId UNIQUEIDENTIFIER, 
    EntityTitle NVARCHAR(255),
	StartDate DATETIME,
	EndDate DATETIME,
	PercentComplete INT
)
AS
BEGIN
	INSERT INTO @TempTable 
			SELECT  AR.RuleId,AR.EntityId,AR.EntityType,AR.Indicator,AR.IndicatorComparision,AR.IndicatorGoal,AR.CompletionGoal,
					AR.Frequency,AR.DayOfWeek,AR.DateOfMonth, AR.UserId, AR.ClientId, 
					P.Title, P.StartDate, P.EndDate, dbo.ConvertDateDifferenceToPercentage(P.StartDate,P.EndDate) AS PercentComplete
			FROM Alert_Rules AR
			CROSS APPLY (
				SELECT P1.PlanId, P1.Title, ISNULL(PC.StartDate,DATEADD(yy, DATEDIFF(yy,0,P1.[Year]), 0)) AS StartDate, ISNULL(PC.EndDate,DATEADD(yy, DATEDIFF(yy,0,P1.[Year]) + 1, -1)) AS EndDate FROM [Plan] P1 
				CROSS APPLY (
					SELECT MIN(PC1.StartDate) AS StartDate, MAX(PC1.EndDate) AS EndDate FROM Plan_Campaign PC1 
					WHERE P1.PlanId = PC1.PlanId AND PC1.IsDeleted = 0) PC
				WHERE P1.PlanId = AR.EntityId AND AR.EntityType = 'Plan' AND P1.IsDeleted = 0 AND AR.IsDisabled = 0 
			) P
			UNION ALL 
			SELECT AR.RuleId,AR.EntityId,AR.EntityType,AR.Indicator,AR.IndicatorComparision,AR.IndicatorGoal,AR.CompletionGoal,
					AR.Frequency,AR.DayOfWeek,AR.DateOfMonth, AR.UserId, AR.ClientId, PC.Title, PC.StartDate, PC.EndDate, dbo.ConvertDateDifferenceToPercentage(PC.StartDate,PC.EndDate) AS PercentComplete
			FROM Alert_Rules AR
			CROSS APPLY (SELECT * FROM [Plan_Campaign] PC WHERE PC.PlanCampaignId = AR.EntityId AND AR.EntityType = 'Campaign' AND PC.IsDeleted = 0 AND AR.IsDisabled = 0 ) PC
			UNION ALL 
			SELECT AR.RuleId,AR.EntityId,AR.EntityType,AR.Indicator,AR.IndicatorComparision,AR.IndicatorGoal,AR.CompletionGoal,
					AR.Frequency,AR.DayOfWeek,AR.DateOfMonth, AR.UserId, AR.ClientId, PCP.Title, PCP.StartDate, PCP.EndDate, dbo.ConvertDateDifferenceToPercentage(PCP.StartDate,PCP.EndDate) AS PercentComplete
			FROM Alert_Rules AR
			CROSS APPLY (SELECT * FROM [Plan_Campaign_Program] PCP WHERE PCP.PlanProgramId = AR.EntityId AND AR.EntityType = 'Program' AND PCP.IsDeleted = 0 AND AR.IsDisabled = 0 ) PCP
			UNION ALL 
			SELECT AR.RuleId,AR.EntityId,AR.EntityType,AR.Indicator,AR.IndicatorComparision,AR.IndicatorGoal,AR.CompletionGoal,
					AR.Frequency,AR.DayOfWeek,AR.DateOfMonth, AR.UserId, AR.ClientId, PCPT.Title, PCPT.StartDate, PCPT.EndDate, dbo.ConvertDateDifferenceToPercentage(PCPT.StartDate,PCPT.EndDate) AS PercentComplete
			FROM Alert_Rules AR
			CROSS APPLY (SELECT * FROM [Plan_Campaign_Program_Tactic] PCPT WHERE AR.EntityId = PCPT.PlanTacticId AND AR.EntityType = 'Tactic' AND PCPT.IsDeleted = 0 AND AR.IsDisabled = 0 AND PCPT.[Status] IN ('In-Progress','Complete','Approved') ) PCPT
			UNION ALL 
			SELECT AR.RuleId,AR.EntityId,AR.EntityType,AR.Indicator,AR.IndicatorComparision,AR.IndicatorGoal,AR.CompletionGoal,
					AR.Frequency,AR.DayOfWeek,AR.DateOfMonth, AR.UserId, AR.ClientId, PCPTL.Title, PCPTL.StartDate, PCPTL.EndDate, dbo.ConvertDateDifferenceToPercentage(PCPTL.StartDate,PCPTL.EndDate) AS PercentComplete
			FROM Alert_Rules AR
			CROSS APPLY (
				SELECT PCPTLineItem.PlanLineItemId, PCPTLineItem.Title, PCPTLT.StartDate, PCPTLT.EndDate FROM [Plan_Campaign_Program_Tactic_LineItem] PCPTLineItem 
				CROSS APPLY (
					SELECT PCPT.StartDate,PCPT.EndDate FROM [Plan_Campaign_Program_Tactic] PCPT 
					WHERE PCPTLineItem.PlanTacticId = PCPT.PlanTacticId AND PCPT.IsDeleted = 0 AND PCPT.[Status] IN ('In-Progress','Complete','Approved') 
				) PCPTLT
				WHERE PCPTLineItem.PlanLineItemId = AR.EntityId AND AR.EntityType = 'LineItem' AND AR.Indicator = 'PLANNEDCOST' AND PCPTLineItem.IsDeleted = 0 AND AR.IsDisabled = 0 
			) PCPTL
			WHERE AR.EntityType IN ('Plan','Campaign','Program','Tactic','LineItem')
					
	RETURN
END
GO

-- =============================================
-- Author:		Arpita Soni
-- Create date: 08/19/2016
-- Description:	Get projected value for tactic
-- =============================================
-- DROP AND CREATE FUNCTION
IF EXISTS (SELECT * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[TacticIndicatorProjectedValue]') AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
  DROP FUNCTION [dbo].[TacticIndicatorProjectedValue]
GO
CREATE FUNCTION [dbo].[TacticIndicatorProjectedValue]
(
	@TacticId INT,
	@ModelId INT,
	@TacticStageCode NVARCHAR(255),
	@IndicatorCode NVARCHAR(255),
	@ClientId UNIQUEIDENTIFIER
)
RETURNS FLOAT
AS
BEGIN
	DECLARE @INQCode NVARCHAR(10) = 'INQ',
			@MQLCode NVARCHAR(10) = 'MQL',
			@CWCode NVARCHAR(10) = 'CW',
			@RevenueCode NVARCHAR(20) = 'REVENUE',
			@PlannedCostCode NVARCHAR(20) = 'PLANNEDCOST',
			@ProjectedStageValue FLOAT,
			@TacticStageLevel INT
					
	
	SET @ProjectedStageValue = (SELECT ProjectedStageValue FROM Plan_Campaign_Program_Tactic WHERE PlanTacticId = @TacticId)
	SET @TacticStageLevel = (SELECT [Level] FROM Stage WHERE Code = @TacticStageCode AND ClientId = @ClientId AND IsDeleted = 0) 

	-- Calculate projected revenue for the tactic
	IF(@IndicatorCode = @RevenueCode)
	BEGIN
		
		SELECT @ProjectedStageValue = @ProjectedStageValue * MIN(M.AverageDealSize) * 
										Exp(Sum(IIf([Value]=0,0,Log([Value]/100))))*IIf(Min([Value])=0,0,1)
										FROM Model_Stage MS
										INNER JOIN Stage S ON MS.StageId = S.StageId
										INNER JOIN [Model] M ON MS.ModelId = M.ModelId
										WHERE MS.ModelId = @ModelId AND MS.StageType ='CR' AND S.[Level] >= @TacticStageLevel 
											  AND S.ClientId = @ClientId AND S.IsDeleted = 0
		
	END
	-- Calculate projected planned cost for the tactic
	ELSE IF(@IndicatorCode = @PlannedCostCode)
	BEGIN
		SELECT @ProjectedStageValue = Cost FROM Plan_Campaign_Program_Tactic WHERE PlanTacticId = @TacticId 
	END
	-- Calculate projected stage values for the tactic
	ELSE
	BEGIN
		IF (@IndicatorCode = @TacticStageCode)
		BEGIN
			SELECT @ProjectedStageValue = @ProjectedStageValue
		END
		ELSE
		BEGIN
			-- Variables
			DECLARE @AboveMQLStageLevels NVARCHAR(50),
					@AboveMQLStageIds NVARCHAR(50),
					@IndicatorStageLevel INT,
					@MQLLevel INT,
					@ProjectedMQLValue INT = 0

			-- Fetch levels of stages
			SET @IndicatorStageLevel = (SELECT [Level] FROM Stage WHERE Code = @IndicatorCode AND ClientId = @ClientId AND IsDeleted = 0) 
			
			SET @MQLLevel = (SELECT [Level] FROM Stage WHERE Code = @MQLCode AND ClientId = @ClientId AND IsDeleted = 0) 

			IF(@TacticStageLevel > @IndicatorStageLevel OR @IndicatorCode = @INQCode)
			BEGIN
				SELECT @ProjectedStageValue = 0 
			END
			ELSE
			BEGIN
				-- Get stages and levels before MQL stage
				SELECT @AboveMQLStageLevels = COALESCE(@AboveMQLStageLevels + ', ' ,'') + CAST([Level] AS NVARCHAR(10)),
					   @AboveMQLStageIds = COALESCE(@AboveMQLStageIds + ', ' ,'') + CAST(StageId AS NVARCHAR(10))
				FROM Stage WHERE [Level] < @MQLLevel AND [Level] >= @TacticStageLevel AND ClientId = @ClientId AND IsDeleted = 0

				-- Calculate MQL from any stage
				IF(@TacticStageLevel = @MQLLevel)
				BEGIN
					SET @ProjectedMQLValue = @ProjectedStageValue
				END
				ELSE IF(@TacticStageLevel < @MQLLevel)
				BEGIN
					-- Aggregate all stages which are above MQL
					SET @ProjectedMQLValue = (SELECT ROUND(@ProjectedStageValue * Exp(Sum(IIf([Value]=0,0,Log([Value]/100))))*IIf(Min([Value])=0,0,1),0)
											  FROM Model_Stage M INNER JOIN dbo.SplitString(@AboveMQLStageIds,',') CSVTable
											  ON M.StageId = CSVTable.Item WHERE ModelId = @ModelId AND StageType ='CR')
				END
			
				-- Calculation for final projected stage value
				IF (@IndicatorCode = @MQLCode)
				BEGIN
					SELECT @ProjectedStageValue = @ProjectedMQLValue 
				END
				-- Calculate CW from any stage
				ELSE IF (@IndicatorCode = @CWCode)
				BEGIN
					-- If level of tactic stage is more than MQL then calculate from that level itself to CW
					IF (@TacticStageLevel > @MQLLevel)
					BEGIN
						SET @ProjectedMQLValue = @ProjectedStageValue
						SET @MQLLevel = @TacticStageLevel
					END

					-- If level of tactic stage is less or equal to MQL then calculate from MQL to CW
					SELECT @ProjectedStageValue = ROUND(@ProjectedMQLValue * Exp(Sum(IIf([Value]=0,0,Log([Value]/100))))*IIf(Min([Value])=0,0,1),0) 
					FROM Model_Stage M INNER JOIN Stage S
					ON M.StageId = S.StageId WHERE ModelId = @ModelId AND StageType ='CR' 
					AND S.[Level] < @IndicatorStageLevel AND S.[Level] >= @MQLLevel AND S.ClientId = @ClientId AND S.IsDeleted = 0
				END
			END
		END
	END

	RETURN @ProjectedStageValue
END
GO

-- =============================================
-- Author:		Arpita Soni
-- Create date: 08/19/2016
-- Description:	Get projected value for Plan
-- =============================================
-- DROP AND CREATE FUNCTION
IF EXISTS (SELECT * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[PlanIndicatorProjectedValue]') AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
  DROP FUNCTION [dbo].[PlanIndicatorProjectedValue]
GO
CREATE FUNCTION [dbo].[PlanIndicatorProjectedValue]
(
	@PlanId INT,
	@ModelId INT,
	@PlanStageCode NVARCHAR(255),
	@IndicatorCode NVARCHAR(50),
	@ClientId UNIQUEIDENTIFIER
)
RETURNS FLOAT
AS
BEGIN
	-- Variables
	DECLARE @INQCode NVARCHAR(10) = 'INQ',
			@MQLCode NVARCHAR(10) = 'MQL',
			@CWCode NVARCHAR(10) = 'CW',
			@RevenueCode NVARCHAR(10) = 'REVENUE',
			@ProjectedStageValue INT,
			@ResultValue FLOAT,
			@AboveStageLevels NVARCHAR(MAX),
			@AboveStageIds NVARCHAR(MAX),
			@IndicatorStageLevel INT,
			@PlanStageLevel INT,
			@MQLLevel INT,
			@CWLevel INT
	
	SELECT @ProjectedStageValue = GoalValue FROM [Plan] WHERE PlanId = @PlanId AND IsDeleted = 0

	IF(@PlanStageCode = @IndicatorCode)
	BEGIN
		RETURN @ProjectedStageValue
	END
	
	-- Fetch levels of stages
	SELECT @IndicatorStageLevel = [Level] FROM Stage WHERE Code = @IndicatorCode AND ClientId = @ClientId AND IsDeleted = 0
	SELECT @PlanStageLevel = [Level] FROM Stage WHERE Code = @PlanStageCode AND ClientId = @ClientId AND IsDeleted = 0
	
	SELECT @MQLLevel = [Level] FROM Stage WHERE Code = @MQLCode AND ClientId = @ClientId AND IsDeleted = 0
	SELECT @CWLevel = [Level] FROM Stage WHERE Code = @CWCode AND ClientId = @ClientId AND IsDeleted = 0
	
	IF(@PlanStageCode != @RevenueCode AND (@IndicatorCode = @MQLCode OR @IndicatorStageLevel < @PlanStageLevel))
	BEGIN
		DECLARE @CalStageFrom INT = @PlanStageLevel

		IF(@IndicatorStageLevel < @PlanStageLevel)
		BEGIN
			SET @CalStageFrom = @IndicatorStageLevel
		END

		-- Get stages and levels before MQL stage
		SELECT @AboveStageLevels = COALESCE(@AboveStageLevels + ', ' ,'') + CAST([Level] AS NVARCHAR(10)),
				@AboveStageIds = COALESCE(@AboveStageIds + ', ' ,'') + CAST(StageId AS NVARCHAR(10))
		FROM Stage WHERE [Level] < @MQLLevel AND [Level] >= @CalStageFrom AND ClientId = @ClientId AND IsDeleted = 0
	END
	ELSE
	BEGIN
		-- Get stages and levels upto CW stage
		SELECT @AboveStageLevels = COALESCE(@AboveStageLevels + ', ' ,'') + CAST([Level] AS NVARCHAR(10)),
				@AboveStageIds = COALESCE(@AboveStageIds + ', ' ,'') + CAST(StageId AS NVARCHAR(10))
		FROM Stage WHERE [Level] < @CWLevel AND [Level] >= ISNULL(@PlanStageLevel, @IndicatorStageLevel) AND ClientId = @ClientId AND IsDeleted = 0
	END

	IF(@PlanStageCode = @RevenueCode OR @IndicatorStageLevel < ISNULL(@PlanStageLevel,0))
	BEGIN
		-- Calculate INQ/MQL/CW from Revenue
		SET @ResultValue = (SELECT @ProjectedStageValue / Exp(Sum(IIf([Value]=0,0,Log([Value]/100))))*IIf(Min([Value])=0,0,1)
							FROM Model_Stage M INNER JOIN dbo.SplitString(@AboveStageIds,',')  CSVTable
							ON M.StageId = CSVTable.Item WHERE ModelId = @ModelId AND StageType ='CR') 
		IF(@PlanStageCode = @RevenueCode)
		BEGIN
			SET @ResultValue = ISNULL(@ResultValue, @ProjectedStageValue) / (SELECT AverageDealSize FROM [Model] WHERE ModelId = @ModelId AND IsDeleted = 0)
		END
			
	END
	ELSE
	BEGIN
		-- Calculate INQ/MQL/CW/REVENUE from INQ/MQL/CW
		SET @ResultValue = (SELECT @ProjectedStageValue * 
							Exp(Sum(IIf([Value]=0,0,Log([Value]/100))))*IIf(Min([Value])=0,0,1)
							FROM Model_Stage M INNER JOIN dbo.SplitString(@AboveStageIds,',') CSVTable
							ON M.StageId = CSVTable.Item WHERE ModelId = @ModelId AND StageType ='CR') 

		-- Multiply with AverageDealSize to calculate Revenue
		IF(@IndicatorCode = @RevenueCode)
		BEGIN
			SET @ResultValue = @ResultValue * (SELECT AverageDealSize FROM [Model] WHERE ModelId = @ModelId AND IsDeleted = 0)
		END
	END
	RETURN ROUND(ISNULL(@ResultValue,0),0)
	
END
GO

-- ======================================================================================================
-- Author:		Arpita Soni
-- Create date: 08/17/2016
-- Description:	Get title of indicator based on rule
-- ======================================================================================================
-- DROP AND CREATE FUNCTION
IF EXISTS (SELECT * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[GetIndicatorTitle]') AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
  DROP FUNCTION [dbo].[GetIndicatorTitle]
GO

CREATE FUNCTION [dbo].[GetIndicatorTitle]
(
	@IndicatorCode NVARCHAR(50),
	@ClientId UNIQUEIDENTIFIER
)
RETURNS NVARCHAR(MAX)
AS
BEGIN
	DECLARE @IndicatorTitle NVARCHAR(MAX)

	IF(@IndicatorCode = 'PLANNEDCOST')
	BEGIN
		SET @IndicatorTitle = 'Planned Cost'
	END
	ELSE IF (@IndicatorCode = 'REVENUE')
	BEGIN
		SET @IndicatorTitle = 'Revenue'
	END
	ELSE
	BEGIN
		SELECT @IndicatorTitle = Title FROM Stage WHERE ClientId = @ClientId AND Code = @IndicatorCode
	END

	RETURN @IndicatorTitle
END
GO

-- ======================================================================================================
-- Author:		Arpita Soni
-- Create date: 08/17/2016
-- Description:	List of tactics with projected and actual values for all entities involved in alert rules
-- ======================================================================================================
-- DROP AND CREATE FUNCTION
IF EXISTS (SELECT * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[GetTacticsForAllRuleEntities]') AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
  DROP FUNCTION [dbo].[GetTacticsForAllRuleEntities]
GO
CREATE FUNCTION [dbo].[GetTacticsForAllRuleEntities](
	@TempEntityTable [TacticForRuleEntities] READONLY
)
RETURNS @TempTable TABLE
(
	EntityId INT,
	EntityType NVARCHAR(50),
	Indicator NVARCHAR(50),
	ProjectedStageValue FLOAT,
	ActualStageValue FLOAT
)
AS
BEGIN
		INSERT INTO @TempTable
		SELECT PlanId,'PLAN',Indicator,SUM(ProjectedStageValue),SUM(ActualStageValue)
		FROM (
			SELECT PlanId, Indicator, ProjectedStageValue, 
			(CASE WHEN Indicator = 'PLANNEDCOST' AND SUM(LineItemActuals) IS NOT NULL THEN SUM(LineItemActuals) ELSE SUM(ActualValue) END) AS ActualStageValue
			FROM
			(
				SELECT Tactic.PlanTacticId, Program.PlanProgramId, Campaign.PlanCampaignId, [Plan].PlanId, 
				(CASE WHEN Indicator = 'PLANNEDCOST' THEN
					dbo.TacticIndicatorProjectedValue(Tactic.PlanTacticId,[Plan].ModelId, [Stage].Code,RuleEntityTable.indicator,RuleEntityTable.ClientId)
				ELSE NULL END) AS ProjectedStageValue,
				ISNULL(MIN(ActualValue),0) ActualValue,RuleEntityTable.Indicator,ISNULL(MIN(LineItemActuals),0) AS LineItemActuals, LineItemActual.Period
				FROM @TempEntityTable RuleEntityTable 
				CROSS APPLY (SELECT PlanId,ModelId From [Plan] WITH (NOLOCK) WHERE [Plan].PlanId = RuleEntityTable.EntityId AND [Plan].IsDeleted=0) [Plan]
				-- CROSS APPLY (SELECT ModelId,ClientId From [Model] WITH (NOLOCK) WHERE [Plan].ModelId = [Plan].ModelId AND [Model].IsDeleted=0) [Model]
				CROSS APPLY (SELECT PlanCampaignId,PlanId FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE Campaign.PlanId = [Plan].PlanId AND Campaign.IsDeleted=0) Campaign 
				CROSS APPLY (SELECT PlanProgramId,PlanCampaignId FROM Plan_Campaign_Program AS Program WITH (NOLOCK) WHERE Program.PlanCampaignId=Campaign.PlanCampaignId AND Program.IsDeleted=0) Program
				CROSS APPLY (SELECT PlanTacticId,StageId,TacticTypeId,PlanProgramId FROM Plan_Campaign_Program_Tactic AS Tactic WITH (NOLOCK)
							 WHERE Tactic.PlanProgramId = Program.PlanProgramId AND Tactic.IsDeleted = 0
							 AND Tactic.[Status] IN ('In-Progress','Complete','Approved')) Tactic
				CROSS APPLY (SELECT StageId, Code FROM Stage WHERE Stage.StageId = Tactic.StageId AND Stage.IsDeleted = 0) Stage
				OUTER APPLY (
								SELECT ActualValue,StageTitle,Period FROM Plan_Campaign_Program_Tactic_Actual Actual WITH (NOLOCK)
								WHERE Tactic.PlanTacticId = Actual.PlanTacticId
								AND StageTitle = CASE WHEN RuleEntityTable.INDICATOR = STAGE.CODE AND RuleEntityTable.INDICATOR NOT IN ('MQL','CW','REVENUE')
														THEN 'PROJECTEDSTAGEVALUE'
														ELSE RuleEntityTable.INDICATOR END
							) Actual
				OUTER APPLY (
								SELECT LineItem.PlanLineItemId,LineItem.PlanTacticId, LTActual.Value AS LineItemActuals,LTActual.Period FROM Plan_Campaign_Program_Tactic_LineItem LineItem
								OUTER APPLY (SELECT Value,PlanLineItemId,Period FROM Plan_Campaign_Program_Tactic_LineItem_Actual Actual
								WHERE LineItem.PlanLineItemId = Actual.PlanLineItemId 
								) LTActual WHERE Tactic.PlanTacticId = LineItem.PlanTacticId
								AND RuleEntityTable.Indicator = 'PLANNEDCOST'
				) LineItemActual
				WHERE RuleEntityTable.EntityType = 'Plan'
				GROUP BY [Tactic].PlanTacticId, Program.PlanProgramId, Campaign.PlanCampaignId,[Plan].PlanId,Actual.Period,RuleEntityTable.Indicator,
				[Plan].ModelId,RuleEntityTable.ClientId,Stage.Code, LineItemActual.Period
			) P
			GROUP BY PlanTacticId, PlanProgramId, PlanCampaignId, PlanId, Indicator, ProjectedStageValue
		) FinalData GROUP BY PlanId,Indicator

		UNION 
		SELECT PlanCampaignId,'CAMPAIGN',Indicator,SUM(ProjectedStageValue),SUM(ActualStageValue)
		FROM (
		SELECT PlanCampaignId, Indicator, ProjectedStageValue, 
		(CASE WHEN Indicator = 'PLANNEDCOST' AND SUM(LineItemActuals) IS NOT NULL THEN SUM(LineItemActuals) ELSE SUM(ActualValue) END) AS ActualStageValue FROM
		(
			SELECT Tactic.PlanTacticId, Program.PlanProgramId, Campaign.PlanCampaignId, [Plan].PlanId, 
			dbo.TacticIndicatorProjectedValue(Tactic.PlanTacticId,[Plan].ModelId, [Stage].Code,RuleEntityTable.indicator,RuleEntityTable.ClientId) AS ProjectedStageValue,
			ISNULL(MIN(ActualValue),0) ActualValue,RuleEntityTable.Indicator,ISNULL(MIN(LineItemActuals),0) AS LineItemActuals, LineItemActual.Period
			FROM @TempEntityTable  RuleEntityTable 
			CROSS APPLY (SELECT PlanCampaignId,PlanId FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE Campaign.PlanCampaignId = RuleEntityTable.EntityId AND Campaign.IsDeleted=0) Campaign 
			CROSS APPLY (SELECT PlanId,ModelId From [Plan] WITH (NOLOCK) WHERE [Plan].PlanId = Campaign.PlanId AND [Plan].IsDeleted=0) [Plan]
			-- CROSS APPLY (SELECT ModelId,ClientId From [Model] WITH (NOLOCK) WHERE [Plan].ModelId = [Plan].ModelId AND [Model].IsDeleted=0) [Model]
			CROSS APPLY (SELECT PlanProgramId,PlanCampaignId FROM Plan_Campaign_Program AS Program WITH (NOLOCK) WHERE Program.PlanCampaignId=Campaign.PlanCampaignId AND Program.IsDeleted=0) Program
			CROSS APPLY (SELECT PlanTacticId,StageId,TacticTypeId,PlanProgramId FROM Plan_Campaign_Program_Tactic AS Tactic WITH (NOLOCK) WHERE Tactic.PlanProgramId=Program.PlanProgramId AND Tactic.IsDeleted=0
						AND Tactic.[Status] IN ('In-Progress','Complete','Approved')) Tactic
			CROSS APPLY (SELECT StageId, Code FROM Stage WHERE Stage.StageId = Tactic.StageId AND Stage.IsDeleted = 0) Stage
			OUTER APPLY (
							SELECT ActualValue,StageTitle,Period FROM Plan_Campaign_Program_Tactic_Actual Actual WITH (NOLOCK)
							WHERE Tactic.PlanTacticId = Actual.PlanTacticId 
							AND StageTitle = CASE WHEN RuleEntityTable.INDICATOR = STAGE.CODE AND RuleEntityTable.INDICATOR NOT IN ('MQL','CW','REVENUE')  
													THEN 'PROJECTEDSTAGEVALUE' 
													ELSE RuleEntityTable.INDICATOR END
						) Actual
			OUTER APPLY (
							SELECT LineItem.PlanLineItemId,LineItem.PlanTacticId, LTActual.Value AS LineItemActuals,LTActual.Period FROM Plan_Campaign_Program_Tactic_LineItem LineItem
							OUTER APPLY (SELECT Value,PlanLineItemId,Period FROM Plan_Campaign_Program_Tactic_LineItem_Actual Actual
							WHERE LineItem.PlanLineItemId = Actual.PlanLineItemId 
							) LTActual WHERE Tactic.PlanTacticId = LineItem.PlanTacticId
							AND RuleEntityTable.Indicator = 'PLANNEDCOST'
			) LineItemActual
			WHERE RuleEntityTable.EntityType = 'Campaign'
			GROUP BY [Tactic].PlanTacticId, Program.PlanProgramId, Campaign.PlanCampaignId,[Plan].PlanId,Actual.Period,RuleEntityTable.Indicator,
			[Plan].ModelId,RuleEntityTable.ClientId,Stage.Code, LineItemActual.Period
		) PC
		GROUP BY PlanTacticId, PlanProgramId, PlanCampaignId, PlanId, Indicator, ProjectedStageValue
		) FinalData GROUP BY PlanCampaignId,Indicator

		UNION 
		SELECT PlanProgramId,'PROGRAM',Indicator,SUM(ProjectedStageValue),SUM(ActualStageValue)
		FROM (
		SELECT PlanProgramId, Indicator, ProjectedStageValue, 
		(CASE WHEN Indicator = 'PLANNEDCOST' AND SUM(LineItemActuals) IS NOT NULL THEN SUM(LineItemActuals) ELSE SUM(ActualValue) END) AS ActualStageValue FROM
		(
			SELECT Tactic.PlanTacticId, Program.PlanProgramId, Campaign.PlanCampaignId, [Plan].PlanId, 
			dbo.TacticIndicatorProjectedValue(Tactic.PlanTacticId,[Plan].ModelId, [Stage].Code,RuleEntityTable.indicator,RuleEntityTable.ClientId) AS ProjectedStageValue,
			ISNULL(MIN(ActualValue),0) ActualValue,RuleEntityTable.Indicator,ISNULL(MIN(LineItemActuals),0) AS LineItemActuals, LineItemActual.Period
			FROM @TempEntityTable  RuleEntityTable 
			CROSS APPLY (SELECT PlanProgramId,PlanCampaignId FROM Plan_Campaign_Program AS Program WITH (NOLOCK) WHERE Program.PlanProgramId=RuleEntityTable.EntityId AND Program.IsDeleted=0) Program
			CROSS APPLY (SELECT PlanCampaignId,PlanId  FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE Campaign.PlanCampaignId = Program.PlanCampaignId AND Campaign.IsDeleted=0) Campaign 
			CROSS APPLY (SELECT PlanId,ModelId From [Plan] WITH (NOLOCK) WHERE [Plan].PlanId = Campaign.PlanId AND [Plan].IsDeleted=0) [Plan]
			-- CROSS APPLY (SELECT ModelId,ClientId From [Model] WITH (NOLOCK) WHERE [Plan].ModelId = [Plan].ModelId AND [Model].IsDeleted=0) [Model]
			CROSS APPLY (SELECT PlanTacticId,StageId,TacticTypeId,PlanProgramId FROM Plan_Campaign_Program_Tactic AS Tactic WITH (NOLOCK) WHERE Tactic.PlanProgramId=Program.PlanProgramId AND Tactic.IsDeleted=0
						AND Tactic.[Status] IN ('In-Progress','Complete','Approved')) Tactic
			CROSS APPLY (SELECT StageId, Code FROM Stage WHERE Stage.StageId = Tactic.StageId AND Stage.IsDeleted = 0) Stage
			OUTER APPLY (
							SELECT ActualValue,StageTitle,Period FROM Plan_Campaign_Program_Tactic_Actual Actual WITH (NOLOCK)
							WHERE Tactic.PlanTacticId = Actual.PlanTacticId 
							AND StageTitle = CASE WHEN RuleEntityTable.INDICATOR = STAGE.CODE AND RuleEntityTable.INDICATOR NOT IN ('MQL','CW','REVENUE')  
													THEN 'PROJECTEDSTAGEVALUE' 
													ELSE RuleEntityTable.INDICATOR END
						) Actual
			OUTER APPLY (
							SELECT LineItem.PlanLineItemId,LineItem.PlanTacticId, LTActual.Value AS LineItemActuals,LTActual.Period FROM Plan_Campaign_Program_Tactic_LineItem LineItem
							OUTER APPLY (SELECT Value,PlanLineItemId,Period FROM Plan_Campaign_Program_Tactic_LineItem_Actual Actual
							WHERE LineItem.PlanLineItemId = Actual.PlanLineItemId 
							) LTActual WHERE Tactic.PlanTacticId = LineItem.PlanTacticId
							AND RuleEntityTable.Indicator = 'PLANNEDCOST'
			) LineItemActual
			WHERE RuleEntityTable.EntityType = 'Program'
			GROUP BY [Tactic].PlanTacticId, Program.PlanProgramId, Campaign.PlanCampaignId,[Plan].PlanId,Actual.Period,RuleEntityTable.Indicator,
			[Plan].ModelId,RuleEntityTable.ClientId,Stage.Code, LineItemActual.Period
		) PCP
		GROUP BY PlanTacticId, PlanProgramId, PlanCampaignId, PlanId, Indicator, ProjectedStageValue
		) FinalData GROUP BY PlanProgramId,Indicator
		
		UNION 
		SELECT PlanTacticId,'TACTIC',Indicator,SUM(ProjectedStageValue),SUM(ActualStageValue)
		FROM (
		SELECT PlanTacticId, Indicator, ProjectedStageValue, 
		(CASE WHEN Indicator = 'PLANNEDCOST' AND SUM(LineItemActuals) IS NOT NULL THEN SUM(LineItemActuals) ELSE SUM(ActualValue) END) AS ActualStageValue FROM
		(
			SELECT Tactic.PlanTacticId, Program.PlanProgramId, Campaign.PlanCampaignId, [Plan].PlanId, 
			dbo.TacticIndicatorProjectedValue(Tactic.PlanTacticId,[Plan].ModelId, [Stage].Code,RuleEntityTable.indicator,RuleEntityTable.ClientId) AS ProjectedStageValue,
			ISNULL(MIN(ActualValue),0) ActualValue,RuleEntityTable.Indicator,ISNULL(MIN(LineItemActuals),0) AS LineItemActuals, LineItemActual.Period
			FROM @TempEntityTable  RuleEntityTable 
			CROSS APPLY (SELECT PlanTacticId,StageId,TacticTypeId,PlanProgramId FROM Plan_Campaign_Program_Tactic AS Tactic WITH (NOLOCK) WHERE Tactic.PlanTacticId=RuleEntityTable.EntityId AND Tactic.IsDeleted=0
						AND Tactic.[Status] IN ('In-Progress','Complete','Approved')) Tactic
			CROSS APPLY (SELECT StageId, Code FROM Stage WHERE Stage.StageId = Tactic.StageId AND Stage.IsDeleted = 0) Stage
			CROSS APPLY (SELECT PlanProgramId,PlanCampaignId FROM Plan_Campaign_Program AS Program WITH (NOLOCK) WHERE Program.PlanProgramId=Tactic.PlanProgramId AND Program.IsDeleted=0) Program
			CROSS APPLY (SELECT PlanCampaignId,PlanId  FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE Campaign.PlanCampaignId = Program.PlanCampaignId AND Campaign.IsDeleted=0) Campaign 
			CROSS APPLY (SELECT PlanId,ModelId From [Plan] WITH (NOLOCK) WHERE [Plan].PlanId = Campaign.PlanId AND [Plan].IsDeleted=0) [Plan]
			-- CROSS APPLY (SELECT ModelId,ClientId From [Model] WITH (NOLOCK) WHERE [Plan].ModelId = [Plan].ModelId AND [Model].IsDeleted=0) [Model]
			OUTER APPLY (
							SELECT ActualValue,StageTitle,Period FROM Plan_Campaign_Program_Tactic_Actual Actual WITH (NOLOCK)
							WHERE Tactic.PlanTacticId = Actual.PlanTacticId 
							AND StageTitle = CASE WHEN RuleEntityTable.INDICATOR = STAGE.CODE AND RuleEntityTable.INDICATOR NOT IN ('MQL','CW','REVENUE')  
													THEN 'PROJECTEDSTAGEVALUE' 
													ELSE RuleEntityTable.INDICATOR END
						) Actual
			OUTER APPLY (
							SELECT LineItem.PlanLineItemId,LineItem.PlanTacticId, LTActual.Value AS LineItemActuals,LTActual.Period FROM Plan_Campaign_Program_Tactic_LineItem LineItem
							OUTER APPLY (SELECT Value,PlanLineItemId,Period FROM Plan_Campaign_Program_Tactic_LineItem_Actual Actual
							WHERE LineItem.PlanLineItemId = Actual.PlanLineItemId 
							) LTActual WHERE Tactic.PlanTacticId = LineItem.PlanTacticId
							AND RuleEntityTable.Indicator = 'PLANNEDCOST'
			) LineItemActual
			WHERE RuleEntityTable.EntityType = 'Tactic'
			GROUP BY [Tactic].PlanTacticId, Program.PlanProgramId, Campaign.PlanCampaignId,[Plan].PlanId,Actual.Period,RuleEntityTable.Indicator,
			[Plan].ModelId,RuleEntityTable.ClientId,Stage.Code, LineItemActual.Period
		) PCPT
		GROUP BY PlanTacticId, PlanProgramId, PlanCampaignId, PlanId, Indicator, ProjectedStageValue
		) FinalData GROUP BY PlanTacticId,Indicator
		
		UNION 
		SELECT PlanLineItemId,'LINEITEM',Indicator,SUM(ProjectedStageValue),SUM(ActualStageValue)
		FROM (
		SELECT PlanLineItemId, Indicator, ProjectedStageValue, 
		SUM(LineItemActuals) AS ActualStageValue FROM
		(
			SELECT PlanLineItemId, RuleEntityTable.Indicator, Cost AS ProjectedStageValue
			, SUM(ActualValue) AS LineItemActuals
			FROM @TempEntityTable  RuleEntityTable 
			CROSS APPLY (SELECT PlanLineItemId,Cost,PlanTacticId FROM [Plan_Campaign_Program_Tactic_LineItem] AS LineItem WITH (NOLOCK) WHERE LineItem.PlanLineItemId=RuleEntityTable.EntityId AND LineItem.IsDeleted=0) LineItem
			OUTER APPLY (SELECT Value AS ActualValue FROM Plan_Campaign_Program_Tactic_LineItem_Actual Actual 
				WHERE LineItem.PlanLineItemId = Actual.PlanLineItemId
			) Actual
			WHERE RuleEntityTable.EntityType = 'LineItem' AND RuleEntityTable.Indicator = 'PLANNEDCOST'
			GROUP BY PlanLineItemId, RuleEntityTable.Indicator, Cost
		) PCPTL
		GROUP BY PlanLineItemId, Indicator, ProjectedStageValue
		) FinalData GROUP BY PlanLineItemId,Indicator

	RETURN
END
GO


-- =============================================
-- Author:		Arpita Soni
-- Create date: 08/17/2016
-- Description:	List of plans with projected and actual values for all entities involved in alert rules
-- =============================================
-- DROP AND CREATE FUNCTION
IF EXISTS (SELECT * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[ProjectedValuesForPlans]') AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
  DROP FUNCTION [dbo].[ProjectedValuesForPlans]
GO
CREATE FUNCTION [dbo].[ProjectedValuesForPlans](
	@TempEntityTable [TacticForRuleEntities] READONLY
)
RETURNS @TempTable TABLE
(
	PlanTacticId INT NULL,
	PlanProgramId INT NULL,
	PlanCampaignId INT NULL,
	PlanId INT,
	Indicator NVARCHAR(50),
	ProjectedStageValue FLOAT,
	ActualStageValue FLOAT NULL
)
AS
BEGIN
		INSERT INTO @TempTable
		SELECT NULL, NULL, NULL, PlanId, Indicator, ProjectedStageValue ,NULL
		FROM
		(
			SELECT [Plan].PlanId, 
			[dbo].[PlanIndicatorProjectedValue]([Plan].PlanId,[Model].ModelId, [Plan].GoalType,RuleEntityTable.Indicator,[Model].ClientId) AS ProjectedStageValue 
			,RuleEntityTable.Indicator
			FROM @TempEntityTable RuleEntityTable 
			CROSS APPLY (SELECT PlanId,ModelId,GoalType From [Plan] WITH (NOLOCK) WHERE [Plan].PlanId = RuleEntityTable.EntityId AND [Plan].IsDeleted=0) [Plan]
			CROSS APPLY (SELECT ModelId,ClientId From [Model] WITH (NOLOCK) WHERE [Model].ModelId = [Plan].ModelId AND [Model].IsDeleted=0) [Model]
			WHERE RuleEntityTable.EntityType = 'Plan'  
			GROUP BY [Plan].PlanId,RuleEntityTable.Indicator,Model.ModelId,Model.ClientId,[Plan].GoalType
		) P
		GROUP BY PlanId, Indicator, ProjectedStageValue
		RETURN 
END
GO

-- ===================================================
-- Author:		Arpita Soni
-- Create date: 08/08/2016
-- Description:	Run alert rules and generate alerts
-- ===================================================
-- DROP AND CREATE STORED PROCEDURE [dbo].[RunAlertRules]
IF EXISTS ( SELECT  * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[RunAlertRules]') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE [dbo].[RunAlertRules]
END
GO
CREATE PROCEDURE [dbo].[RunAlertRules]
AS
BEGIN
	SET NOCOUNT ON;
	
	BEGIN TRY
		-- Constant variables
		DECLARE @txtLessThan	NVARCHAR(20) = 'less than',
				@txtGreaterThan NVARCHAR(20) = 'greater than',
				@txtEqualTo		NVARCHAR(20) = 'equal to'

		DECLARE @TacticsDataForRules			NVARCHAR(MAX) = '',
				@UPDATEQUERYCOMMON				NVARCHAR(MAX) = '',
				@UpdatePlanQuery				NVARCHAR(MAX) = '',
				@UpdateProjectedValuesForPlan	NVARCHAR(MAX) = '',
				@UpdateCampaignQuery			NVARCHAR(MAX) = '',
				@UpdateProgramQuery				NVARCHAR(MAX) = '',
				@UpdateTacticQuery				NVARCHAR(MAX) = '',
				@UpdateLineItemQuery			NVARCHAR(MAX) = '',
				@CalculatePercentGoalQuery		NVARCHAR(MAX) = '',
				@INSERTALERTQUERYCOMMON			NVARCHAR(MAX) = '',
				@InsertQueryForLT				NVARCHAR(MAX) = '',
				@InsertQueryForGT				NVARCHAR(MAX) = '',
				@InsertQueryForEQ				NVARCHAR(MAX) = '',
				@CommonQueryToIgnoreDuplicate	NVARCHAR(MAX) = ''
				

		-- Get projected and actual values of tactic belongs to plan/campaign/program
		SET @TacticsDataForRules = 'DECLARE @TempEntityTable [TacticForRuleEntities];

									-- Get entities from the rule which have reached the completion goal
									INSERT INTO @TempEntityTable([RuleId],[EntityId],[EntityType],[Indicator],[IndicatorComparision],[IndicatorGoal],[CompletionGoal],
																 [Frequency],[DayOfWeek],[DateOfMonth],[UserId],[ClientId],
																 [EntityTitle],[StartDate],[EndDate],[PercentComplete]) 
									SELECT Entity.* FROM dbo.GetEntitiesReachedCompletionGoal() Entity 
									WHERE Entity.PercentComplete >= Entity.CompletionGoal

									-- Table with projected and actual values of tactic belongs to plan/campaign/program
									SELECT * INTO #TacticsDataForAllRuleEntities FROM dbo.[GetTacticsForAllRuleEntities](@TempEntityTable)
									'
		
		-- Common query to update projected and actual values of indicators for entities
		SET @UPDATEQUERYCOMMON =  ';	UPDATE A SET A.ProjectedStageValue = ISNULL(B.ProjectedStageValue,0), A.ActualStageValue = ISNULL(B.ActualStageValue,0)
										FROM @TempEntityTable A INNER JOIN  
										#TacticsDataForAllRuleEntities B
										ON A.EntityId = B.EntityId AND A.EntityType = B.EntityType AND A.Indicator = B.Indicator
										'
		-- Update IndicatorTitle based on Indicator Code
		DECLARE @UpdateIndicatorTitle NVARCHAR(MAX) = ' 
														UPDATE A SET A.IndicatorTitle = dbo.GetIndicatorTitle(A.Indicator,B.ClientId)
														FROM @TempEntityTable A 
														INNER JOIN vClientWise_EntityList B ON A.EntityId = B.EntityId AND A.EntityType = REPLACE(B.Entity,'' '','''')
														'

		-- For plan update projected value using different calculation rest of PLANNEDCOST
		SET @UpdateProjectedValuesForPlan = ';  UPDATE A SET A.ProjectedStageValue = ISNULL(B.ProjectedStageValue,0)
												FROM @TempEntityTable A INNER JOIN
												[dbo].[ProjectedValuesForPlans](@TempEntityTable) B ON A.EntityId = B.PlanId  
												AND A.Indicator = B.Indicator AND A.EntityType = ''Plan''
												AND A.Indicator != ''PLANNEDCOST''
												'
		-- Convert percent of goal from Projected and Actual values
		SET @CalculatePercentGoalQuery = ' UPDATE @TempEntityTable SET CalculatedPercentGoal = 
											CASE WHEN ProjectedStageValue = 0 AND ISNULL(ActualStageValue,0) = 0 THEN 0 
												 WHEN (ProjectedStageValue = 0 AND ISNULL(ActualStageValue,0) != 0) OR (ActualStageValue * 100 / ProjectedStageValue) > 100 THEN 100 
												 ELSE ISNULL(ActualStageValue,0) * 100 / ProjectedStageValue END ;
										   SELECT * FROM @TempEntityTable 
										   '
		-- Common query to create alerts
		SET @INSERTALERTQUERYCOMMON = '	SELECT RuleId, ##DESCRIPTION## AS [Description], UserId,
										(CASE WHEN Frequency = ''WEEKLY'' THEN
											DATEADD(DAY,
											CASE WHEN DATEDIFF(DAY,DATEPART(dw,GETDATE()),DayOfWeek+1) < 0 THEN
												DATEDIFF(DAY,DATEPART(dw,GETDATE()),DayOfWeek + 1) + 7
											ELSE 
												DATEDIFF(DAY,DATEPART(dw,GETDATE()),DayOfWeek + 1) END
											,GETDATE()) 
										WHEN Frequency = ''MONTHLY'' THEN
											CASE WHEN DATEDIFF(DAY,DATEPART(DAY,GETDATE()),DateOfMonth) < 0 THEN
												DATEADD(MONTH,1,DATEADD(DAY,DATEDIFF(DAY,DATEPART(DAY,GETDATE()),DateOfMonth),GETDATE()))
											ELSE 
												DATEADD(DAY,DATEDIFF(DAY,DATEPART(DAY,GETDATE()),DateOfMonth),GETDATE())  END
										ELSE GETDATE() END ) AS DisplayDate										
										FROM @TempEntityTable '

		-- For less than rule
		DECLARE @LessThanWhere		NVARCHAR(MAX) = ' WHERE CalculatedPercentGoal < IndicatorGoal AND IndicatorComparision = ''LT'' AND 
															ISNULL(ProjectedStageValue,0) != 0 AND ISNULL(ActualStageValue,0) != 0 '
		-- For greater than rule
		DECLARE @GreaterThanWhere	NVARCHAR(MAX) = ' WHERE CalculatedPercentGoal > IndicatorGoal AND IndicatorComparision = ''GT'' AND 
															ISNULL(ProjectedStageValue,0) != 0 AND ISNULL(ActualStageValue,0) != 0 '
		-- For equal to rule
		DECLARE @EqualToWhere		NVARCHAR(MAX) = ' WHERE CalculatedPercentGoal = IndicatorGoal AND IndicatorComparision = ''EQ'' AND 
															ISNULL(ProjectedStageValue,0) != 0 AND ISNULL(ActualStageValue,0) != 0 '

		SET @InsertQueryForLT = REPLACE(@INSERTALERTQUERYCOMMON, '##DESCRIPTION##', ' EntityTitle +''''''s ''+ IndicatorTitle +'' is ' + @txtLessThan +' '' + CAST(IndicatorGoal AS NVARCHAR) + ''% of the goal'' ' ) + @LessThanWhere
		SET @InsertQueryForGT = REPLACE(@INSERTALERTQUERYCOMMON, '##DESCRIPTION##', ' EntityTitle +''''''s ''+ IndicatorTitle +'' is ' + @txtGreaterThan +' '' + CAST(IndicatorGoal AS NVARCHAR) + ''% of the goal'' ' ) + @GreaterThanWhere
		SET @InsertQueryForEQ = REPLACE(@INSERTALERTQUERYCOMMON, '##DESCRIPTION##', ' EntityTitle +''''''s ''+ IndicatorTitle +'' is ' + @txtEqualTo +' '' + CAST(IndicatorGoal AS NVARCHAR) + ''% of the goal'' ' ) + @EqualToWhere
		
		SET @CommonQueryToIgnoreDuplicate = '	MERGE INTO [dbo].Alerts AS T1
												USING
												(##INSERTQUERY##) AS T2
												ON (T2.RuleId = T1.RuleId AND T2.Description = T1.Description AND T2.UserId = T1.UserId)
												WHEN NOT MATCHED THEN  
												INSERT ([RuleId],[Description],[UserId],[DisplayDate])
												VALUES ([RuleId],[Description],[UserId],[DisplayDate]) ; '

		SET @InsertQueryForLT = REPLACE(@CommonQueryToIgnoreDuplicate, '##INSERTQUERY##', @InsertQueryForLT)
		SET @InsertQueryForGT = REPLACE(@CommonQueryToIgnoreDuplicate, '##INSERTQUERY##', @InsertQueryForGT)
		SET @InsertQueryForEQ = REPLACE(@CommonQueryToIgnoreDuplicate, '##INSERTQUERY##', @InsertQueryForEQ)
		
		EXEC (@TacticsDataForRules + @UPDATEQUERYCOMMON + @UpdateIndicatorTitle + @UpdateProjectedValuesForPlan + 
				@CalculatePercentGoalQuery + @InsertQueryForLT + @InsertQueryForGT +@InsertQueryForEQ)
		
	END TRY
	BEGIN CATCH
		--Get the details of the error
		 DECLARE   @ErMessage NVARCHAR(2048),
				   @ErSeverity INT,
				   @ErState INT
 
		 SELECT @ErMessage = ERROR_MESSAGE(), @ErSeverity = ERROR_SEVERITY(), @ErState = ERROR_STATE()
 
		 RAISERROR (@ErMessage, @ErSeverity, @ErState)
	END CATCH 
END

GO
/*===================================================================================
Completed By : Arpita Soni
===================================================================================*/
/****** Object:  StoredProcedure [dbo].[SP_Save_AlertRule]    Script Date: 08/20/2016 4:31:26 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_Save_AlertRule]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[SP_Save_AlertRule]
GO
/****** Object:  View [dbo].[vClientWise_EntityList]    Script Date: 08/20/2016 4:31:26 PM ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vClientWise_EntityList]'))
DROP VIEW [dbo].[vClientWise_EntityList]
GO
/****** Object:  View [dbo].[vClientWise_EntityList]    Script Date: 08/20/2016 4:31:26 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vClientWise_EntityList]'))
EXEC dbo.sp_executesql @statement = N'
CREATE VIEW [dbo].[vClientWise_EntityList] AS
WITH AllPlans AS(
SELECT P.PlanId EntityId, P.Title EntityTitle, M.ClientId, ''Plan'' Entity, 1 EntityOrder 
FROM [Plan] P 
INNER JOIN Model M ON M.ModelId = P.ModelId AND P.IsDeleted = 0
WHERE  M.IsDeleted = 0
),
AllCampaigns AS
(
       SELECT P.PlanCampaignId EntityId, P.Title EntityTitle,C.ClientId, ''Campaign'' Entity, 2 EntityOrder 
       FROM Plan_Campaign P
              INNER JOIN AllPlans C ON P.PlanId = C.EntityId 
       WHERE P.IsDeleted = 0
),
AllProgram AS
(
       SELECT P.PlanProgramId EntityId, P.Title EntityTitle,C.ClientId, ''Program'' Entity, 3 EntityOrder 
       FROM Plan_Campaign_Program P
              INNER JOIN AllCampaigns C ON P.PlanCampaignId = C.EntityId 
       WHERE P.IsDeleted = 0
),
AllLinkedTactic as
(
SELECT P.LinkedTacticId 
       FROM Plan_Campaign_Program_Tactic P
              INNER JOIN AllProgram C ON P.PlanProgramId = C.EntityId 
       WHERE P.IsDeleted = 0 and P.Status in (''In-Progress'',''Approved'',''Complete'') and P.LinkedTacticId is not null
	   and (DATEPART(year,P.EndDate)-DATEPART(year,P.StartDate))>0
),
AllTactic AS
(
       SELECT P.PlanTacticId EntityId, P.Title EntityTitle,C.ClientId, ''Tactic'' Entity, 4 EntityOrder 
       FROM Plan_Campaign_Program_Tactic P
              INNER JOIN AllProgram C ON P.PlanProgramId = C.EntityId 
			  LEFT OUTER JOIN AllLinkedTactic L on P.PlanTacticId=L.LinkedTacticId
       WHERE P.IsDeleted = 0 and P.Status in (''In-Progress'',''Approved'',''Complete'') and L.LinkedTacticId is null
),
AllLineitem AS
(
       SELECT P.PlanLineItemId EntityId, P.Title EntityTitle, C.ClientId, ''Line Item'' Entity, 5 EntityOrder 
       FROM Plan_Campaign_Program_Tactic_LineItem P
              INNER JOIN AllTactic C ON P.PlanTacticId = C.EntityId 
       WHERE P.IsDeleted = 0 and P.LineItemTypeId is not null
)
SELECT * FROM AllPlans
UNION ALL 
SELECT * FROM AllCampaigns
UNION ALL 
SELECT * FROM AllProgram
UNION ALL 
SELECT * FROM AllTactic
UNION ALL 
SELECT * FROM AllLineitem
' 
GO
---------------Add column UniqueRuleCode to table Alert_Rules
IF Not EXISTS (SELECT *   FROM   sys.columns   WHERE  object_id = OBJECT_ID(N'[dbo].[Alert_Rules]') AND name = 'UniqueRuleCode')
Begin
	ALTER TABLE Alert_Rules 
	ADD UniqueRuleCode nvarchar(500)
End
---------
Go
/****** Object:  StoredProcedure [dbo].[SP_Save_AlertRule]    Script Date: 08/20/2016 4:31:26 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_Save_AlertRule]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[SP_Save_AlertRule] AS' 
END
GO
-- =============================================
-- Author:		Devanshi gandhi
-- Create date: 20-08-2016
-- Description:	method to save Alert rule 
-- =============================================
ALTER PROCEDURE [dbo].[SP_Save_AlertRule]

	@ClientId NVARCHAR(255)  ,
	@RuleId int,
	@RuleSummary nvarchar(max),
	@EntityId int,
	@EntityType nvarchar(100),
	@Indicator nvarchar(50),
	@IndicatorComparision nvarchar(10),
	@IndicatorGoal int,
	@CompletionGoal int,
	@Frequency nvarchar(50),
	@DayOfWeek tinyint=null,
	@DateOfMonth tinyint=null,
	@UserId NVARCHAR(255),
	@CreatedBy NVARCHAR(255),
	@ModifiedBy  NVARCHAR(255),
	@IsExists int Output

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	Declare @UniqueRule nvarchar(max)
	Declare @FrequencyValue nvarchar(100)=null
	if(@DayOfWeek is not null and @DateOfMonth is null)
		set @FrequencyValue=@DayOfWeek
	else if(@DayOfWeek is null and @DateOfMonth is not null)
		set @FrequencyValue=@DateOfMonth

	set @UniqueRule=CONVERT(nvarchar(50),@EntityId)+'_'+CONVERT(nvarchar(50),@Indicator)+'_'+CONVERT(nvarchar(50),@IndicatorComparision)+'_'+CONVERT(nvarchar(50),@IndicatorGoal)+'_'+CONVERT(nvarchar(50),@CompletionGoal)+'_'+CONVERT(nvarchar(50),@Frequency)
	if(@FrequencyValue is not null)
		set @UniqueRule=@UniqueRule+'_'+@FrequencyValue
	
	If(@RuleId!=0)
	Begin
		If not exists (Select RuleId from Alert_Rules where UniqueRuleCode=@UniqueRule and RuleId!=@RuleId)
		Begin
			Update Alert_Rules set EntityId=@EntityId,EntityType=@EntityType,Indicator=@Indicator,IndicatorComparision=@IndicatorComparision,IndicatorGoal=@IndicatorGoal,
			CompletionGoal=@CompletionGoal,Frequency=@Frequency,DateOfMonth=@DateOfMonth,DayOfWeek=@DayOfWeek,ModifiedBy=@ModifiedBy,ModifiedDate=GETDATE(),
			RuleSummary=@RuleSummary,LastProcessingDate=GETDATE(),UniqueRuleCode=@UniqueRule
			where RuleId=@RuleId
			set @IsExists=0
		End
		Else
		set @IsExists=1
	End
	Else
	Begin
		If not exists (Select RuleId from Alert_Rules where UniqueRuleCode=@UniqueRule)
		Begin
			Insert into Alert_Rules (RuleSummary,EntityId,EntityType,Indicator,IndicatorComparision,IndicatorGoal,CompletionGoal,Frequency,DayOfWeek,DateOfMonth,LastProcessingDate,
				UserId,ClientId,IsDisabled,CreatedDate,CreatedBy,ModifiedDate,ModifiedBy,UniqueRuleCode)
			values(@RuleSummary,@EntityId,@EntityType,@Indicator,@IndicatorComparision,@IndicatorGoal,@CompletionGoal,@Frequency,@DayOfWeek,@DateOfMonth,GETDATE(),
				@UserId,@ClientId,0,GETDATE(),@CreatedBy,null,null,@UniqueRule)
			set @IsExists=0
		End
		Else
		set @IsExists=1
	End
	
END

--exec SP_Save_AlertRule '464eb808-ad1f-4481-9365-6aada15023bd',25,'<h4>Test Plan eCopy_Test_cases  Closed Won are greater than 50% of Goal</h4><span>Start at 50% completion</span><span>Repeat Weekly</span>',295,'Plan','CW','GT',50,50,'Weekly',4,null,'14d7d588-cf4d-46be-b4ed-a74063b67d66','14d7d588-cf4d-46be-b4ed-a74063b67d66','14d7d588-cf4d-46be-b4ed-a74063b67d66',0
GO

--Added by devanshi on 23-8-2016
/****** Object:  StoredProcedure [dbo].[UpdateAlert_Notification]    Script Date: 08/23/2016 3:22:01 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateAlert_Notification]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateAlert_Notification]
GO
/****** Object:  StoredProcedure [dbo].[SP_Save_AlertRule]    Script Date: 08/23/2016 3:22:01 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_Save_AlertRule]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[SP_Save_AlertRule]
GO
/****** Object:  StoredProcedure [dbo].[GetClientEntityList]    Script Date: 08/23/2016 3:22:01 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetClientEntityList]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetClientEntityList]
GO
/****** Object:  StoredProcedure [dbo].[GetClientEntityList]    Script Date: 08/23/2016 3:22:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetClientEntityList]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[GetClientEntityList] AS' 
END
GO
-- =============================================
-- Author:		Devanshi gandhi
-- Create date: 23-8-2016
-- Description:	get the list of al entities for client for create alert Rule
-- =============================================
ALTER PROCEDURE [dbo].[GetClientEntityList]
	-- Add the parameters for the stored procedure here
	@ClientId nvarchar(255)
AS
BEGIN

	SET NOCOUNT ON;

	select * from [vClientWise_EntityList] where clientid=@ClientId

END

GO
/****** Object:  StoredProcedure [dbo].[SP_Save_AlertRule]    Script Date: 08/23/2016 3:22:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_Save_AlertRule]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[SP_Save_AlertRule] AS' 
END
GO
-- =============================================
-- Author:		Devanshi gandhi
-- Create date: 20-08-2016
-- Description:	method to save Alert rule 
-- =============================================
ALTER PROCEDURE [dbo].[SP_Save_AlertRule]

	@ClientId NVARCHAR(255)  ,
	@RuleId int,
	@RuleSummary nvarchar(max),
	@EntityId int,
	@EntityType nvarchar(100),
	@Indicator nvarchar(50),
	@IndicatorComparision nvarchar(10),
	@IndicatorGoal int,
	@CompletionGoal int,
	@Frequency nvarchar(50),
	@DayOfWeek tinyint=null,
	@DateOfMonth tinyint=null,
	@UserId NVARCHAR(255),
	@CreatedBy NVARCHAR(255),
	@ModifiedBy  NVARCHAR(255),
	@IsExists int Output

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	Declare @UniqueRule nvarchar(max)
	Declare @FrequencyValue nvarchar(100)=null
	if(@DayOfWeek is not null and @DateOfMonth is null)
		set @FrequencyValue=@DayOfWeek
	else if(@DayOfWeek is null and @DateOfMonth is not null)
		set @FrequencyValue=@DateOfMonth

	set @UniqueRule=CONVERT(nvarchar(15),@EntityId)+'_'+CONVERT(nvarchar(15),@Indicator)+'_'+CONVERT(nvarchar(15),@IndicatorComparision)+'_'+CONVERT(nvarchar(15),@IndicatorGoal)+'_'+CONVERT(nvarchar(15),@CompletionGoal)+'_'+CONVERT(nvarchar(15),@Frequency)
	if(@FrequencyValue is not null)
		set @UniqueRule=@UniqueRule+'_'+@FrequencyValue
	
	If(@RuleId!=0)
	Begin --Update existing rule
		If not exists (Select RuleId from Alert_Rules where ClientId=@ClientId and  RuleId!=@RuleId and UniqueRuleCode=@UniqueRule)
		Begin
			Update Alert_Rules set EntityId=@EntityId,EntityType=@EntityType,Indicator=@Indicator,IndicatorComparision=@IndicatorComparision,IndicatorGoal=@IndicatorGoal,
			CompletionGoal=@CompletionGoal,Frequency=@Frequency,DateOfMonth=@DateOfMonth,DayOfWeek=@DayOfWeek,ModifiedBy=@ModifiedBy,ModifiedDate=GETDATE(),
			RuleSummary=@RuleSummary,LastProcessingDate=GETDATE(),UniqueRuleCode=@UniqueRule
			where RuleId=@RuleId
			set @IsExists=0
		End
		Else
		set @IsExists=1
	End
	Else
	Begin -- Isert new alert rule
		If not exists (Select RuleId from Alert_Rules where ClientId=@ClientId and UniqueRuleCode=@UniqueRule)
		Begin
			Insert into Alert_Rules (RuleSummary,EntityId,EntityType,Indicator,IndicatorComparision,IndicatorGoal,CompletionGoal,Frequency,DayOfWeek,DateOfMonth,LastProcessingDate,
				UserId,ClientId,IsDisabled,CreatedDate,CreatedBy,ModifiedDate,ModifiedBy,UniqueRuleCode)
			values(@RuleSummary,@EntityId,@EntityType,@Indicator,@IndicatorComparision,@IndicatorGoal,@CompletionGoal,@Frequency,@DayOfWeek,@DateOfMonth,GETDATE(),
				@UserId,@ClientId,0,GETDATE(),@CreatedBy,null,null,@UniqueRule)
			set @IsExists=0
		End
		Else
		set @IsExists=1
	End
	
END

--exec SP_Save_AlertRule '464eb808-ad1f-4481-9365-6aada15023bd',25,'<h4>Test Plan eCopy_Test_cases  Closed Won are greater than 50% of Goal</h4><span>Start at 50% completion</span><span>Repeat Weekly</span>',295,'Plan','CW','GT',50,50,'Weekly',4,null,'14d7d588-cf4d-46be-b4ed-a74063b67d66','14d7d588-cf4d-46be-b4ed-a74063b67d66','14d7d588-cf4d-46be-b4ed-a74063b67d66',0

GO
/****** Object:  StoredProcedure [dbo].[UpdateAlert_Notification]    Script Date: 08/23/2016 3:22:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateAlert_Notification]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[UpdateAlert_Notification] AS' 
END
GO
-- =============================================
-- Author:		Devanshi gandhi
-- Create date: 16-8-2016
-- Description:	method to update isready field for Alert and Notification
-- =============================================
ALTER PROCEDURE [dbo].[UpdateAlert_Notification]
	@UserId UniqueIdentifier,
	@Type nvarchar(100)

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	Declare @ReadDate datetime = GETDATE()
	If(@Type='alert')
	Begin
		Update Alerts set IsRead=1,ReadDate=@ReadDate where UserId=@UserId and  convert(date,DisplayDate)<=convert(date,GETDATE())
	End
	Else If(@Type='notification')
	Begin
		
		Update user_notification_messages set IsRead=1 , ReadDate=@ReadDate where RecipientId=@UserId
	End
END


GO

-- Added By : Viral Kadiya
-- Added Date : 08/24/2016
-- Description :Added "LineItemTypeId" column for LineItem query.


/****** Object:  StoredProcedure [dbo].[Plan_Budget_Cost_Actual_Detail]    Script Date: 08/25/2016 5:16:22 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Plan_Budget_Cost_Actual_Detail]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[Plan_Budget_Cost_Actual_Detail] AS' 
END
GO
-- =============================================
-- Author:		Mitesh Vaishnav
-- Create date: 29th Jun 2016
-- Description:	Sp return datatable which contains plan,Campaign,Program,tactic and line item details with respective budget, cost and actual valus 
-- =============================================
ALTER PROCEDURE [dbo].[Plan_Budget_Cost_Actual_Detail]
( 
@PlanId INT ,
@UserId NVARCHAR(36),
@SelectedTab NVARCHAR(50)
)
AS
BEGIN
	

--If tab is planned then planned cost value return in query

IF (@SelectedTab='Planned')

	BEGIN

	SELECT Id
,ActivityId
,ActivityName
,ActivityType
,ParentActivityId
,MainBudgeted
,IsOwner
,CreatedBy
,IsAfterApproved
,IsEditable
,MainBudgeted as Cost
,[Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12],[Y1] as [CY1],[Y2] as [CY2],[Y3] as [CY3],[Y4] as [CY4],[Y5] as [CY5],[Y6] as [CY6],[Y7] as [CY7],[Y8] as [CY8],[Y9] as [CY9],[Y10] as [CY10],[Y11] as [CY11],[Y12] as [CY12]
,ISNULL([Y1],0)+ISNULL([Y2],0)+ISNULL([Y3],0)+ISNULL([Y4],0)+ISNULL([Y5],0)+ISNULL([Y6],0)+ISNULL([Y7],0)+ISNULL([Y8],0)+ISNULL([Y9],0)+ISNULL([Y10],0)+ISNULL([Y11],0)+ISNULL([Y12],0) TotalBudgetSum
,0 TotalCostSum
,0 as LineItemTypeId
 FROM
			(
			SELECT 
					CAST(P.PlanId as NVARCHAR(20)) as Id
					,'plan_'+CAST(P.PlanId AS NVARCHAR(20)) as ActivityId
					,P.Title as ActivityName 
					,'plan' as ActivityType
					,'0' ParentActivityId
					,Budget as MainBudgeted
					,1 as IsOwner
					,P.CreatedBy
					,0 as IsAfterApproved
					,0 as IsEditable
					,Value 
					,Period
					
					
			  FROM [Plan] P
			  LEFT JOIN  plan_budget PB on P.PlanId=PB.PlanId
			  WHERE P.PlanId = @PlanId
			) Plan_Main
pivot
(
   sum(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) planDetails
UNION ALL

SELECT Id
,ActivityId
,ActivityName
,ActivityType
,ParentActivityId
,MainBudgeted
,IsOwner
,CreatedBy
,IsAfterApproved
,IsEditable
,Cost
,[Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12],[Y1] as [CY1],[Y2] as [CY2],[Y3] as [CY3],[Y4] as [CY4],[Y5] as [CY5],[Y6] as [CY6],[Y7] as [CY7],[Y8] as [CY8],[Y9] as [CY9],[Y10] as [CY10],[Y11] as [CY11],[Y12] as [CY12]
,ISNULL([Y1],0)+ISNULL([Y2],0)+ISNULL([Y3],0)+ISNULL([Y4],0)+ISNULL([Y5],0)+ISNULL([Y6],0)+ISNULL([Y7],0)+ISNULL([Y8],0)+ISNULL([Y9],0)+ISNULL([Y10],0)+ISNULL([Y11],0)+ISNULL([Y12],0) TotalBudgetSum
,0 TotalCostSum
,0 as LineItemTypeId
 FROM
(
 
  SELECT 
		 'c_'+CAST(PC.PlanCampaignId as NVARCHAR(20)) as ActivityId
		 ,CAST(PC.PlanCampaignId as NVARCHAR(20))  Id
		 ,PC.Title as ActivityName
		 ,'campaign' as ActivityType
		 ,'plan_'+CAST(@PlanId as NVARCHAR(25)) ParentActivityId
		 ,CASE WHEN @UserId=PC.CreatedBy THEN 1 ELSE 0 END IsOwner
		 ,CampaignBudget as MainBudgeted
		 ,PC.CreatedBy
		 ,0 as IsAfterApproved
		 ,0 as IsEditable
		 ,value
		 ,period
		 ,0 Cost
		  FROM Plan_Campaign PC
		  LEFT JOIN Plan_Campaign_Budget PCB ON PC.planCampaignid = PCB.PlanCampaignId where PC.PlanId = @PlanId and IsDeleted = 0
  
) Campaign_Main
pivot
(
  sum(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) planCampaignDetails
UNION ALL
SELECT Id
,ActivityId
,ActivityName
,ActivityType
,ParentActivityId
,MainBudgeted
,IsOwner
,CreatedBy
,IsAfterApproved
,IsEditable
,Cost
,[Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12],[Y1] as [CY1],[Y2] as [CY2],[Y3] as [CY3],[Y4] as [CY4],[Y5] as [CY5],[Y6] as [CY6],[Y7] as [CY7],[Y8] as [CY8],[Y9] as [CY9],[Y10] as [CY10],[Y11] as [CY11],[Y12] as [CY12]
,ISNULL([Y1],0)+ISNULL([Y2],0)+ISNULL([Y3],0)+ISNULL([Y4],0)+ISNULL([Y5],0)+ISNULL([Y6],0)+ISNULL([Y7],0)+ISNULL([Y8],0)+ISNULL([Y9],0)+ISNULL([Y10],0)+ISNULL([Y11],0)+ISNULL([Y12],0) TotalBudgetSum 
,0 TotalCostSum 
,0 as LineItemTypeId

FROM
(
SELECT   CAST(PG.PlanProgramId as NVARCHAR(20)) Id,
		 'cp_'+CAST(PG.PlanProgramId as NVARCHAR(20)) ActivityId
		,PG.Title as ActivityName
		,'program' as ActivityType
		,'c_'+CAST(PG.PlanCampaignId as NVARCHAR(25)) ParentActivityId
		,CASE WHEN @UserId=PG.CreatedBy THEN 1 ELSE 0 END IsOwner
		,PG.ProgramBudget as MainBudgeted
		,PG.CreatedBy
		,0 as IsAfterApproved
		,0 as IsEditable
		,Value,Period
		,0 Cost
		FROM Plan_Campaign_Program PG
		INNER JOIN Plan_Campaign PC ON PC.PlanCampaignId=PG.PlanCampaignId AND PC.IsDeleted=0
		LEFT JOIN Plan_Campaign_Program_Budget PGB ON PG.PlanProgramId=PGB.PlanProgramId
		WHERE PC.PlanId=@PlanId AND PG.IsDeleted=0 AND PC.IsDeleted=0
  
) Program_Main
pivot
(
  sum(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) PlanCampaignProgramDetails
UNION ALL

select Id
,ActivityId
,ActivityName
,ActivityType
,ParentActivityId
,MainBudgeted
,IsOwner
,CreatedBy
,IsAfterApproved
,IsEditable
,Cost
,[Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12],[CY1],[CY2],[CY3],[CY4],[CY5],[CY6],[CY7],[CY8],[CY9],[CY10],[CY11],[CY12]

,ISNULL([CY1],0)+ISNULL([CY2],0)+ISNULL([CY3],0)+ISNULL([CY4],0)+ISNULL([CY5],0)+ISNULL([CY6],0)+ISNULL([CY7],0)+ISNULL([CY8],0)+ISNULL([CY9],0)+ISNULL([CY10],0)+ISNULL([CY11],0)+ISNULL([CY12],0) TotalBudgetSum
,ISNULL([Y1],0)+ISNULL([Y2],0)+ISNULL([Y3],0)+ISNULL([Y4],0)+ISNULL([Y5],0)+ISNULL([Y6],0)+ISNULL([Y7],0)+ISNULL([Y8],0)+ISNULL([Y9],0)+ISNULL([Y10],0)+ISNULL([Y11],0)+ISNULL([Y12],0)  TotalCostSum
,0 as LineItemTypeId 
FROM
(
 
 SELECT	 CAST(PT.PlanTacticId AS NVARCHAR(20)) Id
		,'cpt_'+CAST(PT.PlanTacticId AS NVARCHAR(20)) ActivityId
		,PT.Title AS ActivityName
		,'tactic' ActivityType
		,'cp_'+CAST(PPG.PlanProgramId AS NVARCHAR(25)) ParentActivityId
		,CASE WHEN PT.CreatedBy=@UserId THEN 1 ELSE 0 END IsOwner
		,PT.TacticBudget AS MainBudgeted
		,PT.CreatedBy 
		,0 as IsEditable
		,CASE WHEN PT.[Status] in ('Approved','In-Progress','Complete') THEN 1 ELSE 0 END IsAfterApproved
		,PTB.Value
		,PTB.Period
		,PT.Cost
		,'C'+PTCst.Period as CPeriod
		,PTCst.Value as CValue
FROM 
	Plan_Campaign_Program_Tactic PT
	INNER JOIN Plan_Campaign_Program PPG ON PPG.PlanProgramId=PT.PlanProgramId 
	INNER JOIN Plan_Campaign PC ON PC.PlanCampaignId=PPG.PlanCampaignId
	LEFT JOIN Plan_Campaign_Program_Tactic_Budget PTB ON PT.PlanTacticId=PTB.PlanTacticId
	LEFT JOIN Plan_Campaign_Program_Tactic_Cost PTCst ON PT.PlanTacticId=PTCst.PlanTacticId
	WHERE PC.PlanId=@PlanId AND PC.IsDeleted=0 AND PPG.IsDeleted=0 AND PT.IsDeleted=0
) Tactic_Main
pivot
(
  sum(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
 
) PlanCampaignProgramTacticDetails
Pivot
(
sum(CValue)
  for CPeriod in ([CY1], [CY2], [CY3], [CY4],[CY5], [CY6], [CY7], [CY8],[CY9], [CY10], [CY11], [CY12])
)PlanCampaignProgramTacticDetails1
UNION ALL

SELECT Id
,ActivityId
,ActivityName
,ActivityType
,ParentActivityId
,MainBudgeted
,IsOwner
,CreatedBy
,IsAfterApproved
,IsEditable
,Cost
,0 [Y1],0 [Y2],0 [Y3],0 [Y4],0 [Y5],0 [Y6],0 [Y7],0 [Y8],0 [Y9],0 [Y10],0 [Y11],0 [Y12],[CY1],[CY2],[CY3],[CY4],[CY5],[CY6],[CY7],[CY8],[CY9],[CY10],[CY11],[CY12]
,ISNULL([CY1],0)+ISNULL([CY2],0)+ISNULL([CY3],0)+ISNULL([CY4],0)+ISNULL([CY5],0)+ISNULL([CY6],0)+ISNULL([CY7],0)+ISNULL([CY8],0)+ISNULL([CY9],0)+ISNULL([CY10],0)+ISNULL([CY11],0)+ISNULL([CY12],0) TotalBudgetSum
,0 TotalCostSum 
,LineItemTypeId
FROM
(
SELECT 
		CAST(PL.PlanLineItemId as NVARCHAR(20)) Id
		,'cptl_'+CAST(PL.PlanLineItemId as NVARCHAR(20)) ActivityId
		,PL.Title as ActivityName
		,'lineitem' ActivityType
		,'cpt_'+CAST(PL.PlanTacticId as NVARCHAR(25)) ParentActivityId
		,PL.Cost
		,0 MainBudgeted
		,CASE WHEN PL.CreatedBy=@UserId THEN 1 ELSE 0 END IsOwner
		,PL.CreatedBy
		,0 as IsEditable
		,CASE WHEN PT.[Status] in ('Approved','In-Progress','Complete') THEN 1 ELSE 0 END IsAfterApproved
		,PLC.Value
		,'C'+PLC.period as period 
		,PL.LineItemTypeId as LineItemTypeId
	FROM Plan_Campaign_Program_Tactic_LineItem PL
	INNER JOIN Plan_Campaign_Program_Tactic PT ON PT.PlanTacticId=PL.PlanTacticId
	INNER JOIN Plan_Campaign_Program PPG ON PT.PlanProgramId=PPG.PlanProgramId
	INNER JOIN Plan_Campaign PC ON PC.PlanCampaignId=PPG.PlanCampaignId
	LEFT JOIN Plan_Campaign_Program_Tactic_LineItem_Cost PLC ON PL.PlanLineItemId=PLC.PlanLineItemId
	WHERE PC.PlanId=@PlanId AND PC.IsDeleted=0 AND PPG.IsDeleted=0 AND PT.IsDeleted=0 AND PL.IsDeleted=0
)LineItem_Main
Pivot
(
sum (Value)
For Period in ([CY1], [CY2], [CY3], [CY4],[CY5], [CY6], [CY7], [CY8],[CY9], [CY10], [CY11], [CY12])
)PivotLineItem
	END

--If tab is Actual then Actual values return in query 
ELSE 

	BEGIN

		SELECT Id
,ActivityId
,ActivityName
,ActivityType
,ParentActivityId
,MainBudgeted
,IsOwner
,CreatedBy
,IsAfterApproved
,IsEditable
,MainBudgeted as Cost
,[Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12],[Y1] as [CY1],[Y2] as [CY2],[Y3] as [CY3],[Y4] as [CY4],[Y5] as [CY5],[Y6] as [CY6],[Y7] as [CY7],[Y8] as [CY8],[Y9] as [CY9],[Y10] as [CY10],[Y11] as [CY11],[Y12] as [CY12]
,ISNULL([Y1],0)+ISNULL([Y2],0)+ISNULL([Y3],0)+ISNULL([Y4],0)+ISNULL([Y5],0)+ISNULL([Y6],0)+ISNULL([Y7],0)+ISNULL([Y8],0)+ISNULL([Y9],0)+ISNULL([Y10],0)+ISNULL([Y11],0)+ISNULL([Y12],0) TotalBudgetSum
,0 TotalCostSum
,0 as LineItemTypeId
 FROM
			(
			SELECT 
					CAST(P.PlanId as NVARCHAR(20)) as Id
					,'plan_'+CAST(P.PlanId AS NVARCHAR(20)) as ActivityId
					,P.Title as ActivityName 
					,'plan' as ActivityType
					,'0' ParentActivityId
					,Budget as MainBudgeted
					,1 as IsOwner
					,P.CreatedBy
					,0 as IsAfterApproved
					,0 as IsEditable
					,Value 
					,Period
					
					
			  FROM [Plan] P
			  LEFT JOIN  plan_budget PB on P.PlanId=PB.PlanId
			  WHERE P.PlanId = @PlanId
			) Plan_Main
pivot
(
   sum(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) planDetails
UNION ALL

SELECT Id
,ActivityId
,ActivityName
,ActivityType
,ParentActivityId
,MainBudgeted
,IsOwner
,CreatedBy
,IsAfterApproved
,IsEditable
,Cost
,[Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12],[Y1] as [CY1],[Y2] as [CY2],[Y3] as [CY3],[Y4] as [CY4],[Y5] as [CY5],[Y6] as [CY6],[Y7] as [CY7],[Y8] as [CY8],[Y9] as [CY9],[Y10] as [CY10],[Y11] as [CY11],[Y12] as [CY12]
,ISNULL([Y1],0)+ISNULL([Y2],0)+ISNULL([Y3],0)+ISNULL([Y4],0)+ISNULL([Y5],0)+ISNULL([Y6],0)+ISNULL([Y7],0)+ISNULL([Y8],0)+ISNULL([Y9],0)+ISNULL([Y10],0)+ISNULL([Y11],0)+ISNULL([Y12],0) TotalBudgetSum
,0 TotalCostSum
,0 as LineItemTypeId
 FROM
(
 
  SELECT 
		 'c_'+CAST(PC.PlanCampaignId as NVARCHAR(20)) as ActivityId
		 ,CAST(PC.PlanCampaignId as NVARCHAR(20))  Id
		 ,PC.Title as ActivityName
		 ,'campaign' as ActivityType
		 ,'plan_'+CAST(@PlanId as NVARCHAR(25)) ParentActivityId
		 ,CASE WHEN @UserId=PC.CreatedBy THEN 1 ELSE 0 END IsOwner
		 ,CampaignBudget as MainBudgeted
		 ,PC.CreatedBy
		 ,0 as IsAfterApproved
		 ,0 as IsEditable
		 ,value
		 ,period
		 ,0 Cost
		  FROM Plan_Campaign PC
		  LEFT JOIN Plan_Campaign_Budget PCB ON PC.planCampaignid = PCB.PlanCampaignId where PC.PlanId = @PlanId and IsDeleted = 0
  
) Campaign_Main
pivot
(
  sum(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) planCampaignDetails
UNION ALL
SELECT Id
,ActivityId
,ActivityName
,ActivityType
,ParentActivityId
,MainBudgeted
,IsOwner
,CreatedBy
,IsAfterApproved
,IsEditable
,Cost
,[Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12],[Y1] as [CY1],[Y2] as [CY2],[Y3] as [CY3],[Y4] as [CY4],[Y5] as [CY5],[Y6] as [CY6],[Y7] as [CY7],[Y8] as [CY8],[Y9] as [CY9],[Y10] as [CY10],[Y11] as [CY11],[Y12] as [CY12]
,ISNULL([Y1],0)+ISNULL([Y2],0)+ISNULL([Y3],0)+ISNULL([Y4],0)+ISNULL([Y5],0)+ISNULL([Y6],0)+ISNULL([Y7],0)+ISNULL([Y8],0)+ISNULL([Y9],0)+ISNULL([Y10],0)+ISNULL([Y11],0)+ISNULL([Y12],0) TotalBudgetSum 
,0 TotalCostSum
,0 as LineItemTypeId
 FROM
(
SELECT   CAST(PG.PlanProgramId as NVARCHAR(20)) Id,
		 'cp_'+CAST(PG.PlanProgramId as NVARCHAR(20)) ActivityId
		,PG.Title as ActivityName
		,'program' as ActivityType
		,'c_'+CAST(PG.PlanCampaignId as NVARCHAR(25)) ParentActivityId
		,CASE WHEN @UserId=PG.CreatedBy THEN 1 ELSE 0 END IsOwner
		,PG.ProgramBudget as MainBudgeted
		,PG.CreatedBy
		,0 as IsAfterApproved
		,0 as IsEditable
		,Value,Period
		,0 Cost
		FROM Plan_Campaign_Program PG
		INNER JOIN Plan_Campaign PC ON PC.PlanCampaignId=PG.PlanCampaignId AND PC.IsDeleted=0
		LEFT JOIN Plan_Campaign_Program_Budget PGB ON PG.PlanProgramId=PGB.PlanProgramId
		WHERE PC.PlanId=@PlanId AND PG.IsDeleted=0 AND PC.IsDeleted=0
  
) Program_Main
pivot
(
  sum(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
) PlanCampaignProgramDetails
UNION ALL

select Id
,ActivityId
,ActivityName
,ActivityType
,ParentActivityId
,MainBudgeted
,IsOwner
,CreatedBy
,IsAfterApproved
,IsEditable
,Cost
,[Y1],[Y2],[Y3],[Y4],[Y5],[Y6],[Y7],[Y8],[Y9],[Y10],[Y11],[Y12],[CY1],[CY2],[CY3],[CY4],[CY5],[CY6],[CY7],[CY8],[CY9],[CY10],[CY11],[CY12]

,ISNULL([CY1],0)+ISNULL([CY2],0)+ISNULL([CY3],0)+ISNULL([CY4],0)+ISNULL([CY5],0)+ISNULL([CY6],0)+ISNULL([CY7],0)+ISNULL([CY8],0)+ISNULL([CY9],0)+ISNULL([CY10],0)+ISNULL([CY11],0)+ISNULL([CY12],0) TotalBudgetSum
,ISNULL([Y1],0)+ISNULL([Y2],0)+ISNULL([Y3],0)+ISNULL([Y4],0)+ISNULL([Y5],0)+ISNULL([Y6],0)+ISNULL([Y7],0)+ISNULL([Y8],0)+ISNULL([Y9],0)+ISNULL([Y10],0)+ISNULL([Y11],0)+ISNULL([Y12],0)  TotalCostSum 
,0 as LineItemTypeId
FROM
(
 
 SELECT	 CAST(PT.PlanTacticId AS NVARCHAR(20)) Id
		,'cpt_'+CAST(PT.PlanTacticId AS NVARCHAR(20)) ActivityId
		,PT.Title AS ActivityName
		,'tactic' ActivityType
		,'cp_'+CAST(PPG.PlanProgramId AS NVARCHAR(25)) ParentActivityId
		,CASE WHEN PT.CreatedBy=@UserId THEN 1 ELSE 0 END IsOwner
		,PT.TacticBudget AS MainBudgeted
		,PT.CreatedBy 
		,0 as IsEditable
		,CASE WHEN PT.[Status] in ('Approved','In-Progress','Complete') THEN 1 ELSE 0 END IsAfterApproved
		,PTB.Value
		,PTB.Period
		,PT.Cost
		,'C'+PTAct.Period as CPeriod
		,PTAct.ActualValue as CValue
FROM 
	Plan_Campaign_Program_Tactic PT
	INNER JOIN Plan_Campaign_Program PPG ON PPG.PlanProgramId=PT.PlanProgramId 
	INNER JOIN Plan_Campaign PC ON PC.PlanCampaignId=PPG.PlanCampaignId
	LEFT JOIN Plan_Campaign_Program_Tactic_Budget PTB ON PT.PlanTacticId=PTB.PlanTacticId
	LEFT JOIN Plan_Campaign_Program_Tactic_Actual PTAct ON PT.PlanTacticId=PTAct.PlanTacticId AND PTAct.StageTitle='Cost'
	WHERE PC.PlanId=@PlanId AND PC.IsDeleted=0 AND PPG.IsDeleted=0 AND PT.IsDeleted=0 
) Tactic_Main
pivot
(
  sum(value)
  for period in ([Y1], [Y2], [Y3], [Y4],[Y5], [Y6], [Y7], [Y8],[Y9], [Y10], [Y11], [Y12])
 
) PlanCampaignProgramTacticDetails
Pivot
(
sum(CValue)
  for CPeriod in ([CY1], [CY2], [CY3], [CY4],[CY5], [CY6], [CY7], [CY8],[CY9], [CY10], [CY11], [CY12])
)PlanCampaignProgramTacticDetails1
UNION ALL

SELECT Id
,ActivityId
,ActivityName
,ActivityType
,ParentActivityId
,MainBudgeted
,IsOwner
,CreatedBy
,IsAfterApproved
,IsEditable
,Cost
,0 [Y1],0 [Y2],0 [Y3],0 [Y4],0 [Y5],0 [Y6],0 [Y7],0 [Y8],0 [Y9],0 [Y10],0 [Y11],0 [Y12],[CY1],[CY2],[CY3],[CY4],[CY5],[CY6],[CY7],[CY8],[CY9],[CY10],[CY11],[CY12]
,ISNULL([CY1],0)+ISNULL([CY2],0)+ISNULL([CY3],0)+ISNULL([CY4],0)+ISNULL([CY5],0)+ISNULL([CY6],0)+ISNULL([CY7],0)+ISNULL([CY8],0)+ISNULL([CY9],0)+ISNULL([CY10],0)+ISNULL([CY11],0)+ISNULL([CY12],0) TotalBudgetSum
,0 TotalCostSum 
,LineItemTypeId
FROM
(
SELECT 
		CAST(PL.PlanLineItemId as NVARCHAR(20)) Id
		,'cptl_'+CAST(PL.PlanLineItemId as NVARCHAR(20)) ActivityId
		,PL.Title as ActivityName
		,'lineitem' ActivityType
		,'cpt_'+CAST(PL.PlanTacticId as NVARCHAR(25)) ParentActivityId
		,PL.Cost
		,0 MainBudgeted
		,CASE WHEN PL.CreatedBy=@UserId THEN 1 ELSE 0 END IsOwner
		,PL.CreatedBy
		,0 as IsEditable
		,CASE WHEN PT.[Status] in ('Approved','In-Progress','Complete') THEN 1 ELSE 0 END IsAfterApproved
		,PLC.Value
		,'C'+PLC.period as period 
		,PL.LineItemTypeId as LineItemTypeId
	FROM Plan_Campaign_Program_Tactic_LineItem PL
	INNER JOIN Plan_Campaign_Program_Tactic PT ON PT.PlanTacticId=PL.PlanTacticId
	INNER JOIN Plan_Campaign_Program PPG ON PT.PlanProgramId=PPG.PlanProgramId
	INNER JOIN Plan_Campaign PC ON PC.PlanCampaignId=PPG.PlanCampaignId
	LEFT JOIN Plan_Campaign_Program_Tactic_LineItem_Actual PLC ON PL.PlanLineItemId=PLC.PlanLineItemId
	WHERE PC.PlanId=@PlanId AND PC.IsDeleted=0 AND PPG.IsDeleted=0 AND PT.IsDeleted=0 AND PL.IsDeleted=0
)LineItem_Main
Pivot
(
sum (Value)
For Period in ([CY1], [CY2], [CY3], [CY4],[CY5], [CY6], [CY7], [CY8],[CY9], [CY10], [CY11], [CY12])
)PivotLineItem

	END


END



GO




-- ===========================Please put your script above this script=============================
-- Added By : Maitri Gandhi
-- Added Date : 2/22/2016
-- Description :Ensure versioning table exists & Update versioning table with script version
-- ======================================================================================

IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Versioning'))
BEGIN
CREATE TABLE [dbo].[Versioning](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Release Name] [nvarchar](255) NOT NULL,
	[Date Applied] [datetime] NOT NULL,
	[Version] [nvarchar](255) NOT NULL
) ON [PRIMARY]
END
GO

declare @version nvarchar(255)
declare @release nvarchar(255)
set @release = 'August.2016'
set @version = 'August.2016.1'
declare @date as datetime
set @date = getutcdate()

if (NOT EXISTS(SELECT * FROM [dbo].[Versioning]  WHERE Version = @version))
BEGIN
insert into [dbo].[Versioning] values (@release, @date, @version)
END
GO
