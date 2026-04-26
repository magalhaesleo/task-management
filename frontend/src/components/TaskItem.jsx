export default function TaskItem({ task, onToggle }) {
    return (
        <li key={task.id}>
            <label>
                <input
                    type="checkbox"
                    checked={task.completed}
                    onChange={() => onToggle(task.id)}
                />
                {task.title}
            </label>
        </li>
    );
}
