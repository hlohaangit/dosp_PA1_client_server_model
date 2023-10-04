open System
open System.Net
open System.Net.Sockets
open System.Text
open System.IO
open System.Text.RegularExpressions
open System.Threading.Tasks
open System.Threading
open System.Collections.Generic

// Port of the Server
let port = 12345

// List to store the TCP clients connected
let  clientSockets = new List<TcpClient>()


// To validate the message and return Error Codes
let isValidMessage(msg:string) : int = 
    let words = msg.Split[|' '|]
    let mutable res = 0

    // If the input is empty then return Error Code as -1
    if words.Length = 0 then
        res <- -1
    // If the input is bye or terminate then return Error Code as -5
    elif msg = "bye" || msg = "terminate" then
        res <- -5
    // Check if the operator is valid and return the Error Code as -1
    elif(words.[0] <> "add" && words.[0] <> "subtract" && words.[0] <> "multiply") then
        res <- -1
    // Check if the number of inputs is less than 2 and return the Error Code as -2
    elif words[1..].Length < 2 then
        res <- -2
    // Check if the number of inputes is more than 4 return the Error Code as -3
    elif words[1..].Length > 4 then
        res <- -3
    else
        // Check if all the inputs are valid Natural numbers and return the Error Code as -4
        for item in words[1..] do
            let integerPattern = Regex("^-?\\d+$")
            let isInt = integerPattern.IsMatch(item)
            if not isInt then
                res <- -4
    res

let server () =

    // Creating a Socket at localhost with port as 12345
    let ipAddress = IPAddress.Parse("127.0.0.1")
    let listener = new TcpListener(ipAddress, port)
    listener.Start()
    printfn "Server is running and listening on port %d" port

    // Method to handle client requests Asynchronously
    let rec handleClient (client : TcpClient, clientNum : int, clientSockets: List<TcpClient>) =
         async {
            try
                // Initializing Reading and Writing Stream for the Socket
                let stream = client.GetStream()
                let reader = new StreamReader(stream)
                let writer = new StreamWriter(stream)

                // Writing Hello message into the Socket
                writer.WriteLine("Hello!")
                writer.Flush()
                let mutable takeNextCommand = true

                // Take commands until the current child is terminated
                while takeNextCommand do

                    // Read the message from the socket
                    let message = reader.ReadLine()
                    printfn "Received: %s"  message

                    // Prase and Validate the input
                    let words = message.Split[|' '|]
                    let error_code = isValidMessage(message)
                    // If there is an error then send appropriate error_codes to clients
                    if error_code < 0 then
                        printfn "Responding to client %i with result: %i" clientNum error_code
                        if message = "bye" then
                            takeNextCommand <- false
                            writer.WriteLine("-5")
                            writer.Flush()
                        elif message = "terminate" then
                            // When the messge is terminate then terminate all clients and the server programs
                            for cl in clientSockets do
                                let st = cl.GetStream()
                                let wt = new StreamWriter(st)
                                takeNextCommand <- false
                                wt.WriteLine("-5")
                                wt.Flush()
                            Environment.Exit(0)
                        else
                            writer.WriteLine(error_code)
                            writer.Flush()
                    else                    
                        //matching add, subtract, multiply (operators) with operands 
                        let mutable result = 0
                        match words.[0] with
                        | "add" ->
                            let operands = Array.tail words |> Array.map int
                            result <- Array.sum operands
                            
                        | "subtract" ->
                            let operands = Array.tail words |> Array.map int
                            result <- Array.reduce (-) operands
                            
                        | "multiply" ->
                            let operands = Array.tail words |> Array.map int
                            result <- Array.reduce (*) operands
                        
                        | "bye" ->
                            client.Close()
                            result <- -5

                        let response = result
                        printfn "Responding to client %i with result: %i" clientNum response
                        writer.WriteLine(response)
                        writer.Flush()
            with
                | :? System.IO.IOException -> printfn "Client disconnected"
                | ex -> printfn "Error: %s" ex.Message

            return ()
        }

    // variable to store the number of clients
    let mutable count = 0

    //  This method runs asyncronously to accept connections from clients
    let rec acceptClients () =
        
        async {
            let client = listener.AcceptTcpClient()
            count <- count + 1
            clientSockets.Add(client)
            // Starting a new Asynchronous process to handle the client requests
            let! _ = Async.StartChild (handleClient(client,count, clientSockets))
            return! acceptClients ()
        }

    Async.RunSynchronously (acceptClients ())
    

[<EntryPoint>]
server ()
0 // Exit code