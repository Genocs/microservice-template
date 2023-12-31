// AWS Configuration
aws_region   = "ap-south-1"
aws_region_a = "ap-south-1a"
aws_region_b = "ap-south-1b"

// Default Project Tags
environment  = "staging"
owner        = "Mukesh Murugan"
project_name = "genocs-microservice-webapi"

// RDS PostgreSQL Configuration
pg_password = "posgresqladmin"
pg_username = "posgresqladmin"
db_name     = "genocs_db"

ecs_cluster_name = "genocs"

api_container_cpu    = 512
api_container_memory = 1024
api_image_name       = "genocs/dotnet-webapi:latest"
api_service_name     = "dotnet-webapi"

enable_health_check = true
