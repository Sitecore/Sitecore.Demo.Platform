declare @ApplicationName nvarchar(256) = 'sitecore'
declare @UserName nvarchar(256) = 'sitecore\admin'
declare @Password nvarchar(128) = 'PlaceHolderForPassword'
declare @HashAlgorithm nvarchar(10) = 'SHA1'
declare @PasswordFormat int = 1 -- Hashed
declare @CurrentTimeUtc datetime = SYSUTCDATETIME()
declare @Salt varbinary(16) = 0x
declare @HashedPassword varbinary(20)
declare @EncodedHash nvarchar(128)
declare @EncodedSalt nvarchar(128)

-- Generate random salt
while len(@Salt) < 16
begin
	set @Salt = (@Salt + cast(cast(floor(rand() * 256) as tinyint) as binary(1)))
end

-- Hash password
set @HashedPassword = HASHBYTES(@HashAlgorithm, @Salt + cast(@Password as varbinary(128)));

-- Convert hash and salt to BASE64
select @EncodedHash = cast(N'' as xml).value(
                  'xs:base64Binary(xs:hexBinary(sql:column("bin")))'
                , 'varchar(max)'
            ) from (select @HashedPassword as [bin] ) T

select @EncodedSalt = cast(N'' as xml).value(
                  'xs:base64Binary(xs:hexBinary(sql:column("bin")))'
                , 'VARCHAR(MAX)'
            ) from (select @Salt as [bin] ) T 

execute [dbo].[aspnet_Membership_SetPassword] 
   @ApplicationName
  ,@UserName
  ,@EncodedHash
  ,@EncodedSalt
  ,@CurrentTimeUtc
  ,@PasswordFormat



