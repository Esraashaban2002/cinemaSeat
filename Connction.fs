module Connction

open System
open MySql.Data.MySqlClient

let connectionString = "Server=127.0.0.1; Database=cinema;User=root;Password=1234;"
let createTables () =
    let connection = new MySqlConnection(connectionString)
    connection.Open()
    
    let commandText = """
    CREATE TABLE IF NOT EXISTS Seats (
        Id INT AUTO_INCREMENT PRIMARY KEY,
        Row_seat  INT NOT NULL,
        Column_seat INT NOT NULL,
        Status VARCHAR(50) NOT NULL
    );

    CREATE UNIQUE INDEX idx_row_col ON Seats (Row_seat, Column_seat);

    CREATE TABLE IF NOT EXISTS Bookings (
        Id INT AUTO_INCREMENT PRIMARY KEY,
        SeatId INT NOT NULL,
        ShowTime VARCHAR(100) NOT NULL,
        CustomerName VARCHAR(100) NOT NULL,
        FOREIGN KEY (SeatId) REFERENCES Seats(Id)
    );

    CREATE TABLE IF NOT EXISTS Tickets (
        TicketId VARCHAR(50) PRIMARY KEY,
        BookingId INT NOT NULL,
        SeatId INT NOT NULL,
        ShowTime VARCHAR(100) NOT NULL,
        CustomerName VARCHAR(100) NOT NULL,
        FOREIGN KEY (BookingId) REFERENCES Bookings(Id),
        FOREIGN KEY (SeatId) REFERENCES Seats(Id)
    );
    """
    let command = new MySqlCommand(commandText, connection)
    command.ExecuteNonQuery()
    connection.Close()

createTables ()
