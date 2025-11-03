using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LinuxAcademy.Azure.AI.OpticalCharacterRecognition
{
    class Program
    {
        static void Main(string[] args)
        {
            Program.RunAsync().Wait();
        }

        private static async Task RunAsync()
        {
            string endpoint = ; // insert your endpoint
            string key = ; // insert your key

            var cvc = new ComputerVisionClient(
                new ApiKeyServiceClientCredentials(key))
            {
                Endpoint = endpoint
            };

            var file = "image.PNG";
            using (var imageStream = File.OpenRead(file)) 
            {
                Console.WriteLine("Beginning processing...");
                // Start the async process to recognize the text
                var textHeaders = await cvc.BatchReadFileInStreamAsync(imageStream);

                const int numberOfCharsInOperationId = 36;
                var operationId = textHeaders.OperationLocation.Substring(
                    textHeaders.OperationLocation.Length - numberOfCharsInOperationId);

                var result = await cvc.GetReadOperationResultAsync(operationId);

                // Wait for the operation to complete
                var i = 0;
                var maxRetries = 10;
                while ((result.Status == TextOperationStatusCodes.Running ||
                        result.Status == TextOperationStatusCodes.NotStarted) && i++ < maxRetries) 
                { 
                    Console.WriteLine("Server status: {0}, waiting {1} seconds...", result.Status, i);
                    await Task.Delay(1000);

                    result = await cvc.GetReadOperationResultAsync(operationId);
                }
                
                foreach (var item in result.RecognitionResults) 
                { 
                    Console.WriteLine($"Result: {item.Width} {item.Height} {item.Lines.Count}");

                    foreach (Line line in item.Lines) 
                    {
                        line.BoundingBox.ToList().ForEach(bbi => Console.Write($"{bbi} "));
                        Console.WriteLine($"{line.Text}");
                    }
                }

            }
        }
    }
}
