using Boo.Lang;
using GraphQlClient.Core;
using UnityEngine;
using UnityEngine.Networking;

namespace GraphQlClient.Core
{
    [CreateAssetMenu(fileName = "GraphApi", menuName = "GraphQlClient/Core/GraphApi")]
    public class GraphApi : ScriptableObject
    {
        public string url;

        public List<Query> queries = new List<Query>();
        public List<Mutation> mutations = new List<Mutation>();

        private string introspection =
            "{\n  __schema {\n    queryType {\n      name\n      fields{\n        name\n      }\n    }\n    mutationType{\n      name\n      fields{\n        name\n      }\n    }\n  }\n}";

        public async void Introspect(){
            UnityWebRequest request = await HttpHandler.PostAsync(url, introspection);
        }

        public void CreateNewQuery(){
            queries.Add(new Query());
        }

        public void DeleteQuery(int index){
            queries.RemoveAt(index);
        }

        #region Classes

        public class Query
        {
            public string queryName;
        }

        public class Mutation
        {
            public string mutationName;
        }

        #endregion
    }
}

