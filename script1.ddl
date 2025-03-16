#@(#) script1.ddl

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
	vardas varchar (255) NOT NULL,
	pavarde varchar (255) NOT NULL,
	salis varchar (255) NULL,
	id_Autorius integer,
	PRIMARY KEY(id_Autorius)
);

CREATE TABLE Knygos
(
	pavadinm varchar (255) NOT NULL,
	leidimo metai integer NULL,
	psl integer NOT NULL,
	id_Knyga integer,
	PRIMARY KEY(id_Knyga)
);

CREATE TABLE Tipai
(
	zanras varchar (255) NOT NULL,
	id_Tipas integer,
	PRIMARY KEY(id_Tipas)
);

CREATE TABLE Vartotojai
(
	vardas varchar (255) NOT NULL,
	pav varchar (255) NOT NULL,
	salis varchar (255) NULL,
	gmail varchar (255) NOT NULL,
	id_Vartotojas integer,
	PRIMARY KEY(id_Vartotojas)
);

CREATE TABLE Ivertinimai
(
	komentaras varchar (255) NULL,
	Balai char (1) NOT NULL,
	id_Ivertinimas integer,
	fk_Vartotojasid_Vartotojas integer,
	fk_Knygaid_Knyga integer,
	CHECK(Balai in ('1', '2', '3', '4', '5')),
	PRIMARY KEY(id_Ivertinimas),
	CONSTRAINT Palieka FOREIGN KEY(fk_Vartotojasid_Vartotojas) REFERENCES Vartotojai (id_Vartotojas),
	CONSTRAINT Ivertinta FOREIGN KEY(fk_Knygaid_Knyga) REFERENCES Knygos (id_Knyga)
);

CREATE TABLE Parase
(
	fk_Knygaid_Knyga integer,
	fk_Autoriusid_Autorius integer,
	PRIMARY KEY(fk_Knygaid_Knyga, fk_Autoriusid_Autorius),
	CONSTRAINT Parase FOREIGN KEY(fk_Knygaid_Knyga) REFERENCES Knygos (id_Knyga)
);

CREATE TABLE Pastebejimai
(
	citata varchar (255) NOT NULL,
	id_Pastebejimas integer,
	fk_Vartotojasid_Vartotojas integer NOT NULL,
	PRIMARY KEY(id_Pastebejimas),
	CONSTRAINT Raso FOREIGN KEY(fk_Vartotojasid_Vartotojas) REFERENCES Vartotojai (id_Vartotojas)
);

CREATE TABLE Progresai
(
	dabart psl integer NOT NULL,
	skyrius varchar (255) NOT NULL,
	id_Progresas integer,
	fk_Vartotojasid_Vartotojas integer,
	fk_Knygaid_Knyga integer,
	PRIMARY KEY(id_Progresas),
	CONSTRAINT Zymi FOREIGN KEY(fk_Vartotojasid_Vartotojas) REFERENCES Vartotojai (id_Vartotojas),
	CONSTRAINT Fiksuojamas FOREIGN KEY(fk_Knygaid_Knyga) REFERENCES Knygos (id_Knyga)
);

CREATE TABLE Yra
(
	fk_Knygaid_Knyga integer,
	fk_Tipasid_Tipas integer,
	PRIMARY KEY(fk_Knygaid_Knyga, fk_Tipasid_Tipas),
	CONSTRAINT Yra FOREIGN KEY(fk_Knygaid_Knyga) REFERENCES Knygos (id_Knyga)
);

CREATE TABLE Renka
(
	fk_Knygaid_Knyga integer,
	fk_Pastebejimasid_Pastebejimas integer,
	PRIMARY KEY(fk_Knygaid_Knyga, fk_Pastebejimasid_Pastebejimas),
	CONSTRAINT Renka FOREIGN KEY(fk_Knygaid_Knyga) REFERENCES Knygos (id_Knyga)
);
