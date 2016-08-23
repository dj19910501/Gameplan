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
	 insert into Notification(NotificationInternalUseOnly,Title,[Description],NotificationType,EmailContent,IsDeleted,CreatedDate,CreatedBy,ModifiedDate,ModifiedBy,[Subject]) values('TacticIsEdited','When my tactic is edited',null,'AM','Dear [NameToBeReplaced],<br/><br/>Please note that following tactic has been edited.<br><br><table><tr><td>Tactic Name</td><td>:</td><td>[TacticNameToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Edited by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr></table><br><br>Thank You,<br>Hive9 Plan Admin',0,GETDATE(),@Createdby,null,null,'Plan :Tactic is edited')
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
	 insert into Notification(NotificationInternalUseOnly,Title,[Description],NotificationType,EmailContent,IsDeleted,CreatedDate,CreatedBy,ModifiedDate,ModifiedBy,[Subject]) values('CampaignIsEdited','When my campaign is edited',null,'AM','Dear [NameToBeReplaced],<br/><br/>Please note that following campaign has been edited.<br><br><table><tr><td>Campaign Name</td><td>:</td><td>[CampaignNameToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Edited by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr></table><br><br>Thank You,<br>Hive9 Plan Admin',0,GETDATE(),@Createdby,null,null,'Plan : Campaign is edited')
	End

	IF (NOT EXISTS(SELECT * FROM Notification WHERE NotificationInternalUseOnly = 'ProgramIsEdited')) 
	Begin
	 insert into Notification(NotificationInternalUseOnly,Title,[Description],NotificationType,EmailContent,IsDeleted,CreatedDate,CreatedBy,ModifiedDate,ModifiedBy,[Subject]) values('ProgramIsEdited','When my program is edited',null,'AM','Dear [NameToBeReplaced],<br/><br/>Please note that following program has been edited.<br><br><table><tr><td>Program Name</td><td>:</td><td>[ProgramToBeReplaced]</td></tr><tr><td>Plan Name</td><td>:</td><td>[PlanNameToBeReplaced]</td></tr><tr><td>Edited by</td><td>:</td><td>[UserNameToBeReplaced]</td></tr></table><br><br>Thank You,<br>Hive9 Plan Admin',0,GETDATE(),@Createdby,null,null,'Plan : program is edited')
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
		select @NotificationMessage = 'Tactic '+ @componentTitle +' has been changed by ' + @UserName
		if(@Userid <> @EntityOwnerID)
		Begin
			insert into User_Notification_Messages(ComponentName,ComponentId,EntityId,[Description],ActionName,IsRead,UserId,RecipientId,CreatedDate,ClientID)
			values(@TableName,@objectId,@componentId,@NotificationMessage,@action,0,@Userid,@EntityOwnerID,GETDATE(),@ClientId)
		End
	End

	IF (EXISTS(SELECT * FROM #tempNotificationdata WHERE  NotificationInternalUseOnly = @CampaignIsEdited and @action='updated' and  @description ='campaign' )) 
	Begin
		
	    SET @description = UPPER(LEFT(@description,1))+LOWER(SUBSTRING(@description,2,LEN(@description)))
	    select @NotificationMessage = @description + ' '+ @componentTitle +' has been changed by ' + @UserName
		
		if(@Userid <> @EntityOwnerID)
		Begin
			insert into User_Notification_Messages(ComponentName,ComponentId,EntityId,[Description],ActionName,IsRead,UserId,RecipientId,CreatedDate,ClientID)
			values(@TableName,@objectId,@componentId,@NotificationMessage,@action,0,@Userid,@EntityOwnerID,GETDATE(),@ClientId)
		End
	End

	IF (EXISTS(SELECT * FROM #tempNotificationdata WHERE  NotificationInternalUseOnly = @ProgramIsEdited  and @action='updated' and @description ='program')) 
	Begin
		
	    SET @description = UPPER(LEFT(@description,1))+LOWER(SUBSTRING(@description,2,LEN(@description)))
		select @NotificationMessage = @description + ' '+ @componentTitle +' has been changed by ' + @UserName
	
		if(@Userid <> @EntityOwnerID)
		Begin
			insert into User_Notification_Messages(ComponentName,ComponentId,EntityId,[Description],ActionName,IsRead,UserId,RecipientId,CreatedDate,ClientID)
			values(@TableName,@objectId,@componentId,@NotificationMessage,@action,0,@Userid,@EntityOwnerID,GETDATE(),@ClientId)
		End
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
	
		select @NotificationMessage =  @UserName +' has added comment to ' + @description + ' ' + @componentTitle 
	
		insert into User_Notification_Messages(ComponentName,ComponentId,EntityId,[Description],ActionName,IsRead,UserId,RecipientId,CreatedDate,ClientID)
		SELECT @TableName,@objectId,@componentId,@NotificationMessage,@action,0,@Userid,UserId,GETDATE(),@ClientId FROM #tempNotificationdata
		WHERE NotificationInternalUseOnly = @CommentAddedToTactic and UserId <> @Userid
	End

	 IF (EXISTS(SELECT * FROM #tempNotificationdata WHERE  NotificationInternalUseOnly = @CommentAddedToCampaign and @description ='campaign'and @action='commentadded' )) 
	Begin
	
		select @NotificationMessage =  @UserName +' has added comment to ' + @description + ' ' + @componentTitle 
	
		insert into User_Notification_Messages(ComponentName,ComponentId,EntityId,[Description],ActionName,IsRead,UserId,RecipientId,CreatedDate,ClientID)
		SELECT @TableName,@objectId,@componentId,@NotificationMessage,@action,0,@Userid,UserId,GETDATE(),@ClientId FROM #tempNotificationdata
		WHERE NotificationInternalUseOnly = @CommentAddedToCampaign  and UserId <> @Userid
	End

	 IF (EXISTS(SELECT * FROM #tempNotificationdata WHERE NotificationInternalUseOnly = @CommentAddedToProgram and @description ='program'and @action='commentadded' )) 
	Begin
	
		select @NotificationMessage =  @UserName +' has added comment to ' + @description + ' ' + @componentTitle 
	
		insert into User_Notification_Messages(ComponentName,ComponentId,EntityId,[Description],ActionName,IsRead,UserId,RecipientId,CreatedDate,ClientID)
		SELECT @TableName,@objectId,@componentId,@NotificationMessage,@action,0,@Userid,UserId,GETDATE(),@ClientId FROM #tempNotificationdata
		WHERE NotificationInternalUseOnly = @CommentAddedToProgram and UserId <> @Userid
	End

	IF (EXISTS(SELECT * FROM #tempNotificationdata WHERE NotificationInternalUseOnly = @TacticIsApproved and @action='approved' and @description ='tactic' )) 
	Begin
	    SET @description = UPPER(LEFT(@description,1))+LOWER(SUBSTRING(@description,2,LEN(@description)))
		select @NotificationMessage = @description +' '+ @componentTitle +' has been approved by ' + @UserName
		insert into User_Notification_Messages(ComponentName,ComponentId,EntityId,[Description],ActionName,IsRead,UserId,RecipientId,CreatedDate,ClientID)
		SELECT @TableName,@objectId,@componentId,@NotificationMessage,@action,0,@Userid,UserId,GETDATE(),@ClientId FROM #tempNotificationdata
		WHERE (NotificationInternalUseOnly = @TacticIsApproved) and UserId <> @Userid
	End

	IF (EXISTS(SELECT * FROM #tempNotificationdata WHERE  NotificationInternalUseOnly = @CampaignIsApproved  and @action='approved' and  @description ='campaign' )) 
	Begin
	    SET @description = UPPER(LEFT(@description,1))+LOWER(SUBSTRING(@description,2,LEN(@description)))
		select @NotificationMessage = @description +' '+ @componentTitle +' has been approved by ' + @UserName
		insert into User_Notification_Messages(ComponentName,ComponentId,EntityId,[Description],ActionName,IsRead,UserId,RecipientId,CreatedDate,ClientID)
		SELECT @TableName,@objectId,@componentId,@NotificationMessage,@action,0,@Userid,UserId,GETDATE(),@ClientId FROM #tempNotificationdata
		WHERE NotificationInternalUseOnly = @CampaignIsApproved and UserId <> @Userid
	End

	IF (EXISTS(SELECT * FROM #tempNotificationdata WHERE  NotificationInternalUseOnly = @ProgramIsApproved  and @action='approved' and @description ='program')) 
	Begin
	    SET @description = UPPER(LEFT(@description,1))+LOWER(SUBSTRING(@description,2,LEN(@description)))
		select @NotificationMessage = @description +' '+ @componentTitle +' has been approved by ' + @UserName
		insert into User_Notification_Messages(ComponentName,ComponentId,EntityId,[Description],ActionName,IsRead,UserId,RecipientId,CreatedDate,ClientID)
		SELECT @TableName,@objectId,@componentId,@NotificationMessage,@action,0,@Userid,UserId,GETDATE(),@ClientId FROM #tempNotificationdata
		WHERE  NotificationInternalUseOnly = @ProgramIsApproved and UserId <> @Userid
	End

	IF (EXISTS(SELECT * FROM #tempNotificationdata WHERE NotificationInternalUseOnly = @TacticIsSubmitted and @description ='tactic' and @action='submitted' )) 
	Begin
		select @NotificationMessage = @UserName + ' has submitted tactic ' + @componentTitle +' for approval '
		if(@Userid <> @EntityOwnerID)
		Begin
			insert into User_Notification_Messages(ComponentName,ComponentId,EntityId,[Description],ActionName,IsRead,UserId,RecipientId,CreatedDate,ClientID)
			values(@TableName,@objectId,@componentId,@NotificationMessage,@action,0,@Userid,@EntityOwnerID,GETDATE(),@ClientId)
		End
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
	PRINT(@AlterTable)
	EXEC(@AlterTable)
	SET @InsertStatement+=' SELECT *,Col_RowGroup = ROW_NUMBER() OVER (PARTITION BY EntityId, EntityType,CustomFieldId ORDER BY (SELECT 100)) FROM #tblCustomData'
	PRINT(@InsertStatement)
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
			SET @UpdateStatement+='  @Col_'+(SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))+' = ['+(SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))+'] = CASE WHEN Col_RowGroup=1 THEN CONVERT(NVARCHAR(MAX),['+(SELECT REPLACE( REPLACE(REPLACE(@Val,'#',' '),'@','-'),'Col_',''))+']) ELSE @Col_'+(SELECT REPLACE(REPLACE(@Val,' ','#'),'-','@'))+'+'';''+ CONVERT(NVARCHAR(MAX),['+(SELECT REPLACE( REPLACE(REPLACE(@Val,'#',' '),'@','-'),'Col_',''))+']) END'+@Delimeter
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
	PRINT(@SelectGroup)
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

-- DROP AND CREATE FUNCTION
IF EXISTS (SELECT * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[ProjectedValuesForPlans]') AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
  DROP FUNCTION [dbo].[ProjectedValuesForPlans]
GO

-- DROP AND CREATE FUNCTION
IF EXISTS (SELECT * FROM sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[GetTacticsForAllRuleEntities]') AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
  DROP FUNCTION [dbo].[GetTacticsForAllRuleEntities]
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
	ClientId UNIQUEIDENTIFIER, 
    EntityTitle NVARCHAR(255),
	StartDate DATETIME,
	EndDate DATETIME,
	PercentComplete INT
)
AS
BEGIN
	INSERT INTO @TempTable 
			SELECT AR.RuleId,AR.EntityId,AR.EntityType,AR.Indicator,AR.IndicatorComparision,AR.IndicatorGoal,AR.CompletionGoal,AR.ClientId, P.Title, P.StartDate, P.EndDate, dbo.ConvertDateDifferenceToPercentage(P.StartDate,P.EndDate) AS PercentComplete
			FROM Alert_Rules AR
			CROSS APPLY (
				SELECT P1.PlanId, P1.Title, ISNULL(PC.StartDate,DATEADD(yy, DATEDIFF(yy,0,P1.[Year]), 0)) AS StartDate, ISNULL(PC.EndDate,DATEADD(yy, DATEDIFF(yy,0,P1.[Year]) + 1, -1)) AS EndDate FROM [Plan] P1 
				CROSS APPLY (
					SELECT MIN(PC1.StartDate) AS StartDate, MAX(PC1.EndDate) AS EndDate FROM Plan_Campaign PC1 
					WHERE P1.PlanId = PC1.PlanId AND PC1.IsDeleted = 0) PC
				WHERE P1.PlanId = AR.EntityId AND AR.EntityType = 'Plan' AND P1.IsDeleted = 0
			) P
			UNION ALL 
			SELECT AR.RuleId,AR.EntityId,AR.EntityType,AR.Indicator,AR.IndicatorComparision,AR.IndicatorGoal,AR.CompletionGoal,AR.ClientId, PC.Title, PC.StartDate, PC.EndDate, dbo.ConvertDateDifferenceToPercentage(PC.StartDate,PC.EndDate) AS PercentComplete
			FROM Alert_Rules AR
			CROSS APPLY (SELECT * FROM [Plan_Campaign] PC WHERE PC.PlanCampaignId = AR.EntityId AND AR.EntityType = 'Campaign' AND PC.IsDeleted = 0) PC
			UNION ALL 
			SELECT AR.RuleId,AR.EntityId,AR.EntityType,AR.Indicator,AR.IndicatorComparision,AR.IndicatorGoal,AR.CompletionGoal,AR.ClientId, PCP.Title, PCP.StartDate, PCP.EndDate, dbo.ConvertDateDifferenceToPercentage(PCP.StartDate,PCP.EndDate) AS PercentComplete
			FROM Alert_Rules AR
			CROSS APPLY (SELECT * FROM [Plan_Campaign_Program] PCP WHERE PCP.PlanProgramId = AR.EntityId AND AR.EntityType = 'Program' AND PCP.IsDeleted = 0) PCP
			UNION ALL 
			SELECT AR.RuleId,AR.EntityId,AR.EntityType,AR.Indicator,AR.IndicatorComparision,AR.IndicatorGoal,AR.CompletionGoal,AR.ClientId, PCPT.Title, PCPT.StartDate, PCPT.EndDate, dbo.ConvertDateDifferenceToPercentage(PCPT.StartDate,PCPT.EndDate) AS PercentComplete
			FROM Alert_Rules AR
			CROSS APPLY (SELECT * FROM [Plan_Campaign_Program_Tactic] PCPT WHERE AR.EntityId = PCPT.PlanTacticId AND AR.EntityType = 'Tactic' AND PCPT.IsDeleted = 0 AND PCPT.[Status] IN ('In-Progress','Complete','Approved') ) PCPT
			UNION ALL 
			SELECT AR.RuleId,AR.EntityId,AR.EntityType,AR.Indicator,AR.IndicatorComparision,AR.IndicatorGoal,AR.CompletionGoal,AR.ClientId, PCPTL.Title, PCPTL.StartDate, PCPTL.EndDate, dbo.ConvertDateDifferenceToPercentage(PCPTL.StartDate,PCPTL.EndDate) AS PercentComplete
			FROM Alert_Rules AR
			CROSS APPLY (
				SELECT PCPTLineItem.PlanLineItemId, PCPTLineItem.Title, PCPTLT.StartDate, PCPTLT.EndDate FROM [Plan_Campaign_Program_Tactic_LineItem] PCPTLineItem 
				CROSS APPLY (
					SELECT PCPT.StartDate,PCPT.EndDate FROM [Plan_Campaign_Program_Tactic] PCPT 
					WHERE PCPTLineItem.PlanTacticId = PCPT.PlanTacticId AND PCPT.IsDeleted = 0 AND PCPT.[Status] IN ('In-Progress','Complete','Approved') 
				) PCPTLT
				WHERE PCPTLineItem.PlanLineItemId = AR.EntityId AND AR.EntityType = 'LineItem' AND AR.Indicator = 'PLANNEDCOST' AND PCPTLineItem.IsDeleted = 0
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
	PlanLineItemId INT,
	PlanTacticId INT,
	PlanProgramId INT,
	PlanCampaignId INT,
	PlanId INT,
	Indicator NVARCHAR(50),
	IndicatorTitle NVARCHAR(MAX),
	ProjectedStageValue FLOAT,
	ActualStageValue FLOAT
)
AS
BEGIN
		INSERT INTO @TempTable
		SELECT NULL, PlanTacticId, PlanProgramId, PlanCampaignId, PlanId, Indicator, IndicatorTitle, ProjectedStageValue, 
		(CASE WHEN Indicator = 'PLANNEDCOST' AND SUM(LineItemActuals) IS NOT NULL THEN SUM(LineItemActuals) ELSE SUM(ActualValue) END) AS ActualStageValue
		FROM
		(
			SELECT Tactic.PlanTacticId, Program.PlanProgramId, Campaign.PlanCampaignId, [Plan].PlanId, 
			dbo.TacticIndicatorProjectedValue(Tactic.PlanTacticId,[Model].ModelId, [Stage].Code,RuleEntityTable.indicator,[Model].ClientId) AS ProjectedStageValue,
			ISNULL(MIN(ActualValue),0) ActualValue,RuleEntityTable.Indicator,SUM(LineItemActuals) AS LineItemActuals, Stage.IndicatorTitle
			FROM @TempEntityTable  RuleEntityTable 
			CROSS APPLY (SELECT PlanId,ModelId From [Plan] WITH (NOLOCK) WHERE [Plan].PlanId = RuleEntityTable.EntityId AND [Plan].IsDeleted=0) [Plan]
			CROSS APPLY (SELECT ModelId,ClientId From [Model] WITH (NOLOCK) WHERE [Model].ModelId = [Plan].ModelId AND [Model].IsDeleted=0) [Model]
			CROSS APPLY (SELECT PlanCampaignId,PlanId FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE Campaign.PlanId = [Plan].PlanId AND Campaign.IsDeleted=0) Campaign 
			CROSS APPLY (SELECT PlanProgramId,PlanCampaignId FROM Plan_Campaign_Program AS Program WITH (NOLOCK) WHERE Program.PlanCampaignId=Campaign.PlanCampaignId AND Program.IsDeleted=0) Program
			CROSS APPLY (SELECT PlanTacticId,StageId,TacticTypeId,PlanProgramId FROM Plan_Campaign_Program_Tactic AS Tactic WITH (NOLOCK)
						 WHERE Tactic.PlanProgramId = Program.PlanProgramId AND Tactic.IsDeleted = 0
						 AND Tactic.[Status] IN ('In-Progress','Complete','Approved')) Tactic
			CROSS APPLY (SELECT StageId, Code, [dbo].[GetIndicatorTitle](Code,ClientId) AS IndicatorTitle FROM Stage WHERE Stage.StageId = Tactic.StageId AND Stage.IsDeleted = 0) Stage
			OUTER APPLY (
							SELECT ActualValue,StageTitle,Period FROM Plan_Campaign_Program_Tactic_Actual Actual WITH (NOLOCK)
							WHERE Tactic.PlanTacticId = Actual.PlanTacticId
							AND StageTitle = CASE WHEN RuleEntityTable.INDICATOR = STAGE.CODE AND RuleEntityTable.INDICATOR NOT IN ('MQL','CW','REVENUE')
													THEN 'PROJECTEDSTAGEVALUE'
													ELSE RuleEntityTable.INDICATOR END
						) Actual
			OUTER APPLY (
							SELECT LineItem.PlanLineItemId,LineItem.PlanTacticId, LTActual.Value AS LineItemActuals FROM Plan_Campaign_Program_Tactic_LineItem LineItem
							OUTER APPLY (SELECT Value,PlanLineItemId FROM Plan_Campaign_Program_Tactic_LineItem_Actual Actual
							WHERE LineItem.PlanLineItemId = Actual.PlanLineItemId 
							) LTActual WHERE Tactic.PlanTacticId = LineItem.PlanTacticId
							AND RuleEntityTable.Indicator = 'PLANNEDCOST'
			) LineItemActual
			WHERE RuleEntityTable.EntityType = 'Plan'
			GROUP BY [Tactic].PlanTacticId, Program.PlanProgramId, Campaign.PlanCampaignId,[Plan].PlanId,Actual.Period,RuleEntityTable.Indicator,
			Stage.IndicatorTitle, Model.ModelId,Model.ClientId,Stage.Code
		) P
		GROUP BY PlanTacticId, PlanProgramId, PlanCampaignId, PlanId, Indicator, ProjectedStageValue, IndicatorTitle
		
		UNION 
		SELECT NULL, PlanTacticId, PlanProgramId, PlanCampaignId, PlanId, Indicator, IndicatorTitle, ProjectedStageValue, 
		(CASE WHEN Indicator = 'PLANNEDCOST' AND SUM(LineItemActuals) IS NOT NULL THEN SUM(LineItemActuals) ELSE SUM(ActualValue) END) AS ActualStageValue FROM
		(
			SELECT Tactic.PlanTacticId, Program.PlanProgramId, Campaign.PlanCampaignId, [Plan].PlanId, 
			dbo.TacticIndicatorProjectedValue(Tactic.PlanTacticId,[Model].ModelId, [Stage].Code,RuleEntityTable.indicator,[Model].ClientId) AS ProjectedStageValue,
			ISNULL(MIN(ActualValue),0) ActualValue,RuleEntityTable.Indicator,SUM(LineItemActuals) AS LineItemActuals, Stage.IndicatorTitle
			FROM @TempEntityTable  RuleEntityTable 
			CROSS APPLY (SELECT PlanCampaignId,PlanId FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE Campaign.PlanCampaignId = RuleEntityTable.EntityId AND Campaign.IsDeleted=0) Campaign 
			CROSS APPLY (SELECT PlanId,ModelId From [Plan] WITH (NOLOCK) WHERE [Plan].PlanId = Campaign.PlanId AND [Plan].IsDeleted=0) [Plan]
			CROSS APPLY (SELECT ModelId,ClientId From [Model] WITH (NOLOCK) WHERE [Model].ModelId = [Plan].ModelId AND [Model].IsDeleted=0) [Model]
			CROSS APPLY (SELECT PlanProgramId,PlanCampaignId FROM Plan_Campaign_Program AS Program WITH (NOLOCK) WHERE Program.PlanCampaignId=Campaign.PlanCampaignId AND Program.IsDeleted=0) Program
			CROSS APPLY (SELECT PlanTacticId,StageId,TacticTypeId,PlanProgramId FROM Plan_Campaign_Program_Tactic AS Tactic WITH (NOLOCK) WHERE Tactic.PlanProgramId=Program.PlanProgramId AND Tactic.IsDeleted=0
						AND Tactic.[Status] IN ('In-Progress','Complete','Approved')) Tactic
			CROSS APPLY (SELECT StageId, Code, [dbo].[GetIndicatorTitle](Code,ClientId) AS IndicatorTitle FROM Stage WHERE Stage.StageId = Tactic.StageId AND Stage.IsDeleted = 0) Stage
			OUTER APPLY (
							SELECT ActualValue,StageTitle,Period FROM Plan_Campaign_Program_Tactic_Actual Actual WITH (NOLOCK)
							WHERE Tactic.PlanTacticId = Actual.PlanTacticId 
							AND StageTitle = CASE WHEN RuleEntityTable.INDICATOR = STAGE.CODE AND RuleEntityTable.INDICATOR NOT IN ('MQL','CW','REVENUE')  
													THEN 'PROJECTEDSTAGEVALUE' 
													ELSE RuleEntityTable.INDICATOR END
						) Actual
			OUTER APPLY (
							SELECT LineItem.PlanLineItemId,LineItem.PlanTacticId, LTActual.Value AS LineItemActuals FROM Plan_Campaign_Program_Tactic_LineItem LineItem
							OUTER APPLY (SELECT Value,PlanLineItemId FROM Plan_Campaign_Program_Tactic_LineItem_Actual Actual
							WHERE LineItem.PlanLineItemId = Actual.PlanLineItemId 
							) LTActual WHERE Tactic.PlanTacticId = LineItem.PlanTacticId
							AND RuleEntityTable.Indicator = 'PLANNEDCOST'
			) LineItemActual
			WHERE RuleEntityTable.EntityType = 'Campaign'
			GROUP BY [Tactic].PlanTacticId, Program.PlanProgramId, Campaign.PlanCampaignId,[Plan].PlanId,Actual.Period,RuleEntityTable.Indicator,Stage.IndicatorTitle,Model.ModelId,Model.ClientId,Stage.Code
		) PC
		GROUP BY PlanTacticId, PlanProgramId, PlanCampaignId, PlanId, Indicator, ProjectedStageValue, IndicatorTitle
		
		UNION 
		SELECT NULL, PlanTacticId, PlanProgramId, PlanCampaignId, PlanId, Indicator, IndicatorTitle, ProjectedStageValue, 
		(CASE WHEN Indicator = 'PLANNEDCOST' AND SUM(LineItemActuals) IS NOT NULL THEN SUM(LineItemActuals) ELSE SUM(ActualValue) END) AS ActualStageValue FROM
		(
			SELECT Tactic.PlanTacticId, Program.PlanProgramId, Campaign.PlanCampaignId, [Plan].PlanId, 
			dbo.TacticIndicatorProjectedValue(Tactic.PlanTacticId,[Model].ModelId, [Stage].Code,RuleEntityTable.indicator,[Model].ClientId) AS ProjectedStageValue,
			ISNULL(MIN(ActualValue),0) ActualValue,RuleEntityTable.Indicator,SUM(LineItemActuals) AS LineItemActuals, Stage.IndicatorTitle
			FROM @TempEntityTable  RuleEntityTable 
			CROSS APPLY (SELECT PlanProgramId,PlanCampaignId FROM Plan_Campaign_Program AS Program WITH (NOLOCK) WHERE Program.PlanProgramId=RuleEntityTable.EntityId AND Program.IsDeleted=0) Program
			CROSS APPLY (SELECT PlanCampaignId,PlanId  FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE Campaign.PlanCampaignId = Program.PlanCampaignId AND Campaign.IsDeleted=0) Campaign 
			CROSS APPLY (SELECT PlanId,ModelId From [Plan] WITH (NOLOCK) WHERE [Plan].PlanId = Campaign.PlanId AND [Plan].IsDeleted=0) [Plan]
			CROSS APPLY (SELECT ModelId,ClientId From [Model] WITH (NOLOCK) WHERE [Model].ModelId = [Plan].ModelId AND [Model].IsDeleted=0) [Model]
			CROSS APPLY (SELECT PlanTacticId,StageId,TacticTypeId,PlanProgramId FROM Plan_Campaign_Program_Tactic AS Tactic WITH (NOLOCK) WHERE Tactic.PlanProgramId=Program.PlanProgramId AND Tactic.IsDeleted=0
						AND Tactic.[Status] IN ('In-Progress','Complete','Approved')) Tactic
			CROSS APPLY (SELECT StageId, Code, [dbo].[GetIndicatorTitle](Code,ClientId) AS IndicatorTitle FROM Stage WHERE Stage.StageId = Tactic.StageId AND Stage.IsDeleted = 0) Stage
			OUTER APPLY (
							SELECT ActualValue,StageTitle,Period FROM Plan_Campaign_Program_Tactic_Actual Actual WITH (NOLOCK)
							WHERE Tactic.PlanTacticId = Actual.PlanTacticId 
							AND StageTitle = CASE WHEN RuleEntityTable.INDICATOR = STAGE.CODE AND RuleEntityTable.INDICATOR NOT IN ('MQL','CW','REVENUE')  
													THEN 'PROJECTEDSTAGEVALUE' 
													ELSE RuleEntityTable.INDICATOR END
						) Actual
			OUTER APPLY (
							SELECT LineItem.PlanLineItemId,LineItem.PlanTacticId, LTActual.Value AS LineItemActuals FROM Plan_Campaign_Program_Tactic_LineItem LineItem
							OUTER APPLY (SELECT Value,PlanLineItemId FROM Plan_Campaign_Program_Tactic_LineItem_Actual Actual
							WHERE LineItem.PlanLineItemId = Actual.PlanLineItemId 
							) LTActual WHERE Tactic.PlanTacticId = LineItem.PlanTacticId
							AND RuleEntityTable.Indicator = 'PLANNEDCOST'
			) LineItemActual
			WHERE RuleEntityTable.EntityType = 'Program'
			GROUP BY [Tactic].PlanTacticId, Program.PlanProgramId, Campaign.PlanCampaignId,[Plan].PlanId,Actual.Period,RuleEntityTable.Indicator,Stage.IndicatorTitle,Model.ModelId,Model.ClientId,Stage.Code
		) PCP
		GROUP BY PlanTacticId, PlanProgramId, PlanCampaignId, PlanId, Indicator, ProjectedStageValue, IndicatorTitle
		
		UNION 
		SELECT NULL, PlanTacticId, PlanProgramId, PlanCampaignId, PlanId, Indicator, IndicatorTitle, ProjectedStageValue, 
		(CASE WHEN Indicator = 'PLANNEDCOST' AND SUM(LineItemActuals) IS NOT NULL THEN SUM(LineItemActuals) ELSE SUM(ActualValue) END) AS ActualStageValue FROM
		(
			SELECT Tactic.PlanTacticId, Program.PlanProgramId, Campaign.PlanCampaignId, [Plan].PlanId, 
			dbo.TacticIndicatorProjectedValue(Tactic.PlanTacticId,[Model].ModelId, [Stage].Code,RuleEntityTable.indicator,[Model].ClientId) AS ProjectedStageValue,
			ISNULL(MIN(ActualValue),0) ActualValue,RuleEntityTable.Indicator,SUM(LineItemActuals) AS LineItemActuals, Stage.IndicatorTitle
			FROM @TempEntityTable  RuleEntityTable 
			CROSS APPLY (SELECT PlanTacticId,StageId,TacticTypeId,PlanProgramId FROM Plan_Campaign_Program_Tactic AS Tactic WITH (NOLOCK) WHERE Tactic.PlanTacticId=RuleEntityTable.EntityId AND Tactic.IsDeleted=0
						AND Tactic.[Status] IN ('In-Progress','Complete','Approved')) Tactic
			CROSS APPLY (SELECT StageId, Code, [dbo].[GetIndicatorTitle](Code,ClientId) AS IndicatorTitle FROM Stage WHERE Stage.StageId = Tactic.StageId AND Stage.IsDeleted = 0) Stage
			CROSS APPLY (SELECT PlanProgramId,PlanCampaignId FROM Plan_Campaign_Program AS Program WITH (NOLOCK) WHERE Program.PlanProgramId=Tactic.PlanProgramId AND Program.IsDeleted=0) Program
			CROSS APPLY (SELECT PlanCampaignId,PlanId  FROM Plan_Campaign AS Campaign WITH (NOLOCK) WHERE Campaign.PlanCampaignId = Program.PlanCampaignId AND Campaign.IsDeleted=0) Campaign 
			CROSS APPLY (SELECT PlanId,ModelId From [Plan] WITH (NOLOCK) WHERE [Plan].PlanId = Campaign.PlanId AND [Plan].IsDeleted=0) [Plan]
			CROSS APPLY (SELECT ModelId,ClientId From [Model] WITH (NOLOCK) WHERE [Model].ModelId = [Plan].ModelId AND [Model].IsDeleted=0) [Model]
			OUTER APPLY (
							SELECT ActualValue,StageTitle,Period FROM Plan_Campaign_Program_Tactic_Actual Actual WITH (NOLOCK)
							WHERE Tactic.PlanTacticId = Actual.PlanTacticId 
							AND StageTitle = CASE WHEN RuleEntityTable.INDICATOR = STAGE.CODE AND RuleEntityTable.INDICATOR NOT IN ('MQL','CW','REVENUE')  
													THEN 'PROJECTEDSTAGEVALUE' 
													ELSE RuleEntityTable.INDICATOR END
						) Actual
			OUTER APPLY (
							SELECT LineItem.PlanLineItemId,LineItem.PlanTacticId, LTActual.Value AS LineItemActuals FROM Plan_Campaign_Program_Tactic_LineItem LineItem
							OUTER APPLY (SELECT Value,PlanLineItemId FROM Plan_Campaign_Program_Tactic_LineItem_Actual Actual
							WHERE LineItem.PlanLineItemId = Actual.PlanLineItemId 
							) LTActual WHERE Tactic.PlanTacticId = LineItem.PlanTacticId
							AND RuleEntityTable.Indicator = 'PLANNEDCOST'
			) LineItemActual
			WHERE RuleEntityTable.EntityType = 'Tactic'
			GROUP BY [Tactic].PlanTacticId, Program.PlanProgramId, Campaign.PlanCampaignId,[Plan].PlanId,Actual.Period,RuleEntityTable.Indicator,Stage.IndicatorTitle,Model.ModelId,Model.ClientId,Stage.Code
		) PCPT
		GROUP BY PlanTacticId, PlanProgramId, PlanCampaignId, PlanId, Indicator, ProjectedStageValue, IndicatorTitle
		
		UNION 
		SELECT PlanLineItemId, NULL, NULL, NULL, NULL, Indicator, 'Planned Cost', ProjectedStageValue, 
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
CREATE PROCEDURE RunAlertRules
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

									-- Get entities from the rule which are reached to completion goal
									INSERT INTO @TempEntityTable([RuleId],[EntityId],[EntityType],[Indicator],[IndicatorComparision],[IndicatorGoal],[CompletionGoal],[ClientId],
																 [EntityTitle],[StartDate],[EndDate],[PercentComplete]) 
									SELECT Entity.* FROM dbo.GetEntitiesReachedCompletionGoal() Entity 
									WHERE Entity.PercentComplete >= Entity.CompletionGoal

									-- Table with projected and actual values of tactic belongs to plan/campaign/program
									SELECT * INTO #TacticsDataForAllRuleEntities FROM dbo.[GetTacticsForAllRuleEntities](@TempEntityTable)
									-- SELECT * FROM #TacticsDataForAllRuleEntities 
									'
		
		-- Common query to update projected and actual values of indicators for entities
		SET @UPDATEQUERYCOMMON =  ';	UPDATE A SET A.ProjectedStageValue = ISNULL(B.ProjectedStageValue,0), A.ActualStageValue = ISNULL(B.ActualStageValue,0),
										A.IndicatorTitle = B.IndicatorTitle
										FROM @TempEntityTable A INNER JOIN  
										(
											SELECT B.##ENTITYIDCOLNAME##,B.INDICATOR,B.IndicatorTitle,SUM(B.ProjectedStageValue) AS ProjectedStageValue,SUM(B.ActualStageValue) AS ActualStageValue
											FROM @TempEntityTable A
											INNER JOIN #TacticsDataForAllRuleEntities B
											ON A.EntityId = B.##ENTITYIDCOLNAME## AND A.EntityType = ##ENTITYTYPE## AND A.Indicator = B.Indicator
											GROUP BY B.##ENTITYIDCOLNAME##, B.Indicator, B.IndicatorTitle
										) B ON A.EntityId = B.##ENTITYIDCOLNAME##  AND A.Indicator = B.Indicator AND A.EntityType = ##ENTITYTYPE## 
											
										'

		-- Update query for plan
		SET @UpdatePlanQuery = REPLACE(@UPDATEQUERYCOMMON,'##ENTITYIDCOLNAME##','PlanId')
		SET @UpdatePlanQuery = REPLACE(@UpdatePlanQuery,'##ENTITYTYPE##','''Plan''')
		-- Update query for campaign
		SET @UpdateCampaignQuery = REPLACE(@UPDATEQUERYCOMMON,'##ENTITYIDCOLNAME##','PlanCampaignId')
		SET @UpdateCampaignQuery = REPLACE(@UpdateCampaignQuery,'##ENTITYTYPE##','''Campaign''')
		-- Update query for program
		SET @UpdateProgramQuery = REPLACE(@UPDATEQUERYCOMMON,'##ENTITYIDCOLNAME##','PlanProgramId')
		SET @UpdateProgramQuery = REPLACE(@UpdateProgramQuery,'##ENTITYTYPE##','''Program''')
		-- Update query for tactic
		SET @UpdateTacticQuery = REPLACE(@UPDATEQUERYCOMMON,'##ENTITYIDCOLNAME##','PlanTacticId')
		SET @UpdateTacticQuery = REPLACE(@UpdateTacticQuery,'##ENTITYTYPE##','''Tactic''')
		-- Update query for line item
		SET @UpdateLineItemQuery = REPLACE(@UPDATEQUERYCOMMON,'##ENTITYIDCOLNAME##','PlanLineItemId')
		SET @UpdateLineItemQuery = REPLACE(@UpdateLineItemQuery,'##ENTITYTYPE##','''LineItem''')

		-- For plan update projected value using different calculation
		SET @UpdateProjectedValuesForPlan = ';  UPDATE A SET A.ProjectedStageValue = ISNULL(B.ProjectedStageValue,0)
												FROM @TempEntityTable A INNER JOIN
												[dbo].[ProjectedValuesForPlans](@TempEntityTable) B ON A.EntityId = B.PlanId  
												AND A.Indicator = B.Indicator AND A.EntityType = ''Plan''
												'
		-- Convert percent of goal from Projected and Actual values
		SET @CalculatePercentGoalQuery = ' UPDATE @TempEntityTable SET CalculatedPercentGoal = 
											CASE WHEN ProjectedStageValue = 0 AND ActualStageValue = 0 THEN 0 
												 WHEN (ProjectedStageValue = 0 AND ActualStageValue != 0) OR (ActualStageValue * 100 / ProjectedStageValue) > 100 THEN 100 
												 ELSE ActualStageValue * 100 / ProjectedStageValue END ;
										   -- SELECT * FROM @TempEntityTable 
										   '
		-- Common query to create alerts
		SET @INSERTALERTQUERYCOMMON = '	SELECT AR.RuleId, ##DESCRIPTION## AS [Description], AR.UserId,
										(CASE WHEN AR.Frequency = ''WEEKLY'' THEN
											DATEADD(DAY,
											CASE WHEN DATEDIFF(DAY,DATEPART(dw,GETDATE()),AR.DayOfWeek+1) < 0 THEN
												DATEDIFF(DAY,DATEPART(dw,GETDATE()),AR.DayOfWeek + 1) + 7
											ELSE 
												DATEDIFF(DAY,DATEPART(dw,GETDATE()),AR.DayOfWeek + 1) END
											,GETDATE()) 
										WHEN AR.Frequency = ''MONTHLY'' THEN
											CASE WHEN DATEDIFF(DAY,DATEPART(DAY,GETDATE()),AR.DateOfMonth) < 0 THEN
												DATEADD(MONTH,1,DATEADD(DAY,DATEDIFF(DAY,DATEPART(DAY,GETDATE()),AR.DateOfMonth),GETDATE()))
											ELSE 
												DATEADD(DAY,DATEDIFF(DAY,DATEPART(DAY,GETDATE()),AR.DateOfMonth),GETDATE())  END
										ELSE GETDATE() END ) AS DisplayDate										
										FROM @TempEntityTable FinalTable
										INNER JOIN Alert_Rules AR ON FinalTable.RuleId = AR.RuleId '

		-- For less than rule
		DECLARE @LessThanWhere		NVARCHAR(MAX) = ' WHERE FinalTable.CalculatedPercentGoal < AR.IndicatorGoal AND AR.IndicatorComparision = ''LT'' '
		-- For greater than rule
		DECLARE @GreaterThanWhere	NVARCHAR(MAX) = ' WHERE FinalTable.CalculatedPercentGoal > AR.IndicatorGoal AND AR.IndicatorComparision = ''GT'' '
		-- For equal to rule
		DECLARE @EqualToWhere		NVARCHAR(MAX) = ' WHERE FinalTable.CalculatedPercentGoal = AR.IndicatorGoal AND AR.IndicatorComparision = ''EQ'' '

		SET @InsertQueryForLT = REPLACE(@INSERTALERTQUERYCOMMON, '##DESCRIPTION##', ' FinalTable.EntityTitle +''''''s ''+ FinalTable.IndicatorTitle +'' is ' + @txtLessThan +' '' + CAST(AR.IndicatorGoal AS NVARCHAR) + ''% of the goal'' ' ) + @LessThanWhere
		SET @InsertQueryForGT = REPLACE(@INSERTALERTQUERYCOMMON, '##DESCRIPTION##', ' FinalTable.EntityTitle +''''''s ''+ FinalTable.IndicatorTitle +'' is ' + @txtGreaterThan +' '' + CAST(AR.IndicatorGoal AS NVARCHAR) + ''% of the goal'' ' ) + @GreaterThanWhere
		SET @InsertQueryForEQ = REPLACE(@INSERTALERTQUERYCOMMON, '##DESCRIPTION##', ' FinalTable.EntityTitle +''''''s ''+ FinalTable.IndicatorTitle +'' is ' + @txtEqualTo +' '' + CAST(AR.IndicatorGoal AS NVARCHAR) + ''% of the goal'' ' ) + @EqualToWhere
		
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
		
		EXEC (@TacticsDataForRules + @UpdatePlanQuery + @UpdateCampaignQuery + @UpdateProgramQuery + @UpdateTacticQuery + @UpdateLineItemQuery + @UpdateProjectedValuesForPlan + 
				@CalculatePercentGoalQuery + @InsertQueryForLT + @InsertQueryForGT +@InsertQueryForEQ )
	
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
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PullMeasureSFDCActual]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[PullMeasureSFDCActual]
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'INT')
EXEC sys.sp_executesql N'CREATE SCHEMA [INT]'

GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[INT].[AddIntegrationInstanceLog]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [INT].[AddIntegrationInstanceLog] AS' 
END
GO
-- =============================================
-- Author:		Mitesh Vaishnav
-- Create date: 08/20/2016
-- Description:	Add log for integration instance
-- =============================================
ALTER PROCEDURE [INT].[AddIntegrationInstanceLog]
	(
	@IntegrationInstanceId INT,
	@UserID NVARCHAR(36),
	@IntegrationInstanceLogId INT,
	@Status NVARCHAR(20) NULL=NULL,
	@ErrorDescription NVARCHAR(MAX) NULL=NULL,
	@OutPutLogid int output
	)
AS
BEGIN
	IF (@IntegrationInstanceLogId=0)
		BEGIN
	INSERT INTO [dbo].[IntegrationInstanceLog]
           ([IntegrationInstanceId]
           ,[SyncStart]
           ,[SyncEnd]
           ,[Status]
           ,[ErrorDescription]
           ,[CreatedDate]
           ,[CreatedBy]
           ,[IsAutoSync])
     VALUES
           (@IntegrationInstanceId
           ,GETDATE()
           ,NULL
           ,NULL
           ,NULL
           ,GETDATE()
           ,@UserID
           ,NULL)

		   select @IntegrationInstanceLogId=SCOPE_IDENTITY()
		END
	ELSE
		BEGIN
		UPDATE [dbo].[IntegrationInstanceLog]
		SET [SyncEnd]=GETDATE()
		   ,[Status]=@Status
		   ,[ErrorDescription]=@ErrorDescription
		WHERE IntegrationInstanceLogId=@IntegrationInstanceLogId
		END
		set @OutPutLogid=@IntegrationInstanceLogId

END

GO
/****** Object:  StoredProcedure [INT].[AddIntegrationInstanceSectionLog]    Script Date: 08/23/2016 13:02:36 ******/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[INT].[AddIntegrationInstanceSectionLog]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [INT].[AddIntegrationInstanceSectionLog] AS' 
END
GO
-- =============================================
-- Author:		Mitesh Vaishnav 
-- Create date: 08/22/2016
-- Description:	Add integration instance log with section details
-- =============================================
ALTER PROCEDURE [INT].[AddIntegrationInstanceSectionLog]
	(
	@IntegrationInstanceSectionLogId INT=0,
	@IntegrationInstanceId INT,
	@UserID NVARCHAR(36),
	@IntegrationInstanceLogId INT,
	@sectionName NVARCHAR(1000),
	@Status NVARCHAR(20) NULL=NULL,
	@ErrorDescription NVARCHAR(MAX) NULL=NULL,
	@OutPutLogId INT OUTPUT
	)
AS
BEGIN
	IF (@IntegrationInstanceSectionLogId=0)
	BEGIN
	INSERT INTO [dbo].[IntegrationInstanceSection]
           ([IntegrationInstanceLogId]
           ,[IntegrationInstanceId]
           ,[SectionName]
           ,[SyncStart]
           ,[SyncEnd]
           ,[Status]
           ,[Description]
           ,[CreatedDate]
           ,[CreateBy])
     VALUES
           (@IntegrationInstanceLogId
           ,@IntegrationInstanceId
           ,@sectionName
           ,GETDATE()
           ,NULL
           ,@Status
           ,@ErrorDescription
           ,GETDATE()
           ,@UserID)

		   SELECT @IntegrationInstanceSectionLogId=SCOPE_IDENTITY()
		   END
	ELSE
	BEGIN
	UPDATE [dbo].[IntegrationInstanceSection]
			SET [SyncEnd]=GETDATE(),
				[Status]=@Status,
				[Description]=@ErrorDescription
			WHERE IntegrationInstanceSectionId=@IntegrationInstanceSectionLogId
			SELECT @IntegrationInstanceSectionLogId=0
	END

	SET @OutPutLogId=@IntegrationInstanceSectionLogId
END

GO

/****** Object:  StoredProcedure [INT].[GETMeasureClientDbName]    Script Date: 08/23/2016 12:53:22 ******/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[INT].[GETMeasureClientDbName]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [INT].[GETMeasureClientDbName] AS' 
END
GO
-- =============================================
-- Author:		Mitesh Vaishnav
-- Create date: 08/20/2016
-- Description:	Sp will get Measure client database name
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

GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[INT].[CreateMeasureSFDCActualTable]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [INT].[CreateMeasureSFDCActualTable] AS' 
END
GO
-- =============================================
-- Author:		Mitesh Vaishnav
-- Create date: 08/20/2016
-- Description:	Sp will create temp table for MEasure SFDC actuals data 
-- =============================================
ALTER PROCEDURE [INT].[CreateMeasureSFDCActualTable]
(
@ClientId nvarchar(36),
@AuthDatabaseName Nvarchar(1000),
@SFDCActualTempTable  NVARCHAR(1000) OUTPUT
)
AS
BEGIN
	DECLARE @NewSFDCActualTableName NVARCHAR(255)
	DECLARE @CustomQuery NVARCHAR(MAX)
	DECLARE @DataBaseName NVARCHAR(100)
	DECLARE @tempDbName TABLE
	(
		DbName NVARCHAR(100)
	)
	INSERT INTO @tempDbName
	EXEC [INT].[GETMeasureClientDbName] @ClientId=@ClientId,@AuthDatabaseName=@AuthDatabaseName
	
	SELECT @DataBaseName= DbName from @tempDbName

	IF (@DataBaseName <> '' AND @DataBaseName IS NOT NULL)
	BEGIN
		SET @NewSFDCActualTableName='MeasureSFDC_'
									+CONVERT(NVARCHAR(4), DATEPART(YEAR,GETDATE()))+'_'
									+CONVERT(NVARCHAR(2), DATEPART(MONTH,GETDATE()))+'_'
									+CONVERT(NVARCHAR(2), DATEPART(DAY,GETDATE()))+'_'
									+CONVERT(NVARCHAR(2), DATEPART(HOUR,GETDATE()))+'_'
									+CONVERT(NVARCHAR(2), DATEPART(MINUTE,GETDATE()))+'_'
									+CONVERT(NVARCHAR(2), DATEPART(SECOND,GETDATE()))+'_'
									+CONVERT(NVARCHAR(10), DATEPART(MILLISECOND,GETDATE()))

		SET @CustomQuery='CREATE TABLE '+@NewSFDCActualTableName+'
								(
								IntegrationType NVARCHAR(20),
								StageTitle NVARCHAR(20),
								Period NVARCHAR(10),
								ActualValue FLOAT,
								Unit VARCHAR(10),
								PusheeID NVARCHAR(255),
								PulleeID NVARCHAR(255),
								ModifiedDate DateTime
								)
				INSERT INTO ['+@NewSFDCActualTableName+' ]
				SELECT * FROM '+@DataBaseName+'.[INT].[GetTacticActuals](GETDATE(),GETDATE()-1);'
		
		EXEC (@CustomQuery)
		SELECT @SFDCActualTempTable=@NewSFDCActualTableName
	END
END


GO


/****** Object:  StoredProcedure [INT].[PullCw]    Script Date: 08/20/2016 18:07:10 ******/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[INT].[PullCw]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [INT].[PullCw] AS' 
END
GO
-- =============================================
-- Author:	Mitesh Vaishnav
-- Create date: 08/20/2016
-- Description:	Sp contains logic to insert actuals from measure to plan database
-- =============================================
ALTER PROCEDURE [INT].[PullCw]
	(
	@MeasureSFDCActualTableName NVARCHAR(255)
	)
AS
BEGIN

	DECLARE @CustomQuery NVARCHAR(MAX)
	
	--Remove plan tactic actuals for stage title revenue and CW which match with measure sfdc tactics
	SET @CustomQuery=' DELETE A 
		FROM Plan_Campaign_Program_Tactic_Actual A 
		INNER JOIN Plan_Campaign_Program_Tactic PT ON PT.PlanTacticId=A.PlanTacticId
		INNER JOIN '+@MeasureSFDCActualTableName+' T ON SUBSTRING(ISNULL(T.PulleeID,''''),0,16) = SUBSTRING(ISNULL(PT.IntegrationInstanceTacticId,''''),0,16) AND T.StageTitle=''CW''
		WHERE A.StageTitle=''Revenue'' OR A.StageTitle=''CW'';'

	EXEC (@CustomQuery)
	
	--insert plan tactic actuals for stage title CW which match with measure sfdc tactics	
	SET @CustomQuery='INSERT INTO Plan_Campaign_Program_Tactic_Actual
						SELECT PT.PlanTacticId,
								M.StageTitle,
								CASE WHEN CONVERT(INT, DATEPART(YEAR, PT.StartDate))<Convert(INT, Right(M.Period,CHARINDEX(''-'',M.Period)+1))
								THEN ''Y''+CONVERT (NVARCHAR(4),((Convert(INT, Right(M.Period,CHARINDEX(''-'',M.Period)+1))-CONVERT(INT, DATEPART(YEAR, PT.StartDate)))*12)+ CONVERT(INT, LEFT(M.Period,CHARINDEX(''-'',M.Period)-1)))
								ELSE LEFT(M.Period,CHARINDEX(''-'',M.Period)-1) END AS Period,
									
								M.ActualValue,
								GETDATE() as CreatedDate,
								PT.Createdby,
								NULL ModifiedDate,
								NULL ModifiedBy		
						From '+@MeasureSFDCActualTableName+' M 
						INNER JOIN Plan_Campaign_Program_Tactic PT ON SUBSTRING(ISNULL(M.PulleeID,''''),0,16) = SUBSTRING(ISNULL(PT.IntegrationInstanceTacticId,''''),0,16)
						AND PT.IsDeleted=0
						AND	PT.IsDeployedToIntegration=''1'' 
						AND	PT.[Status] IN (''In-Progress'',''Approved'',''Complete'')
						WHERE M.StageTitle=''CW'''

		EXEC (@CustomQuery)

		--insert plan tactic actuals for stage title Revenue which match with measure sfdc tactics	
		SET @CustomQuery='INSERT INTO Plan_Campaign_Program_Tactic_Actual
						  SELECT PT.PlanTacticId,
								M.StageTitle,
								CASE WHEN CONVERT(INT, DATEPART(YEAR, PT.StartDate))<Convert(INT, Right(M.Period,CHARINDEX(''-'',M.Period)+1))
								THEN ''Y''+CONVERT (NVARCHAR(4),((Convert(INT, Right(M.Period,CHARINDEX(''-'',M.Period)+1))-CONVERT(INT, DATEPART(YEAR, PT.StartDate)))*12)+ CONVERT(INT, LEFT(M.Period,CHARINDEX(''-'',M.Period)-1)))
								ELSE LEFT(M.Period,CHARINDEX(''-'',M.Period)-1) END AS Period,
								
								M.ActualValue,
								GETDATE() as CreatedDate,
								PT.Createdby,
								NULL ModifiedDate,
								NULL ModifiedBy	
							From '+@MeasureSFDCActualTableName+' M 
							INNER JOIN Plan_Campaign_Program_Tactic PT ON SUBSTRING(ISNULL(M.PulleeID,''''),0,16) = SUBSTRING(ISNULL(PT.IntegrationInstanceTacticId,''''),0,16) 
							AND PT.IsDeleted=0
							AND	PT.IsDeployedToIntegration=''1'' 
							AND	PT.[Status] IN (''In-Progress'',''Approved'',''Complete'')
							WHERE M.StageTitle=''Revenue'''

		EXEC (@CustomQuery)
		
		--Remove linked plan tactic actuals for stage title revenue and CW which match with measure sfdc tactic's linked tactic
		SET @CustomQuery='DECLARE @LinkStartMnth INT=13
						  DECLARE @LinkEndMnth INT=24
					DELETE A FROM Plan_Campaign_Program_Tactic_Actual A 
					INNER JOIN
					(SELECT  PT.PlanTacticId
							,PT.LinkedTacticId
							,CASE WHEN DATEPART(YEAR, PTL.EndDate)-DATEPART(YEAR, PTL.StartDate)>0
							THEN 1
							ELSE 0 END IsMultiyear
					FROM
					'+@MeasureSFDCActualTableName+' M_SFDC 
					INNER JOIN Plan_Campaign_Program_Tactic PT ON SUBSTRING(ISNULL(M_SFDC.PulleeID,''''),0,16) = SUBSTRING(ISNULL(PT.IntegrationInstanceTacticId,''''),0,16) 
					AND PT.IsDeleted=0
					AND	PT.IsDeployedToIntegration=''1'' 
					AND	PT.[Status] IN (''In-Progress'',''Approved'',''Complete'')
					INNER JOIN Plan_Campaign_Program_Tactic PTL ON PT.LinkedTacticId=PTL.PlanTacticId AND PTL.IsDeleted=0
					) LinkedTactic ON A.PlanTacticId=LinkedTactic.LinkedTacticId AND (
							(LinkedTactic.IsMultiyear=1 AND  CONVERT(INT,REPLACE(ISNULL(A.Period,''Y0''),''Y'',''''))>=@LinkStartMnth AND CONVERT(INT,REPLACE(ISNULL(A.Period,''Y0''),''Y'',''''))<=@LinkEndMnth)
							OR (LinkedTactic.IsMultiyear=0))
					WHERE A.StageTitle=''Revenue'' OR A.StageTitle=''CW'''

		EXEC (@CustomQuery)

		--insert linked plan tactic actuals for stage title CW which match with measure sfdc tactic's linked tactic	
		SET @CustomQuery='INSERT INTO Plan_Campaign_Program_Tactic_Actual
							SELECT PT.LinkedTacticId PlanTacticId
									,M.StageTitle
									,CASE WHEN CONVERT(INT, DATEPART(YEAR, PTL.StartDate))<CONVERT(INT, DATEPART(YEAR, PTL.EndDate))
									THEN ''Y''+CONVERT(NVARCHAR(4), ((CONVERT(INT, DATEPART(YEAR, PTL.EndDate))-CONVERT(INT, DATEPART(YEAR, PTL.StartDate)))*12)+CONVERT(INT, LEFT(M.Period,CHARINDEX(''-'',M.Period)-1)))
									ELSE LEFT(M.Period,CHARINDEX(''-'',M.Period)-1) END AS Period
									,M.ActualValue
									,GETDATE() as CreatedDate
									,PTL.Createdby,
									NULL ModifiedDate,
									NULL ModifiedBy	
							 FROM 
							'+@MeasureSFDCActualTableName+' M 
							INNER JOIN Plan_Campaign_Program_Tactic PT ON SUBSTRING(ISNULL(M.PulleeID,''''),0,16) = SUBSTRING(ISNULL(PT.IntegrationInstanceTacticId,''''),0,16) 
							AND PT.IsDeleted=0
							AND	PT.IsDeployedToIntegration=''1'' 
							AND	PT.[Status] IN (''In-Progress'',''Approved'',''Complete'')
							INNER JOIN Plan_Campaign_Program_Tactic PTL ON PT.LinkedTacticId=PTL.PlanTacticId AND PTL.IsDeleted=0
							WHERE M.StageTitle=''CW'''

		EXEC (@CustomQuery)

		--insert linked plan tactic actuals for stage title Revenue which match with measure sfdc tactic's linked tactic	
		SET @CustomQuery='INSERT INTO Plan_Campaign_Program_Tactic_Actual
		SELECT PT.LinkedTacticId PlanTacticId
				,M.StageTitle
				,CASE WHEN CONVERT(INT, DATEPART(YEAR, PTL.StartDate))<CONVERT(INT, DATEPART(YEAR, PTL.EndDate))
				THEN ''Y''+CONVERT(NVARCHAR(4), ((CONVERT(INT, DATEPART(YEAR, PTL.EndDate))-CONVERT(INT, DATEPART(YEAR, PTL.StartDate)))*12)+CONVERT(INT, LEFT(M.Period,CHARINDEX(''-'',M.Period)-1)))
				ELSE LEFT(M.Period,CHARINDEX(''-'',M.Period)-1) END AS Period
				,M.ActualValue
				,GETDATE() as CreatedDate
				,PTL.Createdby
				,NULL ModifiedDate
				,NULL ModifiedBy	
		 FROM 
		'+@MeasureSFDCActualTableName+' M 
		INNER JOIN Plan_Campaign_Program_Tactic PT ON SUBSTRING(ISNULL(M.PulleeID,''''),0,16) = SUBSTRING(ISNULL(PT.IntegrationInstanceTacticId,''''),0,16) 
		AND PT.IsDeleted=0
		AND	PT.IsDeployedToIntegration=''1'' 
		AND	PT.[Status] IN (''In-Progress'',''Approved'',''Complete'')
		INNER JOIN Plan_Campaign_Program_Tactic PTL ON PT.LinkedTacticId=PTL.PlanTacticId AND PTL.IsDeleted=0
		WHERE M.StageTitle=''Revenue'''

		EXEC (@CustomQuery)
END

GO

/****** Object:  StoredProcedure [INT].[PullMQL]    Script Date: 08/20/2016 18:07:10 ******/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[INT].[PullMQL]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [INT].[PullMQL] AS' 
END
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [INT].[PullMQL] 
	@NewSFDCActualTableName varchar(255)='',
	@clientId uniqueidentifier
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	Declare @query nvarchar(max)='' 
   SET @query =''
			SET @query= @query + '
									Declare @entTacIds nvarchar(max)=''''
									Declare @mqlStageTitle varchar(80)=''MQL''
									Declare @strtLnkMonth int = 13
									Declare @endLnkMonth int = 24
									Declare @mqlStageCode varchar(50)=''MQL''
									Declare @sfdcTypeCode varchar(50)=''Salesforce''
								 '
			SET @query= @query + '
									IF EXISTS ( SELECT ClientIntegrationPermissionId 
												FROM Client_Integration_Permission 
												WHERE IntegrationTypeId IN (
																			 SELECT IntegrationTypeId 
																			 FROM IntegrationType 
																			 WHERE Code =@sfdcTypeCode
																			) 
												AND PermissionCode = @mqlStageTitle 
												AND ClientId = '''+Cast(@clientId as varchar(50))+''''
			SET @query= @query + ' 
			BEGIN

			SELECT @entTacIds= ISNULL(@entTacIds + '','','''') + cast(tac.PlanTacticId as varchar) 
			FROM '+@NewSFDCActualTableName+' as msrData
			JOIN Plan_Campaign_Program_Tactic as tac 
			ON SubSTRING(IsNull(msrData.PulleeID,''''),0,16) = SubSTRING(IsNull(tac.IntegrationInstanceTacticId,''''),0,16)
			WHERE msrData.StageTitle=@mqlStageCode

			-- START: Insert Actual values data to temp table.

			Declare @insertTacActTable Table(PlanTacticId int,StageTitle varchar(50), Period varchar(5), Actualvalue float, CreatedDate datetime, CreatedBy uniqueidentifier, ModifiedDate datetime, ModifiedBy uniqueidentifier) 

			INSERT
			INTO @insertTacActTable
			SELECT tac.PlanTacticId,
					@mqlStageTitle,
					CASE	
						WHEN YEAR(tac.StartDate) < YEAR(tac.EndDate) 
						THEN
							CASE 
								WHEN YEAR(tac.StartDate) < CAST( RIGHT(msrData.Period, LEN(msrData.Period) - CHARINDEX(''-'',msrData.Period)) as INT )	-- IF Tactic start date year less than Period year then convert Period to multi year ex. 2015 < 2016
								THEN ''Y'' + Cast( ( ( ( CAST( RIGHT(msrData.Period, LEN(msrData.Period) - CHARINDEX(''-'',msrData.Period)) as INT ) - YEAR(tac.StartDate) ) * 12 ) + CAST( LEFT(msrData.Period,ChARINDEX(''-'',msrData.Period)-1) as INT ) ) as varchar(2) ) -- convert period to multi year :  = ( (2016 - 2015) * 12 ) + 3 = Y15
								ELSE ''Y'' + LEFT(msrData.Period,CHARINDEX(''-'',msrData.Period)-1)
							END
					ELSE
						CASE
							WHEN YEAR(tac.StartDate) = CAST( RIGHT(msrData.Period, LEN(msrData.Period) - CHARINDEX(''-'',msrData.Period)) as INT )
							THEN ''Y'' + LEFT(msrData.Period,CHARINDEX(''-'',msrData.Period)-1)
						END
					END AS Period,
					msrData.ActualValue,
					GetDate() as CreatedDate,
					tac.CreatedBy,
					Null as ModifiedDate,
					Null as ModifiedBy
			FROM '+@NewSFDCActualTableName+' as msrData
			INNER JOIN  Plan_Campaign_Program_Tactic as tac 
			ON			SubSTRING(IsNull(msrData.PulleeID,''''),0,16) = SubSTRING(IsNull(tac.IntegrationInstanceTacticId,''''),0,16)
			AND			IsNull(Period,'''')<>''''
			WHERE		msrData.StageTitle=@mqlStageCode
			-- END: Insert Actual values data to temp table.

			-- START: Delete existing actual values.

			DELETE Plan_Campaign_Program_Tactic_Actual 
			FROM Plan_Campaign_Program_Tactic_Actual as act
			JOIN 
			(
				-- Get Multi year tactic
				SELECT tact1.PlanTacticId,RN = ROW_NUMBER() OVER (PARTITION BY CASE 
																					WHEN  tact1.PlanTacticId < tact1.LinkedTacticId THEN CAST(tact1.PlanTacticId AS NVARCHAR) + '':'' + CAST (tact1.LinkedTacticId AS NVARCHAR)  
																					ELSE CAST (tact1.LinkedTacticId AS NVARCHAR) + '':'' + CAST(tact1.PlanTacticId AS NVARCHAR) 
																				END 
																   ORDER BY pln.[Year] DESC)
				FROM Plan_Campaign_Program_Tactic as tact1
				JOIN Plan_Campaign_Program as prg on tact1.PlanProgramId = prg.PlanProgramId
				JOIN Plan_Campaign as camp on prg.PlanCampaignId = camp.PlanCampaignId
				JOIN [Plan] as pln on camp.PlanId = pln.PlanId
				WHERE LinkedTacticId > 0 and PlanTacticId In (select val from comma_split(@entTacIds,'',''))
			
				UNION 
				
				-- Get Single year tactics
				SELECT tact1.PlanTacticId,RN = 1 
				FROM Plan_Campaign_Program_Tactic as tact1 
				WHERE PlanTacticId In (SELECT val from comma_split(@entTacIds,'','')) and IsNUll(LinkedTacticId,0) = 0
			
			) as lnkTac 
			ON act.PlanTacticId = lnkTac.PlanTacticId
			AND StageTitle = @mqlStageTitle
			JOIN @insertTacActTable as insrtAct	-- Join with Insert Actual table data to remove the duplicate data from Actual table prior to insert
			ON act.PlanTacticId = insrtAct.PlanTacticId 
			AND IsNull(insrtAct.Period,'''') <> '''' 
			AND (
					(lnkTac.RN > 1 and ( (Cast(Replace(IsNull(act.Period,''Y0''),''Y'','''') as int) between @strtLnkMonth and @endLnkMonth) OR ( insrtAct.Period = act.Period ) )  -- Remove Y13 to Y24 period actual values in case of multi year.
					OR (lnkTac.RN = 1)	-- Delete Y1 to Y12 period actual values in case of single year tactic.
					)
				) 
			
			-- END: Delete existing actual values.
			
			-- START: Insert Actual values to Actual table.
			INSERT INTO Plan_Campaign_Program_Tactic_Actual
			SELECT PlanTacticId,
					StageTitle,
					Period,
					Actualvalue,
					CreatedDate,
					CreatedBy,
					ModifiedDate,
					ModifiedBy
			FROM @insertTacActTable as insrtAct
			WHERE IsNull(insrtAct.Period,'''') <> ''''
			-- END: Insert Actual values to Actual table.
			'
			SET @query= @query + ' END'
			
			EXEC (@query)
END

GO
/****** Object:  StoredProcedure [INT].[PullResponses]    Script Date: 08/20/2016 18:07:10 ******/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[INT].[PullResponses]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [INT].[PullResponses] AS' 
END
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [INT].[PullResponses] 
	@NewSFDCActualTableName varchar(255)=''
AS
BEGIN
	
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	--START:- Pulling Responses
		BEGIN
			Declare @query nvarchar(max)=''
			SET @query= @query + '
									Declare @entTacIds nvarchar(max)=''''
									Declare @inqStageTitle varchar(80)=''ProjectedStageValue''
									Declare @strtLnkMonth int = 13
									Declare @endLnkMonth int = 24
									Declare @inqStageCode varchar(50)=''INQ''
								 '
			SET @query= @query + ' 

			SELECT @entTacIds= ISNULL(@entTacIds + '','','''') + cast(tac.PlanTacticId as varchar) 
			FROM '+@NewSFDCActualTableName+' as msrData
			JOIN Plan_Campaign_Program_Tactic as tac 
			ON SubSTRING(IsNull(msrData.PulleeID,''''),0,16) = SubSTRING(IsNull(tac.IntegrationInstanceTacticId,''''),0,16)
			WHERE msrData.StageTitle=@inqStageCode

			-- START: Insert Actual values data to temp table.

			Declare @insertTacActTable Table(PlanTacticId int,StageTitle varchar(50), Period varchar(5), Actualvalue float, CreatedDate datetime, CreatedBy uniqueidentifier, ModifiedDate datetime, ModifiedBy uniqueidentifier) 

			INSERT
			INTO @insertTacActTable
			SELECT tac.PlanTacticId,
					@inqStageTitle,
					CASE	
						WHEN YEAR(tac.StartDate) < YEAR(tac.EndDate) 
						THEN
							CASE 
								WHEN YEAR(tac.StartDate) < CAST( RIGHT(msrData.Period, LEN(msrData.Period) - CHARINDEX(''-'',msrData.Period)) as INT )	-- IF Tactic start date year less than Period year then convert Period to multi year ex. 2015 < 2016
								THEN ''Y'' + Cast( ( ( ( CAST( RIGHT(msrData.Period, LEN(msrData.Period) - CHARINDEX(''-'',msrData.Period)) as INT ) - YEAR(tac.StartDate) ) * 12 ) + CAST( LEFT(msrData.Period,ChARINDEX(''-'',msrData.Period)-1) as INT ) ) as varchar(2) ) -- convert period to multi year :  = ( (2016 - 2015) * 12 ) + 3 = Y15
								ELSE ''Y'' + LEFT(msrData.Period,CHARINDEX(''-'',msrData.Period)-1)
							END
					ELSE
						CASE
							WHEN YEAR(tac.StartDate) = CAST( RIGHT(msrData.Period, LEN(msrData.Period) - CHARINDEX(''-'',msrData.Period)) as INT )
							THEN ''Y'' + LEFT(msrData.Period,CHARINDEX(''-'',msrData.Period)-1)
						END
					END AS Period,
					msrData.ActualValue,
					GetDate() as CreatedDate,
					tac.CreatedBy,
					Null as ModifiedDate,
					Null as ModifiedBy
			FROM '+@NewSFDCActualTableName+' as msrData
			INNER JOIN  Plan_Campaign_Program_Tactic as tac 
			ON			SubSTRING(IsNull(msrData.PulleeID,''''),0,16) = SubSTRING(IsNull(tac.IntegrationInstanceTacticId,''''),0,16)
			AND			IsNull(Period,'''')<>''''
			WHERE		msrData.StageTitle=@inqStageCode
			-- END: Insert Actual values data to temp table.

			-- START: Delete existing actual values.

			DELETE Plan_Campaign_Program_Tactic_Actual 
			FROM Plan_Campaign_Program_Tactic_Actual as act
			JOIN 
			(
				-- Get Multi year tactic
				SELECT tact1.PlanTacticId,RN = ROW_NUMBER() OVER (PARTITION BY CASE 
																					WHEN  tact1.PlanTacticId < tact1.LinkedTacticId THEN CAST(tact1.PlanTacticId AS NVARCHAR) + '':'' + CAST (tact1.LinkedTacticId AS NVARCHAR)  
																					ELSE CAST (tact1.LinkedTacticId AS NVARCHAR) + '':'' + CAST(tact1.PlanTacticId AS NVARCHAR) 
																				END 
																   ORDER BY pln.[Year] DESC)
				FROM Plan_Campaign_Program_Tactic as tact1
				JOIN Plan_Campaign_Program as prg on tact1.PlanProgramId = prg.PlanProgramId
				JOIN Plan_Campaign as camp on prg.PlanCampaignId = camp.PlanCampaignId
				JOIN [Plan] as pln on camp.PlanId = pln.PlanId
				WHERE LinkedTacticId > 0 and PlanTacticId In (select val from comma_split(@entTacIds,'',''))
			
				UNION 
				
				-- Get Single year tactics
				SELECT tact1.PlanTacticId,RN = 1 
				FROM Plan_Campaign_Program_Tactic as tact1 
				WHERE PlanTacticId In (SELECT val from comma_split(@entTacIds,'','')) and IsNUll(LinkedTacticId,0) = 0
			
			) as lnkTac 
			ON act.PlanTacticId = lnkTac.PlanTacticId
			AND StageTitle = @inqStageTitle
			JOIN @insertTacActTable as insrtAct	-- Join with Insert Actual table data to remove the duplicate data from Actual table prior to insert
			ON act.PlanTacticId = insrtAct.PlanTacticId 
			AND IsNull(insrtAct.Period,'''') <> '''' 
			AND (
					(lnkTac.RN > 1 and ( (Cast(Replace(IsNull(act.Period,''Y0''),''Y'','''') as int) between @strtLnkMonth and @endLnkMonth) OR ( insrtAct.Period = act.Period ) )  -- Remove Y13 to Y24 period actual values in case of multi year.
					OR (lnkTac.RN = 1)	-- Delete Y1 to Y12 period actual values in case of single year tactic.
					)
				) 
			
			-- END: Delete existing actual values.
			
			-- START: Insert Actual values to Actual table.
			INSERT INTO Plan_Campaign_Program_Tactic_Actual
			SELECT PlanTacticId,
					StageTitle,
					Period,
					Actualvalue,
					CreatedDate,
					CreatedBy,
					ModifiedDate,
					ModifiedBy
			FROM @insertTacActTable as insrtAct
			WHERE IsNull(insrtAct.Period,'''') <> ''''
			-- END: Insert Actual values to Actual table.
			'
			EXEC (@query)
		END
		--END:- Pulling Responses

END

GO
/****** Object:  StoredProcedure [INT].[RemoveMeasureSFDCActualTable]    Script Date: 08/20/2016 18:07:10 ******/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[INT].[RemoveMeasureSFDCActualTable]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [INT].[RemoveMeasureSFDCActualTable] AS' 
END
GO
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

GO

/****** Object:  StoredProcedure [INT].[PullMeasureSFDCActual]    Script Date: 08/23/2016 12:53:22 ******/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[INT].[PullMeasureSFDCActual]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [INT].[PullMeasureSFDCActual] AS' 
END
GO
-- =============================================
-- Author:Mitesh Vaishnav
-- Create date:12/08/2016
-- Description:	pull actuals from Measure database
-- =============================================
ALTER PROCEDURE [INT].[PullMeasureSFDCActual]
	(
	@ClientId nvarchar(36),
	@AuthDatabaseName Nvarchar(1000)
	)
AS
BEGIN

--set integration instance id which have type as "Measure Actual"
DECLARE @IntegrationInstanceId INT=0
DECLARE @IntegrationInstanceUserId NVARCHAR(36)=''
DECLARE @SectionName NVARCHAR(1000)=''
DECLARE @IntegrationInstanceLogId INT=0 -- set output perameter in this veriable of add log function

	SELECT TOP 1 @IntegrationInstanceId=I.IntegrationInstanceId
				,@IntegrationInstanceUserId=I.CreatedBy
	FROM IntegrationInstance I INNER JOIN IntegrationType It ON It.IntegrationTypeId=I.IntegrationTypeId AND It.Code='MA'
	WHERE ClientId=@ClientId
	--Add initial instance log
	EXEC [INT].[AddIntegrationInstanceLog] @IntegrationInstanceId=@IntegrationInstanceId,@UserID=@IntegrationInstanceUserId,@IntegrationInstanceLogId=@IntegrationInstanceLogId,@OutPutLogid=@IntegrationInstanceLogId OUTPUT


BEGIN TRY

DECLARE @MeasureSFDCActualTableName nvarchar(255)
DECLARE @CustomQuery NVARCHAR(MAX)
DECLARE @IntegrationInstanceSectionLogId INT=0
DECLARE @tempDbName TABLE
	(
		DbName NVARCHAR(100)
	)

	--Fetch name of the temp table at where SFDC data inserted in plan database
	SET @SectionName='Create Measure Actuals Temp Table'
	EXEC [INT].[AddIntegrationInstanceSectionLog] @IntegrationInstanceId=@IntegrationInstanceId
												,@UserID=@IntegrationInstanceUserId
												,@IntegrationInstanceLogId=@IntegrationInstanceLogId
												,@sectionName=@SectionName
												,@OutPutLogId=@IntegrationInstanceSectionLogId OUTPUT

			--INSERT INTO @tempDbName
			EXEC [INT].[CreateMeasureSFDCActualTable] @ClientId=@ClientId,@AuthDatabaseName=@AuthDatabaseName,@SFDCActualTempTable=@MeasureSFDCActualTableName OUTPUT

			--SELECT @MeasureSFDCActualTableName=DbName from @tempDbName

		IF (@MeasureSFDCActualTableName IS NOT NULL)
		BEGIN
		EXEC [INT].[AddIntegrationInstanceSectionLog] @IntegrationInstanceId=@IntegrationInstanceId
												,@UserID=@IntegrationInstanceUserId
												,@IntegrationInstanceLogId=@IntegrationInstanceLogId
												,@sectionName=@SectionName
												,@Status='Success'
												,@OutPutLogId=@IntegrationInstanceSectionLogId OUTPUT


		--START:- Pulling CW
		SET @SectionName='Pulling CW'
		EXEC [INT].[AddIntegrationInstanceSectionLog] @IntegrationInstanceId=@IntegrationInstanceId
												,@UserID=@IntegrationInstanceUserId
												,@IntegrationInstanceLogId=@IntegrationInstanceLogId
												,@sectionName=@SectionName
												,@OutPutLogId=@IntegrationInstanceSectionLogId OUTPUT

		EXEC [INT].[PullCw] @MeasureSFDCActualTableName

		EXEC [INT].[AddIntegrationInstanceSectionLog] @IntegrationInstanceId=@IntegrationInstanceId
												,@UserID=@IntegrationInstanceUserId
												,@IntegrationInstanceLogId=@IntegrationInstanceLogId
												,@sectionName=@SectionName
												,@Status='Success'
												,@OutPutLogId=@IntegrationInstanceSectionLogId OUTPUT
												
		--END:- Pulling CW

		--START:- Pulling Responses
		BEGIN
		SET @SectionName='Pulling Responses'
		EXEC [INT].[AddIntegrationInstanceSectionLog] @IntegrationInstanceId=@IntegrationInstanceId
												,@UserID=@IntegrationInstanceUserId
												,@IntegrationInstanceLogId=@IntegrationInstanceLogId
												,@sectionName=@SectionName
												,@OutPutLogId=@IntegrationInstanceSectionLogId OUTPUT

		EXEC [INT].[PullResponses] @MeasureSFDCActualTableName

		EXEC [INT].[AddIntegrationInstanceSectionLog] @IntegrationInstanceId=@IntegrationInstanceId
										,@UserID=@IntegrationInstanceUserId
										,@IntegrationInstanceLogId=@IntegrationInstanceLogId
										,@sectionName=@SectionName
										,@Status='Success'
										,@OutPutLogId=@IntegrationInstanceSectionLogId OUTPUT
		END
		--END:- Pulling Responses

		--START:- Pulling MQL
		BEGIN
		SET @SectionName='Pulling MQL'
		EXEC [INT].[AddIntegrationInstanceSectionLog] @IntegrationInstanceId=@IntegrationInstanceId
												,@UserID=@IntegrationInstanceUserId
												,@IntegrationInstanceLogId=@IntegrationInstanceLogId
												,@sectionName=@SectionName
												,@OutPutLogId=@IntegrationInstanceSectionLogId OUTPUT
			     EXEC [INT].[PullMQL] @MeasureSFDCActualTableName,@ClientId 

		EXEC [INT].[AddIntegrationInstanceSectionLog] @IntegrationInstanceId=@IntegrationInstanceId
										,@UserID=@IntegrationInstanceUserId
										,@IntegrationInstanceLogId=@IntegrationInstanceLogId
										,@sectionName=@SectionName
										,@Status='Success'
										,@OutPutLogId=@IntegrationInstanceSectionLogId OUTPUT
		END
		--END:- Pulling MQL

		--Remove SFDC table which will created from Measure database function
		EXEC [INT].[RemoveMeasureSFDCActualTable] @MeasureSFDCActualTableName=@MeasureSFDCActualTableName
		END
	ELSE
		BEGIN
		EXEC [INT].[AddIntegrationInstanceSectionLog] @IntegrationInstanceId=@IntegrationInstanceId
												,@UserID=@IntegrationInstanceUserId
												,@IntegrationInstanceLogId=@IntegrationInstanceLogId
												,@sectionName=@SectionName
												,@Status='Error'
												,@ErrorDescription='Measure Sfdc Actual table not created.'
												,@OutPutLogId=@IntegrationInstanceSectionLogId OUTPUT
		END

		EXEC [INT].[AddIntegrationInstanceLog] @IntegrationInstanceId=@IntegrationInstanceId
										,@UserID=@IntegrationInstanceUserId,
										@IntegrationInstanceLogId=@IntegrationInstanceLogId,
										@Status='Success',
										@OutPutLogid=@IntegrationInstanceLogId OUTPUT
END TRY
BEGIN CATCH
DECLARE @ErrorMsg NVARCHAR(MAX)
SELECT  @ErrorMsg=ERROR_MESSAGE()
EXEC [INT].[AddIntegrationInstanceSectionLog] @IntegrationInstanceId=@IntegrationInstanceId
												,@UserID=@IntegrationInstanceUserId
												,@IntegrationInstanceLogId=@IntegrationInstanceLogId
												,@sectionName=@SectionName
												,@Status='Error'
												,@ErrorDescription=@ErrorMsg
												,@OutPutLogId=@IntegrationInstanceSectionLogId OUTPUT

EXEC [INT].[AddIntegrationInstanceLog] @IntegrationInstanceId=@IntegrationInstanceId
										,@UserID=@IntegrationInstanceUserId,
										@IntegrationInstanceLogId=@IntegrationInstanceLogId,
										@Status='Error',
										@ErrorDescription=@ErrorMsg,
										@OutPutLogid=@IntegrationInstanceLogId OUTPUT
END CATCH
END

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
