Микросервисный бэкенд интернет-магазина (.NET Core+PostgreSQL+Redis+RabbitMQ+Docker).
4 сервиса: Auth, Product, Order, Notification (Worker).

===========НАСТРОЙКА============
Запуск всей инфраструктуры (7 контейнеров):
docker-compose up -d --build

Настройка БД:
Базы данных и таблицы создаются автоматически при старте через EF Core Migrations
Полная очистка БД и кэша (Hard Reset):
docker-compose down -v

===============ИСПОЛЬЗОВАНИЕ===========
Swagger API (Тестирование маршрутов):
Auth: http://localhost:5001/swagger
Products: http://localhost:5002/swagger
Orders: http://localhost:5003/swagger

RabbitMQ (Панель управления): http://localhost:15672 (логин: guest, пароль: guest)
Логи Notification Service (просмотр отправки писем): docker logs -f notification-service