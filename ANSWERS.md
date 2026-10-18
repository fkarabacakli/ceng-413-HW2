# HW-1 Answers

## 1. Order of the Dockerfile instructions

Docker caches every instruction as a layer, and when one layer changes, every layer after it is built again.
That is why my backend Dockerfile first copies only `Nalbur.slnx` and the `.csproj` files, runs `dotnet restore`,
 and only then copies the source code with `COPY . .`. The project files change rarely but the `.cs` files change all the time,
so after a normal code edit the restore layer comes from the cache and the NuGet packages are not downloaded again. If I swapped 
`COPY . .` and `RUN dotnet restore`, any change to any file would invalidate the restore layer, and every build would download all 
packages again and take much longer. The frontend uses the same idea: `package.json` and `package-lock.json` first, then `npm ci`, then the source.

## 2. Data inside a container

Data written into a container's writable layer is deleted together with the container, so if the stock ledger were kept inside the API container, 
`docker compose down` would erase it. To keep data, it has to live outside the container, in a volume or a bind mount. In my project the API container is 
stateless: the ledger is stored in a separate PostgreSQL container, and its data directory is the named volume `pgdata`. Because of this, `docker compose down`
and then `up` keeps every product and movement, and only `docker compose down -v` deletes them. Important state belongs in a database (or other storage) with 
its own volume and backups, not in the application container, so the application container can be removed or replaced at any time.

## 3. The `latest` tag

`latest` is a tag that moves: today it can point to one version and a few months later to a new major version. So the same Dockerfile can produce a 
different image on another machine or on another day, and a build that worked can break without any change in my code. I pinned every base image 
instead: `mcr.microsoft.com/dotnet/sdk:10.0` and `aspnet:10.0`, `node:22-alpine`, `nginxinc/nginx-unprivileged:1.27-alpine` and `postgres:17.6`. 
T he NuGet package versions are fixed in the `.csproj` files, and `npm ci` installs exactly what is in `package-lock.json`. 
To be even stricter, I could pin a full patch version or an `@sha256:` digest.

## Note: image size (multi-stage build)

The final `nalbur-api:1.0.0` image is 363 MB, while the SDK build stage (`docker build --target build`) is 1.98 GB, 
so the multi-stage build leaves about 82% behind. For the frontend, the final nginx image is 74.9 MB and the Node build stage is 346 MB.
