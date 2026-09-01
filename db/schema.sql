IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260827183955_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] int NOT NULL IDENTITY,
        [Description] nvarchar(200) NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260827183955_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] int NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260827183955_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] int NOT NULL IDENTITY,
        [FirstName] nvarchar(50) NOT NULL,
        [LastName] nvarchar(50) NOT NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [RoleId] int NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUsers_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260827183955_InitialCreate'
)
BEGIN
    CREATE TABLE [Accounts] (
        [Id] int NOT NULL IDENTITY,
        [Money] decimal(18,2) NOT NULL,
        [IsBlocked] bit NOT NULL DEFAULT CAST(0 AS bit),
        [UserId] int NOT NULL,
        CONSTRAINT [PK_Accounts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Accounts_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260827183955_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260827183955_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] int NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260827183955_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] int NOT NULL,
        [RoleId] int NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260827183955_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] int NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260827183955_InitialCreate'
)
BEGIN
    CREATE TABLE [Transactions] (
        [Id] int NOT NULL IDENTITY,
        [Amount] decimal(18,2) NOT NULL,
        [Type] int NOT NULL,
        [Concept] nvarchar(200) NULL,
        [Date] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [AccountId] int NOT NULL,
        [ToAccountId] int NULL,
        CONSTRAINT [PK_Transactions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Transactions_Accounts_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [Accounts] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Transactions_Accounts_ToAccountId] FOREIGN KEY ([ToAccountId]) REFERENCES [Accounts] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260827183955_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Accounts_UserId] ON [Accounts] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260827183955_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260827183955_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260827183955_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260827183955_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260827183955_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260827183955_InitialCreate'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260827183955_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_AspNetUsers_Email] ON [AspNetUsers] ([Email]) WHERE [Email] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260827183955_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUsers_RoleId] ON [AspNetUsers] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260827183955_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260827183955_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Transactions_AccountId] ON [Transactions] ([AccountId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260827183955_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Transactions_Date] ON [Transactions] ([Date]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260827183955_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Transactions_ToAccountId] ON [Transactions] ([ToAccountId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260827183955_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260827183955_InitialCreate', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260827204153_SeedData'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ConcurrencyStamp', N'Description', N'Name', N'NormalizedName') AND [object_id] = OBJECT_ID(N'[AspNetRoles]'))
        SET IDENTITY_INSERT [AspNetRoles] ON;
    EXEC(N'INSERT INTO [AspNetRoles] ([Id], [ConcurrencyStamp], [Description], [Name], [NormalizedName])
    VALUES (1, N''b9b271b3-438c-4bf7-a728-49464f787828'', N''Administrador con permisos elevados para gestionar usuarios'', N''Admin'', N''ADMIN''),
    (2, N''5e5705da-92b0-43c5-a07c-19a0c038dcf2'', N''Usuario estandar de la billetera virtual'', N''User'', N''USER'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ConcurrencyStamp', N'Description', N'Name', N'NormalizedName') AND [object_id] = OBJECT_ID(N'[AspNetRoles]'))
        SET IDENTITY_INSERT [AspNetRoles] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260827204153_SeedData'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AccessFailedCount', N'ConcurrencyStamp', N'CreatedAt', N'Email', N'EmailConfirmed', N'FirstName', N'LastName', N'LockoutEnabled', N'LockoutEnd', N'NormalizedEmail', N'NormalizedUserName', N'PasswordHash', N'PhoneNumber', N'PhoneNumberConfirmed', N'RoleId', N'SecurityStamp', N'TwoFactorEnabled', N'UserName') AND [object_id] = OBJECT_ID(N'[AspNetUsers]'))
        SET IDENTITY_INSERT [AspNetUsers] ON;
    EXEC(N'INSERT INTO [AspNetUsers] ([Id], [AccessFailedCount], [ConcurrencyStamp], [CreatedAt], [Email], [EmailConfirmed], [FirstName], [LastName], [LockoutEnabled], [LockoutEnd], [NormalizedEmail], [NormalizedUserName], [PasswordHash], [PhoneNumber], [PhoneNumberConfirmed], [RoleId], [SecurityStamp], [TwoFactorEnabled], [UserName])
    VALUES (1, 0, N''seed-admin-concurrency'', ''2025-01-01T00:00:00.0000000Z'', N''admin@digitalars.com'', CAST(1 AS bit), N''Admin'', N''DigitalArs'', CAST(0 AS bit), NULL, N''ADMIN@DIGITALARS.COM'', N''ADMIN@DIGITALARS.COM'', N''AQAAAAIAAYagAAAAENO87HGO7ibu/kR6bblZLBu39LF1P9oeSEu7bwGb0YRvny7KouBk+XFrlxztTvecMQ=='', NULL, CAST(0 AS bit), 1, N''SEED-ADMIN-SECURITY-STAMP'', CAST(0 AS bit), N''admin@digitalars.com''),
    (2, 0, N''seed-user1-concurrency'', ''2025-01-01T00:00:00.0000000Z'', N''robercarlos3@gmail.com'', CAST(1 AS bit), N''Roberto'', N''Carlos'', CAST(0 AS bit), NULL, N''ROBERCARLOS3@GMAIL.COM'', N''ROBERCARLOS3@GMAIL.COM'', N''AQAAAAIAAYagAAAAELKOHh9OA8J6+VbOTSSbRWvcAbbwaPQ9dZcgFx02YAC1LeN/DPShnsiygednoXJUNQ=='', NULL, CAST(0 AS bit), 2, N''SEED-USER1-SECURITY-STAMP'', CAST(0 AS bit), N''robercarlos3@gmail.com''),
    (3, 0, N''seed-user2-concurrency'', ''2025-01-01T00:00:00.0000000Z'', N''mokha@gmail.com'', CAST(1 AS bit), N''Mohammed'', N''Khan'', CAST(0 AS bit), NULL, N''MOKHA@GMAIL.COM'', N''MOKHA@GMAIL.COM'', N''AQAAAAIAAYagAAAAEErdfRgSxQZB1799p6YXG2T/bLL4bqGYPBRcYHsKas3tZrZUfw1cn6bK9oGtgvtkhA=='', NULL, CAST(0 AS bit), 2, N''SEED-USER2-SECURITY-STAMP'', CAST(0 AS bit), N''mokha@gmail.com'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AccessFailedCount', N'ConcurrencyStamp', N'CreatedAt', N'Email', N'EmailConfirmed', N'FirstName', N'LastName', N'LockoutEnabled', N'LockoutEnd', N'NormalizedEmail', N'NormalizedUserName', N'PasswordHash', N'PhoneNumber', N'PhoneNumberConfirmed', N'RoleId', N'SecurityStamp', N'TwoFactorEnabled', N'UserName') AND [object_id] = OBJECT_ID(N'[AspNetUsers]'))
        SET IDENTITY_INSERT [AspNetUsers] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260827204153_SeedData'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Money', N'UserId') AND [object_id] = OBJECT_ID(N'[Accounts]'))
        SET IDENTITY_INSERT [Accounts] ON;
    EXEC(N'INSERT INTO [Accounts] ([Id], [Money], [UserId])
    VALUES (1, 500000.0, 1),
    (2, 260000.0, 2),
    (3, 185000.5, 3)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Money', N'UserId') AND [object_id] = OBJECT_ID(N'[Accounts]'))
        SET IDENTITY_INSERT [Accounts] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260827204153_SeedData'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260827204153_SeedData', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901031538_Sprint1Fixes_UserRolesSeed_IsDeletedIndex'
)
BEGIN
    EXEC(N'UPDATE [AspNetRoles] SET [ConcurrencyStamp] = N''seed-role-admin-concurrency''
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901031538_Sprint1Fixes_UserRolesSeed_IsDeletedIndex'
)
BEGIN
    EXEC(N'UPDATE [AspNetRoles] SET [ConcurrencyStamp] = N''seed-role-user-concurrency''
    WHERE [Id] = 2;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901031538_Sprint1Fixes_UserRolesSeed_IsDeletedIndex'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'RoleId', N'UserId') AND [object_id] = OBJECT_ID(N'[AspNetUserRoles]'))
        SET IDENTITY_INSERT [AspNetUserRoles] ON;
    EXEC(N'INSERT INTO [AspNetUserRoles] ([RoleId], [UserId])
    VALUES (1, 1),
    (2, 2),
    (2, 3)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'RoleId', N'UserId') AND [object_id] = OBJECT_ID(N'[AspNetUserRoles]'))
        SET IDENTITY_INSERT [AspNetUserRoles] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901031538_Sprint1Fixes_UserRolesSeed_IsDeletedIndex'
)
BEGIN
    CREATE INDEX [IX_AspNetUsers_IsDeleted] ON [AspNetUsers] ([IsDeleted]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901031538_Sprint1Fixes_UserRolesSeed_IsDeletedIndex'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260901031538_Sprint1Fixes_UserRolesSeed_IsDeletedIndex', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901045432_AddAccountCreatedAt'
)
BEGIN
    ALTER TABLE [Accounts] ADD [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE());
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901045432_AddAccountCreatedAt'
)
BEGIN
    EXEC(N'UPDATE [Accounts] SET [CreatedAt] = ''2025-01-01T00:00:00.0000000Z''
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901045432_AddAccountCreatedAt'
)
BEGIN
    EXEC(N'UPDATE [Accounts] SET [CreatedAt] = ''2025-01-01T00:00:00.0000000Z''
    WHERE [Id] = 2;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901045432_AddAccountCreatedAt'
)
BEGIN
    EXEC(N'UPDATE [Accounts] SET [CreatedAt] = ''2025-01-01T00:00:00.0000000Z''
    WHERE [Id] = 3;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260901045432_AddAccountCreatedAt'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260901045432_AddAccountCreatedAt', N'10.0.11');
END;

COMMIT;
GO

