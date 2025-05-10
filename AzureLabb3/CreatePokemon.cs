using AzureLabb3.Models;
using AzureLabb3.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

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
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "Pokemon")] HttpRequest req,
            string name,
            string type,
            int level
        )
        {
            _logger.LogInformation("Creating a new pokemon..");

            Pokemon pokemon = new Pokemon
            {
                Name = name,
                Type = type,
                Level = level,
            };
            var newPokemon = await _repo.AddAsync("Pokemon", pokemon);

            if (newPokemon != null)
            {
                var response = req.HttpContext.Response;
                response.StatusCode = StatusCodes.Status201Created;
                await response.WriteAsJsonAsync(newPokemon);
                _logger.LogInformation($"Pokemon created: {newPokemon.Name}");
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

       
    }
}
