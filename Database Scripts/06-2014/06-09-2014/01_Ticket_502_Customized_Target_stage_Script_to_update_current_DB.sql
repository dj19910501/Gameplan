-- Run this script on MRP Database

-- =========================================== Start - Script- 1 (Add ConversionTitle Column and ADS entry in Stage Table) ==================================================================

IF EXISTS (select 1 from INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Stage')
BEGIN
	
	-- ======================================= Add 'ConversionTitle' column into Stage table ====================================================================
	IF NOT EXISTS (select 1 from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Stage' AND COLUMN_NAME = 'ConversionTitle')
	BEGIN
		ALTER TABLE [dbo].Stage ADD ConversionTitle NVARCHAR(255) NULL
	END
	ELSE
		PRINT('ConversionTitle already exists in Stage table.')

	-- ======================================= Add 'ADS' Title entry for each client into Stage table ====================================================================	
	IF (SELECT Count(*) FROM Stage WHERE Code = 'ADS') = 0
	BEGIN
		INSERT INTO [dbo].[Stage] (ClientId, Title, Description, ColorCode, IsDeleted, CreatedDate, CreatedBy, Code)
		SELECT DISTINCT ClientId, 'ADS', 'Average Deal Size', '2aa9eb', 0, GETDATE(), CreatedBy, 'ADS' 
		FROM Stage 
		WHERE ISNULL(IsDeleted, 0) = 0
	END
END
ELSE 
	PRINT('Stage does not exists.')

-- =========================================== End - Script- 1 (Add ConversionTitle Column and ADS entry in Stage Table) ==================================================================

GO

-- =========================================== Start - Script- 2 (BestInClass drop MetricId and Add StageId and StageType) ==================================================================

BEGIN TRANSACTION

-- ======================================== Truncate 'BestInClass' data, Drop 'MetricId' Column and add 'StageId' and 'StageType' field. Populate 'BestInClass' again. =========================
IF EXISTS (select 1 from INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'BestInClass') AND
	NOT EXISTS (select 1 from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'BestInClass' AND COLUMN_NAME = 'StageId')
BEGIN
		IF EXISTS (select 1 from INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'BestInClass_Bckp')
		BEGIN
			DROP TABLE BestInClass_Bckp
		END	

		select * into BestInClass_Bckp from BestInClass
		
		IF NOT EXISTS (select 1 from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'BestInClass' AND COLUMN_NAME = 'StageId')
		BEGIN
			TRUNCATE TABLE BestInClass
		END
		ELSE
			PRINT('Data has been migrated.')
		

		IF EXISTS (select 1 from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME = 'FK_BestInClass_Metric' AND CONSTRAINT_SCHEMA = 'dbo')
		BEGIN
			ALTER TABLE BestInClass DROP CONSTRAINT FK_BestInClass_Metric
		END
		ELSE
			PRINT('FK_BestInClass_Metric constraints does not exists in BestInClass table')
		
		IF OBJECT_ID('UniqueBICStage', 'UQ') IS NOT NULL
		BEGIN
			ALTER TABLE BestInClass DROP CONSTRAINT UniqueBICStage
		END
		ELSE
			PRINT('UniqueBICStage constraints does not exists in BestInClass table')

		IF EXISTS (select 1 from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'BestInClass' AND COLUMN_NAME = 'MetricId')
		BEGIN
			ALTER TABLE BestInClass DROP COLUMN MetricId
		END
		ELSE 
			PRINT('MetricId column does not exists in BestInClass')

		IF NOT EXISTS (select 1 from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'BestInClass' AND COLUMN_NAME = 'StageId')
		BEGIN
			ALTER TABLE BestInClass ADD StageId INT NOT NULL
		END
		ELSE
			PRINT('StageId column already exists in BestInClass')

		IF NOT EXISTS (select 1 from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'BestInClass' AND COLUMN_NAME = 'StageType')
		BEGIN
			ALTER TABLE BestInClass ADD StageType NVARCHAR(10) NOT NULL
		END
		ELSE
			PRINT('StageType column already exists in BestInClass')

		IF NOT EXISTS (select 1 from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME = 'FK_BestInClass_Stage' AND CONSTRAINT_SCHEMA = 'dbo')
		BEGIN
			ALTER TABLE [dbo].[BestInClass]  WITH CHECK ADD  CONSTRAINT [FK_BestInClass_Stage] FOREIGN KEY([StageId])
			REFERENCES [dbo].[Stage] ([StageId])
			
			ALTER TABLE [dbo].[BestInClass] CHECK CONSTRAINT [FK_BestInClass_Stage]
		END
		ELSE
			PRINT('FK_BestInClass_Stage constraint already exists in BestInClass')
		
		IF OBJECT_ID('UniqueBICStage', 'UQ') IS NULL
		BEGIN
			ALTER TABLE BestInClass ADD CONSTRAINT UniqueBICStage UNIQUE (BestInClassId, StageId, StageType)
		END
		ELSE
			PRINT('UniqueBICStage constraints already exists in BestInClass table')
		
END
ELSE
	PRINT('BestInClass table does not exists.')
	
GO

IF EXISTS (select 1 from INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'BestInClass') AND
	EXISTS (select 1 from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'BestInClass' AND COLUMN_NAME = 'StageId') AND
	EXISTS (select 1 from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'BestInClass' AND COLUMN_NAME = 'StageType')
BEGIN
		DECLARE @RecordCount INT = (SELECT COUNT(1) FROM BestInClass)

		IF (ISNULL(@RecordCount, 0)) = 0
		BEGIN
			INSERT INTO [dbo].[BestInClass] (StageId, StageType, Value, IsDeleted, CreatedDate, CreatedBy, ModifiedDate, ModifiedBy)
			SELECT S.StageId, M.MetricType, BIC.Value, BIC.IsDeleted, BIC.CreatedDate, BIC.CreatedBy, BIC.ModifiedDate, BIC.ModifiedBy 
			FROM BestInClass_Bckp BIC
			INNER JOIN Metric M ON M.MetricId = BIC.MetricId
			INNER JOIN Stage S ON S.Code = M.MetricCode AND S.ClientId = M.ClientId
		END
		ELSE
			PRINT('Data has already migrated. ' + CAST(@RecordCount as varchar(10)) + ' Records found.')
			

		IF EXISTS (select 1 from INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'BestInClass_Bckp')
		BEGIN
			DROP TABLE BestInClass_Bckp
		END	
END

IF @@ERROR != 0
BEGIN
		PRINT('Transaction RollBack')
		SELECT ErrorNumber = ERROR_NUMBER(), ErrorSeverity = ERROR_SEVERITY(), ErrorState = ERROR_STATE(), ErrorProcedure = ERROR_PROCEDURE(), ErrorLine = ERROR_LINE(), ErrorMessage = ERROR_MESSAGE(), AdditionalMessage = 'Error occured.' ;
		ROLLBACK TRANSACTION;
        RETURN;
END
ELSE
BEGIN
    PRINT('Transaction Commited')
	COMMIT TRANSACTION;
END

-- =========================================== End - Script- 2 (BestInClass drop MetricId and Add StageId and StageType) ==================================================================

GO

-- =========================================== Start - Script- 3 (ImprovementType_Metric table drop MetricId and Add StageId and StageType) ==================================================================

BEGIN TRANSACTION

-- ======================================== Truncate 'ImprovementTacticType_Metric' data, Drop 'MetricId' Column and add 'StageId' and 'StageType' field. Populate 'ImprovementTacticType_Metric' again. =========================

IF EXISTS (select 1 from INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'ImprovementTacticType_Metric') AND
	NOT EXISTS (select 1 from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'ImprovementTacticType_Metric' AND COLUMN_NAME = 'StageId')
BEGIN
	
	IF EXISTS (select 1 from INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'ImprovementTacticType_Metric_Bckp')
	BEGIN
		DROP TABLE ImprovementTacticType_Metric_Bckp
	END	

	select * into ImprovementTacticType_Metric_Bckp from  ImprovementTacticType_Metric
		
	IF NOT EXISTS (select 1 from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'ImprovementTacticType_Metric' AND COLUMN_NAME = 'StageId')
	BEGIN
		TRUNCATE TABLE ImprovementTacticType_Metric
	END
	ELSE
		PRINT('Data has already been migrated')
	
	IF EXISTS (select 1 from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME = 'FK_ImprovementTacticType_Metric_Metric' AND CONSTRAINT_SCHEMA = 'dbo')
	BEGIN
		ALTER TABLE ImprovementTacticType_Metric DROP CONSTRAINT FK_ImprovementTacticType_Metric_Metric
	END
	ELSE
		PRINT('FK_ImprovementTacticType_Metric_Metric constraints does not exists in ImprovementTacticType_Metric table')

	IF OBJECT_ID('PK_ImprovementTacticType_Metric', 'UQ') IS NOT NULL
	BEGIN
		ALTER TABLE ImprovementTacticType_Metric DROP CONSTRAINT PK_ImprovementTacticType_Metric
	END
	ELSE
		PRINT('PK_ImprovementTacticType_Metric constraints does not exists in ImprovementTacticType_Metric table')


	IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_NAME = 'PK_ImprovementTacticType_Metric' AND 
	TABLE_NAME = 'ImprovementTacticType_Metric' 
    AND TABLE_SCHEMA ='dbo' )
	BEGIN
		ALTER TABLE ImprovementTacticType_Metric DROP CONSTRAINT PK_ImprovementTacticType_Metric
	END
	ELSE
		PRINT('PK_ImprovementTacticType_Metric constraints does not exists in ImprovementTacticType_Metric table')


	IF EXISTS (select 1 from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'ImprovementTacticType_Metric' AND COLUMN_NAME = 'MetricId')
	BEGIN
		ALTER TABLE ImprovementTacticType_Metric DROP COLUMN MetricId
	END
	ELSE 
		PRINT('MetricId column does not exists in ImprovementTacticType_Metric table')

	IF NOT EXISTS (select 1 from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'ImprovementTacticType_Metric' AND COLUMN_NAME = 'StageId')
	BEGIN
		ALTER TABLE ImprovementTacticType_Metric ADD StageId INT NOT NULL
	END
	ELSE 
		PRINT('StageId column already exists in ImprovementTacticType_Metric table')

	IF NOT EXISTS (select 1 from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'ImprovementTacticType_Metric' AND COLUMN_NAME = 'StageType')
	BEGIN
		ALTER TABLE ImprovementTacticType_Metric ADD StageType NVARCHAR(10) NOT NULL
	END
	ELSE 
		PRINT('StageType column already exists in ImprovementTacticType_Metric table')

	IF NOT EXISTS (select 1 from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME = 'FK_ImprovementTacticType_Metric_Stage' AND CONSTRAINT_SCHEMA = 'dbo')
	BEGIN
		ALTER TABLE [dbo].[ImprovementTacticType_Metric]  WITH CHECK ADD  CONSTRAINT [FK_ImprovementTacticType_Metric_Stage] FOREIGN KEY([StageId])
		REFERENCES [dbo].[Stage] ([StageId])
	
		ALTER TABLE [dbo].[ImprovementTacticType_Metric] CHECK CONSTRAINT [FK_ImprovementTacticType_Metric_Stage]
	END
	ELSE 
		PRINT('FK_ImprovementTacticType_Metric_Stage constraints already exists in ImprovementTacticType_Metric table')

	IF OBJECT_ID('PK_ImprovementTacticType_Stage', 'UQ') IS NULL
	BEGIN
		ALTER TABLE ImprovementTacticType_Metric ADD CONSTRAINT PK_ImprovementTacticType_Stage UNIQUE (ImprovementTacticTypeId, StageId, StageType)
	END
	ELSE
		PRINT('PK_ImprovementTacticType_Stage constraints does not exists in ImprovementTacticType_Metric table')

	IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_NAME = 'PK_ImprovementTacticType_Metric' AND 
		TABLE_NAME = 'ImprovementTacticType_Metric' 
		AND TABLE_SCHEMA ='dbo' )
		BEGIN
			ALTER TABLE ImprovementTacticType_Metric
			ADD CONSTRAINT PK_ImprovementTacticType_Metric 
			PRIMARY KEY (ImprovementTacticTypeId, StageId, StageType) 
		END
		ELSE
			PRINT('PK_ImprovementTacticType_Metric constraints exists in ImprovementTacticType_Metric table')

END
ELSE
	PRINT('ImprovementTacticType_Metric table does not exists.')

GO

IF EXISTS (select 1 from INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'ImprovementTacticType_Metric') AND
	EXISTS (select 1 from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'ImprovementTacticType_Metric' AND COLUMN_NAME = 'StageId') AND
	EXISTS (select 1 from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'ImprovementTacticType_Metric' AND COLUMN_NAME = 'StageType')
BEGIN
	DECLARE @RecordCount INT = (SELECT COUNT(1) FROM ImprovementTacticType_Metric)

	IF (ISNULL(@RecordCount, 0)) = 0
	BEGIN
		INSERT INTO ImprovementTacticType_Metric(ImprovementTacticTypeId, StageId, StageType, [Weight], CreatedDate, CreatedBy) 
		SELECT ITM.ImprovementTacticTypeId,  S.StageId, M.MetricType, ITM.[Weight], ITM.CreatedDate, ITM.CreatedBy 
		FROM ImprovementTacticType_Metric_Bckp ITM
		INNER JOIN Metric M ON M.MetricId = ITM.MetricId 
		INNER JOIN Stage S ON S.Code = M.MetricCode AND S.ClientId = M.ClientId
	END
	ELSE
		PRINT('Data has already migrated. ' + CAST(@RecordCount as varchar(10)) + ' Records found.')

	
	IF EXISTS (select 1 from INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'ImprovementTacticType_Metric_Bckp')
	BEGIN
		DROP TABLE ImprovementTacticType_Metric_Bckp
	END

END

IF @@ERROR != 0
BEGIN
		PRINT('Transaction RollBack')
		SELECT ErrorNumber = ERROR_NUMBER(), ErrorSeverity = ERROR_SEVERITY(), ErrorState = ERROR_STATE(), ErrorProcedure = ERROR_PROCEDURE(), ErrorLine = ERROR_LINE(), ErrorMessage = ERROR_MESSAGE(), AdditionalMessage = 'Error occured.' ;
		ROLLBACK TRANSACTION;
        RETURN;
END
ELSE
BEGIN
    PRINT('Transaction Commited')
	COMMIT TRANSACTION;
END

-- =========================================== End - Script- 3 (ImprovementType_Metric table drop MetricId and Add StageId and StageType) ==================================================================

GO

-- =========================================== Start - Script- 4 (drop ImprovementType_Touches table) ===============================================================

IF EXISTS (select 1 from INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'ImprovementTacticType_Touches')
BEGIN
	
	DROP Table ImprovementTacticType_Touches

END
ELSE
	PRINT('ImprovementTacticType_Touches table does not exists')

-- =========================================== End - Script- 4 (drop ImprovementType_Touches table) ==================================================================

GO

-- =========================================== Start - Script- 5.1 (Add ProjectedStageValue column and populate its data in TacticType table) ===============================================================

IF EXISTS (select 1 from INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'TacticType')
BEGIN
	IF NOT EXISTS (select 1 from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'TacticType' AND COLUMN_NAME = 'ProjectedStageValue')
	BEGIN
		ALTER TABLE TacticType ADD ProjectedStageValue FLOAT NULL
	END
	ELSE 
		PRINT('ProjectedStageValue column already exists in TacticType table')
	
END
ELSE
	PRINT('TacticType does not exists.')

GO

IF EXISTS (select 1 from INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'TacticType')
BEGIN

	IF EXISTS (select 1 from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'TacticType' AND COLUMN_NAME = 'ProjectedInquiries')
		AND EXISTS (select 1 from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'TacticType' AND COLUMN_NAME = 'ProjectedStageValue')
	BEGIN
		EXEC('UPDATE TacticType SET ProjectedStageValue = ProjectedInquiries')
	END
	ELSE 
		PRINT('ProjectedInquiries column does not exists in TacticType table')

END
ELSE
	PRINT('TacticType does not exists.')

-- =========================================== End - Script- 5.1 (Add ProjectedStageValue column and populate its data in TacticType table) ===============================================================

GO

-- =========================================== Start - Script- 5.2 (Drop ProjectedInquiries column from TacticType table) ===============================================================

IF EXISTS (select 1 from INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'TacticType')
BEGIN

	IF EXISTS (select 1 from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'TacticType' AND COLUMN_NAME = 'ProjectedInquiries')
	BEGIN
		ALTER TABLE TacticType DROP COLUMN ProjectedInquiries
	END
	ELSE 
		PRINT('ProjectedInquiries column does not exists in TacticType table')

	IF EXISTS (select 1 from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'TacticType' AND COLUMN_NAME = 'ProjectedMQLs')
	BEGIN
		ALTER TABLE TacticType DROP COLUMN ProjectedMQLs
	END
	ELSE 
		PRINT('ProjectedMQLs column does not exists in TacticType table')
	
END
ELSE
	PRINT('TacticType does not exists.')

-- =========================================== End - Script- 5.2 (Drop ProjectedInquiries column from TacticType table) ===============================================================

GO

-- =========================================== Start - Script- 6.1 (Add ProjectedStageValue and populate data in Plan_Campaign_Program_Tactic table) =====================================================

IF EXISTS (select 1 from INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Plan_Campaign_Program_Tactic')
BEGIN
	IF NOT EXISTS (select 1 from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Plan_Campaign_Program_Tactic' AND COLUMN_NAME = 'ProjectedStageValue')
	BEGIN
		ALTER TABLE Plan_Campaign_Program_Tactic ADD ProjectedStageValue FLOAT NULL
	END
	ELSE 
		PRINT('ProjectedStageValue column already exists in Plan_Campaign_Program_Tactic table')
END
ELSE
	PRINT('Plan_Campaign_Program_Tactic table does not exists.')


GO
IF EXISTS (select 1 from INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Plan_Campaign_Program_Tactic')
BEGIN
	IF EXISTS (select 1 from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Plan_Campaign_Program_Tactic' AND COLUMN_NAME = 'INQs')
	BEGIN
		EXEC('UPDATE Plan_Campaign_Program_Tactic SET ProjectedStageValue = INQs')
	END
	ELSE 
		PRINT('INQs column does not exists in Plan_Campaign_Program_Tactic table')
END
ELSE
	PRINT('Plan_Campaign_Program_Tactic table does not exists.')

-- =========================================== End - Script- 6.1 (Add ProjectedStageValue and populate data in Plan_Campaign_Program_Tactic table) ==========================================================

GO

-- =========================================== Start - Script- 6.2 (Add StageId column and populate StageId data in Plan_Campaign_Program_Tactic table) =====================================================

IF EXISTS (select 1 from INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Plan_Campaign_Program_Tactic')
BEGIN
	IF NOT EXISTS (select 1 from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Plan_Campaign_Program_Tactic' AND COLUMN_NAME = 'StageId')
	BEGIN
		ALTER TABLE Plan_Campaign_Program_Tactic ADD StageId INT NULL
	END
	ELSE 
		PRINT('StageId column already exists in Plan_Campaign_Program_Tactic table')
END
ELSE
	PRINT('Plan_Campaign_Program_Tactic table does not exists.')
GO

IF EXISTS (select 1 from INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Plan_Campaign_Program_Tactic')
BEGIN
	IF EXISTS (select 1 from INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'TacticType')
	BEGIN
		IF EXISTS (select 1 from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'TacticType' AND COLUMN_NAME = 'StageId')
		BEGIN
			UPDATE Plan_Campaign_Program_Tactic SET StageId = T.StageId
			FROM Plan_Campaign_Program_Tactic P
			INNER JOIN TacticType T ON T.TacticTypeId = P.TacticTypeId

			ALTER TABLE Plan_Campaign_Program_Tactic ALTER COLUMN StageId INT NOT NULL
		END
		ELSE 
			PRINT('StageId column does exists in TacticType table')

	END
	ELSE
		PRINT('TacticType table does not exists.')
END
ELSE
	PRINT('Plan_Campaign_Program_Tactic table does not exists.')

	GO

	IF NOT EXISTS (select 1 from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME = 'FK_Plan_Campaign_Program_Tactic_Stage' AND CONSTRAINT_SCHEMA = 'dbo')
	BEGIN
		ALTER TABLE [dbo].[Plan_Campaign_Program_Tactic]  WITH CHECK ADD  CONSTRAINT [FK_Plan_Campaign_Program_Tactic_Stage] FOREIGN KEY([StageId])
		REFERENCES [dbo].[Stage] ([StageId])
	
		ALTER TABLE [dbo].[Plan_Campaign_Program_Tactic] CHECK CONSTRAINT [FK_Plan_Campaign_Program_Tactic_Stage]
	END
	ELSE 
		PRINT('[FK_Plan_Campaign_Program_Tactic_Stage] constraints already exists in [Plan_Campaign_Program_Tactic] table')

-- =========================================== End - Script- 6.2 ( Add StageId column and populate StageId data in Plan_Campaign_Program_Tactic table) ===================================================

GO

-- =========================================== Start - Script- 6.3 ( Drop columns from Plan_Campaign_Prgram_Tactic table) ===================================================

IF EXISTS (select 1 from INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Plan_Campaign_Program_Tactic')
BEGIN
	IF EXISTS (select 1 from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Plan_Campaign_Program_Tactic' AND COLUMN_NAME = 'INQs')
	BEGIN
		ALTER TABLE Plan_Campaign_Program_Tactic DROP COLUMN INQs
	END
	ELSE 
		PRINT('INQs column does not exists in Plan_Campaign_Program_Tactic table')

	IF EXISTS (select 1 from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Plan_Campaign_Program_Tactic' AND COLUMN_NAME = 'INQsActual')
	BEGIN
		ALTER TABLE Plan_Campaign_Program_Tactic DROP COLUMN INQsActual
	END
	ELSE 
		PRINT('INQsActual column does not exists in Plan_Campaign_Program_Tactic table')

	IF EXISTS (select 1 from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Plan_Campaign_Program_Tactic' AND COLUMN_NAME = 'MQLsActual')
	BEGIN
		ALTER TABLE Plan_Campaign_Program_Tactic DROP COLUMN MQLsActual
	END
	ELSE 
		PRINT('MQLsActual column does not exists in Plan_Campaign_Program_Tactic table')

	IF EXISTS (select 1 from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Plan_Campaign_Program_Tactic' AND COLUMN_NAME = 'CWs')
	BEGIN
		ALTER TABLE Plan_Campaign_Program_Tactic DROP COLUMN CWs
	END
	ELSE 
		PRINT('CWs column does not exists in Plan_Campaign_Program_Tactic table')

	IF EXISTS (select 1 from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Plan_Campaign_Program_Tactic' AND COLUMN_NAME = 'CWsActual')
	BEGIN
		ALTER TABLE Plan_Campaign_Program_Tactic DROP COLUMN CWsActual
	END
	ELSE 
		PRINT('CWsActual column does not exists in Plan_Campaign_Program_Tactic table')

	IF EXISTS (select 1 from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Plan_Campaign_Program_Tactic' AND COLUMN_NAME = 'Revenues')
	BEGIN
		ALTER TABLE Plan_Campaign_Program_Tactic DROP COLUMN Revenues
	END
	ELSE 
		PRINT('Revenues column does not exists in Plan_Campaign_Program_Tactic table')

	IF EXISTS (select 1 from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Plan_Campaign_Program_Tactic' AND COLUMN_NAME = 'RevenuesActual')
	BEGIN
		ALTER TABLE Plan_Campaign_Program_Tactic DROP COLUMN RevenuesActual
	END
	ELSE 
		PRINT('RevenuesActual column does not exists in Plan_Campaign_Program_Tactic table')

	IF EXISTS (select 1 from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Plan_Campaign_Program_Tactic' AND COLUMN_NAME = 'ROI')
	BEGIN
		ALTER TABLE Plan_Campaign_Program_Tactic DROP COLUMN ROI
	END
	ELSE 
		PRINT('ROI column does not exists in Plan_Campaign_Program_Tactic table')

	IF EXISTS (select 1 from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Plan_Campaign_Program_Tactic' AND COLUMN_NAME = 'ROIActual')
	BEGIN
		ALTER TABLE Plan_Campaign_Program_Tactic DROP COLUMN ROIActual
	END
	ELSE 
		PRINT('ROIActual column does not exists in Plan_Campaign_Program_Tactic table')
END
ELSE
	PRINT('Plan_Campaign_Program_Tactic table does not exists.')

-- =========================================== End - Script- 6.3 (Drop columns from Plan_Campaign_Prgram_Tactic table) ===================================================

GO

-- =========================================== Start - Script- 7 (Plan_Campaign_Program Drop columns) ====================================================================

IF EXISTS (select 1 from INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Plan_Campaign_Program')
BEGIN
	IF EXISTS (select 1 from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Plan_Campaign_Program' AND COLUMN_NAME = 'INQs')
	BEGIN
		ALTER TABLE Plan_Campaign_Program DROP COLUMN INQs
	END
	ELSE 
		PRINT('INQs column does not exists in Plan_Campaign_Program table')

	IF EXISTS (select 1 from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Plan_Campaign_Program' AND COLUMN_NAME = 'Cost')
	BEGIN
		ALTER TABLE Plan_Campaign_Program DROP COLUMN Cost
	END
	ELSE 
		PRINT('Cost column does not exists in Plan_Campaign_Program table')
END
ELSE
	PRINT('Plan_Campaign_Program does not exists.')

-- =========================================== End - Script- 7 (Plan_Campaign_Program Drop columns) ====================================================================

GO

-- =========================================== Start - Script- 8 (Plan_Campaign Drop columns) ==========================================================================

IF EXISTS (select 1 from INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Plan_Campaign')
BEGIN
	IF EXISTS (select 1 from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Plan_Campaign' AND COLUMN_NAME = 'INQs')
	BEGIN
		ALTER TABLE Plan_Campaign DROP COLUMN INQs
	END
	ELSE 
		PRINT('INQs column does not exists in Plan_Campaign_Program table')

	IF EXISTS (select 1 from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Plan_Campaign' AND COLUMN_NAME = 'Cost')
	BEGIN
		ALTER TABLE Plan_Campaign DROP COLUMN Cost
	END
	ELSE 
		PRINT('Cost column does not exists in Plan_Campaign_Program table')
END
ELSE
	PRINT('Plan_Campaign does not exists.')

-- =========================================== End - Script- 8 (Plan_Campaign Drop columns) ==========================================================================

GO

-- =========================================== Start - Script- 9 (Update_Plan_Tactic_Actual_Stage) ============================================================================

IF EXISTS (select 1 from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Plan_Campaign_Program_Tactic' AND COLUMN_NAME = 'StageId')
BEGIN

	IF NOT EXISTS(SELECT 1 FROM Plan_Campaign_Program_Tactic_Actual WHERE StageTitle = 'ProjectedStageValue')
	BEGIN
		DECLARE @intCount int
		SET @intCount = 1;

		CREATE TABLE #LocalTempTable(
		PlanTacticId int,
		StageTitle varchar(50), 
		Period varchar(5),
		Actualvalue float,
		rowId int)

		insert into #LocalTempTable select PlanTacticId,StageTitle,Period,Actualvalue ,ROW_NUMBER() over(order by plantacticid ) as rowId from Plan_Campaign_Program_Tactic_Actual

		--select * from #LocalTempTable

		WHILE @intCount < (select count(*) from #LocalTempTable)
		BEGIN
				BEGIN TRANSACTION 
				declare @period varchar(5) = (select Period from #LocalTempTable where rowId = @intCount)
				declare @stageTitle varchar(50) = (select StageTitle from #LocalTempTable where rowId = @intCount)
				declare @planTacticId int = (select PlanTacticId from #LocalTempTable where rowId = @intCount)
				declare @stageLevel int = (select Level from Stage  as s inner join Plan_Campaign_Program_Tactic as pt on s.StageId = pt.StageId
											where pt.PlanTacticId = @planTacticId)
				declare @MQLLevel int = (select Level from Stage  as s inner join TacticType as tt on tt.ClientId = s.ClientId 
									  inner join Plan_Campaign_Program_Tactic as pt on tt.TacticTypeId = pt.TacticTypeId
										where pt.PlanTacticId = @planTacticId  and s.Code = 'MQL')
        

				IF @stageTitle = 'CW' OR @stageTitle = 'Revenue' OR @stageTitle = 'ProjectedStageValue'
				begin
					print(@stageTitle)
				end
				ELSE
				begin
					if @stageLevel < @MQLLevel
					begin
						update Plan_Campaign_Program_Tactic_Actual set StageTitle = 'ProjectedStageValue' 
						where PlanTacticId = @planTacticId and Period = @period and StageTitle = 'INQ'
						--print('update')
					end

					if @stageLevel = @MQLLevel
					begin
				
						insert into Plan_Campaign_Program_Tactic_Actual
						select PlanTacticId,'ProjectedStageValue',Period,Actualvalue,CreatedDate,CreatedBy from Plan_Campaign_Program_Tactic_Actual 
						where PlanTacticId = @planTacticId and Period = @period and StageTitle = 'MQL' 
				
						delete from Plan_Campaign_Program_Tactic_Actual
						where PlanTacticId = @planTacticId and Period = @period and StageTitle = 'INQ'

				 
						--print('update and insert')


					end

					if @stageLevel > @MQLLevel
					begin
						update Plan_Campaign_Program_Tactic_Actual set StageTitle = 'ProjectedStageValue' 
						where PlanTacticId = @planTacticId and Period = @period and StageTitle = 'MQL'
								
						delete from Plan_Campaign_Program_Tactic_Actual
						where PlanTacticId = @planTacticId and Period = @period and StageTitle = 'INQ'
				
						--print('update and delete') 
					end

				end

				IF @@ERROR != 0 
					BEGIN 
						PRINT @@ERROR 
						PRINT 'ERROR'
						ROLLBACK TRANSACTION 

						RETURN  
					END   

				ELSE 
					BEGIN 
						COMMIT TRANSACTION 
						PRINT 'COMMITTED SUCCESSFULLY' 
					END 

				SET @intCount = @intCount + 1
		END

		drop table #LocalTempTable
	END
END

-- =========================================== End - Script- 9 (Update_Plan_Tactic_Actual_Stage) ============================================================================

GO

-- =========================================== Start - Script- 10 (Drop Metric Table Script) ============================================================================

IF EXISTS (select 1 from INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Metric')
BEGIN
	IF EXISTS (select 1 from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME = 'FK_BestInClass_Metric' AND CONSTRAINT_SCHEMA = 'dbo')
	BEGIN
		ALTER TABLE BestInClass DROP CONSTRAINT FK_BestInClass_Metric
	END
	ELSE
		PRINT('FK_BestInClass_Metric constraints does not exists in Metric table')

	IF EXISTS (select 1 from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME = 'FK_ImprovementTacticType_Metric_Metric' AND CONSTRAINT_SCHEMA = 'dbo')
	BEGIN
		ALTER TABLE ImprovementTacticType_Metric DROP CONSTRAINT FK_ImprovementTacticType_Metric_Metric
	END
	ELSE
		PRINT('FK_ImprovementTacticType_Metric_Metric constraints does not exists in Metric table')

	IF EXISTS (select 1 from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME = 'FK_ImprovementTacticType_Touches_Metric' AND CONSTRAINT_SCHEMA = 'dbo')
	BEGIN
		ALTER TABLE ImprovementTacticType_Touches DROP CONSTRAINT FK_ImprovementTacticType_Touches_Metric
	END
	ELSE
		PRINT('FK_ImprovementTacticType_Touches_Metric constraints does not exists in Metric table') 

	DROP Table Metric
END
ELSE
	PRINT('Metric table does not exists')

-- =========================================== End - Script- 10 (Drop Metric Table Script) ============================================================================

Go

-- =========================================== Start - Script- 11 (Update ConvertionTitle Value) ============================================================================

IF EXISTS (select 1 from INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Stage')
BEGIN
	
	IF EXISTS (select 1 from INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Stage' AND COLUMN_NAME = 'ConversionTitle')
	BEGIN
		Update S1 SET ConversionTitle = S1.Title + ' -> ' + S2.Title 
		FROM Stage S1
		INNER JOIN Stage S2 ON S2.Level = (S1.Level + 1) AND S1.ClientId = S2.ClientId AND ISNULL(S2.IsDeleted, 0) = 0
		WHERE ISNULL(S2.IsDeleted, 0) = 0 AND S1.ConversionTitle IS NULL AND S1.Code <> 'ADS'  
	END	
	ELSE
		PRINT('ConversionTitle column does not exists in Stage table.')

END
ELSE 
	PRINT('Stage table does not exists.')
-- =========================================== End - Script- 11 (Update ConvertionTitle Value) ============================================================================