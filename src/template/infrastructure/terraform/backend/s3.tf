resource "aws_s3_bucket" "s3_bucket" {
  bucket = "genocs-backend"
  tags = {
    Name = "genocs-backend"
  }
  lifecycle {
    prevent_destroy = true
  }
}
