# Changefeed Sample
This sample shows how users can log the events in a storage account using Changefeed. The program iterates on a scheduled interval through the changefeed and filters and logs events using a predicate. These filters may be changed in the ChangefeedSample.cs file. The ChangefeedSample program is an Azure Function that can be deployed on Azure, but it can also be run locally with emulation.
The ExampleEventCreator program is an optional program that creates a container and example blobs to create events that populate the Changefeed on a scheduled interval.  By default, both functions run every 30 minutes.
To change the interval of ChangeFeedSample, edit the CRON string in the Run function. To change the interval of ExampleEventCreator, edit the number of milliseconds in the Timer object.

## Prerequisites
Requires installation of [.NET Core](https://dotnet.microsoft.com/download/dotnet-core),
[Visual Studio 2019](https://visualstudio.microsoft.com/downloads/) with the Azure development workload also installed,
an [Azure subscription](https://azure.microsoft.com/en-us/free/), 
an [Azure storage account](https://docs.microsoft.com/en-us/azure/storage/common/storage-account-create?tabs=azure-portal) with [Changefeed enabled](https://docs.microsoft.com/en-us/azure/storage/blobs/storage-blob-change-feed?tabs=azure-portal#enable-and-disable-the-change-feed), 
and an [Azure Function](https://docs.microsoft.com/en-us/azure/azure-functions/functions-create-scheduled-function#:~:text=Create%20a%20timer%20triggered%20function%201%20Expand%20your,by%20viewing%20trace%20information%20written%20to%20the%20logs.).

## Code Setup
Before running ExampleEventCreator, users must add values to App.Config for the following variables:
- Connection String

Before running/deploying ChangefeedSample, users must change the variables in the Constants.cs file for the following variables:
- Connection String
- Blob Name (Optional)
- Container Name (Optional)
- Event Type (Optional)

## Step-By-Step Instructions for Running Program
1. If using ExampleEventCreator, enter values in App.config
2. Navigate to ExampleEventCreator directory
3. Build and run ExampleEventCreator
4. Enter values in Constants.cs file of ChangefeedSample project
5. If needed, edit the time trigger interval 
6. Run ChangefeedSample locally or deploy it to an Azure function using instructions below.

## Running ChangefeedSample Locally
### Option 1
1. Navigate to ChangefeedSample directory
2. Build and run the project
### Option 2
1. Open ChangefeedSample project in Visual Studio
2. Press F5 to build and run project

## Deploying ChangefeedSample to Azure Functions
1. Open ChangefeedSample project in Visual Studio
2. Right-click on the project in Solution Explorer and select Publish.
3. For your publish target, choose Azure Function App, and then choose Select Existing. Then click Publish.
4. If you haven't already connected Visual Studio to your Azure account, select Add an account… and follow the onscreen instructions.
5. Under Subscription, select your subscription. Search for your Azure function, and then select it in the section below. Then click OK.
6. In Azure portal navigate to your Function App and select your function under the "Functions" tab.
7. Select "Code + Test", and "Test/Run"
8. Click "Run"


## Viewing logs from Azure Function
To view logs from the deployed ChangefeedSample program, follow the instructions [here](https://docs.microsoft.com/en-us/azure/azure-functions/functions-monitoring?tabs=cmd).
#### Example Query:
traces <br/>
| where message contains "event:"

