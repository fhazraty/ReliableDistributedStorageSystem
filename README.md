# ReliableDistributedStorageSystem
A Reliable Distributed Storage System using Blockchain Networks
Having reliable data storage that has a high availability in a distributed environment is a challenging issue that may face many other issues that can affect the performance of data processing in the network. 
Different types of techniques like replicating data on multiple nodes and updating them with the latest version of data would result in high costs for the network and other related resources. 
The security and privacy of data are other issues that need some techniques to prevent data from being exposed, tampered and manipulated by adversary actors. 
We have proposed a simple permissioned blockchain-inspired system that stores the chunks of files on different nodes and synchronizes them over a period of time. 

## Install Docker
Docker installation
The docker which implements this image must have at least 4 GB of RAM. This resource is not required in this project however, for installing the docker engine, 4GB is [required](https://docs.docker.com/desktop/install/linux-install/).

We have tested this project on a standard virtual server on [HETZNER Cloud](https://www.hetzner.com/legal/cloud-server/). 
The installation is discussed as follows:

Operating System: Ubuntu 22.04
VCPUS: 3 Cores (AMD)
RAM: 4GB
DISK: Depends on the transaction payload

Install the docker from this [link](https://docs.docker.com/engine/install/ubuntu/)

```console
sudo apt-get update
sudo apt-get install \
    ca-certificates \
    curl \
    gnupg \
    lsb-release

sudo mkdir -m 0755 -p /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg
echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu \
  $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

sudo apt-get update
sudo apt-get install docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
```