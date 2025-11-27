# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy all source code (run the build from the Solution Root)
COPY . .

# Restore and publish the UI project
# Replace 'SFMB.UI/BlazorApp.UI.csproj' with the actual path to your UI csproj file
RUN dotnet restore "BlazorApp.UI/BlazorApp.UI.csproj"
RUN dotnet publish "BlazorApp.UI/BlazorApp.UI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

# Blazor Server default port is 80 (or 8080). EXPOSE the port.
EXPOSE 8080

# Set the entrypoint to Blazor UI DLL
ENTRYPOINT ["dotnet", "BlazorApp.UI.dll"]