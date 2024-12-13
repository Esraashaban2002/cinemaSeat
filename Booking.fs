module Booking 

open System
open MySql.Data.MySqlClient
open System.Windows.Forms
open System.Drawing

open Connction

let bookingTicketForm () =
    // Initialize the form with a white background color
    let form = new Form(Text = "Library Management System", AutoSize = true, BackColor = Color.White)

    // Create Controls
    let nameLabel = new Label(Text = "Name:", Location = Point(10, 40), AutoSize = true)
    let nameTextBox = new TextBox(Location = Point(100, 40), Width = 200)

    let statusLabel = new Label(Location = Point(10, 160), Width = 400, Height = 30)

    let bookingButton = new Button(Text = "Booking", AutoSize = true, Location = Point(10, 190), BackColor = ColorTranslator.FromHtml("#FF8E8F"))

    // Event Handler to Show Book Details
    bookingButton.Click.Add(fun _ -> 
        let connectionString = Connction.connectionString
        use conn = new MySqlConnection(connectionString)
        conn.Open()
        // BookingControler.bookingTicket conn nameTextBox statusLabel
    )

    // Add Controls to Form
    form.Controls.Add(nameLabel)
    form.Controls.Add(nameTextBox)
    form.Controls.Add(statusLabel)
    form.Controls.Add(bookingButton)

    form.Show()