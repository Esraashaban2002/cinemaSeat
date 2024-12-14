module Program
open System
open MySql.Data.MySqlClient
open System.Windows.Forms
open System.Drawing
open Connction 
open Booking


let checkSeatButton (conn: MySqlConnection) (button: Button) row col =
    let checkSeatQuery = "SELECT Status FROM Seats WHERE Row_seat = @row AND Column_seat = @col"
    use cmd = new MySqlCommand(checkSeatQuery, conn)
    cmd.Parameters.AddWithValue("@row", row + 1) |> ignore
    cmd.Parameters.AddWithValue("@col", col + 1) |> ignore
    use reader = cmd.ExecuteReader()
    if reader.Read() then
        let status = reader.GetString(0)
        reader.Close()
        button.BackColor <- 
            if status = "Available" then ColorTranslator.FromHtml("#FF8E8F")
            else Color.Gray

// Create Seat Buttons and Add Event Handlers
let createSeatButton (conn: MySqlConnection) row col =
    let button = new Button(Text = sprintf "R%d-C%d" (row + 1) (col + 1), Width = 80, Height = 40)
    button.Font <- new Font("sans", 12.0f)
    button.ForeColor <- Color.White

    checkSeatButton conn button row col 
    
    // insert the seat into the database
    // try
    //     conn.Open()
    //     let insertSeatQuery = "INSERT INTO Seats (Row_seat, Column_seat, Status) VALUES (@Row, @Column, @Status)"
    //     use cmd = new MySqlCommand(insertSeatQuery, conn)
    //     cmd.Parameters.AddWithValue("@Row", row + 1) |> ignore
    //     cmd.Parameters.AddWithValue("@Column", col + 1) |> ignore
    //     cmd.Parameters.AddWithValue("@Status", "Available") |> ignore
    //     cmd.ExecuteNonQuery()
        
    //     MessageBox.Show(sprintf "Seat R%d-C%d successfully added to the database." (row + 1) (col + 1), "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
    // with ex ->
    //     MessageBox.Show(sprintf "Error adding seat: %s" ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
    // conn.Close()
    button

let mainForm = new Form(Text = "Cinema Seat Reservation", AutoSize = true, Height = 700)
mainForm.BackColor <- Color.White
mainForm.StartPosition <- FormStartPosition.CenterScreen

let seatPanel = new TableLayoutPanel(AutoSize = true, RowCount = 10, ColumnCount = 10)
seatPanel.CellBorderStyle <- TableLayoutPanelCellBorderStyle.Single

let connectionString = Connction.connectionString
use conn = new MySqlConnection(connectionString)
conn.Open()

for row in 0 .. 9 do
    for col in 0 .. 9 do
        seatPanel.Controls.Add(createSeatButton conn row col)

let bookingButton = new Button(Text = "Confirm Booking", AutoSize = true, Height = 60)
bookingButton.BackColor <- ColorTranslator.FromHtml("#FFB38E")
bookingButton.ForeColor <- Color.White
bookingButton.Font <- new Font("sans", 20.0f)

bookingButton.Click.Add(fun _ ->
    Booking.bookingTicketForm()
)

mainForm.Controls.Add(bookingButton)
mainForm.Resize.Add(fun _ ->
    bookingButton.Left <- (mainForm.ClientSize.Width - bookingButton.Width) / 2
    bookingButton.Top <- mainForm.ClientSize.Height - bookingButton.Height - 20
)
mainForm.Controls.Add(seatPanel)
mainForm.Resize.Add(fun _ ->
    seatPanel.Left <- (mainForm.ClientSize.Width - seatPanel.Width) / 2
)

[<EntryPoint>]
let main argv =
    Application.Run(mainForm)
    0