#!/bin/bash

docker network create vnext-development

echo "Starting in docker compose"
docker-compose -f docker-compose.yml up --build