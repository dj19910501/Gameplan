GO
SET IDENTITY_INSERT [dbo].[IntegrationType] ON 

GO
INSERT [dbo].[IntegrationType] ([IntegrationTypeId], [Title], [Description], [IsDeleted], [APIVersion], [APIURL]) VALUES (1, N'Eloqua', NULL, 0, N'1.0', N'www.login.eloqua.com')
GO
INSERT [dbo].[IntegrationType] ([IntegrationTypeId], [Title], [Description], [IsDeleted], [APIVersion], [APIURL]) VALUES (2, N'Salesforce', NULL, 0, NULL, N'https://test.salesforce.com/services/oauth2/token')
GO
INSERT [dbo].[IntegrationType] ([IntegrationTypeId], [Title], [Description], [IsDeleted], [APIVersion], [APIURL]) VALUES (3, N'marketo', NULL, 0, NULL, NULL)
GO
SET IDENTITY_INSERT [dbo].[IntegrationType] OFF
GO
SET IDENTITY_INSERT [dbo].[IntegrationTypeAttribute] ON 


GO
INSERT [dbo].[IntegrationTypeAttribute] ([IntegrationTypeAttributeId], [IntegrationTypeId], [Attribute], [AttributeType], [IsDeleted]) VALUES (1, 2, N'ConsumerKey', N'textbox', 0)
GO
INSERT [dbo].[IntegrationTypeAttribute] ([IntegrationTypeAttributeId], [IntegrationTypeId], [Attribute], [AttributeType], [IsDeleted]) VALUES (2, 2, N'ConsumerSecret', N'textbox', 0)
GO
INSERT [dbo].[IntegrationTypeAttribute] ([IntegrationTypeAttributeId], [IntegrationTypeId], [Attribute], [AttributeType], [IsDeleted]) VALUES (3, 2, N'SecurityToken', N'textbox', 0)
GO
SET IDENTITY_INSERT [dbo].[IntegrationTypeAttribute] OFF
GO
