#!/bin/bash

# Exit if any command has a non-zero exit status (Exists if a command returns an exception, like it's a programming language)
# Prevents errors in a pipeline from being hidden. So if any command fails, that return code will be used as the return code of the whole pipeline
set -eo pipefail

check-environment.sh

configure-kerberos.sh

# Starting the kerberos server
/usr/sbin/krb5kdc -P /var/run/krb5kdc.pid

# TODO - add an nginx server in front of this to enable https
PORT=${KERBEROS_API_PORT:-3000}
# run the server
dotnet "$API_HOME"/app_api.dll --urls=http://0.0.0.0:$PORT