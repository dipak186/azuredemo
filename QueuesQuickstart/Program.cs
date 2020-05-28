using System;
using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using System.Threading.Tasks;
namespace QueuesQuickstart
{
    class Program
    {
        

        static async Task Main(string[] args)
        {
            //Storage Access Connection String
            string connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");

            //CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            //CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            //CloudQueue queue = queueClient.GetQueueReference("mystoragequeue");


            // Create a unique name for the queue
            string queueName = "quickstartqueues-" + Guid.NewGuid().ToString();

            Console.WriteLine($"Creating queue: {queueName}");

            // Instantiate a QueueClient which will be
            // used to create and manipulate the queue
            QueueClient queueClient = new QueueClient(connectionString, queueName);

            // Create the queue
            await queueClient.CreateAsync();

            Console.WriteLine("\nAdding messages to the queue...");

            // Send several messages to the queue
            await queueClient.SendMessageAsync("First message");
            await queueClient.SendMessageAsync("Second message");

            // Save the receipt so we can update this message later
            SendReceipt receipt = await queueClient.SendMessageAsync("Third message");

            Console.WriteLine("\nPeek at the messages in the queue...");

            // Peek at messages in the queue
            PeekedMessage[] peekedMessages = await queueClient.PeekMessagesAsync(maxMessages: 10);

            foreach (PeekedMessage peekedMessage in peekedMessages)
            {
                // Display the message
                Console.WriteLine($"Message: {peekedMessage.MessageText}");
            }
            Console.WriteLine("\nUpdating the third message in the queue...");

            // Update a message using the saved receipt from sending the message
            await queueClient.UpdateMessageAsync(receipt.MessageId, receipt.PopReceipt, "Third message has been updated");

            Console.WriteLine("\nReceiving messages from the queue...");

            // Get messages from the queue
            QueueMessage[] messages = await queueClient.ReceiveMessagesAsync(maxMessages: 10);

            Console.WriteLine("\nPress Enter key to 'process' messages and delete them from the queue...");
            Console.ReadLine();

            // Process and delete messages from the queue
            foreach (QueueMessage message in messages)
            {
                // "Process" the message
                Console.WriteLine($"Message: {message.MessageText}");

                // Let the service know we're finished with
                // the message and it can be safely deleted.
                await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);
            }
            Console.WriteLine("\nPress Enter key to delete the queue...");
            Console.ReadLine();

            // Clean up
            Console.WriteLine($"Deleting queue: {queueClient.Name}");
            await queueClient.DeleteAsync();

            Console.WriteLine("Done");
        }
    }
}
