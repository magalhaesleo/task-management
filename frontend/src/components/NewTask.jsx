import { useState } from 'react';
import { v7 as uuidv7 } from 'uuid';

import api from "../services/api";

import './NewTask.css';

export default function NewTask({ onAdd }) {

    const [title, setTitle] = useState('');
    const [content, setContent] = useState('');
    const [loading, setLoading] = useState(false);

    const handleSubmit = async (e) => {
        e.preventDefault();

        if (!title.trim())
            return;

        setLoading(true);

        const id = uuidv7();

        const data = {
            id,
            title,
            content
        };

        try {
            await api.post('tasks', data);
            setTitle("");
            setContent("");
            onAdd();
        } catch (err) {
            console.error(err);
        } finally {
            setLoading(false);
        }
    };

    return (
        <form className="new-task-form" onSubmit={handleSubmit}>
            <input
                className="new-task-input"
                type="text"
                placeholder="Task title"
                value={title}
                onChange={(e) => setTitle(e.target.value)}
            />
            <textarea
                className="new-task-textarea"
                placeholder="Description (optional)"
                value={content}
                onChange={(e) => setContent(e.target.value)}
                rows={3}
            />
            <button className="new-task-btn" type="submit" disabled={loading || !title.trim()}>
                {loading ? "Adding…" : "Add Task"}
            </button>
        </form>
    );
}
