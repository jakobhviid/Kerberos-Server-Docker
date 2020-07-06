#!/bin/bash

# Exit if any command has a non-zero exit status (Exists if a command returns an exception, like it's a programming language)
# Prevents errors in a pipeline from being hidden. So if any command fails, that return code will be used as the return code of the whole pipeline
set -eo pipefail

PORT=${KERBEROS_API_PORT:-3000}

if ! [ -z "$KERBEROS_INIT_USERS" ]; then
    echo "INFO - 'KERBEROS_INIT_USERS' has been provided. Creating users. Waiting for API to be ready"
    sleep 5 # give the server a chance to start up

    IFS=',' # Comma seperated string of users
    read -r -a users <<<"$KERBEROS_INIT_USERS"
    for user in "${users[@]}"; do

        IFS=";" # semi colon seperated user arguments (e.g. Username=example;Password=example;Host=optionalExample)
        read -r -a arguments <<<"$user"
        usernameArgument="${arguments[0]}"
        username="${usernameArgument:9}" # substring "Username=example" so that the variable becomes "example"

        passwordArgument="${arguments[1]}"
        password="${passwordArgument:9}" # substring "Password=example" so that the variable becomes "example"

        echo Username - "$username"
        echo Password - "$password"

        if [[ " ${arguments[@]} " =~ "Host" ]]; then # if host is defined

            hostArgument="${arguments[2]}"
            host="${hostArgument:5}" # substring "Username=example" so that the variable becomes "example"

            echo Host - "$host"

            response=$(curl --fail --max-time 5 -X POST -H "Content-Type: application/json" -d "{\"apiKey\":\""$KERBEROS_API_KEY"\", \"newServiceName\":\""$username"\", \"newServicePassword\":\""$password"\", \"newServiceHost\":\""$host"\"}" "http://127.0.0.1:"$PORT"/create-new-service")
            if [ "$response" == "FAIL" ]; then
                echo -e "\e[1;32mERROR - Creating initial user did not succeed. Most likely error is that users already exist from a previous run \e[0m"
            fi
        else
            response=$(curl --fail --max-time 5 -X POST -H "Content-Type: application/json" -d "{\"apiKey\":\""$KERBEROS_API_KEY"\", \"newUserUsername\":\""$username"\", \"newUserPassword\":\""$password"\"}" "http://127.0.0.1:"$PORT"/create-new-user")
            if [ "$response" == "FAIL" ]; then
                echo -e "\e[1;32mERROR - Creating initial user did not succeed. Most likely error is that users already exist from a previous run \e[0m"
            fi
        fi
    done
fi