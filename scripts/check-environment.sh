#!/bin/bash

if [ -z "$KERBEROS_ADMIN_PW" ]; then
    echo -e "\e[1;32mERROR - Missing 'KERBEROS_ADMIN_PW' \e[0m"
    exit 1
fi

if [ -z "$KERBEROS_HOST_DNS" ]; then
    echo -e "\e[1;32mERROR - Missing 'KERBEROS_HOST_DNS' \e[0m"
    exit 1
fi

if [ -z "$KERBEROS_REALM" ]; then
    echo -e "\e[1;32mERROR - Missing 'KERBEROS_REALM' \e[0m"
    exit 1
fi

if [ -z "$KERBEROS_POSTGRES_CONNECTION_STRING" ]; then
    echo -e "\e[1;32mERROR - Missing 'KERBEROS_POSTGRES_CONNECTION_STRING' \e[0m"
    exit 1
fi

