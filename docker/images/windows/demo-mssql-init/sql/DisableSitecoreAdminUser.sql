USE [Sitecore.Core]

DECLARE @UserId uniqueidentifier
SELECT TOP 1
  @UserId = [UserId]
FROM [aspnet_Users]
WHERE [UserName] = 'sitecore\Admin';

UPDATE
    [aspnet_Membership]
SET
    [IsApproved] = '0',
    [IsLockedOut] = '1'
WHERE
    UserId = @UserId