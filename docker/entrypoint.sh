#!/bin/bash
set -e
export DASHBOARD_HOST="${DASHBOARD_HOST:-localhost}"
export ORDPANEL_HOST="${ORDPANEL_HOST:-localhost}"

mkdir -p /etc/nginx/sites-available /etc/nginx/sites-enabled
envsubst '${DASHBOARD_HOST} ${ORDPANEL_HOST}' < /etc/nginx/templates/app.conf.template > /etc/nginx/sites-available/app.conf
ln -sf /etc/nginx/sites-available/app.conf /etc/nginx/sites-enabled/app.conf
rm -f /etc/nginx/sites-enabled/default 2>/dev/null || true

# Ensure php-fpm workers keep container env vars (DB_SERVER, DB_NAME, etc.).
# Without this, ordpanel falls back to localhost\SQLEXPRESS and returns empty data.
cat > /etc/php/8.2/fpm/pool.d/zz-ordpanel-env.conf <<'EOF'
[www]
clear_env = no
pm.max_children = 20
pm.start_servers = 4
pm.min_spare_servers = 2
pm.max_spare_servers = 8
EOF

exec /usr/bin/supervisord -c /etc/supervisord.conf
