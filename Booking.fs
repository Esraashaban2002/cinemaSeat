module Booking

open System
open MySql.Data.MySqlClient
open System.Windows.Forms
open System.Drawing
open Connction
open BookingControler

let bookingTicketForm (reservedSeats: string list) =
    let form = new Form()
    form.Text <- "Cinema Seat Reservation"
    form.AutoSize <- true
    form.BackColor <- Color.White
    form.StartPosition <- FormStartPosition.CenterScreen

    // Create Controls
    let nameLabel = new Label()
    nameLabel.Text <- "Name:"
    nameLabel.Location <- Point(10, 10)
    nameLabel.AutoSize <- true

    let nameTextBox = new TextBox()
    nameTextBox.Location <- Point(150, 10)
    nameTextBox.Width <- 200

    let seatLabel = new Label()
    seatLabel.Text <- "Seat (e.g., R1-C1):"
    seatLabel.Location <- Point(10, 40)
    seatLabel.AutoSize <- true

    let seatTextBox = new TextBox()
    seatTextBox.Location <- Point(150, 40)
    seatTextBox.Width <- 200
    seatTextBox.AutoSize <- true
    seatTextBox.ReadOnly <- true

    // Automatically populate the seatTextBox with the reserved seats
    seatTextBox.Text <- String.Join(", ", reservedSeats)

    let showtimeLabel = new Label()
    showtimeLabel.Text <- "Showtime (e.g., 7 PM):"
    showtimeLabel.Location <- Point(10, 80)
    showtimeLabel.AutoSize <- true

    let showtimeTextBox = new TextBox()
    showtimeTextBox.Location <- Point(150, 80)
    showtimeTextBox.AutoSize <- true

    let statusLabel = new Label()
    statusLabel.Location <- Point(10, 120)
    statusLabel.Width <- 400
    statusLabel.Height <- 30

    let bookingButton = new Button()
    bookingButton.Text <- "Booking"
    bookingButton.AutoSize <- true
    bookingButton.Location <- Point(10, 160)
    bookingButton.BackColor <- ColorTranslator.FromHtml("#FF8E8F")
    bookingButton.ForeColor <- ColorTranslator.FromHtml("#fff")
    bookingButton.Font <- new Font("sans", 18.0f)

    // Event Handler to Show Book Details
    bookingButton.Click.Add(fun _ -> 
        let connectionString = Connction.connectionString
        use conn = new MySqlConnection(connectionString)
        conn.Open()
        BookingControler.bookingTicket conn nameTextBox reservedSeats showtimeTextBox statusLabel
    )

    // Add Controls to Form
    form.Controls.Add(nameLabel)
    form.Controls.Add(nameTextBox)
    form.Controls.Add(seatLabel)
    form.Controls.Add(seatTextBox)
    form.Controls.Add(showtimeLabel)
    form.Controls.Add(showtimeTextBox)
    form.Controls.Add(statusLabel)
    form.Controls.Add(bookingButton)

    form.Show()
