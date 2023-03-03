#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["../FullFunctionProject/FullFunctionProject.csproj", "FullFunctionProject/"]
COPY ["../FullNodeDataLogger/FullNodeDataLogger.csproj", "FullNodeDataLogger/"]
COPY ["../FullNode/FullNode.csproj", "FullNode/"]
COPY ["../Model/Model.csproj", "Model/"]
COPY ["../Utilities/Utilities.csproj", "Utilities/"]
COPY ["../Observer/Observer.csproj", "Observer/"]
RUN dotnet clean "FullFunctionProject/FullFunctionProject.csproj"
RUN dotnet restore "FullFunctionProject/FullFunctionProject.csproj"
COPY . .
WORKDIR "/src/FullFunctionProject"
RUN dotnet build "FullFunctionProject.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FullFunctionProject.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY ["../FullFunctionProject/I Walk Alone_v720P.mp4", "MinersFile/"]
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FullFunctionProject.dll"]