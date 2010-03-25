CREATE USER [[DBUSER]] FOR LOGIN [[DBUSER]] WITH DEFAULT_SCHEMA=[dbo]
GO
ALTER AUTHORIZATION ON SCHEMA::[db_owner] TO [[DBUSER]]
GO
ALTER AUTHORIZATION ON SCHEMA::[db_datareader] TO [[DBUSER]]
GO
ALTER AUTHORIZATION ON SCHEMA::[db_datawriter] TO [[DBUSER]]
GO
EXEC sp_addrolemember N'db_datareader', N'[DBUSER]'
GO
EXEC sp_addrolemember N'db_owner', N'[DBUSER]'
GO
EXEC sp_addrolemember N'db_datawriter', N'[DBUSER]'
GO
