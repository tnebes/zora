USE master;

IF EXISTS (SELECT name FROM sys.databases WHERE name = 'utakmica')
BEGIN
    ALTER DATABASE utakmica SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE utakmica;
END

CREATE TABLE klub (
    sifra BIGINT IDENTITY(1,1) PRIMARY KEY,
    naziv VARCHAR(255) NOT NULL,
    osnovan DATETIME2,
    stadion VARCHAR(255),
    predsjednik VARCHAR(255),
    drzava CHAR(3),
    liga VARCHAR(255),
    CONSTRAINT UQ_Klub_Naziv UNIQUE (naziv)
);

CREATE TABLE igrac (
    sifra BIGINT IDENTITY(1,1) PRIMARY KEY,
    ime VARCHAR(255) NOT NULL,
    prezime VARCHAR(255) NOT NULL,
    datum_rodjenja DATETIME2,
    broj_dresa INT,
    klub BIGINT,
    pozicija VARCHAR(255),
    CONSTRAINT FK_Igrac_Klub FOREIGN KEY (klub) REFERENCES klub(sifra)
);

CREATE TABLE utakmica (
    sifra BIGINT IDENTITY(1,1) PRIMARY KEY,
    datum DATETIME2,
    domacin BIGINT,
    gost BIGINT,
    CONSTRAINT FK_Utakmica_Domacin FOREIGN KEY (domacin) REFERENCES klub(sifra),
    CONSTRAINT FK_Utakmica_Gost FOREIGN KEY (gost) REFERENCES klub(sifra)
);

CREATE TABLE trener (
      sifra BIGINT IDENTITY(1,1) PRIMARY KEY,
      ime VARCHAR(255) NOT NULL,
      prezime VARCHAR(255) NOT NULL,
      klub BIGINT,
      nacionalnost CHAR(3),
      iskustvo TEXT,
      CONSTRAINT FK_Trener_Klub FOREIGN KEY (klub) REFERENCES klub(sifra)
)

