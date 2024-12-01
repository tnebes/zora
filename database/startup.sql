USE [master]
GO

IF EXISTS (SELECT * FROM sys.server_principals WHERE name = 'zora_service')
BEGIN
    DROP LOGIN [zora_service]
END
GO

CREATE LOGIN [zora_service] WITH 
    PASSWORD = 'YourStrongPasswordHere',
    DEFAULT_DATABASE = [zora],
    CHECK_EXPIRATION = OFF,
    CHECK_POLICY = ON
GO

USE [zora]
GO

IF EXISTS (SELECT * FROM sys.database_principals WHERE name = 'zora_service')
BEGIN
    DROP USER [zora_service]
END
GO

CREATE USER [zora_service] FOR LOGIN [zora_service]
GO

EXEC sp_addrolemember 'db_owner', 'zora_service'
GO

GRANT CONNECT TO [zora_service]
GO