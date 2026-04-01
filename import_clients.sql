-- =============================================
-- Import Clients from JSON Export
-- Clears old data and imports 9 new clients
-- =============================================

BEGIN TRANSACTION;

-- Step 1: Add POP ID service if not exists
IF NOT EXISTS (SELECT 1 FROM Services WHERE ServiceType = 'pop-id')
BEGIN
    INSERT INTO Services (ServiceName, ServiceType, Category, IsActive, SortOrder, CreatedAt, IsDeleted)
    VALUES ('POP ID', 'pop-id', NULL, 1, 11, GETUTCDATE(), 0)
END

-- Step 2: Widen Phone column to fit all data
IF EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Clients' AND COLUMN_NAME = 'Phone' AND CHARACTER_MAXIMUM_LENGTH < 100
)
BEGIN
    ALTER TABLE Clients ALTER COLUMN Phone NVARCHAR(100)
END

-- Step 3: Delete all client-related data (FK order)
DELETE FROM ApiKeys WHERE ClientId IS NOT NULL
DELETE FROM ClientAdwordsDetails
DELETE FROM ClientEmailServices
DELETE FROM ClientProducts
DELETE FROM ClientSeoDetails
DELETE FROM ClientServices
DELETE FROM Enquiries
DELETE FROM Events WHERE ClientId IS NOT NULL
DELETE FROM OwnerClients
DELETE FROM SalesManagerClients
DELETE FROM SalesPersonClients
DELETE FROM Tickets WHERE ClientId IS NOT NULL
DELETE FROM Transactions
DELETE FROM UserClients WHERE ClientId IS NOT NULL
DELETE FROM Webhooks WHERE ClientId IS NOT NULL
DELETE FROM Clients

-- Step 4: Insert 9 clients
INSERT INTO Clients (
    CustomerNo, FormDate, AmountWithoutGst, GstPercentage, GstAmount, TotalPackage,
    CompanyName, ContactPerson, Designation, Address, Phone, Email, DomainName, GstNo,
    SpecificGuidelines, IsPremium, Status, CreatedAt, UpdatedAt, IsDeleted,
    UseSameEmailForEnquiries, WhatsAppSameAsMobile, UseWhatsAppService
)
VALUES
-- 1. SPARK SYSTEMS
(
    'ORD-20260211-001', '2026-01-22', 75000, 18, 13500, 88500,
    'SPARK SYSTEMS', 'Nilesh Gaikwad', NULL,
    '308, Bhawani Peth, Timbar Market Road, Pune - 411042',
    '9699799144', 'info@spark-group.in', 'www.spark-group.in', NULL,
    'Need to google promotion in only Pune city. Special note: The client has already paid 15,000 of the 75,000 package amount; the remaining sum is the outstanding balance. He will pay us the whole sum if we respond well to him and there is a local inquiry; if not, we have already decided that this package will cost 40,000.',
    0, 'Approved', '2026-02-11T11:03:37', NULL, 0, 1, 1, 0
),
-- 2. Pooja Lab Equipments
(
    'ORD-20260227-001', '2026-02-27', 45000, 18, 8100, 53100,
    'Pooja Lab Equipments', 'Karthik K Nair', 'Partner',
    '22, Gr. Floor, Bhandup Indl. Estate, Pannalal Compound, LBS Marg, Bhandup (W), Mumbai-400078',
    '022-67987938, 022-25968723', 'poojalabequipments@yahoo.in', 'https://www.poojalabequips.com/', NULL,
    'All India Promotion',
    1, 'Approved', '2026-02-27T17:08:08', NULL, 0, 1, 1, 0
),
-- 3. S Napkin Bouquet
(
    'ORD-20260227-002', '2026-02-27', 32000, 18, 5760, 37760,
    'S Napkin Bouquet', 'Mansi Pol', 'Director',
    'Janani All In One Store, Sanskruti Apt, Plot No 13, Sec- 34, ICICI Bank Lane, behind pratik garden Kamothe, navi mumbai, Pin - 410209',
    '+91 98678 30335', 'info@napkinbouquet.com', 'https://www.napkinbouquet.com/', NULL,
    NULL,
    1, 'Approved', '2026-02-27T17:12:16', NULL, 0, 1, 1, 0
),
-- 4. RECO Transformers
(
    'ORD-20260227-003', '2026-02-27', 50000, 18, 9000, 59000,
    'RECO Transformers Pvt.Ltd', 'Nitin Prabhu', 'Partner',
    'Hanuman Industrial Est, 18, GD Ambekar Marg, Sahakar Nagar, Wadala, Mumbai, Maharashtra 400031',
    '+91 9324748199 | +91 8879755871', 'rkv@reco-transformers.com', 'https://www.reco-transformers.com/', NULL,
    NULL,
    1, 'Approved', '2026-02-27T17:16:23', NULL, 0, 1, 1, 0
),
-- 5. Mercantile Electric Corporation
(
    'ORD-20260227-004', '2026-02-27', 40000, 18, 7200, 47200,
    'Mercantile Electric Corporation', 'Mr. Dhiren Somaiya', 'Director',
    '154, Kantilal Sharma Marg, Lohar Chawl, Mumbai - 400002',
    '022-22089295 | 022-22062688', NULL, 'https://www.mercantileelectric.in/', NULL,
    NULL,
    0, 'Approved', '2026-02-27T17:19:12', NULL, 0, 1, 1, 0
),
-- 6. ERAM SOLAR POWER SOLUTIONS
(
    'ORD-20260227-005', '2026-02-27', 50000, 18, 9000, 59000,
    'ERAM SOLAR POWER SOLUTIONS', 'Khan Sir', 'Director',
    'Globe Business Park, Plot No-30 Office No. 202, 2nd Floor, Kalyan - Badlapur Rd, Ambernath, Maharashtra 421501',
    '+91 77740 11993', 'erampowerprojects@gmail.com', 'https://www.eramsolar.net/', NULL,
    NULL,
    1, 'Approved', '2026-02-27T17:25:54', NULL, 0, 1, 1, 0
),
-- 7. ERAM POWER PROJECTS
(
    'ORD-20260227-006', '2026-02-27', 50000, 18, 9000, 59000,
    'ERAM POWER PROJECTS', 'Khan Sir', 'Director',
    'Globe Business park, Plot no-30, Office No. 202, 2nd Floor, Kalyan Badlapur Rd, Ambarnath (w) Maharashtra - 421501 . INDIA.',
    '+91-8600604615 / +91-7774011993', 'info@eramgenset.net', 'https://www.eramgenset.net/', NULL,
    NULL,
    1, 'Approved', '2026-02-27T17:29:23', NULL, 0, 1, 1, 0
),
-- 8. Eram Engineering Services
(
    'ORD-20260227-007', '2026-02-27', 50000, 18, 9000, 59000,
    'Eram Engineering Services', 'Khan Sir', 'Director',
    'Plot No.E-18 Additional Industrial Area, Anand Nagar, MIDC, Ambernath (E) - 421506, Dist- Thane',
    '0251-3508285 , +91 7774011993', 'info@eramsoundproofenclosures.com', 'https://www.eramsoundproofenclosures.com/', NULL,
    NULL,
    1, 'Approved', '2026-02-27T17:32:13', NULL, 0, 1, 1, 0
),
-- 9. INDMK Engineering & Construction
(
    'ORD-20260227-008', '2026-02-27', 40000, 18, 7200, 47200,
    'INDMK Engineering & Construction Pvt. Ltd.', 'Khan Sir', 'Director',
    'Plot No.E-18, Additional Indl. Area, Anand Nagar, MIDC, Ambarnath (E), Dist. Thane - 421506, Maharashtra, India.',
    '+91-8600604615 / +91-7774011993 +91-7774011999', 'info@epccontractors.in', 'https://www.epccontractors.in/', NULL,
    NULL,
    1, 'Approved', '2026-02-27T17:34:43', NULL, 0, 1, 1, 0
)

-- Step 5: Insert ClientServices (look up ClientId by CustomerNo)
INSERT INTO ClientServices (ClientId, ServiceId, CreatedAt, UpdatedAt, IsDeleted)
SELECT c.Id, s.Id, GETUTCDATE(), NULL, 0
FROM (VALUES
    -- ORD-20260211-001: domain-hosting, website-design-development, seo
    ('ORD-20260211-001', 'domain-hosting'),
    ('ORD-20260211-001', 'website-design-development'),
    ('ORD-20260211-001', 'seo'),
    -- ORD-20260227-001: website-design-development, seo, domain-hosting, pop-id
    ('ORD-20260227-001', 'website-design-development'),
    ('ORD-20260227-001', 'seo'),
    ('ORD-20260227-001', 'domain-hosting'),
    ('ORD-20260227-001', 'pop-id'),
    -- ORD-20260227-002: domain-hosting, website-design-development, seo, pop-id
    ('ORD-20260227-002', 'domain-hosting'),
    ('ORD-20260227-002', 'website-design-development'),
    ('ORD-20260227-002', 'seo'),
    ('ORD-20260227-002', 'pop-id'),
    -- ORD-20260227-003: domain-hosting, website-design-development, seo
    ('ORD-20260227-003', 'domain-hosting'),
    ('ORD-20260227-003', 'website-design-development'),
    ('ORD-20260227-003', 'seo'),
    -- ORD-20260227-004: domain-hosting, website-design-development, seo
    ('ORD-20260227-004', 'domain-hosting'),
    ('ORD-20260227-004', 'website-design-development'),
    ('ORD-20260227-004', 'seo'),
    -- ORD-20260227-005: domain-hosting, website-design-development, seo, google-ads
    ('ORD-20260227-005', 'domain-hosting'),
    ('ORD-20260227-005', 'website-design-development'),
    ('ORD-20260227-005', 'seo'),
    ('ORD-20260227-005', 'google-ads'),
    -- ORD-20260227-006: domain-hosting, website-design-development, seo, google-ads, pop-id
    ('ORD-20260227-006', 'domain-hosting'),
    ('ORD-20260227-006', 'website-design-development'),
    ('ORD-20260227-006', 'seo'),
    ('ORD-20260227-006', 'google-ads'),
    ('ORD-20260227-006', 'pop-id'),
    -- ORD-20260227-007: domain-hosting, website-design-development, seo, google-ads, pop-id
    ('ORD-20260227-007', 'domain-hosting'),
    ('ORD-20260227-007', 'website-design-development'),
    ('ORD-20260227-007', 'seo'),
    ('ORD-20260227-007', 'google-ads'),
    ('ORD-20260227-007', 'pop-id'),
    -- ORD-20260227-008: domain-hosting, website-design-development, seo, pop-id
    ('ORD-20260227-008', 'domain-hosting'),
    ('ORD-20260227-008', 'website-design-development'),
    ('ORD-20260227-008', 'seo'),
    ('ORD-20260227-008', 'pop-id')
) AS map(CustomerNo, ServiceType)
JOIN Clients c ON c.CustomerNo = map.CustomerNo
JOIN Services s ON s.ServiceType = map.ServiceType AND s.IsDeleted = 0

-- Step 6: Insert ClientSeoDetails (KeywordRange, Location, KeywordsList)
INSERT INTO ClientSeoDetails (ClientId, KeywordRange, Location, KeywordsList, CreatedAt, UpdatedAt, IsDeleted)
SELECT c.Id, map.KeywordRange, map.Location, map.KeywordsList, GETUTCDATE(), NULL, 0
FROM (VALUES
    ('ORD-20260211-001', 'upto-25', 'Pune',
     'Tyrolit Dealers in Pune, IFSC Dealer in Pune, Bosch Dealer in Pune'),
    ('ORD-20260227-001', 'upto-25', 'State-wise, India',
     'Chromatography Refrigerator, Pharmaceutical / Medical Refrigerator, B.O.D. Incubator, Ice Flaker, Illuminated Plant Growth Chamber, Stability Chamber, Freezer cum Refrigerator, Ultra Low Temperature Deep Freezer, Low Temperature Deep Freezer, Deep Freezer Vertical & Horizontal, Orbital Shaking Incubator, Cryostat Bath, Vertical Bacteriological Incubator'),
    ('ORD-20260227-002', 'upto-25', NULL,
     'napkin bouquet'),
    ('ORD-20260227-003', 'upto-25', NULL,
     'Low Voltage Transformers, Low Voltage Current Transformers, Voltage Transformers, Control Transformers, Current Transformers, Resin Cast Current Transformers, Resin Cast Ring Type CT, Resin Cast Window Type CT, Resin Cast Wound Primary CT, Resin Cast Ring Main Unit CT, ABS Encapsulated Current Transformers'),
    ('ORD-20260227-004', 'upto-25', 'State-wise, India',
     'Cable Ties, Zip Ties, Cable Wraps, Nylon Cable Ties, Color Cable Ties, Tag Ties, Push Mount Cable Ties, Self Adhesive Tie Mount Sticker, Stainless Steel Cable Ties, Releasable Cable Ties, Hydraulic Tools, Hand Crimping Tools, Hydraulic Cable Crimper, XTRA Hydraulic Tools, XTRA Crimping Tools, XTRA Taiwan Tools, XTRA Solar Tools, XTRA Cable Cutters'),
    ('ORD-20260227-005', 'upto-25', 'State-wise, India',
     'Commercial Solar Panels, Industrial Solar Panels, Solar Design Solutions, Industrial Solar Rooftop System, Solar Panels'),
    ('ORD-20260227-006', 'upto-25', 'State-wise, India',
     'DG Set, DG Sets, DG Generator, DG Generators, Diesel Generator, Diesel Generators, Diesel Generating Set, Diesel Generating Sets, Domestic Diesel Generator'),
    ('ORD-20260227-007', 'upto-25', 'State-wise, India',
     'Commercial Acoustic Enclosures, Room Acoustic, Acoustic Louver Doors, Acoustic Hanging Baffles, Acoustic Enclosures, Chillers Noise Control'),
    ('ORD-20260227-008', 'upto-25', 'State-wise',
     'EPC Contractor, Engineering Contractor, Construction Contractor, INDMK offers solutions to clients for Project Management')
) AS map(CustomerNo, KeywordRange, Location, KeywordsList)
JOIN Clients c ON c.CustomerNo = map.CustomerNo

-- Step 7: Insert ClientAdwordsDetails (NumberOfKeywords, Period, Location, KeywordsList, SpecialGuidelines)
INSERT INTO ClientAdwordsDetails (ClientId, NumberOfKeywords, Period, Location, KeywordsList, SpecialGuidelines, CreatedAt, UpdatedAt, IsDeleted)
SELECT c.Id, map.NumberOfKeywords, map.Period, map.Location, map.KeywordsList, map.SpecialGuidelines, GETUTCDATE(), NULL, 0
FROM (VALUES
    ('ORD-20260227-005', '5', 'Monthly', 'India',
     'Commercial Solar Panels, Industrial Solar Panels, Solar Design Solutions, Industrial Solar Rooftop System, Solar Panels', NULL),
    ('ORD-20260227-006', '6', 'Monthly', 'India',
     'DG Set, DG Sets, DG Generator, DG Generators, Diesel Generator, Diesel Generators, Diesel Generating Set, Diesel Generating Sets', NULL),
    ('ORD-20260227-007', '5', 'Monthly', 'India',
     'Sound Proof Enclosures, Acoustic Enclosures, Sound Barrier Wall, Room Acoustic, Acoustic Louvers', NULL)
) AS map(CustomerNo, NumberOfKeywords, Period, Location, KeywordsList, SpecialGuidelines)
JOIN Clients c ON c.CustomerNo = map.CustomerNo

COMMIT TRANSACTION;

-- Verify
SELECT COUNT(*) AS ClientsImported FROM Clients WHERE IsDeleted = 0
SELECT COUNT(*) AS ServicesLinked FROM ClientServices WHERE IsDeleted = 0
SELECT COUNT(*) AS SeoDetails FROM ClientSeoDetails WHERE IsDeleted = 0
SELECT COUNT(*) AS AdwordsDetails FROM ClientAdwordsDetails WHERE IsDeleted = 0
