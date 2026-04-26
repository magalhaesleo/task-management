import { useState } from 'react'
import './App.css'
import TaskList from './components/TaskList';

function App() {
  const [tasks, setTasks] = useState([
    { id: 1, title: "Study React", completed: false },
    { id: 2, title: "Build project", completed: false },
  ]);

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
