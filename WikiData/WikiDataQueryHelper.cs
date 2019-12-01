﻿using System;
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

        public async Task<string> GetItemIdByLabelOrAlsoKnownAs(string itemName)
        {
            XmlDocument mainAnswerDoc = await GetAnswerAsXmlDocument(string.Format(getItemIdByLabelOrAlsoKnownAsUrl, itemName, itemName));
            return getPropertyIdFromAnswerXml(mainAnswerDoc);
        }

        public async Task<int> GetFilteredItemsNum(SearchParameters parameters)
        {
            var doc = await SendFilteresRequest(parameters, false);
            if (doc?.LastChild?.LastChild != null)
                return Int32.Parse(doc.LastChild.LastChild.InnerText);
            else
                return 0;
        }

        public async Task<List<string>> GetFilteredItemsLink(SearchParameters parameters)
        {
            var doc = await SendFilteresRequest(parameters, true);
            List<string> results = new List<string>();
            if (doc?.LastChild?.LastChild != null)
            {
                foreach (XmlNode result in doc.LastChild.LastChild.ChildNodes)
                    results.Add(result.InnerText);
            }
            return results;
        }

        public async Task<XmlDocument> SendFilteresRequest(SearchParameters parameters, bool listItems)
        {
            if (string.IsNullOrEmpty(parameters.InstanceOf))
                return null;
            var instanceOfId = await GetItemIdByLabelOrAlsoKnownAs(parameters.InstanceOf);
            string filterText = string.Empty;
            int i = 0;
            foreach (var filter in parameters.Filters)
            {
                var propertyId = await GetPropertyId(filter.Key);
                filterText += " " + String.Format(filterSkeleton, propertyId, "prop" + i, filter.Value);
                i++;
            }

            string requestUri;
            if(!listItems)
                requestUri = String.Format(getFilteredItemsCount, instanceOfId, filterText);
            else
                requestUri = String.Format(listItemsUri, instanceOfId, filterText);
            return await GetAnswerAsXmlDocument(requestUri);
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
        private const string getPropertyIdByNameUrl = "https://query.wikidata.org/sparql?query=SELECT ?property WHERE {{ ?property wikibase:propertyType ?propertyType . ?property rdfs:label ?propertyLabel. FILTER(ucase(?propertyLabel) = ucase(\"{0}\"@en)) .}}";
        private const string getPropertyAdByAlsoKnownAsUrl = "https://query.wikidata.org/sparql?query=SELECT DISTINCT ?property WHERE {{ ?property wikibase:propertyType ?propertyType. ?property skos:altLabel ?propertyAltLabel. FILTER(ucase(?propertyAltLabel) = ucase(\"{0}\"@en)) . SERVICE wikibase:label {{ bd:serviceParam wikibase:language \"en\". }} }}";
        private const string getItemIdByLabelOrAlsoKnownAsUrl = "https://query.wikidata.org/sparql?query=SELECT DISTINCT ?item WHERE {{ ?item wdt:P31 wd:Q55983715 . ?item rdfs:label ?itemLabel . ?item skos:altLabel ?propertyAltLabel. FILTER(ucase(?itemLabel) = ucase('{0}'@en) || ucase(?propertyAltLabel) = ucase('{1}'@en)) SERVICE wikibase:label {{ bd:serviceParam wikibase:language \"en\"}}}}";
        private const string getItemsByInstanceOf = "https://query.wikidata.org/sparql?query=SELECT DISTINCT ?item WHERE {{ ?item wdt:P31 wd:{0} . SERVICE wikibase:label {{ bd:serviceParam wikibase:language \"en\"}}}} LIMIT 100";
        private const string getFilteredItemsCount = "https://query.wikidata.org/sparql?query=SELECT (count(distinct ?item) as ?count) WHERE {{ ?item wdt:P31 wd:{0} . {1} SERVICE wikibase:label {{ bd:serviceParam wikibase:language \"en\" }}}}";
        private const string filterSkeleton = "?item wdt:{0} ?{1}. ?{1} rdfs:label ?{1}Label . FILTER(STRSTARTS(ucase(?{1}Label), ucase('{2}'))) .";
        private const string listItemsUri = "https://query.wikidata.org/sparql?query=SELECT DISTINCT ?item WHERE {{ ?item wdt:P31 wd:{0} . {1} SERVICE wikibase:label {{ bd:serviceParam wikibase:language \"en\" }}}}";

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
