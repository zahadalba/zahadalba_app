<script setup lang="ts">
definePageMeta({
    middleware: 'auth'
});

const authStore = useAuthStore();

const logout = async () => {
    try {
        await $fetch("/api/auth/sign-out", {
            method: "POST",
        });
    } finally {
        authStore.user = null;
        authStore.isAuthenticated = false;
        await navigateTo("/");
    }
};
</script>

<template>
    <p>Профиль</p>
    <button @click="logout">Выйти</button>
</template>