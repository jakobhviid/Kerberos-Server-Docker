#!/bin/bash

servicename="$1"
host="$2"

kadmin.local -q "add_principal -randkey "$servicename"/"$host"@"$KERBEROS_REALM"" > /dev/null 2>&1

kadmin.local -q "xst -kt /keytabs/"$servicename".service.keytab "$servicename"/"$host"@"$KERBEROS_REALM"" > /dev/null 2>&1