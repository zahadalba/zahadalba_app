<script setup lang="ts">
definePageMeta({
    middleware: "auth"
});
</script>

<template>
    <NuxtLink to="/">На главную</NuxtLink>
    <NuxtLink to="/admin/users">Пользователи</NuxtLink>
</template>