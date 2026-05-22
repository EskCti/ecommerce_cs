FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS builder
WORKDIR /app

COPY RetailOps.sln ./
COPY apps/backend/RetailOps.Api/RetailOps.Api.csproj ./apps/backend/RetailOps.Api/
COPY apps/backend/RetailOps.Core/RetailOps.Core.csproj ./apps/backend/RetailOps.Core/
COPY apps/backend/RetailOps.Infrastructure/RetailOps.Infrastructure.csproj ./apps/backend/RetailOps.Infrastructure/
COPY apps/backend/RetailOps.Infrastructure.Legacy/RetailOps.Infrastructure.Legacy.csproj ./apps/backend/RetailOps.Infrastructure.Legacy/
COPY apps/backend/RetailOps.Shared.Kernel/RetailOps.Shared.Kernel.csproj ./apps/backend/RetailOps.Shared.Kernel/
RUN dotnet restore

COPY . .
RUN dotnet publish apps/backend/RetailOps.Api/RetailOps.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runner
WORKDIR /app

RUN adduser -u 1001 --disabled-password --gecos "" appuser
USER appuser

COPY --from=builder /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "RetailOps.Api.dll"]
