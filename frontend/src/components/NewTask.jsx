import { useState } from 'react';
import { v7 as uuidv7 } from 'uuid';

import api from "../services/api";

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
        <form onSubmit={handleSubmit} style={{ marginBottom: "20px" }}>
            <div>
                <input
                    type="text"
                    placeholder="Title"
                    value={title}
                    onChange={(e) => setTitle(e.target.value)}
                />
            </div>

            <div>
                <textarea
                    placeholder="Content"
                    value={content}
                    onChange={(e) => setContent(e.target.value)}
                />
            </div>

            <button type="submit" disabled={loading}>
                {loading ? "Adding..." : "Add Task"}
            </button>
        </form>
    );
}
