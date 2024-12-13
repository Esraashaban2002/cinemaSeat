module Program
open System
open MySql.Data.MySqlClient
open System.IO
open System.Text.Json
open System.Windows.Forms
open System.Drawing
open Connction 
open Booking

let filePath = "seatStates.json"

// Initialize Seat States
let seatStates = Array2D.create 10 10 "available"

// Save Seat States to a File
let saveSeatStates () =
    let seatList = 
        [ for i in 0 .. Array2D.length1 seatStates - 1 -> 
            [ for j in 0 .. Array2D.length2 seatStates - 1 -> seatStates.[i, j] ] ]
    let json = JsonSerializer.Serialize(seatList)
    File.WriteAllText(filePath, json)

// Load Seat States from a File
let loadSeatStates () =
    if File.Exists(filePath) then
        let json = File.ReadAllText(filePath)
        let seatList = JsonSerializer.Deserialize<string list list>(json)
        for i = 0 to seatList.Length - 1 do
            for j = 0 to seatList.[i].Length - 1 do
                seatStates.[i, j] <- seatList.[i].[j]

// Update the button colors and text based on the seat states
let updateSeatDisplay (button: Button) (row: int) (col: int) =
    match seatStates.[row, col] with
    | "available" -> button.BackColor <- ColorTranslator.FromHtml("#FF8E8F")
    | "selected" -> button.BackColor <- Color.DarkRed
    | "booked" -> button.BackColor <- Color.Green
    | _ -> ()

// Display the current seating chart in the console
let displaySeats () =
    for row in 0 .. 9 do
        let rowStatus = 
            [ for col in 0 .. 9 -> 
                match seatStates.[row, col] with
                | "available" -> "O"
                | "selected" -> "S"
                | "booked" -> "X"
                | _ -> "?" ]
            |> String.concat " "
        printfn "Row %d: %s" (row + 1) rowStatus

let isSeatAvailable (row: int) (col: int) =
    if seatStates.[row, col] = "available" then true else false

// Create Seat Buttons and Add Event Handlers
let createSeatButton row col =
    let button = new Button(Text = sprintf "R%d-C%d" (row + 1) (col + 1), Width = 80, Height = 40)
    updateSeatDisplay button row col
    button.Font <- new Font("sans", 12.0f)
    button.ForeColor <- ColorTranslator.FromHtml("#fff")
    button.Click.Add(fun _ ->
       if isSeatAvailable row col then
                seatStates.[row, col] <- "selected"
                updateSeatDisplay button row col
        elif seatStates.[row, col]="selected" then
                seatStates.[row, col] <- "available"
                updateSeatDisplay button row col
        else
            MessageBox.Show(sprintf "Seat R%d-C%d is already booked!" (row + 1) (col + 1), "Information")|> ignore
    )
    button

let mainForm = new Form(Text = "Cinema Seat Reservation", AutoSize = true, Height = 700)
mainForm.BackColor <- Color.White
mainForm.StartPosition <- FormStartPosition.CenterScreen

let seatPanel = new TableLayoutPanel(AutoSize = true, RowCount = 10, ColumnCount = 10)
seatPanel.CellBorderStyle <- TableLayoutPanelCellBorderStyle.Single

loadSeatStates () // Load saved states before creating buttons

for row in 0 .. 9 do
    for col in 0 .. 9 do
        seatPanel.Controls.Add(createSeatButton row col)

let bookingButton = new Button(Text = "Confirm Booking", AutoSize = true, Height = 60)
bookingButton.BackColor <- ColorTranslator.FromHtml("#FFB38E")
bookingButton.ForeColor <- Color.White
bookingButton.Font <- new Font("sans", 20.0f)

bookingButton.Click.Add(fun _ ->
    for control in seatPanel.Controls do
        match control with
        | :? Button as button ->
            let seat = button.Text.Split([|'-'; 'R'; 'C'|], StringSplitOptions.RemoveEmptyEntries)
            if seat.Length = 2 then
                let row = int seat.[0] - 1
                let col = int seat.[1] - 1
                if seatStates.[row, col] = "selected" then
                    seatStates.[row, col] <- "booked"
                    updateSeatDisplay button row col
        | _ -> ()
        
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

mainForm.FormClosing.Add(fun _ -> saveSeatStates ())

[<EntryPoint>]
let main argv =
    Application.Run(mainForm)
    0
