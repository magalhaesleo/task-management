import TaskItem from "./TaskItem";

export default function TaskList({ tasks, onToggle }) {
    if (!tasks.length) {
        return <p>No tasks yet</p>;
    }

    return (
        <ul>
            {tasks.map((task) => (
                <TaskItem task={task} onToggle={onToggle} />
            ))}
        </ul>
    );
}

