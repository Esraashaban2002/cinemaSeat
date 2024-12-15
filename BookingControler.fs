module BookingControler

open System
open MySql.Data.MySqlClient
open System.Windows.Forms
open System.Drawing


let checkAvailability (reservedSeats: string list) : (int * int) list =
    reservedSeats
    |> List.choose (fun seat ->
        let parts = seat.Split('-')
        if parts.Length = 2 && parts.[0].StartsWith("R") && parts.[1].StartsWith("C") then
            let rowSeat = int (parts.[0].Substring(1))  // row number
            let columnSeat = int (parts.[1].Substring(1))  // column number
            Some (rowSeat, columnSeat)  
        else
            None  
    )

// Updates the seating chart based on booking or cancellation
let updateSeatChart (conn: MySqlConnection) (seatId: int) (status: string) (statusLabel: Label) =
    try
        // Prepare the SQL query to update the seat status
        let updateSeatQuery = "UPDATE Seats SET Status = @Status WHERE Id = @SeatId"
        
        // Create a command to execute the query
        use updateSeatCmd = new MySqlCommand(updateSeatQuery, conn)
        updateSeatCmd.Parameters.AddWithValue("@SeatId", seatId) |> ignore
        updateSeatCmd.Parameters.AddWithValue("@Status", status) |> ignore
        
        // Execute the query
        updateSeatCmd.ExecuteNonQuery() |> ignore
        
        // Return success message
        statusLabel.Text <- "Seat updated successfully"
    with
    | ex -> 
        // Handle any errors that occur
        statusLabel.Text <- sprintf "Error: %s" ex.Message
        statusLabel.ForeColor <- Color.Red


// Function to reserve a seat
let reserveSeat (conn: MySqlConnection) (rowSeat: int) (columnSeat: int) (nameTextBox: TextBox) (showtimeTextBox: TextBox) (statusLabel: Label) =
    try
        // Start a transaction
        use transaction = conn.BeginTransaction()

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
            let status = "Reserved"
            updateSeatChart conn seatId status statusLabel

            // Commit transaction
            transaction.Commit()
            statusLabel.Text <- sprintf "Booking successful! Ticket ID: %s" ticketId
            statusLabel.ForeColor <- Color.Green

    with ex ->
        statusLabel.Text <- sprintf "Error: %s" ex.Message
        statusLabel.ForeColor <- Color.Red

let bookingTicket (conn: MySqlConnection) (nameTextBox: TextBox) (reservedSeats: string list) (showtimeTextBox: TextBox) (statusLabel: Label) =
    let availableSeats = checkAvailability reservedSeats
    if List.isEmpty availableSeats then
        // If no valid seats are found
        statusLabel.Text <- "Invalid seat format. Use Row-Column format (e.g., R1-C1)."
        statusLabel.ForeColor <- Color.Red
    else
        // If valid seats are found, reserve each seat
        availableSeats |> List.iter (fun (rowSeat, columnSeat) ->
            reserveSeat conn rowSeat columnSeat nameTextBox showtimeTextBox statusLabel
        )



// // Returns a list of all available seats
let getAvailableSeats (conn: MySqlConnection) (listBox: DataGridView) (statusLabel: Label) =
    try
        // Prepare the SQL query to select available seats
        let getAvailableSeatsQuery = "SELECT * FROM Seats WHERE Status = 'Available'"

        // Create a command to execute the query
        use getSeatsCmd = new MySqlCommand(getAvailableSeatsQuery, conn)
        
        // Execute the query and read the results
        use reader = getSeatsCmd.ExecuteReader()
        
        // Collect available seats 
        if reader.HasRows then
            listBox.Rows.Clear()
            while reader.Read() do
                let Id = reader.GetInt32(0)
                let RowSeat = reader.GetInt32(1)
                let ColumnSeat = reader.GetInt32(2)
                let Status = reader.GetString(3)
                listBox.Rows.Add(Id , RowSeat , ColumnSeat , Status)
        else
            statusLabel.Text <- "No available seat found."                
    with
    | ex -> 
       // Handle any errors that occur
        statusLabel.Text <- sprintf "Error: %s" ex.Message
        statusLabel.ForeColor <- Color.Red
