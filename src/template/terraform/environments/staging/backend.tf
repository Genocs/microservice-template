terraform {
  backend "s3" {
    bucket         = "genocs-backend"
    key            = "api/staging/terraform.tfstate"
    region         = "ap-south-1"
    dynamodb_table = "genocs-state-locks"
  }
}
