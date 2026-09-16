variable "project_name" {
  description = "Short name used as a prefix for all resource names."
  type        = string
  default     = "britehouse-orders"
}

variable "location" {
  description = "Azure region to deploy into."
  type        = string
  default     = "South Africa North"
}

variable "environment" {
  description = "Environment tag/suffix, e.g. dev, assessment."
  type        = string
  default     = "assessment"
}

variable "tags" {
  description = "Common tags applied to all resources."
  type        = map(string)
  default = {
    project    = "britehouse-technical-assessment"
    managed_by = "terraform"
  }
}
