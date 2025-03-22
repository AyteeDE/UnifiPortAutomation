# UnifiPortAutomation Docker Guide

This guide provides instructions on how to deploy and use UnifiPortAutomation using Portainer.

## Prerequisites

- Portainer installed on your homeserver
- A running Unifi Controller

## Quick Start

The easiest way to get started is by using the provided Docker Compose file in the project's root directory.

## Configuration

Edit the docker-compose.yml file to customize your environment variables:

```yaml
version: '3'
services:
  unifiportautomation:
    container_name: portautomation
    image: ghcr.io/ayteede/unifiportautomation:main
    restart: always
    environment:
      INTERVAL_MINUTES: 60
      UNIFI_HOST: "127.0.0.1" # IP of your Unifi Controller
      PORTAINER_HOST: "127.0.0.1:9443" # IP:Port of your main Portainer instance
      UNIFI_USERNAME: "apiuser" # Username for the Unifi API
      UNIFI_PASSWORD: "apipassword" # Password for the Unifi API
      PORTAINER_TOKEN: "portainertoken" # User-Token for the Portainer API
      PORTAINER_CONTAINER_CONFIGURATION: '[{"Name":"Container1","EnvironmentId":1,"Ports":[80,81,82]}, {"Name":"Container2","EnvironmentId":1,"Ports":[83]}]' # JSON Array with the Configuration-Class objects
```

## Running the Container

Create and deploy a Portainer stack with the compose-file and your environment variables.

## Troubleshooting
If you encounter issues:

1. Check the logs for error messages.
2. Check if the Portainer Container configuration file formatting is correct.
3. Verify your connection settings and permissions.
3. Ensure your Unifi Controller is accessible from the Docker container.