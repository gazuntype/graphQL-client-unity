using System.Collections;
using System.Collections.Generic;
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
        string json = JsonConvert.SerializeObject(new{name = pokemonName.text});
        string args = GraphApi.JsonToArgument(json);
        query.SetArgs(args);
        UnityWebRequest request = await pokemonGraph.Post(query);
        displayText.text = HttpHandler.FormatJson(request.downloadHandler.text);
    }

    public async void GetAllPokemonDetails(){
        GraphApi.Query query = pokemonGraph.GetQueryByName("AllPokemon", GraphApi.Query.Type.Query);
        string jsonInput = JsonConvert.SerializeObject(new{first = 100});
        string args = GraphApi.JsonToArgument(jsonInput);
        query.SetArgs(args);
        UnityWebRequest request = await pokemonGraph.Post(query);
        displayText.text = HttpHandler.FormatJson(request.downloadHandler.text);
    }
}
