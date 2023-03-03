﻿using FullNodeDataLogger.EF;
using FullNodeDataLogger.EF.Model;
using FullNodeDataLogger.Management;
using FullNodeDataLogger.Repository;
using Model;
using Observer;

//Test and start logging service for monitoring
var logManagement = new LogManagement(new LogRepository(new FullNodeDataLoggerEntities()));
var logResult =
    logManagement.MakeLog(
        new Log()
        {
            Description = "Observer is starting",
            LogGroup = "1" // Group 1 is info
        });


//Observer is Linux
var isLinux = true;

//Main path to store miners data
var mainPath = "";

if (isLinux)
{
    mainPath = @"Miners/";
}
else
{
    mainPath = @"c:\Miners\";
}




//Cleansing
//Delete old files and folders
try
{
    Directory.Delete(mainPath, true);
}
catch (DirectoryNotFoundException nfex)
{
    logResult =
        logManagement.MakeLog(
            new Log()
            {
                Description = "Directory not found but the observer can continue... ErrorDetail: " + nfex.Message.ToString(),
                LogGroup = "2" // Group 2 is error
            });
}
catch (Exception ex)
{
    logResult =
        logManagement.MakeLog(
            new Log()
            {
                Description = "Error in removing old files and folders... ErrorDetail: " + ex.Message.ToString(),
                LogGroup = "2" // Group 2 is error
            });


    Console.WriteLine("Program cannot continue!!!");
    //Waiting...
    Console.ReadKey();

    //Prevent running program...
    return;
}

// General Info initialization
var observerData = new ObserverData("127.0.0.1", 7000, "127.0.0.1", 7001, "127.0.0.1", 7002);

// Number of full nodes and listening ports
var numberOfFullNodes = 3;

// Network Bandwidth initialization
// 10 nodes and one observer
// 0 means no connection
// 10*1024*1024
// This part is used only for simulation:
//

var networkBandWidth = new List<SpeedLine>();

//Keeping id of full nodes for future tracking ...
List<Guid> nodeIdList = new List<Guid>();

//Adding new guid to each full node
for (int i = 0; i < numberOfFullNodes; i++)
{
    nodeIdList.Add(Guid.NewGuid());
}

//Initializing bandwidth array
for (int i = 0; i < nodeIdList.Count; i++)
{
    for (int j = 0; j < nodeIdList.Count; j++)
    {
        if (i == j)
        {
            networkBandWidth.Add(new SpeedLine() { From = nodeIdList[i], To = nodeIdList[j], Speed = 0 });
        }
        else
        {
            networkBandWidth.Add(new SpeedLine() { From = nodeIdList[i], To = nodeIdList[j], Speed = 10 * 1024 * 1024 });
        }
    }
}

//This guid can be changed :
Guid observerGuid = Guid.Parse("43cf8025-6169-4d8e-bbb2-e82aee2ee1cb");

for (int i = 0; i < nodeIdList.Count; i++)
{
    networkBandWidth.Add(new SpeedLine() { From = nodeIdList[i], To = observerGuid, Speed = 10 * 1024 * 1024 });
}

for (int i = 0; i < nodeIdList.Count; i++)
{
    networkBandWidth.Add(new SpeedLine() { From = observerGuid, To = nodeIdList[i], Speed = 10 * 1024 * 1024 });
}

// Starting observer
var observerNode = new ObserverNode(observerData, networkBandWidth, mainPath, observerGuid);

Console.ReadKey();