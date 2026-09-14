/*
  Chạy trên database EcommerceRecomment nếu bạn đã có schema cũ.
  Nếu dùng EF Core migration thì chỉ cần:
      dotnet ef database update
*/

IF COL_LENGTH('AspNetUsers', 'SellerStatus') IS NULL
    ALTER TABLE AspNetUsers ADD SellerStatus nvarchar(max) NOT NULL CONSTRAINT DF_AspNetUsers_SellerStatus DEFAULT 'None';

IF COL_LENGTH('Products', 'OwnerId') IS NULL
    ALTER TABLE Products ADD OwnerId nvarchar(450) NULL;

IF COL_LENGTH('Orders', 'ReceiverName') IS NULL
    ALTER TABLE Orders ADD ReceiverName nvarchar(200) NOT NULL CONSTRAINT DF_Orders_ReceiverName DEFAULT '';

IF COL_LENGTH('Orders', 'Phone') IS NULL
    ALTER TABLE Orders ADD Phone nvarchar(30) NOT NULL CONSTRAINT DF_Orders_Phone DEFAULT '';

IF COL_LENGTH('Orders', 'ShippingAddress') IS NULL
    ALTER TABLE Orders ADD ShippingAddress nvarchar(500) NOT NULL CONSTRAINT DF_Orders_ShippingAddress DEFAULT '';

IF COL_LENGTH('Orders', 'PaymentMethod') IS NULL
    ALTER TABLE Orders ADD PaymentMethod nvarchar(50) NOT NULL CONSTRAINT DF_Orders_PaymentMethod DEFAULT 'COD';

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Products_OwnerId' AND object_id = OBJECT_ID('Products'))
    CREATE INDEX IX_Products_OwnerId ON Products(OwnerId);

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Products_AspNetUsers_OwnerId')
    ALTER TABLE Products ADD CONSTRAINT FK_Products_AspNetUsers_OwnerId FOREIGN KEY (OwnerId) REFERENCES AspNetUsers(Id) ON DELETE NO ACTION;

IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE NormalizedName = 'SELLER')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (CONVERT(nvarchar(450), NEWID()), 'Seller', 'SELLER', CONVERT(nvarchar(36), NEWID()));
END;
