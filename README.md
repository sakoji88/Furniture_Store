# FurnitureStore.Web (ASP.NET Core MVC, .NET 8)

Учебный проект информационной системы мебельного магазина с кастомной авторизацией без ASP.NET Identity.

## Стек
- ASP.NET Core MVC (.NET 8)
- Entity Framework Core
- MSSQLLocalDB (`FurnitureStore`)
- Cookie Authentication
- Bootstrap 5 + кастомный CSS

## Реализовано
- Регистрация и вход (через собственные таблицы `Users`/`Roles`)
- Безопасное хеширование пароля (PBKDF2)
- Разграничение прав: `User` и `Admin`
- Каталог товаров: поиск, фильтры, сортировка, пагинация
- Админка: управление пользователями (бан/разбан/повышение), CRUD товаров и категорий
- Валидация на нескольких уровнях: HTML + DataAnnotations + ограничения БД
- Обработка ошибок и дружелюбные сообщения
- Сидирование ролей, администратора и тестовых данных

## Структура
- `Controllers` — контроллеры MVC
- `Models` — сущности БД
- `ViewModels` — модели форм/страниц
- `Data` — `ApplicationDbContext`
- `Services` — логика авторизации, хеширования, сидирования
- `Repositories` — репозиторий товаров
- `Views` — Razor-представления
- `wwwroot` — статика (CSS, JS, иконка)

## Как запустить
```bash
dotnet restore
dotnet ef migrations add InitialCreate --project Furniture_Store/Furniture_Store.csproj
dotnet ef database update --project Furniture_Store/Furniture_Store.csproj
dotnet run --project Furniture_Store/Furniture_Store.csproj
```

## Тестовый администратор
- Email: `admin@furniturestore.local`
- Password: `Admin123!`

## Подсказки по безопасности
- Все POST-формы защищены AntiForgeryToken.
- SQL injection предотвращается использованием EF Core (без сырого SQL).
- XSS снижается за счет стандартного HTML-encoding Razor.
- Ввод нормализуется (`Trim`) и ограничивается по длине.
