import TaskItem from "./TaskItem";

import './TaskList.css';

export default function TaskList({ tasks, onToggle }) {
    if (!tasks.length) {
        return <p>No tasks yet</p>;
    }

    const pending = tasks.filter((t) => !t.completed);
    const completed = tasks.filter((t) => t.completed);

    return (
        <div className="task-columns">
            <div className="task-column">
                <h2>To do</h2>
                {pending.length ? (
                    <ul>
                        {pending.map((task) => (
                            <li key={task.id}>
                                <TaskItem task={task} onToggle={onToggle} />
                            </li>
                        ))}
                    </ul>
                ) : (
                    <p className="column-empty">All done!</p>
                )}
            </div>
            <div className="task-column task-column--completed">
                <h2>Completed</h2>
                {completed.length ? (
                    <ul>
                        {completed.map((task) => (
                            <li key={task.id}>
                                <TaskItem task={task} onToggle={onToggle} />
                            </li>
                        ))}
                    </ul>
                ) : (
                    <p className="column-empty">Nothing completed yet</p>
                )}
            </div>
        </div>
    );
}

