#!/bin/bash
set -e

# 1. Cargar variables de entorno
if [ -f ./.env ]; then
    while IFS='=' read -r key value; do
        if [[ ! -z "$key" && ! "${key:0:1}" == "#" ]]; then
            export "$key"="$value"
        fi
    done < ./.env
else
    echo ".env file not found!"
    exit 1
fi

if [ -z "$PEM_PATH" ] || [ -z "$SERVER_ADDRESS" ]; then
    echo "PEM_PATH or SERVER_ADDRESS not set in .env file."
    exit 1
fi

TARGET=${1:-all}
echo "Deploying target: $TARGET to $SERVER_ADDRESS"

# Limpiar tars locales
rm -f urbapf-backend.tar urbapf-frontend.tar

# 2. Build local
if [ "$TARGET" == "all" ] || [ "$TARGET" == "backend" ]; then
    echo "--- Building Backend ---"
    docker build -t urbapf-backend:local -f ./backend/Dockerfile ./backend
    docker save -o urbapf-backend.tar urbapf-backend:local
fi

if [ "$TARGET" == "all" ] || [ "$TARGET" == "frontend" ]; then
    echo "--- Building Frontend ---"
    
    # Build with NO CACHE to force latest changes
    docker build --no-cache \
        -t urbapf-frontend:local -f ./frontend/Dockerfile ./frontend
    docker save -o urbapf-frontend.tar urbapf-frontend:local
fi

# 3. Transferir al servidor
echo "--- Transferring files ---"
# Eliminar tars antiguos en el servidor para evitar corrupciones
ssh -i "$PEM_PATH" -o StrictHostKeyChecking=no -o UserKnownHostsFile=/dev/null "ubuntu@$SERVER_ADDRESS" "mkdir -p ~/urbapf_deploy && rm -f ~/urbapf_deploy/*.tar"

rsync -avzP -e "ssh -i \"$PEM_PATH\" -o StrictHostKeyChecking=no -o UserKnownHostsFile=/dev/null" \
    ./docker-compose.yml \
    ubuntu@"$SERVER_ADDRESS":~/urbapf_deploy/

if [ -f urbapf-backend.tar ]; then
    rsync -avzP -e "ssh -i \"$PEM_PATH\" -o StrictHostKeyChecking=no -o UserKnownHostsFile=/dev/null" \
        urbapf-backend.tar \
        ubuntu@"$SERVER_ADDRESS":~/urbapf_deploy/
fi

if [ -f urbapf-frontend.tar ]; then
    rsync -avzP -e "ssh -i \"$PEM_PATH\" -o StrictHostKeyChecking=no -o UserKnownHostsFile=/dev/null" \
        urbapf-frontend.tar \
        ubuntu@"$SERVER_ADDRESS":~/urbapf_deploy/
fi

# 4. Despliegue en el servidor
echo "--- Remote Deployment ---"
ssh -i "$PEM_PATH" -o StrictHostKeyChecking=no -o UserKnownHostsFile=/dev/null "ubuntu@$SERVER_ADDRESS" << 'EOF'
    set -e
    cd ~/urbapf_deploy

    echo "Loading images..."
    if [ -f urbapf-backend.tar ]; then
        docker load -i urbapf-backend.tar
    fi
    if [ -f urbapf-frontend.tar ]; then
        docker load -i urbapf-frontend.tar
    fi

    echo "Restarting services..."
    # docker compose up -d se encarga de recrear solo lo necesario
    docker compose up -d

    echo "Status:"
    docker compose ps

    echo "Cleaning up tar files..."
    rm -f *.tar
EOF

# Cleanup local
rm -f urbapf-backend.tar urbapf-frontend.tar

echo "Deployment complete!"
