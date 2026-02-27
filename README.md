# ChatApp

A demo project showing how to build a modular, maintainable, and high-performance web front end using **Razor Slices**, **HTMX**, and **htmx-signalr** — without MVC, Razor Pages, or Blazor.

## Stack

- **[Razor Slices](https://github.com/DamianEdwards/RazorSlices)** — lightweight, AOT-compatible Razor templates returned directly from Minimal API endpoints. Each slice is a self-contained UI fragment with its own model, keeping the UI logic close to the markup.
- **[HTMX](https://htmx.org)** — drives interactivity by swapping HTML fragments returned from the server, eliminating the need for a JavaScript framework.
- **[htmx-signalr](https://github.com/Renerick/htmx-signalr)** — HTMX extension that integrates SignalR for real-time server push, enabling live updates without polling or manual WebSocket handling.

## Why this combination?

| Concern | Solution |
|---|---|
| Templating | Razor Slices — strongly typed, trimming/AOT safe |
| Interactivity | HTMX — hypermedia-driven, no client-side state |
| Real-time updates | SignalR + htmx-signalr extension |
| Performance | Native AOT publish, minimal allocations, unbuffered rendering |
| Maintainability | Each UI component is an isolated slice + endpoint pair |

## Google Cloud Run

Cloud Run is a great fit for this project because it scales to zero and bills only for active request time — meaning cold start latency and memory footprint directly affect cost and user experience. Several deliberate choices in this project address both:

| Optimization | Detail |
|---|---|
| **Native AOT** | Published as a fully self-contained native binary. No JIT warm-up, no .NET runtime to load. Cold starts are dramatically faster compared to a standard `dotnet` app. |
| **`CreateSlimBuilder`** | Strips out middleware components that aren't needed (IIS integration, hosting startup scanning, etc.), reducing startup time and binary size further. |
| **No framework overhead** | Razor Slices + Minimal APIs mean there's no MVC pipeline, no controller discovery, and no view engine to initialize at startup. |
| **Small image size** | Because the output is a single self-contained native binary with no external runtime dependency, the container image can be based on a minimal `debian` or `distroless` base image rather than a full .NET SDK/runtime image. |

### Deploying to Cloud Run

This project includes a [`cloudbuild.yaml`](cloudbuild.yaml) and a [`Dockerfile`](Dockerfile). Deployment is done entirely in the cloud — no local Docker install required.

**One-time setup:**

```bash
# Authenticate
gcloud auth login

# Create a new project (skip if using an existing one)
gcloud projects create <your-project-id> --name="ChatApp"

# Find your billing account ID (look for the value in the ACCOUNT_ID column)
gcloud billing accounts list

# Link the billing account (required for Cloud Run)
gcloud billing projects link <your-project-id> --billing-account=<ACCOUNT_ID>

# Set the active project
gcloud config set project <your-project-id>

# Enable the required APIs
gcloud services enable run.googleapis.com cloudbuild.googleapis.com artifactregistry.googleapis.com secretmanager.googleapis.com

# Create an Artifact Registry repository for the container image
gcloud artifacts repositories create chatapp --repository-format=docker --location=asia-southeast1

# Grant Cloud Build permission to deploy to Cloud Run
gcloud projects add-iam-policy-binding <your-project-id> \
  --member="serviceAccount:<project-number>@cloudbuild.gserviceaccount.com" \
  --role="roles/run.admin"
```

**Deploy:**

```bash
gcloud builds submit --config cloudbuild.yaml --substitutions=_REGION=asia-southeast1
```

Cloud Build will compile the AOT binary inside Docker, push the image to Artifact Registry, and deploy to Cloud Run automatically.

### Secrets

Store secrets (e.g. connection strings, API keys) in Secret Manager rather than environment variables or config files:

```bash
# Create a secret — use printf to avoid a trailing newline corrupting the value
# (e.g. a MongoDB database name with \n at the end is valid but nearly impossible to delete)

# Linux / macOS (bash):
printf '%s' "your-secret-value" | gcloud secrets create MY_SECRET --data-file=-

# Windows PowerShell:
[System.IO.File]::WriteAllText("secret.tmp", "your-secret-value")
gcloud secrets create MY_SECRET --data-file="secret.tmp"
Remove-Item "secret.tmp"

# ⚠️  Avoid `echo "value" |` — PowerShell's echo always appends \n regardless of -n

# Grant Cloud Run access to it
gcloud secrets add-iam-policy-binding MY_SECRET \
  --member="serviceAccount:<your-project-number>-compute@developer.gserviceaccount.com" \
  --role="roles/secretmanager.secretAccessor"
```

Then reference it in the `--set-secrets` flag inside [`cloudbuild.yaml`](cloudbuild.yaml):

```yaml
- '--set-secrets=MY_ENV_VAR=MY_SECRET:latest'
```

Cloud Run will inject the secret as an environment variable at runtime — your app code just reads it via `Environment.GetEnvironmentVariable("MY_ENV_VAR")`.

> **Note:** The `Dockerfile` targets `linux-x64` for the AOT build. The `win-x64` target in `ChatApp.csproj` is only used for local Windows builds.

## Running locally

```bash
dotnet run
```

The app publishes an AOT binary for `win-x64` automatically on build via the `AotPublishOnBuild` MSBuild target.
