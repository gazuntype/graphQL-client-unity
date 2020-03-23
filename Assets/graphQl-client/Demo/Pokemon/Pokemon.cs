using GraphQlClient.Core;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Pokemon : MonoBehaviour
{
    public GraphApi pokemonGraph;
    public InputField pokemonName;
    public Text displayText;

    public async void GetPokemonDetails(){
        GraphApi.Query query = pokemonGraph.GetQueryByName("PokemonByName", GraphApi.Query.Type.Query);
        query.SetArgs(new{name = pokemonName.text});
        UnityWebRequest request = await pokemonGraph.Post(query);
        displayText.text = HttpHandler.FormatJson(request.downloadHandler.text);
    }

    public async void GetAllPokemonDetails(){
        GraphApi.Query query = pokemonGraph.GetQueryByName("AllPokemon", GraphApi.Query.Type.Query);
        query.SetArgs(new{first = 100});
        UnityWebRequest request = await pokemonGraph.Post(query);
        displayText.text = HttpHandler.FormatJson(request.downloadHandler.text);
    }
}
