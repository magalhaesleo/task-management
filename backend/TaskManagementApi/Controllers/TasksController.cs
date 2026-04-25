using Microsoft.AspNetCore.Mvc;
using TaskManagementApi.Tasks;

namespace TaskManagementApi.Controllers;

[ApiController]
[Route("[controller]")]
public class TasksController(ITaskService taskService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> Get(CancellationToken cancellationToken)
    {
        return Ok(await taskService.GetTasks(cancellationToken));
    }
    
    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var task = await taskService.GetById(id, cancellationToken);
        if (task is null)
            return NotFound();

        return Ok();
    }
    
    [HttpPost]
    public async Task<ActionResult> AddTask(
        [FromBody] AddTaskRequest taskRequest,
        CancellationToken cancellationToken)
    {
        var task = taskRequest.ToTask();
        await taskService.Add(task, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }
    
    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Toggle(
        [FromRoute] Guid id,
        [FromBody] bool completed,
        CancellationToken cancellationToken)
    {
        var result = await taskService.Toggle(id, completed, cancellationToken);
        if (result)
            return Ok();
        
        return NotFound();
    }
}
