module Program

open System
open MySql.Data.MySqlClient
open System.Windows.Forms
open System.Drawing
open Connction
open Booking

// 2D array to track seat availability
let rows, cols = 10, 10 
let seats = Array2D.init rows cols (fun _ _ -> "Available") 

// Mutable list to store reserved seats
let reservedSeats = ref [] 

// Check seat availability from the database and update UI button
let checkSeatButton (conn: MySqlConnection) (button: Button) row col =
    let checkSeatQuery = "SELECT Status FROM Seats WHERE Row_seat = @Row AND Column_seat = @Col"
    use cmd = new MySqlCommand(checkSeatQuery, conn)
    cmd.Parameters.AddWithValue("@Row", row + 1) |> ignore
    cmd.Parameters.AddWithValue("@Col", col + 1) |> ignore
    use reader = cmd.ExecuteReader()
    if reader.Read() then
        let status = reader.GetString(0)
        reader.Close()
        seats.[row, col] <- status
        button.BackColor <- 
            if status = "Available" then ColorTranslator.FromHtml("#FF8E8F") 
            else Color.Gray 
        button.Enabled <- (status = "Available") 

let reserveSeat (conn: MySqlConnection) (button: Button) row col =
    seats.[row, col] <- "Checked"
    button.BackColor <- Color.Gray
    button.Enabled <- false 
    reservedSeats := reservedSeats.Value @ [sprintf "R%d-C%d" (row + 1) (col + 1)]

let createSeatButton (conn: MySqlConnection) row col =
    let button = new Button(Text = sprintf "R%d-C%d" (row + 1) (col + 1), Width = 80, Height = 40)
    button.Font <- new Font("sans", 12.0f)
    button.BackColor <- ColorTranslator.FromHtml("#FF8E8F")
    button.ForeColor <- Color.White
    checkSeatButton conn button row col

    // insert the seat into the database
    // try
    //     // conn.Open()
    //     let insertSeatQuery = "INSERT INTO Seats (Row_seat, Column_seat, Status) VALUES (@Row, @Column, @Status)"
    //     use cmd = new MySqlCommand(insertSeatQuery, conn)
    //     cmd.Parameters.AddWithValue("@Row", row + 1) |> ignore
    //     cmd.Parameters.AddWithValue("@Column", col + 1) |> ignore
    //     cmd.Parameters.AddWithValue("@Status", "Available") |> ignore
    //     cmd.ExecuteNonQuery()
        
    //     MessageBox.Show(sprintf "Seat R%d-C%d successfully added to the database." (row + 1) (col + 1), "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
    // with ex ->
    //     MessageBox.Show(sprintf "Error adding seat: %s" ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
    // // conn.Close()
    
    button.Click.Add(fun _ -> 
        if seats.[row, col] = "Checked" then
            button.BackColor <- ColorTranslator.FromHtml("#FF8E8F")
            seats.[row, col] <- "Available"
            // Remove the seat from the reserved list
            reservedSeats := reservedSeats.Value |> List.filter (fun s -> s <> sprintf "R%d-C%d" (row + 1) (col + 1))
        else
            seats.[row, col] <- "Checked"
            button.BackColor <- Color.Orange
            // Add the seat to the reserved list
            reservedSeats := reservedSeats.Value @ [sprintf "R%d-C%d" (row + 1) (col + 1)]
    )

    button


// Main form 
let mainForm = new Form(Text = "Cinema Seat Reservation", AutoSize = true, Height = 700)
mainForm.BackColor <- Color.White
mainForm.StartPosition <- FormStartPosition.CenterScreen

let seatPanel = new TableLayoutPanel(AutoSize = true, RowCount = 10, ColumnCount = 10)
seatPanel.CellBorderStyle <- TableLayoutPanelCellBorderStyle.Single

let connectionString = Connction.connectionString
use conn = new MySqlConnection(connectionString)
conn.Open()

// Add seat buttons 
for row in 0 .. 9 do
    for col in 0 .. 9 do
        seatPanel.Controls.Add(createSeatButton conn row col)

// Booking button 
let bookingButton = new Button(Text = "Confirm Booking", AutoSize = true, Height = 60)
bookingButton.BackColor <- ColorTranslator.FromHtml("#FFB38E")
bookingButton.ForeColor <- Color.White
bookingButton.Font <- new Font("sans", 20.0f)

// Available button 
let availableButton = new Button(Text = "Available Seat", AutoSize = true, Height = 60)
availableButton.BackColor <- ColorTranslator.FromHtml("#FFB38E")
availableButton.ForeColor <- Color.White
availableButton.Font <- new Font("sans", 20.0f)

bookingButton.Click.Add(fun _ -> 
    // Pass the list of reserved seats to the booking form
    if reservedSeats.Value.Length > 0 then
        Booking.bookingTicketForm reservedSeats.Value
    else
        MessageBox.Show("No seats have been reserved.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error) |> ignore
)

availableButton.Click.Add(fun _ -> 
   Booking.availableSeatsForm()
)

mainForm.Controls.Add(bookingButton)
mainForm.Resize.Add(fun _ -> 
    bookingButton.Left <- 50
    bookingButton.Top <- mainForm.ClientSize.Height - bookingButton.Height - 20
)
mainForm.Controls.Add(availableButton)
mainForm.Resize.Add(fun _ -> 
    availableButton.Left <- 500
    availableButton.Top <- mainForm.ClientSize.Height - bookingButton.Height - 20
)

mainForm.Controls.Add(seatPanel)
mainForm.Resize.Add(fun _ -> 
    seatPanel.Left <- (mainForm.ClientSize.Width - seatPanel.Width) / 2
)

[<EntryPoint>]
let main argv =
    Application.Run(mainForm)
    0
