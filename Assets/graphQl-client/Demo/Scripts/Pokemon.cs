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

    public async void GetPokemonDetails(){
        GraphApi.Query query = pokemonGraph.GetQueryByName("PokemonByName");
        string json = JsonConvert.SerializeObject(new{name = pokemonName.text});
        string args = GraphApi.JsonToArgument(json);
        query.SetArgs(args);
        UnityWebRequest request = await pokemonGraph.Post(query);
        Debug.Log(request.downloadHandler.text);
    }

    public async void GetAllPokemonDetails(){
        GraphApi.Query query = pokemonGraph.GetQueryByName("AllPokemon");
        string jsonInput = JsonConvert.SerializeObject(new{first = 100});
        string args = GraphApi.JsonToArgument(jsonInput);
        query.SetArgs(args);
        UnityWebRequest request = await pokemonGraph.Post(query);
        Debug.Log(request.downloadHandler.text);
    }
}
