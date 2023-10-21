output "api_url" {
  value       = "http://${aws_lb.genocs_api_alb.dns_name}/api"
  description = "Genocs .NET WebAPI URL"
}
