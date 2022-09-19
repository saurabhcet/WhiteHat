using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace EchoBotTest.Luis
{
    public class LuisHelper : IRecognizer
    {
        private readonly LuisRecognizer _recognizer;
        public LuisHelper()
        {
            //var service = new LuisService
            //{
            //    AppId = "hello",
            //    SubscriptionKey = "xyz",
            //    Region = "uswestcost",
            //    Version = "2.0"
            //};
            //var app = new LuisApplication(service);
            var app = new LuisApplication(applicationId: "a3eb8f84-ac1c-4b3c-bef8-c627d792a9c8", endpointKey: "d577dfd577c44e5188151db1c7192abe", endpoint: "https://luisappmombot.cognitiveservices.azure.com/");
            var regOptions = new LuisRecognizerOptionsV2(app)
            {
                IncludeAPIResults = true,
                PredictionOptions = new LuisPredictionOptions
                {
                    IncludeAllIntents = true,
                    IncludeInstanceData = true,
                }
            };

            _recognizer = new LuisRecognizer(regOptions);
        }

        public async Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            return await _recognizer.RecognizeAsync(turnContext, cancellationToken);
        }

        public async Task<T> RecognizeAsync<T>(ITurnContext turnContext, CancellationToken cancellationToken) where T : IRecognizerConvert, new()
        {
            return await _recognizer.RecognizeAsync<T>(turnContext, cancellationToken);
        }
    }
}