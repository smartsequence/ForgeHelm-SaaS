FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["ForgeHelm.SaaS.csproj", "./"]
RUN dotnet restore "ForgeHelm.SaaS.csproj"

COPY . .
RUN dotnet publish "ForgeHelm.SaaS.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "ForgeHelm.SaaS.dll"]

