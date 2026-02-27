## Production Deployment (Ubuntu VPS)

This app runs **frontend + backend in one container**. The frontend is built inside the backend image and served from `wwwroot`.

### 1) Prerequisites
- Ubuntu 20.04+ VPS
- A domain (optional but recommended)
- Open ports: `80`, `443`, and `8080` (or use Nginx on 80/443 and keep 8080 closed)

### 2) Install Docker + Compose
```bash
sudo apt update
sudo apt install -y ca-certificates curl gnupg
sudo install -m 0755 -d /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg
echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu \
  $(. /etc/os-release && echo "$VERSION_CODENAME") stable" | \
  sudo tee /etc/apt/sources.list.d/docker.list > /dev/null
sudo apt update
sudo apt install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
sudo usermod -aG docker $USER
newgrp docker
```

### 3) Clone the Repo (root, not backend only)
```bash
git clone <YOUR_REPO_URL> app
cd app
```

### 4) Configure Environment
Create `backend_net/.env`:
```bash
SA_PASSWORD=CHANGE_THIS_IN_PRODUCTION
```

Optional (recommended) hardening via appsettings:
- Update `backend_net/appsettings.Production.json` for JWT secrets, email, Twilio, etc.
- If you are not using Twilio or email, leave those settings empty.

### 5) Build and Start (Production Compose)
The Docker build context is the repo root, so **both** `new-dashboard` (frontend)
and `backend_net` (backend) are included in a **single image**.
```bash
cd backend_net
docker compose -f docker-compose.prod.yml up -d --build
```

### 6) Run Database Migrations
```bash
docker compose -f docker-compose.prod.yml exec backend_api dotnet ef database update
```

### 7) Verify
```bash
docker compose -f docker-compose.prod.yml ps
curl http://YOUR_VPS_IP:8080
curl http://YOUR_VPS_IP:8080/swagger
```

---

## Optional: Nginx Reverse Proxy + SSL (Recommended)

### 1) Install Nginx + Certbot
```bash
sudo apt install -y nginx certbot python3-certbot-nginx
```

### 2) Nginx Site Config
Create `/etc/nginx/sites-available/app.conf`:
```nginx
server {
    listen 80;
    server_name your-domain.com;

    location / {
        proxy_pass http://127.0.0.1:8080;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

Enable the site:
```bash
sudo ln -s /etc/nginx/sites-available/app.conf /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl restart nginx
```

### 3) SSL
```bash
sudo certbot --nginx -d your-domain.com
```

---

## Updates / Redeploy
```bash
cd app
git pull
cd backend_net
docker compose -f docker-compose.prod.yml up -d --build
docker compose -f docker-compose.prod.yml exec backend_api dotnet ef database update
```

---

## Troubleshooting
- **Logs**: `docker compose -f docker-compose.prod.yml logs -f backend_api`
- **Database logs**: `docker compose -f docker-compose.prod.yml logs -f sqlserver`
- **Rebuild clean**: `docker compose -f docker-compose.prod.yml build --no-cache`

---

## Notes
- Frontend is bundled into the backend container and served at `/`.
- API is under `/api` and Swagger at `/swagger`.
- The SQL Server runs in the same Docker network; it is not exposed publicly.
