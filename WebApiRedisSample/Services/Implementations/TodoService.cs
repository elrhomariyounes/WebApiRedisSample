using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
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
        public TodoService(IConfiguration configuration, ILogger<TodoService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _dbConnectionString = _configuration.GetConnectionString("Postgres");
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
