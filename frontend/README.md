# Mini Project Manager – Frontend

Responsive React + TypeScript single-page application styled with Tailwind utilities and a small set of handcrafted components.

## Available pages

- **Login / Register** – Auth flow backed by the API.
- **Dashboard** – Project summary, quick creation form, and responsive cards.
- **Project Details** – Task CRUD, inline editing, and smart scheduler planning UI.

## Getting started

```powershell
cd frontend
npm install
npm run dev
```

Set `VITE_API_BASE` in a `.env` file if the backend is not running on `http://localhost:5000/api/v1`.

## Production build

```powershell
npm run build
npm run preview
```

The build output lives in `dist/` and can be deployed to any static hosting provider.
