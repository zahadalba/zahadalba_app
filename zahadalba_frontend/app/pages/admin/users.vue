<script setup lang="ts">
const { data } = await useFetch("/api/admin/users");
</script>

<template>
    <NuxtLink to="/admin">Панель администратора</NuxtLink>
    <ul>
        <li v-for="value in data">
            {{ value }}
        </li>
    </ul>
</template>