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
        let rec sendMessage2() =
            async {
                while true do
                    let response = reader.ReadLine()
                    if response = "-1" then
                        printfn "incorrect operation command"
                    elif response = "-2" then
                        printfn "number of inputs is less than two"
                    elif response = "-3" then
                        printfn "number of inputs is more than four"
                    elif response = "-4" then
                        printfn "one or more of the inputs contain(s) non-number(s)"
                    elif response = "-5" then
                        printfn "exit"
                        Environment.Exit(0) 
                    else
                        printfn "Server response: %s" response 
                    printfn "Sending Command(or 'bye' to quit):"
            }
        async {
            let response = reader.ReadLine()
            printfn "Server response: %s" response
            let!_ = Async.StartChild(sendMessage2())
            printfn "Sending Command(or 'bye' to quit):"
            while true do
                let message = Console.ReadLine()
                writer.WriteLine(message)
                writer.Flush()
                return ()
                
        }
    
    

    
    Async.RunSynchronously (sendMessage ())
    
    //client.Close()

[<EntryPoint>]

client ()
0 // Exit code