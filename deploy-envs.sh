#!/bin/bash

# Set variables
export IMAGE_NAME=netcoreapps-redisgeo
export IMAGE_VERSION=latest

export AWS_DEFAULT_REGION=us-east-1
export AWS_ECS_CLUSTER_NAME=default

export AWS_VIRTUAL_HOST=redisgeo.netcore.io
export AWS_REDIS_HOST=redis-redisgeo