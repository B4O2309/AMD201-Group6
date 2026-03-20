# QuickLink — WebUI

React frontend for the QuickLink URL Shortener microservices system.

## Tech Stack

- **React 19** + React Router 7
- **Axios** for API calls (JWT-based auth)
- **Vite 8** for dev server & build
- **Nginx** for production serving (Docker)

## Quick Start (Development)

```bash
# Install dependencies
npm install

# Start dev server (port 3000, proxies /gateway/* and /api/* → localhost:5000)
npm run dev
```

Make sure the API Gateway is running on `http://localhost:5000`.

## Build & Run with Docker

```bash
docker build -t quicklink-webui .
docker run -p 3000:80 quicklink-webui
```

## Environment Variables

| Variable | Default | Description |
|---|---|---|
| `VITE_API_BASE_URL` | `http://localhost:5000` | API Gateway base URL |

## API Endpoints

### Auth (User Service — via Gateway)

| Method | Endpoint | Body | Description |
|---|---|---|---|
| `POST` | `/gateway/auth/login` | `{ email, password }` | Returns `{ token, user }` |
| `POST` | `/gateway/auth/register` | `{ username, email, password, role }` | Register new user |
| `GET` | `/gateway/auth/me` | — | Verify token, returns user info |

### URL Service (via Gateway) — ⚠️ endpoints TBD, confirm with Member 1

| Method | Endpoint | Body | Description |
|---|---|---|---|
| `POST` | `/api/url/shorten` | `{ url }` | Create short URL |
| `GET` | `/api/url` | — | Get user's links |
| `PUT` | `/api/url/:id` | `{ url }` | Update a link |
| `DELETE` | `/api/url/:id` | — | Delete a link |

## Project Structure

```
webui/
├── src/
│   ├── assets/              # Static images
│   ├── components/
│   │   ├── Navbar.jsx       # Top nav with username + role badge
│   │   ├── ShortenForm.jsx  # URL shorten form
│   │   └── LinksTable.jsx   # Links data table with actions
│   ├── context/
│   │   └── AuthContext.jsx   # JWT auth + fetchMe() on reload
│   ├── pages/
│   │   ├── HomePage.jsx     # Landing page with shorten form
│   │   ├── LoginPage.jsx    # Login (POST /gateway/auth/login)
│   │   ├── RegisterPage.jsx # Register (POST /gateway/auth/register)
│   │   └── DashboardPage.jsx# Link management dashboard
│   ├── services/
│   │   ├── api.js           # Axios instance + JWT interceptor
│   │   └── links.js         # URL service API calls
│   ├── App.jsx              # Routes + ProtectedRoute + GuestRoute
│   ├── main.jsx             # React entry point
│   └── index.css            # Global styles + CSS variables
├── nginx.conf               # Nginx: SPA + /gateway/ + /api/ proxy
├── Dockerfile               # Multi-stage build (node → nginx)
├── .dockerignore
├── .gitignore
├── .env
├── index.html
├── package.json
└── vite.config.js           # Dev proxy /gateway/ + /api/ → :5000
```
