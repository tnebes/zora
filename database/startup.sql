USE [master]
GO

IF EXISTS (SELECT name FROM sys.server_principals WHERE name = 'zora_service')
BEGIN
    DROP LOGIN [zora_service]
END
GO

CREATE LOGIN [zora_service] WITH 
    PASSWORD = 'your_strong_password_here',
    DEFAULT_DATABASE = [zora],
    CHECK_EXPIRATION = OFF,
    CHECK_POLICY = ON
GO

USE [zora]
GO

IF EXISTS (SELECT name FROM sys.database_principals WHERE name = 'zora_service')
BEGIN
    DROP USER [zora_service]
END
GO

CREATE USER [zora_service] FOR LOGIN [zora_service]
GO

ALTER ROLE db_datareader ADD MEMBER [zora_service]
GO
ALTER ROLE db_datawriter ADD MEMBER [zora_service]
GO
ALTER ROLE db_ddladmin ADD MEMBER [zora_service]
GO

GRANT EXECUTE TO [zora_service]
GO