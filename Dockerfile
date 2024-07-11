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

# Utilizar la imagen base de .NET 8 ASP.NET Core Runtime para ejecutar la aplicación
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Actualizar e instalar las dependencias necesarias
RUN apt-get update
RUN apt-get install -y --no-install-recommends \
    libfontconfig1 \
    libfreetype6 \
    libjpeg-turbo8 \
    libpng16-16 \
    libglib2.0-0 \
    libx11-6 \
    libxext6 \
    libxrender1
RUN rm -rf /var/lib/apt/lists/*

# Copiar la aplicación publicada desde el entorno de compilación
COPY --from=build-env /app/ConvertPDF/out .

# Establecer el punto de entrada de la aplicación
ENTRYPOINT ["dotnet", "ConvertPDF.dll"]




