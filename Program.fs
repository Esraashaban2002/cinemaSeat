open System
open System.Windows.Forms
open System.Drawing

// Create the Main Form
let mainForm = new Form(Text = "Cinema Seat Reservation", AutoSize = true, Height = 700)
mainForm.BackColor <- Color.White

// Open Form in the Center of Screen
mainForm.StartPosition <- FormStartPosition.CenterScreen

// Table Layout Panel for Seat Layout
let seatPanel = new TableLayoutPanel(AutoSize = true, RowCount = 10, ColumnCount = 10)
seatPanel.CellBorderStyle <- TableLayoutPanelCellBorderStyle.Single

// Add Buttons to Represent Seats
let createSeatButton row col =
    let button = new Button(Text = sprintf "R%d-C%d" (row + 1) (col + 1), Width = 80, Height = 40)
    button.BackColor <-  ColorTranslator.FromHtml("#FF8E8F")
    button.Font <- new Font("sans", 12.0f)
    button.ForeColor <- ColorTranslator.FromHtml("#fff")
    button

// Populate the Seat Panel with Buttons
for row in 0 .. 9 do
    for col in 0 .. 9 do
        seatPanel.Controls.Add(createSeatButton row col)

// Create the "Confirm Booking" Button
let bookingButton = new Button(Text = "Confirm Booking", AutoSize = true, Height = 60)
bookingButton.BackColor <- ColorTranslator.FromHtml("#FFB38E")
bookingButton.ForeColor <- Color.White
bookingButton.Font <- new Font("sans", 20.0f)

mainForm.Controls.Add(bookingButton)
mainForm.Resize.Add(fun _ ->
    bookingButton.Left <- (mainForm.ClientSize.Width - bookingButton.Width) / 2
    bookingButton.Top <- mainForm.ClientSize.Height - bookingButton.Height - 20
)

// Add Components to the Main Form
mainForm.Controls.Add(seatPanel)
mainForm.Resize.Add(fun _ ->
    seatPanel.Left <- (mainForm.ClientSize.Width - seatPanel.Width) / 2
)

// Run the Application
[<EntryPoint>]
let main argv =
    Application.Run(mainForm)
    0
