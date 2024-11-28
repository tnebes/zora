use master;

IF EXISTS (SELECT name FROM sys.databases WHERE name = 'zora')
BEGIN
    ALTER DATABASE zora SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE zora;
END

CREATE DATABASE zora;
GO

USE zora;
GO

CREATE TABLE zora_user (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    username VARCHAR(255) NOT NULL,
    password VARCHAR(255) NOT NULL,
    email VARCHAR(255) NOT NULL,
    created_at DATETIME2 DEFAULT GETDATE()
);

ALTER TABLE zora_user
ADD CONSTRAINT UQ_User_Username UNIQUE (username),
    CONSTRAINT UQ_User_Email UNIQUE (email);

CREATE TABLE zora_role (
      id BIGINT IDENTITY(1,1) PRIMARY KEY,
      name VARCHAR(255) NOT NULL,
      created_at DATETIME2 DEFAULT GETDATE()
)

CREATE TABLE zora_user_role (
    user_id BIGINT,
    role_id BIGINT,
    PRIMARY KEY (user_id, role_id),
    FOREIGN KEY (user_id) REFERENCES zora_user(id),
    FOREIGN KEY (role_id) REFERENCES zora_role(id)
);

CREATE TABLE zora_permission (
      id BIGINT IDENTITY(1,1) PRIMARY KEY,
      name VARCHAR(255) NOT NULL,
      description TEXT,
      permission_string VARCHAR(4) NOT NULL,
      created_at DATETIME2 DEFAULT GETDATE(),
      CONSTRAINT CHK_Permission_String CHECK (permission_string LIKE '[0-1][0-1][0-1][0-1]')
)

CREATE TABLE zora_role_permission (
    role_id BIGINT,
    permission_id BIGINT,
    PRIMARY KEY (role_id, permission_id),
    FOREIGN KEY (role_id) REFERENCES zora_role(id),
    FOREIGN KEY (permission_id) REFERENCES zora_permission(id)
);

CREATE TABLE zora_work_item (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
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
    FOREIGN KEY (assignee_id) REFERENCES zora_user(id),
    FOREIGN KEY (created_by) REFERENCES zora_user(id),
    FOREIGN KEY (updated_by) REFERENCES zora_user(id)
);

CREATE TABLE zora_permission_work_item (
    permission_id BIGINT,
    work_item_id BIGINT,
    PRIMARY KEY (permission_id, work_item_id),
    FOREIGN KEY (permission_id) REFERENCES zora_permission(id),
    FOREIGN KEY (work_item_id) REFERENCES zora_work_item(id)
);

CREATE TABLE zora_program (
    work_item_id BIGINT PRIMARY KEY,
    description TEXT,
    FOREIGN KEY (work_item_id) REFERENCES zora_work_item(id)
);

CREATE TABLE zora_project (
    work_item_id BIGINT PRIMARY KEY,
    program_id BIGINT,
    project_manager_id BIGINT,
    FOREIGN KEY (work_item_id) REFERENCES zora_work_item(id),
    FOREIGN KEY (program_id) REFERENCES zora_work_item(id),
    FOREIGN KEY (project_manager_id) REFERENCES zora_user(id)
);

CREATE TABLE zora_task (
    work_item_id BIGINT PRIMARY KEY,
    project_id BIGINT,
    priority VARCHAR(50),
    parent_task_id BIGINT,
    FOREIGN KEY (work_item_id) REFERENCES zora_work_item(id),
    FOREIGN KEY (project_id) REFERENCES zora_work_item(id),
    FOREIGN KEY (parent_task_id) REFERENCES zora_work_item(id)
);

CREATE TABLE zora_work_item_relationship (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    source_item_id BIGINT,
    target_item_id BIGINT,
    relationship_type VARCHAR(50),
    created_at DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (source_item_id) REFERENCES zora_work_item(id),
    FOREIGN KEY (target_item_id) REFERENCES zora_work_item(id)
);

CREATE TABLE asset (
      id BIGINT IDENTITY(1,1) PRIMARY KEY,
      name VARCHAR(255) NOT NULL,
      description TEXT,
      asset_path TEXT NOT NULL,
      created_at DATETIME2 DEFAULT GETDATE(),
      created_by BIGINT,
      updated_at DATETIME2,
      updated_by BIGINT,
      FOREIGN KEY (created_by) REFERENCES zora_user(id),
      FOREIGN KEY (updated_by) REFERENCES zora_user(id)
)

CREATE TABLE zora_work_item_asset (
    work_item_id BIGINT,
    asset_id BIGINT,
    PRIMARY KEY (work_item_id, asset_id),
    FOREIGN KEY (work_item_id) REFERENCES zora_work_item(id),
    FOREIGN KEY (asset_id) REFERENCES asset(id)
);

CREATE INDEX IX_WorkItem_Type ON zora_work_item(type);
CREATE INDEX IX_WorkItem_Status ON zora_work_item(status);
CREATE INDEX IX_WorkItem_AssigneeId ON zora_work_item(assignee_id);
CREATE INDEX IX_Task_Priority ON zora_task(priority);
CREATE INDEX IX_WorkItemRelationship_RelationshipType ON zora_work_item_relationship(relationship_type);
CREATE INDEX IX_WorkItemAsset_AssetId ON zora_work_item_asset(asset_id);
CREATE INDEX IX_WorkItemAsset_WorkItemId ON zora_work_item_asset(work_item_id);

INSERT INTO zora_user (username, password, email) VALUES ('tnebes', 'letmeinside', 'tnebes@draucode.com');
INSERT INTO zora_role (name) VALUES ('Admin');
INSERT INTO zora_user_role (user_id, role_id)
   SELECT u.id, r.id
      FROM zora_user u, zora_role r
      WHERE u.username = 'tnebes' AND r.name = 'Admin';
