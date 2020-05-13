# How to use
NETWORK_MODE: 'HOST' which only works on linux is required for the kerberos server to function!

# Volumes
keytabs are saved at ... They are also saved in the database so it's not that important, but you can save it if you like.

A "user" is a keytab without a host specified
A "service" is a keytab with a host specified! That is the only difference. So if you require a keytab with a specific host (mostly used for computer programs such as kafka and zookeeper) create a 'service'

If you require a keytab without a host specified (mostly used for humans interacting with kerberos) create a 'user'