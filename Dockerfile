FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first (better caching)
COPY *.sln .
COPY HospitalChatbot.API/*.csproj HospitalChatbot.API/
COPY HospitalChatbot.Application/*.csproj HospitalChatbot.Application/
COPY HospitalChatbot.Infrastructure/*.csproj HospitalChatbot.Infrastructure/
COPY HospitalChatbot.Domain/*.csproj HospitalChatbot.Domain/

RUN dotnet restore

# Copy the rest of the source
COPY . .

RUN dotnet publish HospitalChatbot.API/HospitalChatbot.API.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "HospitalChatbot.API.dll"]

# FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
# WORKDIR /app
# EXPOSE 8080

# FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# WORKDIR /src
# COPY . .
# RUN dotnet restore
# RUN dotnet publish HospitalChatbot.API/HospitalChatbot.API.csproj -c Release -o /app/publish

# FROM base AS final
# WORKDIR /app
# COPY --from=build /app/publish .
# ENTRYPOINT ["dotnet", "HospitalChatbot.API.dll"]
