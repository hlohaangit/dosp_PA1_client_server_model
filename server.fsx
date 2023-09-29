open System
open System.Net
open System.Net.Sockets
open System.Text
open System.IO
open System.Threading.Tasks

let port = 12345

let server () =
    let ipAddress = IPAddress.Parse("127.0.0.1")
    let listener = new TcpListener(ipAddress, port)
    listener.Start()
    printfn "Server listening on port %d" port

    let rec handleClient (client : TcpClient) =
         async {
            try
                let stream = client.GetStream()
                let reader = new StreamReader(stream)
                let writer = new StreamWriter(stream)
                writer.WriteLine("Hello!")
                writer.Flush()

                while true do
                    let message = reader.ReadLine()
                    printfn "Received: %s" message

                    let words = message.Split[|' '|]
                    
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
                    
                    // | "exit" ->
                    //     result <- -5

                    let response = result
                    
                    writer.WriteLine(response)
                    writer.Flush()
            with
                | :? System.IO.IOException -> printfn "Client disconnected"
                | ex -> printfn "Error: %s" ex.Message

            //return ()
        }

    let rec acceptClients () =
        async {
            let client = listener.AcceptTcpClient()
            let! _ = Async.StartChild (handleClient client)
            return! acceptClients ()
        }

    Async.RunSynchronously (acceptClients ())
    

[<EntryPoint>]

server ()
0 // Exit code