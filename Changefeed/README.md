# Changefeed Sample
This sample shows how users can log the events in a storage account using Changefeed. The program iterates through the changefeed and filters and logs events using a predicate. These filters may be changed in the ChangefeedSample.cs file. The ChangefeedSample program is an Azure Function that can be deployed on Azure, but it can also be run locally with emulation.
The ExampleEventCreator program is an optional program that creates a container and example blobs to create events that populate the Changefeed. 

## Prerequisites
Requires installation of [.NET Core](https://dotnet.microsoft.com/download/dotnet-core),
[Visual Studio 2019](https://visualstudio.microsoft.com/downloads/) with the Azure development workload also installed,
an [Azure subscription](https://azure.microsoft.com/en-us/free/), 
an [Azure storage account](https://docs.microsoft.com/en-us/azure/storage/common/storage-account-create?tabs=azure-portal), 
and an [Azure Function](https://docs.microsoft.com/en-us/azure/azure-functions/functions-create-scheduled-function#:~:text=Create%20a%20timer%20triggered%20function%201%20Expand%20your,by%20viewing%20trace%20information%20written%20to%20the%20logs.).

## Code Setup
Before running ExampleEventCreator, users must add values to App.Config for the following variables:
- Connection String

Before running/deploying ChangefeedSample, users must change the variables in the ChangefeedSample.cs file for the following variables:
- Connection String

## Running ChangefeedSample Locally
### Option 1
1. Navigate to ChangefeedSample directory
2. Build and run the project
### Option 2
1. Open ChangefeedSample project
2. Press F5 to build and run project

## Deploying ChangefeedSample to Azure Functions
1. Open ChangefeedSample project in Visual Studio
2. Right-click on the project in Solution Explorer and select Publish.
3. For your publish target, choose Azure Function App, and then choose Select Existing. Then click Publish.
4. If you haven't already connected Visual Studio to your Azure account, select Add an account… and follow the onscreen instructions.
5. Under Subscription, select your subscription. Search for your Azure function, and then select it in the section below. Then click OK.

## Viewing logs from Azure Function
To view logs from the deployed ChangefeedSample program, follow the instructions [here](https://docs.microsoft.com/en-us/azure/azure-functions/functions-monitoring?tabs=cmd)
