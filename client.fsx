open System
open System.Net
open System.Net.Sockets
open System.Text
open System.IO
open System.Threading.Tasks

let serverAddress = "127.0.0.1"
let port = 12345

let client () =
    let client = new TcpClient(serverAddress, port)
    let stream = client.GetStream()
    let reader = new StreamReader(stream)
    let writer = new StreamWriter(stream)

    printfn "Connected to server at %s:%d" serverAddress port

    let rec sendMessage() =
        async {
            let response = reader.ReadLine()
            printfn "Server response: %s" response
            while true do
                printfn "Enter a message (or 'exit' to quit):"
                
                let message = Console.ReadLine()
                writer.WriteLine(message)
                writer.Flush()

                let response = reader.ReadLine()
                if response <> "-5" then
                    printfn "Server response: %s" response
                else
                    printfn "exit"
                    return! sendMessage()
                
        }

    
    Async.RunSynchronously (sendMessage ())
    //client.Close()

[<EntryPoint>]

client ()
0 // Exit code