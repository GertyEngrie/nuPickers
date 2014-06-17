﻿
using System.Net;
using Newtonsoft.Json.Linq;

namespace nuComponents.DataTypes.Shared.JsonDataSource
{
    using nuComponents.DataTypes.Shared.Editor;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Xml;
    using System.Xml.XPath;
    using umbraco;

    public class JsonDataSource
    {
        public string JsonData { get; set; }

        public string Url { get; set; }

        public string OptionsJsonPath { get; set; }
        
        public string KeyJsonPath { get; set; }
        
        public string LabelJsonPath { get; set; }

        public IEnumerable<EditorDataItem> GetEditorDataItems(int contextId)
        {

            JObject jsonDoc;
            List<EditorDataItem> editorDataItems = new List<EditorDataItem>();

            switch (this.JsonData)
            {
               case "url":
                    jsonDoc = JObject.Parse(GetContents(this.Url));
                    
                    break;

                default:
                    jsonDoc = null;
                    break;
            }

            if (jsonDoc != null)
            { 
                
                HttpContext.Current.Items["pageID"] = contextId; // set here, as this is required for the uQuery.ResolveXPath

                var matching = jsonDoc.SelectTokens(OptionsJsonPath).GetEnumerator();
                List<string> keys = new List<string>(); // used to keep track of keys, so that duplicates aren't added

                string key;
                string label;

                while (matching.MoveNext())
                {
                    
                        key = matching.Current.SelectToken(this.KeyJsonPath).Value<string>();

                        // only add item if it has a unique key - failsafe
                        if (!string.IsNullOrWhiteSpace(key) && !keys.Any(x => x == key))
                        {
                            // TODO: ensure key doens't contain any commas (keys are converted saved as csv)
                            keys.Add(key); // add key so that it's not reused

                            // set default markup to use the configured label XPath
                            label= matching.Current.SelectToken(this.LabelJsonPath).Value<string>();
                            
                            editorDataItems.Add(new EditorDataItem()
                            {
                                Key = key,
                                Label = label
                            });
                        }
                    }
                }
            

            return editorDataItems;
        }

        private string GetContents(string url)
        {
            using (WebClient client = new WebClient())
            {
                return client.DownloadString(url);
            }
        }
    }
}
