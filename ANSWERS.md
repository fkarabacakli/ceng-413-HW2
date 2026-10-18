# HW-1 Answers

## 1. Order of the Dockerfile instructions

Docker saves every instruction as a layer in its cache. If one layer changes, Docker has to build that layer and all the layers after it again.
In my backend Dockerfile, I first copy only `Nalbur.slnx` and the `.csproj` files and run `dotnet restore`. After that, I copy the rest of the code with `COPY . .`.
The `.csproj` files do not change often, but I change the `.cs` files all the time. So when I edit the code, the restore step comes from the cache and the NuGet packages are not downloaded again.
If I put `COPY . .` before `RUN dotnet restore`, every small code change would break the cache of the restore step. Then every build would download all the packages again and it would be much slower.
I did the same thing in the frontend: first `package.json` and `package-lock.json`, then `npm ci`, and then the source code.

## 2. Data inside a container

When a container is deleted, the data inside it is deleted too. So if I kept the stock data inside the API container, `docker compose down` would delete all of it.
To keep the data, it must be stored outside the container, in a volume or a bind mount.
In my project the API container does not keep any data. The data is stored in a separate PostgreSQL container, and PostgreSQL writes it to a named volume called `pgdata`.
Because of this, the products and stock movements are still there after `docker compose down` and `up`. Only `docker compose down -v` deletes them.
I think important data should be in a database with its own volume, not in the application container. Then I can delete or update the application container at any time without losing data.

## 3. The `latest` tag

The `latest` tag is not fixed. Today it can point to one version, and a few months later it can point to a new major version.
This means the same Dockerfile can create a different image on another computer or on another day, and a build that worked before can break even if I did not change my code.
Because of this, I used fixed versions for all base images: `mcr.microsoft.com/dotnet/sdk:10.0`, `aspnet:10.0`, `node:22-alpine`, `nginxinc/nginx-unprivileged:1.27-alpine` and `postgres:17.6`.
The NuGet package versions are also fixed in the `.csproj` files, and `npm ci` installs exactly the versions in `package-lock.json`.
To be even safer, I could use a full patch version or an `@sha256:` digest.

## Note: image size (multi-stage build)

The final `nalbur-api:1.0.0` image is 363 MB, but the SDK build stage (`docker build --target build`) is 1.98 GB. So the multi-stage build makes the image about 82% smaller.
For the frontend, the final nginx image is 74.9 MB and the Node build stage is 346 MB.

## Note: use of an AI assistant

I used an AI assistant mainly to write the frontend code. It also helped with parts of the backend (the PostgreSQL setup, the API docs
and the health check).
