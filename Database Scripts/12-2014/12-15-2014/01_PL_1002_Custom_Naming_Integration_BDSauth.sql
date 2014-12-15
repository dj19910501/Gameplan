-- ======================================================================================
-- Created By : Mitesh Vaishnav
-- Created Date : 12/12/2014
-- Description : Custom Naming: Integration
-- ======================================================================================

IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Client_Activity')
BEGIN

CREATE TABLE [dbo].[Client_Activity](
	[ClientActivityId] [int] IDENTITY(1,1) NOT NULL,
	[ClientId] [uniqueidentifier] NOT NULL,
	[ApplicationActivityId] [int] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
 CONSTRAINT [PK_Client_Activity] PRIMARY KEY CLUSTERED 
(
	[ClientActivityId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


     IF EXISTS(SELECT * FROM sys.columns WHERE [name] = N'ApplicationActivityId' AND [object_id] = OBJECT_ID(N'Application_Activity'))
        BEGIN
             ALTER TABLE [dbo].[Client_Activity]  WITH CHECK ADD  CONSTRAINT [FK_Client_Activity_Application_Activity] FOREIGN KEY([ApplicationActivityId])
             REFERENCES [dbo].[Application_Activity] ([ApplicationActivityId])

             ALTER TABLE [dbo].[Client_Activity] CHECK CONSTRAINT [FK_Client_Activity_Application_Activity]
        END
      IF EXISTS(SELECT * FROM sys.columns WHERE [name] = N'ApplicationActivityId' AND [object_id] = OBJECT_ID(N'Application_Activity'))
        BEGIN
		     ALTER TABLE [dbo].[Client_Activity]  WITH CHECK ADD  CONSTRAINT [FK_Client_Activity_Client] FOREIGN KEY([ClientId])
             REFERENCES [dbo].[Client] ([ClientId])

             ALTER TABLE [dbo].[Client_Activity] CHECK CONSTRAINT [FK_Client_Activity_Client]
		END

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'An auto increment primary key.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Client_Activity', @level2type=N'COLUMN',@level2name=N'ClientActivityId'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK - Refers to associated ClientId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Client_Activity', @level2type=N'COLUMN',@level2name=N'ClientId'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FK - Refers associated application activity.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Client_Activity', @level2type=N'COLUMN',@level2name=N'ApplicationActivityId'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Refers to associated UserId.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Client_Activity', @level2type=N'COLUMN',@level2name=N'CreatedBy'

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date on which the record was created.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Client_Activity', @level2type=N'COLUMN',@level2name=N'CreatedDate'

END

IF NOT EXISTS(SELECT * FROM sys.columns WHERE [name] = N'ActivityType' AND [object_id] = OBJECT_ID(N'Application_Activity'))
	    BEGIN
		    ALTER TABLE [Application_Activity] ADD ActivityType NVARCHAR(255) NULL
	    END

IF EXISTS(SELECT * FROM sys.columns WHERE [name] = N'ActivityType' AND [object_id] = OBJECT_ID(N'Application_Activity'))
	    BEGIN
		UPDATE dbo.Application_Activity SET ActivityType='User'
		END

