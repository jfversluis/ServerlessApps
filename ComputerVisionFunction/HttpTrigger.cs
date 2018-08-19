using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace ComputerVisionFunction
{
	public static class HttpTrigger
	{
		[FunctionName("HttpTrigger")]
		public async static Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequest req, TraceWriter log)
		{
			log.Info("Function invoked");

			// We only support POST requests
			if (req.Method == "GET")
				return new BadRequestResult();
			
			// grab the key and URI from the portal config
			string visionKey = Environment.GetEnvironmentVariable("VisionKey");

			// create a client and request Tags for the image submitted
			var vsc = new ComputerVisionClient(new ApiKeyServiceClientCredentials(visionKey));
			ImageDescription result = null;

			// We read the content as a byte array and assume it's an image
			if (req.Method == "POST")
			{
				try
				{
					result = await vsc.DescribeImageInStreamAsync(req.Body);
				}
				catch { }
			}

			// if we didn't get a result from the service, return a 400
			if (result == null)
				return new BadRequestResult();

			var bestResult = result.Captions.OrderByDescending(c => c.Confidence).FirstOrDefault()?.Text;

			return new OkObjectResult(bestResult
				?? "I'm at a loss for words... I can't describe this image!");
		}
	}
}