export default function TaskItem({ task, onToggle }) {
    return (
        <div>
            <div>
                <label>
                    <input
                        type="checkbox"
                        checked={task.completed}
                        onChange={() => onToggle(task.id)}
                    />
                    {task.title}
                </label>
            </div>
            <div style={{ marginLeft: "24px", color: "#555" }}>
                {task.content}
            </div>
        </div>
    );
}
