import { useEffect, useState } from 'react';
import TaskList from './components/TaskList';
import NewTask from './components/NewTask';
import api from './services/api';

function App() {
  const [tasks, setTasks] = useState([]);

  async function loadTasks() {
    api.get('tasks').then((response) => setTasks(response.data)).catch(console.error);
  } 

  useEffect(() => {
    loadTasks();
  }, []);

  return (
    <div>
      <h1>Tasks Management</h1>
      <NewTask onAdd={() => loadTasks()} />
      <TaskList tasks={tasks} onToggle={() => loadTasks()} />
    </div>
  );
}

export default App;
