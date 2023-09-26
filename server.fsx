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

                let message = reader.ReadLine()
                printfn "Received: %s" message

                let response = sprintf "Server received: %s" message
                writer.WriteLine(response)
                writer.Flush()
            with
                | :? System.IO.IOException -> printfn "Client disconnected"
                | ex -> printfn "Error: %s" ex.Message

            return ()
        }

    let rec acceptClients () =
        async {
            let client = listener.AcceptTcpClient()
            Async.Start (async {
                do! handleClient client
            })
            return! acceptClients ()
        }

    Async.RunSynchronously (acceptClients ())

[<EntryPoint>]

server ()
0 // Exit code