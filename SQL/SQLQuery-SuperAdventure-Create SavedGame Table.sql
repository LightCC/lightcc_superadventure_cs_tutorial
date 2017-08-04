USE [SuperAdventure]
GO
/****** Object: Table [dbo].[SavedGame]   
    Script Date: 2/2/2016 6:21:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SavedGame](
       [CurrentHitPoints] [int] NOT NULL,
       [MaximumHitPoints] [int] NOT NULL,
       [Gold] [int] NOT NULL,
       [ExperiencePoints] [int] NOT NULL,
       [CurrentLocationID] [int] NOT NULL
) ON [PRIMARY]
GO