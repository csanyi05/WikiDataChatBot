// <auto-generated>
// Code generated by LUISGen C:\Users\Lenovo\Documents\suli2019\SZA_hazi\WikiDataHelpdeskBot\TutorialBot\CognitiveModels\WikiDataHelpDeskBot.json -cs Luis.WikiDataHelpDesk -o C:\Users\Lenovo\Documents\suli2019\SZA_hazi\WikiDataHelpdeskBot\TutorialBot\CognitiveModels
// Tool github: https://github.com/microsoft/botbuilder-tools
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
namespace WikiDataHelpDeskBot.CognitiveModels
{
    public partial class WikiDataHelpDesk: IRecognizerConvert
    {
        [JsonProperty("text")]
        public string Text;

        [JsonProperty("alteredText")]
        public string AlteredText;

        public enum Intent {
            Cancel, 
            Filter, 
            List, 
            None, 
            StartSearch
        };
        [JsonProperty("intents")]
        public Dictionary<Intent, IntentScore> Intents;

        public class _Entities
        {
            // Simple entities
            public string[] AttributeName;

            public string[] AttributeValue;

            public string[] InstanceOf;

            // Built-in entities
            public DateTimeSpec[] datetime;

            // Lists
            public string[][] CommonAttributes;

            // Instance
            public class _Instance
            {
                public InstanceData[] AttributeName;
                public InstanceData[] AttributeValue;
                public InstanceData[] CommonAttributes;
                public InstanceData[] InstanceOf;
                public InstanceData[] datetime;
            }
            [JsonProperty("$instance")]
            public _Instance _instance;
        }
        [JsonProperty("entities")]
        public _Entities Entities;

        [JsonExtensionData(ReadData = true, WriteData = true)]
        public IDictionary<string, object> Properties {get; set; }

        public void Convert(dynamic result)
        {
            var app = JsonConvert.DeserializeObject<WikiDataHelpDesk>(JsonConvert.SerializeObject(result, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
            Text = app.Text;
            AlteredText = app.AlteredText;
            Intents = app.Intents;
            Entities = app.Entities;
            Properties = app.Properties;
        }

        public (Intent intent, double score) TopIntent()
        {
            Intent maxIntent = Intent.None;
            var max = 0.0;
            foreach (var entry in Intents)
            {
                if (entry.Value.Score > max)
                {
                    maxIntent = entry.Key;
                    max = entry.Value.Score.Value;
                }
            }
            return (maxIntent, max);
        }
    }
}