# Microservice for purchases

Краткий обзор репозитория с архитектурой, составом модулей и текущими наблюдениями.

## Что это за проект

Проект реализует backend маркетплейса (покупки/корзина/заказы) на **ASP.NET Core + EF Core + PostgreSQL**.
Слои разделены по классической схеме Domain/Application/Infrastructure/Presentation.

## Структура решения

Решение: `MarketplaceSale/MarketplaceSale.sln`

- `MarketplaceSale.Domain` — доменные сущности и бизнес-правила.
- `MarketplaceSale.Domain.ValueObjects` — value objects (Money, Quantity, Username и т.д.).
- `MarketplaceSale.Domain.Services` — доменные сервисы.
- `MarketplaceSale.Domain.Repositories.Abstractions` — абстракции репозиториев и Unit of Work.
- `MarketPlaceSale.Application.Models` — DTO/модели слоя приложения.
- `MarketPlaceSale.Application.Services` — application services + AutoMapper профиль.
- `MarketplaceSale.Infrastructure.EntityFramework` — `ApplicationDbContext` и конфигурации EF.
- `MarketplaceSale.Infrastructure.Repositories.Implementation` — реализации репозиториев.
- `MarketplaceSale.Presentation/WebApi` — Web API, DI, контроллеры, Swagger.
- `ClassTest` — консольный проект с доменным сценарным прогоном.

## API-слой

В `WebApi` есть контроллеры:

- `ProductController`
- `ClientController`
- `SellerController`
- `CartController`
- `OrderController`

Маршруты строятся вокруг `/api/v1/*`, присутствует базовая валидация входных данных на уровне контроллеров.

## Инфраструктура и запуск

- Целевая платформа: **.NET 8**.
- ORM: **Entity Framework Core 9 + Npgsql**.
- Swagger подключен в Development.
- Миграция БД вызывается при старте приложения через extension (`app.MigrateDatabase<ApplicationDbContext>()`).
- Строка подключения по умолчанию в `appsettings.Development.json` указывает на локальный PostgreSQL.

## Предложения по улучшению (приоритетный план)

### P0 — стабильность и качество

1. **Добавить автоматические тесты для Web API и Application Services**.
   - Сейчас `ClassTest` — это консольный проект-сценарий, а не автотесты.
   - Рекомендация: добавить отдельные test-проекты (unit + integration) и покрыть критичные кейсы по заказам/оплатам/возвратам.

2. **Централизовать обработку ошибок и валидацию**.
   - В контроллерах много ручных `BadRequest/NotFound` и проверок входных параметров.
   - Рекомендация: внедрить единый middleware для exception handling + `ProblemDetails`, а валидацию перенести в FluentValidation.

3. **Проверить и стабилизировать кодировку исходников**.
   - В `Program.cs` есть «кракозябры» в комментариях, что обычно признак смешанной кодировки.
   - Рекомендация: привести файлы к UTF-8 и добавить `.editorconfig`/gitattributes для фиксации кодировки.

### P1 — архитектура и поддерживаемость

4. **Убрать дублирование бизнес-проверок из контроллеров**.
   - Например, проверки статусов заказа и авторизации операции частично повторяются в нескольких endpoints.
   - Рекомендация: оставить в контроллерах только transport-логику, а state transitions — в application/domain.

5. **Пересмотреть состав подключенных пакетов и DI-регистраций**.
   - В WebHost подключены пакеты для MassTransit/gRPC/HealthChecks, но в текущем `Program.cs` не видно полной конфигурации.
   - Рекомендация: либо довести интеграции до рабочей конфигурации, либо убрать лишние зависимости до реальной необходимости.

### P2 — эксплуатация и DX

6. **Подготовить контейнерный запуск и CI**.
   - Добавить `Dockerfile` + `docker-compose` (api + postgres).
   - Настроить CI pipeline: restore/build/test/format/lint.

7. **Усилить observability**.
   - Добавить health endpoints, структурированное логирование, correlation-id и метрики.

8. **Сделать API-контракт более строгим**.
   - Добавить версионирование OpenAPI, единые схемы ошибок, и примеры payload для ключевых операций.

## Быстрый старт (локально)

1. Поднять PostgreSQL и создать БД `MarketplaceSale`.
2. Обновить строку подключения в `MarketplaceSale.Presentation/WebApi/appsettings.Development.json`.
3. Выполнить:

```bash
dotnet restore MarketplaceSale/MarketplaceSale.sln
dotnet build MarketplaceSale/MarketplaceSale.sln
dotnet run --project MarketplaceSale/MarketplaceSale.Presentation/WebApi/MarketplaceSale.WebHost.csproj
```

После запуска Swagger обычно доступен по `/swagger` (в Development).
