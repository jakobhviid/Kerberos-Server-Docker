FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as build

ARG BUILDCONFIG=RELEASE
ARG VERSION=1.0.0

COPY ./api/app_api.csproj /build/app_api.csproj

RUN dotnet restore ./build/app_api.csproj

COPY ./api/ /build/

WORKDIR /build/

RUN dotnet publish ./app_api.csproj -c ${BUILDCONFIG} -o out /p:Version=${VERSION}

FROM centos:8

LABEL Maintainer="Oliver Marco van Komen"

ENV API_HOME=/opt/api
ENV ASPNETCORE_ENVIRONMENT=Production

RUN yum update -y && \
    yum install -y krb5-server vim && \
    dnf install aspnetcore-runtime-3.1 -y

COPY --from=build /build/out ${API_HOME}

# Copy scripts
COPY ./scripts /tmp/
RUN chmod +x /tmp/*.sh && \
    mv /tmp/* /usr/bin && \
    rm -rf /tmp/*

COPY ./kerberos_conf_files/krb5.conf /etc/krb5.conf

RUN mkdir /keytabs

CMD [ "docker-entrypoint.sh" ]
