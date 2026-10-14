<script setup lang="ts">
const authStore = useAuthStore();
</script>

<template>
    <h3>Главная</h3>
    <NuxtLink v-if="authStore.status === 'authenticated' || authStore.status === 'error'" to="/profile">Профиль
    </NuxtLink>
    <NuxtLink v-else to="/auth/sign-in">Авторизация</NuxtLink>
    <NuxtLink v-if="authStore.user?.role === 'Admin'" to="/admin">Панель администратора</NuxtLink>
    <NuxtLink to="/about">О проекте</NuxtLink>
</template>