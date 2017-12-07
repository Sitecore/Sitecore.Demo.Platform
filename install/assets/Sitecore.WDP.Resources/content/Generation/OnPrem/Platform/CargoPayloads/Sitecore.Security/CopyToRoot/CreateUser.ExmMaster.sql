Use [PlaceholderForDB]
Go

alter database [PlaceholderForDB] 
set containment = partial
go

CREATE USER [PlaceHolderForUser] WITH PASSWORD = 'PlaceHolderForPassword';
GO

EXEC sp_addrolemember 'db_datareader', [PlaceHolderForUser];
EXEC sp_addrolemember 'db_datawriter', [PlaceHolderForUser];
GO

GRANT EXECUTE TO [PlaceHolderForUser];
GO 