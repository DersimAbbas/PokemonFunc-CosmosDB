using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AzureLabb3.Models;
using AzureLabb3.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureLabb3
{
    public class FetchPokemonAPI
    {
        private readonly ILogger<FetchPokemonAPI> _logger;
        private readonly CosmosRepository _repo;
        private readonly HttpClient? httpClient;

        public FetchPokemonAPI(
            ILogger<FetchPokemonAPI> logger,
            CosmosRepository repo,
            HttpClient httpclient
        )
        {
            _logger = logger;
            _repo = repo;
            httpClient = httpclient;
        }

        [Function("AddPokemon")]
        public async Task<Pokemon> Fetch(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "pokemon/add/{name}")]
                HttpRequest req,
            string name
        )
        {
            var pokeJson = await httpClient.GetFromJsonAsync<JsonElement>(
                $"https://pokeapi.co/api/v2/pokemon/{name}"
            );

            if (!pokeJson.TryGetProperty("id", out var idProp))
            {
                var resp = req.HttpContext.Response;
                resp.StatusCode = StatusCodes.Status404NotFound;
                await resp.WriteAsJsonAsync(new { message = "Pokemon not found" });
            }

            var pokemon = new Pokemon
            {
                pokemonId = idProp.GetInt32(),
                Name = pokeJson.GetProperty("name").GetString()!,

                Types = pokeJson
                    .GetProperty("types")
                    .EnumerateArray()
                    .Select(t => new Types
                    {
                        Type = new Models.Type
                        {
                            Name = t
                                .GetProperty("type")
                                .GetProperty("name")
                                .GetString()!
                        }
                    })
                    .ToList(),
                Abilities = pokeJson
                    .GetProperty("abilities")
                    .EnumerateArray()
                    .Select(a => new Abilities
                    {
                        Ability = new Ability
                        {
                            Name = a.GetProperty("ability")
                                    .GetProperty("name")
                                    .GetString()!
                        }
                    })
                    .ToList(),

                Height = pokeJson.GetProperty("height").GetInt32().ToString(),
                Weight = pokeJson.GetProperty("weight").GetInt32().ToString(),

                Stats = pokeJson
                    .GetProperty("stats")
                    .EnumerateArray()
                    .Select(s => new Stats
                    {
                        BaseStat = s.GetProperty("base_stat").GetInt32(),
                        Stat = new Stat
                        {
                            Name = s.GetProperty("stat").GetProperty("name").GetString()!,
                        },
                    })
                    .ToList(),
            };

            var speciesJson = await httpClient.GetFromJsonAsync<JsonElement>(
                $"https://pokeapi.co/api/v2/pokemon-species/{name}"
            );

            pokemon.Genus = speciesJson
                .GetProperty("genera")
                .EnumerateArray()
                .First(g => g.GetProperty("language").GetProperty("name").GetString() == "en")
                .GetProperty("genus")
                .GetString()!;

            var result = await _repo.AddAsync("Pokemon", pokemon);
            var response = req.HttpContext.Response;
            response.StatusCode = StatusCodes.Status200OK;
            await response.WriteAsJsonAsync(result);
            return result;
        }
    }
}
