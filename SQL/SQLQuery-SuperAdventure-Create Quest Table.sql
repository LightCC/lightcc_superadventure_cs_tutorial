USE [SuperAdventure]
GO
/****** Object: Table [dbo].[Quest]   
    Script Date: 2/2/2016 6:21:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Quest](
       [QuestID] [int] NOT NULL,
       [IsCompleted] [bit] NOT NULL
) ON [PRIMARY]
GO