CREATE TABLE Child
( 
    ID_child             SERIAL PRIMARY KEY,
    Full_name            VARCHAR(255) NOT NULL,
    Birth_date           DATE
);

ALTER TABLE Child
    ADD CONSTRAINT CHK_Child_BirthDate 
    CHECK (Birth_date <= CURRENT_DATE AND 
           Birth_date >= (CURRENT_DATE - INTERVAL '18 years'));

CREATE TABLE Country
( 
    ID_country           SERIAL PRIMARY KEY,
    Name                 VARCHAR(255) NOT NULL
);

CREATE TABLE Disease
( 
    ID_disease           SERIAL PRIMARY KEY,
    Name                 VARCHAR(255) NOT NULL,
    Definition           TEXT
);

CREATE TABLE Fundraising
( 
    ID_fundraising       SERIAL PRIMARY KEY,
    ID_disease           INTEGER NOT NULL,
    ID_child             INTEGER NOT NULL,
    Sum                  DECIMAL(10,2) NOT NULL,
    Date_begin           DATE,
    Flag_end             BOOLEAN NOT NULL DEFAULT FALSE
);

ALTER TABLE Fundraising
    ADD CONSTRAINT CHK_Fundraising_Sum CHECK (Sum > 0);

ALTER TABLE Fundraising
    ADD CONSTRAINT CHK_Fundraising_DateBegin CHECK (Date_begin <= CURRENT_DATE);

CREATE TABLE Donation
( 
    ID_donation          SERIAL,
    ID_fundraising       INTEGER NOT NULL,
    ID_country           INTEGER,
    Sum_donation         DECIMAL(10,2) NOT NULL,
    Date                 DATE,
    Type                 VARCHAR(50) NOT NULL,
    PRIMARY KEY (ID_donation, ID_fundraising)
);

ALTER TABLE Donation
    ADD CONSTRAINT CHK_Donation_Sum CHECK (Sum_donation > 0);

ALTER TABLE Donation
    ADD CONSTRAINT CHK_Donation_Date CHECK (Date <= CURRENT_DATE);

ALTER TABLE Donation
    ADD CONSTRAINT CHK_Donation_Type 
    CHECK (Type IN ('от физического лица', 'от юридического лица', 'неизвестно'));

-- Ссылочная целостность
ALTER TABLE Fundraising
    ADD CONSTRAINT FK_Fundraising_Child 
    FOREIGN KEY (ID_child) REFERENCES Child(ID_child)
    ON UPDATE CASCADE
    ON DELETE NO ACTION;

ALTER TABLE Fundraising
    ADD CONSTRAINT FK_Fundraising_Disease 
    FOREIGN KEY (ID_disease) REFERENCES Disease(ID_disease)
    ON UPDATE CASCADE
    ON DELETE NO ACTION;

ALTER TABLE Donation
    ADD CONSTRAINT FK_Donation_Fundraising 
    FOREIGN KEY (ID_fundraising) REFERENCES Fundraising(ID_fundraising)
    ON UPDATE CASCADE
    ON DELETE CASCADE;

ALTER TABLE Donation
    ADD CONSTRAINT FK_Donation_Country 
    FOREIGN KEY (ID_country) REFERENCES Country(ID_country)
    ON UPDATE CASCADE
    ON DELETE NO ACTION;