#!/bin/bash

# set environment variables used in deploy.sh and AWS task-definition.json:
export IMAGE_NAME=netcoreapps-redisgeo
export IMAGE_VERSION=latest

export AWS_DEFAULT_REGION=eu-central-1
export AWS_ECS_CLUSTER_NAME=default
export AWS_VIRTUAL_HOST=adduranceV2.movements.nl

# set any sensitive information in travis-ci encrypted project settings:
# required: AWS_ACCOUNT_NUMBER, AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY
# optional: SERVICESTACK_LICENSE
export AWS_ACCOUNT_NUMBER=126533691392
export AWS_ACCESS_KEY_ID=AKIAIXQLE3WNDVC33LWA
export AWS_SECRET_ACCESS_KEY=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