using Dapper;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using WebApiRedisSample.Data.Models;
using WebApiRedisSample.Services.Interfaces;

namespace WebApiRedisSample.Services.Implementations
{
    public class TodoService : ITodoService
    {
        private readonly IConfiguration _configuration;
        private readonly string _dbConnectionString;
        private readonly ILogger<TodoService> _logger;
        private readonly IDistributedCache _cache;
        private TimeSpan? AbsoluteExpireTime => TimeSpan.FromSeconds(60);
        private TimeSpan? UnusedExpireTime => null;
        public TodoService(IConfiguration configuration, ILogger<TodoService> logger, IDistributedCache cache)
        {
            _configuration = configuration;
            _logger = logger;
            _dbConnectionString = _configuration.GetConnectionString("Postgres");
            _cache = cache;
        }

        public async Task<int> AddTodo(TodoViewModel model)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_configuration.GetConnectionString("Postgres")))
                {
                    var command = @"INSERT INTO public.todos (Content) VALUES (@Content);";
                    var rowsCount = await connection.ExecuteAsync(command, model);
                    _logger.LogInformation($"Inserted: {rowsCount}");
                    return rowsCount;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return default(int);
            }
        }

        public async Task<IEnumerable<Todo>> GetTodos()
        {
            var cacheKey = "WebApiRedisSample_" + DateTime.Now.ToString("yyyyMMdd_hhmm");
            var cacheData = await _cache.GetStringAsync(cacheKey);

            if(cacheData == null)
            {
                // Fetch from Postgres
                var todos = await FetchTodos();

                //Cache todos
                await CacheTodos(cacheKey, todos);

                return todos;
            }

            // Return cached todos
            return JsonSerializer.Deserialize<List<Todo>>(cacheData);
        }

        private async Task CacheTodos(string key, IEnumerable<Todo> todos)
        {
            var cacheOptions = new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = AbsoluteExpireTime,
                SlidingExpiration = UnusedExpireTime
            };

            var jsonData = JsonSerializer.Serialize(todos);
            await _cache.SetStringAsync(key, jsonData, cacheOptions);
        }

        private async Task<IEnumerable<Todo>> FetchTodos()
        {
            try
            {
                using (var connection = new NpgsqlConnection(_dbConnectionString))
                {
                    var command = @"SELECT * FROM public.todos;";
                    var todos = await connection.QueryAsync<Todo>(command);
                    return todos;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return default(IEnumerable<Todo>);
            }
        }
    }
}
