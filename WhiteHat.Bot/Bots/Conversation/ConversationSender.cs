using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Net.Http.Headers;
using WhiteHat.API.Model;

namespace WhiteHat.Bot.Bots.Conversation
{
    public class ConversationSender : IConversationSender
    {
        string _baseUrl = $"https://apim-bot-framework-resource.azure-api.net/v1/";
        string _controller = "Notes";
        string _sessionId;

        public ConversationSender(string sessionId)
        {
            _sessionId = sessionId;
        }

        public bool AddNote(string note)
        {
            using var client = GetHttpClient();
            NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
            queryString.Add("sessionId", $"{_sessionId}");
            queryString.Add("note", $"{note}");

            string path = $"{_controller}/addnote?{queryString}";

            HttpResponseMessage response = client.PostAsync(path, null).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(response.Content.ReadAsStringAsync().Result);
            }
            else
            {
                return true;
            }
        }

        public bool AddParticipants(string participants)
        {
            using var client = GetHttpClient();
            NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
            queryString.Add("sessionId", $"{_sessionId}");
            queryString.Add("participants", $@"{participants}");

            string path = $"{_controller}/addparticipants?{queryString}";

            HttpResponseMessage response = client.PostAsync(path, null).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(response.Content.ReadAsStringAsync().Result);
            }
            else
            {
                return true;
            }
        }

        public bool DeleteNode(int index)
        {
            using var client = GetHttpClient();
            NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);

            queryString.Add("sessionId", $"{_sessionId}");
            queryString.Add("index", $"{index}");
            string path = $"{_controller}/deletenote?{queryString}";

            HttpResponseMessage response = client.DeleteAsync(path).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(response.Content.ReadAsStringAsync().Result);
            }
            else
            {
                return true;
            }
        }

        public List<string> GetMinutesOfMeeting()
        {
            using var client = GetHttpClient();
            NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);

            queryString.Add("sessionId", $"{_sessionId}");
            string path = $"{_controller}/getmom?{queryString.ToString()}";

            HttpResponseMessage response = client.GetAsync(path).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(response.Content.ReadAsStringAsync().Result);
            }
            else
            {
                var mom = response.Content.ReadAsAsync<Minute>().Result;
                return mom.Notes;
            }
        }

        public bool SendMail()
        {
            using var client = GetHttpClient();
            NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);

            queryString.Add("sessionId", $"{_sessionId}");
            string path = $"{_controller}/emailmom?{queryString.ToString()}";

            HttpResponseMessage response = client.PostAsync(path, null).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(response.Content.ReadAsStringAsync().Result);
            }
            else
            {
                return true;
            }
        }

        private HttpClient GetHttpClient()
        {
            HttpClient client = new()
            {
                BaseAddress = new Uri(_baseUrl)
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }
    }
}