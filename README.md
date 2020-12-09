# What Is The Current Sprint?

www.whatisthecurrentsprint.com

## Docker Compose

**Building:**

```
docker-compose build
```

**Running:**

```
cd docker
docker-compose -f ./docker-compose.yml -f ./docker-compose.override.yml -f ./docker-compose.local.yml up
```

## ngrok

ngrok allows you to expose a web server running on your local machine to the internet. Just tell ngrok what port your web server is listening on.

```
ngrok http 7071 --host-header localhost
```
