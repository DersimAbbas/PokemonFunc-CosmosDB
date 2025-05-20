using System.Text.Json;
using AzureLabb3.Models;
using AzureLabb3.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace AzureLabb3
{
    public class PokemonCRUD
    {
        private readonly ILogger<PokemonCRUD> _logger;
        private readonly CosmosRepository _repo;

        public PokemonCRUD(ILogger<PokemonCRUD> logger, CosmosRepository repo)
        {
            _logger = logger;
            _repo = repo;
        }

        [Function("GetAllPokemons")]
        public async Task<IEnumerable<Pokemon>?> GetAllPokemonsRun(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "pokemons")] HttpRequest req
        )
        {
            _logger.LogInformation("Getting all pokemons..");
            var result = await _repo.GetAllAsync<Pokemon>("Pokemon");

            if (result == null)
            {
                var response = req.HttpContext.Response;
                response.StatusCode = StatusCodes.Status404NotFound;
                await response.WriteAsJsonAsync(new { message = "No pokemons found" });
                _logger.LogError("No pokemons found");
            }
            var httpResponse = req.HttpContext.Response;
            httpResponse.StatusCode = StatusCodes.Status200OK;
            await httpResponse.WriteAsJsonAsync(result);
            return result;
        }

        [Function("GetPokemonById")]
        public async Task<Pokemon?> GetPokemonByIdRun(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Pokemon/get/{id}")]
                HttpRequest req,
            int id
        )
        {
            _logger.LogInformation($"Getting pokemon with id: {id}");
            var result = await _repo.GetByIdAsync<Pokemon>("Pokemon", id);
            if (result == null)
            {
                var response = req.HttpContext.Response;
                response.StatusCode = StatusCodes.Status404NotFound;
                await response.WriteAsJsonAsync(new { message = "Pokemon not found" });
                _logger.LogError("Pokemon not found");
            }
            var httpResponse = req.HttpContext.Response;
            httpResponse.StatusCode = StatusCodes.Status200OK;
            await httpResponse.WriteAsJsonAsync(result);
            return result;
        }

        [Function("UpdatePokemon")]
        public async Task Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "put", Route = "pokemon/update/{id}")]
                HttpRequest req,
            int id,
            string statname,
            int statvalue
        )
        {
            _logger.LogInformation(
                $"UpdatePokemonStat invoked for ID={id}, stat={statname}, value={statvalue}"
            );

            var pokemon = await _repo.GetByIdAsync<Pokemon>("Pokemon", id);
            if (pokemon == null)
            {
                req.HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                await req.HttpContext.Response.WriteAsJsonAsync(
                    new { message = $"Pokemon with id={id} not found" }
                );
                return;
            }

            var entry = pokemon.Stats.FirstOrDefault(s =>
                string.Equals(s.Stat.Name, statname, StringComparison.OrdinalIgnoreCase)
            );
            if (entry == null)
            {
                req.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await req.HttpContext.Response.WriteAsJsonAsync(
                    new { message = $"Stat '{statname}' not found on this Pokémon" }
                );
                return;
            }

            entry.BaseStat = statvalue;

            var updated = await _repo.UpdateAsync<Pokemon>("Pokemon", id, pokemon);

            req.HttpContext.Response.StatusCode = StatusCodes.Status200OK;
            await req.HttpContext.Response.WriteAsJsonAsync(updated);
        }

        [Function("DeletePokemon")]
        public async Task<bool> DeletePokemonRun(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "get",
                "delete",
                Route = "pokemon/delete/{id}"
            )]
                HttpRequest req,
            int id
        )
        {
            _logger.LogInformation($"Deleting pokemon with id: {id}");
            var existingPokemon = await _repo.GetByIdAsync<Pokemon>("Pokemon", id);
            if (existingPokemon == null)
            {
                _logger.LogError($"Pokemon with id {id} not found");
                var nullResponse = req.HttpContext.Response;
                nullResponse.StatusCode = StatusCodes.Status404NotFound;
                await nullResponse.WriteAsJsonAsync(new { message = "Pokemon not found" });
            }
            var result = await _repo.DeleteAsync<Pokemon>("Pokemon", id);
            var response = req.HttpContext.Response;
            response.StatusCode = StatusCodes.Status200OK;
            await response.WriteAsJsonAsync(result);
            return result;
        }
    }
}
