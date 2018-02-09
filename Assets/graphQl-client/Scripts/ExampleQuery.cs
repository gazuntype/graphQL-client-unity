using UnityEngine;
using SimpleJSON;
using graphQLClient;

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
		var N = JSON.Parse(GraphQuery.queryReturn);
		string name = N["data"]["pokemon"]["name"].Value;
		string number = N["data"]["pokemon"]["number"].Value;
		string evolution = N["data"]["pokemon"]["evolutions"][0]["name"].Value;

		display.text = "Pokedex Number: " + number + "\n Name: " + name + "\n Evolve Form: " + evolution;
	}

	void OnDisable()
	{
		GraphQuery.onQueryComplete -= DisplayResult;
	}
}
