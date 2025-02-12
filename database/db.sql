USE master;

IF EXISTS (SELECT name FROM sys.databases WHERE name = 'zora')
BEGIN
    ALTER DATABASE zora SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE zora;
END

CREATE DATABASE zora;
GO

USE zora;
GO

CREATE TABLE zora_users (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    username VARCHAR(255) NOT NULL,
    password VARCHAR(255) NOT NULL,
    email VARCHAR(255) NOT NULL,
    created_at DATETIME2 DEFAULT GETDATE(),
    deleted BIT DEFAULT 0
);

ALTER TABLE zora_users
ADD CONSTRAINT UQ_User_Username UNIQUE (username),
    CONSTRAINT UQ_User_Email UNIQUE (email);

CREATE TABLE zora_roles (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    created_at DATETIME2 DEFAULT GETDATE(),
    deleted BIT DEFAULT 0
);

CREATE TABLE zora_user_roles (
    user_id BIGINT,
    role_id BIGINT,
    PRIMARY KEY (user_id, role_id),
    FOREIGN KEY (user_id) REFERENCES zora_users(id),
    FOREIGN KEY (role_id) REFERENCES zora_roles(id)
);

CREATE TABLE zora_permissions (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    permission_string CHAR(5) NOT NULL,
    created_at DATETIME2 DEFAULT GETDATE(),
    deleted BIT DEFAULT 0,
    CONSTRAINT CHK_Permission_String CHECK (permission_string LIKE '[0-1][0-1][0-1][0-1][0-1]')
);

CREATE TABLE zora_role_permissions (
    role_id BIGINT,
    permission_id BIGINT,
    PRIMARY KEY (role_id, permission_id),
    FOREIGN KEY (role_id) REFERENCES zora_roles(id),
    FOREIGN KEY (permission_id) REFERENCES zora_permissions(id)
);

CREATE TABLE zora_work_items (
    work_item_id BIGINT IDENTITY(1,1) PRIMARY KEY,
    type VARCHAR(50) NOT NULL,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    assignee_id BIGINT,
    status VARCHAR(50) NOT NULL,
    start_date DATETIME2,
    due_date DATETIME2,
    completion_percentage DECIMAL(5,2),
    estimated_hours DECIMAL(10,2),
    actual_hours DECIMAL(10,2),
    created_at DATETIME2 DEFAULT GETDATE(),
    created_by BIGINT,
    updated_at DATETIME2,
    updated_by BIGINT,
    deleted BIT DEFAULT 0,
    FOREIGN KEY (assignee_id) REFERENCES zora_users(id),
    FOREIGN KEY (created_by) REFERENCES zora_users(id),
    FOREIGN KEY (updated_by) REFERENCES zora_users(id)
);

CREATE TABLE zora_permission_work_items (
    permission_id BIGINT,
    work_item_id BIGINT,
    PRIMARY KEY (permission_id, work_item_id),
    FOREIGN KEY (permission_id) REFERENCES zora_permissions(id),
    FOREIGN KEY (work_item_id) REFERENCES zora_work_items(work_item_id)
);

CREATE TABLE zora_programs (
    work_item_id BIGINT PRIMARY KEY,
    description TEXT,
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
    description TEXT,
    asset_path TEXT NOT NULL,
    created_at DATETIME2 DEFAULT GETDATE(),
    created_by BIGINT,
    updated_at DATETIME2,
    updated_by BIGINT,
    deleted BIT DEFAULT 0,
    FOREIGN KEY (created_by) REFERENCES zora_users(id),
    FOREIGN KEY (updated_by) REFERENCES zora_users(id)
);

CREATE TABLE zora_work_item_assets (
    work_item_id BIGINT,
    asset_id BIGINT,
    PRIMARY KEY (work_item_id, asset_id),
    FOREIGN KEY (work_item_id) REFERENCES zora_work_items(work_item_id),
    FOREIGN KEY (asset_id) REFERENCES assets(id)
);

CREATE INDEX IX_WorkItem_Types ON zora_work_items(type);
CREATE INDEX IX_WorkItem_Statuses ON zora_work_items(status);
CREATE INDEX IX_WorkItem_AssigneeIds ON zora_work_items(assignee_id);
CREATE INDEX IX_Task_Priorities ON zora_tasks(priority);
CREATE INDEX IX_WorkItemRelationship_RelationshipTypes ON zora_work_item_relationships(relationship_type);
CREATE INDEX IX_WorkItemAsset_AssetIds ON zora_work_item_assets(asset_id);
CREATE INDEX IX_WorkItemAsset_WorkItemIds ON zora_work_item_assets(work_item_id);

CREATE INDEX IX_User_Username ON zora_users(username);
CREATE INDEX IX_User_Email ON zora_users(email);

CREATE INDEX IX_Role_Name ON zora_roles(name);
CREATE INDEX IX_Permission_Name ON zora_permissions(name);
CREATE INDEX IX_Permission_String ON zora_permissions(permission_string);

CREATE INDEX IX_WorkItem_Name ON zora_work_items(name);
CREATE INDEX IX_WorkItem_CreatedBy ON zora_work_items(created_by);
CREATE INDEX IX_WorkItem_UpdatedBy ON zora_work_items(updated_by);
CREATE INDEX IX_WorkItem_DueDates ON zora_work_items(due_date);
CREATE INDEX IX_WorkItem_StartDates ON zora_work_items(start_date);

CREATE INDEX IX_Project_ProjectManager ON zora_projects(project_manager_id);
CREATE INDEX IX_Project_Program ON zora_projects(program_id);

CREATE INDEX IX_Task_Project ON zora_tasks(project_id);
CREATE INDEX IX_Task_ParentTask ON zora_tasks(parent_task_id);

CREATE INDEX IX_Asset_Name ON assets(name);
CREATE INDEX IX_Asset_CreatedBy ON assets(created_by);
CREATE INDEX IX_Asset_UpdatedBy ON assets(updated_by);

INSERT INTO zora_users (username, password, email) VALUES ('tnebes', '$2a$12$VG2.zKWyQq0rt/MJgarft.AaFr36jlICrCzo5YEN2CWAjtCWSmw6K', 'tnebes@draucode.com');
INSERT INTO zora_roles (name) VALUES ('Admin');
INSERT INTO zora_user_roles (user_id, role_id)
   SELECT u.id, r.id
      FROM zora_users u, zora_roles r
      WHERE u.username = 'tnebes' AND r.name = 'Admin';
