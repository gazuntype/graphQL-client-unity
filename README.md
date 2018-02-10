# graphQL-client-unity
C# script to allow easy way of adding queries and mutations to a graphQL server.

## Motivation
While working on the API for one of my games, I used GraphQL. But when trying to communicate with this API from Unity, I found out it was quite difficult to make queries and mutations. It was also very difficult to insert variables as input parameters within this queries and mutations. Hence this graphQL client for Unity.

## How it works
This script leverages Unity's WWW class to make a POST request to the server's endpoint. Since it's a graph API, it only has one endpoint. So with the url of that endpoint, it sends a POST request. The body of the POST request is the query. Behind the scenes, some functions work on the query sent as the body of the POST request and this functions allow the use of variables and arrays as input parameters with the query or mutation. After the POST request is made, a coroutine is started which waits until the data returned from the query is collected and this is stored in a variable. There is also a delegate that functions can subscribe to so that when the data is received, the function is called and things can be done with the query data stored. [SimpleJSON](http://wiki.unity3d.com/index.php/SimpleJSON) or other methods can then be used to parse the JSON results gotten from the query.

## Example
Within this repository, there's a unitypackage that can be imported into your project and it has an example scene. The Unity project folder of the example is also available within the repository. This example does the basic functionality of querying the graph API created by [Lucas Bento](https://github.com/lucasbento) which is a server that gives information about different Pokemon and its stats [GraphQL Pokemon](https://github.com/lucasbento/graphql-pokemon). It allows you to display information about any Pokemon you put in the inspector in Unity. Then Parses the JSON result using [SimpleJSON](http://wiki.unity3d.com/index.php/SimpleJSON) and displays information about the Pokemon.
![Parsed Pokemon Results](https://imgur.com/a/xjkTY)

## How to use
* Import the graphQL-client-unity unitypackage
* In Assets->graphQL-client->Prefabs, drag the QueryController prefab into your scene.
* In a separate script, set the url of the endpoint by calling GraphQuery.url = "Endpoint_Url". Ensure this scripts using the namespace graphQLClient
* Have the query you want to call stored in a string either by writing it in a Textfield in the inspector or writing it directly in code.
* Create a function that will act as the callback when the result of the query is gotten. This function should subscribe to GraphQuery.onQueryComplete.
* Call GraphQuery.POST(your_query_string).
* Note: If your query has input variables or array, firstly place a "$" before the variable and "^" after it in the query string and be sure to assign them first by calling GraphQuery.variable["variable_name"] = the_actual_variable. If the input is an array, add "[]" after the variable in the query and call GraphQuery.array["array_name"] = the_actual_array.

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
Please help make this better by making pull requests and posting issues. There are still a lot of functionality we can add to make this a great graphQL client for Unity
