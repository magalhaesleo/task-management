import { useState, useEffect } from 'react';

import './App.css';
import TaskList from './components/TaskList';
import api from './services/api';

function App() {
  const [tasks, setTasks] = useState([]);

  useEffect(() => {
    api.get('tasks').then((response) => setTasks(response.data)).catch(console.error)
  }, []);

  const toogleTask = (id) => {
    setTasks((prev) => prev.map((task) => task.id === id ? { ...task, completed: !task.completed } : task));
  }

  return (
    <div>
      <h1>Tasks</h1>
      <TaskList tasks={tasks} onToggle={toogleTask} />
    </div>
  );
}

export default App;
