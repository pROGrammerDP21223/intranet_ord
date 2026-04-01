## Production Deployment (Ubuntu VPS - Local Services)

This app runs **frontend + backend as local services**. The frontend is built separately and served by the .NET backend from `wwwroot`.

> For the current Docker-based VPS flow (`ord-all-in-one`), use:
> `deploy/PROD_UPDATE_REDEPLOY_RUNBOOK.md`

### 1) Prerequisites
- Ubuntu 20.04+ VPS
- .NET 8.0 SDK/Runtime
- Node.js 20+
- SQL Server 2022 (or SQL Server Express)
- Open ports: `80`, `443`, and `8080` (or use Nginx on 80/443)

### 2) Install .NET 8
```bash
wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt update
sudo apt install -y dotnet-sdk-8.0
```

### 3) Install SQL Server
```bash
sudo apt install -y mssql-server
sudo /opt/mssql/bin/mssql-conf setup
sudo systemctl enable mssql-server
sudo systemctl start mssql-server
```

### 4) Install Node.js 20
```bash
curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash -
sudo apt install -y nodejs
```

### 5) Clone the Repo
```bash
git clone <YOUR_REPO_URL> app
cd app
```

### 6) Configure Environment
Update `backend_net/appsettings.Production.json`:
- Set the correct SQL Server connection string (localhost)
- Update JWT secrets, email, and other settings

### 7) Build Frontend
```bash
cd new-dashboard
npm install
npm run build
cp -r dist/* ../backend_net/wwwroot/
cd ..
```

### 8) Build and Publish Backend
```bash
cd backend_net
dotnet publish -c Release -o /var/www/app
```

### 9) Run Database Migrations
```bash
cd backend_net
ASPNETCORE_ENVIRONMENT=Production dotnet ef database update
```

### 10) Run the App
```bash
ASPNETCORE_ENVIRONMENT=Production dotnet /var/www/app/backend_net.dll --urls "http://0.0.0.0:8080"
```

Or set up a systemd service for auto-start (recommended):
```bash
sudo nano /etc/systemd/system/backend_net.service
```
```ini
[Unit]
Description=backend_net ASP.NET Core App
After=network.target

[Service]
WorkingDirectory=/var/www/app
ExecStart=/usr/bin/dotnet /var/www/app/backend_net.dll --urls "http://0.0.0.0:8080"
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=backend_net
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
```
```bash
sudo systemctl daemon-reload
sudo systemctl enable backend_net
sudo systemctl start backend_net
```

---

## Optional: Nginx Reverse Proxy + SSL (Recommended)

### 1) Install Nginx + Certbot
```bash
sudo apt install -y nginx certbot python3-certbot-nginx
```

### 2) Nginx Site Config
For this VPS cutover, update only:
- `intranet.ordbusinesshub.com`
- `ordbusinesshub.com`
- `www.ordbusinesshub.com`

Create `/etc/nginx/sites-available/ord-cutover.conf`:
```nginx
server {
    listen 80;
    server_name intranet.ordbusinesshub.com;

    location / {
        proxy_pass http://127.0.0.1:8080;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}

server {
    listen 80;
    server_name ordbusinesshub.com www.ordbusinesshub.com;

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
sudo ln -s /etc/nginx/sites-available/ord-cutover.conf /etc/nginx/sites-enabled/
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
cd new-dashboard && npm install && npm run build && cp -r dist/* ../backend_net/wwwroot/
cd ../backend_net
dotnet publish -c Release -o /var/www/app
ASPNETCORE_ENVIRONMENT=Production dotnet ef database update
sudo systemctl restart backend_net
```

---

## Troubleshooting
- **Logs**: `sudo journalctl -u backend_net -f`
- **Service status**: `sudo systemctl status backend_net`
- **SQL Server logs**: `sudo cat /var/opt/mssql/log/errorlog`

---

## Notes
- Frontend is built into `wwwroot` and served at `/`.
- API is under `/api` and Swagger at `/swagger`.
- SQL Server runs locally on port 1433.
