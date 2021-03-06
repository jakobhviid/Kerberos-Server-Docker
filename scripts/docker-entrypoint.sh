#!/bin/bash

# Exit if any command has a non-zero exit status (Exists if a command returns an exception, like it's a programming language)
# Prevents errors in a pipeline from being hidden. So if any command fails, that return code will be used as the return code of the whole pipeline
set -eo pipefail

set -m # bash job control (background, foreground)

check-environment.sh

configure-kerberos.sh

api-start.sh &

create-initial-users.sh

fg %1
