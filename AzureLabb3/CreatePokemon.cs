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
    public class CreatePokemon
    {
        private readonly ILogger<CreatePokemon> _logger;
        private readonly CosmosRepository _repo;

        public CreatePokemon(ILogger<CreatePokemon> logger, CosmosRepository repo)
        {
            _logger = logger;
            _repo = repo;
        }

        [Function("CreatePokemon")]
        public async Task CreatePokemonRun(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "Pokemon")]
                HttpRequest req,
            string name,
            string type,
            int level
        )
        {
            _logger.LogInformation("Creating a new pokemon..");

            Pokemon pokemon = new Pokemon
            {
                pokemonId = await _repo.GetNextPokemonIdAsync(),
                Name = name,
                Type = type,
                Level = level,
            };
            var newPokemon = await _repo.AddAsync("Pokemon", pokemon);

            if (pokemon != null)
            {
                var response = req.HttpContext.Response;
                response.StatusCode = StatusCodes.Status201Created;
                await response.WriteAsJsonAsync(newPokemon);
                _logger.LogInformation($"Pokemon created: {pokemon.Name}");
            }
            else
            {
                var response = req.HttpContext.Response;
                response.StatusCode = StatusCodes.Status500InternalServerError;
                await response.WriteAsJsonAsync(new { message = "Failed to create Pokemon" });
                _logger.LogError("Failed to create Pokemon");
            }
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
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Pokemon/{id}")]
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
        public async Task<Pokemon?> UpdatePokemonRun(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "put", Route = "updatePokemon/{id}")]
                HttpRequest req,
            int id,
            string name,
            string type,
            int level
        )
        {
            _logger.LogInformation($"Updating pokemon with id: {id}");
            var existingPokemon = await _repo.GetByIdAsync<Pokemon>("Pokemon", id);
            if (existingPokemon == null)
            {
                _logger.LogError($"Pokemon with id {id} not found");
                var nullResponse = req.HttpContext.Response;
                nullResponse.StatusCode = StatusCodes.Status404NotFound;
                await nullResponse.WriteAsJsonAsync(new { message = "Pokemon not found" });
            }

            Pokemon updatedPokemon = new Pokemon
            {
                Id = existingPokemon.Id,
                pokemonId = existingPokemon.pokemonId,
                Name = name,
                Type = type,
                Level = level,
            };

            var result = await _repo.UpdateAsync<Pokemon>("Pokemon", id, updatedPokemon);
            var response = req.HttpContext.Response;
            response.StatusCode = StatusCodes.Status200OK;
            await response.WriteAsJsonAsync(result);
            return result;
        }

        [Function("DeletePokemon")]
        public async Task<bool> DeletePokemonRun(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "get",
                "delete",
                Route = "deletePokemon/{id}"
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
