
function set_realm_and_host_dns() {
    # kdc.conf
    printf "[kdcdefaults]\n  kdc_ports = 88\n  kdc_tcp_ports = 88\n  default_realm="$KERBEROS_REALM"\n[realms]\n"$KERBEROS_REALM" = {\nacl_file = /var/kerberos/krb5kdc/kadm5.acl\ndict_file = /usr/share/dict/words\nadmin_keytab = /var/kerberos/krb5kdc/kadm5.keytab\nsupported_enctypes = aes256-cts:normal aes128-cts:normal des3-hmac-sha1:normal arcfour-hmac:normal camellia256-cts:normal camellia128-cts:normal des-hmac-sha1:normal des-cbc-md5:normal des-cbc-crc:normal\n}" /var/kerberos/krb5kdc/kdc.conf > /dev/null 2>&1
    # kadm5.acl
    printf "*/admin@"$KERBEROS_REALM" *\n" > /var/kerberos/krb5kdc/kadm5.acl > /dev/null 2>&1
    # krb5.conf
    printf "\n[realms]\n"$KERBEROS_REALM" = {\nadmin_server = "$KERBEROS_HOST_DNS"\nkdc = "$KERBEROS_HOST_DNS"\n}" >> /etc/krb5.conf > /dev/null 2>&1
    awk -v kerberos_realm="$KERBEROS_REALM" '/default_realm/{c++;if(c==1){sub("default_realm.*","default_realm="kerberos_realm);c=0}}1' /etc/krb5.conf >/tmp/tmpfile && mv /tmp/tmpfile /etc/krb5.conf
}
