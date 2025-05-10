#!/bin/bash

# Stop and remove existing container if it's already running
docker stop metabase 2>/dev/null

# Run Metabase container
docker run -d -p 3000:3000 \
  --name metabase \
  --dns 8.8.8.8 \
  metabase/metabase:latest

