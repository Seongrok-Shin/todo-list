﻿# Use the official .NET 9.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Set the working directory
WORKDIR /app

# Copy the project file and restore dependencies
COPY ./ToDoWebApp.csproj ./
RUN dotnet restore ./ToDoWebApp.csproj

# Copy the entire source code
COPY ./ ./

# Build the application (stay in /app directory where the .csproj file is)
RUN dotnet publish ./ToDoWebApp.csproj -c Release -o /app/publish --no-restore

# Use the official ASP.NET Core runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

# Install ca-certificates for HTTPS connections
RUN apt-get update && apt-get install -y --no-install-recommends ca-certificates && rm -rf /var/lib/apt/lists/*

# Set the working directory
WORKDIR /app

# Copy the published app from the build stage
COPY --from=build /app/publish .

# Create a non-root user for security
RUN addgroup --system --gid 1001 appgroup && \
    adduser --system --uid 1001 --gid 1001 appuser

# Change ownership of the app directory
RUN chown -R appuser:appgroup /app

# Switch to the non-root user
USER appuser

# Expose the port that the app will run on
EXPOSE 8080

# Set environment variables for production
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# Add health check to monitor container health
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
  CMD dotnet --info > /dev/null && \
      timeout 5s bash -c "</dev/tcp/localhost/8080" || exit 1

# Set the entry point
ENTRYPOINT ["dotnet", "ToDoWebApp.dll"]
