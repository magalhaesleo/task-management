import { useEffect, useState } from 'react';

import api from "../services/api";

import './TaskItem.css';

export default function TaskItem({ task, onToggle }) {

    const [completed, setCompleted] = useState(task.completed);

    async function toggle() {
        const data = {
            completed
        };

        try {
            await api.patch(`tasks/${task.id}`, data);
            onToggle();
        } catch (err) {
            console.error(err);
        }
    }

    useEffect(() => {
        toggle();
    }, [completed]);

    const handleChange = async (e) => {
        e.preventDefault();
        setCompleted((prev) => !prev);
    };

    return (
        <div className="task-item">
            <div>
                <label className="task-checkbox-container">
                    <input
                        className="task-checkbox-input"
                        type="checkbox"
                        checked={task.completed}
                        onChange={handleChange}
                    />
                    {task.title}
                </label>
            </div>
            <div className="task-content">
                {task.content}
            </div>
        </div>
    );
}
