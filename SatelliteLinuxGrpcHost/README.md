# SatelliteLinuxGrpcHost

Satellite deployment service для Linux на базе .NET 9 и gRPC.

## Возможности

Сервис поддерживает следующие операции развертывания:

- **DeployContainer** - развертывание Docker контейнеров
- **ProcessConfigFile** - обработка конфигурационных файлов
- **RunPowerShellScript** - выполнение PowerShell скриптов
- **CopyFiles** - копирование файлов
- **RunSQLScript** - выполнение SQL скриптов

## Требования

- .NET 9.0 SDK
- Docker (для операций с контейнерами)
- PowerShell 7.x (для выполнения скриптов)

## Запуск

### Локально

```bash
dotnet run --project SatelliteLinuxGrpcHost.csproj
```

Сервис будет доступен на порту 5000 (можно настроить в `appsettings.json`).

### В Docker

```bash
docker build -t satellite-linux-grpc:latest -f Dockerfile ..
docker run -d -p 5000:5000 \
  -v /var/run/docker.sock:/var/run/docker.sock \
  --name satellite-linux \
  satellite-linux-grpc:latest
```

**Важно:** Для работы с Docker контейнерами монтируется Docker socket (`/var/run/docker.sock`).

## Конфигурация

Настройки в `appsettings.json`:

```json
{
  "Service": {
    "Port": "5000"
  }
}
```

## gRPC API

Protobuf определения находятся в `Protos/deployment.proto`.

Основные методы:

- `IsReady()` - проверка готовности сервиса
- `BeginPublication(publicationId)` - начало публикации
- `UploadPackageBuffer(stream)` - загрузка пакета
- `DeployContainer(request)` - развертывание контейнера
- `ProcessConfigFile(request)` - обработка конфига
- `RunPowerShellScript(request)` - выполнение скрипта
- `CopyFiles(request)` - копирование файлов
- `RunSQLScripts(request)` - выполнение SQL
- `Complete()` - завершение публикации
- `Rollback()` - откат изменений

## Разработка

Проект использует:

- ASP.NET Core 9.0
- Grpc.AspNetCore 2.68.0
- gRPC-Web support

## Структура проекта

```
SatelliteLinuxGrpcHost/
├── Protos/
│   └── deployment.proto       # gRPC определения
├── Services/
│   ├── IDeploymentService.cs  # Интерфейс сервиса
│   ├── DeploymentService.cs   # Реализация логики
│   └── DeploymentController.cs # gRPC контроллер
├── Program.cs                 # Точка входа
├── appsettings.json          # Конфигурация
└── Dockerfile                # Docker образ
```

## Примечания

- Для работы с Docker необходим доступ к Docker daemon
- PowerShell скрипты выполняются через `pwsh`
- Все операции логируются в консоль
