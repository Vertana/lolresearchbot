using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Discord.Commands;
using Discord.WebSocket;
using LolResearchBot.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace LolResearchBot.Services
{
    public class SmmryService
    {
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _config;
        private readonly DiscordSocketClient _discord;
        private readonly HttpClient _http;

        public SmmryService(
            HttpClient http,
            DiscordSocketClient discord,
            CommandService commands,
            IConfigurationRoot config)
        {
            _config = config;
            _discord = discord;
            _commands = commands;
            _http = http;
        }

        private string _SM_API_KEY { get; set; }

        public string SM_API_KEY
        {
            get
            {
                _SM_API_KEY = _config["tokens:SMMRY"];
                return _SM_API_KEY;
            }
        }

        public async Task<string> GetSummaryAsync(string SM_URL)
        {
            var builder = new UriBuilder("https://api.smmry.com/");
            builder.Port = -1; //-1 utilizes the default port for the appropriate protocol. 443 in the case of https.
            var query = HttpUtility.ParseQueryString(builder.Query);
            query["SM_API_KEY"] =
                SM_API_KEY; // This needs to be the last variable attached to URL to avoid API issues. 
            query["SM_LENGTH"] = "5";
            query["SM_EXCLAMATION_AVOID"] = "";
            query["SM_IGNORE_LENGTH"] = "";
            query["SM_URL"] = SM_URL; // This needs to be the last variable attached to URL to avoid API issues.
            builder.Query = query.ToString();
            var url = builder.ToString();

            var resp = await _http.GetAsync(url).ConfigureAwait(false);
            var rawJson = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);

            var jsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            var smmry = JsonConvert.DeserializeObject<SmmryResult>(rawJson);

            if (smmry.sm_api_message != null || smmry.sm_api_error >= 1 && smmry.sm_api_error <= 3)
                return
                    "There was an error retrieving the summary. The article is probably either too short or the page is an unrecognizable format.";

            return $@"Title: {smmry.sm_api_title}.{Environment.NewLine}{Environment.NewLine}Summary: {
                    Truncate(smmry.sm_api_content)
                }.{Environment.NewLine}";
        }

        //Discord has a hard character limit of 2000 characters, so we truncate the summary just to be sure we are under the limit.
        public static string Truncate(string value)
        {
            var maxLength =
                1900; //1900 instead of 2000 to ensure there are enough character spaces to account for the other fields.
            if (string.IsNullOrEmpty(value) || maxLength == 0) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
}