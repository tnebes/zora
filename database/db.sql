USE master;

IF EXISTS (SELECT name FROM sys.databases WHERE name = 'zora')
BEGIN
    ALTER DATABASE zora SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE zora;
END

CREATE DATABASE zora;
GO

ALTER DATABASE zora COLLATE Latin1_General_100_CI_AS_SC_UTF8;

USE zora;
GO

CREATE TABLE zora_users (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    username VARCHAR(255) NOT NULL,
    password VARCHAR(255) NOT NULL,
    email VARCHAR(255) NOT NULL,
    created_at DATETIME2 DEFAULT GETDATE(),
    deleted BIT DEFAULT 0 NOT NULL,
    CONSTRAINT CHK_User_Email CHECK (email LIKE '%_@__%.__%')
);

CREATE UNIQUE INDEX IX_User_Username ON zora_users(username) WHERE deleted = 0;
CREATE UNIQUE INDEX IX_User_Email ON zora_users(email) WHERE deleted = 0;

CREATE TABLE zora_roles (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    created_at DATETIME2 DEFAULT GETDATE(),
    deleted BIT DEFAULT 0 NOT NULL
);

CREATE UNIQUE INDEX IX_Role_Name ON zora_roles(name) WHERE deleted = 0;

CREATE TABLE zora_user_roles (
    user_id BIGINT NOT NULL,
    role_id BIGINT NOT NULL,
    PRIMARY KEY (user_id, role_id),
    FOREIGN KEY (user_id) REFERENCES zora_users(id),
    FOREIGN KEY (role_id) REFERENCES zora_roles(id)
);

CREATE TABLE zora_permissions (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    description VARCHAR(MAX),
    permission_string CHAR(5) NOT NULL,
    created_at DATETIME2 DEFAULT GETDATE(),
    deleted BIT DEFAULT 0 NOT NULL,
    CONSTRAINT CHK_Permission_String CHECK (permission_string LIKE '[0-1][0-1][0-1][0-1][0-1]')
);

CREATE UNIQUE INDEX IX_Permission_Name ON zora_permissions(name) WHERE deleted = 0;
CREATE INDEX IX_Permission_String ON zora_permissions(permission_string);

CREATE TABLE zora_role_permissions (
    role_id BIGINT NOT NULL,
    permission_id BIGINT NOT NULL,
    PRIMARY KEY (role_id, permission_id),
    FOREIGN KEY (role_id) REFERENCES zora_roles(id),
    FOREIGN KEY (permission_id) REFERENCES zora_permissions(id)
);

CREATE TABLE zora_work_items (
    work_item_id BIGINT IDENTITY(1,1) PRIMARY KEY,
    type VARCHAR(50) NOT NULL,
    name VARCHAR(255) NOT NULL,
    description VARCHAR(MAX),
    assignee_id BIGINT,
    status VARCHAR(50) NOT NULL,
    start_date DATETIME2,
    due_date DATETIME2,
    completion_percentage DECIMAL(5,2) CHECK (completion_percentage BETWEEN 0 AND 100),
    estimated_hours DECIMAL(10,2) CHECK (estimated_hours >= 0),
    actual_hours DECIMAL(10,2) CHECK (actual_hours >= 0),
    created_at DATETIME2 DEFAULT GETDATE(),
    created_by BIGINT,
    updated_at DATETIME2,
    updated_by BIGINT,
    deleted BIT DEFAULT 0 NOT NULL,
    FOREIGN KEY (assignee_id) REFERENCES zora_users(id),
    FOREIGN KEY (created_by) REFERENCES zora_users(id),
    FOREIGN KEY (updated_by) REFERENCES zora_users(id)
);

CREATE INDEX IX_WorkItem_Types ON zora_work_items(type) WHERE deleted = 0;
CREATE INDEX IX_WorkItem_Statuses ON zora_work_items(status) WHERE deleted = 0;
CREATE INDEX IX_WorkItem_AssigneeIds ON zora_work_items(assignee_id) WHERE deleted = 0;
CREATE INDEX IX_WorkItem_Name ON zora_work_items(name) WHERE deleted = 0;
CREATE INDEX IX_WorkItem_CreatedBy ON zora_work_items(created_by) WHERE deleted = 0;
CREATE INDEX IX_WorkItem_UpdatedBy ON zora_work_items(updated_by) WHERE deleted = 0;
CREATE INDEX IX_WorkItem_DueDates ON zora_work_items(due_date) WHERE deleted = 0;
CREATE INDEX IX_WorkItem_StartDates ON zora_work_items(start_date) WHERE deleted = 0;

CREATE TABLE zora_permission_work_items (
    permission_id BIGINT,
    work_item_id BIGINT,
    PRIMARY KEY (permission_id, work_item_id),
    FOREIGN KEY (permission_id) REFERENCES zora_permissions(id),
    FOREIGN KEY (work_item_id) REFERENCES zora_work_items(work_item_id)
);

CREATE TABLE zora_programs (
    work_item_id BIGINT PRIMARY KEY,
    description VARCHAR(MAX),
    deleted BIT DEFAULT 0,
    FOREIGN KEY (work_item_id) REFERENCES zora_work_items(work_item_id)
);

CREATE TABLE zora_projects (
    work_item_id BIGINT PRIMARY KEY,
    program_id BIGINT,
    project_manager_id BIGINT,
    deleted BIT DEFAULT 0,
    FOREIGN KEY (work_item_id) REFERENCES zora_work_items(work_item_id),
    FOREIGN KEY (program_id) REFERENCES zora_work_items(work_item_id),
    FOREIGN KEY (project_manager_id) REFERENCES zora_users(id)
);

CREATE TABLE zora_tasks (
    work_item_id BIGINT PRIMARY KEY,
    project_id BIGINT,
    priority VARCHAR(50),
    parent_task_id BIGINT,
    deleted BIT DEFAULT 0,
    FOREIGN KEY (work_item_id) REFERENCES zora_work_items(work_item_id),
    FOREIGN KEY (project_id) REFERENCES zora_work_items(work_item_id),
    FOREIGN KEY (parent_task_id) REFERENCES zora_work_items(work_item_id)
);

CREATE TABLE zora_work_item_relationships (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    source_item_id BIGINT,
    target_item_id BIGINT,
    relationship_type VARCHAR(50),
    created_at DATETIME2 DEFAULT GETDATE(),
    deleted BIT DEFAULT 0,
    FOREIGN KEY (source_item_id) REFERENCES zora_work_items(work_item_id),
    FOREIGN KEY (target_item_id) REFERENCES zora_work_items(work_item_id)
);

CREATE TABLE assets (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    description VARCHAR(MAX),
    asset_path VARCHAR(MAX) NOT NULL,
    created_at DATETIME2 DEFAULT GETDATE(),
    created_by BIGINT,
    updated_at DATETIME2,
    updated_by BIGINT,
    deleted BIT DEFAULT 0 NOT NULL,
    FOREIGN KEY (created_by) REFERENCES zora_users(id),
    FOREIGN KEY (updated_by) REFERENCES zora_users(id)
);

CREATE INDEX IX_Asset_Name ON assets(name) WHERE deleted = 0;
CREATE INDEX IX_Asset_CreatedBy ON assets(created_by) WHERE deleted = 0;
CREATE INDEX IX_Asset_UpdatedBy ON assets(updated_by) WHERE deleted = 0;

CREATE TABLE zora_work_item_assets (
    work_item_id BIGINT,
    asset_id BIGINT,
    PRIMARY KEY (work_item_id, asset_id),
    FOREIGN KEY (work_item_id) REFERENCES zora_work_items(work_item_id),
    FOREIGN KEY (asset_id) REFERENCES assets(id)
);

INSERT INTO zora_users (username, password, email) VALUES ('tnebes', '$2a$12$VG2.zKWyQq0rt/MJgarft.AaFr36jlICrCzo5YEN2CWAjtCWSmw6K', 'tnebes@draucode.com');
INSERT INTO zora_roles (name) VALUES ('Admin');
INSERT INTO zora_user_roles (user_id, role_id)
   SELECT u.id, r.id
      FROM zora_users u, zora_roles r
      WHERE u.username = 'tnebes' AND r.name = 'Admin';
