
--INSERT DATA
--MS SQL

-- Insert data into Studies
INSERT INTO Studies (IdStudy, Name)
VALUES (1, 'Architektura')

INSERT INTO Studies (IdStudy, Name)
VALUES (2, 'Budownictwo')

INSERT INTO Studies (IdStudy, Name)
VALUES (3, 'Informatyka')

INSERT INTO Studies (IdStudy, Name)
VALUES (4, 'Zarzadzanie')

-- Insert data into Enrollment
INSERT INTO Enrollment (IdEnrollment, Semester, IdStudy, StartDate)
VALUES (1, 6, 1, CAST('2019/09/01 15:00:00' AS DATETIME))

INSERT INTO Enrollment (IdEnrollment, Semester, IdStudy, StartDate)
VALUES (2, 6, 1, CAST('2019/09/01 15:00:00' AS DATETIME))

INSERT INTO Enrollment (IdEnrollment, Semester, IdStudy, StartDate)
VALUES (3, 6, 2, CAST('2019/09/01 15:00:00' AS DATETIME))

INSERT INTO Enrollment (IdEnrollment, Semester, IdStudy, StartDate)
VALUES (4, 4, 3, CAST('2019/10/01 15:00:00' AS DATETIME))

INSERT INTO Enrollment (IdEnrollment, Semester, IdStudy, StartDate)
VALUES (5, 4, 4, CAST('2019/10/01 15:00:00' AS DATETIME))

INSERT INTO Enrollment (IdEnrollment, Semester, IdStudy, StartDate)
VALUES (6, 4, 4, CAST('2019/10/01 15:00:00' AS DATETIME))





-- Insert data into Student
INSERT INTO Student (IndexNumber, FirstName, LastName, BirthDate, IdEnrollment, HashedPassword, Salt, Refreshtoken)
VALUES('s202001', 'Pam', 'Beesly', CAST('1990/08/01' AS DATE), 1, '', '', '')

INSERT INTO Student (IndexNumber, FirstName, LastName, BirthDate, IdEnrollment, HashedPassword, Salt, Refreshtoken)
VALUES('s202002', 'Ryan', 'Howard', CAST('1990/03/02' AS DATE), 2, '', '', '')

INSERT INTO Student (IndexNumber, FirstName, LastName, BirthDate, IdEnrollment, HashedPassword, Salt, Refreshtoken)
VALUES('s202003', 'Jim', 'Halpert', CAST('1990/08/25' AS DATE), 3,  '', '', '')

INSERT INTO Student (IndexNumber, FirstName, LastName, BirthDate, IdEnrollment, HashedPassword, Salt, Refreshtoken)
VALUES('s202004', 'Oscar', 'Martinez', CAST('1990/04/18' AS DATE), 4,  '', '', '')

INSERT INTO Student (IndexNumber, FirstName, LastName, BirthDate, IdEnrollment, HashedPassword, Salt, Refreshtoken)
VALUES('s202005', 'Phyllis', 'Lapin', CAST('1990/03/05' AS DATE), 5,  '', '', '')

INSERT INTO Student (IndexNumber, FirstName, LastName, BirthDate, IdEnrollment, HashedPassword, Salt, Refreshtoken)
VALUES('s202006', 'Angela', 'Martin', CAST('1990/01/28' AS DATE), 6,  '', '', '')