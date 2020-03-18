# graphQL-client-unity
graphQL-client-unity is, as the name implies, a graphQl client for the Unity game engine. Its major aim is to simplify the creation of graphQl queries and make graphql features like subscriptions as straightforward as possible.

## How it works
When imported, graphQl-client-unity allows the creation of queries within the Unity Editor. The client utilizes one of graphQl's amazing features, [**Introspection**](https://graphql.org/learn/introspection/), to get all the queries, mutations, subscriptions, types and arguments from the graphQl schema. With this information, an easy-to-use editor layout allows the user to create queries by simply selecting from the available options within the editor. Each query created is stored in a scriptable object and can be used as many times as needed by simple calling one line of code.
```C#
UnityWebRequest request = await exampleApi.Post("QueryName", GraphApi.Query.Type.Query);
```
The client also utilizes different Events that can be subscribed to by any function. The Events are called when a request is completed, when data is gotten from a subscription and other useful cases.

## How to use
### Create an API Reference
An API reference is a [Scriptable Object](https://docs.unity3d.com/Manual/class-ScriptableObject.html) that stores all the data relating to an API. For instance, if the API we intend to query is the [Pokemon GraphQl API](https://graphql-pokemon.now.sh/), within Unity, we would create an API Reference and point it to the url of the Pokemon GraphQl API. This API Reference will contain all the queries, mutations and subscriptions we wish to make pertaining to the Pokemon GraphQl API.

To create an API Reference, simply right click in your Asset folder, go to Create -> GraphQLClient -> API Reference. This would automatically create a new API Reference. Name it appropriately, put the url endpoint of the GraphQl API and click **Introspect** to  begin creating queries.

![Create an API Reference](Gifs/CreateApiReference.gif)

### Create a Query, Mutation or Subscription
To create a query, mutation or subscription is very intuitive and the processes are the same. Simply select Create Query (or Mutation, Subscription depending on your goal). Give the Query a name and pick the query from the dropdown menu displayed. After selecting the query you want to create, click confirm query and you can begin adding fields and subfields to the query.

![Create a Query](Gifs/CreateQuery.gif)

### Preview a Query
You can preview a query created to see how it looks as text. This is done simply by clicking the Preview Query button at the bottom of the query. Use the Edit Query button to go back to editing the query

![Create a Query](Gifs/PreviewQuery.gif)

### Using the API Reference
To use an API reference to actually query APIs, you need to reference it within a script
```C#
using GraphQlClient.Core;

public GraphApi pokemonReference;
```
This allows you to drag and drop the API reference into the public field created in the Inspector. With the reference, you can query the API easily using the Post function

```C#
public async void GetPokemons(){
	UnityWebRequest request = await pokemonReference.Post("GetAllPokemons", GraphApi.Query.Type.Query);
}
```
The Post function returns a [UnityWebRequest object](https://docs.unity3d.com/ScriptReference/Networking.UnityWebRequest.html) and data gotten from the UnityWebRequest object can be gotten by

```C#
string data = request.downloadHandler.text;
```
This data is in JSON format and can easily be parsed using a tool like Unity's in-built [JsonUtility class](https://docs.unity3d.com/ScriptReference/JsonUtility.html) or third party JSON parsers like [JSON. Net For Unity](https://assetstore.unity.com/packages/tools/input-management/json-net-for-unity-11347)



## Authentication/Authorization
You can set the Authorization header of your request to the token of your choice by simply ensureing the static variable GraphQuery.auth is given the value of that token. This would automatically set the Authorization header of the request to the token set.

```
query Pokemon
{
  pokemon(name: "$name^") {
    id
    number
    name
    attacks {
      special {
        name
        type
        damage
      }
    }
    evolutions {
      id
      number
      name
      weight {
        minimum
        maximum
      }
      attacks {
        fast {
          name
          type
          damage
        }
      }
    }
  }
}
```

In the above code block, $name^ is the variable that we want to input. Within your C# script you have to then,

```C#
public string pokemonName = "Pikachu";

public void GetPikachuDetails(string username)
	{
		GraphQuery.onQueryComplete += DisplayResult;
		GraphQuery.variable["name"] = pokemonName;
		GraphQuery.POST(getPokemonDetails);
	}
```

Another example which has the input as a particular type. This time, within the query, the $type$ is not put in quotation marks ("").

```
mutation CreateSession{
  newSession(type: $type$, name: "$sessionName^",username: "$username^" ){
    status
    name
    players{
      createrUsername
    }
    id
  }
}
```

The variable defintion in C# is written as

```C#
public string type = "SIMPLE";
public void Create(string type, string sessionName, string username)
	{
		GraphQuery.variable["sessionName"] = sessionName;
		GraphQuery.variable["username"] = username;
		GraphQuery.variable["type"] = type;
	}
```

For an array input, the query can look like below. Where [] is put after the name of the array.

```
query ParticularSession{
  session(id: #id[]^){
id
    name
    players{
      createrUsername
      joinedUsername
    }
  }
}
```

The C# defintion then becomes

```C#
public void ContinueGame(string[] id)
	{
		GraphQuery.array["id[]"] = id;
	}
```

* The result is then gotten from GraphQuery.queryReturn and parsed to get the information needed.
The function below is the function that was subscribed to GraphQuery.onQueryComplete when the POST was made.

```C#
public void DisplayResult()
	{
		Debug.Log(GraphQuery.queryReturn);
		var N = JSON.Parse(GraphQuery.queryReturn);
		string name = N["data"]["pokemon"]["name"].Value;
		string number = N["data"]["pokemon"]["number"].Value;
		string evolution = N["data"]["pokemon"]["evolutions"][0]["name"].Value;

		display.text = "Pokedex Number: " + number + "\n Name: " + name + "\n Evolve Form: " + evolution;
	}
```

## Building your own application
With the QueryController prefab in your scene, using the methods highlighted above, you can now make queries and mutations to a GraphAPI.

## Contributing
To contribute, you can open the Unity project itself and edit the GraphQuery.cs script in Assets->graphQL-client->Scripts. Please help make this better by making pull requests and posting issues. There are still a lot of functionality we can add to make this a great graphQL client for Unity. 
