# New Dashboard - React Frontend

A modern React application with authentication for One Rank Digital, matching the admin template UI.

## Features

- ✅ **JWT Authentication** - Login and Register with JWT tokens
- ✅ **Protected Routes** - Route protection based on authentication status
- ✅ **UI Matching Default Template** - Styled to match the admin template
- ✅ **React Router** - Client-side routing
- ✅ **Axios** - HTTP client with interceptors
- ✅ **Context API** - Global authentication state management

## Project Structure

```
new-dashboard/
├── src/
│   ├── components/        # Reusable components
│   │   └── ProtectedRoute.jsx
│   ├── contexts/          # React contexts
│   │   └── AuthContext.jsx
│   ├── pages/             # Page components
│   │   ├── Login.jsx
│   │   ├── Register.jsx
│   │   └── Dashboard.jsx
│   ├── services/          # API services
│   │   └── api.js
│   ├── styles/            # CSS files
│   │   └── auth.css
│   ├── App.jsx            # Main app component
│   └── main.jsx           # Entry point
├── public/                 # Static assets
└── package.json
```

## Installation

1. Install dependencies:
```bash
npm install
```

2. Configure API URL (optional):
Create a `.env` file:
```
VITE_API_URL=http://localhost:8080
```

3. Start development server:
```bash
npm run dev
```

## API Integration

The frontend connects to the backend API at `http://localhost:8080` by default.

### Endpoints Used:
- `POST /api/auth/login` - User login
- `POST /api/auth/register` - User registration
- `GET /api/auth/me` - Get current user (protected)

## Routes

- `/login` - Login page
- `/register` - Registration page
- `/dashboard` - Dashboard (protected)
- `/` - Redirects to dashboard

## Authentication Flow

1. User logs in or registers
2. JWT token is stored in localStorage
3. Token is automatically added to API requests
4. Protected routes check authentication status
5. On 401 error, user is redirected to login

## Styling

The UI matches the admin template styling:
- Gradient header boxes
- Bootstrap-like grid system
- Form styling matching the template
- Responsive design

## Development

- **Port**: Default Vite dev server port (usually 5173)
- **Hot Reload**: Enabled
- **Build**: `npm run build`

## Environment Variables

- `VITE_API_URL` - Backend API base URL (default: http://localhost:8080)

## Notes

- Make sure the backend is running before testing authentication
- Token is stored in localStorage
- On logout, all auth data is cleared
- Protected routes redirect to login if not authenticated
