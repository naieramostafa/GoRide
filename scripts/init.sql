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
GO

CREATE TABLE [Users] (
    [Id] uniqueidentifier NOT NULL,
    [FirstName] nvarchar(100) NOT NULL,
    [LastName] nvarchar(100) NOT NULL,
    [Email] nvarchar(200) NOT NULL,
    [Phone] nvarchar(20) NOT NULL,
    [Role] nvarchar(20) NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [LastLoginAt] datetime2 NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Vehicles] (
    [Id] uniqueidentifier NOT NULL,
    [Make] nvarchar(50) NOT NULL,
    [Model] nvarchar(50) NOT NULL,
    [Year] nvarchar(max) NOT NULL,
    [Color] nvarchar(max) NOT NULL,
    [LicensePlate] nvarchar(20) NOT NULL,
    [Type] nvarchar(20) NOT NULL,
    [Capacity] int NOT NULL,
    CONSTRAINT [PK_Vehicles] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Passengers] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [Rating] float(3) NOT NULL,
    [TotalRides] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Passengers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Passengers_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [TaskItems] (
    [Id] uniqueidentifier NOT NULL,
    [AssignedToUserId] uniqueidentifier NULL,
    [Title] nvarchar(200) NOT NULL,
    [Description] nvarchar(2000) NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [Priority] nvarchar(20) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [DueDate] datetime2 NULL,
    [CompletedAt] datetime2 NULL,
    [RelatedEntityType] nvarchar(max) NULL,
    [RelatedEntityId] uniqueidentifier NULL,
    CONSTRAINT [PK_TaskItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TaskItems_Users_AssignedToUserId] FOREIGN KEY ([AssignedToUserId]) REFERENCES [Users] ([Id])
);
GO

CREATE TABLE [Drivers] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [LicenseNumber] nvarchar(50) NOT NULL,
    [IsAvailable] bit NOT NULL,
    [IsVerified] bit NOT NULL,
    [Rating] float(3) NOT NULL,
    [TotalRides] int NOT NULL,
    [Latitude] float NULL,
    [Longitude] float NULL,
    [Address] nvarchar(500) NULL,
    [VehicleId] uniqueidentifier NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Drivers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Drivers_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Drivers_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([Id])
);
GO

CREATE TABLE [Rides] (
    [Id] uniqueidentifier NOT NULL,
    [PassengerId] uniqueidentifier NOT NULL,
    [DriverId] uniqueidentifier NULL,
    [PickupLatitude] float NOT NULL,
    [PickupLongitude] float NOT NULL,
    [PickupAddress] nvarchar(500) NULL,
    [DropoffLatitude] float NOT NULL,
    [DropoffLongitude] float NOT NULL,
    [DropoffAddress] nvarchar(500) NULL,
    [Status] nvarchar(20) NOT NULL,
    [FareAmount] decimal(18,2) NOT NULL,
    [Fare_Currency] nvarchar(max) NOT NULL,
    [FinalFareAmount] decimal(18,2) NULL,
    [FinalFare_Currency] nvarchar(max) NULL,
    [DistanceKm] float NULL,
    [DurationMinutes] int NULL,
    [RequestedAt] datetime2 NOT NULL,
    [AcceptedAt] datetime2 NULL,
    [StartedAt] datetime2 NULL,
    [CompletedAt] datetime2 NULL,
    [CancelledAt] datetime2 NULL,
    [CancellationReason] nvarchar(max) NULL,
    CONSTRAINT [PK_Rides] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Rides_Drivers_DriverId] FOREIGN KEY ([DriverId]) REFERENCES [Drivers] ([Id]),
    CONSTRAINT [FK_Rides_Passengers_PassengerId] FOREIGN KEY ([PassengerId]) REFERENCES [Passengers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Payments] (
    [Id] uniqueidentifier NOT NULL,
    [RideId] uniqueidentifier NOT NULL,
    [PassengerId] uniqueidentifier NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Amount_Currency] nvarchar(max) NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [StripePaymentIntentId] nvarchar(200) NULL,
    [StripeChargeId] nvarchar(200) NULL,
    [FailureReason] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [ProcessedAt] datetime2 NULL,
    CONSTRAINT [PK_Payments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Payments_Rides_RideId] FOREIGN KEY ([RideId]) REFERENCES [Rides] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Ratings] (
    [Id] uniqueidentifier NOT NULL,
    [RideId] uniqueidentifier NOT NULL,
    [RatedByUserId] uniqueidentifier NOT NULL,
    [RatedUserId] uniqueidentifier NOT NULL,
    [Score] int NOT NULL,
    [Comment] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Ratings] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Ratings_Rides_RideId] FOREIGN KEY ([RideId]) REFERENCES [Rides] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Drivers_UserId] ON [Drivers] ([UserId]);
GO

CREATE INDEX [IX_Drivers_VehicleId] ON [Drivers] ([VehicleId]);
GO

CREATE INDEX [IX_Passengers_UserId] ON [Passengers] ([UserId]);
GO

CREATE INDEX [IX_Payments_RideId] ON [Payments] ([RideId]);
GO

CREATE INDEX [IX_Ratings_RideId] ON [Ratings] ([RideId]);
GO

CREATE INDEX [IX_Rides_DriverId] ON [Rides] ([DriverId]);
GO

CREATE INDEX [IX_Rides_PassengerId] ON [Rides] ([PassengerId]);
GO

CREATE INDEX [IX_TaskItems_AssignedToUserId] ON [TaskItems] ([AssignedToUserId]);
GO

CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260723225240_InitialCreate', N'8.0.0');
GO

COMMIT;
GO

