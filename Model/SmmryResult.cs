using System.Collections.Generic;

namespace LolResearchBot.Model
{
    public class SmmryResult
    {
        public string sm_api_message { get; set; }
        public string sm_api_character_count { get; set; }
        public string sm_api_content_reduced { get; set; }
        public string sm_api_title { get; set; }
        public string sm_api_content { get; set; }
        public string sm_api_limitation { get; set; }
        public int sm_api_error { get; set; }
        public List<string> sm_api_keyword_array { get; set; }

        public string SetErrorMessage(int error)
        {
            switch (error)
            {
                case 0:
                    return "Internal server problem";
                case 1:
                    return "Incorrect submission variables";
                case 2:
                    return "Intentional restriction (low credits/disabled API key/banned API key)";
                case 3:
                    return "Summarization error";
                default:
                    return "no error";
            }
        }
    }
}