#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS="http://+:9080"
EXPOSE 9080
#EXPOSE 443
#EXPOSE 8080 5001 5000

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/AG.Justin.CustomClaim.Api/AG.Justin.CustomClaim.Api.csproj", "src/AG.Justin.CustomClaim.Api/"]
COPY ["src/AG.Justin.Infrastructure/AG.Justin.Infrastructure.csproj", "src/AG.Justin.Infrastructure/"]
RUN dotnet restore "src/AG.Justin.CustomClaim.Api/AG.Justin.CustomClaim.Api.csproj"
COPY . .
WORKDIR "/src/src/AG.Justin.CustomClaim.Api"
RUN dotnet build "AG.Justin.CustomClaim.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AG.Justin.CustomClaim.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

CMD ["./AG.Justin.CustomClaim.Api"]
#ENTRYPOINT ["dotnet", "AG.Justin.CustomClaim.Api.dll"]