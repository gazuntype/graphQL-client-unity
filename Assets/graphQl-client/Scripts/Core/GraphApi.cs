using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace GraphQlClient.Core
{
    [CreateAssetMenu(fileName = "Api Reference", menuName = "GraphQlClient/Api Reference")]
    public class GraphApi : ScriptableObject
    {
        public string url;

        public List<Query> queries;

        public List<Query> mutations;

        public List<Query> subscriptions;
        
        private string introspection;
        
        public Introspection.SchemaClass schemaClass;

        private string authToken;
        
        private string queryEndpoint;
        private string mutationEndpoint;
        private string subscriptionEndpoint;
        
        private UnityWebRequest request;

        public bool loading;

        public void SetAuthToken(string auth){
            authToken = auth;
        }
        public Query GetQueryByName(string queryName, Query.Type type){
            List<Query> querySearch;
            switch (type){
                case Query.Type.Mutation:
                    querySearch = mutations;
                    break;
                case Query.Type.Query:
                    querySearch = queries;
                    break;
                case Query.Type.Subscription:
                    querySearch = subscriptions;
                    break;
                default:
                    querySearch = queries;
                    break;
            }
            return querySearch.Find(aQuery => aQuery.name == queryName);
        }

        public async Task<UnityWebRequest> Post(Query query){
            if (String.IsNullOrEmpty(query.query))
                query.CompleteQuery();
            return await HttpHandler.PostAsync(url, query.query, authToken);
        }

        public async Task<UnityWebRequest> Post(string queryString){
            return await HttpHandler.PostAsync(url, queryString, authToken);
        }

        public async Task<UnityWebRequest> Post(string queryName, Query.Type type){
            Query query = GetQueryByName(queryName, type);
            return await Post(query);
        }

        public async Task<ClientWebSocket> Subscribe(Query query, string socketId = "1", string protocol = "graphql-ws"){
            if (String.IsNullOrEmpty(query.query))
                query.CompleteQuery();
            return await HttpHandler.WebsocketConnect(url, query.query, authToken, socketId, protocol);
        }

        public async Task<ClientWebSocket> Subscribe(string queryName, Query.Type type, string socketId = "1", string protocol = "graphql-ws"){
            Query query = GetQueryByName(queryName, type);
            return await Subscribe(query, socketId, protocol);
        }

        public async void CancelSubscription(ClientWebSocket cws, string socketId = "1"){
            await HttpHandler.WebsocketDisconnect(cws, socketId);
        }
        

        #region Utility

        private static string JsonToArgument(string jsonInput){
            char[] jsonChar = jsonInput.ToCharArray();
            List<int> indexes = new List<int>();
            jsonChar[0] = ' ';
            jsonChar[jsonChar.Length - 1] = ' ';
            for (int i = 0; i < jsonChar.Length; i++){
                if (jsonChar[i] == '\"'){
                    if (indexes.Count == 2)
                        indexes = new List<int>();
                    indexes.Add(i);
                }

                if (jsonChar[i] == ':'){
                    jsonChar[indexes[0]] = ' ';
                    jsonChar[indexes[1]] = ' ';
                }
            }

            string result = new string(jsonChar);
            return result;
        }

        #endregion

#if UNITY_EDITOR

        #region Editor Use
        
        //Todo: Put schema file in proper location
        public async void Introspect(){
            loading = true;
            request = await HttpHandler.PostAsync(url, Introspection.schemaIntrospectionQuery, authToken);
            EditorApplication.update += HandleIntrospection;
        }

        void HandleIntrospection(){
            if (!request.isDone)
                return;
            EditorApplication.update -= HandleIntrospection;
            introspection = request.downloadHandler.text;
            File.WriteAllText(Application.dataPath + $"{Path.DirectorySeparatorChar}{name}schema.txt",introspection);
            schemaClass = JsonConvert.DeserializeObject<Introspection.SchemaClass>(introspection);
            if (schemaClass.data.__schema.queryType != null)
                queryEndpoint = schemaClass.data.__schema.queryType.name;
            if (schemaClass.data.__schema.mutationType != null)
                mutationEndpoint = schemaClass.data.__schema.mutationType.name;
            if (schemaClass.data.__schema.subscriptionType != null)
                subscriptionEndpoint = schemaClass.data.__schema.subscriptionType.name;
            loading = false;
        }

        public void GetSchema(){
            if (schemaClass == null){
                try{
                    introspection = File.ReadAllText(Application.dataPath + $"{Path.DirectorySeparatorChar}{name}schema.txt");
                }
                catch{
                    return;
                }
                
                schemaClass = JsonConvert.DeserializeObject<Introspection.SchemaClass>(introspection);
                if (schemaClass.data.__schema.queryType != null)
                    queryEndpoint = schemaClass.data.__schema.queryType.name;
                if (schemaClass.data.__schema.mutationType != null)
                    mutationEndpoint = schemaClass.data.__schema.mutationType.name;
                if (schemaClass.data.__schema.subscriptionType != null)
                    subscriptionEndpoint = schemaClass.data.__schema.subscriptionType.name;
            }
        }
        
        

        public void CreateNewQuery(){
            GetSchema();
            if (queries == null)
                queries = new List<Query>();
            Query query = new Query{fields = new List<Field>(), queryOptions = new List<string>(), type = Query.Type.Query};
            
            Introspection.SchemaClass.Data.Schema.Type queryType = schemaClass.data.__schema.types.Find((aType => aType.name == queryEndpoint));
            for (int i = 0; i < queryType.fields.Count; i++){
                query.queryOptions.Add(queryType.fields[i].name);
            }

            queries.Add(query);
        }

        public void CreateNewMutation(){
            GetSchema();
            if (mutations == null)
                mutations = new List<Query>();
            Query mutation = new Query{fields = new List<Field>(), queryOptions = new List<string>(), type = Query.Type.Mutation};
            
            Introspection.SchemaClass.Data.Schema.Type mutationType = schemaClass.data.__schema.types.Find((aType => aType.name == mutationEndpoint));
            if (mutationType == null){
                Debug.Log("No mutations");
                return;
            }
            for (int i = 0; i < mutationType.fields.Count; i++){
                mutation.queryOptions.Add(mutationType.fields[i].name);
            }

            mutations.Add(mutation);
        }
        
        public void CreateNewSubscription(){
            GetSchema();
            if (subscriptions == null)
                subscriptions = new List<Query>();
            Query subscription = new Query{fields = new List<Field>(), queryOptions = new List<string>(), type = Query.Type.Subscription};
            
            Introspection.SchemaClass.Data.Schema.Type subscriptionType = schemaClass.data.__schema.types.Find((aType => aType.name == subscriptionEndpoint));
            if (subscriptionType == null){
                Debug.Log("No subscriptions");
                return;
            }
            for (int i = 0; i < subscriptionType.fields.Count; i++){
                subscription.queryOptions.Add(subscriptionType.fields[i].name);
            }

            subscriptions.Add(subscription);
        }

        public void EditQuery(Query query){
            query.isComplete = false;
        }

        

        public bool CheckSubFields(string typeName){
            Introspection.SchemaClass.Data.Schema.Type type = schemaClass.data.__schema.types.Find((aType => aType.name == typeName));
            if (type?.fields == null || type.fields.Count == 0){
                return false;
            }

            return true;
        }

        //ToDo: Do not allow addition of subfield that already exists
        public void AddField(Query query, string typeName, Field parent = null){
            Introspection.SchemaClass.Data.Schema.Type type = schemaClass.data.__schema.types.Find((aType => aType.name == typeName));
            List<Introspection.SchemaClass.Data.Schema.Type.Field> subFields = type.fields;
            int parentIndex = query.fields.FindIndex(aField => aField == parent);
            List<int> parentIndexes = new List<int>();
            if (parent != null){
                parentIndexes = new List<int>(parent.parentIndexes){parentIndex};
            }
            Field fielder = new Field{parentIndexes = parentIndexes};
            
            foreach (Introspection.SchemaClass.Data.Schema.Type.Field field in subFields){
                fielder.possibleFields.Add((Field)field);
            }

            if (fielder.parentIndexes.Count == 0){
                query.fields.Add(fielder);
            }
            else{

                int index;
                index = query.fields.FindLastIndex(aField =>
                    aField.parentIndexes.Count > fielder.parentIndexes.Count &&
                    aField.parentIndexes.Contains(fielder.parentIndexes.Last()));

                if (index == -1){
                    index = query.fields.FindLastIndex(aField =>
                        aField.parentIndexes.Count > fielder.parentIndexes.Count &&
                        aField.parentIndexes.Last() == fielder.parentIndexes.Last());
                }

                if (index == -1){
                    index = fielder.parentIndexes.Last();
                }

                index++;
                query.fields[parentIndex].hasChanged = false;
                query.fields.Insert(index, fielder);
            }
        }

        private string GetFieldType(Introspection.SchemaClass.Data.Schema.Type.Field field){
            Field newField = (Field)field;
            return newField.type;
        }
        


        public void GetQueryReturnType(Query query, string queryName){
            string endpoint;
            switch (query.type){
                case Query.Type.Query:
                    endpoint = queryEndpoint;
                    break;
                case Query.Type.Mutation:
                    endpoint = mutationEndpoint;
                    break;
                case Query.Type.Subscription:
                    endpoint = subscriptionEndpoint;
                    break;
                default:
                    endpoint = queryEndpoint;
                    break;
            }
            Introspection.SchemaClass.Data.Schema.Type queryType =
                schemaClass.data.__schema.types.Find((aType => aType.name == endpoint));
            Introspection.SchemaClass.Data.Schema.Type.Field field =
                queryType.fields.Find((aField => aField.name == queryName));

            query.returnType = GetFieldType(field);
        }

        public void DeleteQuery(List<Query> query, int index){
            query.RemoveAt(index);
        }

        public void DeleteAllQueries(){
            queries = new List<Query>();
            mutations = new List<Query>();
            subscriptions = new List<Query>();
        }

        #endregion
        
#endif
        
        #region Classes

        [Serializable]
        public class Query
        {
            public string name;
            public Type type;
            public string query;
            public string queryString;
            public string returnType;
            private string args;
            public List<string> queryOptions;
            public List<Field> fields;
            public bool isComplete;

            public enum Type
            {
                Query,
                Mutation,
                Subscription
            }
            public void SetArgs(object inputObject){
                string json = JsonConvert.SerializeObject(inputObject, new EnumInputConverter());
                args = JsonToArgument(json);
                CompleteQuery();
            }

            public void SetArgs(string inputString){
                args = inputString;
                CompleteQuery();
            }
            
            

            public void CompleteQuery(){
                isComplete = true;
                string data = null;
                string parent = null;
                Field previousField = null;
                for (int i = 0; i < fields.Count; i++){
                    Field field = fields[i];
                    if (field.parentIndexes.Count == 0){
                        if (parent == null){
                            data += $"\n{GenerateStringTabs(field.parentIndexes.Count + 2)}{field.name}";
                        }
                        else{
                            int count = previousField.parentIndexes.Count - field.parentIndexes.Count;
                            while (count > 0){
                                data += $"\n{GenerateStringTabs(count + 1)}}}";
                                count--;
                            }

                            data += $"\n{GenerateStringTabs(field.parentIndexes.Count + 2)}{field.name}";
                            parent = null;

                        }

                        previousField = field;
                        continue;
                    }

                    if (fields[field.parentIndexes.Last()].name != parent){

                        parent = fields[field.parentIndexes.Last()].name;

                        if (fields[field.parentIndexes.Last()] == previousField){

                            data += $"{{\n{GenerateStringTabs(field.parentIndexes.Count + 2)}{field.name}";
                        }
                        else{
                            int count = previousField.parentIndexes.Count - field.parentIndexes.Count;
                            while (count > 0){
                                data += $"\n{GenerateStringTabs(count + 1)}}}";
                                count--;
                            }

                            data += $"\n{GenerateStringTabs(field.parentIndexes.Count + 2)}{field.name}";
                        }

                        previousField = field;

                    }
                    else{
                        data += $"\n{GenerateStringTabs(field.parentIndexes.Count + 2)}{field.name}";
                        previousField = field;
                    }

                    if (i == fields.Count - 1){
                        int count = previousField.parentIndexes.Count;
                        while (count > 0){
                            data += $"\n{GenerateStringTabs(count + 1)}}}";
                            count--;
                        }
                    }

                }

                string arg = String.IsNullOrEmpty(args) ? "" : $"({args})";
                string word;
                switch (type){
                    case Type.Query:
                        word = "query";
                        break;
                    case Type.Mutation:
                        word = "mutation";
                        break;
                    case Type.Subscription:
                        word = "subscription";
                        break;
                    default:
                        word = "query";
                        break;
                }
                query = data == null
                    ? $"{word} {name}{{\n{GenerateStringTabs(1)}{queryString}{arg}\n}}"
                    : $"{word} {name}{{\n{GenerateStringTabs(1)}{queryString}{arg}{{{data}\n{GenerateStringTabs(1)}}}\n}}";
            }

            private string GenerateStringTabs(int number){
                string result = "";
                for (int i = 0; i < number; i++){
                    result += "    ";
                }

                return result;
            }
        }

        [Serializable]
        public class Field
        {
            public int index;
            public int Index{
                get => index;
                set{
                    type = possibleFields[value].type;
                    name = possibleFields[value].name;
                    if (value != index)
                        hasChanged = true;
                    index = value;
                    
                }
            }

            public string name;
            public string type;
            public List<int> parentIndexes;
            public bool hasSubField;
            public List<PossibleField> possibleFields;

            public bool hasChanged;

            public Field(){
                possibleFields = new List<PossibleField>();
                parentIndexes = new List<int>();
                index = 0;
            }
            
            public void CheckSubFields(Introspection.SchemaClass schemaClass){
                Introspection.SchemaClass.Data.Schema.Type t = schemaClass.data.__schema.types.Find((aType => aType.name == type));
                if (t.fields == null || t.fields.Count == 0){
                    hasSubField = false;
                    return;
                }

                hasSubField = true;
            }
            
            [Serializable]
            public class PossibleField
            {
                public string name;
                public string type;
                
                public static implicit operator PossibleField(Field field){
                    return new PossibleField{name = field.name, type = field.type};
                }
            }
            public static explicit operator Field(Introspection.SchemaClass.Data.Schema.Type.Field schemaField){
                Introspection.SchemaClass.Data.Schema.Type ofType = schemaField.type;
                string typeName;
                do{
                    typeName = ofType.name;
                    ofType = ofType.ofType;
                } while (ofType != null);
                return new Field{name = schemaField.name, type = typeName};
            }
        }

        #endregion
    }



    public class EnumInputConverter : StringEnumConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer){
            if (value == null){
                writer.WriteNull();
            }
            else{
                Enum @enum = (Enum) value;
                string enumText = @enum.ToString("G");
                writer.WriteRawValue(enumText);
            }
        }
    }
}
