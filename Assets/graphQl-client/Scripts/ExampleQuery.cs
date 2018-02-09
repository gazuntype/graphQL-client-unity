using UnityEngine;
using graphQLClient;

public class ExampleQuery : MonoBehaviour
{

	[Tooltip("This is the query call that gives me Pikachu's details")]
	[TextArea]
	public string getPokemonDetails;


	string pokemonName = "Pikachu";
	//Input parameters for both arrays and single variables.


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
	}

	void OnDisable()
	{
		GraphQuery.onQueryComplete -= DisplayResult;
	}
}
