using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApiRedisSample.Data.Models;
using WebApiRedisSample.Services.Interfaces;

namespace WebApiRedisSample.Controllers
{
    [ApiController]
    public class TodoController : ControllerBase
    {
        private readonly ITodoService _todoService;

        public TodoController(ITodoService todoService)
        {
            _todoService = todoService;
        }

        [HttpPost("todos")]
        public async Task<IActionResult> AddTodo([FromBody] TodoViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _todoService.AddTodo(model);
                if (result != default(int))
                    return Ok(result);

                return BadRequest(result);
            }

            return BadRequest();

        }

        [HttpGet("todos")]
        public async Task<IActionResult> GetTodos()
        {
            var result = await _todoService.GetTodos();
            if (result != default(IEnumerable<Todo>))
                return Ok(result);

            return BadRequest(result);
        }

        [HttpPost("cache")]
        public async Task<IActionResult> CacheTodos()
        {
            var result = await _todoService.CacheTodos();
            return Ok(result);
        }
    }
}
