import { useState } from 'react'
import { ThemeProviderWrapper } from '../context/ThemeContext';
import AppRouter from '../router/AppRouter';
import './App.css'

function App() {

  return (
    <ThemeProviderWrapper>
      <AppRouter />
    </ThemeProviderWrapper>
  )
}

export default App;
