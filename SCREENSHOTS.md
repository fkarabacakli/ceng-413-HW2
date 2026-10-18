# Screenshots

**`docker images` and the running containers answering a request** (`docker ps`, then `curl localhost:3000/api/info`):

![docker images, docker ps and curl](docs/pics/05.png)

**The web app** at http://localhost:3000 (React served by nginx, data from the API and PostgreSQL):

![Nalbur Stok web app](docs/pics/01.png)

**API docs (Scalar)** at http://localhost:8080/scalar:

![Scalar API docs](docs/pics/02.png)

**A request from the API docs:** `GET /api/summary` returns 200 OK:

![GET /api/summary in Scalar](docs/pics/03.png)

**Docker Desktop:** the three containers of the `ceng413-hw1` project running:

![Docker Desktop containers](docs/pics/04.png)
