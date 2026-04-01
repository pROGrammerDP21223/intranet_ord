-- SQL Server: insert industries if missing (by Name). Run against your application database.
-- Table: Industries (see ApplicationDbContext / Industry entity)

SET NOCOUNT ON;

DECLARE @Now datetime2 = SYSUTCDATETIME();

INSERT INTO Industries (Name, Description, Image, TopIndustry, BannerIndustry, CreatedAt, IsDeleted)
SELECT N'Agriculture', N'Farming equipment, irrigation systems, and agro solutions.', N'https://img.icons8.com/color/96/tractor.png', 1, 0, @Now, 0
WHERE NOT EXISTS (SELECT 1 FROM Industries WHERE Name = N'Agriculture');

INSERT INTO Industries (Name, Description, Image, TopIndustry, BannerIndustry, CreatedAt, IsDeleted)
SELECT N'Electrical & Electronics', N'Electrical components, electronics, and automation systems.', N'https://img.icons8.com/color/96/electronics.png', 1, 0, @Now, 0
WHERE NOT EXISTS (SELECT 1 FROM Industries WHERE Name = N'Electrical & Electronics');

INSERT INTO Industries (Name, Description, Image, TopIndustry, BannerIndustry, CreatedAt, IsDeleted)
SELECT N'Mechanical & Engineering', N'Industrial machinery and engineering components.', N'https://img.icons8.com/color/96/gear.png', 1, 0, @Now, 0
WHERE NOT EXISTS (SELECT 1 FROM Industries WHERE Name = N'Mechanical & Engineering');

INSERT INTO Industries (Name, Description, Image, TopIndustry, BannerIndustry, CreatedAt, IsDeleted)
SELECT N'Pharmaceuticals', N'Medicines, pharma manufacturing, and lab products.', N'https://img.icons8.com/color/96/pill.png', 0, 0, @Now, 0
WHERE NOT EXISTS (SELECT 1 FROM Industries WHERE Name = N'Pharmaceuticals');

INSERT INTO Industries (Name, Description, Image, TopIndustry, BannerIndustry, CreatedAt, IsDeleted)
SELECT N'Rubber & Plastics', N'Plastic products, rubber materials, and polymer solutions.', N'https://img.icons8.com/color/96/plastic.png', 0, 0, @Now, 0
WHERE NOT EXISTS (SELECT 1 FROM Industries WHERE Name = N'Rubber & Plastics');

INSERT INTO Industries (Name, Description, Image, TopIndustry, BannerIndustry, CreatedAt, IsDeleted)
SELECT N'Packaging & Labeling', N'Packaging machinery and labeling solutions.', N'https://img.icons8.com/color/96/box.png', 0, 0, @Now, 0
WHERE NOT EXISTS (SELECT 1 FROM Industries WHERE Name = N'Packaging & Labeling');

INSERT INTO Industries (Name, Description, Image, TopIndustry, BannerIndustry, CreatedAt, IsDeleted)
SELECT N'Health Products, Drug and Medicine', N'Healthcare products and OTC medicines.', N'https://img.icons8.com/color/96/medicine.png', 0, 0, @Now, 0
WHERE NOT EXISTS (SELECT 1 FROM Industries WHERE Name = N'Health Products, Drug and Medicine');

INSERT INTO Industries (Name, Description, Image, TopIndustry, BannerIndustry, CreatedAt, IsDeleted)
SELECT N'Chemical & Petrochemical', N'Chemicals, petrochemical products, and processing systems.', N'https://img.icons8.com/color/96/chemical-plant.png', 0, 0, @Now, 0
WHERE NOT EXISTS (SELECT 1 FROM Industries WHERE Name = N'Chemical & Petrochemical');

INSERT INTO Industries (Name, Description, Image, TopIndustry, BannerIndustry, CreatedAt, IsDeleted)
SELECT N'Construction & Building Material', N'Construction equipment and building materials.', N'https://img.icons8.com/color/96/construction.png', 1, 0, @Now, 0
WHERE NOT EXISTS (SELECT 1 FROM Industries WHERE Name = N'Construction & Building Material');

INSERT INTO Industries (Name, Description, Image, TopIndustry, BannerIndustry, CreatedAt, IsDeleted)
SELECT N'Automobiles', N'Vehicles, auto parts, and automotive solutions.', N'https://img.icons8.com/color/96/car.png', 1, 1, @Now, 0
WHERE NOT EXISTS (SELECT 1 FROM Industries WHERE Name = N'Automobiles');

INSERT INTO Industries (Name, Description, Image, TopIndustry, BannerIndustry, CreatedAt, IsDeleted)
SELECT N'Machine & Equipment', N'Industrial machines and automation equipment.', N'https://img.icons8.com/color/96/factory.png', 1, 0, @Now, 0
WHERE NOT EXISTS (SELECT 1 FROM Industries WHERE Name = N'Machine & Equipment');

INSERT INTO Industries (Name, Description, Image, TopIndustry, BannerIndustry, CreatedAt, IsDeleted)
SELECT N'Energy & Power', N'Power generation and renewable energy systems.', N'https://img.icons8.com/color/96/solar-panel.png', 1, 1, @Now, 0
WHERE NOT EXISTS (SELECT 1 FROM Industries WHERE Name = N'Energy & Power');

INSERT INTO Industries (Name, Description, Image, TopIndustry, BannerIndustry, CreatedAt, IsDeleted)
SELECT N'Heating & Thermal System', N'Boilers, furnaces, and heating systems.', N'https://img.icons8.com/color/96/heating-room.png', 0, 0, @Now, 0
WHERE NOT EXISTS (SELECT 1 FROM Industries WHERE Name = N'Heating & Thermal System');

INSERT INTO Industries (Name, Description, Image, TopIndustry, BannerIndustry, CreatedAt, IsDeleted)
SELECT N'Environmental & Geotechnical', N'Environmental protection and geotechnical services.', N'https://img.icons8.com/color/96/earth-planet.png', 0, 0, @Now, 0
WHERE NOT EXISTS (SELECT 1 FROM Industries WHERE Name = N'Environmental & Geotechnical');

INSERT INTO Industries (Name, Description, Image, TopIndustry, BannerIndustry, CreatedAt, IsDeleted)
SELECT N'Technology & Smart Industries', N'AI, IoT, automation, and smart technologies.', N'https://img.icons8.com/color/96/artificial-intelligence.png', 1, 1, @Now, 0
WHERE NOT EXISTS (SELECT 1 FROM Industries WHERE Name = N'Technology & Smart Industries');

INSERT INTO Industries (Name, Description, Image, TopIndustry, BannerIndustry, CreatedAt, IsDeleted)
SELECT N'Education', N'Learning institutions and education services.', N'https://img.icons8.com/color/96/school.png', 0, 0, @Now, 0
WHERE NOT EXISTS (SELECT 1 FROM Industries WHERE Name = N'Education');

INSERT INTO Industries (Name, Description, Image, TopIndustry, BannerIndustry, CreatedAt, IsDeleted)
SELECT N'Real Estate', N'Residential and commercial real estate services.', N'https://img.icons8.com/color/96/city-buildings.png', 0, 0, @Now, 0
WHERE NOT EXISTS (SELECT 1 FROM Industries WHERE Name = N'Real Estate');

INSERT INTO Industries (Name, Description, Image, TopIndustry, BannerIndustry, CreatedAt, IsDeleted)
SELECT N'Travel & Tourism', N'Travel agencies and tourism services.', N'https://img.icons8.com/color/96/beach.png', 0, 1, @Now, 0
WHERE NOT EXISTS (SELECT 1 FROM Industries WHERE Name = N'Travel & Tourism');

INSERT INTO Industries (Name, Description, Image, TopIndustry, BannerIndustry, CreatedAt, IsDeleted)
SELECT N'Medical & Health', N'Hospitals, diagnostics, and healthcare services.', N'https://img.icons8.com/color/96/hospital.png', 1, 1, @Now, 0
WHERE NOT EXISTS (SELECT 1 FROM Industries WHERE Name = N'Medical & Health');

INSERT INTO Industries (Name, Description, Image, TopIndustry, BannerIndustry, CreatedAt, IsDeleted)
SELECT N'Logistic, Trade & Export', N'Logistics, supply chain, and export services.', N'https://img.icons8.com/color/96/delivery.png', 0, 0, @Now, 0
WHERE NOT EXISTS (SELECT 1 FROM Industries WHERE Name = N'Logistic, Trade & Export');

INSERT INTO Industries (Name, Description, Image, TopIndustry, BannerIndustry, CreatedAt, IsDeleted)
SELECT N'Manufacturing & Engineering', N'Production, manufacturing, and industrial engineering.', N'https://img.icons8.com/color/96/assembly-line.png', 1, 0, @Now, 0
WHERE NOT EXISTS (SELECT 1 FROM Industries WHERE Name = N'Manufacturing & Engineering');

PRINT 'Industries import finished (skipped rows that already existed by Name).';
