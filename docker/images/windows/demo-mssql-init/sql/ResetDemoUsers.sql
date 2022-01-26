USE [Sitecore.Core]

DECLARE @UserId uniqueidentifier, @UserName nvarchar(256)

DECLARE uc CURSOR FOR
SELECT UserId, UserName
FROM [aspnet_Users]
WHERE username in ('sitecore\minnie', 'sitecore\pete', 'sitecore\demoadmin', 'sitecore\Content Author', 'sitecore\Marketer')

OPEN uc

FETCH NEXT FROM uc
INTO @UserId, @UserName

WHILE @@FETCH_STATUS = 0
BEGIN

  UPDATE
    [aspnet_Membership]
  SET
    [Password]= $(EncodedPassword),
    [PasswordSalt]= $(EncodedSalt),
    [PasswordFormat] = 1
  WHERE
    UserId = @UserId

  FETCH NEXT FROM uc
  INTO @UserId, @UserName

END
CLOSE uc;
DEALLOCATE uc;
