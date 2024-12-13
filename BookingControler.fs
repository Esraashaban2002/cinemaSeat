module BookingControler

open System
open MySql.Data.MySqlClient
open System.Windows.Forms
open System.Drawing

let bookingTicket (conn: MySqlConnection) (customerName: string) (seatIds: int list) (showTime: string) =
    try
        conn.Open()

        // Start a transaction
        use transaction = conn.BeginTransaction()

        // Check seat availability
        let checkSeatQuery = "SELECT Id, Status FROM Seats WHERE Id = @SeatId"
        let checkAvailability seatId =
            use cmd = new MySqlCommand(checkSeatQuery, conn, transaction)
            cmd.Parameters.AddWithValue("@SeatId", seatId) |> ignore
            use reader = cmd.ExecuteReader()
            if reader.Read() then
                let status = reader.["Status"].ToString()
                reader.Close()
                if status = "Available" then true else false
            else
                reader.Close()
                false

        let availableSeats = seatIds |> List.filter checkAvailability

        if availableSeats.Length <> seatIds.Length then
            raise (Exception("Some seats are not available."))

        // Insert booking into the Bookings table
        let bookingQuery = "INSERT INTO Bookings (SeatId, ShowTime, CustomerName) VALUES (@SeatId, @ShowTime, @CustomerName)"
        let insertBooking seatId =
            use cmd = new MySqlCommand(bookingQuery, conn, transaction)
            cmd.Parameters.AddWithValue("@SeatId", seatId) |> ignore
            cmd.Parameters.AddWithValue("@ShowTime", showTime) |> ignore
            cmd.Parameters.AddWithValue("@CustomerName", customerName) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            int cmd.LastInsertedId

        let bookingIds = availableSeats |> List.map insertBooking

        // Insert tickets into the Tickets table
        let ticketQuery = "INSERT INTO Tickets (TicketId, BookingId, SeatId, ShowTime, CustomerName) VALUES (@TicketId, @BookingId, @SeatId, @ShowTime, @CustomerName)"
        let generateTicketId bookingId seatId = sprintf "TICKET-%d-%d-%d" bookingId seatId (DateTime.UtcNow.Ticks)

        availableSeats |> List.iter2 (fun seatId bookingId ->
            use cmd = new MySqlCommand(ticketQuery, conn, transaction)
            let ticketId = generateTicketId bookingId seatId
            cmd.Parameters.AddWithValue("@TicketId", ticketId) |> ignore
            cmd.Parameters.AddWithValue("@BookingId", bookingId) |> ignore
            cmd.Parameters.AddWithValue("@SeatId", seatId) |> ignore
            cmd.Parameters.AddWithValue("@ShowTime", showTime) |> ignore
            cmd.Parameters.AddWithValue("@CustomerName", customerName) |> ignore
            cmd.ExecuteNonQuery() |> ignore
        ) bookingIds

        // Mark seats as reserved
        let updateSeatQuery = "UPDATE Seats SET Status = 'Reserved' WHERE Id = @SeatId"
        availableSeats |> List.iter (fun seatId ->
            use cmd = new MySqlCommand(updateSeatQuery, conn, transaction)
            cmd.Parameters.AddWithValue("@SeatId", seatId) |> ignore
            cmd.ExecuteNonQuery() |> ignore
        )

        // Commit the transaction
        transaction.Commit()
        printfn "Booking completed successfully!"

    with ex ->
        printfn "An error occurred: %s" ex.Message
        conn.Rollback()

    finally
        conn.Close()

let bookingTicket  (conn: MySqlConnection) (nameTextBox: TextBox) (statusLabel: Label) = []