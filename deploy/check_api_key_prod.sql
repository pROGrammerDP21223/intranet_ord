SET NOCOUNT ON;

SELECT
    Id,
    ClientId,
    Name,
    IsActive,
    ExpiresAt,
    IsDeleted
FROM ApiKeys
WHERE [Key] = 'sk_6kFaKcrA5N4eIcXPX09tiP6H7X3uiJK0KOzowNv0wP5CuRaFI68Y7gDahByOH1ZG';
