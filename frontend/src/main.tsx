import React from "react";
import { createRoot } from "react-dom/client";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import Login from "./pages/Login";
import Register from "./pages/Register";
import Dashboard from "./pages/Dashboard";
import ProjectDetails from "./pages/ProjectDetails";
import Layout from "././components/Layout";
import "./styles.css";

function App(){
  const token = localStorage.getItem("token");
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<Layout/>}>
          <Route path="/dashboard" element={ token ? <Dashboard/> : <Navigate to="/login" /> }/>
          <Route path="/projects/:id" element={ token ? <ProjectDetails/> : <Navigate to="/login" /> }/>
        </Route>
        <Route path="/login" element={ <Login/> }/>
        <Route path="/register" element={ <Register/> }/>
        <Route path="*" element={<Navigate to={token ? "/dashboard" : "/login"} />} />
      </Routes>
    </BrowserRouter>
  );
}

createRoot(document.getElementById("root")!).render(<App/>);
