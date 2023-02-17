# ReliableDistributedStorageSystem
A Reliable Distributed Storage System using Blockchain Networks
Having reliable data storage that has a high availability in a distributed environment is a challenging issue that may face many other issues that can affect the performance of data processing in the network. 
Different types of techniques like replicating data on multiple nodes and updating them with the latest version of data would result in high costs for the network and other related resources. 
The security and privacy of data are other issues that need some techniques to prevent data from being exposed, tampered and manipulated by adversary actors. 
We have proposed a simple permissioned blockchain-inspired system that stores the chunks of files on different nodes and synchronizes them over a period of time. 

## Building and Run
First download the whole source code or clone this repository on local computer. Then download and install latest version of [the .NET Core SDK](https://www.microsoft.com/net/download). Then run
these commands from the CLI in the directory of FullFunctionProject:

```console
dotnet build
dotnet run
```

These will install any needed dependencies, build the project, and run
the project respectively.

## Data Storage
Please be careful that project uses c drive to store fullnodes data. 
```csharp
//Main path to store miners data
var mainPath = @"c:\Miners\";
```

In FullFunctionProject\Program.cs you can change the transaction file:
```csharp
    StarterModel startModel = new StarterModel()
    {
        FileName = "I Walk Alone_v720P.mp4",
        FileContent = File.ReadAllBytes(@"C:\Users\farhad\Downloads\Video\I Walk Alone_v720P.mp4"),
        SleepRetryObserver = 50,
        NumberOfRetryObserver = 50,
        SleepRetrySendFile = 1000,
        NumberOfRetrySendFile = 3,
        RandomizeRangeSleep = 1000,
        Node = fullNode
    };
```