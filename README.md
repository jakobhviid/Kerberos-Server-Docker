# About
A kerberos server API. Useful in IoT and microservices which require kerberos authentication. The container can also be used in container orchestration environments, because it uses a database and will reconstruct every kerberos principal/keytab (user) if it is restarted.

The server works by having one admin user who is able to create other users/services. The users/services will then get a password from the admin and is able to request their kerberos keytab as many times as they want.

# How to use
This docker-compose file show the deployment of the container
As can be seed 'network_mode' has been set to "host". This is required for the kerberos server to function properly. This is a mode which only works on linux machines

```
version: "3"

services:
  kerberos_server:
    image: cfei/docker-kerberos
    container_name: kerberos
    network_mode: "host"
    restart: always
    environment:
      KERBEROS_ADMIN_PW: password
      KERBEROS_HOST_DNS: <<host_dns>>
      KERBEROS_REALM: CFEI.SECURE
      KERBEROS_API_PORT: 3000
      KERBEROS_MSSQL_CONNECTION_STRING: "Server=<<mssql_ip_address>>;Database=<<database_name>;User Id=<<database_user>>;Password=<<database_password>>;"
```

## API Endpoints
**Terms explained**
A 'user' is a keytab without a host specified. 
A 'service' is a keytab with a host specified! That is the only difference. So if you require a keytab with a specific host (mostly used for programs such as kafka and zookeeper) create a 'service'. Whereas if you require a keytab without a host specified (mostly used for humans interacting with kerberos) create a 'user'

In the following examples the host is example.com and the port is 3000.

**Note** The kerberos API is versioned. So all statuscodes and responses will stay exactly the same for the version that you use.

### example.com:3000/create-new-user
This endpoint creates a user. The admin password provided during the container setup is required here.
##### Example Request (JSON)
```
{
	"adminPassword": "password",
	"newUserUsername": "TestUser",
	"newUserPassword": "TestPassword"
}
```
##### Returns one of the following
- 403: Admin Password incorrect
- 400: User Already exists with a keytab
- 201: User successfully created

### example.com:3000/create-new-service
This endpoint creates a service, so a host is required. The admin password provided during the container setup is required here.
##### Example Request (JSON)
```
{
	"adminPassword": "password",
	"newServiceName": "kafka",
	"newServicePassword": "kafkaPassword",
	"newServiceHost": "127.0.0.1"
}
```
##### Returns one of the following
- 403: Admin Password incorrect
- 400: Service already exists with a keytab
- 201: Service successfully created

### example.com:3000/get-keytab
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
- 400: User does not exist, contact an administrator to get a user and a kerberos keytab
- 200: A file with the content type of "application/octet-stream"


# Configuration

- `KERBEROS_ADMIN_PW`: The admin password to use. The admin is able to create users and services with this password, so store it safely! Required.

- `KERBEROS_HOST_DNS`: The DNS-resolvable hostname to use which the kerberos server is identified by. It should be set to the FQDN of the server on which kerberos is running on. Required.

- `KERBEROS_REALM`: The kerberos realm which will be used in keytabs and principals. Required.

- `KERBEROS_API_PORT`: The port on which the API will listen for connections.

- `KERBEROS_MSSQL_CONNECTION_STRING`: The connection string for a mssql database. This database is crucial in order to save and store all principals and users in case the container restarts.

# Volumes

- `/keytabs/`: Keytabs are the only thing important in this container. And these are also saved to the database. But in case you want to use these keytabs another way you are able to use this volume.
  
- TODO: API Logging

# Security
TODO: Add support for https