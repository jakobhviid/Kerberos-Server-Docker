#!/bin/bash

if ! [ -z "$KERBEROS_INIT_USERS" ]; then
    PORT=${KERBEROS_API_PORT:-3000}
    echo "INFO - 'KERBEROS_INIT_USERS' has been provided. Creating users. Waiting for API to be ready"
    # Wait until the api has started and listens on the port. The max is 15 seconds
    counter=0
    while [ -z "$(netstat -tln | grep "$PORT")" ]; do # Listen on localhost open ports and greps PORT
        if [ "$counter" -eq 15 ]; then         # 15 seconds have passed
            echo -e "\e[1;32mERROR - Creating initial user did not succeed. Server did not start \e[0m"
            exit 1
        else
            echo "Waiting for API to start ..."
            sleep 1
            ((counter++))
        fi
    done
    echo "API has started"

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

            response=$(curl --fail --connect-timeout 5 --retry 5 --retry-delay 5 --retry-max-time 30 --retry-connrefused --max-time 5 -X POST -H "Content-Type: application/json" -d "{\"apiKey\":\""$KERBEROS_API_KEY"\", \"newServiceName\":\""$username"\", \"newServicePassword\":\""$password"\", \"newServiceHost\":\""$host"\"}" "http://127.0.0.1:"$PORT"/create-new-service" || echo "FAIL")
            if [ "$response" == "FAIL" ]; then
                echo -e "\e[1;32mERROR - Creating initial user did not succeed. See curl error above. \e[0m"
            fi
        else
            response=$(curl --fail --connect-timeout 5 --retry 5 --retry-delay 5 --retry-max-time 30 --retry-connrefused --max-time 5 -X POST -H "Content-Type: application/json" -d "{\"apiKey\":\""$KERBEROS_API_KEY"\", \"newUserUsername\":\""$username"\", \"newUserPassword\":\""$password"\"}" "http://127.0.0.1:"$PORT"/create-new-user" || echo "FAIL")
            if [ "$response" == "FAIL" ]; then
                echo -e "\e[1;32mERROR - Creating initial user did not succeed. See curl error above. \e[0m"
            fi
        fi
    done
fi