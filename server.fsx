open System
open System.Net
open System.Net.Sockets
open System.Text
open System.IO
open System.Text.RegularExpressions
open System.Threading.Tasks
open System.Threading

let port = 12345

let cancelSource = new CancellationTokenSource()

let isValidMessage(msg:string) : int = 
    let words = msg.Split[|' '|]
    let mutable res = 0
    if words.Length = 0 then
        res <- -1
        res
    elif msg = "bye" || msg = "terminate" then
        res <- -5
        res
    elif(words.[0] <> "add" && words.[0] <> "subtract" && words.[0] <> "multiply") then
        res <- -1
        res
    elif words[1..].Length < 2 then
        res <- -2
        res
    elif words[1..].Length > 4 then
        res <- -3
        res
    else
        for item in words[1..] do
            let integerPattern = Regex("^-?\\d+$")
            let isInt = integerPattern.IsMatch(item)
            if not isInt then
                res <- -4
        res


let server () =
    let ipAddress = IPAddress.Parse("127.0.0.1")
    let listener = new TcpListener(ipAddress, port)
    listener.Start()
    printfn "Server is running and listening on port %d" port

    let rec handleClient (client : TcpClient, clientNum : int) =
         async {
            try
                let stream = client.GetStream()
                let reader = new StreamReader(stream)
                let writer = new StreamWriter(stream)
                writer.WriteLine("Hello!")
                writer.Flush()

                while true do
                    let message = reader.ReadLine()
                    printfn "Received: %s"  message

                    let words = message.Split[|' '|]
                    let error_code = isValidMessage(message)
                    if error_code < 0 then
                        printfn "Responding to client %i with result: %i" clientNum error_code
                        writer.WriteLine(error_code)
                        writer.Flush()
                        if message = "terminate" then
                            ()
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

    let mutable count = 0

    let rec acceptClients () =
        
        async {
            let client = listener.AcceptTcpClient()
            count <- count + 1
            let! _ = Async.StartChild (handleClient(client,count) )
            return! acceptClients ()
        }

    Async.RunSynchronously (acceptClients ())
    

[<EntryPoint>]
server ()
0 // Exit code