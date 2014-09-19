----01_PL_571_Input actual costs - Tactics.sql
--===================[Plan_Campaign_Program_Tactic_LineItem_Actual]==================
BEGIN
		IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Plan_Campaign_Program_Tactic_LineItem_Actual')

		BEGIN
					CREATE TABLE [dbo].[Plan_Campaign_Program_Tactic_LineItem_Actual](
						[PlanLineItemId] [int] NOT NULL,
						[Period] [nvarchar](5) NOT NULL,
						[Value] [float] NOT NULL,
						[CreatedDate] [datetime] NOT NULL,
						[CreatedBy] [uniqueidentifier] NOT NULL,
					 CONSTRAINT [PK_Plan_Campaign_Program_Tactic_LineItem_Actual] PRIMARY KEY CLUSTERED 
					(
						[PlanLineItemId] ASC,
						[Period] ASC
					)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
					 CONSTRAINT [IX_Plan_Campaign_Program_Tactic_LineItem_Actual] UNIQUE NONCLUSTERED 
					(
						[PlanLineItemId] ASC,
						[Period] ASC
					)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
					) ON [PRIMARY]


					IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Plan_Campaign_Program_Tactic_LineItem')
					BEGIN

						ALTER TABLE [dbo].[Plan_Campaign_Program_Tactic_LineItem_Actual]  WITH CHECK ADD  CONSTRAINT [FK_Plan_Campaign_Program_Tactic_LineItem_Actual_Plan_Campaign_Program_Tactic_LineItem] FOREIGN KEY([PlanLineItemId])
						REFERENCES [dbo].[Plan_Campaign_Program_Tactic_LineItem] ([PlanLineItemId])
						ALTER TABLE [dbo].[Plan_Campaign_Program_Tactic_LineItem_Actual] CHECK CONSTRAINT [FK_Plan_Campaign_Program_Tactic_LineItem_Actual_Plan_Campaign_Program_Tactic_LineItem]

					END


					EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK - Refers to associated PlanLineItemId and composite PK with Period to uniquely identify line item and period combination.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Tactic_LineItem_Actual', @level2type=N'COLUMN',@level2name=N'PlanLineItemId'


					EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Composite PK with LineItemTypeId to uniquely identify line item and period combination.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Tactic_LineItem_Actual', @level2type=N'COLUMN',@level2name=N'Period'


					EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Value for a period.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Tactic_LineItem_Actual', @level2type=N'COLUMN',@level2name=N'Value'


					EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date on which the record was created.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Tactic_LineItem_Actual', @level2type=N'COLUMN',@level2name=N'CreatedDate'


					EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Refers to associated UserId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plan_Campaign_Program_Tactic_LineItem_Actual', @level2type=N'COLUMN',@level2name=N'CreatedBy'

		END

--=================== End [Plan_Campaign_Program_Tactic_LineItem_Actual]==================


END
GO

----01_PL_773_Integration_Pushing_actual_cost_values_to_Salesforce_Eloqua.sql
BEGIN
	-- ======================================================================================
	-- Created By : Sohel Pathan
	-- Created Date : 11/09/2014
	-- Description : Set IsGet field to '0' for ActualCost DataType in GameplanDataType table
	-- ======================================================================================

	IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'GameplanDataType' AND TABLE_SCHEMA = 'dbo')
	BEGIN
		IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'IsGet' AND TABLE_NAME = 'GameplanDataType' AND TABLE_SCHEMA = 'dbo')
		BEGIN
			IF (SELECT COUNT(1) FROM GameplanDataType WHERE ActualFieldName = 'CostActual' AND ISNULL(IsDeleted, 0) = 0 AND ISNULL(IsGet, 0) = 1) > 0
			BEGIN
				UPDATE GameplanDataType 
				SET IsGet = 0 
				WHERE ActualFieldName = 'CostActual' AND ISNULL(IsDeleted, 0) = 0 AND ISNULL(IsGet, 0) = 1
			END
		END
	END

END
GO

----01_608_Create data model for advanced budgeting.sql
BEGIN
			--Create By : Kalpesh Sharma 09/11/2014
			--PL # #608 Create data model for advanced budgeting 


			IF OBJECT_ID(N'TempDB.dbo.#TempCostActual', N'U') IS NOT NULL
			BEGIN
			  DROP TABLE #TempCostActual
			END

			DECLARE @i INT
				,@TacticID INT
				,@MonthsCount INT
				,@CostActual VARCHAR(100)
				,@Values NVARCHAR(100)
				,@ReminderValues NVARCHAR(20)
				,@PeriodMonth DATE
				,@intErrorCode INT
				,@CreatedBy uniqueidentifier
				,@CurrentDate DATETIME

			SET @CreatedBy = 'E5EF88EB-4748-4436-9ACC-ABA6B2C5F6A9' 
			SET @CurrentDate = CONVERT(DATETIME, Convert(VARCHAR(10), GETDATE(), 111))

			CREATE TABLE #TempCostActual (
				TacticID INT
				,MonthsCount INT
				,CostActual DECIMAL
				,PeriodMonth DATE
				)

			INSERT INTO #TempCostActual (
				TacticID
				,MonthsCount
				,CostActual
				,PeriodMonth
				)
			SELECT PlanTacticId
				,(
					SELECT DATEDIFF(MONTH, StartDate, EndDate)
					) + 1 AS MonthsCount
				,CostActual
				,
				StartDate AS PeriodMonth
			FROM Plan_Campaign_Program_Tactic
			WHERE CostActual <> 0

			BEGIN TRANSACTION TranCostActual;
			 BEGIN TRY

			DECLARE myCursor CURSOR LOCAL FAST_FORWARD
			FOR
			SELECT *
			FROM #TempCostActual

			OPEN myCursor

			FETCH NEXT
			FROM myCursor
			INTO @TacticID
				,@MonthsCount
				,@CostActual
				,@PeriodMonth

			WHILE @@FETCH_STATUS = 0
			BEGIN
				SET @Values = FLOOR(Cast(@CostActual AS DECIMAL) / CAST(@MonthsCount AS INT))
				SET @ReminderValues = (Cast(@CostActual AS DECIMAL) - (Cast(@Values AS DECIMAL) * CAST(@MonthsCount AS INT)))

				DECLARE @RemainingMonthCount INT
				DECLARE @MONTHPERIOD NVARCHAR(20)

				SET @RemainingMonthCount = 0
		
				SET @i = 1

				WHILE (@i <= @MonthsCount)
				BEGIN
					IF (@i = @MonthsCount)
					BEGIN
						SET @Values = (Cast(@Values AS DECIMAL) + cast(@ReminderValues AS DECIMAL))
					END
					SET @MONTHPERIOD = Convert(nvarchar,CONCAT('Y', DATEPART(mm,DATEADD(mm,@RemainingMonthCount,@PeriodMonth))))
					INSERT INTO [dbo].[Plan_Campaign_Program_Tactic_Actual]
							 ([PlanTacticId]
							 ,[StageTitle]
							 ,[Period]
							 ,[Actualvalue]
							 ,[CreatedDate]
							 ,[CreatedBy]
							 ,[ModifiedDate]
							 ,[ModifiedBy])
					   VALUES(@TacticID,'Cost',@MONTHPERIOD,CONVERT(float,@Values),@CurrentDate,@CreatedBy,null,null)

					--PRINT (
					--		Convert(NVARCHAR, @TacticID) + ' Cost ' + @MONTHPERIOD + ' Value ' + @Values  + 'null' + 'null'
					--		)

					IF EXISTS(
								SELECT tactic.PlanLineItemId
								FROM Plan_Campaign_Program_Tactic_LineItem tactic
								WHERE tactic.PlanTacticId = @TacticID
									AND tactic.LineItemTypeId IS NULL
							)
					BEGIN
						DECLARE @LineItemID INT

						SET @LineItemID = (
								SELECT tactic.PlanLineItemId
								FROM Plan_Campaign_Program_Tactic_LineItem tactic
								WHERE tactic.PlanTacticId = @TacticID
									AND tactic.LineItemTypeId IS NULL
								)
						
						INSERT INTO [dbo].[Plan_Campaign_Program_Tactic_LineItem_Actual]
								 ([PlanLineItemId]
								 ,[Period]
								 ,[Value]
								 ,[CreatedDate]
								 ,[CreatedBy])
						   VALUES(@LineItemID,@MONTHPERIOD,CONVERT(float,@Values),@CurrentDate,@CreatedBy)
			
						--PRINT (
						--		Convert(NVARCHAR, @LineItemID) + CONCAT (
						--			' Y'
						--			,DATEPART(mm, DATEADD(mm, @RemainingMonthCount, @PeriodMonth))
						--			) + ' Value ' + @Values + ' Created Date ' + Convert(VARCHAR(50), CONVERT(DATETIME, Convert(VARCHAR(10), GETDATE(), 111))) 
						--		)

					END
			
					update Plan_Campaign_Program_Tactic 
					set  CostActual = 0
					Where PlanTacticId = @TacticID
			

					SET @RemainingMonthCount = @RemainingMonthCount + 1
					SET @i = @i + 1
				END
				--print(' CostActual ' + Convert(varchar,@CostActual) + ' Months ' + Convert(varchar,@MonthsCount) +' Value ' + Convert(varchar,@Values) + ' Reminders ' +  Convert(varchar,@ReminderValues) )
				FETCH NEXT
				FROM myCursor
				INTO @TacticID
					,@MonthsCount
					,@CostActual
					,@PeriodMonth
			END

			print('Completed')

			CLOSE myCursor -- close the cursor

			DEALLOCATE myCursor -- Deallocate the cursor

			  ---Successfully deleted
			COMMIT TRANSACTION TranCostActual;
			 END TRY
				BEGIN CATCH
				  ---Unsuccess
					ROLLBACK TRANSACTION TranCostActual;
				END CATCH 
END
GO

----01_PL_718_Custom_fields_for_Campaigns.sql
BEGIN
			--==================CustomFieldType=====================

			IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='CustomFieldType')
			BEGIN
				CREATE TABLE [dbo].[CustomFieldType](
					   [CustomFieldTypeId] [int] IDENTITY(1,1) NOT NULL,
					   [Name] [nvarchar](50) NOT NULL,
					   [Description] [nvarchar](4000) NULL,
				CONSTRAINT [PK_CustomFieldType] PRIMARY KEY CLUSTERED 
				(
					   [CustomFieldTypeId] ASC
				)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
				) ON [PRIMARY]



				EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'An auto increment primary key to uniquely identify custom field type.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomFieldType', @level2type=N'COLUMN',@level2name=N'CustomFieldTypeId'
				EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Name of custom field type.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomFieldType', @level2type=N'COLUMN',@level2name=N'Name'
				EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Description for custom field type.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomFieldType', @level2type=N'COLUMN',@level2name=N'Description'

				--Insert script for CustomFieldType
				INSERT INTO [dbo].[CustomFieldType]([Name],[Description])VALUES('TextBox','');
				INSERT INTO [dbo].[CustomFieldType]([Name],[Description])VALUES('DropDownList','');
			END

			--==================End CustomFieldType=====================

			--==================CustomField=====================

			IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='CustomField')
			BEGIN
				CREATE TABLE [dbo].[CustomField](
					   [CustomFieldId] [int] IDENTITY(1,1) NOT NULL,
					   [Name] [nvarchar](255) NOT NULL,
					   [CustomFieldTypeId] [int] NOT NULL,
					   [Description] [nvarchar](4000) NULL,
					   [IsRequired] [bit] NOT NULL,
					   [EntityType] [nvarchar](50) NOT NULL,
					   [ClientId] [uniqueidentifier] NOT NULL,
					   [IsDeleted] [bit] NOT NULL,
					   [CreatedDate] [datetime] NOT NULL,
					   [CreatedBy] [uniqueidentifier] NOT NULL,
					   [ModifiedDate] [datetime] NULL,
					   [ModifiedBy] [uniqueidentifier] NULL,
				CONSTRAINT [PK_CustomField] PRIMARY KEY CLUSTERED 
				(
					   [CustomFieldId] ASC
				)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
				) ON [PRIMARY]



				ALTER TABLE [dbo].[CustomField] ADD  CONSTRAINT [DF_CustomField_IsRequired]  DEFAULT ((0)) FOR [IsRequired]


				ALTER TABLE [dbo].[CustomField] ADD  CONSTRAINT [DF_CustomField_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]


				ALTER TABLE [dbo].[CustomField]  WITH CHECK ADD  CONSTRAINT [FK_CustomField_CustomFieldType] FOREIGN KEY([CustomFieldTypeId])
				REFERENCES [dbo].[CustomFieldType] ([CustomFieldTypeId])

				ALTER TABLE [dbo].[CustomField] CHECK CONSTRAINT [FK_CustomField_CustomFieldType]


				EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'An auto increment primary key to uniquely identify custom field.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField', @level2type=N'COLUMN',@level2name=N'CustomFieldId'


				EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Name of custom field.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField', @level2type=N'COLUMN',@level2name=N'Name'


				EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK - Refers to associated CustomFieldTypeId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField', @level2type=N'COLUMN',@level2name=N'CustomFieldTypeId'


				EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Description for custom field.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField', @level2type=N'COLUMN',@level2name=N'Description'


				EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Flag to identify custom field is required or not.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField', @level2type=N'COLUMN',@level2name=N'IsRequired'


				EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Type of entity. EntityType can be Plan,Campaign,Program,Tactic,LineItem.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField', @level2type=N'COLUMN',@level2name=N'EntityType'


				EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK - Refers to associated ClientId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField', @level2type=N'COLUMN',@level2name=N'ClientId'


				EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Flag to identify record is deleted or not.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField', @level2type=N'COLUMN',@level2name=N'IsDeleted'


				EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date on which the record was created.
				' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField', @level2type=N'COLUMN',@level2name=N'CreatedDate'


				EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Refers to associated UserId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField', @level2type=N'COLUMN',@level2name=N'CreatedBy'


				EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date on which the record was last modified.
				' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField', @level2type=N'COLUMN',@level2name=N'ModifiedDate'

				EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Refers to associated UserId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField', @level2type=N'COLUMN',@level2name=N'ModifiedBy'

			END

			--==================End CustomField=====================

			--==================CustomFieldOption=====================

			IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='CustomFieldOption')
			BEGIN

					CREATE TABLE [dbo].[CustomFieldOption](
						   [CustomFieldOptionId] [int] IDENTITY(1,1) NOT NULL,
						   [CustomFieldId] [int] NOT NULL,
						   [Value] [nvarchar](255) NOT NULL,
						   [CreatedDate] [datetime] NOT NULL,
						   [CreatedBy] [uniqueidentifier] NOT NULL,
					CONSTRAINT [PK_CustomFieldOption] PRIMARY KEY CLUSTERED 
					(
						   [CustomFieldOptionId] ASC
					)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
					) ON [PRIMARY]


					ALTER TABLE [dbo].[CustomFieldOption]  WITH CHECK ADD  CONSTRAINT [FK_CustomFieldOption_CustomField] FOREIGN KEY([CustomFieldId])
					REFERENCES [dbo].[CustomField] ([CustomFieldId])

					ALTER TABLE [dbo].[CustomFieldOption] CHECK CONSTRAINT [FK_CustomFieldOption_CustomField]

					EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'An auto increment primary key to uniquely identify custom field option.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomFieldOption', @level2type=N'COLUMN',@level2name=N'CustomFieldOptionId'

					EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK - Refers to associated CustomFieldId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomFieldOption', @level2type=N'COLUMN',@level2name=N'CustomFieldId'

					EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Option of custom field.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomFieldOption', @level2type=N'COLUMN',@level2name=N'Value'

					EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date on which the record was created.
					' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomFieldOption', @level2type=N'COLUMN',@level2name=N'CreatedDate'

					EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Refers to associated UserId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomFieldOption', @level2type=N'COLUMN',@level2name=N'CreatedBy'




			END

			--==================End CustomFieldOption=====================

			--==================CustomField_Entity=====================

			IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='CustomField_Entity')
			BEGIN

					CREATE TABLE [dbo].[CustomField_Entity](
						   [CustomFieldEntityId] [int] IDENTITY(1,1) NOT NULL,
						   [EntityId] [int] NOT NULL,
						   [CustomFieldId] [int] NOT NULL,
						   [Value] [nvarchar](255) NOT NULL,
						   [CreatedDate] [datetime] NOT NULL,
						   [CreatedBy] [uniqueidentifier] NOT NULL,
					CONSTRAINT [PK_CustomField_Entity] PRIMARY KEY CLUSTERED 
					(
						   [CustomFieldEntityId] ASC
					)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
					) ON [PRIMARY]


							ALTER TABLE [dbo].[CustomField_Entity]  WITH CHECK ADD  CONSTRAINT [FK_CustomField_Entity_CustomField] FOREIGN KEY([CustomFieldId])
			REFERENCES [dbo].[CustomField] ([CustomFieldId])

					ALTER TABLE [dbo].[CustomField_Entity] CHECK CONSTRAINT [FK_CustomField_Entity_CustomField]

					EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'An auto increment primary key to uniquely identify value of custom field and entity.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField_Entity', @level2type=N'COLUMN',@level2name=N'CustomFieldEntityId'

					EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Refers to associated Plan, Campaign, Program, Tactic and LineItem id.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField_Entity', @level2type=N'COLUMN',@level2name=N'EntityId'

					EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK - Refers to associated CustomFieldId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField_Entity', @level2type=N'COLUMN',@level2name=N'CustomFieldId'

					EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Value for custom field defined for an entity.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField_Entity', @level2type=N'COLUMN',@level2name=N'Value'

							EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date on which the record was created.
			' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField_Entity', @level2type=N'COLUMN',@level2name=N'CreatedDate'

					EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Refers to associated UserId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'CustomField_Entity', @level2type=N'COLUMN',@level2name=N'CreatedBy'


			END

			--==================End CustomField_Entity=====================
END
GO