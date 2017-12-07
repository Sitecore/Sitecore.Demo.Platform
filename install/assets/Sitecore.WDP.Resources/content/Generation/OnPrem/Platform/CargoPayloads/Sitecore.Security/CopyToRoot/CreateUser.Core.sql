Use [PlaceholderForDB]
Go

alter database [PlaceholderForDB] 
set containment = partial
go

CREATE USER [PlaceHolderForUser] WITH PASSWORD = 'PlaceHolderForPassword';
GO

EXEC sp_addrolemember 'db_datareader', [PlaceHolderForUser];
EXEC sp_addrolemember 'db_datawriter', [PlaceHolderForUser];
EXEC sp_addrolemember 'aspnet_Membership_BasicAccess', [PlaceHolderForUser];
EXEC sp_addrolemember 'aspnet_Membership_FullAccess', [PlaceHolderForUser];
EXEC sp_addrolemember 'aspnet_Membership_ReportingAccess', [PlaceHolderForUser];
EXEC sp_addrolemember 'aspnet_Profile_BasicAccess', [PlaceHolderForUser];
EXEC sp_addrolemember 'aspnet_Profile_FullAccess', [PlaceHolderForUser];
EXEC sp_addrolemember 'aspnet_Profile_ReportingAccess', [PlaceHolderForUser];
EXEC sp_addrolemember 'aspnet_Roles_BasicAccess', [PlaceHolderForUser];
EXEC sp_addrolemember 'aspnet_Roles_FullAccess', [PlaceHolderForUser];
EXEC sp_addrolemember 'aspnet_Roles_ReportingAccess', [PlaceHolderForUser];
GO

GRANT EXECUTE TO [PlaceHolderForUser];
GO 