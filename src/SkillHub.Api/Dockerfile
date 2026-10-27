FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY SkillHub.sln ./
COPY src/SkillHub.Domain/SkillHub.Domain.csproj src/SkillHub.Domain/
COPY src/SkillHub.Application/SkillHub.Application.csproj src/SkillHub.Application/
COPY src/SkillHub.Infrastructure/SkillHub.Infrastructure.csproj src/SkillHub.Infrastructure/
COPY src/SkillHub.Api/SkillHub.Api.csproj src/SkillHub.Api/
RUN dotnet restore src/SkillHub.Api/SkillHub.Api.csproj
COPY src/ src/
RUN dotnet publish src/SkillHub.Api/SkillHub.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "SkillHub.Api.dll"]
