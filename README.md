# Система Бронирования Билетов в Кино (Desktop)

## Описание Проекта

Этот проект представляет собой простое настольное приложение, разработанное на C# с использованием Windows Forms и базы данных SQLite. Приложение предназначено для симуляции процесса бронирования билетов в кинотеатр.

Пользователи могут просматривать список доступных фильмов, выбирать сеансы и места в зале. Администраторы имеют расширенные возможности для управления фильмами и сеансами.

Проект создан в учебных целях и служит примером реализации основных функций системы бронирования с использованием локальной базы данных.

## Основные Функции

*   **Регистрация и Авторизация:** Пользователи могут зарегистрироваться или войти в существующую учетную запись. Реализовано разделение ролей (пользователь/администратор).
*   **Просмотр Фильмов:** Отображение списка доступных фильмов с основной информацией (название, описание, длительность, постер).
*   **Выбор Сеанса:** Выбор времени сеанса для выбранного фильма.
*   **Визуализация Зала:** Отображение схемы зала с занятыми и свободными местами.
*   **Выбор Мест:** Возможность выбора одного или нескольких свободных мест.
*   **Процесс Бронирования/Оплаты:** Симуляция процесса оплаты и сохранение бронирования в базе данных (без интеграции с реальными платежными системами).
*   **Подтверждение Бронирования:** Отображение информации о совершенном бронировании.
*   **(Для Администратора) Добавление Фильмов:** Возможность добавления новых фильмов в базу данных с информацией о названии, описании, длительности и постере.
*   **(Для Администратора) Управление Фильмами:** (Частично реализовано) Возможность удаления фильмов.

## Использованные Технологии

*   **Язык Программирования:** C#
*   **Фреймворк:** .NET Framework (Windows Forms)
*   **База данных:** SQLite (`System.Data.SQLite` NuGet package)

## Требования к Системе

*   Операционная система Windows
*   Установленная среда разработки Visual Studio (или аналогичная IDE с поддержкой .NET)
*   .NET Framework (совместимая версия)

## Установка и Настройка

1.  **Клонируйте репозиторий:**
    ```bash
    git clone https://github.com/begin79/individual.git
    ```
2.  **Откройте проект в Visual Studio:** Откройте файл `.sln` в Visual Studio.
3.  **Установите NuGet пакеты:** Убедитесь, что установлен пакет `System.Data.SQLite.Core`. Visual Studio обычно предлагает восстановить пакеты при открытии проекта, но если нет, сделайте это вручную через "Manage NuGet Packages for Solution".
4.  **Настройте базу данных:**
    *   Убедитесь, что файл базы данных `individual.db` находится по пути, указанному в файле `app.config`.
    *   **Важно:** Проверьте и обновите строку подключения (`ConnectionString`) в файле `app.config`, если путь к файлу базы данных отличается:
        ```xml
        <configuration>
            <appSettings>
                <!-- Обновите путь к вашему файлу individual.db -->
                <add key="ConnectionString" value="Data Source=C:\Путь\К\Вашей\Папке\individual.db;Version=3;" />
            </appSettings>
        </configuration>
        ```
5.  **Настройте пути к постерам:** Проверьте и обновите пути к постерам в коде (`MoviesForm.cs`, `AddMovieForm.cs`) если они отличаются от указанных:
    *   Папка для дефолтного постера (например, `C:\Posters\default_poster.jpg`).
    *   Папка для сохранения новых постеров (например, `D:\photo`).

## Как Запустить

1.  В Visual Studio выполните сборку решения (`Build` -> `Build Solution` или `Rebuild Solution`).
2.  Запустите приложение, нажав кнопку "Start" (или F5).

## База Данных

Проект использует локальную базу данных SQLite (`individual.db`). Основные таблицы:

*   `Users`: Информация о пользователях (id, name, email, login, password, role). Роль `0` - пользователь, `1` - администратор.
*   `Movies`: Информация о фильмах (id, title, description, duration, poster, time).
*   `Sessions`: Информация о сеансах (id, movie_id, date, hall_number, total_seats, available_seats).
*   `Bookings`: Информация о бронированиях (id, user_id, session_id, seat, full_name, card_last4, session_time).

Файл базы данных должен находиться по пути, указанному в `app.config`.

## Учетные Записи для Тестирования

Для удобства тестирования в базе данных могут быть предопределены следующие учетные записи:

```text
Администратор:
Логин: admin
Пароль: admin

Пользователь:
Логин: misha
Пароль: misha

Примечание: Эти данные предоставлены для тестирования локальной версии приложения. В реальных проектах следует использовать безопасные методы аутентификации и хранения учетных данных.

```

## Планы и Возможности для Расширения
* Реализация полного функционала управления сеансами для администратора (добавление, редактирование, удаление сеансов).
* Разработка формы просмотра забронированных пользователем билетов.
* Создание формы для администратора для просмотра всех бронирований.
* Улучшение валидации данных и обработка ошибок.
* Разделение бизнес-логики и UI (например, с использованием паттернов MVC или MVVM).
* Более продвинутая схема зала и логика выбора мест.
* Интеграция с реальным платежным шлюзом (для коммерческого использования).
* Добавление Unit и Integration тестов.
