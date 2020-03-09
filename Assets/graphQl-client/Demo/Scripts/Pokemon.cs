using System.Collections;
using System.Collections.Generic;
using GraphQlClient.Core;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Pokemon : MonoBehaviour
{
    public GraphApi pokemonGraph;
    public InputField pokemonName;

    public async void GetPokemonDetails(){
        GraphApi.Query query = pokemonGraph.GetQueryByName("PokemonByName");
        string args = $"name: \"{pokemonName.text}\"";
        query.SetArgs(args);
        UnityWebRequest request = await pokemonGraph.Post(query);
        Debug.Log(request.downloadHandler.text);
    }

    public async void GetAllPokemonDetails(){
        GraphApi.Query query = pokemonGraph.GetQueryByName("AllPokemon");
        string args = $"first: 100";
        query.SetArgs(args);
        UnityWebRequest request = await pokemonGraph.Post(query);
        Debug.Log(request.downloadHandler.text);
    }
}
