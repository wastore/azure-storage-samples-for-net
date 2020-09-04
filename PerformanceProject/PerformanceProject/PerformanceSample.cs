using System;
using Azure.Storage.Blobs;
using System.IO;
using System.Diagnostics;
using Azure.Storage;
using Excel = Microsoft.Office.Interop.Excel;
using System.Reflection;


namespace PerformanceProject
{
    class Performance
    {
        static void Main(string[] args)
        {
            Console.WriteLine("The purpose of this program is to gather data on which upload options create the most time efficient upload.\n" +
                "This program will upload 25 blobs to a container and keep track of the amount time the upload took. The program will first check\n" +
                "get the upload time of the control upload where max concurrency is 1 and initial transfer size as well as max transfer size are 1*1024*1024.\n" +
                "The program will then test the upload time again while changing only one of these variables and keeping the other two constant in order to see\n" +
                "trends in upload time from each variable. Max concurrency will be tested from 2 to 20 at increments of 1, and initial and max transfer size will\n" +
                "be tested from 16*1024*1024 to 56*1024*1024 at increments of 16*1024*1024. This data will be inputted to an excel sheet and made into a graph.\n" +
                "Lastly, the program will check all of the previously mentioned combinations at the same time, to see which is the fastest overall upload. All of\n" +
                "the data found in this program will be saved to a file. This program will run 3 times to test the upload of 32MB, 64MB, and 128MB blobs.\n" +
                "PLEASE NOTE: this program can take multiple hours to finish running and may need to run overnight.\n");
            // Create BlobServiceClient to create conatiner client
            BlobServiceClient blobServiceClient = new BlobServiceClient(Constants.connectionString);

            // Create container or get container for blob upload
            BlobContainerClient containerClient;
            try
            {
                containerClient = blobServiceClient.CreateBlobContainer(Constants.containerName);
            }
            catch
            {
                containerClient = blobServiceClient.GetBlobContainerClient(Constants.containerName);
            }

            int blobSize = 32000000;

            // Run SetUp 3 times with different blob size each time
            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine("\nGathering data for the upload time of 25 " + blobSize + " byte blobs...\nThis may take multiple hours to complete...\n");
                // Call to method to setup container in storage account for blob upload
                SetUp(
                    Constants.blobNameBase,
                    containerClient,
                    blobSize);

                blobSize = blobSize * 2;
            }
            

        }


        /*
         * A method to upload control blob with control storage transfer options
         */
        private static void SetUp
            (string blobName,
            BlobContainerClient contClient,
            int size)
        {
            Console.WriteLine("Getting control data...\n");
            // Create a local file to upload
            File.WriteAllBytes(blobName, new byte[size]);

            // Setting options for upload of control blob
            var options = new StorageTransferOptions { MaximumConcurrency = 1, InitialTransferSize = (1 * 1024 * 1024), MaximumTransferSize = (1 * 1024 * 1024) };

            // Create a stopwatch
            Stopwatch stopwatch = new Stopwatch();

            for (int i = 1; i <= 25; i++)
            {
                // Create blob client
                BlobClient blobClient = contClient.GetBlobClient(i + blobName);

                // Start stopwatch
                stopwatch.Start();
                // Upload blob to container using transfer options
                blobClient.Upload("./" + blobName, null, null, null, null, null, options, default);
                // Stop stopwatch
                stopwatch.Stop();

                // Delete blob
                blobClient.Delete();
            }

            // Get total time and set variables to track fastest and slowest upload times
            TimeSpan totalTime = stopwatch.Elapsed;
            TimeSpan slowest = totalTime;
            TimeSpan fastest = totalTime;

            // Call method to check trends of individual upload options
            MaxConcurrencyTrend(contClient, blobName);
            InitialTransferTrend(contClient, blobName);
            MaxTransferTrend(contClient, blobName);
            // Call method to upload the blob with all combinations
            CheckAllCombinations(slowest, fastest, contClient, blobName);

            // Delete file
            File.Delete("./" + blobName);

            // Reset the stopwatch
            stopwatch.Reset();

            // Output control data
            Console.WriteLine("Control data: max concurrency = 1, inital transfer size = 1*1024*1024, max transfer size = 1*1024*1024: " + totalTime);

            // Write data to a file
            File.WriteAllText("./mostEfficientOptionsCombination.txt", size + " bytes\n");
        }

        /*
         * A method to check the trends of changes in the max concurrency
         */
        private static void MaxConcurrencyTrend
            (BlobContainerClient contClient,
            String blobName)
        {
            Console.WriteLine("\nTiming upload using different max concurrency values...");

            // Create a matrix to save data to
            Object[,] concurrencyArray = new object[10, 2];
            concurrencyArray.SetValue("Max Concurrency", 0, 0);
            concurrencyArray.SetValue("Average Time", 0, 1);

            // Loop to test different maximum concurrency levels
            int location = 1;
            for (int concurrency = 2; concurrency <= 20; concurrency++)
            {
                // Setting options for upload
                var options = new StorageTransferOptions { MaximumConcurrency = concurrency, InitialTransferSize = (1 * 1024 * 1024), MaximumTransferSize = (1 * 1024 * 1024) };

                // Create a stopwatch
                Stopwatch stopwatch = new Stopwatch();

                for (int i = 1; i <= 25; i++)
                {
                    // Create blob client
                    BlobClient blobClient = contClient.GetBlobClient(i + blobName);

                    // Start stopwatch
                    stopwatch.Start();
                    // Upload blob to container using transfer options
                    blobClient.Upload("./" + blobName, null, null, null, null, null, options, default);
                    // Stop stopwatch
                    stopwatch.Stop();

                    // Delete blob
                    blobClient.Delete();
                }

                // Get total time
                TimeSpan totalTime = stopwatch.Elapsed;

                // Reset the stopwatch
                stopwatch.Reset();

                // Write data to a matrix
                concurrencyArray.SetValue(concurrency, location, 0);
                concurrencyArray.SetValue(totalTime / 25, location, 1);
                location++;
                
            }

            // Output data
            Console.Write(concurrencyArray[0, 0] + " " + concurrencyArray[0, 1] + "\n");
            string data = concurrencyArray[0, 0] + " " + concurrencyArray[0, 1] + "\n";
            for (int i = 1; i <= 9; i++)
            {
                Console.Write(concurrencyArray[i, 0] + "                 " + concurrencyArray[i, 1] + "\n");
                data = data + concurrencyArray[i, 0] + "                 " + concurrencyArray[i, 1] + "\n";
            }

            // Write data to a file
            File.WriteAllText("./mostEfficientOptionsCombination.txt", data + "\n");

            // Writing data to excel
            Excel.Application oXL;
            Excel._Workbook oWB;
            Excel._Worksheet oSheet;
            Excel.Range oRng;

            try
            {
                // Start excel and create excel workbook
                oXL = new Excel.Application();
                oWB = (Excel._Workbook)(oXL.Workbooks.Add(Missing.Value));
                oSheet = (Excel._Worksheet)oWB.ActiveSheet;

                // Fill excel sheet w array
                oSheet.get_Range("A1", "B10").Value2 = concurrencyArray;

                // Format excel sheet
                oSheet.get_Range("A1", "B1").Font.Bold = true;
                oSheet.get_Range("A1", "B1").VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                oRng = oSheet.get_Range("A1", "B1");
                oRng.EntireColumn.AutoFit();

                oXL.Visible = true;
            }catch (Exception theException)
            {
                String errorMessage;
                errorMessage = "Error: ";
                errorMessage = String.Concat(errorMessage, theException.Message);
                errorMessage = String.Concat(errorMessage, " Line: ");
                errorMessage = String.Concat(errorMessage, theException.Source);

                Console.WriteLine(errorMessage, "Error");
            }

        }


        /*
         * A method to check the trends of changes in the initial transfer size
         */
        private static void InitialTransferTrend
            (BlobContainerClient contClient,
            String blobName)
        {
            Console.WriteLine("\nTiming upload using different initial transfer size values...");

            // Create a matrix to save data to
            Object[,] initArray = new object[10, 2];
            initArray.SetValue("Initial Transfer Size", 0, 0);
            initArray.SetValue("Average Time", 0, 1);

            // Loop to test different maximum concurrency levels
            int location = 1;
            for (int init = (16 * 1024 * 1024); init <= (56 * 1024 * 1024); init = init + (16 * 1024 * 1024))
            {
                // Setting options for upload
                var options = new StorageTransferOptions { MaximumConcurrency = 1, InitialTransferSize = init, MaximumTransferSize = (10 * 1024 * 1024) };

                // Create a stopwatch
                Stopwatch stopwatch = new Stopwatch();

                for (int i = 1; i <= 25; i++)
                {
                    // Create blob client
                    BlobClient blobClient = contClient.GetBlobClient(i + blobName);

                    // Start stopwatch
                    stopwatch.Start();
                    // Upload blob to container using transfer options
                    blobClient.Upload("./" + blobName, null, null, null, null, null, options, default);
                    // Stop stopwatch
                    stopwatch.Stop();

                    // Delete blob
                    blobClient.Delete();
                }

                // Get total time
                TimeSpan totalTime = stopwatch.Elapsed;

                // Reset the stopwatch
                stopwatch.Reset();

                // Write data to a matrix
                initArray.SetValue(init, location, 0);
                initArray.SetValue(totalTime / 25, location, 1);
                location++;
            }

            // Output data
            // Output data
            Console.Write(initArray[0, 0] + "    " + initArray[0, 1] + "\n");
            string data = initArray[0, 0] + "    " + initArray[0, 1] + "\n";
            for (int i = 1; i <= 9; i++)
            {
                Console.Write(initArray[i, 0] + "                 " + initArray[i, 1] + "\n");
                data = data + initArray[0, 0] + "    " + initArray[0, 1] + "\n";
            }

            // Write data to a file
            File.WriteAllText("./mostEfficientOptionsCombination.txt", data + "\n");

            // Writing data to excel
            Excel.Application oXL;
            Excel._Workbook oWB;
            Excel._Worksheet oSheet;
            Excel.Range oRng;

            try
            {
                // Start excel and create excel workbook
                oXL = new Excel.Application();
                oWB = (Excel._Workbook)(oXL.Workbooks.Add(Missing.Value));
                oSheet = (Excel._Worksheet)oWB.ActiveSheet;

                // Fill excel sheet w array
                oSheet.get_Range("A1", "B10").Value2 = initArray;

                // Format excel sheet
                oSheet.get_Range("A1", "B1").Font.Bold = true;
                oSheet.get_Range("A1", "B1").VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                oRng = oSheet.get_Range("A1", "B1");
                oRng.EntireColumn.AutoFit();

                oXL.Visible = true;
            }
            catch (Exception theException)
            {
                String errorMessage;
                errorMessage = "Error: ";
                errorMessage = String.Concat(errorMessage, theException.Message);
                errorMessage = String.Concat(errorMessage, " Line: ");
                errorMessage = String.Concat(errorMessage, theException.Source);

                Console.WriteLine(errorMessage, "Error");
            }

        }


        /*
         * A method to check the trends of changes in the max transfer size
         */
        private static void MaxTransferTrend
            (BlobContainerClient contClient,
            String blobName)
        {
            Console.WriteLine("\nTiming upload using different max transfer size values...");

            // Create a matrix to save data to
            Object[,] maxTransferArray = new object[10, 2];
            maxTransferArray.SetValue("Max Transfer Size", 0, 0);
            maxTransferArray.SetValue("Average Time", 0, 1);

            // Loop to test different maximum concurrency levels
            int location = 1;
            for (int maxTransfer = (16 * 1024 * 1024); maxTransfer <= (56 * 1024 * 1024); maxTransfer = maxTransfer + (16 * 1024 * 1024))
            {
                // Setting options for upload
                var options = new StorageTransferOptions { MaximumConcurrency = 1, InitialTransferSize = (10 * 1024 * 1024), MaximumTransferSize = maxTransfer };

                // Create a stopwatch
                Stopwatch stopwatch = new Stopwatch();

                for (int i = 1; i <= 25; i++)
                {
                    // Create blob client
                    BlobClient blobClient = contClient.GetBlobClient(i + blobName);

                    // Start stopwatch
                    stopwatch.Start();
                    // Upload blob to container using transfer options
                    blobClient.Upload("./" + blobName, null, null, null, null, null, options, default);
                    // Stop stopwatch
                    stopwatch.Stop();

                    // Delete blob
                    blobClient.Delete();
                }

                // Get total time
                TimeSpan totalTime = stopwatch.Elapsed;

                // Reset the stopwatch
                stopwatch.Reset();

                // Write data to a matrix
                maxTransferArray.SetValue(maxTransfer, location, 0);
                maxTransferArray.SetValue(totalTime / 25, location, 1);
                location++;
            }

            // Output data
            Console.Write(maxTransferArray[0, 0] + "        " + maxTransferArray[0, 1] + "\n");
            string data = maxTransferArray[0, 0] + "        " + maxTransferArray[0, 1] + "\n";
            for (int i = 1; i <= 9; i++)
            {
                Console.Write(maxTransferArray[i, 0] + "                 " + maxTransferArray[i, 1] + "\n");
                data = data + maxTransferArray[0, 0] + "        " + maxTransferArray[0, 1] + "\n";
            }

            // Write data to a file
            File.WriteAllText("./mostEfficientOptionsCombination.txt", data + "\n");

            // Writing data to excel
            Excel.Application oXL;
            Excel._Workbook oWB;
            Excel._Worksheet oSheet;
            Excel.Range oRng;

            try
            {
                // Start excel and create excel workbook
                oXL = new Excel.Application();
                oWB = (Excel._Workbook)(oXL.Workbooks.Add(Missing.Value));
                oSheet = (Excel._Worksheet)oWB.ActiveSheet;

                // Fill excel sheet w array
                oSheet.get_Range("A1", "B10").Value2 = maxTransferArray;

                // Format excel sheet
                oSheet.get_Range("A1", "B1").Font.Bold = true;
                oSheet.get_Range("A1", "B1").VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                oRng = oSheet.get_Range("A1", "B1");
                oRng.EntireColumn.AutoFit();

                oXL.Visible = true;
            }
            catch (Exception theException)
            {
                String errorMessage;
                errorMessage = "Error: ";
                errorMessage = String.Concat(errorMessage, theException.Message);
                errorMessage = String.Concat(errorMessage, " Line: ");
                errorMessage = String.Concat(errorMessage, theException.Source);

                Console.WriteLine(errorMessage, "Error");
            }

        }


        /*
         * A method to upload blobs with different values for the storage transfer options.
         * This method outputs the most and least time efficient combination of those storage transfer options.
         */
        private static void CheckAllCombinations
            (TimeSpan slow,
            TimeSpan fast,
            BlobContainerClient contClient,
            String blobName)
        {
            Console.WriteLine("\nTiming upload using all combinations of upload options...");

            int slowConcurrency = 1;
            int fastConcurrency = 1;
            int slowInitialTransfer = (10 * 1024 * 1024);
            int fastInitialTransfer = (10 * 1024 * 1024);
            int slowMaximumTransfer = (10 * 1024 * 1024);
            int fastMaximumTransfer = (10 * 1024 * 1024);

            // Loop to test different initial trasnfer sizes
            for (int initialTransfer = (16 * 1024 * 1024); initialTransfer <= (56 * 1024 * 1024); initialTransfer = initialTransfer + (16 * 1024 * 1024))
            {
                // Loop to test different maximum transfer sizes
                for (int maximumTransfer = (16 * 1024 * 1024); maximumTransfer <= (56 * 1024 * 1024); maximumTransfer = maximumTransfer + (16 * 1024 * 1024))
                {
                    // Loop to test different maximum concurrency levels
                    for (int concurrency = 2; concurrency <= 20; concurrency++)
                    {
                        // Setting options for upload
                        var options = new StorageTransferOptions { MaximumConcurrency = concurrency, InitialTransferSize = initialTransfer, MaximumTransferSize = maximumTransfer };

                        // Create a stopwatch
                        Stopwatch stopwatch = new Stopwatch();

                        for (int i = 1; i <= 25; i++)
                        {
                            // Create blob client
                            BlobClient blobClient = contClient.GetBlobClient(i + blobName);

                            // Start stopwatch
                            stopwatch.Start();
                            // Upload blob to container using transfer options
                            blobClient.Upload("./" + blobName, null, null, null, null, null, options, default);
                            // Stop stopwatch
                            stopwatch.Stop();

                            // Delete blob
                            blobClient.Delete();
                        }

                        // Get total time
                        TimeSpan totalTime = stopwatch.Elapsed;

                        // Reset the stopwatch
                        stopwatch.Reset();

                        // Get fastest and slowest upload time
                        if (totalTime > slow)
                        {
                            slow = totalTime;
                            slowConcurrency = concurrency;
                            slowInitialTransfer = initialTransfer;
                            slowMaximumTransfer = maximumTransfer;
                        }
                        else if (totalTime < fast)
                        {
                            fast = totalTime;
                            fastConcurrency = concurrency;
                            fastInitialTransfer = initialTransfer;
                            fastMaximumTransfer = maximumTransfer;
                        }
                    }
                }
            }
            string data = "\nFastest Upload Settings\nMax Concurrency: " + fastConcurrency + " \nInitial Transfer Size: " + fastInitialTransfer + "\nMaximum Transfer Size: " + fastMaximumTransfer + "\nTotal Upload Time Elapsed: " + fast + "\nAverage Upload Time Elapsed: " + (fast / 25) +
                "\n\nSlowest Upload Settings\nMax Concurrency: " + slowConcurrency + "\nInitial Transfer Size: " + slowInitialTransfer + "\nMaximum Transfer Size: " + slowMaximumTransfer + "\nTotal Upload Time Elapsed: " + slow + "\nAverage Upload Time Elapsed: " + (slow / 25);

            // Output data
            Console.WriteLine(data);

            // Write data to a file
            File.WriteAllText("./mostEfficientOptionsCombination.txt", data);
        }
    }
}
