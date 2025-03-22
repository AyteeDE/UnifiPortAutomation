# UnifiPortAutomation Docker Guide

This guide provides instructions on how to deploy and use UnifiPortAutomation using Portainer.

## Use Case

This project will open or close the ports in Unifi OS depending on the associated container's status. 
e.g. You don't want to leave the ports for your Minecraft server open all the time when the container is not running.

It uses the Portainer and Unifi OS API. 

## Prerequisites

- Portainer installed on your homeserver
- A running Unifi Controller

## Quick Start

The easiest way to get started is by using the provided Docker Compose file in the project's root directory.
Edit the [docker-compose.yml](https://github.com/AyteeDE/UnifiPortAutomation/blob/main/docker-compose.yaml) file to customize your environment variables.

Create and deploy a Portainer stack with the compose-file and your environment variables.

## Troubleshooting
If you encounter issues:

1. Check the logs for error messages.
2. Check if the Portainer Container configuration file formatting is correct.
3. Verify your connection settings and permissions.
4. Ensure your Unifi Controller is accessible from the Docker container.