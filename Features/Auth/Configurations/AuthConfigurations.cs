using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GROUPFLOW.Features.Auth.Entities;

namespace GROUPFLOW.Features.Auth.Configurations;

// Auth feature uses AuthPayload which is a DTO record, not a database entity
// This file is a placeholder for any future auth-related entity configurations
// (e.g., RefreshToken entity if implementing token storage in DB)

/// <summary>
/// Placeholder configuration for auth-related entities.
/// Currently empty as AuthPayload is not persisted to database.
/// </summary>
public static class AuthConfigurations
{
    // Add configurations here if auth entities are added to the database
}
