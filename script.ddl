#@(#) script.ddl

DROP TABLE IF EXISTS Renka;
DROP TABLE IF EXISTS Yra;
DROP TABLE IF EXISTS Progresai;
DROP TABLE IF EXISTS Pastebejimai;
DROP TABLE IF EXISTS Parase;
DROP TABLE IF EXISTS Ivertinimai;
DROP TABLE IF EXISTS Vartotojai;
DROP TABLE IF EXISTS Tipai;
DROP TABLE IF EXISTS Knygos;
DROP TABLE IF EXISTS Autoriai;
CREATE TABLE Autoriai
(
	vardas varchar (50) NOT NULL,
	pavarde varchar (100) NOT NULL,
	salis varchar (50) NULL,
	id_Autorius integer NOT NULL,
	PRIMARY KEY(id_Autorius)
);

CREATE TABLE Knygos
(
	pavadinm varchar (50) NOT NULL,
	leidimo metai integer NULL,
	psl integer NULL,
	id_Knyga integer NOT NULL,
	PRIMARY KEY(id_Knyga)
);

CREATE TABLE Tipai
(
	zanras varchar (50) NULL,
	id_Tipas integer NOT NULL,
	PRIMARY KEY(id_Tipas)
);

CREATE TABLE Vartotojai
(
	vardas varchar (50) NOT NULL,
	pav varchar (50) NOT NULL,
	salis varchar (50) NULL,
	gmail varchar (50) NOT NULL,
	slaptazodis varchar (50) NOT NULL,
	id_Vartotojas integer NOT NULL,
	PRIMARY KEY(id_Vartotojas)
);

CREATE TABLE Ivertinimai
(
	komentaras varchar (255),
	Balai char (1),
	id_Ivertinimas integer NOT NULL,
	fk_Vartotojasid_Vartotojas integer,
	fk_Knygaid_Knyga integer,
	CHECK(Balai in ('1', '2', '3', '4', '5')),
	PRIMARY KEY(id_Ivertinimas),
	CONSTRAINT Palieka FOREIGN KEY(fk_Vartotojasid_Vartotojas) REFERENCES Vartotojas (id_Vartotojas),
	CONSTRAINT Ivertinta FOREIGN KEY(fk_Knygaid_Knyga) REFERENCES Knyga (id_Knyga)
);

CREATE TABLE Parase
(
	fk_Knygaid_Knyga integer,
	fk_Autoriusid_Autorius integer,
	PRIMARY KEY(fk_Knygaid_Knyga, fk_Autoriusid_Autorius),
	CONSTRAINT Parase FOREIGN KEY(fk_Knygaid_Knyga) REFERENCES Knyga (id_Knyga)
);

CREATE TABLE Pastebejimai
(
	citata varchar (255) NULL,
	id_Pastebejimas integer NOT NULL,
	fk_Vartotojasid_Vartotojas integer NOT NULL,
	PRIMARY KEY(id_Pastebejimas),
	CONSTRAINT Raso FOREIGN KEY(fk_Vartotojasid_Vartotojas) REFERENCES Vartotojas (id_Vartotojas)
);

CREATE TABLE Progresai
(
	dabart psl integer NOT NULL,
	skyrius varchar (50) NULL,
	id_Progresas integer NOT NULL,
	fk_Knygaid_Knyga integer,
	fk_Vartotojasid_Vartotojas integer,
	PRIMARY KEY(id_Progresas),
	CONSTRAINT Fiksuojamas FOREIGN KEY(fk_Knygaid_Knyga) REFERENCES Knyga (id_Knyga),
	CONSTRAINT Zymi FOREIGN KEY(fk_Vartotojasid_Vartotojas) REFERENCES Vartotojas (id_Vartotojas)
);

CREATE TABLE Yra
(
	fk_Knygaid_Knyga integer,
	fk_Tipasid_Tipas integer,
	PRIMARY KEY(fk_Knygaid_Knyga, fk_Tipasid_Tipas),
	CONSTRAINT Yra FOREIGN KEY(fk_Knygaid_Knyga) REFERENCES Knyga (id_Knyga)
);

CREATE TABLE Renka
(
	fk_Knygaid_Knyga integer,
	fk_Pastebejimasid_Pastebejimas integer,
	PRIMARY KEY(fk_Knygaid_Knyga, fk_Pastebejimasid_Pastebejimas),
	CONSTRAINT Renka FOREIGN KEY(fk_Knygaid_Knyga) REFERENCES Knyga (id_Knyga)
);
