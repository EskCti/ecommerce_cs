FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS builder
WORKDIR /app

COPY apps/backend/RetailOps.Api/RetailOps.Api.csproj ./apps/backend/RetailOps.Api/
COPY apps/backend/RetailOps.Core/RetailOps.Core.csproj ./apps/backend/RetailOps.Core/
COPY apps/backend/RetailOps.Infrastructure/RetailOps.Infrastructure.csproj ./apps/backend/RetailOps.Infrastructure/
COPY apps/backend/RetailOps.Infrastructure.Legacy/RetailOps.Infrastructure.Legacy.csproj ./apps/backend/RetailOps.Infrastructure.Legacy/
COPY apps/backend/RetailOps.Shared.Kernel/RetailOps.Shared.Kernel.csproj ./apps/backend/RetailOps.Shared.Kernel/
COPY apps/backend/RetailOps.Identity.Core/RetailOps.Identity.Core.csproj ./apps/backend/RetailOps.Identity.Core/
COPY apps/backend/RetailOps.Identity.Infrastructure/RetailOps.Identity.Infrastructure.csproj ./apps/backend/RetailOps.Identity.Infrastructure/
COPY apps/backend/RetailOps.Platform.Core/RetailOps.Platform.Core.csproj ./apps/backend/RetailOps.Platform.Core/
COPY apps/backend/RetailOps.Platform.Infrastructure/RetailOps.Platform.Infrastructure.csproj ./apps/backend/RetailOps.Platform.Infrastructure/
RUN dotnet restore apps/backend/RetailOps.Api/RetailOps.Api.csproj

COPY . .
RUN dotnet publish apps/backend/RetailOps.Api/RetailOps.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim AS runner
WORKDIR /app

RUN groupadd --gid 1001 appuser \
    && useradd --uid 1001 --gid 1001 --create-home appuser
USER appuser

COPY --from=builder /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "RetailOps.Api.dll"]
