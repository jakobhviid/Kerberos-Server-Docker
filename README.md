# About
A kerberos server API. Useful in IoT and microservices which require kerberos authentication. The container can also be used in container orchestration environments, because it uses a database and will reconstruct every kerberos principal/keytab (user) if it is restarted.

The server works by having one api key which has access to create users and services.

# How to use
This docker-compose file show the deployment of the container
As can be seen 'network_mode' has been set to "host". This is required for the kerberos server to function properly. This is a mode which only works on linux hosts.

```
version: "3"

services:
  kerberos_server:
    image: cfei/docker-kerberos
    container_name: kerberos
    network_mode: "host"
    restart: always
    environment:
      KERBEROS_API_KEY: password
      KERBEROS_HOST_DNS: <<host_dns>>
      KERBEROS_REALM: CFEI.SECURE
      KERBEROS_API_PORT: 6000
      KERBEROS_POSTGRES_CONNECTION_STRING: "Host=<<postgres_ip>>;Port=5432;Database=<<database_name>;Username=<<database_user>>;Password=<<database_password>>;"
```

# Configuration

#### Required environment variables

- `KERBEROS_API_KEY`: The api key to use. With this key you can create users and services, so store it safely!

- `KERBEROS_HOST_DNS`: The DNS-resolvable hostname to use which the kerberos server is identified by. It should be set to the FQDN of the server on which kerberos is running on.

- `KERBEROS_REALM`: The kerberos realm which will be used in keytabs and principals.

- `KERBEROS_POSTGRES_CONNECTION_STRING`: The connection string for a postgres database. This database is crucial in order to save and store all principals and users in case the container restarts.

#### Optional environment variables

- `KERBEROS_API_PORT`: The port on which the API will listen for connections.

- `KERBEROS_INIT_USERS`: The image can create users during startup. This environment variable has to be a comma-seperated string of users, with the format "Username=REQUIRED;Password=REQUIRED;Host=OPTIONAL". (example: `KERBEROS_INIT_USERS: "Username=zookeeper;Password=testPassword1;Host=127.0.0.1,Username=admin;Password=adminPassword"`).


## API Endpoints
**Terms explained**
A 'user' is a keytab without a host specified. 
A 'service' is a keytab with a host specified! That is the only difference. So if you require a keytab with a specific host (mostly used for programs such as kafka and zookeeper) create a 'service'. Whereas if you require a keytab without a host specified (mostly used for humans interacting with kerberos) create a 'user'

In the following examples the host is example.com and the port is 6000.

**Note** The kerberos API is versioned. So all statuscodes and responses will stay exactly the same for the version that you use.

### example.com:6000/create-new-user
This endpoint creates a user. The api key provided during the container setup is required here.
##### Example Request (JSON)
```
{
	"apiKey": "password",
	"newUserUsername": "TestUser",
	"newUserPassword": "TestPassword"
}
```
##### Returns one of the following
- 403: API key incorrect
- 400: User already exists with a keytab
- 201: User successfully created

### example.com:6000/create-new-service
This endpoint creates a service, so a host is required. The api key password provided during the container setup is required here.
##### Example Request (JSON)
```
{
	"apiKey": "password",
	"newServiceName": "kafka",
	"newServicePassword": "kafkaPassword",
	"newServiceHost": "127.0.0.1"
}
```
##### Returns one of the following
- 403: API key incorrect
- 400: Service already exists with a keytab
- 201: Service successfully created

### example.com:6000/get-keytab
A user and service uses this endpoint to fetch their keytab. The password they were created with is therefor needed here.
##### Example Request (JSON)
**For a user**:
```
{
	"username": "TestUser",
	"password": "TestPassword"
}
```
**For a service**:
```
{
	"username": "kafka",
	"password": "kafkaPassword",
    "host": "127.0.0.1"
}
```
##### Returns one of the following
- 400: User does not exist
- 200: A file with the content type of "application/octet-stream"

# Volumes

- `/keytabs/`: Keytabs are the only thing important in this container. And these are also saved to the database. But in case you want to use these keytabs another way you are able to use this volume.
  
- TODO: API Logging

# Security
TODO: Add support for https