using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace WikiDataHelpDeskBot.WikiData
{
    public sealed class WikiDataQueryHelper
    {
        private WikiDataQueryHelper()
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.Add("User-Agent", "PostmanRuntime/7.20.1");
        }

        public async Task<string> GetPropertyId(string propertyName)
        {
            XmlDocument mainAnswerDoc = await GetAnswerAsXmlDocument(string.Format(getPropertyIdByNameUrl, propertyName));
            var mainProperty = getPropertyIdFromAnswerXml(mainAnswerDoc);
            if(mainProperty != null)
            {
                return mainProperty;
            }
            // olyan propertyt keresünk aminek az egyik szinonímája az adott szó
            else
            {
                XmlDocument answerDoc = await GetAnswerAsXmlDocument(string.Format(getPropertyAdByAlsoKnownAsUrl, propertyName));
                var property = getPropertyIdFromAnswerXml(answerDoc);
                if (property != null)
                    return property;
                else
                    throw new PropertyNotFoundException();
            }
        }

        public async Task<string> GetItemByLabelOrAlsoKnownAs(string itemName)
        {
            XmlDocument mainAnswerDoc = await GetAnswerAsXmlDocument(string.Format(getItemIdByLabelOrAlsoKnownAsUrl, itemName, itemName));
            return getPropertyIdFromAnswerXml(mainAnswerDoc);
        }

        private string getPropertyIdFromAnswerXml(XmlDocument doc)
        {
            try
            {
                var urlToProperty = doc?.LastChild?.LastChild?.FirstChild?.FirstChild?.InnerText;
                if (urlToProperty == null)
                    return null;
                var urlParts = urlToProperty.Split("entity/");
                return urlParts[1];
            }
            catch
            {
                return null;
            }
        }

        private async Task<XmlDocument> GetAnswerAsXmlDocument(string getUrl)
        {
            var answer = await client.GetAsync(getUrl);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(await answer.Content.ReadAsStringAsync());
            return doc;
        }

        private static readonly object padlock = new object();
        private readonly HttpClient client;
        private static string instanceOfPropertyId = "P31";
        private static string humenObjectId = "Q5";
        private const string getPropertyIdByNameUrl = "https://query.wikidata.org/sparql?query=SELECT ?property WHERE {{ ?property wikibase:propertyType ?propertyType . ?property rdfs:label ?propertyLabel. FILTER(?propertyLabel = \"{0}\"@en) .}}";
        private const string getPropertyAdByAlsoKnownAsUrl = "https://query.wikidata.org/sparql?query=SELECT DISTINCT ?property WHERE {{ ?property wikibase:propertyType ?propertyType. ?property skos:altLabel ?propertyAltLabel. FILTER(?propertyAltLabel = \"{0}\"@en) . SERVICE wikibase:label {{ bd:serviceParam wikibase:language \"en\". }} }}";
        private const string getItemIdByLabelOrAlsoKnownAsUrl = "https://query.wikidata.org/sparql?query=SELECT DISTINCT ?item WHERE {{ ?item wdt:P31 wd:Q55983715 . ?item rdfs:label ?itemLabel . ?item skos:altLabel ?propertyAltLabel. FILTER(?itemLabel = '{0}'@en || ?propertyAltLabel = '{1}'@en) SERVICE wikibase:label {{ bd:serviceParam wikibase:language \"en\"}}}}";

        private static WikiDataQueryHelper instance = null;
        public static WikiDataQueryHelper Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new WikiDataQueryHelper();
                    }
                    return instance;
                }
            }
        }
    }
}
