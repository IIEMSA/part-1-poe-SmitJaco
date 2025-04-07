USE master
IF EXISTS (select * from sys.databases WHERE name = 'EventEaseDB')
Drop database EventEaseDB
CREATE database EventEaseDB

USE EventEaseDB
-- create Venue
CREATE TABLE Venue (
    VenueId INT PRIMARY KEY IDENTITY(1,1),
    VenueName NVARCHAR(100) NOT NULL,
    Location NVARCHAR(100) NOT NULL,
    Capacity INT NOT NULL,
    ImageURL NVARCHAR(255)
);
-- create Event
CREATE TABLE Event (
    EventId INT PRIMARY KEY IDENTITY(1,1),
    EventName NVARCHAR(100) NOT NULL,
    EventDate DATE NOT NULL,
    Description NVARCHAR(500),
    VenueId INT,
    FOREIGN KEY (VenueId) REFERENCES Venue(VenueId)
);
-- create Boooking
CREATE TABLE Booking (
    BookingId INT PRIMARY KEY IDENTITY(1,1),
    EventId INT NOT NULL,
    VenueId INT NOT NULL,
    BookingDate DATE NOT NULL,
    FOREIGN KEY (EventId) REFERENCES Event(EventId),
    FOREIGN KEY (VenueId) REFERENCES Venue(VenueId)
);

CREATE UNIQUE INDEX IDX_UniqueBooking ON Booking (VenueId, BookingDate);


-- Insert sample data for testing
INSERT INTO Venue (VenueName, Location, Capacity, ImageURL)
VALUES 
    ('Grand Hall', 'New York', 500, 'https://via.placeholder.com/150'),
    ('City Plaza', 'Los Angeles', 300, 'https://via.placeholder.com/150');

INSERT INTO Event (EventName, EventDate, Description, VenueId)
VALUES 
    ('Tech Conference', '2025-05-01', 'Annual tech event', 1),
    ('Music Festival', '2025-06-15', 'Summer music fest', 2);

INSERT INTO Booking (EventId, VenueId, BookingDate)
VALUES 
    (1, 1, '2025-05-01'),
    (2, 2, '2025-06-15');


-- Verify the data
SELECT * FROM Venue;
SELECT * FROM Event;
SELECT * FROM Booking;
