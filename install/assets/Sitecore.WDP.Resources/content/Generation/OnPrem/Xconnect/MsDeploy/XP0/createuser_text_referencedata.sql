exec sp_configure 'contained database authentication', 1
go
reconfigure
go

alter database [PlaceHolderForDatabaseName] 
set containment = partial
go

CREATE USER [PlaceHolderForUser] WITH PASSWORD = 'PlaceHolderForPassword';
GO

EXEC sp_addrolemember 'db_datareader', [PlaceHolderForUser];
EXEC sp_addrolemember 'db_datawriter', [PlaceHolderForUser];
GO

GRANT EXECUTE TO [PlaceHolderForUser];
GO 