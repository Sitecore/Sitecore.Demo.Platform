USE [Sitecore.Core]

DECLARE @UserId uniqueidentifier
SELECT TOP 1
  @UserId = [UserId]
FROM [aspnet_Users]
WHERE [UserName] = 'sitecore\Admin';

 UPDATE
    [aspnet_Membership]
SET
    [Password]= $(EncodedPassword),
    [PasswordSalt]= $(EncodedSalt),
    [PasswordFormat] = 1
WHERE
    UserId = @UserId
