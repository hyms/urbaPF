#!/bin/bash

# Cargar variables de entorno de forma robusta
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

echo "Deploying to $SERVER_ADDRESS using $PEM_PATH"

# 1. Build Docker images locally
echo "Building backend Docker image locally..."
docker build -t urbapf-backend:local -f ./backend/Dockerfile ./backend

echo "Building frontend Docker image locally..."
docker build -t urbapf-frontend:local -f ./frontend/Dockerfile ./frontend

# 2. Save Docker images to tar files
echo "Saving backend Docker image to urbapf-backend.tar..."
docker save -o urbapf-backend.tar urbapf-backend:local

echo "Saving frontend Docker image to urbapf-frontend.tar..."
docker save -o urbapf-frontend.tar urbapf-frontend:local

# 3. Transfer images and docker-compose.yml to the server using rsync
echo "Transferring Docker images and docker-compose.yml to the server..."
# Add StrictHostKeyChecking=no and UserKnownHostsFile=/dev/null to rsync for new server connections
rsync -avzP -e "ssh -i \"$PEM_PATH\" -o StrictHostKeyChecking=no -o UserKnownHostsFile=/dev/null" \
    urbapf-backend.tar \
    urbapf-frontend.tar \
    ./docker-compose.yml \
    ubuntu@"$SERVER_ADDRESS":~/urbapf_deploy/

# 4. Connect to AWS server and deploy with docker-compose
# Add StrictHostKeyChecking=no and UserKnownHostsFile=/dev/null to ssh for new server connections
ssh -i "$PEM_PATH" -o StrictHostKeyChecking=no -o UserKnownHostsFile=/dev/null "ubuntu@$SERVER_ADDRESS" << 'EOF'
    echo "Entering deployment directory..."
    mkdir -p ~/urbapf_deploy
    cd ~/urbapf_deploy

    # Server Environment Validation (Docker and Docker Compose)
    echo "Validating server environment..."
    if ! command -v docker &> /dev/null; then
        echo "Docker is not installed. Installing Docker..."
        sudo apt-get update
        sudo apt-get install -y docker.io
    fi
    if ! sudo systemctl is-active --quiet docker; then
        echo "Docker is not running. Starting Docker..."
        sudo systemctl start docker
        sudo systemctl enable docker
    fi

    # Check for docker compose plugin (preferred) or install older docker-compose
    if ! docker compose version &> /dev/null; then
        echo "Docker Compose plugin is not installed. Attempting to install..."
        # This command installs the Docker Compose plugin for Docker CLI
        sudo apt-get update
        sudo apt-get install -y docker-compose-plugin
        if ! docker compose version &> /dev/null; then
            echo "Docker Compose plugin installation failed. Trying standalone docker-compose..."
            sudo curl -L "https://github.com/docker/compose/releases/download/v2.20.2/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
            sudo chmod +x /usr/local/bin/docker-compose
            if ! docker-compose version &> /dev/null; then
                echo "Standalone docker-compose installation failed. Exiting."
                exit 1
            fi
        fi
    fi

    echo "Server environment validated."

    # Load Docker images
    echo "Loading Docker images..."
    docker load -i urbapf-backend.tar
    docker load -i urbapf-frontend.tar

    # Clean up old containers and networks before starting new ones with compose
    echo "Cleaning up previous Docker Compose deployment (stopping and removing containers/networks/volumes)..."
    docker compose down || true

    echo "Removing old Docker image tarballs from server to ensure fresh transfer..."
    rm -f ~/urbapf_deploy/urbapf-backend.tar
    rm -f ~/urbapf_deploy/urbapf-frontend.tar

    # Verify docker-compose.yml on server
    echo "Verifying docker-compose.yml on server..."
    if [ ! -f docker-compose.yml ]; then
        echo "Error: docker-compose.yml not found in ~/urbapf_deploy. Exiting."
        exit 1
    fi

    # Validate docker-compose.yml configuration
    echo "Validating docker-compose.yml configuration..."
    docker compose config || { echo "Error: docker-compose.yml validation failed. Exiting."; exit 1; }

    # Deploy with Docker Compose
    echo "\n--- Deploying services with Docker Compose --- "
    echo "Current memory usage before deployment:"
    free -h

    echo "Starting Docker Compose services in detached mode..."
    docker compose up -d
    echo "Docker Compose services initiated."

    echo "Waiting 5 seconds for services to initialize..."
    sleep 5

    echo "Checking the status of deployed Docker Compose services:"
    docker compose ps -a

    echo "\n--- Deployment process finished on remote server ---"

    echo "Deployment complete!"
EOF

echo "Deployment script updated and ready to run."
