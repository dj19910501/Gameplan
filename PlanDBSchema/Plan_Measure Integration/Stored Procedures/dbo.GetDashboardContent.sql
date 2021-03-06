IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'GetDashboardContent') AND xtype IN (N'P'))
    DROP PROCEDURE [GetDashboardContent]
GO

CREATE PROCEDURE [dbo].[GetDashboardContent]
(
	@HomepageId INT = 0,
	@DashboardId INT = 0,
	@DashboardPageId INT = 0,
	@UserId UNIQUEIDENTIFIER = '14d7d588-cf4d-46be-b4ed-a74063b67d66'
)
AS
BEGIN
	DECLARE @ColumnNames NVARCHAR(MAX) 
	SELECT @ColumnNames = COALESCE(@ColumnNames + ', ', '') +  AttributeKey FROM (SELECT DISTINCT  '[' + AttributeKey + ']' AttributeKey FROM ChartOptionAttribute)  A
	IF(@ColumnNames IS NULL OR @ColumnNames ='')
		SET @ColumnNames = 'X'
		IF(@HomepageId != 0)
		BEGIN
			--First Query
			SELECT HC.Id, HC.DashboardContentId, HC.KeyDataId, HC.DashboardId, HC.RowNumber, HC.DisplayOrder, HC.DisplayTitle, HC.Height, HC.Width, HC.DisplayDateStamp, D.* 
			FROM Dashboard D INNER JOIN HomePageContent HC ON HC.DashboardId = D.id  LEFT JOIN DashboardContents DC ON DC.id=HC.DashboardContentId  
			WHERE HC.HomepageID = @HomepageId AND D.IsDeleted = 0 AND ISNULL(DC.IsDeleted,0)=0  ORDER BY D.DisplayOrder;
				
			--Second Query
			;WITH Data AS( 
				SELECT 
						D.Rows,
						D.Columns,
						DC.ReportTableId,
						ISNULL(DC.HelpTextId, 0) HelpTextId,
						ISNULL(T.ShowFooterRow,0) ShowFooterRow,
						ISNULL(T.IsGoalDisplay,0) IsGoalDisplay,
						ISNULL(T.IsGoalDisplayForFilterCritaria,0) IsGoalDisplayForFilterCritaria,
						ISNULL(T.DateRangeGoalOption,1) DateRangeGoalOption,
						ISNULL(T.IsComparisonDisplay,0) IsComparisonDisplay,
						ISNULL(T.ComparisonType,0) ComparisonType,
						ISNULL(T.IsGoalDisplayAsColumn,0) IsGoalDisplayAsColumn,
						DC.ReportGraphId,
						DC.DisplayName,
						DC.Height,
						DC.Width, 
						ROW_NUMBER() OVER (PARTITION BY D.id ORDER BY Dc.DisplayOrder) DisplayOrder,
						D.id DashboardId,
						IsSubDashboard = CASE WHEN D.ParentDashboardId IS NULL THEN 1 ELSE 0 END,
						DC.id DashboardContentId,
						DisplayIfZero = ISNULL(DC.DisplayIfZero,''),
						SortColumnNumber = ISNULL(T.SortColumnNumber,1),
						SortDirection = UPPER(ISNULL(T.SortDirection,'ASC')) 
				FROM Dashboard D 
					INNER JOIN  DashboardContents DC on D.id = DC.DashboardId 
					INNER JOIN  HomepageContent HC on DC.id = HC.DashboardContentId 
					LEFT JOIN ReportTable T ON T.id = DC.ReportTableId 
				WHERE 
					D.IsDeleted=0 AND 
					DC.IsDeleted=0
				),
				RowNumData AS ( 
					SELECT 
							*,
							RowNum = CASE WHEN DisplayOrder % Columns <> 0 THEN (DisplayOrder / Columns) + 1  ELSE (DisplayOrder / Columns) END FROM Data 
				),
				CalculatedData AS (
					SELECT 
							*,
							CalculatedWidth1 = CASE WHEN Width IS NOT NULL THEN Width ELSE CAST((100 - SUM(Width) OVER (PARTITION BY RowNum)) AS DECIMAL)/CAST((COUNT(*) OVER (PARTITION BY RowNum) - COUNT(Width) OVER (PARTITION BY RowNum)) AS DECIMAL) END,
							CalculatedHeight1 = MAX(Height) OVER (PARTITION BY RowNum) 
					FROM RowNumData
				)
				SELECT 
						*,
						CalculatedHeight = CASE WHEN CalculatedHeight1 IS NULL THEN 350 ELSE CalculatedHeight1 END, 
						CalculatedWidth = CASE WHEN CalculatedWidth1 IS NULL THEN CAST(100/Columns AS DECIMAL) ELSE CalculatedWidth1 END ,
						AxisCount = CASE WHEN ReportTableId IS NOT NULL THEN  (SELECT COUNT(*) FROM ReportTableDimension WHERE ReportTableDimension.ReportTableId = CalculatedData.ReportTableId) ELSE  (SELECT COUNT(*) FROM ReportAxis WHERE ReportAxis.ReportGraphId = CalculatedData.ReportGraphId) END,
						IsDateDimensionOnly = CASE WHEN (SELECT COUNT(*) FROM ReportAxis A WHERE A.ReportGraphId = CalculatedData.ReportGraphId) = 1 THEN  CASE WHEN (SELECT ISNULL(Dimension.IsDateDimension,0) FROM ReportAxis INNER JOIN Dimension ON Dimension.Id = ReportAxis.Dimensionid WHERE ReportAxis.ReportGraphId = CalculatedData.ReportGraphId) = 1 THEN 1 ELSE 0 END ELSE 0 END,
						DisplayAsColumn = (SELECT COUNT(*) FROM ReportTableDimension WHERE ReportTableDimension.ReportTableId = CalculatedData.ReportTableId AND ISNULL(DisplayAsColumn,0) = 1),
						DashboardContentId 
				FROM CalculatedData ORDER BY DisplayOrder


				exec('
						;WITH ChartAttrb AS
						(
							SELECT * FROM (
									SELECT  C1.ReportGraphId ReportGraphId1,C1.AttributeKey,C1.AttributeValue,C2.AttributeValue ColorSequenceNo FROM ChartOptionAttribute C1
									LEFT JOIN ChartOptionAttribute C2 ON C1.ReportGraphId = C2.ReportGraphId AND  C2.AttributeKey = ''ColorSequenceNumber''
							) AS R
							PIVOT 
							(
								MIN(AttributeValue)
								FOR AttributeKey IN ('+ @ColumnNames +')
							) AS A
						)

						SELECT G.id,G.id ReportGraphId,DC.DashboardId,ISNULL(DC.HelpTextId, 0) HelpTextId,HC.DashboardContentId, G.GraphType ChartType,(select top 1 symboltype from ReportGraphColumn where ReportGraphId=G.ID ) AS SymbolType,
						(select (CASE 
							WHEN ((select COUNT(*) from ReportGraphColumn where ReportGraphId=G.ID and ISNULL(TotalDecimalPlaces, -1) <> -1) > 1) 
								THEN (select ISNULL(TotalDecimalPlaces, -1) from ReportGraph where id=G.ID)
							WHEN ((select COUNT(*) from ReportGraphColumn where ReportGraphId=G.ID and ISNULL(TotalDecimalPlaces, -1) <> -1) = 0) 
								THEN (select ISNULL(TotalDecimalPlaces, -1) from ReportGraph where id=G.ID)
							ELSE
								(select top 1 TotalDecimalPlaces from ReportGraphColumn where ReportGraphId=G.ID and ISNULL(TotalDecimalPlaces, -1) <> -1)
							END)) AS TotalDecimalPlaces, 

						(select (CASE 
							WHEN ((select COUNT(*) from ReportGraphColumn where ReportGraphId=G.ID and LOWER(MagnitudeValue) IN (''k'',''m'',''b'',''t'',''q'',''3'',''6'',''9'',''12'',''15'') and ISNULL										(MagnitudeValue, ''0'') <> ''0'') > 1) 								
								THEN (select (CASE WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''k'' THEN ''3'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''m'' THEN ''6'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''b'' THEN									''9'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''t'' THEN ''12'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''q'' THEN ''15'' WHEN (LOWER(ISNULL(MagnitudeValue,0)) IN													(''3'',''6'',''9'',''12'',''15'')) THEN MagnitudeValue ELSE 0 END) as MagnitudeValue 
									from ReportGraph where id=G.ID)
							WHEN ((select COUNT(*) from ReportGraphColumn where ReportGraphId=G.ID and LOWER(MagnitudeValue) IN (''k'',''m'',''b'',''t'',''q'',''3'',''6'',''9'',''12'',''15'') and ISNULL										(MagnitudeValue, ''0'') <> ''0'') = 0)
								THEN (select (CASE WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''k'' THEN ''3'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''m'' THEN ''6'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''b'' THEN								''9'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''t'' THEN ''12'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''q'' THEN ''15'' WHEN (LOWER(ISNULL(MagnitudeValue,0)) IN													(''3'',''6'',''9'',''12'',''15'')) THEN MagnitudeValue ELSE 0 END) as MagnitudeValue 
								from ReportGraph where id=G.ID)	
							ELSE		
								(select top 1 (CASE WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''k'' THEN ''3'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''m'' THEN ''6'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''b'' THEN								''9'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''t'' THEN ''12'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''q'' THEN ''15'' WHEN (LOWER(ISNULL(MagnitudeValue,0)) IN													(''3'',''6'',''9'',''12'',''15'')) THEN MagnitudeValue ELSE 0 END) as MagnitudeValue from ReportGraphColumn where ReportGraphId=G.ID and ISNULL(MagnitudeValue, ''0'') <> ''0'' and									LOWER(MagnitudeValue) IN (''k'',''m'',''b'',''t'',''q'',''3'',''6'',''9'',''12'',''15''))
							END)) AS MagnitudeValue,

						 G.IsLableDisplay,G.LabelPosition,G.IsLegendVisible,G.LegendPosition,G.IsDataLabelVisible,G.DataLablePosition,G.DefaultRows, G.ChartAttribute,G.IsComparisonDisplay,IsNULL(G.IsIndicatorDisplay,0) IsIndicatorDisplay,G.CustomQuery,ISNULL(G.IsGoalDisplay,0) IsGoalDisplay,ISNULL(G.IsTrendLineDisplay,0) IsTrendLineDisplay,ISNULL(IsGoalDisplayForFilterCritaria,0) IsGoalDisplayForFilterCritaria,DateRangeGoalOption, A.*, ChartColor = dbo.GetColor(A.ColorSequenceNo), ISNULL(G.IsSortByValue,0) AS IsSortByValue, G.SortOrder,ISNULL(G.DisplayGoalAsLine,0) AS DisplayGoalAsLine
						FROM HomepageContent HC
						INNER JOIN  DashboardContents DC ON HC.DashboardContentId = DC.Id 
						INNER JOIN ReportGraph G ON G.Id = DC.ReportGraphId
						LEFT OUTER JOIN ChartAttrb A ON A.ReportGraphId1 = G.id 
						WHERE HC.HomepageId = '+ @HomepageId +'
				')

				IF ((SELECT COUNT(*) FROM HomepageContent HC INNER JOIN  DashboardContents DC ON HC.DashboardContentId = DC.Id INNER JOIN ReportGraph G ON G.Id = DC.ReportGraphId WHERE HC.HomepageId = @HomepageId					and RTRIM(LTRIM(LOWER(GraphType))) = 'bullet') > 0)
				BEGIN
					SELECT RG.id, (case when RGC.TotalDecimalPlaces > 0 then RGC.TotalDecimalPlaces else RG.TotalDecimalPlaces end) AS TotalDecimalPlaces,

					(CASE WHEN RGC.MagnitudeValue IS NOT NULL THEN 
					/* In Case of NOT NULL value in ReportTableColumn table */
					(CASE 
						WHEN LOWER(RGC.MagnitudeValue) = 'k' THEN 3 
						WHEN LOWER(RGC.MagnitudeValue) = 'm' THEN 6 
						WHEN LOWER(RGC.MagnitudeValue) = 'b' THEN 9 
						WHEN LOWER(RGC.MagnitudeValue) = 't' THEN 12 
						WHEN LOWER(RGC.MagnitudeValue) = 'q' THEN 15 
						WHEN (LOWER(RGC.MagnitudeValue) IN ('3','6','9','12','15')) THEN RGC.MagnitudeValue 
						ELSE 
							/* In Case of Wrong value in ReportTableColumn table */
							(CASE 
								WHEN LOWER(ISNULL(RG.MagnitudeValue,0)) = 'k' THEN 3 
								WHEN LOWER(ISNULL(RG.MagnitudeValue,0)) = 'm' THEN 6 
								WHEN LOWER(ISNULL(RG.MagnitudeValue,0)) = 'b' THEN 9 
								WHEN LOWER(ISNULL(RG.MagnitudeValue,0)) = 't' THEN 12 
								WHEN LOWER(ISNULL(RG.MagnitudeValue,0)) = 'q' THEN 15 
								WHEN (LOWER(ISNULL(RG.MagnitudeValue,0)) IN ('3','6','9','12','15')) THEN RG.MagnitudeValue 
								ELSE 0 
							END)
						END) 
						ELSE 
						/* In Case of NULL value in ReportTableColumn table */
						(CASE 
							WHEN LOWER(ISNULL(RG.MagnitudeValue,0)) = 'k' THEN 3 
							WHEN LOWER(ISNULL(RG.MagnitudeValue,0)) = 'm' THEN 6 
							WHEN LOWER(ISNULL(RG.MagnitudeValue,0)) = 'b' THEN 9 
							WHEN LOWER(ISNULL(RG.MagnitudeValue,0)) = 't' THEN 12 
							WHEN LOWER(ISNULL(RG.MagnitudeValue,0)) = 'q' THEN 15 
							WHEN (LOWER(ISNULL(RG.MagnitudeValue,0)) IN ('3','6','9','12','15')) THEN RG.MagnitudeValue 
							ELSE 0 
						END) 
					END) AS MagnitudeValue
					FROM HomepageContent HC 
					INNER JOIN  DashboardContents DC ON HC.DashboardContentId = DC.Id 
					INNER JOIN ReportGraph RG ON RG.Id = DC.ReportGraphId and RTRIM(LTRIM(LOWER(GraphType))) = 'bullet'
					inner join ReportGraphColumn RGC on (RGC.ReportGraphId = RG.id)
					WHERE HC.HomepageId = @HomepageId
				END
		END
		ELSE IF(@DashboardId != 0)
		BEGIN
			--First Query
			SELECT D.Id,D.Name,D.DisplayName,D.Rows,D.Columns,D.ParentDashboardId FROM Dashboard D WHERE D.Id = @DashboardId AND D.IsDeleted=0 ORDER BY D.DisplayOrder;
					
			--Second Query
			;WITH Data AS( 
			SELECT 
					D.Rows,
					D.Columns,
					DC.ReportTableId,
					ISNULL(DC.HelpTextId, 0) HelpTextId,
					DC.KeyDataId,
					ISNULL(T.ShowFooterRow,0) ShowFooterRow,
					ISNULL(T.IsGoalDisplay,0) IsGoalDisplay,
					ISNULL(T.IsGoalDisplayForFilterCritaria,0) IsGoalDisplayForFilterCritaria,
					ISNULL(T.DateRangeGoalOption,1) DateRangeGoalOption,
					ISNULL(T.IsComparisonDisplay,0) IsComparisonDisplay,
				    ISNULL(T.ComparisonType,0) ComparisonType,
					ISNULL(T.IsGoalDisplayAsColumn,0) IsGoalDisplayAsColumn,
					DC.ReportGraphId,
					DC.DisplayName,
					DC.Height,
					DC.Width,
					DC.IsCumulativeData, 
					ROW_NUMBER() OVER (PARTITION BY D.id ORDER BY Dc.DisplayOrder) 
					DisplayOrder,
					D.id DashboardId,
					IsSubDashboard = CASE WHEN D.ParentDashboardId IS NULL THEN 1 ELSE 0 END,
					DC.id DashboardContentId,
					DisplayIfZero = ISNULL(DC.DisplayIfZero,''),
					SortColumnNumber = ISNULL(T.SortColumnNumber,1),
					SortDirection = UPPER(ISNULL(T.SortDirection,'ASC')) 
			FROM Dashboard D 
				INNER JOIN  DashboardContents DC on D.id = DC.DashboardId  
				--LEFT JOIN ReportGraph RG ON RG.id = DC.ReportGraphId 
				LEFT JOIN ReportTable T ON T.id = DC.ReportTableId 
			WHERE 
					(D.id = @DashboardId OR D.ParentDashboardid = @DashboardId) AND 
					ISNULL(DC.DashboardPageID,0)= @DashboardPageId AND 
					D.IsDeleted=0 AND DC.IsDeleted=0
			),
			RowNumData AS ( 
				SELECT 
						*,
						RowNum = CASE WHEN DisplayOrder % Columns <> 0 THEN (DisplayOrder / Columns) + 1  ELSE (DisplayOrder / Columns) END FROM Data ),
						CalculatedData AS (SELECT *,CalculatedWidth1 = CASE WHEN Width IS NOT NULL THEN Width ELSE CAST((100 - SUM(Width) OVER (PARTITION BY RowNum)) AS DECIMAL)/CAST((COUNT(*) OVER (PARTITION BY RowNum) - COUNT(Width) OVER (PARTITION BY RowNum)) AS DECIMAL) END,
						CalculatedHeight1 = MAX(Height) OVER (PARTITION BY RowNum) 
				FROM RowNumData
			)
			SELECT 
					*,
					CalculatedHeight = CASE WHEN CalculatedHeight1 IS NULL THEN 350 ELSE CalculatedHeight1 END, 
					CalculatedWidth = CASE WHEN CalculatedWidth1 IS NULL THEN CAST(100/Columns AS DECIMAL) ELSE CalculatedWidth1 END ,
					AxisCount = CASE WHEN ReportTableId IS NOT NULL THEN  (SELECT COUNT(*) FROM ReportTableDimension WHERE ReportTableDimension.ReportTableId = CalculatedData.ReportTableId) ELSE  (SELECT COUNT(*) FROM ReportAxis WHERE ReportAxis.ReportGraphId = CalculatedData.ReportGraphId) END,
					IsDateDimensionOnly = CASE WHEN (SELECT COUNT(*) FROM ReportAxis A WHERE A.ReportGraphId = CalculatedData.ReportGraphId) = 1 THEN  CASE WHEN (SELECT ISNULL(Dimension.IsDateDimension,0) FROM ReportAxis INNER JOIN Dimension ON Dimension.Id = ReportAxis.Dimensionid WHERE ReportAxis.ReportGraphId = CalculatedData.ReportGraphId) = 1 THEN 1 ELSE 0 END ELSE 0 END, 
					DisplayAsColumn = (SELECT COUNT(*) FROM ReportTableDimension WHERE ReportTableDimension.ReportTableId = CalculatedData.ReportTableId AND ISNULL(DisplayAsColumn,0) = 1),
					DashboardContentId 
			FROM CalculatedData 
			ORDER BY 
					DisplayOrder;
			
			--Third Query
			SELECT D.Id, D.Name, D.DisplayName, D.Rows, D.Columns, D.ParentDashboardId, 0 IsSubDashboard, ISNULL(D.HelpTextId, 0) HelpTextId FROM Dashboard D  
			INNER JOIN	(
							SELECT DISTINCT DashboardId,UserId FROM User_Permission
						) UP ON UP.DashboardId = D.id 
			WHERE ParentDashboardId = @DashboardId AND UP.UserId = @UserId AND D.IsDeleted=0 ORDER BY DisplayOrder

			SET @DashboardPageId = ISNULL(@DashboardPageId,0)
			--Forth Query
			exec('
						;WITH ChartAttrb AS
						(
							SELECT * FROM (
									SELECT  C1.ReportGraphId ReportGraphId1,C1.AttributeKey,C1.AttributeValue,C2.AttributeValue ColorSequenceNo FROM ChartOptionAttribute C1
									LEFT JOIN ChartOptionAttribute C2 ON C1.ReportGraphId = C2.ReportGraphId AND  C2.AttributeKey = ''ColorSequenceNumber''
							) AS R
							PIVOT 
							(
								MIN(AttributeValue)
								FOR AttributeKey IN ('+ @ColumnNames +')
							) AS A
						)

						SELECT  G.id,G.id ReportGraphId,D.id DashboardId, DC.Id DashboardContentId,ISNULL(DC.HelpTextId, 0) HelpTextId, G.GraphType ChartType,(select top 1 symboltype from ReportGraphColumn where ReportGraphId=G.Id ) AS SymbolType,
						--(select (CASE 
						--	WHEN ((select COUNT(*) from ReportGraphColumn where ReportGraphId=G.ID and TotalDecimalPlaces <> 0) > 1) 
						--		THEN (select TotalDecimalPlaces from ReportGraph where id=G.ID)
						--	WHEN ((select COUNT(*) from ReportGraphColumn where ReportGraphId=G.ID and TotalDecimalPlaces <> 0) = 0) 
						--		THEN (select TotalDecimalPlaces from ReportGraph where id=G.ID)
						--	ELSE
						--		(select top 1 TotalDecimalPlaces from ReportGraphColumn where ReportGraphId=G.ID and TotalDecimalPlaces <> 0)
						--	END)) AS TotalDecimalPlaces, 
						(select (CASE 
							WHEN ((select COUNT(*) from ReportGraphColumn where ReportGraphId=G.ID and ISNULL(TotalDecimalPlaces, -1) <> -1) > 1) 
								THEN (select ISNULL(TotalDecimalPlaces, -1) from ReportGraph where id=G.ID)
							WHEN ((select COUNT(*) from ReportGraphColumn where ReportGraphId=G.ID and ISNULL(TotalDecimalPlaces, -1) <> -1) = 0) 
								THEN (select ISNULL(TotalDecimalPlaces, -1) from ReportGraph where id=G.ID)
							ELSE
								(select top 1 TotalDecimalPlaces from ReportGraphColumn where ReportGraphId=G.ID and ISNULL(TotalDecimalPlaces, -1) <> -1)
							END)) AS TotalDecimalPlaces, 

						(select (CASE 
							WHEN ((select COUNT(*) from ReportGraphColumn where ReportGraphId=G.ID and LOWER(MagnitudeValue) IN (''k'',''m'',''b'',''t'',''q'',''3'',''6'',''9'',''12'',''15'') and ISNULL										(MagnitudeValue, ''0'') <> ''0'') > 1) 								
								THEN (select (CASE WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''k'' THEN ''3'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''m'' THEN ''6'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''b'' THEN									''9'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''t'' THEN ''12'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''q'' THEN ''15'' WHEN (LOWER(ISNULL(MagnitudeValue,0)) IN													(''3'',''6'',''9'',''12'',''15'')) THEN MagnitudeValue ELSE 0 END) as MagnitudeValue 
									from ReportGraph where id=G.ID)
							WHEN ((select COUNT(*) from ReportGraphColumn where ReportGraphId=G.ID and LOWER(MagnitudeValue) IN (''k'',''m'',''b'',''t'',''q'',''3'',''6'',''9'',''12'',''15'') and ISNULL										(MagnitudeValue, ''0'') <> ''0'') = 0)
								THEN (select (CASE WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''k'' THEN ''3'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''m'' THEN ''6'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''b'' THEN								''9'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''t'' THEN ''12'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''q'' THEN ''15'' WHEN (LOWER(ISNULL(MagnitudeValue,0)) IN													(''3'',''6'',''9'',''12'',''15'')) THEN MagnitudeValue ELSE 0 END) as MagnitudeValue 
								from ReportGraph where id=G.ID)	
							ELSE		
								(select top 1 (CASE WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''k'' THEN ''3'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''m'' THEN ''6'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''b'' THEN								''9'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''t'' THEN ''12'' WHEN LOWER(ISNULL(MagnitudeValue,0)) = ''q'' THEN ''15'' WHEN (LOWER(ISNULL(MagnitudeValue,0)) IN													(''3'',''6'',''9'',''12'',''15'')) THEN MagnitudeValue ELSE 0 END) as MagnitudeValue from ReportGraphColumn where ReportGraphId=G.ID and ISNULL(MagnitudeValue, ''0'') <> ''0'' and									LOWER(MagnitudeValue) IN (''k'',''m'',''b'',''t'',''q'',''3'',''6'',''9'',''12'',''15''))
							END)) AS MagnitudeValue,

						 G.IsLableDisplay,G.LabelPosition,G.IsLegendVisible,G.LegendPosition,G.IsDataLabelVisible,G.DataLablePosition,G.DefaultRows, G.ChartAttribute,G.IsComparisonDisplay,IsNULL(G.IsIndicatorDisplay,0) IsIndicatorDisplay,ISNULL(G.IsTrendLineDisplay,0) IsTrendLineDisplay,G.CustomQuery,ISNULL(G.IsGoalDisplay,0) IsGoalDisplay,ISNULL(IsGoalDisplayForFilterCritaria,0) IsGoalDisplayForFilterCritaria,DateRangeGoalOption, A.*, ChartColor = dbo.GetColor(A.ColorSequenceNo), ISNULL(G.IsSortByValue,0) AS IsSortByValue, G.SortOrder, ISNULL(G.DisplayGoalAsLine,0) AS DisplayGoalAsLine
						FROM Dashboard D
						INNER JOIN  DashboardContents DC ON D.Id = DC.DashboardId 
						INNER JOIN ReportGraph G ON G.Id = DC.ReportGraphId
						LEFT OUTER JOIN ChartAttrb A ON A.ReportGraphId1 = G.id 
						WHERE (D.Id  = '+ @DashboardId +' OR D.ParentDashboardId  = '+ @DashboardId +') AND ISNULL(DC.DashboardPageID,0) = '+ @DashboardPageId +' AND 
						D.IsDeleted=0 AND DC.IsDeleted=0
				')
			IF ((SELECT COUNT(*) FROM Dashboard D 
				INNER JOIN  DashboardContents DC ON D.Id = DC.DashboardId 
				INNER JOIN ReportGraph RG ON RG.Id = DC.ReportGraphId and RTRIM(LTRIM(LOWER(GraphType))) = 'bullet'
				WHERE (D.Id  = @DashboardId OR D.ParentDashboardId  = @DashboardId) AND ISNULL(DC.DashboardPageID,0) = @DashboardPageId AND 
					D.IsDeleted=0 AND DC.IsDeleted=0) > 0)
			BEGIN
				SELECT RG.id, (case when RGC.TotalDecimalPlaces > 0 then RGC.TotalDecimalPlaces else RG.TotalDecimalPlaces end) AS TotalDecimalPlaces,

				(CASE WHEN RGC.MagnitudeValue IS NOT NULL THEN 
				/* In Case of NOT NULL value in ReportTableColumn table */
				(CASE 
					WHEN LOWER(RGC.MagnitudeValue) = 'k' THEN 3 
					WHEN LOWER(RGC.MagnitudeValue) = 'm' THEN 6 
					WHEN LOWER(RGC.MagnitudeValue) = 'b' THEN 9 
					WHEN LOWER(RGC.MagnitudeValue) = 't' THEN 12 
					WHEN LOWER(RGC.MagnitudeValue) = 'q' THEN 15 
					WHEN (LOWER(RGC.MagnitudeValue) IN ('3','6','9','12','15')) THEN RGC.MagnitudeValue 
					ELSE 
						/* In Case of Wrong value in ReportTableColumn table */
						(CASE 
							WHEN LOWER(ISNULL(RG.MagnitudeValue,0)) = 'k' THEN 3 
							WHEN LOWER(ISNULL(RG.MagnitudeValue,0)) = 'm' THEN 6 
							WHEN LOWER(ISNULL(RG.MagnitudeValue,0)) = 'b' THEN 9 
							WHEN LOWER(ISNULL(RG.MagnitudeValue,0)) = 't' THEN 12 
							WHEN LOWER(ISNULL(RG.MagnitudeValue,0)) = 'q' THEN 15 
							WHEN (LOWER(ISNULL(RG.MagnitudeValue,0)) IN ('3','6','9','12','15')) THEN RG.MagnitudeValue 
							ELSE 0 
						END)
					END) 
					ELSE 
					/* In Case of NULL value in ReportTableColumn table */
					(CASE 
						WHEN LOWER(ISNULL(RG.MagnitudeValue,0)) = 'k' THEN 3 
						WHEN LOWER(ISNULL(RG.MagnitudeValue,0)) = 'm' THEN 6 
						WHEN LOWER(ISNULL(RG.MagnitudeValue,0)) = 'b' THEN 9 
						WHEN LOWER(ISNULL(RG.MagnitudeValue,0)) = 't' THEN 12 
						WHEN LOWER(ISNULL(RG.MagnitudeValue,0)) = 'q' THEN 15 
						WHEN (LOWER(ISNULL(RG.MagnitudeValue,0)) IN ('3','6','9','12','15')) THEN RG.MagnitudeValue 
						ELSE 0 
					END) 
				END) AS MagnitudeValue
				FROM Dashboard D 
				INNER JOIN  DashboardContents DC ON D.Id = DC.DashboardId 
				INNER JOIN ReportGraph RG ON RG.Id = DC.ReportGraphId and RTRIM(LTRIM(LOWER(GraphType))) = 'bullet'
				inner join ReportGraphColumn RGC on (RGC.ReportGraphId = RG.id)
				WHERE (D.Id  = @DashboardId OR D.ParentDashboardId  = @DashboardId) AND ISNULL(DC.DashboardPageID,0) = @DashboardPageId AND 
					D.IsDeleted=0 AND DC.IsDeleted=0
			END
		END
END

Go