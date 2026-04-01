# Production Update + Redeploy Runbook (VPS)

This is the operational guide for future updates on the current production setup:

- Host Nginx (Ubuntu VPS) proxies to `127.0.0.1:8080`
- App runs in Docker container: `ord-all-in-one`
- Domains:
  - `intranet.ordbusinesshub.com` (dashboard + .NET API)
  - `ordbusinesshub.com` and `www.ordbusinesshub.com` (ordpanel)

> Important scope rule: do not modify `spark-group.in` configs/files during ord deployment.

---

## 1) Prerequisites

- SSH access to VPS
- Docker installed on VPS
- Local repo with latest tested changes
- Domain SSL already configured on host Nginx

Recommended local pre-check:

```bash
# from repo root
cd new-dashboard
npm install
npm run build
cd ..
```

---

## 2) Files/Folders used on VPS

- Build context folder: `/root/ord-all-in-one-build`
- Running container: `ord-all-in-one`
- Host reverse proxy (already present):
  - `/etc/nginx/sites-available/intranet.ordbusinesshub.com`
  - `/etc/nginx/sites-available/www.ordbusinesshub.com`

---

## 3) Backup before update

Run on VPS:

```bash
docker ps --filter "name=ord-all-in-one"
docker images | head
docker tag ord-all-in-one:latest ord-all-in-one:backup-$(date +%Y%m%d-%H%M%S)
```

---

## 4) Sync latest code to VPS build context

From local machine (repo root), copy changed files or the full project into `/root/ord-all-in-one-build`.

Example (full sync via archive):

```bash
tar -czf ord-all-in-one-build.tgz Dockerfile docker backend_net new-dashboard ordpanel
scp ord-all-in-one-build.tgz root@<VPS_IP>:/root/
ssh root@<VPS_IP> "rm -rf /root/ord-all-in-one-build && mkdir -p /root/ord-all-in-one-build && tar -xzf /root/ord-all-in-one-build.tgz -C /root/ord-all-in-one-build --strip-components=0"
```

Or copy only changed files with `scp`.

---

## 5) Build new image

Run on VPS:

```bash
cd /root/ord-all-in-one-build
docker build -t ord-all-in-one:latest .
```

---

## 6) Redeploy container

Stop old and run new container with required env vars.

```bash
docker rm -f ord-all-in-one || true

docker run -d --name ord-all-in-one --restart unless-stopped \
  -p 127.0.0.1:8080:80 \
  -e DASHBOARD_HOST='intranet.ordbusinesshub.com' \
  -e ORDPANEL_HOST='ordbusinesshub.com www.ordbusinesshub.com' \
  -e Frontend__BaseUrl='https://intranet.ordbusinesshub.com' \
  -e Frontend__AllowedOrigins__0='https://ordbusinesshub.com' \
  -e Frontend__AllowedOrigins__1='https://www.ordbusinesshub.com' \
  -e ConnectionStrings__DefaultConnection='Server=<SQL_HOST>,1433;Database=<DB_NAME>;User Id=<DB_USER>;Password=<DB_PASSWORD>;TrustServerCertificate=True;Encrypt=True;' \
  -e DB_SERVER='<SQL_HOST>,1433' \
  -e DB_NAME='<DB_NAME>' \
  -e DB_USER='<DB_USER>' \
  -e DB_PASSWORD='<DB_PASSWORD>' \
  -e API_BASE_URL='http://127.0.0.1:8080' \
  ord-all-in-one:latest
```

Notes:
- Keep host Nginx upstream as `127.0.0.1:8080`.
- Do not expose container on public `0.0.0.0`.

---

## 7) Post-deploy verification

Run on VPS:

```bash
curl -skS -m 30 -o /dev/null -w 'intranet:%{http_code} %{time_total}\n' https://intranet.ordbusinesshub.com/
curl -skS -m 30 -o /dev/null -w 'ordpanel:%{http_code} %{time_total}\n' https://www.ordbusinesshub.com/
curl -skS -m 30 -o /dev/null -w 'contact:%{http_code} %{time_total}\n' https://www.ordbusinesshub.com/contact
curl -skS -m 30 -o /dev/null -w 'robots:%{http_code}\n' https://www.ordbusinesshub.com/robots.txt
curl -skS -m 30 -o /dev/null -w 'sitemap:%{http_code}\n' https://www.ordbusinesshub.com/sitemap.xml
curl -skS -m 30 -o /dev/null -w 'favicon:%{http_code}\n' https://www.ordbusinesshub.com/images/logo-sm.png
docker logs --tail 150 ord-all-in-one
```

Expected:
- HTTP `200` for key URLs
- No repeated fatal startup failures in logs

---

## 8) Rollback procedure

If deploy fails:

```bash
docker rm -f ord-all-in-one || true
docker run -d --name ord-all-in-one --restart unless-stopped \
  -p 127.0.0.1:8080:80 \
  ...same env vars... \
  ord-all-in-one:backup-<timestamp>
```

Re-run verification curls.

---

## 9) Common issues and fixes

- **502 Bad Gateway**  
  Check container running and `docker logs`. Usually php-fpm/nginx startup config issue.

- **504 Gateway Timeout**  
  Check app health + database connectivity + host nginx timeout settings.

- **No DB data / PDO driver errors**  
  Ensure `DB_*` env vars are passed and container includes ODBC packages (`msodbcsql18`, `php8.2-odbc`).

- **Wrong domain served (intranet vs ordpanel)**  
  Verify `ORDPANEL_HOST='ordbusinesshub.com www.ordbusinesshub.com'` and host Nginx `server_name`.

---

## 10) Change log practice (recommended)

For every production update, record:
- Date/time
- Git commit/branch
- Image tag used
- Summary of changed modules
- Verification results
- Rollback image tag

