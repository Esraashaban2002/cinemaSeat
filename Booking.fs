module Booking

open System
open MySql.Data.MySqlClient
open System.Windows.Forms
open System.Drawing
open Connction

// Booking Form
let bookingTicketForm () =
    // Initialize the form with a white background color
    let form = new Form(Text = "Library Management System", AutoSize = true, BackColor = Color.White)
    form.StartPosition <- FormStartPosition.CenterScreen

    // Create Controls
    let nameLabel = new Label(Text = "Name:", Location = Point(10, 10), AutoSize = true)
    let nameTextBox = new TextBox(Location = Point(150, 10), Width = 200)

    let seatLabel = new Label(Text = "Seat (e.g., R1-C1):", Location = Point(10, 40), AutoSize = true)
    let seatTextBox = new TextBox(Location = Point(150, 40), Width = 200)

    let showtimeLabel = new Label(Text = "Showtime (e.g., 7 PM):", Location = Point(10, 80), AutoSize = true)
    let showtimeTextBox = new TextBox(Location = Point(150, 80), Width = 200)

    let statusLabel = new Label(Location = Point(10, 120), Width = 400, Height = 30)

    // Booking Button
    let bookingButton = new Button(Text = "Booking", AutoSize = true, Location = Point(10, 160), 
                                   BackColor = ColorTranslator.FromHtml("#FF8E8F"), 
                                   ForeColor = ColorTranslator.FromHtml("#fff"),
                                   Font = new Font("sans", 18.0f)
                                   )

    // Event Handler to Show Book Details
    bookingButton.Click.Add(fun _ -> 
        let connectionString = Connction.connectionString
        use conn = new MySqlConnection(connectionString)
        conn.Open()
        BookingControler.bookingTicket conn nameTextBox seatTextBox showtimeTextBox statusLabel
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
