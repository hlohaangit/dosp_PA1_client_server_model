open System
open System.Diagnostics
open System.Threading

// Define expected message and response
let clientMessageToSend = "add 20 4"
let expectedServerResponse = "Server resposded: 24"

// Start the server in a separate process
let startServer() =
    let serverProcess = new Process()
    serverProcess.StartInfo.FileName <- "dotnet"
    serverProcess.StartInfo.Arguments <- "fsi server.fsx"
    serverProcess.Start() |> ignore
    serverProcess

// Start the client and capture its output
let startClientAndGetOutput() =
    printfn "starting client function"
    let clientProcess = new Process()
    clientProcess.StartInfo.FileName <- "dotnet"
    clientProcess.StartInfo.Arguments <- "fsi client.fsx"
    clientProcess.StartInfo.Arguments <- clientMessageToSend
    clientProcess.StartInfo.RedirectStandardOutput <- true
    clientProcess.StartInfo.UseShellExecute <- false
    clientProcess.Start() |> ignore

    let output = clientProcess.StandardOutput.ReadToEnd()
    clientProcess.WaitForExit()
    output

// Test case: Verify client receives expected message from server
let testCase1() =
    let serverProcess = startServer()
    Thread.Sleep(300)  // Give server some time to start
    let clientOutput = startClientAndGetOutput()
    serverProcess.Kill()  // Stop the server process

    printfn "Server message : %s" clientOutput
    if clientOutput.Contains(expectedServerResponse) then
        printfn "Test Case 1: Passed"
    else
        printfn "Test Case 1: Failed"

// Run the test case
testCase1()
