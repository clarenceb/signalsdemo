FROM mcr.microsoft.com/dotnet/sdk:9.0 AS builder
WORKDIR /app

# caches restore result by copying csproj file separately
COPY *.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish --output /app/ --configuration Release --no-restore
RUN sed -n 's:.*<AssemblyName>\(.*\)</AssemblyName>.*:\1:p' *.csproj > __assemblyname
RUN if [ ! -s __assemblyname ]; then filename=$(ls *.csproj); echo ${filename%.*} > __assemblyname; fi

# Stage 2
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=builder /app .

ENV PORT 80
EXPOSE 80

# Use shell form to ensure the assembly name is dynamically resolved
ENTRYPOINT ["dotnet", "signalsdemo.dll", "--urls", "http://*:80"]
