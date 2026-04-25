using Microsoft.AspNetCore.Mvc;
using TaskManagementApi.Infrastructure;

namespace TaskManagementApi.Controllers;

[ApiController]
[Route("[controller]")]
public class TasksController(ITaskRepository taskRepository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> Get(CancellationToken cancellationToken)
    {
        return Ok(await taskRepository.GetTasks(cancellationToken));
    }
    
    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var task = await taskRepository.GetById(id, cancellationToken);
        if (task is null)
            return NotFound($"Task with ID {id} not found.");

        return Ok(task);
    }
    
    [HttpPost]
    public async Task<ActionResult> AddTask(
        [FromBody] Tasks.AddTaskRequest taskRequest,
        CancellationToken cancellationToken)
    {
        var task = taskRequest.ToTask();
        await taskRepository.Add(task, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }
    
    [HttpPatch("{id:guid}")]
    public async Task<ActionResult> Toggle(
        [FromRoute] Guid id,
        [FromBody] bool completed,
        CancellationToken cancellationToken)
    {
        var task = await taskRepository.Toggle(id, completed, cancellationToken);
        if (task is null)
            return NotFound($"Task with ID {id} not found.");
        
        return Ok(task);
    }
}
