# HW-2 Answers

## 1. Calling the second service by name

Docker Compose creates a network for my project (`hw2_default`) and connects all the services to it. This network has its own DNS server, so every container can find the others by their service name.
That is why my app uses `Host=db` in the connection string, and nginx in the `web` container sends `/api` requests to `http://app:8080`. Docker turns `db` and `app` into the right container IP addresses, and I never have to write an IP.
The default `bridge` network that `docker run` uses does not have this DNS, so containers on it can only reach each other by IP address. The IP can change every time a container starts, so it is not a good idea to use it.
If I want names to work without Compose, I have to create my own network with `docker network create` and start the containers on it.

## 2. Stateless and stateful services

`app` and `web` are stateless. `app` keeps all products and stock movements in PostgreSQL and does not write anything to its own disk, and `web` only serves static files. `db` is the only stateful service, because PostgreSQL writes its data to the named volume `pgdata`.
To test this, I added two products (`HW2-001` and `HW2-002`) with POST requests, then checked them with `GET /api/products?search=HW2` after each step (screenshot `docs/pics/05.png`):
- `docker compose rm -sf app` and `docker compose up -d`: the app container was deleted and created again, but both products were still there.
- `docker compose down` and `docker compose up -d`: all containers and the `hw2_default` network were deleted, but both products were still there, because the volume was not deleted.
- `docker compose down -v` and `docker compose up -d`: this time the `hw2_pgdata` volume was deleted too, and the result was `[]`, so my products were gone.
After `down -v` the app shows sample products again, but this is only because `SEED_SAMPLE_DATA=true` fills an empty database at startup. They are new rows, not the old data.

## 3. Where the database password should come from

The password should come from outside the code and the image, for example from environment variables or a secret manager. In my project it is in a `.env` file that is listed in `.gitignore`, and `compose.yaml` reads it with `${POSTGRES_PASSWORD}`. I only commit `.env.example` with a fake value.
If the `.env` file is missing, Compose stops with an error, so the system cannot start with an empty or default password by mistake.
Committing a password is a problem even in a private repository, because Git keeps the full history. If I delete the password in a later commit, it is still in the old commits.
Also, many people and tools can read a private repository (classmates, CI services, forks, or someone who gets access later). If it is leaked once, the only real fix is to change the password everywhere.

## Bonus: third service

My system has a third service, `web`. It is an nginx reverse proxy that serves the React frontend and forwards `/api` and `/health` to `app`. So the browser only talks to one address (`localhost:3000`), and there is no CORS problem.

## Note: use of an AI assistant

I used an AI assistant in this homework. It helped me compare my project with the assignment, rename the service to `app`, and prepare a first draft of these answers. I ran the tests myself and changed the answers to match what I saw.
