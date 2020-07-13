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
        # Checking that the username argument follows the correct format
        if ! [[ $usernameArgument == Username=* ]]; then
            echo -e "\e[1;31mERROR - Creating initial users did not succeed. "$usernameArgument" does not follow the correct format \e[0m"
            exit 1
        fi
        username="${usernameArgument:9}" # substring "Username=example" so that the variable becomes "example"

        passwordArgument="${arguments[1]}"
        # Checking that the password argument follows the correct format
        if ! [[ $passwordArgument == Password=* ]]; then
            echo -e "\e[1;31mERROR - Creating initial users did not succeed. "$passwordArgument" does not follow the correct format \e[0m"
            exit 1
        fi
        password="${passwordArgument:9}" # substring "Password=example" so that the variable becomes "example"

        echo Username - "$username"
        echo Password - "$password"

        if [[ " ${arguments[@]} " =~ "Host" ]]; then # if host is defined

            hostArgument="${arguments[2]}"

            # Checking that the host argument follows the correct format
            if ! [[ $hostArgument == Host=* ]]; then
                echo -e "\e[1;31mERROR - Creating initial users did not succeed. "$hostArgument" does not follow the correct format \e[0m"
                exit 1
            fi

            host="${hostArgument:5}" # substring "Host=example" so that the variable becomes "example"

            # If it is a single server setup, we need to resolve ip addresses before adding them as hosts
            if ! [ -z "$SINGLE_SERVER" ]; then

                # check if the input contains numbers. If they do we cannot DNS resolve it
                if [[ $host =~ [0-9] ]]; then
                    echo -e "\e[1;31mERROR - Creating initial users did not succeed. When 'SINGLE_SERVER' is defined all the hosts in initial users has to point to another docker container on the same network \e[0m"
                    exit 1
                fi
                addressString=$(nslookup "$host" | tail -n 2) # gets the ip address of the host ("Address=hostIp")
                address="${addressString:9}"                  # removing "Address" from addressString
                host=$address
            fi

            echo Host - "$host"

            response=$(curl --fail --max-time 5 -X POST -H "Content-Type: application/json" -d "{\"apiKey\":\""$KERBEROS_API_KEY"\", \"newServiceName\":\""$username"\", \"newServicePassword\":\""$password"\", \"newServiceHost\":\""$host"\"}" "http://127.0.0.1:"$PORT"/create-new-service" || echo "FAIL")
            if [ "$response" == "FAIL" ]; then
                echo -e "\e[1;31mERROR - Creating initial user did not succeed. See curl error above \e[0m"
            fi
        else
            response=$(curl --fail --max-time 5 -X POST -H "Content-Type: application/json" -d "{\"apiKey\":\""$KERBEROS_API_KEY"\", \"newUserUsername\":\""$username"\", \"newUserPassword\":\""$password"\"}" "http://127.0.0.1:"$PORT"/create-new-user" || echo "FAIL")
            if [ "$response" == "FAIL" ]; then
                echo -e "\e[1;31mERROR - Creating initial user did not succeed. See curl error above \e[0m"
            fi
        fi
    done
fi
