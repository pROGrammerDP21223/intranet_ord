SELECT 
    COL_LENGTH('dbo.Clients','IsPremium') AS ClientsIsPremium,
    COL_LENGTH('dbo.Categories','IsHome') AS CategoriesIsHome,
    COL_LENGTH('dbo.Categories','IsTop') AS CategoriesIsTop;
GO
