module BookingControler

open System
open MySql.Data.MySqlClient
open System.Windows.Forms
open System.Drawing

let bookingTicket (conn: MySqlConnection) (nameTextBox: TextBox) (seatTextBox: TextBox) (showtimeTextBox: TextBox) (statusLabel: Label) =
    try

        // Start a transaction
        use transaction = conn.BeginTransaction()

       // Split the input and validate the format
        let seatParts = seatTextBox.Text.Split('-')
        if seatParts.Length <> 2 || not (seatParts.[0].StartsWith("R")) || not (seatParts.[1].StartsWith("C")) then
            raise (Exception("Invalid seat format. Use Row-Column format (e.g., R1-C1)."))

        // Extract and convert row and column numbers
        let rowSeat = 
            if seatParts.[0].Length > 1 then 
                int(seatParts.[0].Substring(1))
            else 
                raise (Exception("Invalid row format. Use Row-Column format (e.g., R1-C1)."))

        let columnSeat = 
            if seatParts.[1].Length > 1 then 
                int(seatParts.[1].Substring(1))
            else 
                raise (Exception("Invalid column format. Use Row-Column format (e.g., R1-C1)."))

        // Check seat availability
        let checkSeatQuery = "SELECT Id, Status FROM Seats WHERE Row_seat = @RowSeat AND Column_seat = @ColumnSeat"
        let checkAvailability rowSeat columnSeat =
            use cmd = new MySqlCommand(checkSeatQuery, conn, transaction)
            cmd.Parameters.AddWithValue("@RowSeat", rowSeat) |> ignore
            cmd.Parameters.AddWithValue("@ColumnSeat", columnSeat) |> ignore
            use reader = cmd.ExecuteReader()
            if reader.Read() then
                let seatId = reader.GetInt32(0)
                let status = reader.GetString(1)
                reader.Close()
                if status = "Available" then 
                    Some seatId 
                else None
            else
                reader.Close()
                None

        match checkAvailability rowSeat columnSeat with
        | None -> raise (Exception("Selected seat is not available."))
        | Some seatId ->
            // Insert booking into the Bookings table
            let bookingQuery = "INSERT INTO Bookings (SeatId, ShowTime, CustomerName) VALUES (@SeatId, @ShowTime, @CustomerName)"
            use bookingCmd = new MySqlCommand(bookingQuery, conn, transaction)
            bookingCmd.Parameters.AddWithValue("@SeatId", seatId) |> ignore
            bookingCmd.Parameters.AddWithValue("@ShowTime", showtimeTextBox.Text) |> ignore
            bookingCmd.Parameters.AddWithValue("@CustomerName", nameTextBox.Text) |> ignore
            bookingCmd.ExecuteNonQuery() |> ignore
            let bookingId = int bookingCmd.LastInsertedId

            // Generate and insert ticket into the Tickets table
            let ticketId = sprintf "TICKET-%d-%d" bookingId DateTime.UtcNow.Ticks
            let ticketQuery = "INSERT INTO Tickets (TicketId, BookingId, SeatId, ShowTime, CustomerName) VALUES (@TicketId, @BookingId, @SeatId, @ShowTime, @CustomerName)"
            use ticketCmd = new MySqlCommand(ticketQuery, conn, transaction)
            ticketCmd.Parameters.AddWithValue("@TicketId", ticketId) |> ignore
            ticketCmd.Parameters.AddWithValue("@BookingId", bookingId) |> ignore
            ticketCmd.Parameters.AddWithValue("@SeatId", seatId) |> ignore
            ticketCmd.Parameters.AddWithValue("@ShowTime", showtimeTextBox.Text) |> ignore
            ticketCmd.Parameters.AddWithValue("@CustomerName", nameTextBox.Text) |> ignore
            ticketCmd.ExecuteNonQuery() |> ignore

            // Update seat status
            let updateSeatQuery = "UPDATE Seats SET Status = 'Reserved' WHERE Id = @SeatId"
            use updateSeatCmd = new MySqlCommand(updateSeatQuery, conn, transaction)
            updateSeatCmd.Parameters.AddWithValue("@SeatId", seatId) |> ignore
            updateSeatCmd.ExecuteNonQuery() |> ignore

            // Commit transaction
            transaction.Commit()
            statusLabel.Text <- sprintf "Booking successful! Ticket ID: %s" ticketId
            statusLabel.ForeColor <- Color.Green

    with ex ->
        statusLabel.Text <- sprintf "Error: %s" ex.Message
        statusLabel.ForeColor <- Color.Red
