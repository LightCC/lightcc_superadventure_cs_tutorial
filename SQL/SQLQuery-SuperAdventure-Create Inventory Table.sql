USE [SuperAdventure]
GO
/****** Object: Table [dbo].[Inventory]   
    Script Date: 2/2/2016 6:20:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Inventory](
       [InventoryItemID] [int] NOT NULL,
       [Quantity] [int] NOT NULL
) ON [PRIMARY]
GO