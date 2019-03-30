using UnityEngine;
using System.Collections.Generic;
using SimpleJSON;
using graphQLClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class ExampleQuery : MonoBehaviour
{
	[Tooltip("Name of the pokemon you want to query")]
	public string pokemonName = "Pikachu";

	[Tooltip("This is the query call that gives me Pikachu's details")]
	[TextArea]
	public string getPokemonDetails;

	public UnityEngine.UI.Text display;
	void Start()
	{
		GraphQuery.url = "https://graphql-pokemon.now.sh/";
		GetPikachuDetails(pokemonName);
	}


	public void GetPikachuDetails(string username)
	{
		GraphQuery.onQueryComplete += DisplayResult;
		GraphQuery.variable["name"] = pokemonName;
		GraphQuery.POST(getPokemonDetails);
	}

	public void DisplayResult()
	{
		Debug.Log(GraphQuery.queryReturn);
		string data = ParseData(GraphQuery.queryReturn, "pokemon");
		Pokemon pokemon = JsonConvert.DeserializeObject<Pokemon>(data);
		Debug.Log("The pokemon name: " + pokemon.attacks.special[1].damage);
		display.text = "Pokedex Number: " + pokemon.number + "\n Name: " + pokemon.name + "\n Evolve Form: " + pokemon.evolutions[0].name;
	}

	void OnDisable()
	{
		GraphQuery.onQueryComplete -= DisplayResult;
	}

	string ParseData(string query, string queryName){
		JObject obj = JsonConvert.DeserializeObject<JObject>(query);
		return JsonConvert.SerializeObject(obj["data"][queryName]);
	}

	public class Pokemon
	{
		public string name;
		public string number;
		public List<Evolution> evolutions;
		public Attack attacks;

		public class Evolution
		{
			public string name;
		}

		public class Attack
		{
			public List<Special> special;
			public class Special
			{
				public string name;
				public string type;
				public int damage;
			}
		}
	}
}
