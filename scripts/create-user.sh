#!/bin/bash

principal_name="$1"

kadmin.local -q "add_principal -randkey "$principal_name"@"$KERBEROS_REALM""

kadmin.local -q "xst -kt /keytabs/"$principal_name".user.keytab "$principal_name"@"$KERBEROS_REALM""