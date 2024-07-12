# Utilizar la imagen base de .NET 8 SDK para la compilación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copiar los archivos de la solución y restaurar las dependencias
COPY *.sln ./
COPY ConvertPDF/*.csproj ./ConvertPDF/
RUN dotnet restore

# Copiar el resto de los archivos y compilar la aplicación
COPY ConvertPDF/. ./ConvertPDF/
WORKDIR /app/ConvertPDF
RUN dotnet publish -c Release -o out

# Utilizar una imagen base mínima para ASP.NET Core
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime-env
WORKDIR /app
COPY --from=build-env /app/ConvertPDF/out .

# Copiar el directorio wwwroot con dpkeys
COPY --from=build-env /app/ConvertPDF/wwwroot /app/wwwroot

# Establecer el punto de entrada de la aplicación
ENTRYPOINT ["dotnet", "ConvertPDF.dll"]

