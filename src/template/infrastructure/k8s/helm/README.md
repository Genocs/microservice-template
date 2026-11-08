# Genocs application Helm chart

The `charts/apps` chart deploys the application workloads, not their infrastructure:

- API Gateway
- Identities
- Products
- Orders
- Notifications
- BookStore

MongoDB, RabbitMQ, SQL Server, Seq, Jaeger, Consul, and Fabio must be provisioned separately. Set their non-sensitive endpoints in `charts/apps/values.yaml` or an override file. The MongoDB and SQL Server connection strings, RabbitMQ username and password, and Seq API key are read from a Kubernetes Secret; no infrastructure components or credentials are embedded in this chart.

## Configure external credentials

Before installing, provision a Secret in the release namespace using your secret manager or another secure process. By default, the chart reads the following keys from `gnx-app-secrets`:

| Key | Used for |
| --- | --- |
| `MONGODB_CONNECTION_STRING` | MongoDB URI used by the application services |
| `RABBITMQ_USER_NAME` | RabbitMQ username |
| `RABBITMQ_PASSWORD` | RabbitMQ password |
| `SEQ_APIKEY` | Seq API key |
| `MSSQL_CONNECTION_STRING` | SQL Server connection string used by BookStore |

Set `secrets.existingSecret` and the corresponding `secrets.keys.*` values to use a differently named Secret or key names. Do not put secret values in Helm values files or commit Kubernetes Secret manifests containing credentials.

External service addresses are configurable through `env.*` and `externalServices.*`. The MongoDB and SQL Server hostnames are part of their secret-provided connection strings.

## Validate and install

From the repository root:

```bash
helm lint infrastructure/k8s/helm/charts/apps
helm template genocs infrastructure/k8s/helm/charts/apps --namespace gnx-apps
helm upgrade --install genocs infrastructure/k8s/helm/charts/apps --namespace gnx-apps --create-namespace
```

Set image tags and site-specific endpoints with a separate values file or `--set` overrides as appropriate. Ensure the external dependencies and Secret are available before expecting the application pods to become ready.
