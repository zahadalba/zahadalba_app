<script setup lang="ts">
</script>

<template>
    <NuxtLink to="/">На главную</NuxtLink>
    <h3>О проекте</h3>
    <p>
        Стэк: Nuxt 4 (Composition API) + ASP.NET Core 10 (Minimals API + Results API) + PostgreSQL + EF Core. SSR схема
        приложения.
    </p>
    <p>
        Валидация:
    <ul>
        <li>Nuxt: Zod + Regle.</li>
        <li>ASP.NET Core: FluentValidation.</li>
    </ul>
    </p>
    <p>
        В Nuxt Query и Mutation запросы реализованы с использованием библиотеки Pinia Colada. Стор - Pinia Store.
    </p>
    <p>
        В проекте реализовано:
    <ul>
        <li>регистрация пользователей;</li>
        <li>авторизация пользователей;</li>
        <li>логаут;</li>
        <li>личный профиль;</li>
        <li>панель администратора.</li>
    </ul>
    </p>
    <p>
        Авторизация выполняется с использованием access (короткоживущий) и refresh (долгоживущий) токенов. В браузере
        токены хранятся в httpOnly Cookie.
    </p>
    <p>
        Реализован механизм ротации токенов: при истечении access токена и наличии refresh токена происходит их
        обновление и переустановка. На бэкенде refresh организован с защитой от гонки запросов при помощи grace периода
        и пессимистической блокировки (transaction + FOR UPDATE) и от повторно использования Revoked токена (Reuse
        detection). Эндпойнт авторизации защищен от брутфорса.
    </p>
    <p>
        Таблицы в PostgreSQL создаются с помощью миграций.
    </p>
    <p>
        Проект собран с помощью Docker и запущен на VPS с использованием revers proxy Caddy.
    </p>
    <p>
        Разработка продолжается...
    </p>
</template>