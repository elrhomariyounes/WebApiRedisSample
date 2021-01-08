using System.Collections.Generic;
using System.Threading.Tasks;
using WebApiRedisSample.Data.Models;

namespace WebApiRedisSample.Services.Interfaces
{
    public interface ITodoService
    {
        Task<IEnumerable<Todo>> GetTodos();
        Task<int> AddTodo(TodoViewModel model);

        Task<string> CacheTodos();
    }
}
