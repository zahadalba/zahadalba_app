<script setup lang="ts">
import type { ApiError } from '~/types/apiError.type';
import type { GetMeResponseDto } from '~/types/auth.type';
import type { User } from '~/types/user.type';

definePageMeta({
    middleware: 'auth'
});

const authStore = useAuthStore();

const user = ref<User>();
const { data, error } = await useFetch<GetMeResponseDto, ApiError>("/api/auth/me");
user.value = data.value;

const logout = async () => {
    try {
        const headers = useRequestHeaders(["cookie"]);
        await $fetch("/api/auth/sign-out", {
            method: "POST",
            headers,
        });
    } finally {
        authStore.user = null;
        authStore.isAuthenticated = false;
        await navigateTo("/");
    }
};
</script>

<template>
    <h3>Профиль</h3>
    <p>Привет, {{ user?.firstName }}!</p>
    <p>ID: {{ user?.id }}</p>
    <button @click="logout">Выйти</button>
    <p>{{ error?.statusCode }} {{ error?.statusMessage }}</p>
</template>