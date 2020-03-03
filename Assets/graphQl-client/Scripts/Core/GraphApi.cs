using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQlClient.Core;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace GraphQlClient.Core
{
    [CreateAssetMenu(fileName = "GraphApi", menuName = "GraphQlClient/Core/GraphApi")]
    public class GraphApi : ScriptableObject
    {
        public string url;

        public List<Query> queries;

        public List<Mutation> mutations;
        
        private string introspection;

        private string introspectionQuery;
        
        public async void Introspect(){
            introspectionQuery =
                "{\n  __schema {\n      queryType {\n        fields{\n          name\n          type{\n            name\n            kind\n            ofType {\n              kind\n              name\n            }\n          }\n        }\n      }\n  }\n}";
            UnityWebRequest request = await HttpHandler.PostAsync(url, introspectionQuery);
            introspection = request.downloadHandler.text;
        }

        public void CreateNewQuery(){
            if (queries == null)
                queries = new List<Query>();
            Query query = new Query();
            JObject jObject = JObject.Parse(introspection);
            JArray jArray = (JArray) jObject["data"]["__schema"]["queryType"]["fields"];
            query.fields = new List<Field>();
            query.queryOptions = new List<string>();
            for (int i = 1; i < jArray.Count; i++){
                query.queryOptions.Add(jArray[i]["name"].ToString());
            }

            queries.Add(query);
        }
        
        public void GetQueryReturnType(Query query, int index){
            JObject jObject = JObject.Parse(introspection);
            JArray jArray = (JArray) jObject["data"]["__schema"]["queryType"]["fields"];
            string fieldName;
            switch (jArray[index+1]["type"]["kind"].ToString()){
                case "OBJECT":
                    fieldName = jArray[index+1]["type"]["name"].ToString();
                    break;
                case "LIST":
                    fieldName = jArray[index+1]["type"]["ofType"]["name"].ToString();
                    break;
                default:
                    fieldName = jArray[index+1]["type"]["name"].ToString();
                    break;
            }

            query.returnType = fieldName;
        }

        public Field CreateSubFields(Query query, string type){
            Field field = query.fields.Find((aField => aField.name == type));
            IntrospectType(query, type, field);
            return field;
        }

        private async void IntrospectType(Query query, string type, Field parent = null){
            string queryText =
                $"{{\n__type(name: \"{type}\"){{\n    name\n    fields{{\n      name\n      type{{\n        name\n            kind\n        ofType{{\n          name\n        }}\n      }}\n    }}\n  }}\n}}";
            UnityWebRequest request = await HttpHandler.PostAsync(url, queryText);
            string result = request.downloadHandler.text;
            JObject jObject = JObject.Parse(result);
            JArray jArray = (JArray) jObject["data"]["__type"]["fields"];
            for (int i = 0; i < jArray.Count; i++){
                string fieldType;
                switch (jArray[i]["type"]["kind"].ToString()){
                    case "OBJECT":
                        fieldType = jArray[i]["type"]["name"].ToString();
                        break;
                    case "LIST":
                        fieldType = jArray[i]["type"]["ofType"]["name"].ToString();
                        break;
                    case "NON_NULL":
                        fieldType = jArray[i]["type"]["ofType"]["name"].ToString();
                        break;
                    default:
                        fieldType = jArray[i]["type"]["name"].ToString();
                        break;
                }
                Field field = new Field{name = jArray[i]["name"].ToString(),type = fieldType, parent = parent, index = query.fields.Count};
                query.fields.Add(field);
            }
        }

        public void DeleteQuery(int index){
            queries.RemoveAt(index);
        }

        public void DeleteAllQueries(){
            queries = new List<Query>();
        }

        #region Classes

        [Serializable]
        public class Query
        {
            public string name;
            public string queryString;
            public string returnType;
            public List<string> queryOptions;
            public List<Field> fields;
        }

        public class Mutation
        {
            public string name;
        }

        public class Field
        {
            public int index;
            public string name;
            public string type;
            public Field parent;
            public List<Field> children;
        }

        #endregion
    }
}