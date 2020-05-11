#!/bin/bash

. conf-helpers.sh

set_realm_and_host_dns

/usr/sbin/kdb5_util create -s -r "$KERBEROS_REALM" -P kerberos-paasword
kadmin.local -q "add_principal -pw kerberos-paasword admin/admin"

