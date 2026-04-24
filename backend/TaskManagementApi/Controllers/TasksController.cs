using Microsoft.AspNetCore.Mvc;

namespace TaskManagementApi.Controllers;

[ApiController]
[Route("[controller]")]
public class TasksController : ControllerBase
{
    [HttpGet]
    public IEnumerable<Task> Get()
    {
        return [];
    }
}
